using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class ChuyenXuCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(ChuyenXuConst.TableName, ChuyenXuConst.MaChuyenXu, true);
        public ChuyenXuCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = ChuyenXuConst.MaChuyenXu;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoDanConst.MaGiaoDan;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
