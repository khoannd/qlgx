using GxGlobal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongBoDuLieu
{
    class BiTichChiTietCompare : CompareRelationTable
    {
        public BiTichChiTietCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        private List<Dictionary<string, string>> listTracksDotBiTich;
        private List<Dictionary<string, string>> listTracksGiaoDan;

        public override void importCacObject()
        {
            foreach (var objectTrack in listTracksDotBiTich)
            {
                importObjectRelation(objectTrack, DotBiTichConst.MaDotBiTich, GiaoDanConst.MaGiaoDan, listTracksGiaoDan, BiTichChiTietConst.TableName);
            }
        }
        public void getlistTracksDotBiTich(List<Dictionary<string, string>> dotBiTichTracks)
        {
            listTracksDotBiTich = dotBiTichTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, string>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }

        public override void deleteObjectRelation()
        {
            deleteObjecChild(ListTracks, BiTichChiTietConst.MaDotBiTich, BiTichChiTietConst.MaGiaoDan, BiTichChiTietConst.TableName);
        }
    }
}
