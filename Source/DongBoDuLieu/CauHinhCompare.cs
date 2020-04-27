using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class CauHinhCompare : CompareKeyStringTable
    {
        public CauHinhCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = CauHinhConst.MaCauHinh;
            ProcessData();
        }

    }
}
