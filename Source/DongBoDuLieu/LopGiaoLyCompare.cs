using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class LopGiaoLyCompare : CompareMasterTable
    {
        private DataTable tblLopGiaoLy;
        private int newIDMayKhach = Memory.Instance.GetNextId(LopGiaoLyConst.TableName, LopGiaoLyConst.MaLop, true);
        public LopGiaoLyCompare(string dir, string nameCSV,DataTable tblLopGiaoLy):base(dir,nameCSV)
        {
            this.tblLopGiaoLy = tblLopGiaoLy;
        }

        public override void ExProcessData()
        {
            Tbl = tblLopGiaoLy;
            KhoaChinh = LopGiaoLyConst.MaLop;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = KhoiGiaoLyConst.MaKhoi;
            TableNameKhoaNgoai = KhoiGiaoLyConst.TableName;
            ProcessData();
        }
    }
}
