using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class LinhMucCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(LinhMucConst.TableName, LinhMucConst.MaLinhMuc, true);
        public LinhMucCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = LinhMucConst.MaLinhMuc;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
