using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class KhoiGiaoLyCompare:CompareMasterTable
    {
        private DataTable tblKhoiGiaoLy;
        private int newIDMayKhach = Memory.Instance.GetNextId(KhoiGiaoLyConst.TableName, KhoiGiaoLyConst.MaKhoi, true);
        public KhoiGiaoLyCompare(string dir, string nameCSV,DataTable tblKhoiGiaoLy):base(dir,nameCSV)
        {
            this.tblKhoiGiaoLy = tblKhoiGiaoLy;
        }

        public override void ExProcessData()
        {
            Tbl = tblKhoiGiaoLy;
            KhoaChinh = KhoiGiaoLyConst.MaKhoi;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = KhoiGiaoLyConst.NguoiQuanLy;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
