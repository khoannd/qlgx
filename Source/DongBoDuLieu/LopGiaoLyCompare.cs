using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class LopGiaoLyCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(LopGiaoLyConst.TableName, LopGiaoLyConst.MaLop, true);
        public LopGiaoLyCompare(string dir, string nameCSV):base(dir,nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = LopGiaoLyConst.MaLop;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = KhoiGiaoLyConst.MaKhoi;
            TableNameKhoaNgoai = KhoiGiaoLyConst.TableName;
            ProcessData();
        }
    }
}
