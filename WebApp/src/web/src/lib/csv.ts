/**
 * Tải một chuỗi CSV xuống máy người dùng dưới dạng tệp — dùng cho nút "Xuất dữ liệu (CSV)"
 * của `GxToolbar` trên các màn hình danh sách (giáo dân, gia đình).
 *
 * Lỗi hay gặp nhất khi xuất CSV tiếng Việt: Excel trên Windows tự đoán bảng mã theo hệ thống
 * (thường là Windows-1252) khi mở một tệp .csv không có dấu hiệu gì về UTF-8, nên chữ có dấu
 * bị vỡ thành ký tự lạ. Thêm BOM UTF-8 (U+FEFF) vào ĐẦU nội dung là cách chuẩn để Excel nhận
 * ra tệp là UTF-8 và hiển thị đúng — AG Grid `getDataAsCsv()` không tự thêm BOM này.
 */
export function taiXuongCsv(noiDungCsv: string, tenFile: string): void {
  const BOM = '﻿'
  const blob = new Blob([BOM + noiDungCsv], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  try {
    const a = document.createElement('a')
    a.href = url
    a.download = tenFile
    document.body.appendChild(a)
    a.click()
    a.remove()
  } finally {
    URL.revokeObjectURL(url)
  }
}
