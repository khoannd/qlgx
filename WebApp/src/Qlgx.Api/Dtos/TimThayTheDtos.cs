namespace Qlgx.Api.Dtos;

/// <summary>"Tìm và thay thế" — xem docs/superpowers/specs/man-hinh/tim-thay-the.md.</summary>
public enum BangTimThayThe { GiaoDan, GiaDinh }

public record TimThayTheRequest(BangTimThayThe Bang, string Truong, string GiaTriTim, string GiaTriThay);

public record TimThayTheXemTruocKetQua(int SoBanGhiKhop);

public record TimThayTheKetQua(int SoBanGhiDaThay);
