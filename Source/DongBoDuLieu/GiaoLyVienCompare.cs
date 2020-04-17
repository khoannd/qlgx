using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoLyVienCompare:CompareRelationTable
    {
        DataTable tblGiaoLyVien;
        public GiaoLyVienCompare(string dir, string nameCSV,DataTable tblGiaoLyVien):base(dir,nameCSV)
        {
            this.tblGiaoLyVien = tblGiaoLyVien;
        }

        public override void ExProcessData()
        {
            Tbl = tblGiaoLyVien;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = LopGiaoLyConst.MaLop;
            TableNameKhoaChinh2 = LopGiaoLyConst.TableName;
            ProcessData();
        }
    }
}
