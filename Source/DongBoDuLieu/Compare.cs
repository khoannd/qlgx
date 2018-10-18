using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class Compare
    {
        private List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> listTracks = new List<Dictionary<string, string>>();
        public Compare(string dir, string nameCSV)
        {
            ReadFileCSV readFile = new ReadFileCSV(dir + nameCSV);
            this.Data = readFile.Data;
        }
        public DataTable assignData(Dictionary<string, string> objectCSV, DataTable objectClient, string nameTable)
        {
            foreach (var item in objectCSV)
            {
                objectClient.Rows[0][item.Key] = processTypeValue(item.Value,objectClient.Columns[item.Key].DataType);

            }
            objectClient.TableName = nameTable;
            return objectClient;
        }
        public DataTable assignDataAdd(Dictionary<string, string> objectCSV, string nameTable)
        {
            string query = string.Format(@"Select Top 1 * from {0} Where DaXoa=2", nameTable);
            DataTable tbl = Memory.GetData(query);
            if (tbl != null)
            {
                tbl.TableName = nameTable;
                DataRow row = tbl.NewRow();
                foreach (var item in objectCSV)
                {
                    row[item.Key] = processTypeValue(item.Value,tbl.Columns[item.Key].DataType);
                }
                tbl.Rows.Add(row);
                return tbl;
            }
            return null;


        }
        public int findIdObjectCSV(List<Dictionary<string, string>> listTracks, string idDB)
        {
            if (listTracks != null && listTracks.Count > 0)
            {
                foreach (var item in ListTracks)
                {
                    if (compareString(item["oldIdIsCsv"], "true"))
                    {
                        if (compareString(item["newId"], idDB))
                        {
                            return int.Parse(item["oldId"]);
                        }
                    }
                    else
                    {
                        if (compareString(item["oldId"], idDB))
                        {
                            return int.Parse(item["newId"]);
                        }
                    }
                }
            }
            return 0;
        }
        public List<Dictionary<string, string>> getListByID(List<Dictionary<string, string>> data, string fieldID1, string id)
        {
            List<Dictionary<string, string>> temp = new List<Dictionary<string, string>>();
            foreach (var item in data)
            {
                if (compareString(item[fieldID1], id))
                {
                    temp.Add(item);
                }
            }
            return temp;
        }
        public bool compareString(string a, string b)
        {
            if (string.Compare(a, b) == 0)
            {
                return true;
            }
            return false;
        }
        public int findIdObjectClient(List<Dictionary<string, string>> listTracks, string idCSV)
        {

            if (listTracks != null && listTracks.Count > 0)
            {
                foreach (var item in ListTracks)
                {
                    if (compareString(item["oldIdIsCsv"], "true"))
                    {
                        if (compareString(item["oldId"], idCSV))
                        {
                            return int.Parse(item["newId"]);
                        }
                    }
                    else
                    {
                        if (compareString(item["newId"], idCSV))
                        {
                            return int.Parse(item["oldId"]);
                        }
                    }
                }
            }
            return 0;
        }

        public bool compareDate(string dateCSV, string dateClient)
        {
            DateTime dt1 = DateTime.Parse(dateCSV);
            DateTime dt2 = DateTime.Parse(dateClient);
            return (DateTime.Compare(dt1, dt2) > 0) ? true : false;
        }
        public void processDataNull(Dictionary<string, string> objectCSV, DataTable objectClient)
        {
            foreach (var item in objectCSV)
            {
                if (objectClient.Rows[0][item.Key].ToString() == "" && item.Value != "")
                {
                    objectClient.Rows[0][item.Key] = item.Value;
                }
            }
        }
        public DataTable getAll(string nameTable)
        {
            DataTable tbl = null;
            try
            {
                tbl = Memory.GetData(@"SELECT * FROM " + nameTable);
            }
            catch (System.Exception ex)
            {
                tbl = null;
            }
            return tbl;
        }
        public void update(DataTable tblObject)
        {
            if (tblObject != null && tblObject.Rows.Count > 0)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(tblObject);
                Memory.UpdateDataSet(ds);
            }
        }
        //public void update(Dictionary<string, string> objectCSV, string nameTable, string fieldID1, string ID1, string fieldID2 = "", string ID2 = "")
        //{
        //    string field = "";
        //    object[] value = new object[objectCSV.Count];
        //    objectCSV["UpdateDate"] = DateTime.Now.ToString();
        //    int i = 0;
        //    foreach (var item in objectCSV)
        //    {
        //        field += item.Key + "=?,";
        //        value[i++] = item.Value;

        //    }
        //    field = field.Remove(field.Length - 1, 1);
        //    string query = "";
        //    if (!string.IsNullOrEmpty(fieldID2) && !string.IsNullOrEmpty(ID2))
        //    {
        //        query = string.Format(@"UPDATE {0} SET {1} WHERE {2}=? AND {3}=?", nameTable, field, fieldID1, fieldID2);
        //        Memory.ExecuteSqlCommand(query, value, ID1, ID2);
        //        Memory.ClearError();

        //    }
        //    else
        //    {
        //        query = string.Format(@"UPDATE {0} SET {1} WHERE {2}=?", nameTable, field, fieldID1);
        //        Memory.ExecuteSqlCommand(query, value, ID1);
        //        Memory.ClearError();
        //    }



        //}

        private object processTypeValue(string value, Type typeValue)
        {
            if (value != "")
            {
                if (typeValue.Name == "Boolean")
                {
                    return (value == "0") ? false : true;
                }
                return Convert.ChangeType(value, typeValue);

            }
            return DBNull.Value;
        }
        public void delete(string condition, string nameTable, params object[] paramater)
        {
            string query = string.Format(@"DELETE FROM {0} WHERE {1}", nameTable, condition);
            Memory.ExecuteSqlCommand(query, paramater);
            Memory.ClearError();
        }
        //public void insert(Dictionary<string, string> data, string nameTable)
        //{
        //    string field = "";
        //    string mask = "";
        //    object[] value = new object[data.Count];
        //    int i = 0;
        //    data["UpdateDate"] = DateTime.Now.ToString();
        //    foreach (var item in data)
        //    {
        //        field += item.Key + ",";
        //        mask += "?,";
        //        value[i++] = item.Value;

        //    }
        //    mask = mask.Remove(mask.Length - 1, 1);
        //    string query = string.Format(@"INSERT INTO {0} ({1}) VALUES ({2})", nameTable, field, mask);
        //    Memory.ExecuteSqlCommand(query,value);
        //    Memory.ClearError();
        //}

        public List<Dictionary<string, string>> Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        public List<Dictionary<string, string>> ListTracks
        {
            get
            {
                return listTracks;
            }

            set
            {
                listTracks = value;
            }
        }

    }
}
