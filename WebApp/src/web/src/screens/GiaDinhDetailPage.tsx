import { useCallback, useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaDinhDetail as GiaDinhDetailDuLieu } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaDinhDetail, type YeuCauCapNhatGiaDinh } from './GiaDinhDetail'

type Props = {
  /** `null` = bản ghi mới (chưa có API tạo mới, xem GiaDinhDetail). */
  id: string | null
  moGiaoDan?: (id: string) => void
  moDanhSachGiaDinh?: () => void
}

/**
 * Container nối `GiaDinhDetail` với `GET`/`PUT /api/gia-dinh/{id}`. Sau khi lưu thành công
 * tải lại chi tiết từ máy chủ (không tự cập nhật state bằng payload đã gửi) để RowVersion mới
 * và mọi giá trị hiển thị đều đến từ nguồn sự thật là CSDL — đúng yêu cầu "lưu và tải lại
 * được". Khi máy chủ báo xung đột RowVersion (409), hiển thị nguyên văn thông báo tiếng Việt
 * của backend thay vì âm thầm ghi đè hay mất dữ liệu người dùng vừa gõ.
 */
export function GiaDinhDetailPage({ id, moGiaoDan, moDanhSachGiaDinh }: Props) {
  const [duLieu, setDuLieu] = useState<GiaDinhDetailDuLieu | null>(null)
  const [dangTai, setDangTai] = useState(id !== null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBaoLuu, setThongBaoLuu] = useState<string | null>(null)

  const tai = useCallback(() => {
    if (id === null) return
    setDangTai(true)
    setLoi(null)
    api.giaDinh.chiTiet(id)
      .then((ct) => setDuLieu(ct))
      .catch((e: unknown) => {
        console.error(`Không tải được chi tiết gia đình ${id}`, e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [id])

  useEffect(tai, [tai])

  if (id === null) {
    return <GiaDinhDetail moGiaoDan={moGiaoDan} moDanhSachGiaDinh={moDanhSachGiaDinh} />
  }

  async function luu(payload: YeuCauCapNhatGiaDinh) {
    setDangLuu(true)
    setThongBaoLuu(null)
    try {
      await api.giaDinh.capNhat(id as string, payload)
      setThongBaoLuu('Đã lưu thành công.')
      tai()
    } catch (e) {
      if (e instanceof LoiXungDot) {
        setThongBaoLuu(e.message)
      } else {
        console.error(`Không lưu được gia đình ${id}`, e)
        setThongBaoLuu(e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
      }
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      {duLieu && (
        <GiaDinhDetail
          duLieu={duLieu}
          moGiaoDan={moGiaoDan}
          moDanhSachGiaDinh={moDanhSachGiaDinh}
          onLuu={luu}
          dangLuu={dangLuu}
          thongBaoLuu={thongBaoLuu}
        />
      )}
    </TrangThaiTai>
  )
}
