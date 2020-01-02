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
                if(chuHoRow != null)
                {
                    word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, 0), "Chủ hộ");
                }
                else
                {
                    chuHoRow = tblGiaoDan.Rows[0];
                }
                word.Replace(RpChuyenGiaoXuConst.FullTenChuHo, string.Concat(chuHoRow[GiaoDanConst.TenThanh], " ", chuHoRow[GiaoDanConst.HoTen]));

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
            //hiepdv begin add
            const int ROW_MAX_TEMPLATE = 7;
            int maxRow = tbl.Rows.Count;
            //xóa hàng khi số lượng thành viên ít hơn row max template
            if (ROW_MAX_TEMPLATE > maxRow)
            {
                for (int j = ROW_MAX_TEMPLATE; j > maxRow; j--)
                {
                    word.DeleteRow(1, j + 1);
                }
                word.FormatTable(1, Microsoft.Office.Interop.Word.WdPaperSize.wdPaperA4);
            }

            //check số thành viên cần in
            if (maxRow > ROW_MAX_TEMPLATE)
            {
                //Tiến hành add thêm row. số row cần add = maxrow - row_holder
                word.AddRowColumnFirstSTT(maxRow - ROW_MAX_TEMPLATE, 1,5);
                word.FormatTable(1, Microsoft.Office.Interop.Word.WdPaperSize.wdPaperA4);
            }
            //hiepdv end add

            for (int i = 0;i< maxRow; i++)
            {
                //if (i < maxRow)
                //{
                    var dataRow = (tbl.Rows[i] as DataRow);
                    word.Replace(GetName(GiaoDanConst.TenThanh, i), dataRow[GiaoDanConst.TenThanh]);
                    word.Replace(GetName(GiaoDanConst.HoTen, i), dataRow[GiaoDanConst.HoTen]);
                    word.Replace(GetName(GiaoDanConst.NgaySinh, i), dataRow[GiaoDanConst.NgaySinh]);
                    word.Replace(GetName(GiaoDanConst.NoiSinh, i), dataRow[GiaoDanConst.NoiSinh]);
                    word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, i), vaiTro[(int)dataRow[ThanhVienGiaDinhConst.VaiTro]]);
                    
                //}
                //else
                //{
                //    word.Replace(GetName(GiaoDanConst.TenThanh, i), "");
                //    word.Replace(GetName(GiaoDanConst.HoTen, i), "");
                //    word.Replace(GetName(GiaoDanConst.NgaySinh, i), "");
                //    word.Replace(GetName(GiaoDanConst.NoiSinh, i), "");
                //    word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, i), "");
                //}
              
            }
        }
        public string GetName(string constans,int number)
        {
            return string.Concat(constans, number);
        }
    }
}
