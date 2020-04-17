using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class ChiTietHoiDoanCompare : CompareRelationTable
    {
        private DataTable tblCTHD;
        public ChiTietHoiDoanCompare(string dir, string nameCSV,DataTable tblCTHD) : base(dir, nameCSV)
        {
            this.tblCTHD = tblCTHD;
        }

        public override void ExProcessData()
        {
            Tbl = tblCTHD;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = HoiDoanConst.MaHoiDoan;
            TableNameKhoaChinh2 = HoiDoanConst.TableName;
            ProcessData();
        }
    }
}
