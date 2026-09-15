import { useCallback, useEffect, useRef, useState } from 'react'
import { demHangCho, docHangCho, type DongHangCho } from '../kho/hangCho'
import { docNoiDungFileDuPhong, napLaiFileDuPhong } from '../dongbo/tepDuPhong'
import { layThietBiId, type TrangThaiBoDongBo } from '../dongbo/boDongBo'
import { layTrangThaiOffline, layTrangThaiBuLai } from '../dongbo/khoiDongOffline'
import { api } from '../api/client'

/**
 * Thanh trạng thái — spec 9.1. Một dòng, một chấm màu, LUÔN nhìn thấy (gắn ở `AppShell`, ngoài
 * phạm vi task này để gắn dây — component này TỰ ĐỦ, không cần props, đọc thẳng
 * `layTrangThaiOffline()` (Task 10 mục "nối dây") + gọi `/api/can-xem-lai`).
 *
 * QUY TẮC CỨNG (spec 9.1, brief): "không bao giờ hiện 'Đã lưu an toàn' khi thay đổi còn nằm trong
 * máy" — `tinhTrangThai()` dưới đây là hàm THUẦN tách riêng để test được trực tiếp bất biến này mà
 * không cần dựng cả component (mutation testing nhắm chủ yếu vào hàm này, xem báo cáo Task 10).
 */

export type MauTrangThai = 'xanh' | 'vang' | 'do'
export type KetQuaTinhTrangThai = { mau: MauTrangThai; dongChu: string }

/**
 * Quy tắc màu (spec 9.1) — thứ tự kiểm tra CỐ Ý: 🔴 (hai lý do độc lập, câu chữ khác nhau — spec
 * 4.8.6/task-10-brief.md) LUÔN được xét TRƯỚC 🟡/🟢, rồi mới tới 🟡 (hàng chờ chưa rỗng), CUỐI CÙNG
 * mới tới 🟢. Nhờ thứ tự này, 🟢 CHỈ có thể trả về khi `soHangCho === 0` — đây chính là bất biến
 * "không bao giờ hiện xanh khi hàng chờ chưa rỗng" mà brief yêu cầu kiểm thử tường minh.
 */
export function tinhTrangThai(
  soHangCho: number,
  soCanXemLai: number,
  trangThaiBo: TrangThaiBoDongBo | null,
  // L3 (fix round Task 11, spec 4.8.6): đang bù lại hàng chờ sau khi phát hiện máy chủ vừa được
  // khôi phục (epoch không khớp) — ưu tiên THẤP hơn `dung_do_may_chu_di_lui` (lỗi nghiêm trọng hơn,
  // xét trước) nhưng CAO hơn `soCanXemLai > 0`/hàng chờ thường, vì đây là một trạng thái tạm thời
  // cần người dùng biết NGAY đang có chuyện gì xảy ra, khác câu chữ "đang gửi về" bình thường.
  dangBuLai = false,
): KetQuaTinhTrangThai {
  if (trangThaiBo === 'dung_do_may_chu_di_lui') {
    return {
      mau: 'do',
      dongChu: 'Máy chủ có dấu hiệu vừa bị đưa về bản cũ — cần người hỗ trợ kiểm tra trước khi tiếp tục',
    }
  }
  if (dangBuLai) {
    return {
      mau: 'vang',
      dongChu: `Máy chủ vừa được khôi phục. Đang gửi lại ${soHangCho} thay đổi mà máy này còn giữ.`,
    }
  }
  if (soCanXemLai > 0) {
    return { mau: 'do', dongChu: `Cần xem lại (${soCanXemLai})` }
  }
  if (soHangCho > 0) {
    return { mau: 'vang', dongChu: `Đã lưu ở máy này (${soHangCho}) — đang gửi về` }
  }
  return { mau: 'xanh', dongChu: 'Đã lưu an toàn' }
}

const MAU_CHAM: Record<MauTrangThai, string> = { xanh: '#1a7f37', vang: '#b98900', do: '#c22c2c' }
const CHU_KY_LAM_MOI_MS = 5000
// I5 (fix round 1): hỏi máy chủ (`/api/can-xem-lai`) không cần nhanh bằng đếm hàng chờ cục bộ (đọc
// IndexedDB — rẻ) — giãn ra một chu kỳ riêng, dài hơn hẳn, để giảm số lượt gọi mạng khi thanh trạng
// thái luôn hiện (mọi màn hình, mọi lúc).
const CHU_KY_HOI_MAY_CHU_MS = 30000

export function ThanhTrangThai({ onMoBanGiaoMay }: { onMoBanGiaoMay?: () => void } = {}) {
  // `null` = CHƯA đọc được lần nào (tầng offline chưa khởi động xong, hoặc lượt đọc đầu chưa
  // xong) — KHÔNG được suy ra 0 một cách lạc quan ở đây: đoán "0" khi chưa thật sự biết là đúng
  // kiểu lỗi quy tắc cứng cấm ("hiện xanh khi chưa chắc hàng chờ đã rỗng"). Component tự ẩn hẳn
  // (return null) cho tới khi có số liệu THẬT đầu tiên.
  const [soHangCho, setSoHangCho] = useState<number | null>(null)
  const [soCanXemLai, setSoCanXemLai] = useState(0)
  // N4: cờ đánh dấu ĐÃ hỏi máy chủ (`/api/can-xem-lai`) thành công ít nhất một lần — khác `soHangCho`
  // (vốn dùng `null` để phân biệt "chưa biết"), `soCanXemLai` khởi tạo `0` nên cần cờ riêng để phân
  // biệt "chưa từng hỏi được" với "hỏi được, và đúng là 0".
  const [daHoiCanXemLaiLanDau, setDaHoiCanXemLaiLanDau] = useState(false)
  const [trangThaiBo, setTrangThaiBo] = useState<TrangThaiBoDongBo | null>(null)
  // L3 (spec 4.8.6): trạng thái "đang bù lại" — đọc từ `khoiDongOffline.ts` (poll cùng nhịp với
  // `lamMoiHangCho`, không cần interval riêng). `tongKetBuLai` là dòng tổng kết "Đã gửi lại N thay
  // đổi." hiện MỘT LẦN khi phát hiện `soDongDaBu` mới (so với giá trị trước đó qua `useRef` —
  // tránh đặt lại state mỗi 5 giây trong lúc `soDongDaBu` không đổi).
  const [dangBuLai, setDangBuLai] = useState(false)
  const [tongKetBuLai, setTongKetBuLai] = useState<string | null>(null)
  const soDongDaBuTruocRef = useRef<number | null>(null)
  const [moRong, setMoRong] = useState(false)
  const [moKyThuat, setMoKyThuat] = useState(false)
  const [hangChoChiTiet, setHangChoChiTiet] = useState<DongHangCho[]>([])
  const [dangNap, setDangNap] = useState(false)
  const [thongBaoNap, setThongBaoNap] = useState<string | null>(null)
  const [daSaoChep, setDaSaoChep] = useState(false)
  const [loiSaoChep, setLoiSaoChep] = useState(false)
  // I2: phiên bản phần mềm (khối "Thông tin kỹ thuật") — cùng cách `SideNav.tsx` lấy (`GET
  // /api/suc-khoe`, ẩn danh). `null` khi chưa tải xong hoặc gọi lỗi.
  const [phienBan, setPhienBan] = useState<string | null>(null)
  // I2: lỗi gần nhất gặp phải khi tự làm mới (đếm hàng chờ hoặc hỏi "cần xem lại") — hiện trong khối
  // "Thông tin kỹ thuật" để người hỗ trợ từ xa có manh mối, không chỉ có trong console.
  const [loiGanNhat, setLoiGanNhat] = useState<string | null>(null)
  const inputTepRef = useRef<HTMLInputElement | null>(null)

  useEffect(() => {
    let huy = false
    api.he.sucKhoe()
      .then((tt) => { if (!huy) setPhienBan(tt.phienBan) })
      .catch(() => { if (!huy) setPhienBan(null) })
    return () => { huy = true }
  }, [])

  const ghiLoiGanNhat = useCallback((loi: unknown) => {
    setLoiGanNhat(`${new Date().toLocaleString('vi-VN')}: ${loi instanceof Error ? loi.message : String(loi)}`)
  }, [])

  // I5: đếm hàng chờ (đọc IndexedDB cục bộ — rẻ) — giữ nguyên chu kỳ 5 giây cũ.
  // L3: đọc `layTrangThaiBuLai()` CÙNG nhịp — không cần interval riêng cho trạng thái "đang bù lại".
  const lamMoiHangCho = useCallback(async () => {
    const trangThaiBu = layTrangThaiBuLai()
    setDangBuLai(trangThaiBu.dangBu)
    if (trangThaiBu.soDongDaBu !== null && trangThaiBu.soDongDaBu !== soDongDaBuTruocRef.current) {
      soDongDaBuTruocRef.current = trangThaiBu.soDongDaBu
      setTongKetBuLai(`Đã gửi lại ${trangThaiBu.soDongDaBu} thay đổi.`)
    }

    const tt = layTrangThaiOffline()
    if (!tt) return
    try {
      const n = await demHangCho(tt.kho)
      setSoHangCho(n)
    } catch (loi) {
      console.error('ThanhTrangThai: khong dem duoc hang cho', loi)
      ghiLoiGanNhat(loi)
    }
    setTrangThaiBo(tt.dieuKhien.trangThai())
  }, [ghiLoiGanNhat])

  // I5: hỏi máy chủ (`/api/can-xem-lai`) — chu kỳ RIÊNG, dài hơn (30s), và bỏ qua hẳn khi biết chắc
  // đang offline (tránh lỗi ồn ào + tránh nguy cơ đá người dùng ra do 401 giữa lúc token có thể còn
  // hợp lệ khi có mạng lại).
  const lamMoiCanXemLai = useCallback(async () => {
    if (!navigator.onLine) return
    try {
      const ds = await api.canXemLai.danhSach()
      setSoCanXemLai(ds.length)
      setDaHoiCanXemLaiLanDau(true)
    } catch (loi) {
      // Mat mang/loi may chu khi hoi so "can xem lai" KHONG duoc lam sap thanh trang thai — giu
      // gia tri cu (co the hoi cu nhung van con hon lam nguoi dung hoang mang vi bien mat).
      console.error('ThanhTrangThai: khong hoi duoc /api/can-xem-lai', loi)
      ghiLoiGanNhat(loi)
    }
  }, [ghiLoiGanNhat])

  useEffect(() => {
    lamMoiHangCho()
    const id = setInterval(lamMoiHangCho, CHU_KY_LAM_MOI_MS)
    return () => clearInterval(id)
  }, [lamMoiHangCho])

  useEffect(() => {
    lamMoiCanXemLai()
    const id = setInterval(lamMoiCanXemLai, CHU_KY_HOI_MAY_CHU_MS)
    return () => clearInterval(id)
  }, [lamMoiCanXemLai])

  // Nạp lại danh sách chi tiết hàng chờ CHỈ khi bảng giải thích đang mở (tránh đọc thêm một
  // transaction IndexedDB mỗi 5 giây khi không ai xem tới).
  useEffect(() => {
    if (!moRong) return
    const tt = layTrangThaiOffline()
    if (!tt) return
    let huy = false
    docHangCho(tt.kho)
      .then((ds) => {
        if (!huy) setHangChoChiTiet(ds)
      })
      .catch((loi) => console.error('ThanhTrangThai: khong doc duoc chi tiet hang cho', loi))
    return () => {
      huy = true
    }
  }, [moRong, soHangCho])

  async function napLaiFileDuPhongTuMay(tep: File) {
    const tt = layTrangThaiOffline()
    if (!tt) {
      setThongBaoNap('Tầng ngoại tuyến chưa sẵn sàng — thử lại sau ít phút.')
      return
    }
    setDangNap(true)
    setThongBaoNap(null)
    try {
      const vanBan = await tep.text()
      const hangCho = docNoiDungFileDuPhong(vanBan)
      await napLaiFileDuPhong(tt.kho, hangCho)
      setThongBaoNap(`Đã nạp lại ${hangCho.length} việc từ file dự phòng.`)
      await lamMoiHangCho()
      await lamMoiCanXemLai()
    } catch (loi) {
      setThongBaoNap(loi instanceof Error ? loi.message : 'Không nạp được file dự phòng.')
    } finally {
      setDangNap(false)
      if (inputTepRef.current) inputTepRef.current.value = ''
    }
  }

  // I2 (fix round 1, spec 9.1): khối "Thông tin kỹ thuật" — dùng chung cho cả bản hiện trên màn
  // hình lẫn bản sao chép clipboard, tránh hai nơi lệch nhau. "Lần gửi gần nhất"/"lần lấy dữ liệu
  // gần nhất" CỐ Ý CHƯA có ở đây — `DieuKhienBoDongBo` (`dongbo/boDongBo.ts`) chưa lưu hai mốc thời
  // gian này, việc thêm nằm NGOÀI phạm vi fix round này (xem task-10-fix-round-1-brief.md mục I2) —
  // hoãn có chủ ý sang task sau, không phải bỏ sót.
  function thongTinKyThuat() {
    return {
      thietBiId: layThietBiId(),
      phienBanPhanMem: phienBan,
      trangThaiBoDongBo: trangThaiBo,
      soViecChuaGui: soHangCho,
      soMucCanXemLai: soCanXemLai,
      loiGanNhat,
      thoiDiem: new Date().toISOString(),
    }
  }

  async function saoChepThongTinKyThuat() {
    const noiDung = JSON.stringify(thongTinKyThuat(), null, 2)
    try {
      await navigator.clipboard.writeText(noiDung)
      setDaSaoChep(true)
      setLoiSaoChep(false)
      setTimeout(() => setDaSaoChep(false), 3000)
    } catch (loi) {
      // N2: loi sao chep (vi du trinh duyet chan Clipboard API) phai bao cho nguoi dung, khong chi
      // nam trong console — ho can biet de tu chon roi sao chep tay.
      console.error('ThanhTrangThai: khong sao chep duoc thong tin ky thuat', loi)
      setLoiSaoChep(true)
    }
  }

  // Chưa có số liệu THẬT nào — không hiện gì (xem chú thích khai báo state ở trên).
  if (soHangCho === null) return null

  const { mau, dongChu } = tinhTrangThai(soHangCho, soCanXemLai, trangThaiBo, dangBuLai)

  return (
    <div className="thanh-trang-thai">
      <button
        type="button"
        className="trang-thai-dongbo"
        onClick={() => setMoRong((v) => !v)}
        aria-expanded={moRong}
      >
        {/* Chấm màu là màu ĐỘNG theo state (xanh/vàng/đỏ) — giữ inline style riêng phần tô màu,
            không hợp để đặt cứng trong CSS class tĩnh; kích thước/bo góc cũng để cùng chỗ cho gọn. */}
        <span
          aria-hidden="true"
          style={{
            display: 'inline-block', width: 10, height: 10, borderRadius: '50%', background: MAU_CHAM[mau],
          }}
        />
        <span>{dongChu}</span>
      </button>

      {moRong && (
        <div className="trang-thai-dongbo-panel" role="region" aria-label="Chi tiết trạng thái lưu dữ liệu">
          <p>
            Những thay đổi này đã lưu trong máy và sẽ tự gửi lên khi có mạng. Anh/chị không cần làm
            gì, cũng không mất dữ liệu.
          </p>

          <p>Số việc đang chờ gửi: {soHangCho}</p>
          {/* L3 (spec 4.8.6): dòng tổng kết sau khi bù xong lần gần nhất — hiện tới khi có lần bù
              tiếp theo/tải lại trang, không tự ẩn sau một khoảng thời gian (xem chú thích khai báo
              trangThaiBuLai ở khoiDongOffline.ts). */}
          {tongKetBuLai && <p>{tongKetBuLai}</p>}
          {/* N4: chưa hỏi máy chủ được lần nào -> soCanXemLai (khởi tạo 0) có thể chưa đúng thật,
              khác han soHangCho (khởi tạo null, đã có gác ở trên) — báo rõ để tránh hiểu nhầm "0" là
              "đã biết chắc = 0". */}
          {!daHoiCanXemLaiLanDau && (
            <p>Chưa hỏi được máy chủ lần nào — số mục cần xem lại có thể chưa cập nhật.</p>
          )}
          {hangChoChiTiet.length > 0 && (
            <ul>
              {hangChoChiTiet.slice(0, 20).map((d) => {
                // N3: DongHangCho đã có `& Record<string, unknown>` sẵn (kho/hangCho.ts) — không cần
                // ép kiểu. Thiếu cả bang lẫn truong -> hiện maThaoTac thay vì " — " rỗng nghĩa.
                const bang = d.bang
                const truong = d.truong
                const nhan = bang == null && truong == null
                  ? String(d.maThaoTac)
                  : `${String(bang ?? '')} — ${String(truong ?? '')}`
                return <li key={d.maThaoTac}>{nhan}</li>
              })}
              {hangChoChiTiet.length > 20 && <li>… và {hangChoChiTiet.length - 20} việc khác</li>}
            </ul>
          )}

          <div>
            <button type="button" className="btn btn-sm btn-quiet" onClick={() => inputTepRef.current?.click()} disabled={dangNap}>
              Nạp lại file dự phòng
            </button>
            <input
              ref={inputTepRef}
              type="file"
              accept="application/json"
              style={{ display: 'none' }}
              onChange={(e) => {
                const tep = e.target.files?.[0]
                if (tep) napLaiFileDuPhongTuMay(tep)
              }}
            />
            {thongBaoNap && <span className="trang-thai-dongbo-inline">{thongBaoNap}</span>}
          </div>

          {onMoBanGiaoMay && (
            <p>
              <button type="button" className="btn btn-sm btn-quiet" onClick={onMoBanGiaoMay}>Bàn giao máy này</button>
            </p>
          )}

          {/* I2: "lần gửi gần nhất"/"lần lấy dữ liệu gần nhất" (spec 9.1) CHƯA có nguồn dữ liệu ở
              tầng này — `DieuKhienBoDongBo` chưa lưu hai mốc thời gian này. Hoãn có chủ ý sang task
              sau (xem `thongTinKyThuat()` ở trên), không phải bỏ sót. */}
          <details open={moKyThuat} onToggle={(e) => setMoKyThuat((e.target as HTMLDetailsElement).open)}>
            <summary>Thông tin kỹ thuật</summary>
            <pre style={{ whiteSpace: 'pre-wrap' }}>
              {JSON.stringify(thongTinKyThuat(), null, 2)}
            </pre>
            <button type="button" className="btn btn-sm btn-quiet" onClick={saoChepThongTinKyThuat}>
              {daSaoChep ? 'Đã sao chép' : 'Sao chép để gửi người hỗ trợ'}
            </button>
            {loiSaoChep && (
              <span role="alert" className="trang-thai-dongbo-inline">
                Không sao chép được — xin chọn rồi sao chép tay đoạn chữ ở trên.
              </span>
            )}
          </details>
        </div>
      )}
    </div>
  )
}
