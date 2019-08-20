using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class TanHienCompare : CompareMasterTable
    {
        private List<Dictionary<string, object>> listGiaoDanTracks;
        public TanHienCompare(string dir, string nameCSV) : base(dir, nameCSV)
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
                if (compareDate(objectCSV["UpdateDate"], item.Rows[0][TanHienConst.UpdateDate].ToString()))
                {
                    //delete TanHien
                    delete(@"MaTanHien = ?", TanHienConst.TableName, item.Rows[0][TanHienConst.MaTanHien]);
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
                item[TanHienConst.MaGiaoDan] = findIdObjectClient(listGiaoDanTracks, item[TanHienConst.MaGiaoDan]).ToString();
                if (int.Parse(item[TanHienConst.MaGiaoDan].ToString()) == 0)
                {
                    continue;
                }
                DataTable tanhien = findTanHien(item);
                //delete giaoDan
                if (deleteObjectMaster(item, tanhien))
                {
                    continue;
                }
                importObjectMaster(item, tanhien, "MaTanHien", TanHienConst.TableName);
            }
        }

        private DataTable findTanHien(Dictionary<string, object> item)
        {
            var sql = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaGiaoDan = ?", TanHienConst.TableName);
            var tbl = Memory.GetData(sql, item[TanHienConst.MaGiaoDan]);
            if (tbl.Rows.Count > 0)
            {
                return tbl;
            }

            return null;
        }
    }
}
