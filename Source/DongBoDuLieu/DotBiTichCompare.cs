using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class DotBiTichCompare : CompareMasterTable
    {
        int newIDMayKhach = Memory.Instance.GetNextId(DotBiTichConst.TableName, DotBiTichConst.MaDotBiTich, true);
        public DotBiTichCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public override void ExProcessData()
        {
            KhoaChinh = BiTichChiTietConst.MaDotBiTich;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
