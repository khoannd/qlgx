using GxGlobal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoLyVienCompare : CompareRelationTable
    {
        public GiaoLyVienCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        private List<Dictionary<string, object>> listTracksLopGiaoLy;
        private List<Dictionary<string, object>> listTracksGiaoDan;

        public override void importCacObject()
        {
            foreach (var objectTrack in listTracksLopGiaoLy)
            {
                importObjectRelation(objectTrack, LopGiaoLyConst.MaLop, GiaoDanConst.MaGiaoDan, listTracksGiaoDan, GiaoLyVienConst.TableName);
            }
        }
        public void getListTracksLopGiaoLy(List<Dictionary<string, object>> lopGiaoLyTracks)
        {
            listTracksLopGiaoLy = lopGiaoLyTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, object>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }

        //public override void deleteObjectRelation()
        //{
        //    deleteObjecChild(ListTracks, GiaoLyVienConst.MaLop, GiaoLyVienConst.MaGiaoDan, GiaoLyVienConst.TableName);
        //}
    }
}
