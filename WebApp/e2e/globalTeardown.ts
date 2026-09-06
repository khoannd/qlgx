import { Client } from 'pg'
import { chuoiKetNoiGoc, runtimeEnv, thanhPgConfig, xoaRuntimeEnv } from './runtimeEnv.ts'

/** Dọn sạch database e2e riêng — KHÔNG được để lại rác, và tuyệt đối không đụng `qlgx_thu`.
 * Đây VẪN dùng cơ chế `globalTeardown` chuẩn của Playwright (không như `prepareDb.ts`) vì
 * không có vấn đề thứ tự nào ở chiều dọn dẹp — Playwright luôn dừng hết `webServer` trước khi
 * gọi `globalTeardown`. */
export default async function globalTeardown() {
  const env = runtimeEnv()
  const goc = chuoiKetNoiGoc()

  const client = new Client(thanhPgConfig(goc, 'postgres'))
  await client.connect()
  try {
    await client.query(`DROP DATABASE IF EXISTS "${env.tenDb}" WITH (FORCE)`)
    console.log(`[e2e] Da xoa database "${env.tenDb}".`)
  } finally {
    await client.end()
  }

  xoaRuntimeEnv()
}
