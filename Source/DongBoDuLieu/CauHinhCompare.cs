using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class CauHinhCompare : CompareKeyStringTable
    {
        private DataTable tblCauHinh;
        public CauHinhCompare(string dir, string nameCSV, DataTable tblCauHinh) : base(dir, nameCSV)
        {
            this.tblCauHinh = tblCauHinh;
        }

        public override void ExProcessData()
        {
            Tbl = tblCauHinh;
            KhoaChinh = CauHinhConst.MaCauHinh;
            ProcessData();
        }

    }
}
