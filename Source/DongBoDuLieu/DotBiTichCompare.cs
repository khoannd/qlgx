using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class DotBiTichCompare : CompareMasterTable
    {
        private DataTable tblDotBiTich;
        int newIDMayKhach = Memory.Instance.GetNextId(DotBiTichConst.TableName, DotBiTichConst.MaDotBiTich, true);
        public DotBiTichCompare(string dir, string nameCSV, DataTable tblDotBiTich) : base(dir, nameCSV)
        {
            this.tblDotBiTich = tblDotBiTich;
        }
        public override void ExProcessData()
        {
            Tbl = tblDotBiTich;
            KhoaChinh = BiTichChiTietConst.MaDotBiTich;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
