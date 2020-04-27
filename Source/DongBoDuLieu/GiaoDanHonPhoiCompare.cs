using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanHonPhoiCompare:CompareRelationTable
    {
        public GiaoDanHonPhoiCompare(string dir, string nameCSV ):base(dir,nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = HonPhoiConst.MaHonPhoi;
            TableNameKhoaChinh2 = HonPhoiConst.TableName;
            ProcessData();
        }
    }
}
