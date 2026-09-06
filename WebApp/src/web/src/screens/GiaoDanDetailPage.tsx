import { useCallback, useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanDetail as GiaoDanDetailDuLieu, HonPhoiCuaGiaoDan } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaoDanDetail, type YeuCauCapNhatGiaoDan, type YeuCauCapNhatHonPhoi } from './GiaoDanDetail'

type Props = {
  /** `null` = bản ghi mới (chưa có API tạo mới, xem GiaoDanDetail). */
  id: string | null
  moGiaDinh?: (id: string) => void
  moDanhSachGiaoDan?: () => void
}

/** Container nối `GiaoDanDetail` với `GET`/`PUT /api/giao-dan/{id}` — cùng khuôn tải lại sau
 * khi lưu và xử lý xung đột RowVersion như `GiaDinhDetailPage`. */
export function GiaoDanDetailPage({ id, moGiaDinh, moDanhSachGiaoDan }: Props) {
  const [duLieu, setDuLieu] = useState<GiaoDanDetailDuLieu | null>(null)
  const [dangTai, setDangTai] = useState(id !== null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBaoLuu, setThongBaoLuu] = useState<string | null>(null)
  const [honPhoi, setHonPhoi] = useState<HonPhoiCuaGiaoDan[]>([])
  const [dangTaiHonPhoi, setDangTaiHonPhoi] = useState(id !== null)

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

  if (id === null) {
    return <GiaoDanDetail moGiaDinh={moGiaDinh} moDanhSachGiaoDan={moDanhSachGiaoDan} />
  }

  async function luu(payload: YeuCauCapNhatGiaoDan) {
    setDangLuu(true)
    setThongBaoLuu(null)
    try {
      await api.giaoDan.capNhat(id as string, payload)
      setThongBaoLuu('Đã lưu thành công.')
      tai()
    } catch (e) {
      if (e instanceof LoiXungDot) {
        setThongBaoLuu(e.message)
      } else {
        console.error(`Không lưu được giáo dân ${id}`, e)
        setThongBaoLuu(e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
      }
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

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      {duLieu && (
        <GiaoDanDetail
          duLieu={duLieu}
          moGiaDinh={moGiaDinh}
          moDanhSachGiaoDan={moDanhSachGiaoDan}
          onLuu={luu}
          dangLuu={dangLuu}
          thongBaoLuu={thongBaoLuu}
          danhSachHonPhoi={honPhoi}
          dangTaiHonPhoi={dangTaiHonPhoi}
          onLuuHonPhoi={luuHonPhoi}
        />
      )}
    </TrangThaiTai>
  )
}
