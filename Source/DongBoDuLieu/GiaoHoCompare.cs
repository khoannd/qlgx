using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GxGlobal;

namespace DongBoDuLieu
{
    class GiaoHoCompare : CompareMasterTable
    {
        private DataTable tblGiaoHo;
        int newIDMayKhach = Memory.Instance.GetNextId(GiaoHoConst.TableName, GiaoHoConst.MaGiaoHo, true);
        public GiaoHoCompare(string dir, string nameCSV,DataTable tblGiaoHo) : base(dir, nameCSV)
        {
            this.tblGiaoHo = tblGiaoHo;
        }
        public override void ExProcessData()
        {
            Tbl = tblGiaoHo;
            KhoaChinh = GiaoHoConst.MaGiaoHo;
            NewIDMayKhach = newIDMayKhach;
            ProcessData();
            //chưa làm
            //processMaGiaoHoCha();
        }
        private void processMaGiaoHoCha()
        {
            //if (Data.Count > 0)
            //{
            //    foreach (var item in Data)
            //    {
            //        if (item["DeleteSV"].ToString()=="1")
            //        {
            //            continue;
            //        }
            //        if (!string.IsNullOrEmpty(item["MaGiaoHoCha"].ToString()))
            //        {
            //            int idGiaoHoCha = findIdObjectClient(ListTracks, item["MaGiaoHoCha"]);
            //            if (idGiaoHoCha != 0)
            //            {
            //                int idGiaoHo = findIdObjectClient(ListTracks, item["MaGiaoHo"]);
            //                Memory.ExecuteSqlCommand(@"Update GiaoHo Set MaGiaoHoCha=? Where MaGiaoHo=?", idGiaoHoCha, idGiaoHo);
            //                Memory.ClearError();
            //            }
            //        }
            //    }
            //}
        }
    }
}
