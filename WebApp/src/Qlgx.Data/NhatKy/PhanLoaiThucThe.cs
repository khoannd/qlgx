namespace Qlgx.Data.NhatKy;

/// <summary>
/// Không phải mọi thực thể kế thừa <see cref="Qlgx.Domain.Entities.ThucTheCoSo"/> đều nên vào
/// nhật ký. Giả định ban đầu ("kế thừa ThucTheCoSo" == "cần nhật ký") đã để lọt
/// <see cref="Qlgx.Domain.Entities.TaiKhoan"/>: nếu ghi nhật ký, mã băm mật khẩu
/// (MatKhauBam) và câu trả lời gợi nhớ (CauTraLoiGoiY, văn bản rõ) sẽ được
/// <c>hieu_luc</c> phát xuống MỌI máy con — và vì nhật ký giữ cả giá trị cũ, mật khẩu đã đổi
/// vẫn nằm vĩnh viễn trên laptop offline. Thêm nữa, SoLanDangNhapSaiLienTiep/KhoaDangNhapDenLuc
/// đổi ở MỌI lần đăng nhập (đúng lẫn sai) — ghi nhật ký sẽ bơm chuỗi hieu_luc đầy dữ liệu phiên
/// đăng nhập, đúng loại nhiễu mà CotLoaiTru tồn tại để chặn, chỉ khác là ở mức BẢNG chứ không
/// phải mức CỘT.
///
/// Vì vậy việc "có ghi nhật ký hay không" phải là một quyết định TƯỜNG MINH theo tên bảng,
/// không được suy luận ngầm từ việc kế thừa ThucTheCoSo. PhanLoaiThucTheTests ép mọi thực thể
/// kế thừa ThucTheCoSo phải nằm trong đúng MỘT trong hai danh sách dưới đây — thêm bảng mới mà
/// quên phân loại sẽ làm test đỏ ngay, không được trôi qua im lặng.
/// </summary>
public static class PhanLoaiThucThe
{
    /// <summary>Sổ sách giáo xứ — cần đồng bộ xuống máy con qua hieu_luc.</summary>
    public static readonly HashSet<string> DuocGhi = new(StringComparer.Ordinal)
    {
        "GiaoHo", "GiaDinh", "GiaoDan", "HonPhoi",
        "CauHinh", "DuLieuChung", "VaiTro", "TenLoaiTaiKhoan",
        "DotBiTich", "BiTichChiTiet", "ChuyenXu", "RaoHonPhoi", "TanHien", "LinhMuc",
        "KhoiGiaoLy", "LopGiaoLy", "ChiTietLopGiaoLy", "GiaoLyVien", "HoiDoan", "ChiTietHoiDoan",
    };

    /// <summary>
    /// Cố ý KHÔNG ghi nhật ký, dù có kế thừa ThucTheCoSo.
    ///
    /// - TaiKhoan: dữ liệu quản trị đăng nhập (mật khẩu băm, câu hỏi gợi nhớ, bộ đếm đăng nhập
    ///   sai) — không phải sổ sách cần phát xuống máy con. Xem lý do đầy đủ ở phần mở đầu file.
    ///
    /// MauInTuyChinh và CachHienThiDungSai (mẫu in tuỳ chỉnh, câu chữ hiển thị đúng/sai) CŨNG cố
    /// ý không vào nhật ký — nhưng KHÔNG liệt ở đây vì chúng không kế thừa ThucTheCoSo (xem
    /// MauInTuyChinh.cs, CachHienThiDungSai.cs), nên PhanLoaiThucTheTests không thấy chúng là
    /// ứng viên. Ghi chú ở đây để người sau không tưởng nhầm là bị bỏ sót.
    /// </summary>
    public static readonly HashSet<string> KhongGhi = new(StringComparer.Ordinal)
    {
        "TaiKhoan",
    };

    public static bool DuocGhiNhatKy(string tenBang) => DuocGhi.Contains(tenBang);
}
