/// <reference types="node" />
import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import { describe, expect, it } from 'vitest'
import { catMicroGiay, dungDauDongHo, mocSangMs, nangDau, soSanh, soSanhGuid, tuMocMs, type DauDongHo } from './dauDongHo'

/**
 * Vector kiểm thử DÙNG CHUNG với `WebApp/tests/Qlgx.Data.Tests/DongHoLaiTests.cs` — cả hai bộ test
 * (TS ở đây, C# ở đó) đọc CÙNG một file, để không viết hai bộ ca kiểm thử độc lập rồi mới cố khớp
 * (xem `task-4-brief.md`). File nằm ở gốc `WebApp/`, không phải trong `src/web/`.
 *
 * Dựng đường dẫn từ `process.cwd()` (không phải `import.meta.url`): dưới vite/vitest,
 * `import.meta.url` bị biến đổi thành một URL ảo `http://localhost:.../@fs/...` khi dùng làm tham
 * số thứ hai của `new URL()`, khiến `fileURLToPath` ném lỗi "The URL must be of scheme file" — đã
 * kiểm chứng thật bằng cách chạy thử, không đoán. `process.cwd()` khi chạy `npx vitest run` từ
 * `WebApp/src/web` (đúng thư mục brief yêu cầu) luôn là chính thư mục đó.
 */
const DUONG_DAN_VECTOR = join(process.cwd(), '..', '..', 'dong-ho-lai-vector.json')

type CaSoSanh = { ham: 'SoSanh'; vao: { a: DauDongHo; b: DauDongHo }; ra: { dau: number } }
type CaSoSanhGuid = { ham: 'SoSanhGuid'; vao: { a: string | null; b: string | null }; ra: { dau: number } }
type CaNangDau = {
  ham: 'NangDau'
  vao: { dauCuoiCuaTa: DauDongHo; nhanDuoc: DauDongHo | null; gioHienTai: string }
  ra: { vatLy: string; logic: number }
}
type CaKiemThu = CaSoSanh | CaSoSanhGuid | CaNangDau

function docVector(): CaKiemThu[] {
  return JSON.parse(readFileSync(DUONG_DAN_VECTOR, 'utf-8')) as CaKiemThu[]
}

describe('dauDongHo (Task 4 Phần A — port DongHoLai.cs) — vector dùng chung với C#', () => {
  const vector = docVector()

  it('vector co du toi thieu 15 ca, bao du ca ham', () => {
    expect(vector.length).toBeGreaterThanOrEqual(15)
    expect(vector.some((c) => c.ham === 'SoSanh')).toBe(true)
    expect(vector.some((c) => c.ham === 'SoSanhGuid')).toBe(true)
    expect(vector.some((c) => c.ham === 'NangDau')).toBe(true)
  })

  for (const [i, ca] of docVector().entries()) {
    it(`ca #${i} (${ca.ham})${'_ghiChu' in ca ? `: ${(ca as unknown as Record<string, string>)._ghiChu}` : ''}`, () => {
      if (ca.ham === 'SoSanh') {
        expect(Math.sign(soSanh(ca.vao.a, ca.vao.b))).toBe(ca.ra.dau)
      } else if (ca.ham === 'SoSanhGuid') {
        expect(Math.sign(soSanhGuid(ca.vao.a, ca.vao.b))).toBe(ca.ra.dau)
      } else {
        const phat = nangDau(ca.vao.dauCuoiCuaTa, ca.vao.nhanDuoc, ca.vao.gioHienTai)
        expect(phat.vatLy).toBe(ca.ra.vatLy)
        expect(phat.logic).toBe(ca.ra.logic)
      }
    })
  }
})

describe('so sánh chuỗi ISO cùng độ dài cho đúng thứ tự thời gian (giả định nền tảng của toàn bộ file)', () => {
  it('so chuoi tu dien tren dinh dang co dinh khop voi so thoi gian thuc', () => {
    // Bọc mọi cặp qua một hàm nhận tham số (thay vì so hai chuỗi literal trực tiếp) — ngoài việc rõ
    // ràng hơn, còn tránh cảnh báo `no-constant-binary-expression` của oxlint (nó coi so hai chuỗi
    // literal là biểu thức "hằng số" vô nghĩa, dù ở đây chính là điều ta CHỦ ĐỘNG muốn kiểm chứng).
    const soChuoi = (a: string, b: string): boolean => a < b

    const moc = (ngay: string) => `2026-09-${ngay}T10:00:00.000000Z`
    expect(soChuoi(moc('01'), moc('02'))).toBe(true)
    expect(soChuoi(moc('09'), moc('10'))).toBe(true) // qua bien 1 chu so -> 2 chu so ngay
    expect(soChuoi('2026-01-01T00:00:00.000000Z', '2026-01-01T00:00:00.000001Z')).toBe(true)
    expect(soChuoi('2026-01-01T23:59:59.999999Z', '2027-01-01T00:00:00.000000Z')).toBe(true) // qua bien nam
    expect(soChuoi('2026-09-13T09:59:59.000000Z', '2026-09-13T10:00:00.000000Z')).toBe(true) // qua bien gio
  })

  it('hai chuoi khac do dai (thieu chu so) se so SAI - vi sao phai luon co dinh 6 chu so', () => {
    // Minh hoạ vì sao catMicroGiay/tuMocMs PHẢI luôn trả đủ 6 chữ số: nếu lỡ để lọt một chuỗi
    // ngắn hơn (thiếu chữ số ở CUỐI thay vì đệm thêm số 0), so từ điển có thể cho kết quả SAI dù
    // giá trị thời gian thực đúng ra nhỏ hơn.
    const ngan = '2026-09-13T10:00:00.4Z' // 0.4 giay, thieu chu so, viet tay de minh hoa
    const day = '2026-09-13T10:00:00.450000Z' // 0.45 giay - LON HON ngan ve gia tri thuc
    // Ky tu ngay sau "00.4" cua "ngan" la 'Z' (ma 0x5A), con cua "day" la '5' (ma 0x35). Vi 'Z' >
    // '5' theo ma UTF-16, "ngan" so ra LON HON "day" du gia tri thoi gian thuc NHO HON - day chinh
    // la ly do catMicroGiay/tuMocMs bat buoc luon dem du 6 chu so, khong duoc de sot mot chuoi
    // ngan hon lot vao he thong.
    expect(ngan > day).toBe(true)
  })
})

describe('catMicroGiay/tuMocMs/mocSangMs', () => {
  it('cat bo phan le duoi micro giay, khong lam tron', () => {
    expect(catMicroGiay('2026-09-13T10:00:00.1237890Z')).toBe('2026-09-13T10:00:00.123789Z')
  })

  it('CAT (khong LAM TRON) - chu so thu 7 >= 5 van khong duoc lam vatLy tang len', () => {
    // Chu so thu 7 la '6' (>=5): neu lam tron thay vi cat, ket qua se la "...123790Z" (tang len 1),
    // khac voi "...123789Z" (cat dung). Ca truoc dung chu so thu 7 la '0' nen khong phan biet duoc
    // hai each tiep can - ca nay moi thuc su bat duoc mutation "lam tron thay vi cat".
    expect(catMicroGiay('2026-09-13T10:00:00.1237896Z')).toBe('2026-09-13T10:00:00.123789Z')
  })

  it('dem them so 0 neu thieu chu so', () => {
    expect(catMicroGiay('2026-09-13T10:00:00.5Z')).toBe('2026-09-13T10:00:00.500000Z')
  })

  it('nem loi neu mot dinh dang khac (khong phai UTC co dinh)', () => {
    expect(() => catMicroGiay('2026-09-13T10:00:00.000+07:00')).toThrow()
    expect(() => catMicroGiay('2026-09-13T10:00:00Z')).toThrow()
  })

  it('tuMocMs luon tra ve dung 6 chu so, 3 chu so cuoi la 000 (JS chi co do phan giai mili giay)', () => {
    const ms = Date.UTC(2026, 8, 13, 10, 0, 0, 123)
    expect(tuMocMs(ms)).toBe('2026-09-13T10:00:00.123000Z')
  })

  it('mocSangMs la nghich dao (mat mat duoi mili giay) cua tuMocMs', () => {
    const ms = Date.UTC(2026, 8, 13, 10, 0, 0, 123)
    expect(mocSangMs(tuMocMs(ms))).toBe(ms)
  })

  it('dungDauDongHo tu dong cat vatLy giong constructor DauDongHo ben C#', () => {
    const dau = dungDauDongHo('2026-09-13T10:00:00.1237890Z', 0, null, '00000000-0000-0000-0000-000000000000')
    expect(dau.vatLy).toBe('2026-09-13T10:00:00.123789Z')
  })
})

describe('soSanh / soSanhGuid — cac bat bien khong duoc phep vi pham', () => {
  it('bat bien doi xung nghich va bang-nhau-la-that-su-bang-nhau tren mot mang mau', () => {
    const tb1 = '00000000-0000-0000-0000-000000000001'
    const tb2 = '00000000-0000-0000-0000-000000000002'
    const m1 = '00000000-0000-0000-0000-0000000000a1'
    const m2 = '00000000-0000-0000-0000-0000000000a2'
    const goc = '2026-09-13T10:00:00.000000Z'
    const mau: DauDongHo[] = [
      dungDauDongHo(goc, 0, null, m1),
      dungDauDongHo(goc, 0, tb1, m1),
      dungDauDongHo(goc, 0, tb1, m2),
      dungDauDongHo(goc, 0, tb2, m1),
      dungDauDongHo(goc, 1, tb1, m1),
      dungDauDongHo('2026-09-13T10:00:01.000000Z', 0, tb1, m1),
    ]

    for (const a of mau) {
      for (const b of mau) {
        // Cong hai dau bang 0 thay vi so truc tiep bang `toBe`: Math.sign(0) la +0, con
        // -Math.sign(0) la -0 - hai gia tri nay KHAC NHAU duoi Object.is (dung boi `toBe`) du ve
        // mat toan hoc chung bang nhau. Phep cong tranh duoc sai lech gia -0/+0 khong lien quan.
        expect(Math.sign(soSanh(a, b)) + Math.sign(soSanh(b, a))).toBe(0)
        const bangNhau = a.vatLy === b.vatLy && a.logic === b.logic && a.thietBiId === b.thietBiId && a.maThaoTac === b.maThaoTac
        expect(soSanh(a, b) === 0).toBe(bangNhau)
      }
    }
  })

  it('quy null ve GUID_RONG se phai bi coi la SAI - null va Guid rong KHONG duoc bang nhau', () => {
    const a = dungDauDongHo('2026-09-13T10:00:00.000000Z', 0, null, '00000000-0000-0000-0000-000000000000')
    const b = dungDauDongHo('2026-09-13T10:00:00.000000Z', 0, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000')
    expect(soSanh(a, b)).not.toBe(0)
  })
})

describe('nangDau — bat bien song con: khong duoc phat ra moc som hon moc dau vao', () => {
  it('fuzz voi moc le duoi micro giay: moc phat ra khong bao gio som hon dauCuoiCuaTa lan nhanDuoc', () => {
    // Tương đương tinh thần bài fuzz trong DongHoLaiTests.cs — không cần 20000 vòng như C# (không
    // có bẫy tick 100ns ở tầng TypeScript vì ta không tự sinh moc thô hơn micro giây), nhưng vẫn
    // kiểm chứng bất biến cốt lõi qua nhiều tổ hợp ngẫu nhiên nhỏ.
    let hat = 20260913
    const rnd = (): number => {
      hat = (hat * 1103515245 + 12345) & 0x7fffffff
      return hat / 0x7fffffff
    }
    const gocMs = Date.UTC(2026, 8, 13, 10, 0, 0, 0)

    for (let i = 0; i < 2000; i++) {
      const ta = dungDauDongHo(tuMocMs(gocMs + Math.floor(rnd() * 5000)), Math.floor(rnd() * 5), null, '00000000-0000-0000-0000-000000000000')
      const ho = dungDauDongHo(
        tuMocMs(gocMs + Math.floor(rnd() * 5000)),
        Math.floor(rnd() * 5),
        '00000000-0000-0000-0000-000000000002',
        '00000000-0000-0000-0000-000000000003',
      )
      const nay = tuMocMs(gocMs + Math.floor((rnd() - 0.5) * 5000))

      const phat = nangDau(ta, ho, nay)
      const dauPhat = dungDauDongHo(phat.vatLy, phat.logic, null, '00000000-0000-0000-0000-000000000000')

      expect(phat.vatLy >= ta.vatLy).toBe(true)
      expect(phat.vatLy >= ho.vatLy).toBe(true)
      expect(soSanh(dauPhat, ta)).toBeGreaterThan(0)
      expect(soSanh(dauPhat, ho)).toBeGreaterThan(0)
    }
  })
})
