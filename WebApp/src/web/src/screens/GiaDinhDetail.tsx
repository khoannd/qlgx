import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import type {
  GiaDinhDetail as GiaDinhDetailDuLieu, GiaoDanListItem, GiaoDanTimKiem, GiaoHo, ThanhVien,
} from '../api/types'
import { DANH_SACH_VAI_TRO_THANH_VIEN, tenVaiTro } from '../lib/vaiTroGiaDinh'
import { GxDate } from '../components/GxDate'
import { GxField, GxInline } from '../components/GxField'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'
import { GxPicker } from '../components/GxPicker'
import { useTuDongLuuBanNhap, xoaBanNhap } from '../lib/banNhap'

/** Sentinel hiển thị cho "Ngoài xứ" — ứng với `giaoHoId === null` (xem NGOAI_XU ở
 * GiaoDanDetail.tsx, cùng quy ước). */
const NGOAI_XU = 'Ngoài xứ'

/** Các trường gửi lên `PUT /api/gia-dinh/{id}` — đúng `CapNhatGiaDinhRequest` phía backend,
 * trừ `giaoHoId` (xem ghi chú tại chỗ dựng payload bên dưới) và `honPhoi` (khối hôn phối
 * chưa có giao diện sửa ở Phase 1 này nên luôn gửi `null` — nghĩa là "không đụng tới hôn
 * phối hiện có", KHÔNG phải "xoá hôn phối", xem GiaDinhService.CapNhat). */
export type YeuCauCapNhatGiaDinh = {
  tenGiaDinh: string | null
  giaoHoId: string | null
  dienThoai: string | null
  diaChi: string | null
  soHoKhau: string | null
  dienGiaDinh: string | null
  ghiChu: string | null
  daChuyenXu: boolean
  ngayChuyen: string | null
  noiChuyen: string | null
  khongThongKe: boolean
  rowVersion: number
  honPhoi: null
  /** Chủ hộ CHỈ có thể là Người nam (0) hay Người nữ (1) — hai radio `name="chuho"` cạnh hai ô
   * đó (xem `GiaDinhService.CapNhat`, chỉ ghi `ChuHo` vào dòng Chồng/Vợ). `null` = không radio
   * nào được chọn (kể cả trường hợp cả hai đều bị disable vì chưa có Người nam/nữ) — máy chủ ghi
   * đúng y giá trị này (kể cả "gỡ chủ hộ"), không tự đoán/giữ lại giá trị cũ. */
  chuHoVaiTro: 0 | 1 | null
}

type Props = {
  duLieu?: GiaDinhDetailDuLieu
  /** Nút "Xem & sửa" trên lưới thành viên mở thẻ chi tiết giáo dân tương ứng. */
  moGiaoDan?: (id: string) => void
  /** "Quay về" và "← Danh sách" mở lại thẻ danh sách gia đình — có thẻ thì chuyển tiêu
   * điểm, chưa có thì mở mới (quy tắc của `useTabDocs`), không quay về thẻ Tổng quan. */
  moDanhSachGiaDinh?: () => void
  /** Nút "Cập nhật" gọi hàm này với payload đã dựng từ form — container (`GiaDinhDetailPage`)
   * chịu trách nhiệm gọi `api.giaDinh.capNhat`, xử lý xung đột RowVersion và tải lại. Bản ghi
   * mới (`duLieu` chưa có `id`) chưa có API tạo mới nên nút bị vô hiệu hoá khi thiếu prop này. */
  onLuu?: (payload: YeuCauCapNhatGiaDinh) => void
  dangLuu?: boolean
  /** Thông báo kết quả lần lưu gần nhất — container quyết định nội dung (thành công/lỗi/xung
   * đột), component này chỉ hiển thị nguyên văn. */
  thongBaoLuu?: string | null
  /** Danh mục Giáo họ THẬT (GET /api/giao-ho) — thay `data/giaoHoTam.ts` hard-code theo tên. */
  danhMucGiaoHo?: GiaoHo[]
  /** Chọn/đổi Người nam (vaiTro=0) hoặc Người nữ (vaiTro=1) qua GxPicker — container
   * (`GiaDinhDetailPage`) chịu trách nhiệm gọi `PUT .../vo-chong/{vaiTro}`, chạy cây quyết định
   * `NguoiCu` khi cần (xem `lib/nguoiCu.ts`) và hiển thị lỗi/cảnh báo qua hộp thoại trong ứng
   * dụng (`useHoiDap`) — component này chỉ chuyển tiếp lựa chọn của người dùng. */
  onGanVoChong?: (vaiTro: 0 | 1, giaoDan: GiaoDanTimKiem) => void
  /** Nút "Bỏ chọn" (X) cạnh Người nam/Người nữ — cũng chạy qua cây quyết định `NguoiCu` nếu
   * vai trò đang có người (xem `GiaDinhDetailPage.boChonVoChong`). */
  onBoChonVoChong?: (vaiTro: 0 | 1) => void
  /** Thêm một người vào lưới "Thành viên khác" (`addGiaoDan`) — container xử lý chuỗi cảnh báo
   * (đã thuộc gia đình khác/đã xoá/đã chuyển xứ) qua hộp thoại trong ứng dụng. */
  onThemThanhVien?: (giaoDan: GiaoDanTimKiem, vaiTro: number) => void
  /** Xoá VĨNH VIỄN một thành viên khỏi gia đình (can-review-sau.md mục 5) — container hỏi xác
   * nhận trước khi gọi API. */
  onXoaThanhVien?: (giaoDanId: string, vaiTro: number) => void
  /** Tạo gia đình mới trống (chỉ Tên gia đình + Giáo họ) — chỉ dùng khi `moi`; sau khi tạo
   * xong, container chuyển sang chế độ sửa bình thường (có id thật) để dùng được các thao tác
   * Người nam/nữ/thành viên ở trên. */
  onTaoMoi?: (payload: { tenGiaDinh: string | null; giaoHoId: string | null }) => void
  /** Khoá `localStorage` cho bản nháp ngoại tuyến của form này — xem cùng tên ở
   * `GiaoDanDetail.Props` (`lib/banNhap.ts`). */
  khoaBanNhap?: string | null
  tenTaiKhoan?: string | null
  banNhap?: YeuCauCapNhatGiaDinh | null
  /** Đếm tăng dần mỗi lần container lưu THÀNH CÔNG (`PUT /api/gia-dinh/{id}`) — không dùng
   * chuỗi `thongBaoLuu` để dò vì nội dung câu chữ có thể đổi; đổi giá trị này là tín hiệu đủ để
   * xoá bản nháp tương ứng (xem effect bên dưới). */
  luuThanhCongDem?: number
}

const rong = (): GiaDinhDetailDuLieu => ({
  id: '', maGiaDinhCu: 0, maGiaDinhRieng: null, tenGiaDinh: null, giaoHoId: null,
  dienThoai: null, diaChi: null, soHoKhau: null, dienGiaDinh: null, ghiChu: null,
  daChuyenXu: false, ngayChuyen: null, noiChuyen: null, khongThongKe: false,
  rowVersion: 0, thanhVien: [],
})

const DIEN_GIA_DINH = ['', 'Nghèo', 'Cận nghèo', 'Neo đơn', 'Khuyết tật']

/** `ThanhVien` (tóm tắt trong `GiaDinhDetail`) chỉ mang vài trường hiển thị; các trường còn
 * lại của `GiaoDanListItem` chưa có (sẽ do endpoint thành viên riêng — Task 6/7 — trả về đầy
 * đủ hơn khi nối API thật) nên tạm điền `null`/giá trị mặc định để dùng chung cột với
 * `GxGiaoDanList`. */
function tuThanhVien(tv: ThanhVien, giaDinhId: string): GiaoDanListItem {
  return {
    id: tv.giaoDanId, maGiaoDanCu: 0, tenThanh: tv.tenThanh, hoTen: tv.hoTen, phai: tv.phai,
    ngaySinh: tv.ngaySinh, namSinh: tv.ngaySinh?.slice(0, 4) ?? '', ngayRuaToi: null,
    ngayRuocLe: null, ngayThemSuc: null, lapGd: false, hoTenCha: null, hoTenMe: null,
    tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
    diaChi: null, tenGiaoHo: null, daChuyenDi: false, trinhDoVanHoa: null,
    trinhDoChuyenMon: null, bietNgoaiNgu: null, quaDoi: tv.quaDoi, ngayQuaDoi: null,
    noiAnTang: null, noiSinh: null, noiRuaToi: null, noiRuocLe: null, noiThemSuc: null,
    quanHe: tenVaiTro(tv.vaiTro), giaDinhId, khongThongKe: false,
  }
}

/**
 * Chi tiết gia đình — bố cục neo đúng bản mẫu: đầu trang một dòng, khối thông tin dùng
 * `.cols`, lưới thành viên `GxGiaoDanList` (quanHeGiaDinh) chiếm phần dưới và tự cuộn, thanh
 * nút dưới cùng. Trường "Ngày chuyển"/"Nơi chuyển" chỉ dựng khi tick "Đã chuyển đi xứ khác",
 * và bị BỎ HẲN khỏi DOM lúc ẩn (không chỉ gắn `hidden`) để khớp `queryByLabelText` khi test.
 */
export function GiaDinhDetail({
  duLieu, moGiaoDan, moDanhSachGiaDinh, onLuu, dangLuu, thongBaoLuu, danhMucGiaoHo = [],
  onGanVoChong, onBoChonVoChong, onThemThanhVien, onXoaThanhVien, onTaoMoi,
  khoaBanNhap = null, tenTaiKhoan = null, banNhap = null, luuThanhCongDem,
}: Props) {
  // banNhap đè lên dữ liệu gốc khi người dùng bấm "Khôi phục" ở BanNhapBanner — xem chú thích ở
  // GiaoDanDetail.tsx (cùng cơ chế, container luôn đổi `key` kèm theo).
  const f = { ...(duLieu ?? rong()), ...(banNhap ?? {}) }
  const moi = !duLieu?.id
  const formRef = useRef<HTMLFormElement>(null)
  // Thêm thành viên: chọn người qua GxPicker rồi chọn vai trò trước khi bấm "Thêm vào gia đình"
  // — tách hai bước vì máy chủ cần biết VaiTro ngay từ đầu (không có vai trò "mặc định" hợp lý).
  const [dangThem, setDangThem] = useState<GiaoDanTimKiem | null>(null)
  const [vaiTroMoi, setVaiTroMoi] = useState(DANH_SACH_VAI_TRO_THANH_VIEN[0].giaTri)

  const [daChuyenXu, setDaChuyenXu] = useState(f.daChuyenXu)
  // null = "Ngoài xứ" (xem NGOAI_XU ở trên) — KHÔNG phải khoá ngoại tới bảng giao_ho.
  const [giaoHoId, setGiaoHoId] = useState<string | null>(f.giaoHoId)

  const tenGiaoHoHienTai = giaoHoId === null ? NGOAI_XU
    : danhMucGiaoHo.find((g) => g.id === giaoHoId)?.tenGiaoHo ?? `(#${giaoHoId.slice(0, 8)}…)`
  const dsGiaoHo = giaoHoId && !danhMucGiaoHo.some((g) => g.id === giaoHoId)
    ? [{ id: giaoHoId, tenGiaoHo: tenGiaoHoHienTai, maGiaoHoCu: 0, giaoHoChaId: null }, ...danhMucGiaoHo]
    : danhMucGiaoHo

  // Quy ước đã chốt: vaiTro 0 = Chồng, 1 = Vợ, giá trị khác (2=Con, 3=Cháu, 4=Cha... 100=Chưa
  // rõ, xem lib/vaiTroGiaDinh.ts) = "thành viên khác". "Thành viên khác trong gia đình" loại
  // trừ vợ chồng (vaiTro 0/1) — họ đã hiển thị riêng ở hai ô "Người nam"/"Người nữ" phía trên,
  // đúng hàm `thanhVienCua()` của bản mẫu (lọc bỏ quanHe "Chồng"/"Vợ" khỏi lưới thành viên
  // khác). KHÔNG dùng `chuHo` cho việc này — đúng MỘT người trong gia đình có `chuHo = true`
  // (chủ hộ theo hộ khẩu), không nhất thiết là người nam hay đã kết hôn.
  const thanhVien = useMemo(
    () => f.thanhVien.filter((t) => t.vaiTro !== 0 && t.vaiTro !== 1).map((t) => tuThanhVien(t, f.id)),
    [f.thanhVien, f.id],
  )
  const chuHo = f.thanhVien.find((t) => t.chuHo)
  const nguoiNam = f.thanhVien.find((t) => t.vaiTro === 0)
  const nguoiNu = f.thanhVien.find((t) => t.vaiTro === 1)

  const tenTieuDe = moi ? 'Gia đình mới' : `Gia đình ${f.tenGiaDinh ?? ''}`.trim()

  // Xem & sửa một thành viên mở thẳng thẻ chi tiết giáo dân đó — cùng cách "Xem gia đình"
  // của GxGiaoDanList dùng ở màn hình danh sách, chỉ khác đích đến. Thêm mục "Xoá khỏi gia
  // đình" (xoá VĨNH VIỄN, can-review-sau.md mục 5) — cần tra lại vaiTro thô từ f.thanhVien vì
  // GiaoDanListItem hiển thị trên lưới chỉ mang nhãn (quanHe), không mang số vai trò.
  const menuThanhVien = useMemo(
    () => [
      ...menuGiaoDanMacDinh((d) => moGiaoDan?.(d.id), () => {}),
      {
        nhan: 'Xoá khỏi gia đình',
        chay: (d: GiaoDanListItem) => {
          const vaiTro = f.thanhVien.find((t) => t.giaoDanId === d.id)?.vaiTro
          if (vaiTro !== undefined) onXoaThanhVien?.(d.id, vaiTro)
        },
      },
    ],
    [moGiaoDan, onXoaThanhVien, f.thanhVien],
  )

  // Dựng payload từ form (chủ yếu là input không kiểm soát — defaultValue) — dùng chung cho
  // việc lưu thật (xuLySubmit, chỉ ở chế độ sửa — chế độ tạo mới payload khác hẳn nên tách
  // riêng bên dưới) VÀ tự lưu nháp định kỳ (useTuDongLuuBanNhap). giaoHoId lấy từ state thật
  // (danh mục GET /api/giao-ho, xem prop danhMucGiaoHo) — trước đây field này cố tình giữ
  // nguyên giá trị cũ vì combobox chỉ liệt kê TÊN cứng không có Id.
  function dungPayloadTuForm(fd: FormData): YeuCauCapNhatGiaDinh {
    const chuoi = (ten: string) => (fd.get(ten) as string | null)?.trim() || null
    const chuHoThoRaw = fd.get('chuho')
    const chuHoVaiTro: 0 | 1 | null = chuHoThoRaw === '0' ? 0 : chuHoThoRaw === '1' ? 1 : null
    return {
      tenGiaDinh: chuoi('tenGiaDinh'),
      giaoHoId,
      chuHoVaiTro,
      dienThoai: chuoi('dienThoai'),
      diaChi: chuoi('diaChi'),
      soHoKhau: chuoi('soHoKhau'),
      dienGiaDinh: chuoi('dienGiaDinh'),
      ghiChu: chuoi('ghiChu'),
      daChuyenXu,
      ngayChuyen: daChuyenXu ? chuoi('ngayChuyen') : null,
      noiChuyen: daChuyenXu ? chuoi('noiChuyen') : null,
      khongThongKe: fd.get('khongThongKe') === 'on',
      rowVersion: f.rowVersion,
      honPhoi: null,
    }
  }

  function xuLySubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!formRef.current) return
    const fd = new FormData(formRef.current)

    if (moi) {
      // Rule 6 (checkInput, dòng 1301): "Hãy nhập tên gia đình!" — chỉ kiểm tra tối thiểu này
      // ở bước Tạo mới, các quy tắc còn lại (Giáo họ, chủ hộ...) chưa migrate đủ ở web (xem
      // gia-dinh-chi-tiet.md mục 10, "Validate... Thiếu ở frontend").
      const ten = (fd.get('tenGiaDinh') as string | null)?.trim() || null
      if (!ten) { window.alert('Hãy nhập tên gia đình!'); return }
      onTaoMoi?.({ tenGiaDinh: ten, giaoHoId })
      return
    }

    if (!onLuu) return
    onLuu(dungPayloadTuForm(fd))
  }

  // Tự lưu nháp định kỳ (xem lib/banNhap.ts) — chỉ áp dụng cho chế độ SỬA (form tạo mới chỉ có
  // hai trường tối thiểu, rủi ro mất công gõ thấp hơn nhiều so với 60+ trường của form giáo dân
  // nên không bật ở đây để đỡ phức tạp — xem can-review-sau.md).
  useTuDongLuuBanNhap(moi ? null : khoaBanNhap, tenTaiKhoan, () => {
    if (moi || !formRef.current) return null
    return dungPayloadTuForm(new FormData(formRef.current))
  })

  // Lưu thành công — xoá nháp ngay (xem chú thích cùng tên ở GiaoDanDetail.tsx).
  useEffect(() => {
    if (luuThanhCongDem !== undefined && luuThanhCongDem > 0 && khoaBanNhap) xoaBanNhap(khoaBanNhap)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [luuThanhCongDem])

  return (
    <form className="page detail-page" ref={formRef} onSubmit={xuLySubmit}>
      <div className="page-head detail-head">
        <button type="button" className="btn btn-sm btn-quiet" onClick={() => moDanhSachGiaDinh?.()}>
          ← Danh sách
        </button>
        <h1>{tenTieuDe}</h1>
        <span className="head-sub">
          {moi ? 'Chưa lưu · nhập thông tin rồi bấm Cập nhật' : `${tenGiaoHoHienTai} · ${f.thanhVien.length} nhân khẩu`}
        </span>
        <div className="spacer" />
        <span className={'tag tag-' + (moi ? 'violet' : f.khongThongKe ? 'amber' : 'mint')}>
          {moi ? 'Bản nháp' : f.khongThongKe ? 'Không tính thống kê' : 'Đang hoạt động'}
        </span>
      </div>

      <div className="cols">
        <div className="card glass">
          <div className="card-head"><h2>Thông tin gia đình</h2><span className="eyebrow">Sổ gia đình</span></div>
          <GxField label="Mã gia đình" id="gdinh-ma"
            extra={<><GxInline>Số hộ khẩu</GxInline><input aria-label="Số hộ khẩu" name="soHoKhau" type="text" defaultValue={f.soHoKhau ?? ''} /></>}>
            <input id="gdinh-ma" type="text" value={moi ? '(tự sinh khi lưu)' : String(f.maGiaDinhCu)} disabled style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người nam" id="gdinh-nguoinam"
            extra={<label className="seg">
              <input type="radio" name="chuho" value="0" disabled={!nguoiNam}
                defaultChecked={!!nguoiNam && chuHo?.giaoDanId === nguoiNam.giaoDanId} />Chủ hộ
            </label>}>
            <GxPicker id="gdinh-nguoinam" value={nguoiNam ? `${nguoiNam.tenThanh ?? ''} ${nguoiNam.hoTen}`.trim() : null}
              onChon={moi ? undefined : (gd) => onGanVoChong?.(0, gd)}
              onBoChon={moi || !nguoiNam ? undefined : () => onBoChonVoChong?.(0)} />
          </GxField>
          <GxField label="Người nữ" id="gdinh-nguoinu"
            extra={<label className="seg">
              <input type="radio" name="chuho" value="1" disabled={!nguoiNu}
                defaultChecked={!!nguoiNu && chuHo?.giaoDanId === nguoiNu.giaoDanId} />Chủ hộ
            </label>}>
            <GxPicker id="gdinh-nguoinu" value={nguoiNu ? `${nguoiNu.tenThanh ?? ''} ${nguoiNu.hoTen}`.trim() : null}
              onChon={moi ? undefined : (gd) => onGanVoChong?.(1, gd)}
              onBoChon={moi || !nguoiNu ? undefined : () => onBoChonVoChong?.(1)} />
          </GxField>
          <GxField label="Tên gia đình" id="gdinh-ten">
            <input id="gdinh-ten" name="tenGiaDinh" type="text" defaultValue={f.tenGiaDinh ?? ''} />
          </GxField>
          <GxField label="Giáo họ" id="gdinh-giaoho">
            <select id="gdinh-giaoho" value={giaoHoId ?? NGOAI_XU}
              onChange={(e) => setGiaoHoId(e.target.value === NGOAI_XU ? null : e.target.value)}>
              <option value={NGOAI_XU}>{NGOAI_XU}</option>
              {dsGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
            </select>
          </GxField>
          <GxField label="Điện thoại" id="gdinh-dienthoai"
            extra={
              <>
                <GxInline>Diện</GxInline>
                <select aria-label="Diện gia đình" name="dienGiaDinh" defaultValue={f.dienGiaDinh ?? ''}>
                  {DIEN_GIA_DINH.map((d) => <option key={d} value={d}>{d || 'Không thuộc diện nào'}</option>)}
                </select>
              </>
            }>
            <input id="gdinh-dienthoai" name="dienThoai" type="text" defaultValue={f.dienThoai ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Địa chỉ" id="gdinh-diachi" extra={<button type="button" className="btn btn-sm btn-quiet">Bản đồ</button>}>
            <input id="gdinh-diachi" name="diaChi" type="text" defaultValue={f.diaChi ?? ''} />
          </GxField>
          <GxField label="Ghi chú" id="gdinh-ghichu">
            <textarea id="gdinh-ghichu" name="ghiChu" defaultValue={f.ghiChu ?? ''} placeholder="Ghi chú nội bộ về gia đình…" />
          </GxField>
          <GxField label="">
            <label className="toggle">
              <input type="checkbox" checked={daChuyenXu} onChange={(e) => setDaChuyenXu(e.target.checked)} />
              Đã chuyển đi xứ khác
            </label>
            <label className="toggle">
              <input type="checkbox" name="khongThongKe" defaultChecked={f.khongThongKe} />
              Không tính vào thống kê
            </label>
          </GxField>
          {daChuyenXu && (
            <>
              <GxField label="Ngày chuyển" id="gdinh-ngaychuyen">
                <GxDate id="gdinh-ngaychuyen" name="ngayChuyen" defaultValue={f.ngayChuyen} style={{ maxWidth: 190 }} />
              </GxField>
              <GxField label="Nơi chuyển" id="gdinh-noichuyen">
                <input id="gdinh-noichuyen" name="noiChuyen" type="text" defaultValue={f.noiChuyen ?? ''} placeholder="Giáo xứ / địa phương chuyển đến" />
              </GxField>
            </>
          )}
        </div>

        <div className="col-stack">
          <div className="card glass">
            <div className="card-head"><h2>Hình gia đình</h2></div>
            <div className="photo-slot">Chưa có hình<br />Nhấp để tải ảnh lên</div>
          </div>
        </div>
      </div>

      <div className="card-head" style={{ padding: '0 2px' }}>
        <h2>Thành viên khác trong gia đình</h2>
        <div className="spacer" />
        <span className="count-pill"><b>{thanhVien.length}</b> người</span>
      </div>
      {!moi && (
        <div className="thanhvien-them" style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '4px 2px 10px' }}>
          <GxPicker id="gdinh-them-thanhvien" value={dangThem ? `${dangThem.tenThanh ?? ''} ${dangThem.hoTen}`.trim() : null}
            onChon={setDangThem} onBoChon={() => setDangThem(null)} />
          <select aria-label="Vai trò thành viên mới" value={vaiTroMoi} onChange={(e) => setVaiTroMoi(Number(e.target.value))}>
            {DANH_SACH_VAI_TRO_THANH_VIEN.map((v) => <option key={v.giaTri} value={v.giaTri}>{v.nhan}</option>)}
          </select>
          <button type="button" className="btn btn-sm" disabled={!dangThem}
            onClick={() => { if (dangThem) { onThemThanhVien?.(dangThem, vaiTroMoi); setDangThem(null) } }}>
            Thêm vào gia đình
          </button>
        </div>
      )}
      <GxGiaoDanList
        quanHeGiaDinh
        rows={thanhVien}
        hangLoc={false}
        onMo={(d) => moGiaoDan?.(d.id)}
        menuChuotPhai={menuThanhVien}
      />

      <div className="cmdbar">
        <span className="hint" role={thongBaoLuu ? 'status' : undefined}>
          {thongBaoLuu ?? (moi ? 'Nhập Tên gia đình rồi bấm "Tạo gia đình" — chọn Người nam/nữ và thành viên sau khi đã tạo' : 'Chưa có thay đổi')}
        </span>
        <div className="spacer" />
        <button type="button" className="btn">In lý lịch cá nhân</button>
        <button type="button" className="btn">In phiếu gia đình</button>
        <button type="button" className="btn btn-quiet" onClick={() => moDanhSachGiaDinh?.()}>Quay về</button>
        <button type="submit" className="btn btn-primary" disabled={moi ? (!onTaoMoi || dangLuu) : (!onLuu || dangLuu)}>
          {dangLuu ? 'Đang lưu…' : moi ? 'Tạo gia đình' : 'Cập nhật'}
        </button>
      </div>
    </form>
  )
}
