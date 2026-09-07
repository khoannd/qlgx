/**
 * Nhúng các trang hướng dẫn HTML tĩnh cũ (BIN/help/*.htm) vào /huong-dan với
 * kiểu dáng hiện đại của trang, thay vì để chúng nằm riêng như file tĩnh thời
 * Windows XP.
 *
 * Ba việc script này làm:
 *   1. Đọc từng trang .htm, bỏ <style>/<script> và các dòng điều hướng cũ
 *      ("[Trở về mục lục] [Trang chủ]") vì trang mới có điều hướng riêng.
 *   2. Viết lại href nội bộ (xxx.htm) thành đường dẫn mới (/huong-dan/xxx),
 *      và copy ảnh minh hoạ vào public/images/help/ kèm kích thước thật (đọc
 *      bằng `image-size`, không giải mã ảnh) để không gây nhảy layout.
 *   3. Khử trùng bằng `sanitize-html` (bỏ script/style/onXXX/iframe...) rồi
 *      ghi ra src/lib/content/huong-dan-data.json.
 *
 * AN TOÀN: nội dung đọc từ các tệp .htm nằm sẵn trong kho mã (không phải dữ
 * liệu người dùng nhập lúc chạy), đã khử trùng ở bước build này. Trang
 * /huong-dan/[slug] dùng dangerouslySetInnerHTML với đúng lý do đó — xem ghi
 * chú tại nơi dùng.
 *
 * Chạy: npm run huong-dan:import   (cần Node 22+, dùng --experimental-strip-types)
 */

import { readFile, writeFile, mkdir, copyFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import * as cheerio from "cheerio";
import sanitizeHtml from "sanitize-html";

/**
 * Đọc kích thước ảnh PNG/JPEG thẳng từ phần đầu tệp, không giải mã ảnh.
 *
 * Viết tay thay vì dùng một thư viện đa định dạng (`image-size`): thư viện đó
 * có lỗ hổng từ chối dịch vụ đã biết trong bộ phân tích ICNS/JXL/HEIF, không
 * có bản vá. Toàn bộ 51 ảnh trong BIN/help/image chỉ có hai định dạng PNG và
 * JPEG, nên chỉ cần đọc đúng hai định dạng đó — diện tấn công nhỏ hơn nhiều.
 */
function readImageSize(buffer) {
  // PNG: 8 byte chữ ký, rồi IHDR chunk chứa width/height ở offset 16/20 (big-endian).
  if (buffer.length > 24 && buffer.readUInt32BE(0) === 0x89504e47) {
    return { width: buffer.readUInt32BE(16), height: buffer.readUInt32BE(20) };
  }
  // JPEG: quét các segment SOFx (0xC0–0xC3, 0xC5–0xC7, ...) để lấy width/height.
  if (buffer.length > 4 && buffer[0] === 0xff && buffer[1] === 0xd8) {
    let offset = 2;
    while (offset + 9 < buffer.length) {
      if (buffer[offset] !== 0xff) {
        offset += 1;
        continue;
      }
      const marker = buffer[offset + 1];
      const isSOF =
        marker >= 0xc0 && marker <= 0xcf && marker !== 0xc4 && marker !== 0xc8 && marker !== 0xcc;
      const segmentLength = buffer.readUInt16BE(offset + 2);
      if (isSOF) {
        return {
          height: buffer.readUInt16BE(offset + 5),
          width: buffer.readUInt16BE(offset + 7),
        };
      }
      offset += 2 + segmentLength;
    }
  }
  return null;
}

const here = path.dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = path.join(here, "..", "..");
const HELP_SRC = path.join(REPO_ROOT, "BIN", "help");
const IMAGE_SRC = path.join(HELP_SRC, "image");
const IMAGE_DEST = path.join(here, "..", "public", "images", "help");
const OUT_JSON = path.join(here, "..", "src", "lib", "content", "huong-dan-data.json");

/** slug hoá tên tệp: "Quan_ly_tai_khoan" -> "quan-ly-tai-khoan" */
function slugify(stem) {
  return stem.toLowerCase().replaceAll("_", "-");
}

/**
 * Mục lục thật, giữ đúng thứ tự và tựa đề như index.htm gốc — đây là bảng
 * chức năng chính do tác giả sắp xếp, không phải suy đoán.
 */
const TOC = [
  { file: "cai_dat.htm", title: "Cài đặt chương trình", group: "Bắt đầu" },
  { file: "cap_nhat.htm", title: "Cập nhật chương trình", group: "Bắt đầu" },
  {
    file: "thao_tac_chung.htm",
    title: "Hướng dẫn những thao tác chung (nên đọc kỹ)",
    group: "Bắt đầu",
  },
  { file: "cach_nhap_lieu.htm", title: "Hướng dẫn bắt đầu nhập dữ liệu", group: "Bắt đầu" },
  {
    file: "giao_xu.htm",
    title: "Nhập thông tin Giáo xứ, Linh mục",
    group: "Nhập & quản lý dữ liệu",
  },
  { file: "giao_ho.htm", title: "Nhập thông tin Giáo họ", group: "Nhập & quản lý dữ liệu" },
  { file: "giao_dan.htm", title: "Quản lý thông tin giáo dân", group: "Nhập & quản lý dữ liệu" },
  { file: "gia_dinh.htm", title: "Quản lý thông tin gia đình", group: "Nhập & quản lý dữ liệu" },
  {
    file: "giao_dan_luu_tru.htm",
    title: "Quản lý hồ sơ lưu trữ giáo dân",
    group: "Nhập & quản lý dữ liệu",
  },
  {
    file: "gia_dinh_luu_tru.htm",
    title: "Quản lý hồ sơ lưu trữ gia đình",
    group: "Nhập & quản lý dữ liệu",
  },
  { file: "dot_bi_tich.htm", title: "Quản lý các đợt bí tích", group: "Nhập & quản lý dữ liệu" },
  {
    file: "rao_hon_phoi.htm",
    title: "Lập danh sách rao hôn phối",
    group: "Nhập & quản lý dữ liệu",
  },
  {
    file: "chuyen_ho.htm",
    title: "Chuyển giáo họ hàng loạt",
    group: "Nhập & quản lý dữ liệu",
  },
  { file: "tim_kiem.htm", title: "Các cách tìm kiếm", group: "Tra cứu & xử lý dữ liệu" },
  { file: "kiem_tra_du_lieu.htm", title: "Kiểm tra dữ liệu", group: "Tra cứu & xử lý dữ liệu" },
  { file: "thong_ke.htm", title: "Thống kê", group: "Tra cứu & xử lý dữ liệu" },
  {
    file: "sao_luu_khoi_phuc.htm",
    title: "Sao lưu và khôi phục dữ liệu",
    group: "Vận hành & an toàn dữ liệu",
  },
  {
    file: "ket_noi_du_lieu.htm",
    title: "Nhập / kết nối dữ liệu từ nhiều máy và sao lưu dữ liệu",
    group: "Vận hành & an toàn dữ liệu",
  },
  { file: "tuy_chon.htm", title: "Các tùy chọn", group: "Vận hành & an toàn dữ liệu" },
  {
    file: "Quan_ly_tai_khoan.htm",
    title: "Quản lý tài khoản",
    group: "Vận hành & an toàn dữ liệu",
  },
  { file: "lien_he.htm", title: "Liên hệ tác giả", group: "Hỗ trợ" },
];

const SLUG_BY_FILE = new Map(TOC.map((e) => [e.file, slugify(path.parse(e.file).name)]));

const SANITIZE_OPTIONS = {
  allowedTags: [
    "h1",
    "h2",
    "h3",
    "h4",
    "p",
    "br",
    "strong",
    "b",
    "em",
    "i",
    "u",
    "ul",
    "ol",
    "li",
    "blockquote",
    "a",
    "img",
    "span",
    "div",
    "sup",
    "sub",
    "table",
    "thead",
    "tbody",
    "tr",
    "td",
    "th",
  ],
  allowedAttributes: {
    a: ["href", "target", "rel"],
    img: ["src", "alt", "width", "height"],
  },
  allowedSchemes: ["http", "https", "mailto"],
  transformTags: {
    a: (tagName, attribs) => {
      const href = attribs.href ?? "";
      const isInternal = href.startsWith("/") || href === "";
      return {
        tagName: "a",
        attribs: isInternal
          ? { href }
          : { href, target: "_blank", rel: "noopener noreferrer" },
      };
    },
  },
};

async function copyReferencedImage(srcRelPath) {
  // srcRelPath dạng "image/setup_step1.JPG" — bỏ tiền tố "image/".
  const fileName = srcRelPath.replace(/^image\//i, "");
  const srcPath = path.join(IMAGE_SRC, fileName);
  const destName = fileName.toLowerCase();
  const destPath = path.join(IMAGE_DEST, destName);

  if (!existsSync(srcPath)) {
    console.warn(`  ! Thiếu ảnh: ${fileName}`);
    return null;
  }
  await copyFile(srcPath, destPath);

  const buffer = await readFile(srcPath);
  const size = readImageSize(buffer);
  if (!size) console.warn(`  ! Không đọc được kích thước ảnh: ${fileName}`);
  return { publicPath: `/images/help/${destName}`, width: size?.width ?? null, height: size?.height ?? null };
}

async function processFile(entry) {
  const raw = await readFile(path.join(HELP_SRC, entry.file), "utf8");
  const withoutBom = raw.replace(/^﻿/, "");
  const $ = cheerio.load(withoutBom, { decodeEntities: true });

  $("style, script").remove();

  // Dòng điều hướng cũ: <p>[Trở về mục lục] [Trang chủ]</p>
  $("p").each((_, el) => {
    const text = $(el).text().replace(/\s+/g, " ").trim();
    if (/^\[.*Trở về mục lục.*\]/.test(text) || /^\[.*Trang chủ.*\]$/.test(text)) {
      $(el).remove();
    }
  });

  // Viết lại link nội bộ và link tải bản cũ đã chết.
  $("a[href]").each((_, el) => {
    const href = $(el).attr("href") ?? "";
    if (/quanlygiaoxu\.net\/download\.aspx/i.test(href)) {
      $(el).attr("href", "/#tai-ve");
      return;
    }
    const fileMatch = href.match(/^([A-Za-z0-9_]+\.htm)(#.*)?$/);
    if (fileMatch && SLUG_BY_FILE.has(fileMatch[1])) {
      $(el).attr("href", `/huong-dan/${SLUG_BY_FILE.get(fileMatch[1])}`);
    }
  });

  // Viết lại ảnh: copy file, gán kích thước thật để tránh nhảy layout.
  const imgs = $("img[src]").toArray();
  for (const el of imgs) {
    const src = $(el).attr("src") ?? "";
    if (!src.startsWith("image/")) continue;
    const info = await copyReferencedImage(src);
    if (!info) {
      $(el).remove();
      continue;
    }
    $(el).attr("src", info.publicPath);
    if (info.width && info.height) {
      $(el).attr("width", String(info.width));
      $(el).attr("height", String(info.height));
    }
    if (!$(el).attr("alt")) $(el).attr("alt", entry.title);
  }

  const bodyHtml = $("body").html() ?? "";
  const clean = sanitizeHtml(bodyHtml, SANITIZE_OPTIONS).trim();

  return {
    slug: SLUG_BY_FILE.get(entry.file),
    title: entry.title,
    group: entry.group,
    html: clean,
  };
}

async function main() {
  await mkdir(IMAGE_DEST, { recursive: true });

  const pages = [];
  for (const entry of TOC) {
    console.log(`Đang xử lý ${entry.file} ...`);
    pages.push(await processFile(entry));
  }

  await writeFile(OUT_JSON, JSON.stringify(pages, null, 2), "utf8");
  console.log(`\nĐã ghi ${OUT_JSON} (${pages.length} trang).`);
}

main();
