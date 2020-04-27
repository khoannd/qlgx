using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class HonPhoiCompare :CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(HonPhoiConst.TableName, HonPhoiConst.MaHonPhoi, true);
        public HonPhoiCompare(string dir, string nameCSV):base(dir,nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = HonPhoiConst.MaHonPhoi;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
