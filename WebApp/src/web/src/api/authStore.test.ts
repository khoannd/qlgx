import { afterEach, describe, expect, it, vi } from 'vitest'
import { authStore } from './authStore'

describe('authStore', () => {
  afterEach(() => {
    authStore.xoaToken()
    authStore.huy401()
    localStorage.clear()
  })

  it('luu va doc lai token qua localStorage', () => {
    authStore.datToken('token-gia')

    expect(authStore.layToken()).toBe('token-gia')
  })

  it('xoaToken lam layToken tra ve null', () => {
    authStore.datToken('token-gia')

    authStore.xoaToken()

    expect(authStore.layToken()).toBeNull()
  })

  it('baoHet401 goi ham da dang ky qua dangKy401', () => {
    const fn = vi.fn()
    authStore.dangKy401(fn)

    authStore.baoHet401()

    expect(fn).toHaveBeenCalledTimes(1)
  })

  it('huy401 lam baoHet401 khong con goi ham cu', () => {
    const fn = vi.fn()
    authStore.dangKy401(fn)
    authStore.huy401()

    authStore.baoHet401()

    expect(fn).not.toHaveBeenCalled()
  })
})
