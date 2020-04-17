using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaDinhCompare:CompareMasterTable
    {
        private DataTable tblGiaDinh;
        private int newIDMayKhach = Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, true);
        public GiaDinhCompare(string dir, string nameCSV,DataTable tblGiaDinh):base(dir,nameCSV)
        {
            this.tblGiaDinh = tblGiaDinh;
        }

        public override void ExProcessData()
        {
            Tbl = tblGiaDinh;
            KhoaChinh = GiaDinhConst.MaGiaDinh;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = GiaoHoConst.MaGiaoHo;
            TableNameKhoaNgoai = GiaoHoConst.TableName;
            ProcessData();
        }
    }
}
