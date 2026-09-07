/**
 * Hằng số dùng chung cho mọi endpoint tải trực tiếp từ kho mã trên GitHub.
 * Tách riêng để `/api/tai-ve/[kenh]` (bản mới nhất) và
 * `/api/tai-ve/phien-ban-cu/[phien_ban]` (bản cũ) luôn trỏ về cùng một repo,
 * không lệch nhau khi đổi biến môi trường.
 */
export const GITHUB_REPO = process.env.QLGX_GITHUB_REPO ?? "khoannd/qlgx";
export const GITHUB_BRANCH = process.env.QLGX_GITHUB_BRANCH ?? "master";
export const GITHUB_RAW_BASE = `https://raw.githubusercontent.com/${GITHUB_REPO}/${GITHUB_BRANCH}`;

/**
 * Kho nhị phân RIÊNG, tách khỏi kho mã nguồn `qlgx` — chỉ chứa các tệp cài đặt
 * và gói cập nhật (`Release/*.exe`, `Release/update/*_update.zip`), không có
 * `VersionConfig.xml` hay trang HTML ghi chú (những thứ đó lấy từ kho `qlgx`,
 * xem `GITHUB_RAW_BASE`). Xem HOP_DONG_MAY_CHU_CAP_NHAT.md ở gốc kho `qlgx`.
 */
export const QLGX_BIN_REPO = process.env.QLGX_BIN_GITHUB_REPO ?? "khoannd/qlgx_bin";
export const QLGX_BIN_BRANCH = process.env.QLGX_BIN_GITHUB_BRANCH ?? "master";
export const QLGX_BIN_RAW_BASE = `https://raw.githubusercontent.com/${QLGX_BIN_REPO}/${QLGX_BIN_BRANCH}`;
