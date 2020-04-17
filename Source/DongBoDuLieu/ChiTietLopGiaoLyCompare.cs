using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class ChiTietLopGiaoLyCompare:CompareRelationTable 
    {
        private DataTable tblCTLGL;
        public ChiTietLopGiaoLyCompare(string dir, string nameCSV, DataTable tblCTLGL) :base(dir,nameCSV)
        {
            this.tblCTLGL = tblCTLGL;
        }

        public override void ExProcessData()
        {
            Tbl = tblCTLGL;
            KhoaChinh1 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaChinh1 = GiaoDanConst.TableName;
            KhoaChinh2 = LopGiaoLyConst.MaLop;
            TableNameKhoaChinh2 = LopGiaoLyConst.TableName;
            ProcessData();
        }
    }
}
