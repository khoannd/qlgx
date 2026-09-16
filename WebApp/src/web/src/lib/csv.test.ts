import { afterEach, describe, expect, it, vi } from 'vitest'
import { taiXuongCsv } from './csv'

describe('taiXuongCsv', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('them BOM UTF-8 vao dau noi dung de Excel khong vo chu tieng Viet, va dat dung ten tep', async () => {
    let blobDaTao: Blob | undefined
    let tenTepDaTai: string | undefined
    const click = vi.fn()

    // URL.createObjectURL nhan Blob | MediaSource, nen mock phai khai bao dung kieu rong
    // do roi thu hep lai — khai bao hep thang thanh Blob se lam `tsc -b` (va `npm run build`) do.
    vi.spyOn(URL, 'createObjectURL').mockImplementation((obj: Blob | MediaSource) => {
      blobDaTao = obj as Blob
      return 'blob:gia-lap'
    })
    const thuHoi = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    const guiA = document.createElement.bind(document)
    vi.spyOn(document, 'createElement').mockImplementation((tag: string) => {
      const el = guiA(tag) as HTMLAnchorElement
      if (tag === 'a') {
        el.click = click
        Object.defineProperty(el, 'download', { get: () => tenTepDaTai, set: (v) => { tenTepDaTai = v } })
      }
      return el
    })

    taiXuongCsv('Họ tên,Giáo họ\nNguyễn Văn A,Thánh Tâm\n', 'danh-sach-giao-dan-2026-09-07.csv')

    expect(click).toHaveBeenCalledOnce()
    expect(tenTepDaTai).toBe('danh-sach-giao-dan-2026-09-07.csv')
    expect(thuHoi).toHaveBeenCalledWith('blob:gia-lap')
    expect(blobDaTao).toBeInstanceOf(Blob)

    // `Blob.text()` tự giải mã UTF-8 và ÂM THẦM bỏ BOM đầu chuỗi (đúng đặc tả TextDecoder,
    // ignoreBOM mặc định false nghĩa là "bỏ BOM khỏi kết quả" chứ không phải "giữ nguyên") —
    // nên phải đọc byte thô qua arrayBuffer() mới thấy đúng 3 byte EF BB BF đã được thêm vào.
    const bytes = new Uint8Array(await blobDaTao!.arrayBuffer())
    expect([bytes[0], bytes[1], bytes[2]]).toEqual([0xef, 0xbb, 0xbf])

    const noiDung = await blobDaTao!.text()
    expect(noiDung).toContain('Nguyễn Văn A')
  })
})
