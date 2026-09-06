import { useMemo, useState } from 'react'
import type { GiaDinhDetail as GiaDinhDetailDuLieu, GiaoDanListItem, ThanhVien } from '../api/types'
import { GxField, GxInline } from '../components/GxField'
import { GxGiaoDanList, menuGiaoDanMacDinh } from '../components/GxGiaoDanList'
import { GxPicker } from '../components/GxPicker'
import { DANH_SACH_GIAO_HO_TAM, NGOAI_XU } from '../data/giaoHoTam'

type Props = {
  duLieu?: GiaDinhDetailDuLieu
  /** Nút "Xem & sửa" trên lưới thành viên mở thẻ chi tiết giáo dân tương ứng. */
  moGiaoDan?: (id: string) => void
  /** "Quay về" và "← Danh sách" mở lại thẻ danh sách gia đình — có thẻ thì chuyển tiêu
   * điểm, chưa có thì mở mới (quy tắc của `useTabDocs`), không quay về thẻ Tổng quan. */
  moDanhSachGiaDinh?: () => void
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
function tuThanhVien(tv: ThanhVien): GiaoDanListItem {
  return {
    id: tv.giaoDanId, maGiaoDanCu: 0, tenThanh: tv.tenThanh, hoTen: tv.hoTen, phai: tv.phai,
    ngaySinh: tv.ngaySinh, namSinh: tv.ngaySinh?.slice(0, 4) ?? '', ngayRuaToi: null,
    ngayRuocLe: null, ngayThemSuc: null, lapGd: false, hoTenCha: null, hoTenMe: null,
    tanTong: false, conHoc: false, ngheNghiep: null, ghiChu: null, dienThoai: null,
    diaChi: null, tenGiaoHo: null, daChuyenDi: false, trinhDoVanHoa: null,
    trinhDoChuyenMon: null, bietNgoaiNgu: null, quaDoi: tv.quaDoi, ngayQuaDoi: null,
    noiAnTang: null, noiSinh: null, noiRuaToi: null, noiRuocLe: null, noiThemSuc: null,
    quanHe: null,
  }
}

/**
 * Chi tiết gia đình — bố cục neo đúng bản mẫu: đầu trang một dòng, khối thông tin dùng
 * `.cols`, lưới thành viên `GxGiaoDanList` (quanHeGiaDinh) chiếm phần dưới và tự cuộn, thanh
 * nút dưới cùng. Trường "Ngày chuyển"/"Nơi chuyển" chỉ dựng khi tick "Đã chuyển đi xứ khác",
 * và bị BỎ HẲN khỏi DOM lúc ẩn (không chỉ gắn `hidden`) để khớp `queryByLabelText` khi test.
 */
export function GiaDinhDetail({ duLieu, moGiaoDan, moDanhSachGiaDinh }: Props) {
  const f = duLieu ?? rong()
  const moi = !duLieu?.id

  const [daChuyenXu, setDaChuyenXu] = useState(f.daChuyenXu)
  const [giaoHo, setGiaoHo] = useState<string>(f.giaoHoId ?? NGOAI_XU)

  const dsGiaoHo = f.giaoHoId && !DANH_SACH_GIAO_HO_TAM.includes(f.giaoHoId)
    ? [f.giaoHoId, ...DANH_SACH_GIAO_HO_TAM]
    : DANH_SACH_GIAO_HO_TAM

  // "Thành viên khác trong gia đình" loại trừ vợ chồng (chuHo) — họ đã hiển thị riêng ở hai ô
  // "Người nam"/"Người nữ" phía trên, đúng hàm `thanhVienCua()` của bản mẫu (lọc bỏ quanHe
  // "Chồng"/"Vợ" khỏi lưới thành viên khác).
  const thanhVien = useMemo(
    () => f.thanhVien.filter((t) => !t.chuHo).map(tuThanhVien),
    [f.thanhVien],
  )
  const chuHo = f.thanhVien.find((t) => t.chuHo)
  const nguoiNam = f.thanhVien.find((t) => t.chuHo && t.phai === 'Nam')
  const nguoiNu = f.thanhVien.find((t) => t.chuHo && t.phai === 'Nữ')

  const tenTieuDe = moi ? 'Gia đình mới' : `Gia đình ${f.tenGiaDinh ?? ''}`.trim()

  // Xem & sửa một thành viên mở thẳng thẻ chi tiết giáo dân đó — cùng cách "Xem gia đình"
  // của GxGiaoDanList dùng ở màn hình danh sách, chỉ khác đích đến.
  const menuThanhVien = useMemo(
    () => menuGiaoDanMacDinh((d) => moGiaoDan?.(d.id), () => {}),
    [moGiaoDan],
  )

  return (
    <section className="page detail-page">
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
            extra={<><GxInline>Số hộ khẩu</GxInline><input aria-label="Số hộ khẩu" type="text" defaultValue={f.soHoKhau ?? ''} /></>}>
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
            <input id="gdinh-ten" type="text" defaultValue={f.tenGiaDinh ?? ''} />
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
                <select aria-label="Diện gia đình" defaultValue={f.dienGiaDinh ?? ''}>
                  {DIEN_GIA_DINH.map((d) => <option key={d} value={d}>{d || 'Không thuộc diện nào'}</option>)}
                </select>
              </>
            }>
            <input id="gdinh-dienthoai" type="text" defaultValue={f.dienThoai ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Địa chỉ" id="gdinh-diachi" extra={<button type="button" className="btn btn-sm btn-quiet">Bản đồ</button>}>
            <input id="gdinh-diachi" type="text" defaultValue={f.diaChi ?? ''} />
          </GxField>
          <GxField label="Ghi chú" id="gdinh-ghichu">
            <textarea id="gdinh-ghichu" defaultValue={f.ghiChu ?? ''} placeholder="Ghi chú nội bộ về gia đình…" />
          </GxField>
          <GxField label="">
            <label className="toggle">
              <input type="checkbox" checked={daChuyenXu} onChange={(e) => setDaChuyenXu(e.target.checked)} />
              Đã chuyển đi xứ khác
            </label>
            <label className="toggle">
              <input type="checkbox" defaultChecked={f.khongThongKe} />
              Không tính vào thống kê
            </label>
          </GxField>
          {daChuyenXu && (
            <>
              <GxField label="Ngày chuyển" id="gdinh-ngaychuyen">
                <input id="gdinh-ngaychuyen" type="date" defaultValue={f.ngayChuyen ?? ''} style={{ maxWidth: 190 }} />
              </GxField>
              <GxField label="Nơi chuyển" id="gdinh-noichuyen">
                <input id="gdinh-noichuyen" type="text" defaultValue={f.noiChuyen ?? ''} placeholder="Giáo xứ / địa phương chuyển đến" />
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
        <span className="hint">Thay đổi chưa được lưu</span>
        <div className="spacer" />
        <button type="button" className="btn">In lý lịch cá nhân</button>
        <button type="button" className="btn">In phiếu gia đình</button>
        <button type="button" className="btn btn-quiet" onClick={() => moDanhSachGiaDinh?.()}>Quay về</button>
        <button type="button" className="btn btn-primary">Cập nhật</button>
      </div>
    </section>
  )
}
