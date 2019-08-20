using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class CauHinhCompare : CompareMasterTable
    {
        public CauHinhCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public override void importCacObject()
        {
            foreach (var item in Data)
            {
                DataTable cauhinh = findCauHinh(item);
                //delete cauhinh
                if (deleteObjectMaster(item, cauhinh))
                {
                    continue;
                }
                importObjectMaster(item, cauhinh, CauHinhConst.MaCauHinh, CauHinhConst.TableName);
            }
        }
        private DataTable findCauHinh(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaCauHinh = ?", CauHinhConst.TableName);
                tbl = Memory.GetData(query, objectCSV[CauHinhConst.MaCauHinh]);
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
                if (compareDate(objectCSV["UpdateDate"], item.Rows[0][CauHinhConst.UpdateDate].ToString()))
                {
                    delete(@"MaCauHinh = ?", CauHinhConst.TableName, item.Rows[0][CauHinhConst.MaCauHinh]);
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
