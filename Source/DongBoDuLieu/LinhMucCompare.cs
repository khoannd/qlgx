using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class LinhMucCompare : CompareMasterTable
    {
        private DataTable tblLinhMuc;
        private int newIDMayKhach = Memory.Instance.GetNextId(LinhMucConst.TableName, LinhMucConst.MaLinhMuc, true);
        public LinhMucCompare(string dir, string nameCSV, DataTable tblLinhMuc) : base(dir, nameCSV)
        {
            this.tblLinhMuc = tblLinhMuc;
        }

        public override void ExProcessData()
        {
            Tbl = tblLinhMuc;
            KhoaChinh = LinhMucConst.MaLinhMuc;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
        }
    }
}
