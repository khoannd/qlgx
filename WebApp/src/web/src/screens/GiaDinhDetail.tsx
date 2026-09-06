import { useMemo, useRef, useState, type FormEvent } from 'react'
import type { GiaDinhDetail as GiaDinhDetailDuLieu, GiaoDanListItem, ThanhVien } from '../api/types'
import { GxField, GxInline } from '../components/GxField'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'
import { GxPicker } from '../components/GxPicker'
import { DANH_SACH_GIAO_HO_TAM, NGOAI_XU } from '../data/giaoHoTam'

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
    quanHe: null, giaDinhId, khongThongKe: false,
  }
}

/**
 * Chi tiết gia đình — bố cục neo đúng bản mẫu: đầu trang một dòng, khối thông tin dùng
 * `.cols`, lưới thành viên `GxGiaoDanList` (quanHeGiaDinh) chiếm phần dưới và tự cuộn, thanh
 * nút dưới cùng. Trường "Ngày chuyển"/"Nơi chuyển" chỉ dựng khi tick "Đã chuyển đi xứ khác",
 * và bị BỎ HẲN khỏi DOM lúc ẩn (không chỉ gắn `hidden`) để khớp `queryByLabelText` khi test.
 */
export function GiaDinhDetail({
  duLieu, moGiaoDan, moDanhSachGiaDinh, onLuu, dangLuu, thongBaoLuu,
}: Props) {
  const f = duLieu ?? rong()
  const moi = !duLieu?.id
  const formRef = useRef<HTMLFormElement>(null)

  const [daChuyenXu, setDaChuyenXu] = useState(f.daChuyenXu)
  const [giaoHo, setGiaoHo] = useState<string>(f.giaoHoId ?? NGOAI_XU)

  const dsGiaoHo = f.giaoHoId && !DANH_SACH_GIAO_HO_TAM.includes(f.giaoHoId)
    ? [f.giaoHoId, ...DANH_SACH_GIAO_HO_TAM]
    : DANH_SACH_GIAO_HO_TAM

  // Quy ước đã chốt: vaiTro 0 = Chồng, 1 = Vợ, 2 = Con. "Thành viên khác trong gia đình" loại
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
  // của GxGiaoDanList dùng ở màn hình danh sách, chỉ khác đích đến.
  const menuThanhVien = useMemo(
    () => menuGiaoDanMacDinh((d) => moGiaoDan?.(d.id), () => {}),
    [moGiaoDan],
  )

  // Dựng payload từ form (chủ yếu là input không kiểm soát — defaultValue) rồi giao cho
  // container qua onLuu; container gọi API thật, xử lý thành công/lỗi/xung đột RowVersion.
  // giaoHoId CỐ TÌNH giữ nguyên giá trị đã tải (f.giaoHoId), KHÔNG suy từ <select> "Giáo họ":
  // combobox đó tạm liệt kê TÊN giáo họ cứng (`DANH_SACH_GIAO_HO_TAM`, xem file đó) vì backend
  // chưa có danh mục giáo họ thật kèm Id — gửi nhầm tên lên chỗ backend cần Guid sẽ hỏng dữ
  // liệu, nên màn hình này chưa cho đổi giáo họ qua API cho tới khi có danh mục thật.
  function xuLySubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!onLuu || !formRef.current) return
    const fd = new FormData(formRef.current)
    const chuoi = (ten: string) => (fd.get(ten) as string | null)?.trim() || null

    onLuu({
      tenGiaDinh: chuoi('tenGiaDinh'),
      giaoHoId: f.giaoHoId,
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
    })
  }

  return (
    <form className="page detail-page" ref={formRef} onSubmit={xuLySubmit}>
      <div className="page-head detail-head">
        <button type="button" className="btn btn-sm btn-quiet" onClick={() => moDanhSachGiaDinh?.()}>
          ← Danh sách
        </button>
        <h1>{tenTieuDe}</h1>
        <span className="head-sub">
          {moi ? 'Chưa lưu · nhập thông tin rồi bấm Cập nhật' : `${giaoHo} · ${f.thanhVien.length} nhân khẩu`}
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
            extra={<label className="seg"><input type="radio" name="chuho" defaultChecked={chuHo?.phai === 'Nam'} />Chủ hộ</label>}>
            <GxPicker id="gdinh-nguoinam" value={nguoiNam ? `${nguoiNam.tenThanh ?? ''} ${nguoiNam.hoTen}`.trim() : null} />
          </GxField>
          <GxField label="Người nữ" id="gdinh-nguoinu"
            extra={<label className="seg"><input type="radio" name="chuho" defaultChecked={chuHo?.phai === 'Nữ'} />Chủ hộ</label>}>
            <GxPicker id="gdinh-nguoinu" value={nguoiNu ? `${nguoiNu.tenThanh ?? ''} ${nguoiNu.hoTen}`.trim() : null} />
          </GxField>
          <GxField label="Tên gia đình" id="gdinh-ten">
            <input id="gdinh-ten" name="tenGiaDinh" type="text" defaultValue={f.tenGiaDinh ?? ''} />
          </GxField>
          <GxField label="Giáo họ" id="gdinh-giaoho">
            <select id="gdinh-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
              <option value={NGOAI_XU}>{NGOAI_XU}</option>
              {dsGiaoHo.map((g) => <option key={g} value={g}>{g}</option>)}
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
                <input id="gdinh-ngaychuyen" name="ngayChuyen" type="date" defaultValue={f.ngayChuyen ?? ''} style={{ maxWidth: 190 }} />
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
      <GxGiaoDanList
        quanHeGiaDinh
        rows={thanhVien}
        hangLoc={false}
        onMo={(d) => moGiaoDan?.(d.id)}
        menuChuotPhai={menuThanhVien}
      />

      <div className="cmdbar">
        <span className="hint" role={thongBaoLuu ? 'status' : undefined}>
          {thongBaoLuu ?? (moi ? 'Chưa hỗ trợ tạo mới gia đình qua web ở giai đoạn này' : 'Thay đổi chưa được lưu')}
        </span>
        <div className="spacer" />
        <button type="button" className="btn">In lý lịch cá nhân</button>
        <button type="button" className="btn">In phiếu gia đình</button>
        <button type="button" className="btn btn-quiet" onClick={() => moDanhSachGiaDinh?.()}>Quay về</button>
        <button type="submit" className="btn btn-primary" disabled={moi || !onLuu || dangLuu}>
          {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
        </button>
      </div>
    </form>
  )
}
