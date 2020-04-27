using System;
using GxGlobal;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public class RaoHonPhoiCompare : CompareMasterTable
    {
        private int newIDMayKhach = Memory.Instance.GetNextId(RaoHonPhoiConst.TableName, RaoHonPhoiConst.MaRaoHonPhoi, true);
        public RaoHonPhoiCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
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
