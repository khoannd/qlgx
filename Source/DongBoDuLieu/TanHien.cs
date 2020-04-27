using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class TanHienCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(TanHienConst.TableName, TanHienConst.MaTanHien, true);
        public TanHienCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = TanHienConst.MaTanHien;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoDanConst.MaGiaoDan;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
