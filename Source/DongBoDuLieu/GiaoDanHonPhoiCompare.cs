using GxGlobal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanHonPhoiCompare : CompareRelationTable
    {
        public GiaoDanHonPhoiCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        private List<Dictionary<string, string>> listTracksHonPhoi;
        private List<Dictionary<string, string>> listTracksGiaoDan;
        public override void deleteObjectRelation()
        {
            deleteObjecChild(ListTracks, GiaoDanHonPhoiConst.MaHonPhoi, GiaoDanHonPhoiConst.MaGiaoDan, GiaoDanHonPhoiConst.TableName);

        }

        public override void importCacObject()
        {
            foreach (var objectTrack in listTracksHonPhoi)
            {
                importObjectRelation(objectTrack, HonPhoiConst.MaHonPhoi, GiaoDanConst.MaGiaoDan, listTracksGiaoDan, GiaoDanHonPhoiConst.TableName);
            }
        }
        public void getListTracksHonPhoi(List<Dictionary<string, string>> honPhoiTracks)
        {
            listTracksHonPhoi = honPhoiTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, string>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }
    }
}
