using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanCompare:CompareMasterTable
    {
        private DataTable tblGiaoDan;
        int newIDMayKhach = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, true);
        public GiaoDanCompare(string dir, string nameCSV,DataTable tblGiaoDan):base(dir,nameCSV)
        {
            this.tblGiaoDan = tblGiaoDan;
        }
        public override void ExProcessData()
        {
            Tbl = tblGiaoDan;
            KhoaChinh = GiaoDanConst.MaGiaoDan;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai= GiaoHoConst.MaGiaoHo;
            TableNameKhoaNgoai = GiaoHoConst.TableName;
            ProcessData();
        }
    }
}
