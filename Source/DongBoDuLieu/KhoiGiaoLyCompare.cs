using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class KhoiGiaoLyCompare:CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(KhoiGiaoLyConst.TableName, KhoiGiaoLyConst.MaKhoi, true);
        public KhoiGiaoLyCompare(string dir, string nameCSV):base(dir,nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = KhoiGiaoLyConst.MaKhoi;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = KhoiGiaoLyConst.NguoiQuanLy;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
