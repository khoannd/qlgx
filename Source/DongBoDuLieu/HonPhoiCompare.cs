using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class HonPhoiCompare :CompareMasterTable
    {
        private DataTable tblHonPhoi;
        private int newIDMayKhach = Memory.Instance.GetNextId(HonPhoiConst.TableName, HonPhoiConst.MaHonPhoi, true);
        public HonPhoiCompare(string dir, string nameCSV,DataTable tblHonPhoi):base(dir,nameCSV)
        {
            this.tblHonPhoi = tblHonPhoi;
        }

        public override void ExProcessData()
        {
            Tbl = tblHonPhoi;
            KhoaChinh = HonPhoiConst.MaHonPhoi;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
