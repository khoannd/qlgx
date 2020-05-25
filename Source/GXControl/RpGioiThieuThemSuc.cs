using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GxControl
{
    class RpGioiThieuThemSuc : RpGioiThieuBase
    {
        public override int SetReplace(DataTable tblGiaoXuNhan, DataTable tblGiaoXu, DataTable tblGiaoDan)
        {
           try
            {
                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];
                word.Replace(GiaoXuNhanConst.TenGiaoPhan2, rowGiaoXuNhan[GiaoXuNhanConst.TenGiaoPhan2]);
                word.Replace(GiaoXuNhanConst.TenGiaoXu2, rowGiaoXuNhan[GiaoXuNhanConst.TenGiaoXu2]);
                word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                word.Replace(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.HoTen]);
                word.Replace(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh]);
                word.Replace(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh]);
                word.Replace(GiaoDanConst.HoTenCha, rowGiaoDan[GiaoDanConst.HoTenCha]);
                word.Replace(GiaoDanConst.HoTenMe, rowGiaoDan[GiaoDanConst.HoTenMe]);
                word.Replace("HoTenLinhMuc", TenLinhMuc);
                if (string.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Trim()))
                {
                    word.Replace(GiaoDanConst.NgayRuaToi, "...................");
                }
                else
                word.Replace(GiaoDanConst.NgayRuaToi, rowGiaoDan[GiaoDanConst.NgayRuaToi]);
                word.Replace(GiaoDanConst.NoiRuaToi, rowGiaoDan[GiaoDanConst.NoiRuaToi]);
            }
            catch(Exception ex)
            {
                return -1;
            }
            return 1;
        }
    }
}
