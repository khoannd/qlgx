using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaDinhCompare:CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, true);
        public GiaDinhCompare(string dir, string nameCSV):base(dir,nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = GiaDinhConst.MaGiaDinh;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoHoConst.MaGiaoHo;
            TableNameKhoaNgoai = GiaoHoConst.TableName;
            ProcessData();
        }
    }
}
