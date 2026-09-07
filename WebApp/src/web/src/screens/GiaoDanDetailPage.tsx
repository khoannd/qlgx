import { useCallback, useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type {
  GiaoDanDetail as GiaoDanDetailDuLieu, GiaoHo, HoiDoanCuaGiaoDan, HoiDoanDanhMuc,
  HonPhoiCuaGiaoDan, TanHienCuaGiaoDan,
} from '../api/types'
import { BanNhapBanner } from '../components/BanNhapBanner'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { banNhapKhoa, docBanNhap, xoaBanNhap } from '../lib/banNhap'
import {
  GiaoDanDetail, type YeuCauCapNhatGiaoDan, type YeuCauCapNhatHoiDoan, type YeuCauCapNhatHonPhoi,
  type YeuCauLuuTanHien, type YeuCauThemHoiDoan,
} from './GiaoDanDetail'

type Props = {
  /** `null` = bản ghi mới — form trống, nút "Thêm giáo dân" gọi `POST /api/giao-dan` (xem
   * hàm `tao` bên dưới). */
  id: string | null
  moGiaDinh?: (id: string) => void
  moDanhSachGiaoDan?: () => void
  /** Mở (một thẻ tài liệu mới cho) chi tiết giáo dân theo id — dùng để chuyển từ thẻ "nháp"
   * sang thẻ thật ngay sau khi tạo mới thành công. */
  moGiaoDan?: (id: string | null) => void
  /** Tên tài khoản đang đăng nhập — App.tsx truyền xuống từ `useAuth()` (không gọi thẳng
   * `useAuth()` ở đây để component này không bắt buộc phải render trong `<AuthProvider>`, giữ
   * nguyên khả năng test độc lập của các bài test hiện có). Dùng để khoá bản nháp ngoại tuyến
   * theo tài khoản (Task 16, xem `lib/banNhap.ts`) — không truyền = tắt tính năng bản nháp. */
  tenTaiKhoan?: string | null
}

/** Container nối `GiaoDanDetail` với `GET`/`PUT /api/giao-dan/{id}` — cùng khuôn tải lại sau
 * khi lưu và xử lý xung đột RowVersion như `GiaDinhDetailPage`. */
export function GiaoDanDetailPage({ id, moGiaDinh, moDanhSachGiaoDan, moGiaoDan, tenTaiKhoan = null }: Props) {
  // Bản nháp ngoại tuyến (Task 16, xem lib/banNhap.ts) — khoá cố định cho suốt vòng đời thẻ
  // này (không phụ thuộc dữ liệu đã tải), gắn kèm tài khoản đang đăng nhập để không lẫn nháp
  // giữa hai người dùng chung một trình duyệt.
  const khoaBanNhap = tenTaiKhoan ? banNhapKhoa('giaoDan', id, tenTaiKhoan) : null
  const [banNhapCho, setBanNhapCho] = useState<{ duLieu: YeuCauCapNhatGiaoDan; thoiDiem: string } | null>(null)
  const [banNhapApDung, setBanNhapApDung] = useState<YeuCauCapNhatGiaoDan | null>(null)
  const [remountKey, setRemountKey] = useState(0)

  // Kiểm tra một lần khi mở thẻ này (đổi tài khoản/id) — có bản nháp cũ nằm sẵn thì hỏi trước
  // khi hiện form, KHÔNG tự động đè lên dữ liệu vừa/sắp tải từ máy chủ.
  useEffect(() => {
    setBanNhapApDung(null)
    if (!khoaBanNhap || !tenTaiKhoan) { setBanNhapCho(null); return }
    setBanNhapCho(docBanNhap<YeuCauCapNhatGiaoDan>(khoaBanNhap, tenTaiKhoan))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [khoaBanNhap, tenTaiKhoan])

  function khoiPhucBanNhap() {
    if (!banNhapCho) return
    setBanNhapApDung(banNhapCho.duLieu)
    setRemountKey((k) => k + 1)
    setBanNhapCho(null)
  }

  function boQuaBanNhap() {
    if (khoaBanNhap) xoaBanNhap(khoaBanNhap)
    setBanNhapCho(null)
  }

  const [duLieu, setDuLieu] = useState<GiaoDanDetailDuLieu | null>(null)
  const [dangTai, setDangTai] = useState(id !== null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [dangIn, setDangIn] = useState(false)
  const [thongBaoLuu, setThongBaoLuu] = useState<string | null>(null)
  // Màu của `thongBaoLuu` ở thanh lệnh cuối form — xem GiaoDanDetail.Props.loaiThongBao.
  const [loaiThongBao, setLoaiThongBao] = useState<'thanhcong' | 'canhbao' | 'loi' | null>(null)
  const [honPhoi, setHonPhoi] = useState<HonPhoiCuaGiaoDan[]>([])
  const [dangTaiHonPhoi, setDangTaiHonPhoi] = useState(id !== null)
  const [tanHien, setTanHien] = useState<TanHienCuaGiaoDan[]>([])
  const [dangTaiTanHien, setDangTaiTanHien] = useState(id !== null)
  const [hoiDoan, setHoiDoan] = useState<HoiDoanCuaGiaoDan[]>([])
  const [dangTaiHoiDoan, setDangTaiHoiDoan] = useState(id !== null)
  const [danhMucHoiDoan, setDanhMucHoiDoan] = useState<HoiDoanDanhMuc[]>([])
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])

  const tai = useCallback(() => {
    if (id === null) return
    setDangTai(true)
    setLoi(null)
    api.giaoDan.chiTiet(id)
      .then((ct) => setDuLieu(ct))
      .catch((e: unknown) => {
        console.error(`Không tải được chi tiết giáo dân ${id}`, e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [id])

  useEffect(tai, [tai])

  // Tải riêng, độc lập với `tai()` — tab "Hôn phối" lưu qua endpoint riêng
  // (`PUT /api/giao-dan/hon-phoi/{id}`), không đi qua nút "Cập nhật" của form giáo dân chính.
  const taiHonPhoi = useCallback(() => {
    if (id === null) return
    setDangTaiHonPhoi(true)
    api.giaoDan.honPhoi(id)
      .then(setHonPhoi)
      .catch((e: unknown) => {
        console.error(`Không tải được danh sách hôn phối của giáo dân ${id}`, e)
      })
      .finally(() => setDangTaiHonPhoi(false))
  }, [id])

  useEffect(taiHonPhoi, [taiHonPhoi])

  // Tải riêng, độc lập — tab "Ơn gọi tận hiến" lưu qua endpoint riêng
  // (`POST`/`PUT /api/giao-dan/(id/)tan-hien(/{id})`), cùng khuôn với hôn phối ở trên.
  const taiTanHien = useCallback(() => {
    if (id === null) return
    setDangTaiTanHien(true)
    api.giaoDan.tanHien(id)
      .then(setTanHien)
      .catch((e: unknown) => {
        console.error(`Không tải được thông tin ơn gọi tận hiến của giáo dân ${id}`, e)
      })
      .finally(() => setDangTaiTanHien(false))
  }, [id])

  useEffect(taiTanHien, [taiTanHien])

  // Tải riêng, độc lập — tab "Hội đoàn" lưu qua endpoint riêng
  // (`POST`/`PUT /api/giao-dan/(id/)hoi-doan(/{id})`).
  const taiHoiDoan = useCallback(() => {
    if (id === null) return
    setDangTaiHoiDoan(true)
    api.giaoDan.hoiDoan(id)
      .then(setHoiDoan)
      .catch((e: unknown) => {
        console.error(`Không tải được danh sách hội đoàn của giáo dân ${id}`, e)
      })
      .finally(() => setDangTaiHoiDoan(false))
  }, [id])

  useEffect(taiHoiDoan, [taiHoiDoan])

  // Danh mục hội đoàn (combo "Tên hội đoàn" khi thêm mới) — không phụ thuộc `id`, tải một lần.
  useEffect(() => {
    api.hoiDoan.danhMuc()
      .then(setDanhMucHoiDoan)
      .catch((e: unknown) => { console.error('Không tải được danh mục hội đoàn', e) })
  }, [])

  // Danh mục Giáo họ thật (combo "Giáo họ") — không phụ thuộc `id`, tải một lần.
  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  // Gộp MỌI cảnh báo nghiệp vụ áp dụng được (xem TaoGiaoDanRequest.BoQuaCanhBao phía backend)
  // thành MỘT hộp thoại xác nhận, thay vì chuỗi hộp thoại Yes/No tuần tự của desktop — cùng
  // tinh thần "chặn tới khi được xác nhận rõ ràng", chỉ khác cách trình bày. Trả về `true` nếu
  // đã lưu thành công (hoặc không có gì cần lưu thêm), `false` nếu người dùng huỷ.
  function xacNhanCanhBao(canhBao: string[]): boolean {
    // Một vài cảnh báo (rule 9/10/12 của KiemTraNghiepVu) đã tự kết bằng đúng câu hỏi xác nhận
    // này — chỉ nối thêm khi nội dung CHƯA kết bằng câu đó, tránh hỏi lặp lại hai lần trong
    // cùng một hộp thoại (phát hiện khi kiểm thử khám phá 2026-09-07).
    const hoiXacNhan = 'Bạn có chắc muốn lưu thông tin giáo dân này không?'
    const noiDung = canhBao.join('\n\n')
    return window.confirm(noiDung.endsWith(hoiXacNhan) ? noiDung : noiDung + '\n\n' + hoiXacNhan)
  }

  if (id === null) {
    async function tao(payload: YeuCauCapNhatGiaoDan) {
      setDangLuu(true)
      setThongBaoLuu(null)
      setLoaiThongBao(null)
      try {
        let ket = await api.giaoDan.taoMoi(payload)
        if (!ket.id && ket.canhBao.length > 0) {
          if (!xacNhanCanhBao(ket.canhBao)) {
            setThongBaoLuu('Đã hủy — chưa lưu giáo dân này.')
            setLoaiThongBao('canhbao')
            return
          }
          ket = await api.giaoDan.taoMoi({ ...payload, boQuaCanhBao: true })
        }
        if (ket.id) {
          setThongBaoLuu('Đã tạo giáo dân mới.')
          setLoaiThongBao('thanhcong')
          moGiaoDan?.(ket.id)
        }
      } catch (e) {
        console.error('Không tạo được giáo dân mới', e)
        setThongBaoLuu(e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
        setLoaiThongBao('loi')
      } finally {
        setDangLuu(false)
      }
    }

    return (
      <>
        {banNhapCho && <BanNhapBanner thoiDiem={banNhapCho.thoiDiem} onKhoiPhuc={khoiPhucBanNhap} onBoQua={boQuaBanNhap} />}
        <GiaoDanDetail
          key={remountKey}
          moGiaDinh={moGiaDinh}
          moDanhSachGiaoDan={moDanhSachGiaoDan}
          onLuu={tao}
          dangLuu={dangLuu}
          thongBaoLuu={thongBaoLuu}
          loaiThongBao={loaiThongBao}
          danhMucGiaoHo={danhMucGiaoHo}
          khoaBanNhap={khoaBanNhap}
          tenTaiKhoan={tenTaiKhoan}
          banNhap={banNhapApDung}
        />
      </>
    )
  }

  async function luu(payload: YeuCauCapNhatGiaoDan) {
    setDangLuu(true)
    setThongBaoLuu(null)
    setLoaiThongBao(null)
    try {
      let ket = await api.giaoDan.capNhat(id as string, payload)
      if (!ket.id && ket.canhBao.length > 0) {
        if (!xacNhanCanhBao(ket.canhBao)) {
          setThongBaoLuu('Đã hủy — thay đổi chưa được lưu.')
          setLoaiThongBao('canhbao')
          return
        }
        ket = await api.giaoDan.capNhat(id as string, { ...payload, boQuaCanhBao: true })
      }
      setThongBaoLuu('Đã lưu thành công.')
      setLoaiThongBao('thanhcong')
      tai()
    } catch (e) {
      if (e instanceof LoiXungDot) {
        setThongBaoLuu(e.message)
      } else {
        console.error(`Không lưu được giáo dân ${id}`, e)
        setThongBaoLuu(e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
      }
      setLoaiThongBao('loi')
    } finally {
      setDangLuu(false)
    }
  }

  // KhoiHonPhoi (trong GiaoDanDetail) tự quản lý trạng thái "đang lưu"/thông báo của riêng nó
  // — hàm này chỉ gọi API thật rồi tải lại danh sách; lỗi (kể cả LoiXungDot) được ném lại
  // nguyên vẹn để khối hôn phối tự hiển thị đúng thông báo.
  async function luuHonPhoi(honPhoiId: string, payload: YeuCauCapNhatHonPhoi) {
    await api.giaoDan.capNhatHonPhoi(honPhoiId, payload)
    taiHonPhoi()
  }

  // Cùng khuôn với hôn phối: khối con tự quản lý trạng thái "đang lưu"/thông báo, hàm này chỉ
  // gọi API thật rồi tải lại danh sách; lỗi (kể cả LoiXungDot) ném lại nguyên vẹn.
  async function luuTanHien(tanHienId: string, payload: YeuCauLuuTanHien) {
    await api.giaoDan.capNhatTanHien(tanHienId, payload)
    taiTanHien()
  }

  async function themTanHien(payload: YeuCauLuuTanHien) {
    await api.giaoDan.themTanHien(id as string, payload)
    taiTanHien()
  }

  async function luuHoiDoan(chiTietId: string, payload: YeuCauCapNhatHoiDoan) {
    await api.giaoDan.capNhatHoiDoan(chiTietId, payload)
    taiHoiDoan()
  }

  async function themHoiDoan(payload: YeuCauThemHoiDoan) {
    await api.giaoDan.themHoiDoan(id as string, payload)
    taiHoiDoan()
  }

  // In "Lý lịch cá nhân" (VIEC-TIEP-THEO.md mục 1.1) — tải PDF thẳng về máy, KHÔNG đi qua
  // luồng `thongBaoLuu`/`loaiThongBao` của form (đó là cho lưu dữ liệu, không phải in). Lỗi
  // hiện bằng alert() cho đơn giản — cùng mức thô sơ với `chuaHoTro()` mà các mục menu chưa
  // làm khác đang dùng, tránh thêm một kênh thông báo mới chỉ cho một nút.
  async function inLyLichCaNhan() {
    setDangIn(true)
    try {
      await api.giaoDan.inLyLichCaNhan(id as string)
    } catch (e) {
      console.error(`Không in được lý lịch cá nhân của giáo dân ${id}`, e)
      window.alert(e instanceof Error ? e.message : 'In thất bại, thử lại sau.')
    } finally {
      setDangIn(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      {banNhapCho && <BanNhapBanner thoiDiem={banNhapCho.thoiDiem} onKhoiPhuc={khoiPhucBanNhap} onBoQua={boQuaBanNhap} />}
      {duLieu && (
        <GiaoDanDetail
          key={remountKey}
          duLieu={duLieu}
          moGiaDinh={moGiaDinh}
          moDanhSachGiaoDan={moDanhSachGiaoDan}
          onLuu={luu}
          dangLuu={dangLuu}
          thongBaoLuu={thongBaoLuu}
          loaiThongBao={loaiThongBao}
          danhSachHonPhoi={honPhoi}
          dangTaiHonPhoi={dangTaiHonPhoi}
          onLuuHonPhoi={luuHonPhoi}
          danhSachTanHien={tanHien}
          dangTaiTanHien={dangTaiTanHien}
          onLuuTanHien={luuTanHien}
          onThemTanHien={themTanHien}
          danhSachHoiDoan={hoiDoan}
          dangTaiHoiDoan={dangTaiHoiDoan}
          onLuuHoiDoan={luuHoiDoan}
          onThemHoiDoan={themHoiDoan}
          danhMucHoiDoan={danhMucHoiDoan}
          danhMucGiaoHo={danhMucGiaoHo}
          khoaBanNhap={khoaBanNhap}
          tenTaiKhoan={tenTaiKhoan}
          banNhap={banNhapApDung}
          onIn={inLyLichCaNhan}
          dangIn={dangIn}
          onLayAnh={api.giaoDan.layAnh}
          onTaiAnhLen={api.giaoDan.taiAnhLen}
          onXoaAnh={api.giaoDan.xoaAnh}
        />
      )}
    </TrangThaiTai>
  )
}
