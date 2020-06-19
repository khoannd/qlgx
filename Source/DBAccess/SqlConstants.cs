using System;
using System.Collections.Generic;
using System.Text;

namespace GxGlobal
{
    public class SqlConstants
    {
        // 12/06/2019 Hiệp add start
        /// <summary>
        /// danh sách hội đoàn
        /// </summary>
        public const string SELECT_LIST_HOIDOAN = @"Select *
                                                    from 
                                                        HoiDoan";

        /// <summary>
        /// Danh sách hội viên trong hội đoàn
        /// </summary>
        public const string SELECT_LIST_HOIVIEN_HOIDOAN = @"select IIF((ChiTietHoiDoan.NgayRaHoiDoan <>''),0,-1) as Deleted,
                                                                    1 as Existed, GiaoDan.*, ChiTietHoiDoan.VaiTro, 
                                                                    ChiTietHoiDoan.NgayVaoHoiDoan,ChiTietHoiDoan.NgayRaHoiDoan 
                                                            from 
                                                                    ChiTietHoiDoan inner join GiaoDan on ChiTietHoiDoan.MaGiaoDan=GiaoDan.MaGiaoDan 
                                                            where 
                                                                    ( ChiTietHoiDoan.NgayRaHoiDoan='')
                                                            and     ChiTietHoiDoan.MaHoiDoan= ? ";

        /// <summary>
        /// Danh sách hội viên đã ra khỏi hội đoàn
        /// </summary>
        public const string SELECT_LIST_HISTORY_HOIVIEN_HOIDOAN = @"select IIF(ChiTietHoiDoan.NgayRaHoiDoan <>'',0,-1) as Deleted,
                                                                    1 as Existed, GiaoDan.*, ChiTietHoiDoan.VaiTro, 
                                                                    ChiTietHoiDoan.NgayVaoHoiDoan,ChiTietHoiDoan.NgayRaHoiDoan 
                                                            from 
                                                                    ChiTietHoiDoan inner join GiaoDan on ChiTietHoiDoan.MaGiaoDan=GiaoDan.MaGiaoDan 
                                                            where ChiTietHoiDoan.MaHoiDoan= ? 
                                                             order by NgayRaHoiDoan is null";

        /// <summary>
        /// Danh sách hội đoàn bằng ID
        /// </summary>
        public const string SELECT_HOIDOAN_BY_MAHOIDOAN = @"Select * 
                                                     from 
                                                            HoiDoan 
                                                     where 
                                                            MaHoiDoan = ? ";

        /// <summary>
        /// Danh sách hội đoàn chi tiết bằng mã hội đoàn
        /// </summary>
        public const string SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN = @"Select ChiTietHoiDoan.*,GiaoDan.HoTen 
                                                     from 
                                                            ChiTietHoiDoan left join GiaoDan On ChiTietHoiDoan.MaGiaoDan=GiaoDan.MaGiaoDan
                                                     where 
                                                            MaHoiDoan = ? ";
        public const string SELECT_LIST_HISTORY_HOIDOAN_BY_MAGIAODAN = @"select 
                                                                            CTHD.NgayVaoHoiDoan,CTHD.NgayRaHoiDoan,
                                                                            CTHD.VaiTro,HD.TenHoiDoan 
                                                                        from 
                                                                            HoiDoan as HD inner join ChiTietHoiDoan as CTHD on  HD.MaHoiDoan=CTHD.MaHoiDoan
                                                                        where 
                                                                            CTHD.MaGiaoDan=? order by CTHD.NgayRaHoiDoan is null";

        // 26/07/2019 hiepdv end add

        //17/09/2019 hiepdv begin add
        public const string UPDATE_MAGIAOXUDOI = @"UPDATE GiaoXu SET 
                                                GiaoXu.MaGiaoXuDoi = ?, GiaoXu.UpdateDate=?";
        public const string UPDATE_GIAOPHAN = @"UPDATE GiaoPhan SET 
                                                GiaoPhan.TenGiaoPhan = ?, GiaoPhan.MaGiaoPhanRieng=?,GiaoPhan.UpdateDate=?";
        public const string UPDATE_GIAOHAT = @"UPDATE GiaoHat SET 
                                                GiaoHat.TenGiaoHat = ?, GiaoHat.MaGiaoHatRieng=?,GiaoHat.UpdateDate=?";
        public const string UPDATE_GIAOXU = @"UPDATE GiaoXu SET 
                                                GiaoXu.TenGiaoXu = ?, GiaoXu.DiaChi=?, GiaoXu.DienThoai=?, GiaoXu.Email=?,GiaoXu.Website=?,
                                                GiaoXu.Hinh=?, GiaoXu.GhiChu=?, GiaoXu.MaGiaoXuRieng=?,GiaoXu.UpdateDate=? ";


        //17/07/2019 hiepdv end add

        //20/11/2019 hiepdv begin add
        public const string SELECT_INFO_GIAOXU = @"SELECT GiaoPhan.TenGiaoPhan,GiaoPhan.MaGiaoPhanRieng, GiaoHat.TenGiaoHat, GiaoHat.MaGiaoHatRieng, GiaoXu.* 
                                                FROM GiaoPhan RIGHT JOIN (GiaoHat RIGHT JOIN GiaoXu ON GiaoHat.MaGiaoHat = GiaoXu.MaGiaoHat) ON GiaoPhan.MaGiaoPhan = GiaoHat.MaGiaoPhan ";
        //20/11/2019 hiepdv end add


        //2018-08-14 Gia add start
        public const string UPDATE_MAGIAOHATRIENG = @"UPDATE GiaoHat SET 
                                                GiaoHat.MaGiaoHatRieng = ?";
        public const string UPDATE_MAGIAOPHANRIENG = @"UPDATE GiaoPhan SET 
                                                GiaoPhan.MaGiaoPhanRieng = ?";
        //2018-08-14 Gia add end

        //2018-08-08 Gia add start
        public const string UPDATE_LASTUPLOAD = @"UPDATE GiaoXu SET 
                                                GiaoXu.LastUpload = ?,GiaoXu.UpdateDate=?";
        //2018-08-08 Gia add end

        //2018-08-01 Gia add start
        public const string UPDATE_MAGIAOXURIENG = @"UPDATE GiaoXu SET 
                                                GiaoXu.MaGiaoXuRieng = ?,UpdateDate=?";
        public const string SELECT_MAGIAOXURIENG = @"SELECT GiaoXu.MaGiaoXuRieng
                                                from GiaoXu
                                                ";
        //2018-08-01 Gia add end

        //2018-07-23 Gia add start
        public const string UPDATE_GIAODAN_DACOGIADINH = @"UPDATE GiaoDan 
                                                SET GiaoDan.UpdateDate=?, GiaoDan.DaCoGiaDinh = 0
                                                where MaGiaoDan=?
                                                ";
        public const string SELECT_GIAODAN_HONPHOI_WITH_ID = @"Select GiaoDan.MaGiaoDan,GiaoDan.QuaDoi,GiaoDan.DaCoGiaDinh
                                from (Select MaGiaoDan
                                from (SELECT MaHonPhoi as test
                                FROM GiaoDanHonPhoi 
                                WHERE MaGiaoDan=?),GiaoDanHonPhoi
                                where MaGiaoDan<>? and test=GiaoDanHonPhoi.MaHonPhoi)as MGD2,GiaoDan
                                where MGD2.MaGiaoDan=GiaoDan.MaGiaoDan";
        //2018-07-23 Gia add end

        //2018-07-18 Gia add start
        public const string SELECT_MAHONPHOI_HOPLE = @"Select MaHonPhoi
                            from (Select MaGiaoDan
                                            from (Select MaGiaoDan as abc
                                            from (SELECT MaHonPhoi
                                            FROM GiaoDanHonPhoi 
                                            WHERE MaGiaoDan=?) as Test,GiaoDanHonPhoi
                                            where  Test.MaHonPhoi=GiaoDanHonPhoi.MaHonPhoi And MaGiaoDan<>?
                                            ),GiaoDan
                                            where abc=GiaoDan.MaGiaoDan and GiaoDan.QuaDoi=?) as MHP2,GiaoDanHonPhoi
                            where GiaoDanHonPhoi.MaGiaoDan=MHP2.MaGiaoDan";
        //2018-07-18 Gia add end
        //        public const string SELECT_GIAODAN_LIST_CO_GIAOHO = @"SELECT GiaoDan.*,  IIF(ThanhVienGiaDinh.MaGiaoDan1 IS NOT NULL,-1,0) AS DaCoGiaDinh
        //                                            FROM (SELECT GiaoDan.*, GiaoHo.TenGiaoHo, IIF(ChuyenXu.MaGiaoDan IS NOT NULL,-1,0) AS DaChuyenXu,
        //                                                                                     IIF(GiaoDan.NgaySinh IS NOT NULL,RIGHT(GiaoDan.NgaySinh,4),"""") AS NamSinh
        //                                                    FROM (
        //                                                            (SELECT DISTINCT ChuyenXu.MaGiaoDan
        //                                                            FROM ChuyenXu WHERE LoaiChuyen=2
        //                                                            GROUP BY ChuyenXu.MaGiaoDan) AS ChuyenXu 
        //                                                        RIGHT JOIN GiaoDan ON ChuyenXu.MaGiaoDan = GiaoDan.MaGiaoDan
        //                                                        ) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo
        //                                            )  AS GiaoDan 
        //                                            LEFT JOIN (SELECT DISTINCT MaGiaoDan AS MaGiaoDan1 FROM ThanhVienGiaDinh
        //                                                    WHERE VaiTro=0 OR VaiTro=1
        //                                                    GROUP BY MaGiaoDan, VaiTro) AS ThanhVienGiaDinh 
        //                                            ON GiaoDan.MaGiaoDan = ThanhVienGiaDinh.MaGiaoDan1
        //                                            WHERE 1 ";
        public const string SELECT_GIAODAN_LIST_CO_GIAOHO = @"SELECT GiaoDan.*
                                                    FROM (SELECT GiaoDan.*, GiaoHo.MaGiaoHoCha, IIF(GiaoDan.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo, IIF(ChuyenXu.MaGiaoDan IS NOT NULL,-1,0) AS DaChuyenXu,
                                                                                             IIF(GiaoDan.NgaySinh IS NOT NULL,RIGHT(GiaoDan.NgaySinh,4),"""") AS NamSinh
                                                            FROM (
                                                                    (SELECT DISTINCT ChuyenXu.MaGiaoDan
                                                                    FROM ChuyenXu WHERE LoaiChuyen=2
                                                                    GROUP BY ChuyenXu.MaGiaoDan) AS ChuyenXu 
                                                                RIGHT JOIN GiaoDan ON ChuyenXu.MaGiaoDan = GiaoDan.MaGiaoDan
                                                                ) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo ORDER BY GiaoDan.MaGiaoDan ASC
                                                    )  AS GiaoDan 
                                                    WHERE 1 ";
        //2018-01-07 SonVc add start
        public const string SELECT_LISTGIAODAN_BYGIADINH =
@"
SELECT DISTINCT GiaoDan.*
                                                    FROM (SELECT GiaoDan.*, GiaoHo.MaGiaoHoCha, IIF(GiaoDan.MaGiaoHo = 0, 'Ngoài xứ', GiaoHo.TenGiaoHo) AS TenGiaoHo, IIF(ChuyenXu.MaGiaoDan IS NOT NULL,-1,0) AS DaChuyenXu,
                                                                                             IIF(GiaoDan.NgaySinh IS NOT NULL,RIGHT(GiaoDan.NgaySinh,4),'') AS NamSinh
                                                            FROM (
                                                                    (SELECT DISTINCT ChuyenXu.MaGiaoDan
                                                                    FROM ChuyenXu WHERE LoaiChuyen=2
                                                                    GROUP BY ChuyenXu.MaGiaoDan) AS ChuyenXu 
                                                                RIGHT JOIN GiaoDan ON ChuyenXu.MaGiaoDan = GiaoDan.MaGiaoDan
                                                                ) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo ORDER BY GiaoDan.MaGiaoDan ASC
                                                    )  AS GiaoDan INNER JOIN ThanhVienGiaDinh ON GiaoDan.MaGiaoDan = ThanhVienGiaDinh.MaGiaoDan
                                                    WHERE 1 ";
        //2018-01-07 SonVc add end

        //2018-01-15 SonVc add start


        public const string SELECT_ACCOUNT = @"SELECT TenTaiKhoan FROM TaiKhoan WHERE TenTaiKhoan = ?";
        public const string SELECT_LIST_ACCOUNT = @"SELECT * FROM TaiKhoan WHERE DaXoa = 0";
        public const string SELECT_LIST_ACCCOUT_WITH_NAME = @"
SELECT HoTenNguoiDung,TenTaiKhoan,Email,SoDienThoai,TenLoai,MatKhau,LoaiTaiKhoan,CauHoiGoiY,CauTraLoiGoiY
FROM TaiKhoan INNER JOIN TenLoaiTaiKhoan ON TenLoaiTaiKhoan.ID = TaiKhoan.LoaiTaiKhoan
WHERE DaXoa = 0
";
        public const string SELECT_LIST_LOAI_TAI_KHOAN = @"SELECT TenLoai FROM TenLoaiTaiKhoan";
        public const string UPDATE_TAIKHOAN = @"UPDATE TaiKhoan SET TaiKhoan.UpdateDate=?,
HoTenNguoiDung = ?,MatKhau = ? ,Email = ? ,SoDienThoai = ? ,LoaiTaiKhoan = ?,CauHoiGoiY = ?,CauTraLoiGoiY = ? WHERE TenTaiKhoan = ?";
        public const string UPDATE_TAIKHOAN_NOTPASSWORD = @"UPDATE TaiKhoan SET TaiKhoan.UpdateDate=?, HoTenNguoiDung = ?,Email = ? ,SoDienThoai = ?,LoaiTaiKhoan = ?,CauHoiGoiY = ?,CauTraLoiGoiY = ? WHERE TenTaiKhoan = ?";
        public const string DELETE_ACCOUNT = @"UPDATE TaiKhoan SET DaXoa = -1 WHERE TenTaiKhoan = ?";
        public const string SELECT_QUESTION = @"SELECT CauHoiGoiY,CauTraLoiGoiY FROM TaiKhoan WHERE TenTaiKhoan = ? AND DaXoa = 0";
        public const string UPDATE_PASSWORD = @"UPDATE TaiKhoan SET TaiKhoan.UpdateDate=?, MatKhau = ? WHERE TenTaiKhoan = ?";
        public const string CHECK_LOGIN = @"SELECT TenTaiKhoan,LoaiTaiKhoan FROM TaiKhoan WHERE TenTaiKhoan = ? AND MatKhau = ? AND DaXoa = 0";

        public const string SELECT_CHITIET_GIADINH = @"SELECT GiaoDan.*,TVGD.VaiTro,TVGD.ChuHo,LGD.TenGiaDinh
FROM
(
SELECT *
FROM SELECT_GIADINH_LIST AS LGD INNER JOIN ThanhVienGiaDinh AS TVGD ON LGD.MaGiaDinh = TVGD.MaGiaDinh 
) AS GD,GiaoDan
WHERE GD.MaGiaoDan = GiaoDan.MaGiaoDan  AND TVGD.MaGiaDinh = ?
ORDER BY TVGD.VaiTro";
        public const string SET_DA_CO_GIADINH = @"UPDATE GiaoDan SET GiaoDan.UpdateDate=?, GiaoDan.DaCoGiaDinh = -1 WHERE MaGiaoDan = ?";
        //2018-01-15 SonVc add end

        /// <summary>
        /// Luu y phai su dung bang string.Format va dua vao tham so la thong tin table GiaoDan
        /// </summary>
        public const string SELECT_THANHVIEN_GIADINH = @"SELECT IIF(GiaoDan.QuaDoi=0 and GiaoDan.DaChuyenXu=0 and 
( GiaoDan.MaGiaoDan not in 
(select TVGD.MaGiaoDan from ThanhVienGiaDinh TVGD where TVGD.MaGiaoDan=GiaoDan.MaGiaoDan and TVGD.MaGiaDinh<>ThanhVienGiaDinh.MaGiaDinh )),0,-1) as GACH,ThanhVienGiaDinh.MaGiaDinh, ThanhVienGiaDinh.VaiTro, ThanhVienGiaDinh.ChuHo, GiaoDan.*
        FROM((ThanhVienGiaDinh INNER JOIN GiaDinh ON ThanhVienGiaDinh.MaGiaDinh = GiaDinh.MaGiaDinh)
                                            INNER JOIN({0}) AS GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo
                                                WHERE GiaoDan.DaXoa=0";
        //public const string SELECT_THANHVIEN_GIADINH = @"SELECT ThanhVienGiaDinh.MaGiaDinh, ThanhVienGiaDinh.VaiTro, ThanhVienGiaDinh.ChuHo, GiaoDan.*
        //                                    FROM ((ThanhVienGiaDinh INNER JOIN GiaDinh ON ThanhVienGiaDinh.MaGiaDinh = GiaDinh.MaGiaDinh) 
        //                                    INNER JOIN ({0}) AS GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo
        //                                    WHERE GiaoDan.DaXoa=0";
        public const string SELECT_GIAODAN_THEO_ID = @"SELECT GiaoDan.*, ChuyenXu.MaChuyenXu, ChuyenXu.NgayChuyen, ChuyenXu.NoiChuyen, ChuyenXu.LoaiChuyen, ChuyenXu.GhiChuChuyen
                                                        FROM GiaoDan LEFT JOIN ChuyenXu ON GiaoDan.MaGiaoDan = ChuyenXu.MaGiaoDan
                                                        WHERE GiaoDan.MaGiaoDan = ? ";
        public const string SELECT_CHECK_GIAODAN_TONTAI = "SELECT GiaoDan.* FROM GiaoDan WHERE HoTen = ? AND TenThanh = ? AND NgaySinh = ? AND GiaoDan.DaXoa = 0 AND GiaoDan.MaGiaoDan <> ?";
        /// <summary>
        /// Yeu cau 2 tham so la MaGiaoHo va MaGiaDinh
        /// </summary>
        //public const string SELECT_GIADINH_THEO_ID = @"SELECT * FROM SELECT_GIADINH_LIST WHERE MaGiaDinh = ?";
        public const string SELECT_GIADINH_THEO_ID = @"SELECT TOP 1 GiaDinh.*, HonPhoi.SoHonPhoi, HonPhoi.NgayHonPhoi, HonPhoi.NoiHonPhoi, HonPhoi.LinhMucChung, HonPhoi.NguoiChung1, HonPhoi.NguoiChung2, HonPhoi.CachThucHonPhoi
                                            FROM (((SELECT_GIADINH_LIST GiaDinh INNER JOIN ThanhVienGiaDinh TVGD ON GiaDinh.MaGiaDinh=TVGD.MaGiaDinh)
                                            LEFT JOIN GiaoDanHonPhoi GDHP ON GDHP.MaGiaoDan=TVGD.MaGiaoDan)
                                            LEFT JOIN HonPhoi ON HonPhoi.MaHonPhoi=GDHP.MaHonPhoi)
                                             WHERE (TVGD.VaiTro=0 OR TVGD.VaiTro=1) AND GiaDinh.MaGiaDinh = ?";

        /// <summary>
        /// Yeu cau tham so MaGiaDinh. Luu y phai su dung bang string.Format va dua vao tham so la thong tin table GiaoDan
        /// </summary>
        public const string SELECT_CHA_ME_THEO_GIADINH = @"SELECT ThanhVienGiaDinh.MaGiaDinh, GiaoDan.*, ThanhVienGiaDinh.VaiTro, ThanhVienGiaDinh.ChuHo
                                                        FROM ThanhVienGiaDinh INNER JOIN ({0}) AS GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan
                                                        WHERE (ThanhVienGiaDinh.VaiTro=0 Or ThanhVienGiaDinh.VaiTro=1) AND MaGiaDinh = ?
                                                         ";
        /// <summary>
        /// Select danh sach gia dinh
        /// </summary>
        public const string SELECT_GIADINH_LIST = @"SELECT * FROM SELECT_GIADINH_LIST WHERE MaGiaDinh<>NULL ";

        public const string SELECT_GIADINH_LIST_CO_HONPHOI = @"SELECT DISTINCT * FROM (SELECT DISTINCT GiaDinh.*, HonPhoi.SoHonPhoi, HonPhoi.NgayHonPhoi, HonPhoi.NoiHonPhoi, HonPhoi.LinhMucChung, HonPhoi.NguoiChung1, HonPhoi.NguoiChung2, HonPhoi.CachThucHonPhoi
                                            FROM (((SELECT_GIADINH_LIST GiaDinh INNER JOIN ThanhVienGiaDinh TVGD ON GiaDinh.MaGiaDinh=TVGD.MaGiaDinh)
                                            LEFT JOIN GiaoDanHonPhoi GDHP ON GDHP.MaGiaoDan=TVGD.MaGiaoDan)
                                            LEFT JOIN HonPhoi ON HonPhoi.MaHonPhoi=GDHP.MaHonPhoi)
                                             WHERE (TVGD.VaiTro=0 OR TVGD.VaiTro=1)) WHERE 1 ";

        /*
        public const string SELECT_GIADINH_LIST = @"TRANSFORM First([TenThanh]+" "+[HoTen]) AS TenDayDu
                                                SELECT GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo
                                                FROM ((SELECT * FROM ThanhVienGiaDinh INNER JOIN VaiTro ON(ThanhVienGiaDinh.VaiTRo = VaiTro.ID))  AS TVGD INNER JOIN (SELECT * FROM GiaoDan)  AS A ON TVGD.MaGiaoDan = A.MaGiaoDan) INNER JOIN (SELECT GiaDinh.*, GiaoHo.TenGiaoHo FROM GiaDinh INNER JOIN GiaoHo ON (GiaDinh.MaGiaoHo = GiaoHo.MaGiaoHo))  AS GD ON TVGD.MaGiaDinh = GD.MaGiaDinh
                                                WHERE TVGD.VaiTro=0 OR TVGD.ThanhVienGiaDinh.VaiTro=1 AND GD.DaXoa=0
                                                GROUP BY GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo
                                                PIVOT TVGD.Value ";
        */
        public const string DELETE_GIADINH_THEO_ID = "UPDATE GiaDinh SET DaXoa=-1, UpdateDate = ? WHERE MaGiaDinh = ? ";//"DELETE FROM GiaDinh WHERE MaGiaDinh = ? ";

        public const string DELETE_CONCAI_THEO_GIADINH = "DELETE FROM ThanhVienGiaDinh WHERE MaGiaDinh = ? ";
        public const string SELECT_GIAOHO = "SELECT * FROM GiaoHo WHERE DaXoa = 0 ";
        public const string SELECT_COUNT_GIADINH_THUOC_GIAOHO = "SELECT COUNT(MaGiaDinh) AS SoLuong FROM GiaDinh WHERE (MaGiaoHo = ? OR MaGiaoHoCha = ?) ";
        public const string SELECT_COUNT_GIAODAN_THUOC_GIAOHO = "SELECT COUNT(MaGiaoDan) AS SoLuong FROM GiaoDan WHERE (MaGiaoHo = ? OR MaGiaoHoCha = ?) ";
        public const string DELETE_GIAOHO_KHONG_CO_GIADINH = "DELETE FROM GiaoHo WHERE MaGiaoHo = ? ";
        public const string DELETE_GIAOHO_CO_GIADINH = "UPDATE GiaoHo SET DaXoa=-1, UpdateDate = ? WHERE MaGiaoHo = ? ";
        public const string DELETE_GIAODAN = "UPDATE GiaoDan SET DaXoa=-1, UpdateDate = ? WHERE MaGiaoDan = ? ";

        public const string SELECT_LINHMUC_LIST = "SELECT * FROM LinhMuc WHERE DaXoa = 0 ";
        public const string SELECT_GIAOXU = @"SELECT GiaoPhan.TenGiaoPhan, GiaoHat.TenGiaoHat, GiaoHat.MaGiaoHatRieng, GiaoXu.* 
                                                FROM GiaoPhan RIGHT JOIN (GiaoHat RIGHT JOIN GiaoXu ON GiaoHat.MaGiaoHat = GiaoXu.MaGiaoHat) ON GiaoPhan.MaGiaoPhan = GiaoHat.MaGiaoPhan ";
        public const string SELECT_CHECK_LINHMUC = "SELECT MaLinhMuc FROM LinhMuc WHERE DaXoa = 0 AND TenThanh = ? AND HoTen = ? AND NgaySinh = ? AND MaLinhMuc <> ? ";
        public const string DELETE_LINHMUC_THEO_ID = "UPDATE LinhMuc SET DaXoa=-1, UpdateDate = ? WHERE MaLinhMuc = ? ";//"DELETE FROM GiaDinh WHERE MaGiaDinh = ? ";
                                                                                                                            //        public const string SELECT_GIAODAN_THEO_HONPHOI = @"SELECT A.*, B.TenGiaoHo
                                                                                                                            //                                                        FROM (SELECT     GiaDinh.MaGiaDinh, GiaDinh.NguoiChong, GiaDinh.NguoiVo, GiaDinh.NgayHonPhoi, GiaoDan.*
                                                                                                                            //                                                            FROM         (GiaoDan RIGHT OUTER JOIN
                                                                                                                            //                                                                              GiaDinh ON GiaoDan.MaGiaoDan = GiaDinh.NguoiChong OR GiaoDan.MaGiaoDan = GiaDinh.NguoiVo)
                                                                                                                            //                                                                          ) A
                                                                                                                            //                                                        LEFT JOIN GiaoHo B ON A.MaGiaoHo = B.MaGiaoHo WHERE A.DaXoa = 0 ";

        public const string UPDATE_GIAOHO = "UPDATE GiaoHo SET TenGiaoHo = ?, UpdateDate = ? WHERE MaGiaoHo = ?";
        //public const string SELECT_TENTHANH = "SELECT DISTINCT TenThanh FROM GiaoDan ";
        public const string SELECT_TENTHANH = "SELECT DISTINCT DuLieu1 AS TenThanh FROM DuLieuChung WHERE LoaiDuLieu=1 ";

        public const string SELECT_CHA_ME = @"SELECT S_GiaDinh.TenVo AS TenMe, S_GiaDinh.TenChong AS TenCha FROM (SELECT * FROM SELECT_GIADINH_LIST) 
                                    AS S_GiaDinh INNER JOIN ThanhVienGiaDinh ON ThanhVienGiaDinh.MaGiaDinh = S_GiaDinh.MaGiaDinh 
                                    WHERE ThanhVienGiaDinh.MaGiaoDan = ?";
        //        /// <summary>
        //        /// Phải dùng string.Format nếu có điều kiện WHERE
        //        /// </summary>
        //        public const string SELECT_GIADINH_GIAODAN = @"SELECT GiaDinh.MaGiaDinh, GiaDinh.TenGiaDinh, GiaDinh.MaGiaoHo, HonPhoi.SoHonPhoi, 
        //                                    HonPhoi.NgayHonPhoi, HonPhoi.NoiHonPhoi, HonPhoi.LinhMucChung, HonPhoi.NguoiChung1, HonPhoi.NguoiChung2, HonPhoi.CachThucHonPhoi, 
        //                                    GiaDinh.GhiChu, GiaDinh.DienThoai, GiaDinh.DiaChi, GiaDinh.DaXoa, GiaDinh.UpdateDate, GiaDinh.TenGiaoHo, GiaDinh.TenChong, GiaDinh.TenVo, 
        //                                    GiaDinh.GiaDinhAo, GiaDinh.MaNhanDang, GiaDinh.MaGiaDinhRieng
        //                                    FROM (((SELECT_GIADINH_LIST AS GiaDinh INNER JOIN ThanhVienGiaDinh ON GiaDinh.MaGiaDinh = ThanhVienGiaDinh.MaGiaDinh) 
        //                                    INNER JOIN GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan) LEFT JOIN GiaoDanHonPhoi ON GiaoDan.MaGiaoDan = GiaoDanHonPhoi.MaGiaoDan) 
        //                                    LEFT JOIN HonPhoi ON GiaoDanHonPhoi.MaHonPhoi = HonPhoi.MaHonPhoi
        //                                    {0}
        //                                    GROUP BY GiaDinh.MaGiaDinh, GiaDinh.TenGiaDinh, GiaDinh.MaGiaoHo, HonPhoi.SoHonPhoi, HonPhoi.NgayHonPhoi, HonPhoi.NoiHonPhoi, HonPhoi.LinhMucChung, HonPhoi.NguoiChung1, HonPhoi.NguoiChung2, HonPhoi.CachThucHonPhoi, GiaDinh.GhiChu, GiaDinh.DienThoai, GiaDinh.DiaChi, GiaDinh.DaXoa, GiaDinh.UpdateDate, GiaDinh.TenGiaoHo, GiaDinh.TenChong, GiaDinh.TenVo, GiaDinh.GiaDinhAo, GiaDinh.MaNhanDang, GiaDinh.MaGiaDinhRieng";

        /// <summary>
        /// Phải dùng string.Format nếu có điều kiện WHERE
        /// </summary>
        public const string SELECT_GIADINH_GIAODAN = @"SELECT * FROM SELECT_GIADINH_LIST WHERE MaGiaDinh IN 
                                                     (SELECT    DISTINCT    GiaDinh.MaGiaDinh
                                                      FROM      ((SELECT_GIADINH_LIST GiaDinh INNER JOIN
                                                                    ThanhVienGiaDinh ON GiaDinh.MaGiaDinh = ThanhVienGiaDinh.MaGiaDinh) INNER JOIN
                                                                    GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan)
                                                        {0}
                                                        )";

        public const string SELECT_GIAOPHAN = "SELECT * FROM GiaoPhan ";
        public const string SELECT_GIAOHAT = "SELECT * FROM GiaoHat ";

        public const string SELECT_CHUYENXU_THEOID = "SELECT * FROM ChuyenXu WHERE MaGiaoDan = ? ";
        public const string SELECT_CHUYENXU_LIST = "SELECT * FROM ChuyenXu WHERE 1 ";

        public const string SELECT_DOTBITICH_LIST = @"SELECT DotBiTich.*, SL.SoLuong
                                                    FROM DotBiTich
                                                    LEFT JOIN (
                                                            SELECT BiTichChiTiet.MaDotBiTich As MaDot, Count(BiTichChiTiet.MaGiaoDan) AS SoLuong
                                                            FROM BiTichChiTiet
                                                            GROUP BY BiTichChiTiet.MaDotBiTich) AS SL
                                                            ON DotBiTich.MaDotBiTich = SL.MaDot
                                                    WHERE 1 
                                                     ";
        public const string SELECT_BITICH_CHITIET_THEODOT = @"SELECT 1 as Existed, GiaoDan.*,
                                                    BiTichChiTiet.MaDotBiTich, BiTichChiTiet.GhiChu AS GhiChu1, BiTichChiTiet.VaiTro, BiTichChiTiet.TenGiaDinh,
                                                    MaGiaDinh AS MaGiaDinh, MaGiaDinh AS MaGiaDinhCo 
                                                    FROM GiaoDan INNER JOIN
                                                    (SELECT BiTichChiTiet.*,  ThanhVienGiaDinh.MaGiaDinh, ThanhVienGiaDinh.TenGiaDinh, ThanhVienGiaDinh.VaiTro 
                                                    FROM BiTichChiTiet LEFT JOIN 
                                                        (SELECT ThanhVienGiaDinh.*, GiaDinh.TenGiaDinh
                                                        FROM ThanhVienGiaDinh INNER JOIN GiaDinh ON ThanhVienGiaDinh.MaGiaDinh = GiaDinh.MaGiaDinh
                                                        WHERE VaiTro = 2) AS ThanhVienGiaDinh ON (BiTichChiTiet.MaGiaoDan = ThanhVienGiaDinh.MaGiaoDan)
                                                    ) AS BiTichChiTiet 
                                                    ON GiaoDan.MaGiaoDan = BiTichChiTiet.MaGiaoDan
                                                    WHERE GiaoDan.DaXoa=0 AND BiTichChiTiet.MaDotBiTich = ?  ";

        public const string SELECT_THANHVIEN_GIADINH_LIST = "SELECT ThanhVienGiaDinh.*, GiaDinh.TenGiaDinh FROM ThanhVienGiaDinh INNER JOIN GiaDinh ON ThanhVienGiaDinh.MaGiaDinh = GiaDinh.MaGiaDinh WHERE 1 ";
        public const string SELECT_BITICH_CHITIET_THEOLOAI = "SELECT BiTichChiTiet.*, DotBiTich.LoaiBiTich FROM DotBiTich INNER JOIN BiTichChiTiet ON DotBiTich.MaDotBiTich = BiTichChiTiet.MaDotBiTich WHERE 1 ";
        public const string SELECT_CHECK_VOCHONG = @"SELECT HP1.MaHonPhoi, HonPhoi.TenHonPhoi, GiaoDan.*
                                                    FROM ((GiaoDanHonPhoi HP1 
                                                    INNER JOIN GiaoDanHonPhoi HP2 ON (HP1.MaHonPhoi=HP2.MaHonPhoi AND HP1.MaGiaoDan <> HP2.MaGiaoDan))
                                                    INNER JOIN HonPhoi ON HP1.MaHonPhoi = HonPhoi.MaHonPhoi)
                                                    INNER JOIN GiaoDan ON GiaoDan.MaGiaoDan = HP2.MaGiaoDan
                                                    WHERE HP1.MaGiaoDan=? ";
        /// <summary>
        /// Select thong tin giao dan cua nguoi vo, nguoi chong dua vao ma gia dinh, vi vay phai input MaGiaDinh
        /// </summary>
        public const string SELECT_VOCHONG_THEO_MAGIADINH = "SELECT GiaoDan.*, ThanhVienGiaDinh.MaGiaDinh, ThanhVienGiaDinh.VaiTro FROM ThanhVienGiaDinh INNER JOIN GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan WHERE ThanhVienGiaDinh.MaGiaDinh=? ";

        public const string SELECT_THANHVIENGIADINH_GIAODAN = "SELECT GiaoDan.*, ThanhVienGiaDinh.MaGiaDinh, ThanhVienGiaDinh.VaiTro FROM ThanhVienGiaDinh INNER JOIN GiaoDan ON ThanhVienGiaDinh.MaGiaoDan = GiaoDan.MaGiaoDan WHERE 1 ";

        public const string SELECT_RAOHONPHOI_LIST = @"SELECT RaoHonPhoi.*, GiaoDan.TenTHanh & ' ' & GiaoDan.HoTen AS Nguoi1, GiaoDan_1.TenThanh & ' ' & GiaoDan_1.HoTen As Nguoi2
                                                            FROM (RaoHonPhoi INNER JOIN GiaoDan ON RaoHonPhoi.MaGiaoDan1 = GiaoDan.MaGiaoDan) 
                                                            INNER JOIN GiaoDan AS GiaoDan_1 ON RaoHonPhoi.MaGiaoDan2 = GiaoDan_1.MaGiaoDan 
                                                            WHERE 1 ";
        public const string DELETE_RAOHONPHOI = "DELETE FROM RaoHonPhoi WHERE MaRaoHonPhoi=? ";
        /// <summary>
        /// Can thong tin nguoi check trong cau WHERE MaGiaoDan1=? OR MaGiaoDan2=?
        /// </summary>
        public const string CHECK_DA_RAOHONPHOI = @"SELECT MaRaoHonPhoi FROM RaoHonPhoi WHERE MaGiaoDan1=? OR MaGiaoDan2=?";
        public const string SELECT_REPORT_RAOHONPHOI_LIST = @"SELECT RaoHonPhoiTMP.STT, RaoHonPhoiTMP.MaRaoHonPhoi, RaoHonPhoiTMP.TenRaoHonPhoi, 
                                                        RaoHonPhoiTMP.LanRao, RaoHonPhoiTMP.ThuocXu, RaoHonPhoiTMP.TruocOXu, GiaoDan.* 
                                                   FROM RaoHonPhoiTMP INNER JOIN GiaoDan ON RaoHonPhoiTMP.MaGiaoDan = GiaoDan.MaGiaoDan";
        public const string SELECT_VOCHONG_THEO_HONPHOI = @"SELECT GiaoDan.* 
                                                FROM GiaoDanHonPhoi 
                                                    INNER JOIN GiaoDan ON GiaoDanHonPhoi.MaGiaoDan=GiaoDan.MaGiaoDan 
                                                WHERE MaHonPhoi=?
                                                ORDER BY GiaoDan.Phai ";

        public const string SELECT_HONPHOI_THEO_ID = @"SELECT * FROM HonPhoi WHERE MaHonPhoi=? ";
        /// <summary>
        /// Select ra danh sach cac hon phoi cua nguoi nao do, nhung thong tin hon phoi se lap lai 2 lan, nen phai dung them dieu kien de loc ra trong tung truong hop cu the
        /// Vi du:
        /// - De loc ra danh sach hon phoi cua cung 1 nguoi ma khong bi trung nhau thi them: AND GiaoDanHonPhoi_1.MaGiaoDan != ? (cung ma giao dan cua 1 nguoi)
        /// - De loc ra thong tin hon phoi cua 2 nguoi thi them: AND GiaoDanHonPhoi_1.MaGiaoDan = ? (ma giao dan cua nguoi con lai)
        /// </summary>
        public const string SELECT_HONPHOI_THEO_MAGIAODAN = @"SELECT HP.*, GiaoDanHonPhoi.SoThuTu, GiaoDanHonPhoi_1.MaGiaoDan, GiaoDan.TenThanh + ' ' + GiaoDan.HoTen AS VoChong, GiaoDan.NgaySinh AS NgaySinhVoChong
                                                FROM ((HonPhoi AS HP INNER JOIN GiaoDanHonPhoi ON HP.MaHonPhoi = GiaoDanHonPhoi.MaHonPhoi) 
                                                    INNER JOIN GiaoDanHonPhoi AS GiaoDanHonPhoi_1 ON HP.MaHonPhoi = GiaoDanHonPhoi_1.MaHonPhoi) 
                                                    INNER JOIN GiaoDan ON GiaoDanHonPhoi_1.MaGiaoDan = GiaoDan.MaGiaoDan
                                                WHERE GiaoDanHonPhoi.[MaGiaoDan]=? ";
        public const string SELECT_HONPHOI_THEO_MAGIAODAN2 = @"SELECT HP.*, GiaoDanHonPhoi.SoThuTu, GiaoDanHonPhoi_1.MaGiaoDan, GiaoDan.TenThanh + ' ' + GiaoDan.HoTen AS VoChong, GiaoDan.NgaySinh AS NgaySinhVoChong,GiaoDan.QuaDoi
                                                FROM ((HonPhoi AS HP INNER JOIN GiaoDanHonPhoi ON HP.MaHonPhoi = GiaoDanHonPhoi.MaHonPhoi) 
                                                    INNER JOIN GiaoDanHonPhoi AS GiaoDanHonPhoi_1 ON HP.MaHonPhoi = GiaoDanHonPhoi_1.MaHonPhoi) 
                                                    INNER JOIN GiaoDan ON GiaoDanHonPhoi_1.MaGiaoDan = GiaoDan.MaGiaoDan
                                                WHERE GiaoDanHonPhoi.[MaGiaoDan]=? ";
        public const string SELECT_HONPHOI_CHECK_LIST = @"SELECT HP.*, GiaoDanHonPhoi.MaGiaoDan AS MaGiaoDan, GiaoDanHonPhoi_1.MaGiaoDan AS MaGiaoDan1
                                                FROM ((HonPhoi AS HP INNER JOIN GiaoDanHonPhoi ON HP.MaHonPhoi = GiaoDanHonPhoi.MaHonPhoi) 
                                                    INNER JOIN GiaoDanHonPhoi AS GiaoDanHonPhoi_1 ON HP.MaHonPhoi = GiaoDanHonPhoi_1.MaHonPhoi) 
                                                 WHERE GiaoDanHonPhoi.MaGiaoDan<>GiaoDanHonPhoi_1.MaGiaoDan ";

        public const string SELECT_HONPHOI_LIST = "SELECT * FROM SELECT_HONPHOI_LIST WHERE 1 ";

        public const string SELECT_CHECK_GIADINH_THEO_VOCHONG = @"SELECT A.MaGiaDinh, A.MaGiaoDan AS NguoiThu1,B.MaGiaoDan AS NguoiThu2 FROM
                                                ((SELECT * FROM ThanhVienGiaDinh WHERE  (VaiTro=0 OR VaiTro=1)) AS A
                                                INNER JOIN 
                                                (SELECT * FROM ThanhVienGiaDinh WHERE  (VaiTro=0 OR VaiTro=1)) AS B ON A.MaGiaDinh=B.MaGiaDinh
                                                )
                                                WHERE A.MaGiaoDan=? AND B.MaGiaoDan=?";
        public const string SELECT_TANHIEN_HIENTAI = @"SELECT * FROM TanHien WHERE DaHoiTuc=0 AND MaGiaoDan=?";
        //2018-08-23 Gia add start
        #region SELECT_SYN
        public const string SELECT_ALLBiTichChiTiet = @"Select * From BiTichChiTiet";

        public const string SELECT_ALLCauHinh = @"Select * From CauHinh";

        public const string SELECT_ALLChiTietHoiDoan = @"Select * From ChiTietHoiDoan";

        public const string SELECT_ALLChiTietLopGiaoLy = @"Select * From ChiTietLopGiaoLy";

        public const string SELECT_ALLChuyenXu = @"Select * From ChuyenXu";

        public const string SELECT_ALLDongBo = @"Select * From DongBo";

        public const string SELECT_ALLDotBiTich = @"Select * From DotBiTich";

        public const string SELECT_ALLDuLieuChung = @"Select * From DuLieuChung";

        public const string SELECT_ALLGiaDinh = @"Select * From GiaDinh";

        public const string SELECT_ALLGiaoDan = @"Select * From GiaoDan";

        public const string SELECT_ALLGiaoDanHonPhoi = @"Select * From GiaoDanHonPhoi";

        public const string SELECT_ALLGiaoHat = @"Select * From GiaoHat";

        public const string SELECT_ALLGiaoHo = @"Select * From GiaoHo";

        public const string SELECT_ALLGiaoLyVien = @"Select * From GiaoLyVien";

        public const string SELECT_ALLGiaoPhan = @"Select * From GiaoPhan";

        public const string SELECT_ALLGiaoXu = @"Select * From GiaoXu";

        public const string SELECT_ALLHonPhoi = @"Select * From HonPhoi";

        public const string SELECT_ALLHoiDoan = @"Select * From HoiDoan";

        public const string SELECT_ALLKhoiGiaoLy = @"Select * From KhoiGiaoLy";

        public const string SELECT_ALLLinhMuc = @"Select * From LinhMuc";

        public const string SELECT_ALLLopGiaoLy = @"Select * From LopGiaoLy";

        public const string SELECT_ALLRaoHonPhoi = @"Select * From RaoHonPhoi";

        public const string SELECT_ALLTaiKhoan = @"Select * From TaiKhoan";

        public const string SELECT_ALLTanHien = @"Select * From TanHien";

        public const string SELECT_ALLTenLoaiTaiKhoan = @"Select * From TenLoaiTaiKhoan";

        public const string SELECT_ALLThanhVienGiaDinh = @"Select * From ThanhVienGiaDinh";

        public const string SELECT_ALLVaiTro = @"Select * From VaiTro";

        #endregion

        //2018-08-23 Gia add end

        //2018-08-26 Gia add start
        public const string UPDATE_GiaoDan_SET = @"UPDATE GiaoDan SET UpdateDate=?,";//frm Bi tich Chi tiet

        public const string UPDATE_CAUHINH = @"UPDATE CauHinh SET {0}='{1}',UpdateDate=? WHERE {2}='{3}";

        public const string UPDATE_LOPGIAOLY = @"update ChiTietLopGiaoLy set hoanthanh = ?, ghichugly = ?,UpdateDate=? where MaLop = ? and MaGiaoDan = ?";

        public const string UPDATE_THANHVIENGIADINH = @"UPDATE ThanhVienGiaDinh SET MaGiaoDan=?,UpdateDate=?  WHERE MaGiaoDan=?";

        public const string UPDATE_BTCT = @"UPDATE BiTichChiTiet SET MaGiaoDan=?,UpdateDate=?  WHERE MaGiaoDan=?";

        public const string UPDATE_CX = @"UPDATE ChuyenXu SET MaGiaoDan=?,UpdateDate=? WHERE MaGiaoDan=?";

        //2018-08-26 Gia add end
        public const string INSERT_GIAO_PHAN = @"INSERT INTO GiaoPhan(MaGiaoPhan,TenGiaoPhan,MaGiaoPhanRieng) VALUES(1,?,?)";
        public const string INSERT_GIAO_HAT = @"INSERT INTO GiaoHat(MaGiaoHat,TenGiaoHat,MaGiaoHatRieng,MaGiaoPhan) VALUES(1,?,?,1)";
        public const string INSERT_GIAO_XU = @"INSERT INTO GiaoXu(TenGiaoXu,DiaChi,DienThoai,Website,Hinh,MaGiaoXuRieng,Email,MaGiaoHat) VALUES(?,?,?,?,?,?,?,1)";
        public const string INSERT_TVGD = @"INSERT INTO ThanhVienGiaDinh(MaGiaDinh,MaGiaoDan,VaiTro,ChuHo,UpdateDate) VALUES(?, ?, ?, ?,?)";
        public const string INSERT_DONGBO = @"INSERT INTO DONGBO(TenBang,MaIDMayKhach,MaIDMayChu,UpdateDate) VALUES(?, ?, ?, ?)";

        public const string UPDATE_BEGINPULLDATE = @"UPDATE giaoxu set BeginPullDate=?,UpdateDate=?";
    }
}
