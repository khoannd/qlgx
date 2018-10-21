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

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(HonPhoiConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[HonPhoiConst.MaHonPhoi].ToString());
                    if (idCSV == 0)
                    {
                        //Xoa Hon Phoi
                        delete(@"MaHonPhoi=?", HonPhoiConst.TableName, item[HonPhoiConst.MaHonPhoi]);
                        //Xoa Thanh Vien Gia Dinh
                        delete(@"MaHonPhoi=?", GiaoDanHonPhoiConst.TableName, item[GiaoDanHonPhoiConst.MaHonPhoi]);
                    }

                }

            }
        }

        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable honPhoi = findHonPhoi(item);
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

            return null;

        }
    }
}
