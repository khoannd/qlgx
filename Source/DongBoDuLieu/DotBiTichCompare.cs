using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class DotBiTichCompare : CompareMasterTable
    {
        public DotBiTichCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(DotBiTichConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[DotBiTichConst.MaDotBiTich].ToString());
                    if (idCSV == 0)
                    {
                        delete(@"MaDotBiTich=?", DotBiTichConst.TableName, item[DotBiTichConst.MaDotBiTich]);

                        delete(@"MaDotBiTich=?", DotBiTichConst.TableName, item[DotBiTichConst.MaDotBiTich]);
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
                    DataTable dotbitich = findDotBiTich(item);
                    importObjectMaster(item, dotbitich, DotBiTichConst.MaDotBiTich, DotBiTichConst.TableName);
                }
            }
        }

        private DataTable findDotBiTich(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                //MoTa,NgayBiTich,LoaiBiTich,LinhMuc
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MoTa=? AND NgayBiTich=? AND LoaiBiTich=? AND LinhMuc=?", GiaDinhConst.TableName);
                tbl = Memory.GetData(query,new object[] { objectCSV["MoTa"], objectCSV["NgayBiTich"], objectCSV["LoaiBiTich"], objectCSV["LinhMuc"] });
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl != null)
            {
                return tbl;
            }
            return null;
        }
    }
}
