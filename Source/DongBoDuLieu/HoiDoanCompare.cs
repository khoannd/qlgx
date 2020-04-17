using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class HoiDoanCompare : CompareMasterTable
    {
        private DataTable tblHoiDoan;
        private int newIDMayKhach = Memory.Instance.GetNextId(HoiDoanConst.TableName, HoiDoanConst.MaHoiDoan, true);
        public HoiDoanCompare(string dir, string nameCSV, DataTable tblHoiDoan) : base(dir, nameCSV)
        {
            this.tblHoiDoan = tblHoiDoan;
        }

        public override void ExProcessData()
        {
            Tbl = tblHoiDoan;
            KhoaChinh = HoiDoanConst.MaHoiDoan;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
