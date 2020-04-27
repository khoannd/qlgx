using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class HoiDoanCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(HoiDoanConst.TableName, HoiDoanConst.MaHoiDoan, true);
        public HoiDoanCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = HoiDoanConst.MaHoiDoan;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
