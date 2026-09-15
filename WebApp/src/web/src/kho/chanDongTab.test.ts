import { afterEach, describe, expect, it, vi } from 'vitest'
import { chanDongTabKhiConHangCho } from './chanDongTab'

function baoUnload(): BeforeUnloadEvent {
  const ev = new Event('beforeunload') as BeforeUnloadEvent
  Object.defineProperty(ev, 'returnValue', { value: '', writable: true })
  return ev
}

describe('chanDongTabKhiConHangCho (Task 7 — spec 6.6, chế độ tắt offline)', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('hang cho CO gi: goi preventDefault() va gan returnValue KHAC RONG (Chrome/Edge cu can gia tri nay)', () => {
    const huy = chanDongTabKhiConHangCho(() => 3)
    const ev = baoUnload()
    const spy = vi.spyOn(ev, 'preventDefault')

    window.dispatchEvent(ev)

    expect(spy).toHaveBeenCalled()
    // returnValue = '' KHONG kich hoat hop thoai theo dac ta HTML — phai la mot gia tri "truthy".
    expect(ev.returnValue).toBeTruthy()
    huy()
  })

  it('hang cho RONG: KHONG chan gi ca', () => {
    const huy = chanDongTabKhiConHangCho(() => 0)
    const ev = baoUnload()
    const spy = vi.spyOn(ev, 'preventDefault')

    window.dispatchEvent(ev)

    expect(spy).not.toHaveBeenCalled()
    huy()
  })

  it('goi ham huy dang ky thi KHONG con chan nua, du hang cho van con', () => {
    const huy = chanDongTabKhiConHangCho(() => 5)
    huy()

    const ev = baoUnload()
    const spy = vi.spyOn(ev, 'preventDefault')
    window.dispatchEvent(ev)

    expect(spy).not.toHaveBeenCalled()
  })

  it('doc SO MOI NHAT moi lan xay ra su kien (khong cache gia tri luc dang ky)', () => {
    let soHangCho = 0
    const huy = chanDongTabKhiConHangCho(() => soHangCho)

    const ev1 = baoUnload()
    const spy1 = vi.spyOn(ev1, 'preventDefault')
    window.dispatchEvent(ev1)
    expect(spy1).not.toHaveBeenCalled()

    soHangCho = 1
    const ev2 = baoUnload()
    const spy2 = vi.spyOn(ev2, 'preventDefault')
    window.dispatchEvent(ev2)
    expect(spy2).toHaveBeenCalled()

    huy()
  })
})
