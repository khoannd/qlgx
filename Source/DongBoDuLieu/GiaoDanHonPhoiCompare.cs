using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanHonPhoiCompare:CompareRelationTable
    {
        DataTable tblGDHP;
        public GiaoDanHonPhoiCompare(string dir, string nameCSV,DataTable tblGDHP):base(dir,nameCSV)
        {
            this.tblGDHP = tblGDHP;
        }

        public override void ExProcessData()
        {
            Tbl = tblGDHP;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = HonPhoiConst.MaHonPhoi;
            TableNameKhoaChinh2 = HonPhoiConst.TableName;
            ProcessData();
        }
    }
}
