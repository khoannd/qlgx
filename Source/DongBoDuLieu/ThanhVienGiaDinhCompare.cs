using GxGlobal;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongBoDuLieu
{
    class ThanhVienGiaDinhCompare : CompareRelationTable
    {
        public ThanhVienGiaDinhCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        private List<Dictionary<string, object>> listTracksGiaDinh;
        private List<Dictionary<string, object>> listTracksGiaoDan;

        public override void importCacObject()
        {
            foreach (var objectTrack in listTracksGiaDinh)
            {
                importObjectRelation(objectTrack, GiaDinhConst.MaGiaDinh, GiaoDanConst.MaGiaoDan,listTracksGiaoDan ,ThanhVienGiaDinhConst.TableName);
            }
        }
        public void getListTracksGiaDinh(List<Dictionary<string, object>> giaDinhTracks)
        {
            listTracksGiaDinh = giaDinhTracks;
        }
        public void getListTracksGiaoDan(List<Dictionary<string, object>> giaoDanTracks)
        {
            listTracksGiaoDan = giaoDanTracks;
        }

        public override void deleteObjectRelation()
        {
            deleteObjecChild(ListTracks, ThanhVienGiaDinhConst.MaGiaDinh, ThanhVienGiaDinhConst.MaGiaoDan, ThanhVienGiaDinhConst.TableName);
        }
    }
}
