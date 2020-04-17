using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class ThanhVienGiaDinhCompare : CompareRelationTable
    {
        DataTable tblTVGD;
        public ThanhVienGiaDinhCompare(string dir, string nameCSV, DataTable tblTVGD) : base(dir, nameCSV)
        {
            this.tblTVGD = tblTVGD;
        }

        public override void ExProcessData()
        {
            Tbl = tblTVGD;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = GiaDinhConst.MaGiaDinh;
            TableNameKhoaChinh2 = GiaDinhConst.TableName;
            ProcessData();
        }

    }
}
