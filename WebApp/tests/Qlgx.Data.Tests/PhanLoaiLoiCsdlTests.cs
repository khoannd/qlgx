using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Data.DongBo;

namespace Qlgx.Data.Tests;

/// <summary>
/// <see cref="PhanLoaiLoiCsdl"/> là một HÀM THUẦN, nên kiểm nó như một hàm thuần.
///
/// Vì sao không để bộ test tích hợp gánh: test tích hợp chỉ dựng được các ca TẤT ĐỊNH (dễ tạo:
/// ghi null vào cột NOT NULL, ghi trùng khoá). Chiều còn lại — deadlock, mất kết nối, hết thời
/// gian chờ khoá — gần như không dựng được một cách tất định trong một bộ test. Mà đó lại đúng là
/// chiều NGUY HIỂM: nhận nhầm một lỗi tạm thời thành "tất định" sẽ VỨT BỎ một thao tác đáng lẽ
/// thành công ở lần thử sau — mất dữ liệu thật, im lặng. Đột biến nới danh sách mã lỗi thành
/// "mọi mã đều tất định" sống sót qua toàn bộ test tích hợp; chỉ lớp này bắt được.
/// </summary>
public class PhanLoaiLoiCsdlTests
{
    private static PostgresException Loi(string maLoi)
        => new("loi gia lap de kiem phan loai", "ERROR", "ERROR", maLoi);

    [Theory]
    // 22xxx data_exception: sai kiểu, chuỗi quá dài, ngày không hợp lệ.
    [InlineData("22001")]  // string_data_right_truncation
    [InlineData("22007")]  // invalid_datetime_format
    [InlineData("22P02")]  // invalid_text_representation
    // 23xxx integrity_constraint_violation.
    [InlineData("23502")]  // not_null_violation — ca "HoTen de rong" cua may con ban cu
    [InlineData("23503")]  // foreign_key_violation
    [InlineData("23505")]  // unique_violation — ca "MaGiaoDanCu trung"
    [InlineData("23514")]  // check_violation
    public void Loi_ve_DU_LIEU_va_RANG_BUOC_la_tat_dinh(string maLoi)
        => PhanLoaiLoiCsdl.LaTatDinh(Loi(maLoi)).Should().BeTrue(
            "gui lai dung du lieu do mot trieu lan van ra dung loi ay, nen quay lui ca lo khong " +
            "mua duoc gi — chi lam giao xu ket vinh vien trong vong lap gui lai");

    [Theory]
    [InlineData("40001")]  // serialization_failure
    [InlineData("40P01")]  // deadlock_detected
    [InlineData("55P03")]  // lock_not_available — het thoi gian cho khoa dong dem
    [InlineData("08006")]  // connection_failure
    [InlineData("08003")]  // connection_does_not_exist
    [InlineData("53300")]  // too_many_connections
    [InlineData("57014")]  // query_canceled
    public void Loi_TAM_THOI_khong_duoc_coi_la_tat_dinh(string maLoi)
        => PhanLoaiLoiCsdl.LaTatDinh(Loi(maLoi)).Should().BeFalse(
            "nhung loi nay thu lai THAT SU co giup — coi la tat dinh la VUT BO mot thao tac dang " +
            "le thanh cong o lan sau, tuc la mat du lieu that ma khong ai biet");

    [Fact]
    public void Nhan_ra_loi_nam_o_INNER_exception()
    {
        // EF luôn bọc lỗi CSDL trong DbUpdateException; mã lỗi nằm ở lớp trong.
        var boc = new DbUpdateException("Loi khi luu", Loi("23505"));
        PhanLoaiLoiCsdl.LaTatDinh(boc).Should().BeTrue(
            "chi nhin lop ngoai thi moi loi CSDL deu thanh 'khong tat dinh' va ca co che vo nghia");

        var bocTamThoi = new DbUpdateException("Loi khi luu", Loi("40P01"));
        PhanLoaiLoiCsdl.LaTatDinh(bocTamThoi).Should().BeFalse();
    }

    [Fact]
    public void Loi_cot_bat_buoc_do_chinh_EF_chan_truoc_khi_xuong_CSDL_la_tat_dinh()
    {
        // EF chặn TRƯỚC khi ra tới CSDL nên KHÔNG có mã lỗi nào để tra — phải nhận theo văn bản.
        // Test này chính là cái chuông: EF đổi câu chữ thì đây đỏ, chứ không âm thầm quay về chế
        // độ gãy cả lô.
        var loi = new InvalidOperationException(
            "The property 'GiaoDan.QuaDoi' contains null, but the property is marked as required.");
        PhanLoaiLoiCsdl.LaTatDinh(loi).Should().BeTrue();
    }

    [Fact]
    public void Loi_khong_lien_quan_toi_CSDL_khong_bi_nhan_nham()
    {
        PhanLoaiLoiCsdl.LaTatDinh(new InvalidOperationException("mot loi bat ky"))
            .Should().BeFalse();
        PhanLoaiLoiCsdl.LaTatDinh(new OperationCanceledException()).Should().BeFalse();
        // Lỗi đã mang ILoiTatDinh thì đã được xử ở tầng trên rồi, không đi qua đây.
        PhanLoaiLoiCsdl.LaTatDinh(new LoiRaoChan("cot cam")).Should().BeFalse();
    }
}
