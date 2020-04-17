using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class BiTichChiTietCompare : CompareRelationTable
    {
        DataTable tblBiTichChiTiet;
        public BiTichChiTietCompare(string dir, string nameCSV,DataTable tblBiTichChiTiet) : base(dir, nameCSV)
        {
            this.tblBiTichChiTiet = tblBiTichChiTiet;
        }

        public override void ExProcessData()
        {
            Tbl = tblBiTichChiTiet;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = DotBiTichConst.MaDotBiTich;
            TableNameKhoaChinh2 = DotBiTichConst.TableName;
            ProcessData();
        }
    }
}
