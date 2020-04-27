using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class BiTichChiTietCompare : CompareRelationTable
    {
        public BiTichChiTietCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = DotBiTichConst.MaDotBiTich;
            TableNameKhoaChinh2 = DotBiTichConst.TableName;
            ProcessData();
        }
    }
}
