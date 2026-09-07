/**
 * Lịch sử các phiên bản đã phát hành — thay cho chủ đề "Thông tin cập nhật"
 * trên diễn đàn (forum.quanlygiaoxu.net), nơi thông tin này từng nằm duy nhất.
 * Diễn đàn dự kiến sẽ ngừng hoạt động trong tương lai, nên trang này là nơi
 * lưu giữ lâu dài — không phụ thuộc vào việc diễn đàn còn sống hay không.
 *
 * Nội dung chi tiết (mục "cải tiến"/"sửa lỗi") của các bản 3.3.3 → 3.3.7 đối
 * chiếu giữa hai nguồn: bài đăng gốc trên diễn đàn VÀ `Release/VersionConfig.xml`
 * (tệp mà chính bản desktop dùng để tự báo cập nhật cho người dùng — coi là
 * nguồn xác thực hơn khi hai nguồn lệch nhau, vì đây là dữ liệu tác giả tự
 * duy trì lâu dài, không phải một bài đăng viết một lần). Bản 4.0.0 và 4.0.1
 * lấy từ nội dung bài viết cùng tên trong static-provider.ts — cùng một
 * nguồn, không viết lại hai lần. Các bản trước 3.3.3 chỉ còn số phiên bản và
 * ngày (lấy từ tiêu đề chủ đề trên diễn đàn) vì không có bản cài đặt nào
 * tương ứng còn giữ trong `Release/` để kiểm chứng lại nội dung — liệt kê cho
 * đủ dòng lịch sử, không suy diễn thêm chi tiết.
 *
 * LỖI ĐÃ SỬA: bản nháp đầu tiên gán nội dung của bài đăng diễn đàn tiêu đề
 * "Phiên bản 3.3.5 [23-06-2020]" cho đúng phiên bản 3.3.5. Đối chiếu lại với
 * VersionConfig.xml thì nội dung và ngày đó (23-06-2020) thật ra là của bản
 * 3.3.4, còn 3.3.5 (26-06-2020) chỉ có một dòng sửa lỗi ngắn — khả năng cao
 * bài đăng trên diễn đàn khi đó tự tay ghi nhầm số phiên bản trong tiêu đề.
 *
 * Đây là dữ liệu tĩnh (không sửa qua /admin), giống `screenshots` trong
 * site.ts: thêm một bản mới vào đây LUÔN đi kèm việc thêm tệp cài đặt thật vào
 * `Release/` — một việc làm ở tầng mã nguồn, không phải nội dung biên tập.
 */

export type VersionHistoryEntry = {
  /** Hiển thị làm tiêu đề dòng — ví dụ "3.3.7" hoặc "Ra mắt phần mềm". */
  label: string;
  /** ISO 8601 (YYYY-MM-DD). Bỏ trống nếu chưa xác minh được ngày chính xác. */
  date?: string;
  /** Bỏ trống nếu không có nguồn tin cậy để liệt kê chi tiết (ví dụ 3.3.4). */
  highlights?: string[];
  /** Chỉ đặt khi tệp cài đặt tương ứng còn tồn tại thật trong Release/. */
  download?: { fileName: string; size: string };
  /** Bài đăng gốc trên diễn đàn — có thể mất hiệu lực khi diễn đàn ngừng hoạt động. */
  sourceUrl?: string;
};

const FORUM_BASE = "https://forum.quanlygiaoxu.net";

export const versionHistory: VersionHistoryEntry[] = [
  {
    label: "4.0.2",
    date: "2026-09-07",
    highlights: [
      "Sửa lỗi nghiêm trọng: cài đè bộ cài 4.0.0 hoặc 4.0.1 lên máy đã có QLGX có thể chạy xong, báo thành công, nhưng chương trình thật ra vẫn là bản cũ (đã kiểm chứng bằng cách cài thật). Bản 4.0.2 sửa triệt để.",
      "Bộ cài gọn lại chỉ còn một tệp duy nhất.",
      "Thư mục cài đặt mặc định chuyển từ ổ D: sang ổ C: — nhiều máy, nhất là máy xách tay, không có ổ D:.",
      "Giao diện cài đặt quay lại tiếng Anh (bản 4.0.1 dịch tiếng Việt nhưng bị lỗi hiển thị trống chữ do một lỗi cú pháp khi dịch).",
    ],
    download: { fileName: "qlgx_4_0_2.exe", size: "Khoảng 8,2 MB" },
  },
  {
    label: "4.0.1",
    date: "2026-09-06",
    highlights: [
      "Bộ cài nay có đủ thư mục Template, Resources và help — bản 4.0.0 thiếu ba thư mục này nên máy cài mới không in được biểu mẫu và không mở được hướng dẫn.",
      "Sửa lỗi lối tắt trên Desktop và Start Menu chỉ mở ra một thư mục thay vì mở chương trình.",
      "Toàn bộ màn hình cài đặt nay bằng tiếng Việt.",
      "Thêm mục Công cụ › Sửa lỗi, gồm sửa lỗi màn hình hôn phối không hiện tên vợ chồng và sửa lỗi không xuất được Word/Excel.",
    ],
    download: { fileName: "qlgx_4_0_1.exe", size: "Khoảng 8,2 MB" },
  },
  {
    label: "4.0.0",
    date: "2026-09-04",
    highlights: [
      "Chuyển từ .NET Framework 2.0 lên 4.8 để chạy ổn định trên Windows 10 và Windows 11.",
      "Làm lại cách kết nối với Microsoft Word và Excel cho phù hợp các phiên bản Office mới.",
      "Chương trình tự nhận ra lỗi kết nối Office và mời sửa ngay tại chỗ báo lỗi.",
      "Nhập đầy đủ hơn dữ liệu từ phần mềm MGC, kể cả người chỉ có tên trong Sổ Rửa tội.",
      "Sửa lỗi mục Hôn phối trong Sổ bí tích chỉ hiện mã số thay vì tên người chồng và người vợ.",
    ],
    download: { fileName: "qlgx_4_0_0.exe", size: "Khoảng 4,3 MB" },
  },
  {
    label: "3.3.7",
    date: "2024-07-07",
    highlights: [
      "Hiển thị thêm thông tin qua đời trong màn hình danh sách giáo dân.",
      "Hiển thị Tên Cha, Tên Mẹ, Ngày Xưng tội Rước lễ vào danh sách học sinh giáo lý.",
      "Cho phép chọn giáo xứ khi nhập từ phần mềm MGC nếu tệp dữ liệu có nhiều hơn một giáo xứ.",
      "Thêm thông tin tên người vợ/chồng vào mẫu in Lý lịch cá nhân.",
      "Cho phép nhập ngày bí tích sau ngày hiện tại, chỉ cảnh báo chứ không chặn khi lưu.",
    ],
    download: { fileName: "qlgx_3_3_7.exe", size: "Khoảng 8,9 MB" },
    sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.7_[07_07_2024]-2362.html`,
  },
  {
    label: "3.3.6",
    date: "2022-02-28",
    highlights: [
      "Thêm thông tin bao đồng, Giáo lý Hôn nhân vào chi tiết giáo dân.",
      "Sửa một số lỗi khi nhập dữ liệu từ Excel.",
    ],
    download: { fileName: "qlgx_3_3_6.exe", size: "Khoảng 8,7 MB" },
    sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.6_[28_02_2022]-2361.html`,
  },
  {
    label: "3.3.5",
    // Theo VersionConfig.xml — không phải bài đăng diễn đàn ghi "3.3.5", xem
    // ghi chú "LỖI ĐÃ SỬA" ở đầu tệp.
    date: "2020-06-26",
    highlights: ["Sửa lỗi không hiển thị được tên vợ chồng trong màn hình thống kê hôn phối."],
    download: { fileName: "qlgx_3_3_5.exe", size: "Khoảng 8,7 MB" },
  },
  {
    label: "3.3.4",
    // Nội dung này diễn đàn từng đăng dưới tiêu đề "Phiên bản 3.3.5
    // [23-06-2020]" — VersionConfig.xml cho thấy cả nội dung lẫn ngày thật ra
    // thuộc về 3.3.4. Xem ghi chú "LỖI ĐÃ SỬA" ở đầu tệp.
    date: "2020-06-23",
    highlights: [
      "Màn hình chọn giáo dân vào gia đình, sổ bí tích hoặc lớp giáo lý: cho tìm theo tên thay vì hiện hết cả giáo họ.",
      "Cải tiến chức năng nhập dữ liệu từ phần mềm MGC để nhập được nhiều trường hợp hơn.",
      "Thêm mục \"Ngày sinh\" và \"Địa chỉ\" vào màn hình tìm kiếm giáo dân.",
      "Sửa lỗi thống kê hôn phối báo lỗi và không in được danh sách kết quả trong một số trường hợp.",
    ],
    download: { fileName: "qlgx_3_3_4.exe", size: "Khoảng 8,7 MB" },
    sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.5_[23_06_2020]-2360.html`,
  },
  {
    label: "3.3.3",
    date: "2020-05-04",
    highlights: [
      "Cho phép xem gia đình từ màn hình cá nhân giáo dân.",
      "Cho phép xoá hình ảnh gia đình, cá nhân.",
      "Khi đánh dấu một người qua đời, tự tìm vợ/chồng người đó để đưa về tình trạng độc thân.",
      "Sửa lỗi in phiếu gia đình báo lỗi khi chọn in người chứng hôn phối.",
      "Sửa lỗi thống kê hôn phối không hiện giáo họ, tên vợ chồng bị thay bằng mã giáo dân.",
    ],
    download: { fileName: "qlgx_3_3_3.exe", size: "Khoảng 8,7 MB" },
    sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.3_[04_05_2020]-2359.html`,
  },

  // Các bản trước đây không còn tệp cài đặt để cung cấp tải về — chỉ còn mốc
  // thời gian, lấy từ tiêu đề chủ đề trên diễn đàn (mục "Thông tin cập nhật").
  { label: "3.3.2", date: "2020-03-30", sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.2_[30_03_2020]-2357.html` },
  { label: "3.3.1", date: "2020-03-15", sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.1_[15_03_2020]-2356.html` },
  { label: "3.3.0", date: "2020-01-04", sourceUrl: `${FORUM_BASE}/phiên_bản_3.3.0_[04_01_2020]-2355.html` },
  { label: "3.2.2", date: "2019-08-11", sourceUrl: `${FORUM_BASE}/phiên_bản_3.2.2_[11_08_2019]-2354.html` },
  { label: "3.2.1", date: "2018-12-30", sourceUrl: `${FORUM_BASE}/phiên_bản_3.2.1_[30_12_2018]-2353.html` },
  { label: "3.2", date: "2018-12-11", sourceUrl: `${FORUM_BASE}/phiên_bản_3.2_[11_12_2018]-1938.html` },
  { label: "3.1", date: "2018-12-04", sourceUrl: `${FORUM_BASE}/phiên_bản_3.1_[04_12_2018]-1906.html` },
  { label: "2.22", date: "2016-04-09", sourceUrl: `${FORUM_BASE}/phiên_bản_2.22_[09_04_2016]-1540.html` },
  { label: "2.21", date: "2015-12-22", sourceUrl: `${FORUM_BASE}/phiên_bản_2.21_[22_12_2015]-973.html` },
  { label: "2.20", date: "2015-10-25", sourceUrl: `${FORUM_BASE}/phiên_bản_2.20_[25_10_2015]-969.html` },
  { label: "2.1.1.7", date: "2015-08-09", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.7_[09_08_2015]-709.html` },
  { label: "2.1.1.6", date: "2015-07-16", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.6_[16_07_2015]-678.html` },
  { label: "2.1.1.5", date: "2015-01-05", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.5_[05_01_2015]-514.html` },
  { label: "2.1.1.4", date: "2015-01-04", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.4_[04_01_2015]-513.html` },
  { label: "2.1.1.2", date: "2014-12-04", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.2_[04_12_2014]-507.html` },
  { label: "2.1.1.0", date: "2012-09-25", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.1.0_[25_09_2012]-408.html` },
  { label: "2.1.0.9", date: "2012-09-23", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.9_[23_09_2012]-405.html` },
  { label: "2.1.0.8", date: "2012-09-19", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.8_[19_09_2012]-402.html` },
  { label: "2.1.0.7", date: "2012-03-06", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.7_[06_03_2012]-358.html` },
  { label: "2.1.0.6", date: "2012-03-05", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.6_[05_03_2012]-356.html` },
  { label: "2.1.0.5", date: "2011-10-19", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.5_[19_10_2011]-307.html` },
  { label: "2.1.0.4", date: "2011-10-18", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.4_[18_10_2011]-302.html` },
  { label: "2.1.0.3", date: "2011-07-17", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.3_[17_07_2011]-249.html` },
  { label: "2.1.0.2", date: "2011-06-22", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.2_[22_06_2011]-226.html` },
  { label: "2.1.0.1", date: "2011-03-18", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.1_[18_03_2011]-198.html` },
  { label: "2.1.0.0", date: "2011-03-11", sourceUrl: `${FORUM_BASE}/phiên_bản_2.1.0.0_[11_03_2011]-197.html` },
  { label: "2.0.0.4", date: "2010-06-07", sourceUrl: `${FORUM_BASE}/phiên_bản_2.0.0.4_[07_06_2010]-196.html` },
  { label: "2.0.0.3", date: "2010-03-10", sourceUrl: `${FORUM_BASE}/phiên_bản_2.0.0.3_[10_03_2010]-195.html` },
  { label: "2.0.0.2", date: "2010-01-03", sourceUrl: `${FORUM_BASE}/phiên_bản_2.0.0.2_[03_01_2010]-194.html` },
  { label: "2.0.0.1", date: "2009-12-07", sourceUrl: `${FORUM_BASE}/phiên_bản_2.0.0.1_[07_12_2009]-193.html` },
  { label: "2.0.0.0", date: "2009-11-06", sourceUrl: `${FORUM_BASE}/phiên_bản_2.0.0.0_[06_11_2009]-192.html` },
  { label: "1.0.0.3", date: "2009-07-31", sourceUrl: `${FORUM_BASE}/phiên_bản_1.0.0.3_[31_07_2009]-191.html` },
  { label: "1.0.0.2", date: "2009-07-24", sourceUrl: `${FORUM_BASE}/phiên_bản_1.0.0.2_[24_07_2009]-190.html` },
  { label: "Ra mắt phần mềm", date: "2009-07-21", sourceUrl: `${FORUM_BASE}/ra_mắt_phần_mềm_[21_07_2009]-189.html` },
];

/** Chỉ những bản còn tệp cài đặt thật — dùng làm danh sách trắng cho endpoint tải. */
export const downloadableVersions = versionHistory.filter(
  (v): v is VersionHistoryEntry & { download: NonNullable<VersionHistoryEntry["download"]> } =>
    Boolean(v.download),
);
