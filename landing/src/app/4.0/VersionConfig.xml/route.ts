import { versionConfigXmlResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi các máy cài bản 4.0.0–4.0.1 — nội dung y hệt
 * `/capnhat/VersionConfig.xml`. KHÔNG được xoá — xem HOP_DONG_MAY_CHU_CAP_NHAT.md,
 * mục "Các địa chỉ cũ".
 */
export const GET = versionConfigXmlResponse;
