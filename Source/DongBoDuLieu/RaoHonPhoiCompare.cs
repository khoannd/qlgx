using System;
using GxGlobal;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class RaoHonPhoiCompare : CompareMasterTable
    {
        private DataTable tblRaoHonPhoi;
        private int newIDMayKhach = Memory.Instance.GetNextId(RaoHonPhoiConst.TableName, RaoHonPhoiConst.MaRaoHonPhoi, true);
        public RaoHonPhoiCompare(string dir, string nameCSV, DataTable tblRaoHonPhoi) : base(dir, nameCSV)
        {
            this.tblRaoHonPhoi = tblRaoHonPhoi;
        }

        public override void ExProcessData()
        {
            Tbl = tblRaoHonPhoi;
            KhoaChinh = RaoHonPhoiConst.MaRaoHonPhoi;
            NewIDMayKhach = newIDMayKhach;
            KhoaNgoai = RaoHonPhoiConst.MaGiaoDan1;
            TableNameKhoaNgoai = GiaoDanConst.TableName;
            KhoaNgoai2 = RaoHonPhoiConst.MaGiaoDan2;
            TableNameKhoaNgoai2 = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
