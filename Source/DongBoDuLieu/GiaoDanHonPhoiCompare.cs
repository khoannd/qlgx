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
        private List<Dictionary<string, object>> listTracksHonPhoi;
        private List<Dictionary<string, object>> listTracksGiaoDan;
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
        public void getListTracksHonPhoi(List<Dictionary<string, object>> honPhoiTracks)
        {
            listTracksHonPhoi = honPhoiTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, object>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }
    }
}
