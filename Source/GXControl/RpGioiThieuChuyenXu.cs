using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
namespace GxControl
{
    class RpGioiThieuChuyenXu : RpGioiThieuBase
    {
        private Dictionary<int, string> vaiTro;
        public RpGioiThieuChuyenXu()
        {
            vaiTro = Memory.GetQuanHeList(true);
        }
        public override int SetReplace(DataTable tblGiaoXuNhan, DataTable tblGiaoXu, DataTable tblGiaoDan)
        {
            try
            {
                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];
                word.Replace(GiaoXuNhanConst.TenGiaoPhan2, rowGiaoXuNhan[GiaoXuNhanConst.TenGiaoPhan2]);
                word.Replace(GiaoXuNhanConst.TenGiaoXu2, rowGiaoXuNhan[GiaoXuNhanConst.TenGiaoXu2]);
                word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                word.Replace("FullNameLinhMuc", TenLinhMuc);
                //Chu ho
                var chuHoRow = GetChuHo(tblGiaoDan);
                word.Replace(RpChuyenGiaoXuConst.FullTenChuHo, string.Concat(chuHoRow[GiaoDanConst.TenThanh], " ", chuHoRow[GiaoDanConst.HoTen]));
                word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, 0),"Chủ hộ");
                SetTableFimaly(tblGiaoDan);

            }
            catch (Exception ex)
            {
                return -1;
            }
            return 1;
        }
        public DataRow GetChuHo(DataTable tbl)
        {
            foreach(var row in tbl.Rows)
            {
                var dataRow = (row as DataRow);
                if (dataRow[ThanhVienGiaDinhConst.ChuHo].Equals(true)) return dataRow;
            }
            return null;
        }
        public void SetTableFimaly(DataTable tbl)
        {
        
            for(int i = 0;i<7;i++)
            {
                if (i < tbl.Rows.Count)
                {
                    var dataRow = (tbl.Rows[i] as DataRow);
                    word.Replace(GetName(GiaoDanConst.TenThanh, i), dataRow[GiaoDanConst.TenThanh]);
                    word.Replace(GetName(GiaoDanConst.HoTen, i), dataRow[GiaoDanConst.HoTen]);
                    word.Replace(GetName(GiaoDanConst.NgaySinh, i), dataRow[GiaoDanConst.NgaySinh]);
                    word.Replace(GetName(GiaoDanConst.NoiSinh, i), dataRow[GiaoDanConst.NoiSinh]);
                    word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, i), vaiTro[(int)dataRow[ThanhVienGiaDinhConst.VaiTro]]);
                }
                else
                {
                    word.Replace(GetName(GiaoDanConst.TenThanh, i), "");
                    word.Replace(GetName(GiaoDanConst.HoTen, i), "");
                    word.Replace(GetName(GiaoDanConst.NgaySinh, i), "");
                    word.Replace(GetName(GiaoDanConst.NoiSinh, i), "");
                    word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, i), "");
                }
              
            }
        }
        public string GetName(string constans,int number)
        {
            return string.Concat(constans, number);
        }
    }
}
