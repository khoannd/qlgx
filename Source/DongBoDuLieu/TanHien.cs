using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class TanHienCompare : CompareMasterTable
    {
        private DataTable tblTanHien;
        private int newIDMayKhach = Memory.Instance.GetNextId(TanHienConst.TableName, TanHienConst.MaTanHien, true);
        public TanHienCompare(string dir, string nameCSV, DataTable tblTanHien) : base(dir, nameCSV)
        {
            this.tblTanHien = tblTanHien;
        }

        public override void ExProcessData()
        {
            Tbl = tblTanHien;
            KhoaChinh = TanHienConst.MaTanHien;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoDanConst.MaGiaoDan;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
