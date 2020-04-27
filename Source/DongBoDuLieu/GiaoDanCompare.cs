using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanCompare:CompareMasterTable
    {
        int newIDMayKhach = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, true);
        public GiaoDanCompare(string dir, string nameCSV):base(dir,nameCSV)
        {
        }
        public override void ExProcessData()
        {
            KhoaChinh = GiaoDanConst.MaGiaoDan;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai= GiaoHoConst.MaGiaoHo;
            TableNameKhoaNgoai = GiaoHoConst.TableName;
            ProcessData();
        }
    }
}
