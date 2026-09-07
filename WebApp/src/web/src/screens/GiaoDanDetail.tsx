import { useEffect, useRef, useState, type FormEvent } from 'react'
import type {
  GiaoDanDetail as GiaoDanDetailDuLieu, GiaoDanTimKiem, GiaoHo, HoiDoanCuaGiaoDan,
  HoiDoanDanhMuc, HonPhoiCuaGiaoDan, TanHienCuaGiaoDan,
} from '../api/types'
import { AnhDaiDien } from '../components/AnhDaiDien'
import { GxDate } from '../components/GxDate'
import { GxField, GxInline } from '../components/GxField'
import { GxFormTabs } from '../components/GxFormTabs'
import { GxGoiY } from '../components/GxGoiY'
import { GxPicker } from '../components/GxPicker'
import { dinhDangNgay } from '../lib/ngay'
import { useTuDongLuuBanNhap, xoaBanNhap } from '../lib/banNhap'

/** Sentinel hiển thị khi chưa chọn giáo họ nào — đúng quy ước "MaGiaoHo = 0 nghĩa là Ngoài xứ"
 * của `frmGiaoDan.cs`. Ở bản web, "Ngoài xứ" ứng với `giaoHoId === null` (không phải khoá
 * ngoại tới một dòng thật trong bảng giao_ho — xem GET /api/giao-ho). */
const NGOAI_XU = 'Ngoài xứ'

/** Các trường gửi lên `PUT /api/giao-dan/hon-phoi/{honPhoiId}` — đúng `CapNhatHonPhoiRequest`
 * phía backend (dùng lại nguyên type đã có từ Task 7, xem GiaDinhDtos.cs). */
export type YeuCauCapNhatHonPhoi = {
  soHonPhoi: string | null
  ngayHonPhoi: string | null
  noiHonPhoi: string | null
  linhMucChung: string | null
  nguoiChung1: string | null
  nguoiChung2: string | null
  cachThucHonPhoi: string | null
  ghiChu: string | null
  rowVersion: number
}

/** Danh sách "Tình trạng hôn phối" — lấy bản 9 giá trị của `GxCachThucHonPhoi` (dùng trong
 * khối hôn phối nhúng ở form gia đình bản desktop), KHÔNG dùng bản 6 giá trị cũ hơn của
 * `frmHonPhoi` (thiếu Ly thân/Ly dị/Đã được tháo gỡ) — quyết định có ý thức, xem
 * docs/superpowers/specs/man-hinh/hon-phoi.md mục 8 và can-review-sau.md mục 12. */
const CACH_THUC_HON_PHOI = [
  '', 'Hợp pháp', 'Hợp thức hóa', 'Chuẩn', 'Không theo phép đạo',
  'Ly thân', 'Ly dị', 'Đã được tháo gỡ', 'Không xác định',
]

/**
 * Một hôn phối trong tab "Hôn phối" — form riêng, lưu độc lập với form giáo dân chính (gọi
 * `PUT /api/giao-dan/hon-phoi/{id}` riêng, không đi qua nút "Cập nhật" ở cuối trang). Chưa hỗ
 * trợ đổi Người nam/Người nữ (cần picker + toàn bộ kiểm tra nghiệp vụ của `frmHonPhoi`, xem
 * hon-phoi.md mục 8) — chỉ xem tên người phối ngẫu và sửa các trường còn lại.
 */
function KhoiHonPhoi({
  hp, thuTu, onLuu, giaoXuId,
}: {
  hp: HonPhoiCuaGiaoDan
  thuTu: number
  onLuu?: (honPhoiId: string, payload: YeuCauCapNhatHonPhoi) => Promise<void>
  giaoXuId: string | null
}) {
  // KHÔNG dùng thẻ <form> ở đây: toàn bộ tab này được `GxFormTabs` render bên trong thẻ
  // <form> duy nhất bọc cả trang của `GiaoDanDetail` (dùng cho nút "Cập nhật" chính) — lồng
  // một <form> thứ hai bên trong là HTML không hợp lệ, và đã đo được hậu quả thật: trình
  // duyệt bỏ qua thẻ <form> lồng, khiến nút submit của khối này kích hoạt submit của form
  // NGOÀI CÙNG (điều hướng GET với toàn bộ giá trị input dồn vào query string thay vì gọi
  // API). Dùng <div> + đọc giá trị input qua querySelector theo `name`, nút kiểu "button".
  const containerRef = useRef<HTMLDivElement>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  function docGiaTri(ten: string): string | null {
    const el = containerRef.current?.querySelector<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>(`[name="${ten}"]`)
    return el?.value.trim() || null
  }

  async function xuLyLuu() {
    if (!onLuu) return
    setDangLuu(true)
    setThongBao(null)
    try {
      await onLuu(hp.id, {
        soHonPhoi: docGiaTri('soHonPhoi'),
        ngayHonPhoi: docGiaTri('ngayHonPhoi'),
        noiHonPhoi: docGiaTri('noiHonPhoi'),
        linhMucChung: docGiaTri('linhMucChung'),
        nguoiChung1: docGiaTri('nguoiChung1'),
        nguoiChung2: docGiaTri('nguoiChung2'),
        cachThucHonPhoi: docGiaTri('cachThucHonPhoi'),
        ghiChu: docGiaTri('ghiChu'),
        rowVersion: hp.rowVersion,
      })
      setThongBao('Đã lưu thành công.')
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  const idBase = `hp-${hp.id}`
  return (
    <div className="card glass" ref={containerRef} style={{ marginBottom: 12 }}>
      <div className="card-head">
        <h2>{hp.tenHonPhoi || `Đôi hôn phối #${thuTu + 1}`}</h2>
        <span className="eyebrow">Người phối ngẫu: <strong>{hp.tenVoChong ?? 'chưa rõ'}</strong></span>
      </div>
      <div className="card-row">
        <div>
          <GxField label="Số hôn phối" id={`${idBase}-so`}>
            <input id={`${idBase}-so`} name="soHonPhoi" type="text" defaultValue={hp.soHonPhoi ?? ''} />
          </GxField>
          <GxField label="Ngày hôn phối" id={`${idBase}-ngay`}>
            <GxDate id={`${idBase}-ngay`} name="ngayHonPhoi" defaultValue={hp.ngayHonPhoi} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Nơi hôn phối" id={`${idBase}-noi`}>
            <GxGoiY id={`${idBase}-noi`} name="noiHonPhoi" truong="noiHonPhoi" giaoXuId={giaoXuId}
              defaultValue={hp.noiHonPhoi} />
          </GxField>
          <GxField label="Linh mục chứng" id={`${idBase}-lm`}>
            <GxGoiY id={`${idBase}-lm`} name="linhMucChung" truong="linhMucChung" giaoXuId={giaoXuId}
              defaultValue={hp.linhMucChung} />
          </GxField>
        </div>
        <div>
          <GxField label="Người chứng 1" id={`${idBase}-c1`}>
            <input id={`${idBase}-c1`} name="nguoiChung1" type="text" defaultValue={hp.nguoiChung1 ?? ''} />
          </GxField>
          <GxField label="Người chứng 2" id={`${idBase}-c2`}>
            <input id={`${idBase}-c2`} name="nguoiChung2" type="text" defaultValue={hp.nguoiChung2 ?? ''} />
          </GxField>
          <GxField label="Tình trạng hôn phối" id={`${idBase}-ct`}>
            <select id={`${idBase}-ct`} name="cachThucHonPhoi" defaultValue={hp.cachThucHonPhoi ?? ''}>
              {CACH_THUC_HON_PHOI.map((c) => <option key={c} value={c}>{c || '(chưa xác định)'}</option>)}
            </select>
          </GxField>
          <GxField label="Ghi chú" id={`${idBase}-ghichu`}>
            <textarea id={`${idBase}-ghichu`} name="ghiChu" defaultValue={hp.ghiChu ?? ''} />
          </GxField>
        </div>
      </div>
      <div className="cmdbar">
        <span className="hint" role={thongBao ? 'status' : undefined}>{thongBao}</span>
        <div className="spacer" />
        <button type="button" className="btn btn-primary" disabled={!onLuu || dangLuu} onClick={xuLyLuu}>
          {dangLuu ? 'Đang lưu…' : 'Cập nhật hôn phối'}
        </button>
      </div>
    </div>
  )
}

/** Các trường gửi lên `POST /api/giao-dan/{id}/tan-hien` và `PUT /api/giao-dan/tan-hien/{id}` —
 * đúng `LuuTanHienRequest` phía backend. */
export type YeuCauLuuTanHien = {
  ngayBatDau: string | null
  chucVu: string | null
  noiTu: string | null
  dongTu: string | null
  noiPhucVu: string | null
  diaChiPhucVu: string | null
  dienThoaiPhucVu: string | null
  emailPhucVu: string | null
  ghiChu: string | null
  daHoiTuc: boolean
  ngayVaoDCV: string | null
  ngayVaoNhaThu: string | null
  ngayVaoNhaTap: string | null
  ngayVaoKhanLanDau: string | null
  ngayVaoKhanTronDoi: string | null
  ngayPhoTe: string | null
  ngayThuPhongLM: string | null
  ngayBonMang: string | null
  rowVersion: number
}

/** 7 giá trị cố định của `cbChucVu` bên `GxTanHien` (không đọc từ CSDL) — xem
 * docs/superpowers/specs/man-hinh/tan-hien.md mục 4. */
const CHUC_VU_TAN_HIEN = ['', 'Tu sĩ', 'Chủng sinh', 'Phó tế', 'Linh mục', 'Giám mục', 'Khấn trọn', 'Khác']

const TAN_HIEN_RONG: Omit<TanHienCuaGiaoDan, 'id' | 'rowVersion'> = {
  ngayBatDau: null, chucVu: null, noiTu: null, dongTu: null, noiPhucVu: null,
  diaChiPhucVu: null, dienThoaiPhucVu: null, emailPhucVu: null, ghiChu: null, daHoiTuc: false,
  ngayVaoDCV: null, ngayVaoNhaThu: null, ngayVaoNhaTap: null, ngayVaoKhanLanDau: null,
  ngayVaoKhanTronDoi: null, ngayPhoTe: null, ngayThuPhongLM: null, ngayBonMang: null,
}

/**
 * Một bản ghi Ơn gọi tận hiến — dùng chung cho khối "đã có" (sửa qua `onLuu`) và khối "thêm
 * giai đoạn mới" (`th` = `undefined`, lưu qua `onThem`). Cùng lý do tránh lồng `<form>` như
 * `KhoiHonPhoi` — xem chú thích ở đó.
 */
function KhoiTanHien({
  th, thuTu, onLuu, onThem,
}: {
  th?: TanHienCuaGiaoDan
  thuTu: number
  onLuu?: (tanHienId: string, payload: YeuCauLuuTanHien) => Promise<void>
  onThem?: (payload: YeuCauLuuTanHien) => Promise<void>
}) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const gt = th ?? { id: `moi-${thuTu}`, rowVersion: 0, ...TAN_HIEN_RONG }

  function docChuoi(ten: string): string | null {
    const el = containerRef.current?.querySelector<HTMLInputElement | HTMLSelectElement>(`[name="${ten}"]`)
    return el?.value.trim() || null
  }
  function docBool(ten: string): boolean {
    return containerRef.current?.querySelector<HTMLInputElement>(`[name="${ten}"]`)?.checked ?? false
  }

  function dungPayload(): YeuCauLuuTanHien {
    return {
      ngayBatDau: docChuoi('ngayBatDau'), chucVu: docChuoi('chucVu'), noiTu: docChuoi('noiTu'),
      dongTu: docChuoi('dongTu'), noiPhucVu: docChuoi('noiPhucVu'),
      diaChiPhucVu: docChuoi('diaChiPhucVu'), dienThoaiPhucVu: docChuoi('dienThoaiPhucVu'),
      emailPhucVu: docChuoi('emailPhucVu'), ghiChu: docChuoi('ghiChu'), daHoiTuc: docBool('daHoiTuc'),
      ngayVaoDCV: docChuoi('ngayVaoDCV'), ngayVaoNhaThu: docChuoi('ngayVaoNhaThu'),
      ngayVaoNhaTap: docChuoi('ngayVaoNhaTap'), ngayVaoKhanLanDau: docChuoi('ngayVaoKhanLanDau'),
      ngayVaoKhanTronDoi: docChuoi('ngayVaoKhanTronDoi'), ngayPhoTe: docChuoi('ngayPhoTe'),
      ngayThuPhongLM: docChuoi('ngayThuPhongLM'), ngayBonMang: docChuoi('ngayBonMang'),
      rowVersion: gt.rowVersion,
    }
  }

  async function xuLyLuu() {
    setDangLuu(true)
    setThongBao(null)
    try {
      if (th && onLuu) {
        await onLuu(th.id, dungPayload())
        setThongBao('Đã lưu thành công.')
      } else if (!th && onThem) {
        await onThem(dungPayload())
        setThongBao('Đã thêm giai đoạn mới.')
      }
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  const idBase = `th-${gt.id}`
  const coTheLuu = th ? !!onLuu : !!onThem
  return (
    <div className="card glass" ref={containerRef} style={{ marginBottom: 12 }}>
      <div className="card-head">
        <h2>{th ? `Ơn gọi tận hiến${gt.chucVu ? ' — ' + gt.chucVu : ''}` : 'Thêm giai đoạn mới'}</h2>
      </div>
      <div className="card-row">
        <div>
          <GxField label="Ngày nhập dòng" id={`${idBase}-batdau`}>
            <GxDate id={`${idBase}-batdau`} name="ngayBatDau" defaultValue={gt.ngayBatDau} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày vào nhà thử" id={`${idBase}-nhathu`}>
            <GxDate id={`${idBase}-nhathu`} name="ngayVaoNhaThu" defaultValue={gt.ngayVaoNhaThu} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày vào nhà tập" id={`${idBase}-nhatap`}>
            <GxDate id={`${idBase}-nhatap`} name="ngayVaoNhaTap" defaultValue={gt.ngayVaoNhaTap} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày vào ĐCV" id={`${idBase}-dcv`}>
            <GxDate id={`${idBase}-dcv`} name="ngayVaoDCV" defaultValue={gt.ngayVaoDCV} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày khấn lần đầu" id={`${idBase}-khan1`}>
            <GxDate id={`${idBase}-khan1`} name="ngayVaoKhanLanDau" defaultValue={gt.ngayVaoKhanLanDau} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày khấn vĩnh viễn" id={`${idBase}-khanvv`}>
            <GxDate id={`${idBase}-khanvv`} name="ngayVaoKhanTronDoi" defaultValue={gt.ngayVaoKhanTronDoi} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày lãnh chức phó tế" id={`${idBase}-phote`}>
            <GxDate id={`${idBase}-phote`} name="ngayPhoTe" defaultValue={gt.ngayPhoTe} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày thụ phong LM" id={`${idBase}-tplm`}>
            <GxDate id={`${idBase}-tplm`} name="ngayThuPhongLM" defaultValue={gt.ngayThuPhongLM} style={{ maxWidth: 180 }} />
          </GxField>
          <GxField label="Ngày mừng bổn mạng" id={`${idBase}-bonmang`}>
            <GxDate id={`${idBase}-bonmang`} name="ngayBonMang" defaultValue={gt.ngayBonMang} style={{ maxWidth: 180 }} />
          </GxField>
        </div>
        <div>
          {/* Nhãn "Địa chỉ" nhưng gắn với cột NoiTu — chép nguyên văn nhãn gốc của GxTanHien
           * Designer, xem tan-hien.md mục 2 (khả năng cao là nhãn đặt sai/đổi ý giữa chừng). */}
          <GxField label="Địa chỉ" id={`${idBase}-noitu`}>
            <input id={`${idBase}-noitu`} name="noiTu" type="text" defaultValue={gt.noiTu ?? ''} />
          </GxField>
          <GxField label="Chức vụ" id={`${idBase}-chucvu`}>
            <select id={`${idBase}-chucvu`} name="chucVu" defaultValue={gt.chucVu ?? ''}>
              {CHUC_VU_TAN_HIEN.map((c) => <option key={c} value={c}>{c || '(chưa xác định)'}</option>)}
            </select>
          </GxField>
          <GxField label="Dòng tu/chủng viện" id={`${idBase}-dongtu`}>
            <input id={`${idBase}-dongtu`} name="dongTu" type="text" defaultValue={gt.dongTu ?? ''} />
          </GxField>
          <GxField label="Nơi phục vụ" id={`${idBase}-noiphucvu`}>
            <input id={`${idBase}-noiphucvu`} name="noiPhucVu" type="text" defaultValue={gt.noiPhucVu ?? ''} />
          </GxField>
          <GxField label="Địa chỉ nơi phục vụ" id={`${idBase}-diachipv`}>
            <input id={`${idBase}-diachipv`} name="diaChiPhucVu" type="text" defaultValue={gt.diaChiPhucVu ?? ''} />
          </GxField>
          <GxField label="Điện thoại nơi phục vụ" id={`${idBase}-dtpv`}>
            <input id={`${idBase}-dtpv`} name="dienThoaiPhucVu" type="text" defaultValue={gt.dienThoaiPhucVu ?? ''} />
          </GxField>
          <GxField label="Email  nơi phục vụ" id={`${idBase}-emailpv`}>
            <input id={`${idBase}-emailpv`} name="emailPhucVu" type="text" defaultValue={gt.emailPhucVu ?? ''} />
          </GxField>
          <GxField label="">
            <label className="toggle">
              <input type="checkbox" name="daHoiTuc" defaultChecked={gt.daHoiTuc} />
              Đã hồi tục
            </label>
          </GxField>
          <GxField label="Ghi chú" id={`${idBase}-ghichu`}>
            <textarea id={`${idBase}-ghichu`} name="ghiChu" defaultValue={gt.ghiChu ?? ''} />
          </GxField>
        </div>
      </div>
      <div className="cmdbar">
        <span className="hint" role={thongBao ? 'status' : undefined}>{thongBao}</span>
        <div className="spacer" />
        <button type="button" className="btn btn-primary" disabled={!coTheLuu || dangLuu} onClick={xuLyLuu}>
          {dangLuu ? 'Đang lưu…' : th ? 'Cập nhật ơn gọi tận hiến' : 'Thêm giai đoạn mới'}
        </button>
      </div>
    </div>
  )
}

/** Các trường gửi lên `POST /api/giao-dan/{id}/hoi-doan` — đúng `ThemHoiDoanRequest` phía
 * backend (không có VaiTro: hard-code "Hội viên" ở tầng dịch vụ, giống `GxHistoryHoiDoan`, xem
 * hoi-doan.md mục 2). */
export type YeuCauThemHoiDoan = {
  hoiDoanId: string
  ngayVaoHoiDoan: string | null
  ngayRaHoiDoan: string | null
}

/** Các trường gửi lên `PUT /api/giao-dan/hoi-doan/{id}` — đúng `CapNhatHoiDoanRequest`. */
export type YeuCauCapNhatHoiDoan = {
  ngayVaoHoiDoan: string | null
  ngayRaHoiDoan: string | null
  vaiTro: string | null
  rowVersion: number
}

/** Một lượt tham gia hội đoàn đã có — sửa Ngày vào/Ngày ra/Vai trò (mở rộng có chủ đích so với
 * bản desktop, nơi `GxHistoryHoiDoan` không cho sửa lượt đã lưu — xem hoi-doan.md mục 8). */
function KhoiHoiDoan({
  hd, onLuu,
}: {
  hd: HoiDoanCuaGiaoDan
  onLuu?: (chiTietId: string, payload: YeuCauCapNhatHoiDoan) => Promise<void>
}) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  function docGiaTri(ten: string): string | null {
    const el = containerRef.current?.querySelector<HTMLInputElement>(`[name="${ten}"]`)
    return el?.value.trim() || null
  }

  async function xuLyLuu() {
    if (!onLuu) return
    setDangLuu(true)
    setThongBao(null)
    try {
      await onLuu(hd.id, {
        ngayVaoHoiDoan: docGiaTri('ngayVaoHoiDoan'),
        ngayRaHoiDoan: docGiaTri('ngayRaHoiDoan'),
        vaiTro: docGiaTri('vaiTro'),
        rowVersion: hd.rowVersion,
      })
      setThongBao('Đã lưu thành công.')
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  const idBase = `hd-${hd.id}`
  return (
    <div className="card glass" ref={containerRef} style={{ marginBottom: 12 }}>
      <div className="card-head"><h2>{hd.tenHoiDoan}</h2></div>
      <GxField label="Ngày vào hội đoàn" id={`${idBase}-vao`}>
        <GxDate id={`${idBase}-vao`} name="ngayVaoHoiDoan" defaultValue={hd.ngayVaoHoiDoan} style={{ maxWidth: 180 }} />
        <GxInline>Ngày ra hội đoàn</GxInline>
        <GxDate ariaLabel="Ngày ra hội đoàn" name="ngayRaHoiDoan" defaultValue={hd.ngayRaHoiDoan} style={{ maxWidth: 180 }} />
      </GxField>
      <GxField label="Vai trò" id={`${idBase}-vaitro`}>
        <input id={`${idBase}-vaitro`} name="vaiTro" type="text" defaultValue={hd.vaiTro ?? ''} style={{ maxWidth: 220 }} />
      </GxField>
      <div className="cmdbar">
        <span className="hint" role={thongBao ? 'status' : undefined}>{thongBao}</span>
        <div className="spacer" />
        <button type="button" className="btn btn-primary" disabled={!onLuu || dangLuu} onClick={xuLyLuu}>
          {dangLuu ? 'Đang lưu…' : 'Cập nhật hội đoàn'}
        </button>
      </div>
    </div>
  )
}

/** Khối "Thêm hội đoàn" — chọn hội đoàn từ danh mục (đúng combo `cbTenHoiDoan` của
 * `GxHistoryHoiDoan`, KHÔNG cho gõ tay), nhập Ngày vào/Ngày ra. Vai trò không có ở đây — hard-code
 * "Hội viên" ở tầng dịch vụ, xem hoi-doan.md mục 2. */
function KhoiThemHoiDoan({
  danhMuc, onThem,
}: {
  danhMuc: HoiDoanDanhMuc[]
  onThem?: (payload: YeuCauThemHoiDoan) => Promise<void>
}) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  async function xuLyThem() {
    if (!onThem) return
    const hoiDoanId = containerRef.current?.querySelector<HTMLSelectElement>('[name="hoiDoanIdMoi"]')?.value
    if (!hoiDoanId) {
      setThongBao('Vui lòng chọn tên hội đoàn.')
      return
    }
    const ngayVao = containerRef.current?.querySelector<HTMLInputElement>('[name="ngayVaoHoiDoanMoi"]')?.value.trim() || null
    const ngayRa = containerRef.current?.querySelector<HTMLInputElement>('[name="ngayRaHoiDoanMoi"]')?.value.trim() || null
    setDangLuu(true)
    setThongBao(null)
    try {
      await onThem({ hoiDoanId, ngayVaoHoiDoan: ngayVao, ngayRaHoiDoan: ngayRa })
      setThongBao('Đã thêm hội đoàn mới.')
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : 'Thêm thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <div className="card glass" ref={containerRef}>
      <div className="card-head"><h2>Thêm vào hội đoàn</h2></div>
      <GxField label="Tên hội đoàn" id="gd-hd-ten">
        {danhMuc.length === 0 ? (
          <span className="hint">Hiện tại chưa có hội đoàn nào.</span>
        ) : (
          <select id="gd-hd-ten" name="hoiDoanIdMoi" defaultValue="" style={{ maxWidth: 320 }}>
            <option value="" disabled>— Chọn hội đoàn —</option>
            {danhMuc.map((h) => <option key={h.id} value={h.id}>{h.tenHoiDoan}</option>)}
          </select>
        )}
      </GxField>
      <GxField label="Ngày vào hội đoàn" id="gd-hd-vao">
        <GxDate id="gd-hd-vao" name="ngayVaoHoiDoanMoi" style={{ maxWidth: 190 }} />
      </GxField>
      <GxField label="Ngày ra hội đoàn" id="gd-hd-ra">
        <GxDate id="gd-hd-ra" name="ngayRaHoiDoanMoi" style={{ maxWidth: 190 }} />
      </GxField>
      <div className="cmdbar">
        <span className="hint" role={thongBao ? 'status' : undefined}>{thongBao}</span>
        <div className="spacer" />
        <button type="button" className="btn btn-primary" disabled={!onThem || dangLuu || danhMuc.length === 0} onClick={xuLyThem}>
          {dangLuu ? 'Đang thêm…' : 'Thêm hội đoàn'}
        </button>
      </div>
    </div>
  )
}

/** Các trường gửi lên `PUT /api/giao-dan/{id}` — đúng `CapNhatGiaoDanRequest` phía backend.
 * Vài trường chỉ hiển thị qua `GxPicker` (tên cha mẹ, người ban bí tích…) chưa có ô nhập thật
 * ở Phase 1 nên giữ nguyên giá trị đã tải thay vì đọc từ DOM — xem chỗ dựng payload. */
export type YeuCauCapNhatGiaoDan = {
  hoTen: string
  tenThanh: string | null
  phai: string | null
  ngaySinh: string | null
  noiSinh: string | null
  cmnd: string | null
  danToc: string | null
  giaoHoId: string | null
  diaChi: string | null
  dienThoai: string | null
  email: string | null
  hoTenCha: string | null
  hoTenMe: string | null
  chaId: string | null
  meId: string | null
  soRuaToi: string | null
  ngayRuaToi: string | null
  noiRuaToi: string | null
  chaRuaToi: string | null
  nguoiDoDauRuaToi: string | null
  soRuocLe: string | null
  ngayRuocLe: string | null
  noiRuocLe: string | null
  chaRuocLe: string | null
  soThemSuc: string | null
  ngayThemSuc: string | null
  noiThemSuc: string | null
  chaThemSuc: string | null
  nguoiDoDauThemSuc: string | null
  ngayXucDau: string | null
  nguoiXucDau: string | null
  tinhTrangXucDau: string | null
  ghiChuXucDau: string | null
  trinhDoVanHoa: string | null
  trinhDoChuyenMon: string | null
  bietNgoaiNgu: string | null
  ngheNghiep: string | null
  conHoc: boolean
  daCoGiaDinh: boolean
  tanTong: boolean
  khongThongKe: boolean
  quaDoi: boolean
  ngayQuaDoi: string | null
  noiQuaDoi: string | null
  soAnTang: string | null
  noiAnTang: string | null
  ghiChu: string | null
  // --- Tab "Giáo lý" — trước đây UI hoàn toàn tĩnh (xem review-frontend, can-review-sau.md
  // mục 19), nay nối vào payload lưu.
  ngayBD1: string | null
  noiBD1: string | null
  ngayBD2: string | null
  noiBD2: string | null
  ngayTHVaoDoi: string | null
  noiTHVaoDoi: string | null
  ngayGLHN1: string | null
  ngayGLHN2: string | null
  noiGLHN: string | null
  nguoiChungNhanGLHN: string | null
  xepLoaiGLHN: string | null
  // --- Thông tin chuyển xứ — `null` = "không đụng gì" (đúng khi Ngoài xứ, khối bị ẩn khỏi
  // DOM); một khi ĐÃ gửi thì `loaiChuyen=0` xoá hẳn bản ghi hiện có (xem CapNhatChuyenXuRequest
  // phía backend).
  chuyenXu: {
    loaiChuyen: number
    ngayChuyen: string | null
    noiChuyen: string | null
    ghiChuChuyen: string | null
    rowVersion?: number
  } | null
  rowVersion: number
  /** Máy chủ kiểm tra nghiệp vụ (checkInput() của frmGiaoDan.cs) trước khi lưu: một số quy
   * tắc chặn cứng (400, không lưu gì); một số khác chỉ CẢNH BÁO kiểu Yes/No của desktop — nếu
   * còn cảnh báo chưa xác nhận, máy chủ KHÔNG lưu và trả lại danh sách cảnh báo thay vì lưu
   * luôn. `false` = lần gửi đầu (mặc định); container gửi lại `true` sau khi người dùng xác
   * nhận muốn tiếp tục bất chấp cảnh báo — xem GiaoDanDetailPage.tao/luu. */
  boQuaCanhBao: boolean
}

type Props = {
  duLieu?: GiaoDanDetailDuLieu
  /** Nút "Xem gia đình" mở thẻ chi tiết gia đình tương ứng. */
  moGiaDinh?: (id: string) => void
  /** "Quay về" và "← Danh sách" mở lại thẻ danh sách giáo dân — có thẻ thì chuyển tiêu
   * điểm, chưa có thì mở mới (quy tắc của `useTabDocs`), không quay về thẻ Tổng quan. */
  moDanhSachGiaoDan?: () => void
  /** Nút "Cập nhật"/"Thêm giáo dân" gọi hàm này với payload dựng từ form — container
   * (`GiaoDanDetailPage`) gọi API thật (POST khi bản ghi mới, PUT khi sửa), xử lý cảnh báo
   * nghiệp vụ và xung đột RowVersion. */
  onLuu?: (payload: YeuCauCapNhatGiaoDan) => void
  dangLuu?: boolean
  thongBaoLuu?: string | null
  /** Loại thông báo đang hiển thị ở `thongBaoLuu` — quyết định màu của dòng chữ ở thanh lệnh
   * cuối form: `loi` (chặn cứng, ví dụ kiểm tra tuổi cha/mẹ) và `canhbao`/hủy đều cần nổi bật
   * (đỏ/cam) để không bị bỏ qua như trước đây (góp ý người dùng), `thanhcong` dịu hơn (xanh lá),
   * không truyền = trung tính (xám, như mặc định "Bản nháp chưa lưu"). */
  loaiThongBao?: 'thanhcong' | 'canhbao' | 'loi' | null
  /** Danh sách hôn phối của giáo dân này (có thể nhiều bản ghi — goá rồi tái hôn), xem
   * tab "Hôn phối" và `hon-phoi.md`. Mặc định rỗng khi chưa truyền (bản ghi mới, hoặc màn
   * hình gọi component này mà chưa tải xong). */
  danhSachHonPhoi?: HonPhoiCuaGiaoDan[]
  dangTaiHonPhoi?: boolean
  /** Lưu một bản ghi hôn phối — container gọi API thật `PUT /api/giao-dan/hon-phoi/{id}`
   * (tách biệt hoàn toàn nút "Cập nhật" của form giáo dân chính) và xử lý xung đột RowVersion;
   * mỗi khối hôn phối tự quản lý trạng thái "đang lưu"/thông báo của riêng nó. */
  onLuuHonPhoi?: (honPhoiId: string, payload: YeuCauCapNhatHonPhoi) => Promise<void>
  /** Danh sách Ơn gọi tận hiến của giáo dân này. Bản desktop chỉ hỗ trợ một bản ghi/giáo dân
   * (xem tan-hien.md mục 3); bản web mở rộng có chủ đích thành danh sách. */
  danhSachTanHien?: TanHienCuaGiaoDan[]
  dangTaiTanHien?: boolean
  onLuuTanHien?: (tanHienId: string, payload: YeuCauLuuTanHien) => Promise<void>
  onThemTanHien?: (payload: YeuCauLuuTanHien) => Promise<void>
  /** Danh sách lượt tham gia hội đoàn (lịch sử) của giáo dân này — xem hoi-doan.md. */
  danhSachHoiDoan?: HoiDoanCuaGiaoDan[]
  dangTaiHoiDoan?: boolean
  onLuuHoiDoan?: (chiTietId: string, payload: YeuCauCapNhatHoiDoan) => Promise<void>
  onThemHoiDoan?: (payload: YeuCauThemHoiDoan) => Promise<void>
  /** Danh mục hội đoàn của giáo xứ — dùng cho combo chọn khi thêm một lượt tham gia mới. */
  danhMucHoiDoan?: HoiDoanDanhMuc[]
  /** Danh mục Giáo họ THẬT (GET /api/giao-ho) — thay `data/giaoHoTam.ts` hard-code theo tên
   * (xem docs/superpowers/specs/man-hinh/can-review-sau.md mục 19). */
  danhMucGiaoHo?: GiaoHo[]
  /** Khoá `localStorage` cho bản nháp ngoại tuyến của CHÍNH form này (xem `lib/banNhap.ts`) —
   * `null`/không truyền = tắt tự lưu nháp (ví dụ chưa xác định được tài khoản đăng nhập). Do
   * container (`GiaoDanDetailPage`) tính sẵn vì nó biết cả tài khoản lẫn id bản ghi. */
  khoaBanNhap?: string | null
  tenTaiKhoan?: string | null
  /** Dữ liệu bản nháp người dùng chọn "Khôi phục" ở `BanNhapBanner` — đè lên `duLieu` (hoặc
   * bản trống nếu đang tạo mới) làm giá trị khởi tạo. Container phải đổi `key` của component
   * này kèm theo để các input không kiểm soát/`GxDate` dựng lại từ đầu với giá trị mới — đổi
   * `defaultValue` không tự cập nhật lại ô đã dựng (xem GxDate.tsx). */
  banNhap?: YeuCauCapNhatGiaoDan | null
  /** In "Lý lịch cá nhân" (VIEC-TIEP-THEO.md mục 1.1) — container gọi
   * `api.giaoDan.inLyLichCaNhan`. Không truyền/`undefined` khi bản ghi chưa lưu (`moi`), nút
   * tự vô hiệu vì chưa có id để in. */
  onIn?: () => void
  dangIn?: boolean
  /** Ảnh đại diện (VIEC-TIEP-THEO.md mục 1.2) — container truyền `api.giaoDan.layAnh`/
   * `taiAnhLen`/`xoaAnh`. Không bắt buộc để các bài test dựng component này không cần mock
   * fetch riêng cho ảnh (mặc định là no-op, ô ảnh tự vô hiệu khi `moi`/thiếu id). */
  onLayAnh?: (id: string) => Promise<string | null>
  onTaiAnhLen?: (id: string, tep: File) => Promise<void>
  onXoaAnh?: (id: string) => Promise<void>
  /** Khoá giáo xứ đang đăng nhập (claim, không phải tham số trình duyệt) — dùng để tách gợi ý
   * nhập liệu theo tần suất lưu ở `localStorage` (xem `lib/goiYNhapLieu.ts`). `null`/không
   * truyền = tắt phần lịch sử ở mọi ô `GxGoiY` trong form này (các bài test dựng component độc
   * lập không cần biết tới cơ chế này). */
  giaoXuId?: string | null
  /** Danh mục "Tên thánh" tĩnh (GET /api/danh-muc/ten-thanh, bảng `du_lieu_chung`) — một trong
   * hai nguồn gợi ý của ô "Tên thánh", xem `GxGoiY`. */
  danhMucTenThanh?: string[]
}

const rong = (): GiaoDanDetailDuLieu => ({
  id: '', maGiaoDanCu: 0, hoTen: '', tenThanh: null, phai: 'Nam', ngaySinh: null,
  noiSinh: null, cmnd: null, danToc: null, giaoHoId: null, diaChi: null, dienThoai: null,
  email: null, hoTenCha: null, hoTenMe: null, chaId: null, meId: null, soRuaToi: null, ngayRuaToi: null,
  noiRuaToi: null, chaRuaToi: null, nguoiDoDauRuaToi: null, soRuocLe: null, ngayRuocLe: null,
  noiRuocLe: null, chaRuocLe: null, soThemSuc: null, ngayThemSuc: null, noiThemSuc: null,
  chaThemSuc: null, nguoiDoDauThemSuc: null, ngayXucDau: null, nguoiXucDau: null,
  tinhTrangXucDau: null, ghiChuXucDau: null, trinhDoVanHoa: null, trinhDoChuyenMon: null,
  bietNgoaiNgu: null, ngheNghiep: null, conHoc: false, daCoGiaDinh: false, tanTong: false,
  khongThongKe: false, quaDoi: false, ngayQuaDoi: null, noiQuaDoi: null, soAnTang: null,
  noiAnTang: null, ghiChu: null,
  ngayBD1: null, noiBD1: null, ngayBD2: null, noiBD2: null,
  ngayTHVaoDoi: null, noiTHVaoDoi: null,
  ngayGLHN1: null, ngayGLHN2: null, noiGLHN: null, nguoiChungNhanGLHN: null, xepLoaiGLHN: null,
  chuyenXu: null,
  giaDinhId: null, tenGiaDinh: null, vaiTro: null, rowVersion: 0,
})

const TINH_TRANG_XUC_DAU = ['', 'Nguy tử', 'Thông thường']
const CHUYEN_XU = ['Ở tại xứ', 'Chuyển từ xứ khác đến', 'Đã chuyển đi xứ khác']
/** Đúng 3 giá trị của `cbGLHNXepLoai` (frmGiaoDan.cs:108-111) — chuỗi hiển thị CHÍNH LÀ giá trị
 * lưu (`cbGLHNXepLoai.Text`), không có mã số riêng. */
const XEP_LOAI_GLHN = ['', 'Trung Bình', 'Khá', 'Giỏi']

/**
 * Chi tiết giáo dân — dựng theo đúng 5 tab của `frmGiaoDan` bản desktop. Ba liên động bắt
 * buộc lấy nguyên từ `frmGiaoDan.cs`:
 *  - `chkQuaDoi_CheckedChanged`: tick "Qua đời" hiện 4 trường ngày/số sổ/nơi qua đời/nơi an
 *    táng, đồng thời tự bỏ tick "Còn học".
 *  - `chkConHoc_CheckedChanged`: tick "Còn học" tự bỏ tick "Qua đời" (loại trừ hai chiều).
 *  - `cbGiaoHo_SelectedIndexChanged`: chọn "Ngoài xứ" (MaGiaoHo = 0) thì hiện "Giáo xứ" /
 *    "Giáo phận", đồng thời ẩn khối "Thông tin chuyển xứ".
 * Các trường ẩn được BỎ HẲN khỏi cây DOM khi đang ẩn (không chỉ gắn `hidden`) để khớp với
 * `queryByLabelText`/`queryByText` trong bài test — khác cách bản mẫu chỉ set `.hidden`.
 */
export function GiaoDanDetail({
  duLieu, moGiaDinh, moDanhSachGiaoDan, onLuu, dangLuu, thongBaoLuu, loaiThongBao,
  danhSachHonPhoi = [], dangTaiHonPhoi = false, onLuuHonPhoi,
  danhSachTanHien = [], dangTaiTanHien = false, onLuuTanHien, onThemTanHien,
  danhSachHoiDoan = [], dangTaiHoiDoan = false, onLuuHoiDoan, onThemHoiDoan,
  danhMucHoiDoan = [], danhMucGiaoHo = [], khoaBanNhap = null, tenTaiKhoan = null, banNhap = null,
  onIn, dangIn = false,
  onLayAnh, onTaiAnhLen, onXoaAnh,
  giaoXuId = null, danhMucTenThanh = [],
}: Props) {
  // banNhap (nếu người dùng vừa bấm "Khôi phục" ở BanNhapBanner) đè lên dữ liệu gốc — xem chú
  // thích ở Props.banNhap. Container LUÔN đổi `key` khi truyền banNhap mới nên các state dưới
  // đây chỉ cần đọc đúng `p` một lần lúc dựng (mount), không cần đồng bộ lại sau đó.
  const p = { ...(duLieu ?? rong()), ...(banNhap ?? {}) }
  const moi = !duLieu?.id
  const formRef = useRef<HTMLFormElement>(null)

  const [quaDoi, setQuaDoi] = useState(p.quaDoi)
  const [conHoc, setConHoc] = useState(p.conHoc)
  // null = "Ngoài xứ" (không phải khoá ngoại tới bảng giao_ho — xem NGOAI_XU ở trên).
  const [giaoHoId, setGiaoHoId] = useState<string | null>(p.giaoHoId)
  const [giaoDanAo, setGiaoDanAo] = useState(p.khongThongKe)
  const [tenCha, setTenCha] = useState(p.hoTenCha)
  const [chaId, setChaId] = useState(p.chaId)
  const [tenMe, setTenMe] = useState(p.hoTenMe)
  const [meId, setMeId] = useState(p.meId)
  // Thông tin chuyển xứ (uiGroupBox6) — 0 = Ở tại xứ (xem CHUYEN_XU); điều khiển việc hiện/ẩn
  // ba ô Ngày/Nơi/Ghi chú bên dưới, đúng `cbChuyenXu_SelectedIndexChanged`.
  const [loaiChuyenXu, setLoaiChuyenXu] = useState(p.chuyenXu?.loaiChuyen ?? 0)

  const doiQuaDoi = (v: boolean) => { setQuaDoi(v); if (v) setConHoc(false) }
  const doiConHoc = (v: boolean) => { setConHoc(v); if (v) setQuaDoi(false) }
  const doiGiaoDanAo = (v: boolean) => { setGiaoDanAo(v); if (v) setGiaoHoId(null) }
  const chonCha = (gd: GiaoDanTimKiem) => { setChaId(gd.id); setTenCha((gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen) }
  const chonMe = (gd: GiaoDanTimKiem) => { setMeId(gd.id); setTenMe((gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen) }

  const ngoaiXu = giaoHoId === null
  // giaoHoId thật hiện tại có thể chưa nằm trong danh mục tải về (dữ liệu đang chờ tải, hoặc
  // giáo họ đã bị xoá mềm) — chèn thêm để <select> không rơi vào "không khớp option nào".
  const dsGiaoHo = giaoHoId && !danhMucGiaoHo.some((g) => g.id === giaoHoId)
    ? [{ id: giaoHoId, tenGiaoHo: `(#${giaoHoId.slice(0, 8)}…)`, maGiaoHoCu: 0, giaoHoChaId: null }, ...danhMucGiaoHo]
    : danhMucGiaoHo

  const tenDayDu = moi ? 'Giáo dân mới' : `${p.tenThanh ? p.tenThanh + ' ' : ''}${p.hoTen}`
  const tinhTrang = quaDoi ? 'Đã qua đời' : moi ? 'Bản nháp' : 'Đang hoạt động'
  const tagTone = quaDoi ? 'amber' : moi ? 'violet' : 'mint'

  const tabCaNhan = (
    <>
      {/* Bản desktop (`grbCaNhan` của frmGiaoDan.Designer.cs, dòng 1039-1337) chia BA cột.
          Bản web CỐ Ý đổi thành HAI cột bằng nhau theo yêu cầu trực tiếp của người dùng, để
          thẳng hàng với hai tấm 50/50 ngay bên dưới (Rửa tội ‖ Rước lễ lần đầu) — xem
          can-review-sau.md mục "W2 — khác biệt có chủ đích". Ảnh đại diện chuyển vào đầu cột
          trái (`.canhan-top`), thu nhỏ và đặt cạnh Mã giáo dân/Tên thánh thay vì có cả cột
          riêng, để không lặp lại khoảng trống lớn mà người dùng đã phàn nàn một lần trước đó.
          Thứ tự trong `.canhan-top`: các trường TRƯỚC, ảnh SAU (ảnh nằm bên PHẢI của Mã giáo
          dân/Tên thánh) — góp ý kiểm thử tiếp theo, xem can-review-sau.md.

          Góp ý tiếp theo (2026-09-07, ảnh chụp khối "Thông tin cá nhân"): "Họ tên"/"Giáo họ"
          trước đây nằm NGOÀI `.canhan-top-fields`, mỗi ô rộng hết cột trái — khiến khối trái
          cao hơn khung ảnh 3x4 bên phải khá nhiều (ảnh chỉ cao 92px cố định, dư khoảng trắng
          dưới ảnh) VÀ tạo ra khoảng trống thừa ngay dưới "Tên thánh": `.canhan-top` dùng
          `align-items: flex-start` nên hàng flex cao theo phần tử cao nhất (khung ảnh 92px),
          còn `.canhan-top-fields` (chỉ 2 dòng Mã giáo dân/Tên thánh, thấp hơn 92px) trôi nổi ở
          trên, để lại khoảng trắng chính giữa khung ảnh và Họ tên bên dưới — không phải do
          margin nào cả. Chuyển "Họ tên"/"Giáo họ" VÀO `.canhan-top-fields` (rộng hẹp bằng "Tên
          thánh" — đúng ý người dùng "ngắn lại bằng tên thánh") để cột trái thành 4 dòng xếp
          khít nhau (margin-bottom 6px như mọi `.frow`, không có khoảng hở nào xen giữa); đổi
          `.canhan-top` sang `align-items: stretch` (CSS, xem qlgx.css) để khung ảnh bên phải tự
          giãn cao bằng đúng số dòng đó — vừa hết khoảng trắng, vừa cho ảnh cao thêm cân đối như
          yêu cầu. Field "Giáo xứ"/"Giáo phận" (chỉ hiện khi chọn "Ngoài xứ", hiếm gặp) CỐ Ý giữ
          nguyên ngoài `.canhan-top-fields` — không phải trọng tâm góp ý này, để full-width như
          cũ tránh cắt chữ "Giáo phận".

          Góp ý tiếp theo (2026-09-07, người dùng): "đưa CCCD và checkbox trong hình qua bên
          trái, bên dưới giáo họ" — chuyển GxField "CMND / CCCD" (kèm ô đánh dấu "Là giáo dân
          không được thống kê" qua prop `extra`) từ cột phải sang cột trái, làm dòng thứ 5 trong
          `.canhan-top-fields` ngay dưới "Giáo họ". Không cần sửa gì thêm ở CSS: `.canhan-top`
          vẫn `align-items: stretch` nên khung ảnh bên phải tự giãn cao theo đúng 5 dòng mới —
          xem `getBoundingClientRect()` đo trước/sau trong task-3-viec-giao-dien-excel.md. */}
      <div className="card glass">
        <div className="card-head"><h2>Thông tin cá nhân</h2><span className="eyebrow">Hồ sơ giáo dân</span></div>
        <div className="canhan-cols">
          <div>
            <div className="canhan-top">
              <div className="canhan-top-fields">
                <GxField label="Mã giáo dân" id="gd-ma">
                  <input id="gd-ma" type="text" value={moi ? '(tự sinh khi lưu)' : String(p.maGiaoDanCu)} disabled />
                </GxField>
                <GxField label="Tên thánh" id="gd-tenthanh">
                  <GxGoiY id="gd-tenthanh" name="tenThanh" truong="tenThanh" giaoXuId={giaoXuId}
                    danhMuc={danhMucTenThanh} defaultValue={p.tenThanh} />
                </GxField>
                <GxField label="Họ tên" id="gd-hoten">
                  <input id="gd-hoten" name="hoTen" type="text" defaultValue={p.hoTen} />
                </GxField>
                <GxField label="Giáo họ" id="gd-giaoho">
                  <select id="gd-giaoho" value={giaoHoId ?? NGOAI_XU} onChange={(e) => setGiaoHoId(e.target.value === NGOAI_XU ? null : e.target.value)}>
                    <option value={NGOAI_XU}>{NGOAI_XU}</option>
                    {dsGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
                  </select>
                </GxField>
                <GxField label="CMND / CCCD" id="gd-cmnd"
                  extra={
                    <label className="toggle">
                      <input type="checkbox" checked={giaoDanAo} onChange={(e) => doiGiaoDanAo(e.target.checked)} />
                      Là giáo dân không được thống kê
                    </label>
                  }>
                  <input id="gd-cmnd" name="cmnd" type="text" defaultValue={p.cmnd ?? ''} />
                </GxField>
              </div>
              <AnhDaiDien
                id={moi ? null : p.id}
                onLayAnh={onLayAnh ?? (async () => null)}
                onTaiLen={onTaiAnhLen ?? (async () => {})}
                onXoa={onXoaAnh ?? (async () => {})}
                nhan="ảnh 3x4"
                tiLe34
              />
            </div>
            {ngoaiXu && (
              <GxField label="Giáo xứ" id="gd-giaoxu">
                <input id="gd-giaoxu" type="text" defaultValue="" />
                <GxInline>Giáo phận</GxInline>
                <input aria-label="Giáo phận" type="text" defaultValue="" />
              </GxField>
            )}
          </div>

          <div>
            <GxField label="Giới tính" id="gd-phai">
              <select id="gd-phai" name="phai" defaultValue={p.phai ?? 'Nam'} style={{ maxWidth: 100 }}>
                <option value="Nam">Nam</option>
                <option value="Nữ">Nữ</option>
              </select>
              <GxInline>Ngày sinh</GxInline>
              <GxDate ariaLabel="Ngày sinh" name="ngaySinh" defaultValue={p.ngaySinh} />
            </GxField>
            <GxField label="Nơi sinh" id="gd-noisinh">
              <GxGoiY id="gd-noisinh" name="noiSinh" truong="noiSinh" giaoXuId={giaoXuId} defaultValue={p.noiSinh} />
            </GxField>
            <GxField label="Tên Cha" id="gd-tencha">
              <GxPicker id="gd-tencha" value={tenCha} onChon={chonCha}
                onBoChon={() => { setChaId(null); setTenCha(null) }} />
            </GxField>
            <GxField label="Tên Mẹ" id="gd-tenme">
              <GxPicker id="gd-tenme" value={tenMe} onChon={chonMe}
                onBoChon={() => { setMeId(null); setTenMe(null) }} />
            </GxField>
          </div>
        </div>
      </div>

      <div className="card-row">
        <div className="card glass">
          <div className="card-head"><h2>Rửa tội</h2></div>
          <GxField label="Ngày rửa tội" id="gd-ngayruatoi">
            <GxDate id="gd-ngayruatoi" name="ngayRuaToi" defaultValue={p.ngayRuaToi} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ rửa tội" name="soRuaToi" type="text" defaultValue={p.soRuaToi ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-charuatoi"><GxPicker id="gd-charuatoi" value={p.chaRuaToi} /></GxField>
          <GxField label="Người đỡ đầu" id="gd-dodauruatoi">
            <GxGoiY id="gd-dodauruatoi" name="nguoiDoDauRuaToi" truong="nguoiDoDau" giaoXuId={giaoXuId}
              defaultValue={p.nguoiDoDauRuaToi} />
            <GxInline>Nơi rửa tội</GxInline>
            <GxGoiY ariaLabel="Nơi rửa tội" name="noiRuaToi" truong="noiRuaToi" giaoXuId={giaoXuId}
              defaultValue={p.noiRuaToi} />
          </GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Rước lễ lần đầu</h2></div>
          <GxField label="Ngày rước lễ" id="gd-ngayruocle">
            <GxDate id="gd-ngayruocle" name="ngayRuocLe" defaultValue={p.ngayRuocLe} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ rước lễ" name="soRuocLe" type="text" defaultValue={p.soRuocLe ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-charuocle"><GxPicker id="gd-charuocle" value={p.chaRuocLe} /></GxField>
          <GxField label="Nơi rước lễ" id="gd-noiruocle">
            <GxGoiY id="gd-noiruocle" name="noiRuocLe" truong="noiRuocLe" giaoXuId={giaoXuId} defaultValue={p.noiRuocLe} />
          </GxField>
        </div>
      </div>

      <div className="card-row">
        <div className="card glass">
          <div className="card-head"><h2>Thêm sức</h2></div>
          <GxField label="Ngày thêm sức" id="gd-ngaythemsuc">
            <GxDate id="gd-ngaythemsuc" name="ngayThemSuc" defaultValue={p.ngayThemSuc} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ thêm sức" name="soThemSuc" type="text" defaultValue={p.soThemSuc ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-chathemsuc"><GxPicker id="gd-chathemsuc" value={p.chaThemSuc} /></GxField>
          <GxField label="Người đỡ đầu" id="gd-dodauthemsuc">
            <GxGoiY id="gd-dodauthemsuc" name="nguoiDoDauThemSuc" truong="nguoiDoDau" giaoXuId={giaoXuId}
              defaultValue={p.nguoiDoDauThemSuc} />
            <GxInline>Nơi thêm sức</GxInline>
            <GxGoiY ariaLabel="Nơi thêm sức" name="noiThemSuc" truong="noiThemSuc" giaoXuId={giaoXuId}
              defaultValue={p.noiThemSuc} />
          </GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Xức dầu</h2></div>
          <GxField label="Ngày xức dầu" id="gd-ngayxucdau">
            <GxDate id="gd-ngayxucdau" name="ngayXucDau" defaultValue={p.ngayXucDau} style={{ maxWidth: 170 }} />
            <GxInline>Tình trạng</GxInline>
            <select aria-label="Tình trạng xức dầu" name="tinhTrangXucDau" defaultValue={p.tinhTrangXucDau ?? ''} style={{ maxWidth: 180 }}>
              {TINH_TRANG_XUC_DAU.map((t) => <option key={t} value={t}>{t || 'Chưa xác định'}</option>)}
            </select>
          </GxField>
          <GxField label="Người ban bí tích" id="gd-nguoixucdau"><GxPicker id="gd-nguoixucdau" value={p.nguoiXucDau} /></GxField>
          <GxField label="Ghi chú" id="gd-ghichuxucdau">
            <input id="gd-ghichuxucdau" name="ghiChuXucDau" type="text" defaultValue={p.ghiChuXucDau ?? ''} placeholder="Ghi chú xức dầu…" />
          </GxField>
        </div>
      </div>

      {/* Đúng khối `uiGroupBox6` — chiếm trọn chiều ngang, ẩn khi Ngoài xứ
          (cbGiaoHo_SelectedIndexChanged). Trước đây chỉ có ô "Thông tin hiện tại" mà không có
          `name`, không lưu được gì (xem review-frontend, can-review-sau.md mục 19) — nay nối cả
          ba ô còn lại (Ngày chuyển/Nơi chuyển/Ghi chú), hiện khi khác "Ở tại xứ" — đúng
          `txtGiaoXuChuyen.Visible` của `cbChuyenXu_SelectedIndexChanged` (frmGiaoDan.cs:1248-
          1260). Bản desktop có thêm hộp thoại Yes/No cảnh báo khi đổi TỪ một loại chuyển xứ đã
          lưu VỀ "Ở tại xứ" — tiện ích UX, không phải ràng buộc cứng (chọn No chỉ phục hồi lựa
          chọn cũ), cố ý CHƯA migrate ở lượt này, xem can-review-sau.md mục 19. */}
      {!ngoaiXu && (
        <div className="card glass">
          <div className="card-head"><h2>Thông tin chuyển xứ</h2></div>
          <GxField label="Thông tin hiện tại" id="gd-chuyenxu">
            <select id="gd-chuyenxu" name="loaiChuyenXu" value={loaiChuyenXu}
              onChange={(e) => setLoaiChuyenXu(Number(e.target.value))} style={{ maxWidth: 320 }}>
              {CHUYEN_XU.map((c, i) => <option key={c} value={i}>{c}</option>)}
            </select>
          </GxField>
          {loaiChuyenXu !== 0 && (
            <>
              <GxField label="Ngày chuyển" id="gd-ngaychuyenxu">
                <GxDate id="gd-ngaychuyenxu" name="ngayChuyenXu" defaultValue={p.chuyenXu?.ngayChuyen ?? null} style={{ maxWidth: 180 }} />
              </GxField>
              <GxField label="Nơi chuyển" id="gd-noichuyenxu">
                <input id="gd-noichuyenxu" name="noiChuyenXu" type="text" defaultValue={p.chuyenXu?.noiChuyen ?? ''}
                  placeholder="Giáo xứ chuyển đi/đến…" />
              </GxField>
              <GxField label="Ghi chú" id="gd-ghichuchuyenxu">
                <input id="gd-ghichuchuyenxu" name="ghiChuChuyenXu" type="text" defaultValue={p.chuyenXu?.ghiChuChuyen ?? ''} />
              </GxField>
            </>
          )}
        </div>
      )}

      {/* Đúng khối `uiGroupBox5` "Thông tin khác" — thứ tự/gộp hàng theo toạ độ Designer. */}
      <div className="card glass">
        <div className="card-head"><h2>Thông tin khác</h2></div>
        <GxField label="Trình độ văn hóa" id="gd-vanhoa">
          <input id="gd-vanhoa" name="trinhDoVanHoa" type="text" defaultValue={p.trinhDoVanHoa ?? ''} style={{ maxWidth: 180 }} />
          <GxInline>Trình độ ch.môn</GxInline>
          <input aria-label="Trình độ chuyên môn" name="trinhDoChuyenMon" type="text" defaultValue={p.trinhDoChuyenMon ?? ''} />
        </GxField>
        <GxField label="Biết ngoại ngữ" id="gd-ngoaingu">
          <input id="gd-ngoaingu" name="bietNgoaiNgu" type="text" defaultValue={p.bietNgoaiNgu ?? ''} style={{ maxWidth: 180 }} />
          <label className="toggle">
            <input type="checkbox" name="conHoc" checked={conHoc} onChange={(e) => doiConHoc(e.target.checked)} />
            Còn học
          </label>
        </GxField>
        <GxField label="Địa chỉ" id="gd-diachi">
          <input id="gd-diachi" name="diaChi" type="text" defaultValue={p.diaChi ?? ''} />
        </GxField>
        <GxField label="Nghề nghiệp" id="gd-nghenghiep">
          <input id="gd-nghenghiep" name="ngheNghiep" type="text" defaultValue={p.ngheNghiep ?? ''} style={{ maxWidth: 200 }} />
          <GxInline>Dân tộc</GxInline>
          <input aria-label="Dân tộc" name="danToc" type="text" defaultValue={p.danToc ?? ''} />
        </GxField>
        <GxField label="Điện thoại" id="gd-dienthoai">
          <input id="gd-dienthoai" name="dienThoai" type="text" defaultValue={p.dienThoai ?? ''} style={{ maxWidth: 170 }} />
          <GxInline>Email</GxInline>
          <input aria-label="Email" name="email" type="text" defaultValue={p.email ?? ''} />
        </GxField>
        <GxField label="">
          <label className="toggle">
            <input type="checkbox" name="tanTong" defaultChecked={p.tanTong} />
            Tân tòng
          </label>
          <label className="toggle">
            <input type="checkbox" name="daCoGiaDinh" defaultChecked={p.daCoGiaDinh} />
            Có gia đình
          </label>
          <label className="toggle">
            <input type="checkbox" name="quaDoi" checked={quaDoi} onChange={(e) => doiQuaDoi(e.target.checked)} />
            Qua đời
          </label>
        </GxField>
        {quaDoi && (
          <>
            <GxField label="Ngày qua đời" id="gd-ngayquadoi">
              <GxDate id="gd-ngayquadoi" name="ngayQuaDoi" defaultValue={p.ngayQuaDoi} style={{ maxWidth: 180 }} />
              <GxInline>Số sổ</GxInline>
              <input aria-label="Số sổ qua đời" name="soAnTang" type="text" defaultValue={p.soAnTang ?? ''} style={{ maxWidth: 140 }} />
            </GxField>
            <GxField label="Nơi qua đời" id="gd-noiquadoi">
              <input id="gd-noiquadoi" name="noiQuaDoi" type="text" defaultValue={p.noiQuaDoi ?? ''} />
              <GxInline>Nơi an táng</GxInline>
              <input aria-label="Nơi an táng" name="noiAnTang" type="text" defaultValue={p.noiAnTang ?? ''} />
            </GxField>
          </>
        )}
        <GxField label="Ghi chú chung" id="gd-ghichu">
          <textarea id="gd-ghichu" name="ghiChu" defaultValue={p.ghiChu ?? ''} placeholder="Ghi chú nội bộ về giáo dân…" />
        </GxField>
      </div>
    </>
  )

  const tabGiaoLy = (
    <>
      <div className="card-row">
        <div className="card glass">
          <div className="card-head"><h2>Bao đồng 1</h2></div>
          <GxField label="Ngày kết thúc khóa học" id="gd-gl-bd1">
            <GxDate id="gd-gl-bd1" name="ngayBD1" defaultValue={p.ngayBD1} style={{ maxWidth: 190 }} />
          </GxField>
          <GxField label="Tại giáo xứ" id="gd-gl-bd1-gx">
            <input id="gd-gl-bd1-gx" name="noiBD1" type="text" defaultValue={p.noiBD1 ?? ''} />
          </GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Bao đồng 2</h2></div>
          <GxField label="Ngày rước lễ trọng thể" id="gd-gl-bd2">
            <GxDate id="gd-gl-bd2" name="ngayBD2" defaultValue={p.ngayBD2} style={{ maxWidth: 190 }} />
          </GxField>
          <GxField label="Tại giáo xứ" id="gd-gl-bd2-gx">
            <input id="gd-gl-bd2-gx" name="noiBD2" type="text" defaultValue={p.noiBD2 ?? ''} />
          </GxField>
        </div>
      </div>
      <div className="card glass">
        <div className="card-head"><h2>Vào đời</h2></div>
        <GxField label="Ngày tuyên hứa" id="gd-gl-vd">
          <GxDate id="gd-gl-vd" name="ngayTHVaoDoi" defaultValue={p.ngayTHVaoDoi} style={{ maxWidth: 190 }} />
        </GxField>
        <GxField label="Tại giáo xứ" id="gd-gl-vd-gx">
          <input id="gd-gl-vd-gx" name="noiTHVaoDoi" type="text" defaultValue={p.noiTHVaoDoi ?? ''} />
        </GxField>
      </div>
      <div className="card glass">
        <div className="card-head"><h2>Hôn nhân</h2></div>
        <GxField label="Khóa học từ ngày" id="gd-gl-hn-tu">
          <GxDate id="gd-gl-hn-tu" name="ngayGLHN1" defaultValue={p.ngayGLHN1} style={{ maxWidth: 180 }} />
          <GxInline>đến ngày</GxInline>
          <GxDate name="ngayGLHN2" ariaLabel="Khóa học đến ngày" defaultValue={p.ngayGLHN2} style={{ maxWidth: 180 }} />
        </GxField>
        <GxField label="Tại giáo xứ" id="gd-gl-hn-gx">
          <input id="gd-gl-hn-gx" name="noiGLHN" type="text" defaultValue={p.noiGLHN ?? ''} />
        </GxField>
        {/* Bản desktop dùng ô văn bản tự do (txtGLHNNguoiCap), KHÔNG phải liên kết tới một
            giáo dân khác — GxPicker trước đây dựng ở đây không khớp kiểu dữ liệu thật
            (string, không phải khoá ngoại) nên không thể có onChon hợp lệ; đổi thành input
            văn bản thường để nối được vào payload. */}
        <GxField label="Người cấp chứng nhận" id="gd-gl-hn-nguoicap">
          <input id="gd-gl-hn-nguoicap" name="nguoiChungNhanGLHN" type="text" defaultValue={p.nguoiChungNhanGLHN ?? ''} />
        </GxField>
        <GxField label="Xếp loại" id="gd-gl-hn-xeploai">
          <select id="gd-gl-hn-xeploai" name="xepLoaiGLHN" defaultValue={p.xepLoaiGLHN ?? ''} style={{ maxWidth: 180 }}>
            {XEP_LOAI_GLHN.map((x) => <option key={x} value={x}>{x || 'Chưa xếp loại'}</option>)}
          </select>
        </GxField>
      </div>
    </>
  )

  const tabHonPhoi = (
    <>
      {dangTaiHonPhoi && <p className="hint">Đang tải danh sách hôn phối…</p>}
      {!dangTaiHonPhoi && danhSachHonPhoi.length === 0 && (
        <div className="card glass">
          <div className="card-head"><h2>Thông tin đôi hôn phối</h2></div>
          <p className="hint">Giáo dân này chưa có bản ghi hôn phối nào.</p>
        </div>
      )}
      {danhSachHonPhoi.map((hp, i) => (
        <KhoiHonPhoi key={hp.id} hp={hp} thuTu={i} onLuu={onLuuHonPhoi} giaoXuId={giaoXuId} />
      ))}
    </>
  )

  const tabOnGoi = (
    <>
      {dangTaiTanHien && <p className="hint">Đang tải thông tin ơn gọi tận hiến…</p>}
      {!dangTaiTanHien && danhSachTanHien.map((th, i) => (
        <KhoiTanHien key={th.id} th={th} thuTu={i} onLuu={onLuuTanHien} />
      ))}
      {!dangTaiTanHien && (
        <KhoiTanHien thuTu={danhSachTanHien.length} onThem={onThemTanHien} />
      )}
    </>
  )

  const tabHoiDoan = (
    <>
      {dangTaiHoiDoan && <p className="hint">Đang tải danh sách hội đoàn…</p>}
      {!dangTaiHoiDoan && danhSachHoiDoan.length === 0 && (
        <div className="card glass">
          <div className="card-head"><h2>Lịch sử hội đoàn</h2></div>
          <p className="hint">Giáo dân này chưa tham gia hội đoàn nào.</p>
        </div>
      )}
      {!dangTaiHoiDoan && danhSachHoiDoan.map((hd) => (
        <KhoiHoiDoan key={hd.id} hd={hd} onLuu={onLuuHoiDoan} />
      ))}
      {!dangTaiHoiDoan && <KhoiThemHoiDoan danhMuc={danhMucHoiDoan} onThem={onThemHoiDoan} />}
    </>
  )

  // Dựng payload từ form (input không kiểm soát — defaultValue) — dùng chung cho việc lưu thật
  // (xuLySubmit) VÀ tự lưu nháp định kỳ (useTuDongLuuBanNhap ở dưới) để không lặp lại danh sách
  // trường hai lần. Vài trường (tên cha/mẹ, người ban bí tích…) chỉ hiển thị qua GxPicker — chưa
  // có ô nhập thật ở Phase 1 nên giữ nguyên giá trị đã tải thay vì đọc từ DOM.
  function dungPayloadTuForm(fd: FormData): YeuCauCapNhatGiaoDan {
    const chuoi = (ten: string) => (fd.get(ten) as string | null)?.trim() || null

    return {
      hoTen: chuoi('hoTen') ?? p.hoTen,
      tenThanh: chuoi('tenThanh'),
      phai: chuoi('phai'),
      ngaySinh: chuoi('ngaySinh'),
      noiSinh: chuoi('noiSinh'),
      cmnd: chuoi('cmnd'),
      danToc: chuoi('danToc'),
      giaoHoId,
      diaChi: chuoi('diaChi'),
      dienThoai: chuoi('dienThoai'),
      email: chuoi('email'),
      hoTenCha: tenCha,
      hoTenMe: tenMe,
      chaId,
      meId,
      soRuaToi: chuoi('soRuaToi'),
      ngayRuaToi: chuoi('ngayRuaToi'),
      noiRuaToi: chuoi('noiRuaToi'),
      chaRuaToi: p.chaRuaToi,
      nguoiDoDauRuaToi: chuoi('nguoiDoDauRuaToi'),
      soRuocLe: chuoi('soRuocLe'),
      ngayRuocLe: chuoi('ngayRuocLe'),
      noiRuocLe: chuoi('noiRuocLe'),
      chaRuocLe: p.chaRuocLe,
      soThemSuc: chuoi('soThemSuc'),
      ngayThemSuc: chuoi('ngayThemSuc'),
      noiThemSuc: chuoi('noiThemSuc'),
      chaThemSuc: p.chaThemSuc,
      nguoiDoDauThemSuc: chuoi('nguoiDoDauThemSuc'),
      ngayXucDau: chuoi('ngayXucDau'),
      nguoiXucDau: p.nguoiXucDau,
      tinhTrangXucDau: chuoi('tinhTrangXucDau'),
      ghiChuXucDau: chuoi('ghiChuXucDau'),
      trinhDoVanHoa: chuoi('trinhDoVanHoa'),
      trinhDoChuyenMon: chuoi('trinhDoChuyenMon'),
      bietNgoaiNgu: chuoi('bietNgoaiNgu'),
      ngheNghiep: chuoi('ngheNghiep'),
      conHoc,
      daCoGiaDinh: fd.get('daCoGiaDinh') === 'on',
      tanTong: fd.get('tanTong') === 'on',
      khongThongKe: giaoDanAo,
      quaDoi,
      ngayQuaDoi: quaDoi ? chuoi('ngayQuaDoi') : null,
      noiQuaDoi: quaDoi ? chuoi('noiQuaDoi') : null,
      soAnTang: quaDoi ? chuoi('soAnTang') : null,
      noiAnTang: quaDoi ? chuoi('noiAnTang') : null,
      ghiChu: chuoi('ghiChu'),
      ngayBD1: chuoi('ngayBD1'),
      noiBD1: chuoi('noiBD1'),
      ngayBD2: chuoi('ngayBD2'),
      noiBD2: chuoi('noiBD2'),
      ngayTHVaoDoi: chuoi('ngayTHVaoDoi'),
      noiTHVaoDoi: chuoi('noiTHVaoDoi'),
      ngayGLHN1: chuoi('ngayGLHN1'),
      ngayGLHN2: chuoi('ngayGLHN2'),
      noiGLHN: chuoi('noiGLHN'),
      nguoiChungNhanGLHN: chuoi('nguoiChungNhanGLHN'),
      xepLoaiGLHN: chuoi('xepLoaiGLHN'),
      chuyenXu: ngoaiXu ? null : {
        loaiChuyen: loaiChuyenXu,
        ngayChuyen: loaiChuyenXu !== 0 ? chuoi('ngayChuyenXu') : null,
        noiChuyen: loaiChuyenXu !== 0 ? chuoi('noiChuyenXu') : null,
        ghiChuChuyen: loaiChuyenXu !== 0 ? chuoi('ghiChuChuyenXu') : null,
        rowVersion: p.chuyenXu?.rowVersion,
      },
      rowVersion: p.rowVersion,
      boQuaCanhBao: false,
    }
  }

  function xuLySubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!onLuu || !formRef.current) return
    onLuu(dungPayloadTuForm(new FormData(formRef.current)))
  }

  // Tự lưu nháp định kỳ (xem lib/banNhap.ts) — đọc lại form MỖI LẦN chạy interval (không phải
  // một lần lúc mount) để luôn bắt được giá trị mới nhất đang gõ.
  useTuDongLuuBanNhap(khoaBanNhap, tenTaiKhoan, () => {
    if (!formRef.current) return null
    return dungPayloadTuForm(new FormData(formRef.current))
  })

  // Lưu thành công (loaiThongBao chuyển sang 'thanhcong') — xoá nháp ngay để lần mở lại không
  // hỏi khôi phục nhầm dữ liệu đã lưu rồi.
  useEffect(() => {
    if (loaiThongBao === 'thanhcong' && khoaBanNhap) xoaBanNhap(khoaBanNhap)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaiThongBao])

  return (
    <form className="page detail-page" ref={formRef} onSubmit={xuLySubmit}>
      <div className="page-head detail-head">
        <button type="button" className="btn btn-sm btn-quiet" onClick={() => moDanhSachGiaoDan?.()}>
          ← Danh sách
        </button>
        <h1>{tenDayDu}</h1>
        <span className="head-sub">
          {moi ? 'Chưa lưu · nhập thông tin rồi bấm Thêm giáo dân'
            : `${p.maGiaoDanCu} · ${dsGiaoHo.find((g) => g.id === giaoHoId)?.tenGiaoHo ?? NGOAI_XU}${p.ngaySinh ? ' · sinh ' + dinhDangNgay(p.ngaySinh) : ''}`}
        </span>
        <div className="spacer" />
        <span className={'tag tag-' + tagTone}>{tinhTrang}</span>
      </div>

      <GxFormTabs
        tabs={[
          { title: 'Cá nhân', noiDung: tabCaNhan },
          { title: 'Giáo lý', noiDung: tabGiaoLy },
          { title: 'Hôn phối', noiDung: tabHonPhoi },
          { title: 'Ơn gọi tận hiến', noiDung: tabOnGoi },
          { title: 'Hội đoàn', noiDung: tabHoiDoan },
        ]}
      />

      <div className="cmdbar">
        <span
          className={'hint' + (thongBaoLuu && loaiThongBao ? ` hint-${loaiThongBao}` : '')}
          role={thongBaoLuu ? 'status' : undefined}
        >
          {thongBaoLuu ?? (moi ? 'Bản nháp chưa lưu' : 'Chưa có thay đổi')}
        </span>
        <div className="spacer" />
        <button type="button" className="btn" disabled={!p.giaDinhId} onClick={() => p.giaDinhId && moGiaDinh?.(p.giaDinhId)}>
          Xem gia đình
        </button>
        <button type="button" className="btn" disabled={!onIn || dangIn} onClick={() => onIn?.()}>
          {dangIn ? 'Đang tạo PDF…' : 'In lý lịch cá nhân'}
        </button>
        <button type="button" className="btn btn-quiet" onClick={() => moDanhSachGiaoDan?.()}>Quay về</button>
        <button type="submit" className="btn btn-primary" disabled={!onLuu || dangLuu}>
          {dangLuu ? 'Đang lưu…' : moi ? 'Thêm giáo dân' : 'Cập nhật'}
        </button>
      </div>
    </form>
  )
}
