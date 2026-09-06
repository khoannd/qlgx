import { useRef, useState, type FormEvent } from 'react'
import type { GiaoDanDetail as GiaoDanDetailDuLieu } from '../api/types'
import { GxField, GxInline } from '../components/GxField'
import { GxFormTabs } from '../components/GxFormTabs'
import { GxPicker } from '../components/GxPicker'
import { DANH_SACH_GIAO_HO_TAM, NGOAI_XU } from '../data/giaoHoTam'

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
  rowVersion: number
}

type Props = {
  duLieu?: GiaoDanDetailDuLieu
  /** Nút "Xem gia đình" mở thẻ chi tiết gia đình tương ứng. */
  moGiaDinh?: (id: string) => void
  /** "Quay về" và "← Danh sách" mở lại thẻ danh sách giáo dân — có thẻ thì chuyển tiêu
   * điểm, chưa có thì mở mới (quy tắc của `useTabDocs`), không quay về thẻ Tổng quan. */
  moDanhSachGiaoDan?: () => void
  /** Nút "Cập nhật" gọi hàm này với payload dựng từ form — container (`GiaoDanDetailPage`)
   * gọi API thật và xử lý xung đột RowVersion. Bản ghi mới chưa có API tạo nên nút bị vô hiệu
   * hoá khi thiếu prop này. */
  onLuu?: (payload: YeuCauCapNhatGiaoDan) => void
  dangLuu?: boolean
  thongBaoLuu?: string | null
}

const rong = (): GiaoDanDetailDuLieu => ({
  id: '', maGiaoDanCu: 0, hoTen: '', tenThanh: null, phai: 'Nam', ngaySinh: null,
  noiSinh: null, cmnd: null, danToc: null, giaoHoId: null, diaChi: null, dienThoai: null,
  email: null, hoTenCha: null, hoTenMe: null, soRuaToi: null, ngayRuaToi: null,
  noiRuaToi: null, chaRuaToi: null, nguoiDoDauRuaToi: null, soRuocLe: null, ngayRuocLe: null,
  noiRuocLe: null, chaRuocLe: null, soThemSuc: null, ngayThemSuc: null, noiThemSuc: null,
  chaThemSuc: null, nguoiDoDauThemSuc: null, ngayXucDau: null, nguoiXucDau: null,
  tinhTrangXucDau: null, ghiChuXucDau: null, trinhDoVanHoa: null, trinhDoChuyenMon: null,
  bietNgoaiNgu: null, ngheNghiep: null, conHoc: false, daCoGiaDinh: false, tanTong: false,
  khongThongKe: false, quaDoi: false, ngayQuaDoi: null, noiQuaDoi: null, soAnTang: null,
  noiAnTang: null, ghiChu: null, giaDinhId: null, tenGiaDinh: null, vaiTro: null, rowVersion: 0,
})

const TINH_TRANG_XUC_DAU = ['', 'Nguy tử', 'Thông thường']
const CHUYEN_XU = ['Ở tại xứ', 'Chuyển từ xứ khác đến', 'Đã chuyển đi xứ khác']

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
  duLieu, moGiaDinh, moDanhSachGiaoDan, onLuu, dangLuu, thongBaoLuu,
}: Props) {
  const p = duLieu ?? rong()
  const moi = !duLieu?.id
  const formRef = useRef<HTMLFormElement>(null)

  const [quaDoi, setQuaDoi] = useState(p.quaDoi)
  const [conHoc, setConHoc] = useState(p.conHoc)
  const [giaoHo, setGiaoHo] = useState<string>(p.giaoHoId ?? NGOAI_XU)
  const [giaoDanAo, setGiaoDanAo] = useState(p.khongThongKe)

  const doiQuaDoi = (v: boolean) => { setQuaDoi(v); if (v) setConHoc(false) }
  const doiConHoc = (v: boolean) => { setConHoc(v); if (v) setQuaDoi(false) }
  const doiGiaoDanAo = (v: boolean) => { setGiaoDanAo(v); if (v) setGiaoHo(NGOAI_XU) }

  const ngoaiXu = giaoHo === NGOAI_XU
  // giaoHoId thật (không phải sentinel "Ngoài xứ") có thể chưa nằm trong danh mục tạm bên
  // dưới — chèn thêm để <select> không rơi vào trạng thái "không khớp option nào".
  const dsGiaoHo = p.giaoHoId && !DANH_SACH_GIAO_HO_TAM.includes(p.giaoHoId)
    ? [p.giaoHoId, ...DANH_SACH_GIAO_HO_TAM]
    : DANH_SACH_GIAO_HO_TAM

  const tenDayDu = moi ? 'Giáo dân mới' : `${p.tenThanh ? p.tenThanh + ' ' : ''}${p.hoTen}`
  const tinhTrang = quaDoi ? 'Đã qua đời' : moi ? 'Bản nháp' : 'Đang hoạt động'
  const tagTone = quaDoi ? 'amber' : moi ? 'violet' : 'mint'

  const tabCaNhan = (
    <>
      <div className="cols cols-even">
        <div className="card glass">
          <div className="card-head"><h2>Thông tin cá nhân</h2><span className="eyebrow">Hồ sơ giáo dân</span></div>
          <GxField label="Mã giáo dân" id="gd-ma">
            <input id="gd-ma" type="text" value={moi ? '(tự sinh khi lưu)' : String(p.maGiaoDanCu)} disabled style={{ maxWidth: 150 }} />
            <GxInline>Giới tính</GxInline>
            <select aria-label="Giới tính" name="phai" defaultValue={p.phai ?? 'Nam'} style={{ maxWidth: 100 }}>
              <option value="Nam">Nam</option>
              <option value="Nữ">Nữ</option>
            </select>
            <GxInline>Ngày sinh</GxInline>
            <input type="date" aria-label="Ngày sinh" name="ngaySinh" defaultValue={p.ngaySinh ?? ''} style={{ maxWidth: 170 }} />
          </GxField>
          <GxField label="Tên thánh" id="gd-tenthanh">
            <input id="gd-tenthanh" name="tenThanh" type="text" defaultValue={p.tenThanh ?? ''} style={{ maxWidth: 170 }} />
            <GxInline>Họ tên</GxInline>
            <input aria-label="Họ tên" name="hoTen" type="text" defaultValue={p.hoTen} />
          </GxField>
          <GxField label="Nơi sinh" id="gd-noisinh">
            <input id="gd-noisinh" name="noiSinh" type="text" defaultValue={p.noiSinh ?? ''} />
          </GxField>
          <GxField label="Tên Cha" id="gd-tencha"><GxPicker id="gd-tencha" value={p.hoTenCha} /></GxField>
          <GxField label="Tên Mẹ" id="gd-tenme"><GxPicker id="gd-tenme" value={p.hoTenMe} /></GxField>
          <GxField label="Giáo họ" id="gd-giaoho">
            <select id="gd-giaoho" value={giaoHo} onChange={(e) => setGiaoHo(e.target.value)}>
              <option value={NGOAI_XU}>{NGOAI_XU}</option>
              {dsGiaoHo.map((g) => <option key={g} value={g}>{g}</option>)}
            </select>
          </GxField>
          {ngoaiXu && (
            <GxField label="Giáo xứ" id="gd-giaoxu">
              <input id="gd-giaoxu" type="text" defaultValue="" style={{ maxWidth: 230 }} />
              <GxInline>Giáo phận</GxInline>
              <input aria-label="Giáo phận" type="text" defaultValue="" />
            </GxField>
          )}
          <GxField label="CMND / CCCD" id="gd-cmnd"
            extra={
              <label className="toggle">
                <input type="checkbox" checked={giaoDanAo} onChange={(e) => doiGiaoDanAo(e.target.checked)} />
                Là giáo dân không được thống kê
              </label>
            }>
            <input id="gd-cmnd" name="cmnd" type="text" defaultValue={p.cmnd ?? ''} style={{ maxWidth: 210 }} />
          </GxField>
        </div>

        <div className="col-stack">
          <div className="card glass">
            <div className="card-head"><h2>Ảnh đại diện (ảnh 3x4)</h2></div>
            <div className="photo-slot">Chưa có hình<br />Nhấp để tải ảnh lên</div>
          </div>
          {!ngoaiXu && (
            <div className="card glass">
              <div className="card-head"><h2>Thông tin chuyển xứ</h2></div>
              <GxField label="Thông tin hiện tại" id="gd-chuyenxu">
                <select id="gd-chuyenxu" defaultValue={CHUYEN_XU[0]}>
                  {CHUYEN_XU.map((c) => <option key={c} value={c}>{c}</option>)}
                </select>
              </GxField>
              <GxField label="Giáo xứ" id="gd-giaoxu-chuyen">
                <input id="gd-giaoxu-chuyen" type="text" placeholder="Giáo xứ chuyển đi / chuyển đến" />
              </GxField>
            </div>
          )}
        </div>
      </div>

      <div className="card-row">
        <div className="card glass">
          <div className="card-head"><h2>Rửa tội</h2></div>
          <GxField label="Ngày rửa tội" id="gd-ngayruatoi">
            <input id="gd-ngayruatoi" name="ngayRuaToi" type="date" defaultValue={p.ngayRuaToi ?? ''} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ rửa tội" name="soRuaToi" type="text" defaultValue={p.soRuaToi ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-charuatoi"><GxPicker id="gd-charuatoi" value={p.chaRuaToi} /></GxField>
          <GxField label="Người đỡ đầu" id="gd-dodauruatoi">
            <input id="gd-dodauruatoi" name="nguoiDoDauRuaToi" type="text" defaultValue={p.nguoiDoDauRuaToi ?? ''} />
          </GxField>
          <GxField label="Nơi rửa tội" id="gd-noiruatoi">
            <input id="gd-noiruatoi" name="noiRuaToi" type="text" defaultValue={p.noiRuaToi ?? ''} />
          </GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Rước lễ lần đầu</h2></div>
          <GxField label="Ngày rước lễ" id="gd-ngayruocle">
            <input id="gd-ngayruocle" name="ngayRuocLe" type="date" defaultValue={p.ngayRuocLe ?? ''} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ rước lễ" name="soRuocLe" type="text" defaultValue={p.soRuocLe ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-charuocle"><GxPicker id="gd-charuocle" value={p.chaRuocLe} /></GxField>
          <GxField label="Nơi rước lễ" id="gd-noiruocle">
            <input id="gd-noiruocle" name="noiRuocLe" type="text" defaultValue={p.noiRuocLe ?? ''} />
          </GxField>
        </div>
      </div>

      <div className="card-row">
        <div className="card glass">
          <div className="card-head"><h2>Thêm sức</h2></div>
          <GxField label="Ngày thêm sức" id="gd-ngaythemsuc">
            <input id="gd-ngaythemsuc" name="ngayThemSuc" type="date" defaultValue={p.ngayThemSuc ?? ''} style={{ maxWidth: 170 }} />
            <GxInline>Số sổ</GxInline>
            <input aria-label="Số sổ thêm sức" name="soThemSuc" type="text" defaultValue={p.soThemSuc ?? ''} style={{ maxWidth: 150 }} />
          </GxField>
          <GxField label="Người ban bí tích" id="gd-chathemsuc"><GxPicker id="gd-chathemsuc" value={p.chaThemSuc} /></GxField>
          <GxField label="Người đỡ đầu" id="gd-dodauthemsuc">
            <input id="gd-dodauthemsuc" name="nguoiDoDauThemSuc" type="text" defaultValue={p.nguoiDoDauThemSuc ?? ''} />
          </GxField>
          <GxField label="Nơi thêm sức" id="gd-noithemsuc">
            <input id="gd-noithemsuc" name="noiThemSuc" type="text" defaultValue={p.noiThemSuc ?? ''} />
          </GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Xức dầu</h2></div>
          <GxField label="Ngày xức dầu" id="gd-ngayxucdau">
            <input id="gd-ngayxucdau" name="ngayXucDau" type="date" defaultValue={p.ngayXucDau ?? ''} style={{ maxWidth: 170 }} />
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

      <div className="card glass">
        <div className="card-head"><h2>Thông tin khác</h2></div>
        <GxField label="Trình độ văn hóa" id="gd-vanhoa">
          <input id="gd-vanhoa" name="trinhDoVanHoa" type="text" defaultValue={p.trinhDoVanHoa ?? ''} style={{ maxWidth: 180 }} />
          <GxInline>Chuyên môn</GxInline>
          <input aria-label="Chuyên môn" name="trinhDoChuyenMon" type="text" defaultValue={p.trinhDoChuyenMon ?? ''} />
        </GxField>
        <GxField label="Biết ngoại ngữ" id="gd-ngoaingu">
          <input id="gd-ngoaingu" name="bietNgoaiNgu" type="text" defaultValue={p.bietNgoaiNgu ?? ''} style={{ maxWidth: 180 }} />
        </GxField>
        <GxField label="Nghề nghiệp" id="gd-nghenghiep">
          <input id="gd-nghenghiep" name="ngheNghiep" type="text" defaultValue={p.ngheNghiep ?? ''} style={{ maxWidth: 200 }} />
          <GxInline>Điện thoại</GxInline>
          <input aria-label="Điện thoại" name="dienThoai" type="text" defaultValue={p.dienThoai ?? ''} style={{ maxWidth: 170 }} />
          <GxInline>Email</GxInline>
          <input aria-label="Email" name="email" type="text" defaultValue={p.email ?? ''} />
        </GxField>
        <GxField label="Địa chỉ" id="gd-diachi">
          <input id="gd-diachi" name="diaChi" type="text" defaultValue={p.diaChi ?? ''} />
        </GxField>
        <GxField label="">
          <label className="toggle">
            <input type="checkbox" name="quaDoi" checked={quaDoi} onChange={(e) => doiQuaDoi(e.target.checked)} />
            Qua đời
          </label>
          <label className="toggle">
            <input type="checkbox" name="conHoc" checked={conHoc} onChange={(e) => doiConHoc(e.target.checked)} />
            Còn học
          </label>
          <label className="toggle">
            <input type="checkbox" name="tanTong" defaultChecked={p.tanTong} />
            Tân tòng
          </label>
          <label className="toggle">
            <input type="checkbox" name="daCoGiaDinh" defaultChecked={p.daCoGiaDinh} />
            Có gia đình
          </label>
        </GxField>
        {quaDoi && (
          <>
            <GxField label="Ngày qua đời" id="gd-ngayquadoi">
              <input id="gd-ngayquadoi" name="ngayQuaDoi" type="date" defaultValue={p.ngayQuaDoi ?? ''} style={{ maxWidth: 180 }} />
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
          <GxField label="Ngày kết thúc khóa học" id="gd-gl-bd1"><input id="gd-gl-bd1" type="date" style={{ maxWidth: 190 }} /></GxField>
          <GxField label="Tại giáo xứ" id="gd-gl-bd1-gx"><input id="gd-gl-bd1-gx" type="text" /></GxField>
        </div>
        <div className="card glass">
          <div className="card-head"><h2>Bao đồng 2</h2></div>
          <GxField label="Ngày rước lễ trọng thể" id="gd-gl-bd2"><input id="gd-gl-bd2" type="date" style={{ maxWidth: 190 }} /></GxField>
          <GxField label="Tại giáo xứ" id="gd-gl-bd2-gx"><input id="gd-gl-bd2-gx" type="text" /></GxField>
        </div>
      </div>
      <div className="card glass">
        <div className="card-head"><h2>Vào đời</h2></div>
        <GxField label="Ngày tuyên hứa" id="gd-gl-vd"><input id="gd-gl-vd" type="date" style={{ maxWidth: 190 }} /></GxField>
        <GxField label="Tại giáo xứ" id="gd-gl-vd-gx"><input id="gd-gl-vd-gx" type="text" /></GxField>
      </div>
      <div className="card glass">
        <div className="card-head"><h2>Hôn nhân</h2></div>
        <GxField label="Khóa học từ ngày" id="gd-gl-hn-tu">
          <input id="gd-gl-hn-tu" type="date" style={{ maxWidth: 180 }} />
          <GxInline>đến ngày</GxInline>
          <input aria-label="Khóa học đến ngày" type="date" style={{ maxWidth: 180 }} />
        </GxField>
        <GxField label="Tại giáo xứ" id="gd-gl-hn-gx"><input id="gd-gl-hn-gx" type="text" /></GxField>
        <GxField label="Người cấp chứng nhận" id="gd-gl-hn-nguoicap"><GxPicker id="gd-gl-hn-nguoicap" /></GxField>
      </div>
    </>
  )

  const tabHonPhoi = (
    <div className="card glass">
      <div className="card-head"><h2>Thông tin đôi hôn phối</h2></div>
      <div className="card-row">
        <div>
          <GxField label="Người nam" id="gd-hp-nam"><GxPicker id="gd-hp-nam" value={p.phai === 'Nam' ? `${p.tenThanh ?? ''} ${p.hoTen}`.trim() : null} /></GxField>
          <GxField label="Người nữ" id="gd-hp-nu"><GxPicker id="gd-hp-nu" value={p.phai === 'Nữ' ? `${p.tenThanh ?? ''} ${p.hoTen}`.trim() : null} /></GxField>
          <GxField label="Đôi hôn phối" id="gd-hp-doi"><input id="gd-hp-doi" type="text" defaultValue={p.tenGiaDinh ?? ''} disabled /></GxField>
        </div>
        <div>
          <GxField label="Nơi hôn phối" id="gd-hp-noi"><input id="gd-hp-noi" type="text" /></GxField>
          <GxField label="Linh mục chứng" id="gd-hp-lm"><GxPicker id="gd-hp-lm" /></GxField>
        </div>
      </div>
    </div>
  )

  const tabOnGoi = (
    <div className="card glass">
      <div className="card-head"><h2>Thông tin ơn gọi</h2></div>
      <div className="card-row">
        <div>
          <GxField label="Ngày nhập dòng" id="gd-og-nhapdong"><input id="gd-og-nhapdong" type="date" style={{ maxWidth: 190 }} /></GxField>
          <GxField label="Ngày vào nhà tập" id="gd-og-nhatap"><input id="gd-og-nhatap" type="date" style={{ maxWidth: 190 }} /></GxField>
        </div>
        <div>
          <GxField label="Ngày vào ĐCV" id="gd-og-dcv"><input id="gd-og-dcv" type="date" style={{ maxWidth: 190 }} /></GxField>
          <GxField label="Ngày khấn lần đầu" id="gd-og-khan1"><input id="gd-og-khan1" type="date" style={{ maxWidth: 190 }} /></GxField>
        </div>
      </div>
      <GxField label="Dòng tu / chủng viện" id="gd-og-dong"><input id="gd-og-dong" type="text" /></GxField>
    </div>
  )

  const tabHoiDoan = (
    <div className="card glass">
      <div className="card-head"><h2>Thêm vào hội đoàn</h2></div>
      <GxField label="Tên hội đoàn" id="gd-hd-ten"><input id="gd-hd-ten" type="text" style={{ maxWidth: 320 }} /></GxField>
      <GxField label="Ngày vào hội đoàn" id="gd-hd-vao"><input id="gd-hd-vao" type="date" style={{ maxWidth: 190 }} /></GxField>
      <GxField label="Ngày ra hội đoàn" id="gd-hd-ra"><input id="gd-hd-ra" type="date" style={{ maxWidth: 190 }} /></GxField>
    </div>
  )

  // Dựng payload từ form (input không kiểm soát — defaultValue) rồi giao cho container qua
  // onLuu. Vài trường (tên cha/mẹ, người ban bí tích…) chỉ hiển thị qua GxPicker — chưa có ô
  // nhập thật ở Phase 1 nên giữ nguyên giá trị đã tải thay vì đọc từ DOM.
  function xuLySubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!onLuu || !formRef.current) return
    const fd = new FormData(formRef.current)
    const chuoi = (ten: string) => (fd.get(ten) as string | null)?.trim() || null

    onLuu({
      hoTen: chuoi('hoTen') ?? p.hoTen,
      tenThanh: chuoi('tenThanh'),
      phai: chuoi('phai'),
      ngaySinh: chuoi('ngaySinh'),
      noiSinh: chuoi('noiSinh'),
      cmnd: chuoi('cmnd'),
      danToc: p.danToc,
      giaoHoId: p.giaoHoId,
      diaChi: chuoi('diaChi'),
      dienThoai: chuoi('dienThoai'),
      email: chuoi('email'),
      hoTenCha: p.hoTenCha,
      hoTenMe: p.hoTenMe,
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
      rowVersion: p.rowVersion,
    })
  }

  return (
    <form className="page detail-page" ref={formRef} onSubmit={xuLySubmit}>
      <div className="page-head detail-head">
        <button type="button" className="btn btn-sm btn-quiet" onClick={() => moDanhSachGiaoDan?.()}>
          ← Danh sách
        </button>
        <h1>{tenDayDu}</h1>
        <span className="head-sub">
          {moi ? 'Chưa lưu · nhập thông tin rồi bấm Cập nhật'
            : `${p.maGiaoDanCu} · ${ngoaiXu ? NGOAI_XU : giaoHo}${p.ngaySinh ? ' · sinh ' + p.ngaySinh : ''}`}
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
        <span className="hint" role={thongBaoLuu ? 'status' : undefined}>
          {thongBaoLuu ?? (moi ? 'Chưa hỗ trợ tạo mới giáo dân qua web ở giai đoạn này' : 'Thay đổi chưa được lưu')}
        </span>
        <div className="spacer" />
        <button type="button" className="btn" disabled={!p.giaDinhId} onClick={() => p.giaDinhId && moGiaDinh?.(p.giaDinhId)}>
          Xem gia đình
        </button>
        <button type="button" className="btn">In lý lịch cá nhân</button>
        <button type="button" className="btn btn-quiet" onClick={() => moDanhSachGiaoDan?.()}>Quay về</button>
        <button type="submit" className="btn btn-primary" disabled={moi || !onLuu || dangLuu}>
          {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
        </button>
      </div>
    </form>
  )
}
