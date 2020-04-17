using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class ChuyenXuCompare : CompareMasterTable
    {
        private DataTable tblChuyenXu;
        private int newIDMayKhach = Memory.Instance.GetNextId(ChuyenXuConst.TableName, ChuyenXuConst.MaChuyenXu, true);
        public ChuyenXuCompare(string dir, string nameCSV, DataTable tblChuyenXu) : base(dir, nameCSV)
        {
            this.tblChuyenXu = tblChuyenXu;
        }

        public override void ExProcessData()
        {
            Tbl = tblChuyenXu;
            KhoaChinh = ChuyenXuConst.MaChuyenXu;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoDanConst.MaGiaoDan;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
