using GxGlobal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongBoDuLieu
{
    class ChiTietLopGiaoLyCompare : CompareRelationTable
    {
        public ChiTietLopGiaoLyCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {

        }
        private List<Dictionary<string, string>> listTracksLopGiaoLy;
        private List<Dictionary<string, string>> listTracksGiaoDan;

        public override void importCacObject()
        {
            foreach (var objectTrack in listTracksLopGiaoLy)
            {
                importObjectRelation(objectTrack, LopGiaoLyConst.MaLop, ChiTietLopGiaoLyConst.MaGiaoDan, listTracksGiaoDan, ChiTietLopGiaoLyConst.TableName);
            }
        }
        public void getListTracksLopGiaoLy(List<Dictionary<string, string>> lopGiaoLyTracks)
        {
            listTracksLopGiaoLy = lopGiaoLyTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, string>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }

        public override void deleteObjectRelation()
        {
            deleteObjecChild(ListTracks, ChiTietLopGiaoLyConst.MaLop, ChiTietLopGiaoLyConst.MaGiaoDan, ChiTietLopGiaoLyConst.TableName);
        }
    }
}
