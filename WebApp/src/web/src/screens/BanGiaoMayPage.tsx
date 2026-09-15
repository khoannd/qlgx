import { useCallback, useEffect, useState } from 'react'
import { demHangCho } from '../kho/hangCho'
import { api } from '../api/client'
import { layTrangThaiBuLai, layTrangThaiOffline } from '../dongbo/khoiDongOffline'

/**
 * Trang "Bàn giao máy này" — spec 9 (thanh trạng thái → bấm vào → "Bàn giao máy này"). Liệt kê số
 * việc chưa gửi (`demHangCho(kho)`, Task 2) và số mục cần xem lại (`GET /api/can-xem-lai`, Task 6/9)
 * — CHỈ khi CẢ HAI bằng 0 mới hiện nút xanh "Đã gửi về hết. Có thể gỡ máy này ra an toàn." — đúng
 * quy tắc cứng của brief: không được cho người dùng yên tâm gỡ máy khi còn dữ liệu chưa lên máy chủ.
 */
export function BanGiaoMayPage() {
  const [soHangCho, setSoHangCho] = useState<number | null>(null)
  const [soCanXemLai, setSoCanXemLai] = useState<number | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  // I2 (fix round cuối): hai lý do NGOÀI "hàng chờ/cần xem lại đều 0" vẫn cấm hiện nút xanh — xem
  // `anToan` bên dưới. Đọc MỘT LẦN mỗi lượt `taiLai()` (cùng nhịp với hai con số kia) thay vì gọi
  // thẳng trong thân render, để cả trang luôn nhất quán với đúng một lượt kiểm tra.
  const [dungDoMayChuDiLui, setDungDoMayChuDiLui] = useState(false)
  const [dangBuLai, setDangBuLai] = useState(false)

  const taiLai = useCallback(async () => {
    setLoi(null)
    setDangBuLai(layTrangThaiBuLai().dangBu)
    const tt = layTrangThaiOffline()
    if (tt) {
      setDungDoMayChuDiLui(tt.dieuKhien.trangThai() === 'dung_do_may_chu_di_lui')
      try {
        setSoHangCho(await demHangCho(tt.kho))
      } catch (e) {
        setLoi(e instanceof Error ? e.message : 'Không đếm được số việc chưa gửi.')
        // N1: KHÔNG giữ giá trị cũ khi lượt kiểm tra này thất bại — trả về "chưa biết" (`null`,
        // đúng khuôn `ThanhTrangThai.tsx`) để nút xanh không hiện dựa trên số liệu cũ đã lỗi thời.
        setSoHangCho(null)
      }
    } else {
      setDungDoMayChuDiLui(false)
      // Tầng offline chưa sẵn sàng — KHÔNG được suy ra 0 (xem quy tắc cứng ở ThanhTrangThai.tsx,
      // cùng lý do áp dụng ở đây: một trang cho phép "gỡ máy an toàn" càng không được đoán mò).
      setSoHangCho(null)
    }
    try {
      const ds = await api.canXemLai.danhSach()
      setSoCanXemLai(ds.length)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không tải được danh sách cần xem lại.')
      // N1: xem chú thích ở nhánh `demHangCho` phía trên — mất mạng lúc bấm "Kiểm tra lại" mà giữ
      // nguyên con số cũ (ví dụ 0 của lượt trước) là đúng đường hiện nút xanh trên số liệu chưa xác
      // nhận lại được, đúng thứ trang này tuyệt đối không được làm.
      setSoCanXemLai(null)
    }
  }, [])

  useEffect(() => {
    taiLai()
  }, [taiLai])

  const dangTai = soHangCho === null || soCanXemLai === null
  // I2 (fix round cuối): hai con số bằng 0 CHƯA đủ để nói "gỡ máy này an toàn".
  // - LỚP 2 (`dung_do_may_chu_di_lui`): bộ đồng bộ đã TỰ DỪNG vì nghi máy chủ bị đưa về bản cũ —
  //   máy này có thể đang là bản sao CUỐI CÙNG còn dữ liệu, và hàng chờ rỗng chỉ vì không còn gửi
  //   gì nữa, không phải vì đã gửi xong.
  // - Đang bù lại: dữ liệu đang trên đường gửi lại sau khi máy chủ được khôi phục — con số đọc
  //   được ngay lúc này là ảnh chụp giữa chừng, chưa phải kết quả cuối.
  const anToan = !dangTai && soHangCho === 0 && soCanXemLai === 0 && !dungDoMayChuDiLui && !dangBuLai

  return (
    <div style={{ padding: 16 }}>
      <h2>Bàn giao máy này</h2>
      {loi && <p role="alert">{loi}</p>}
      {dangTai && !loi && <p>Đang kiểm tra…</p>}
      {!dangTai && (
        <>
          <p>Số việc chưa gửi lên máy chủ: {soHangCho}</p>
          <p>Số mục cần xem lại: {soCanXemLai}</p>
          {anToan ? (
            <button
              type="button"
              style={{ background: '#1a7f37', color: 'white', border: 'none', borderRadius: 6, padding: '8px 16px' }}
            >
              Đã gửi về hết. Có thể gỡ máy này ra an toàn.
            </button>
          ) : dungDoMayChuDiLui ? (
            // I2: câu chữ nói ĐÚNG nguyên nhân thật, không gộp chung vào câu "còn dữ liệu chưa gửi"
            // (ở đây hàng chờ có thể đã rỗng — người dùng đọc câu kia sẽ không hiểu vì sao bị chặn).
            <p>
              Máy chủ có dấu hiệu vừa bị đưa về bản cũ nên việc gửi dữ liệu đã tạm dừng — CHƯA nên
              gỡ máy này ra, đây có thể là bản sao cuối cùng còn giữ dữ liệu. Xin báo người hỗ trợ.
            </p>
          ) : dangBuLai ? (
            <p>
              Đang gửi lại dữ liệu sau khi máy chủ được khôi phục — xin đợi gửi xong rồi bấm
              &quot;Kiểm tra lại&quot;.
            </p>
          ) : (
            <p>
              Máy này còn dữ liệu chưa gửi lên máy chủ hoặc còn việc cần xem lại — CHƯA nên gỡ máy
              này ra hoặc xoá dữ liệu, có thể mất những gì chưa gửi được.
            </p>
          )}
        </>
      )}
      <button type="button" onClick={taiLai}>Kiểm tra lại</button>
    </div>
  )
}
