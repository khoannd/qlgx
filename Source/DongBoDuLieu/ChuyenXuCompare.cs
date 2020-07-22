using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class ChuyenXuCompare : CompareMasterTable
    {
        private List<Dictionary<string, object>> listGiaoDanTracks;
        public ChuyenXuCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public void getListGiaoDanTracks(List<Dictionary<string, object>> giaoDanTracks)
        {
            listGiaoDanTracks = giaoDanTracks;
        }

        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (objectCSV["DeleteSV"].ToString() == "1" && item != null && item.Rows.Count > 0)
            {
                if (compareDate(objectCSV["UpdateDate"], item.Rows[0][ChuyenXuConst.UpdateDate].ToString()))
                {
                    //delete ChuyenXu
                    delete(@"MaChuyenXu = ?", ChuyenXuConst.TableName, item.Rows[0][ChuyenXuConst.MaChuyenXu]);
                }
                return true;
            }

            if (objectCSV["DeleteSV"].ToString() == "1")
            {
                return true;
            }
            return false;
        }

        public override void importCacObject()
        {
            foreach (var item in Data)
            {
                item["MaGiaoDan"] = findIdObjectClient(listGiaoDanTracks, item["MaGiaoDan"]).ToString();
                if (int.Parse(item["MaGiaoDan"].ToString()) == 0)
                {
                    continue;
                }
                DataTable chuyenxu = findChuyenXu(item);
                //delete giaoDan
                if (deleteObjectMaster(item, chuyenxu))
                {
                    continue;
                }
                importObjectMaster(item, chuyenxu, "MaChuyenXu", ChuyenXuConst.TableName);
            }
        }

        private DataTable findChuyenXu(Dictionary<string, object> item)
        {
            var sql = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaGiaoDan = ?",ChuyenXuConst.TableName);
            var tbl =  Memory.GetData(sql, item[ChuyenXuConst.MaGiaoDan]);
            if (tbl.Rows.Count > 0)
            {
                return tbl;
            }

            return null;
        }
    }
}
