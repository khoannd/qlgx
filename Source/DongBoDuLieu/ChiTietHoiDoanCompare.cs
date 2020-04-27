using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace DongBoDuLieu
{
    public class ChiTietHoiDoanCompare : CompareMasterTable
    {
        public ChiTietHoiDoanCompare(string dir, string nameCSV ) : base(dir, nameCSV)
        {
        }

        public override void ExProcessData()
        {
            KhoaChinh = ChiTietHoiDoanConst.ID;
            NewIDMayKhach = -1;
            KhoaNgoai = HoiDoanConst.MaHoiDoan;
            TableNameKhoaNgoai = HoiDoanConst.TableName;
            KhoaNgoai2 = GiaoDanConst.MaGiaoDan;
            TableNameKhoaNgoai2 = GiaoDanConst.TableName;
            ProcessData();
        }
    }
}
