using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class LinhMucCompare : CompareMasterTable
    {
        public LinhMucCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        private List<Dictionary<string, object>> ListGiaoHoTracks;
        public void getListGiaoHoTracks(List<Dictionary<string, object>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public override void importCacObject()
        {
            foreach (var item in Data)
            {
                DataTable linhMuc = findLinhMuc(item);
                ////delete linhmuc
                //if (deleteObjectMaster(item, linhMuc))
                //{
                //    continue;
                //}
                importObjectMaster(item, linhMuc, LinhMucConst.MaLinhMuc, LinhMucConst.TableName);
            }
        }
        private DataTable findLinhMuc(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                if (objectCSV["HoTen"].ToString() != "" && objectCSV["TenThanh"].ToString() != "" &&
                    objectCSV["ChucVu"].ToString() != "")
                {
                    string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE HoTen=? AND TenThanh=? AND ChucVu=?", LinhMucConst.TableName);
                    tbl = Memory.GetData(query, objectCSV["HoTen"], objectCSV["TenThanh"], objectCSV["ChucVu"]);
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return tbl;
            }
            return null;
        }
        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (objectCSV["DeleteSV"].ToString() == "1" && item != null && item.Rows.Count > 0)
            {
                if (compareDate(objectCSV["UpdateDate"], item.Rows[0][LinhMucConst.UpdateDate].ToString()))
                {
                    delete(@"MaLinhMuc = ?", LinhMucConst.TableName, item.Rows[0][LinhMucConst.MaLinhMuc]);
                }

                return true;
            }

            if (objectCSV["DeleteSV"].ToString() == "1")
            {
                return true;
            }
            return false;
        }
    }
}
