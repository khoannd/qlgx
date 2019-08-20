using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class HonPhoiCompare : CompareMasterTable
    {
        public HonPhoiCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1 && item != null && item.Rows.Count > 0
                                                           && compareDate(objectCSV[GxSyn.UpdateDate], item.Rows[0][GxSyn.UpdateDate].ToString()))
            {
                //Xoa Hon Phoi
                delete(@"MaHonPhoi=?", HonPhoiConst.TableName, item.Rows[0][HonPhoiConst.MaHonPhoi]);
                //Xoa giao dan hon phoi
                delete(@"MaHonPhoi=?", GiaoDanHonPhoiConst.TableName, item.Rows[0][GiaoDanHonPhoiConst.MaHonPhoi]);
                return true;
            }
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1)
            {
                return true;
            }
            return false;

        }
        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable honPhoi = findHonPhoi(item);
                    if (deleteObjectMaster(item,honPhoi))
                        continue;
                    importObjectMaster(item, honPhoi, "MaHonPhoi", HonPhoiConst.TableName);
                }
            }
        }

        private DataTable findHonPhoi(Dictionary<string, object> objectCSV)
        {
            //check Ma nhan dang
            DataTable tbl = null;
            try
            {
                if (!string.IsNullOrEmpty(objectCSV["MaNhanDang"].ToString()))
                {
                    string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", HonPhoiConst.TableName);
                    tbl = Memory.GetData(query, objectCSV["MaNhanDang"]);
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
            //check ten hon phoi, ngay hon phoi, so hon phoi
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE TenHonPhoi=? AND NgayHonPhoi=? AND SoHonPhoi=?", HonPhoiConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenHonPhoi"], objectCSV["NgayHonPhoi"], objectCSV["SoHonPhoi"]);
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
    }
}
