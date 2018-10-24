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

    

        public override bool deleteObjectMaster(object CSVDelete, DataTable item)
        {
            if (int.Parse(CSVDelete.ToString()) == 1 && item != null && item.Rows.Count > 0)
            {
                delete(@"MaDotBiTich=?", DotBiTichConst.TableName, item.Rows[0][DotBiTichConst.MaDotBiTich]);

                delete(@"MaDotBiTich=?", BiTichChiTietConst.TableName, item.Rows[0][BiTichChiTietConst.MaDotBiTich]);
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
                    DataTable dotbitich = findDotBiTich(item);
                    if (deleteObjectMaster(item["DeleteSV"],dotbitich))
                    {
                        continue;
                    }
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
                tbl = Memory.GetData(query, new object[] { objectCSV["MoTa"], objectCSV["NgayBiTich"], objectCSV["LoaiBiTich"], objectCSV["LinhMuc"] });
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
