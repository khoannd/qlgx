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
        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> listTracks = new List<Dictionary<string, object>>();
        public Compare(string dir, string nameCSV)
        {
            ReadFileCSV readFile = new ReadFileCSV(dir + nameCSV);
            this.Data = readFile.Data;
        }
        public DataTable assignData(Dictionary<string, object> objectCSV, DataTable objectClient, string nameTable,string fieldID1=null,string fieldID2=null)
        {
            foreach (var item in objectCSV)
            {
                if (item.Key == fieldID1||item.Key==fieldID2)
                    continue;
                objectClient.Rows[0][item.Key] = item.Value;
            }
            objectClient.TableName = nameTable;
            return objectClient;
        }
        public DataTable assignDataAdd(Dictionary<string, object> objectCSV,string fieldID1,object ID1 ,string nameTable,string fieldID2=null,object ID2=null)
        {
            string query = string.Format(@"Select * from {0} Where UpdateDate=#01/01/0001#", nameTable);
            DataTable tbl = Memory.GetData(query);
            if (tbl != null)
            {
                tbl.TableName = nameTable;
                DataRow row = tbl.NewRow();
                foreach (var item in objectCSV)
                {
                    if (item.Key==fieldID1)
                    {
                        row[fieldID1] = ID1;
                        continue;
                    }
                    if (item.Key==fieldID2)
                    {
                        row[fieldID2] = ID2;
                        continue;
                    }
                    row[item.Key] = item.Value;
                }
                tbl.Rows.Add(row);
                return tbl;
            }
            return null;


        }
        public int findIdObjectCSV(List<Dictionary<string, object>> listTracksTemp, string idDB)
        {
            if (listTracks != null && listTracksTemp.Count > 0)
            {
                foreach (var item in listTracksTemp)
                {
                    if (compareString(item["oldIdIsCsv"], "true"))
                    {
                        if (compareString(item["newId"], idDB))
                        {
                            return int.Parse(item["oldId"].ToString());
                        }
                    }
                    else
                    {
                        if (compareString(item["oldId"], idDB))
                        {
                            return int.Parse(item["newId"].ToString());
                        }
                    }
                }
            }
            return 0;
        }
        public List<Dictionary<string, object>> getListByID(List<Dictionary<string, object>> data, string fieldID1, object id)
        {
            List<Dictionary<string, object>> temp = new List<Dictionary<string, object>>();
            foreach (var item in data)
            {
                if (compareString(item[fieldID1], id))
                {
                    temp.Add(item);
                }
            }
            return temp;
        }
        public bool compareString(object a, object b)
        {
            if (string.Compare(a.ToString(), b.ToString()) == 0)
            {
                return true;
            }
            return false;
        }
        public int findIdObjectClient(List<Dictionary<string, object>> ListTracksTemp, object idCSV)
        {

            if (listTracks != null && ListTracksTemp.Count > 0)
            {
                foreach (var item in ListTracksTemp)
                {
                    if (compareString(item["oldIdIsCsv"], "true"))
                    {
                        if (compareString(item["oldId"], idCSV))
                        {
                            return int.Parse(item["newId"].ToString());
                        }
                    }
                    else
                    {
                        if (compareString(item["newId"], idCSV))
                        {
                            return int.Parse(item["oldId"].ToString());
                        }
                    }
                }
            }
            return 0;
        }

        public bool compareDate(object dateCSV, string dateClient)
        {
            DateTime dt1 = DateTime.Parse(dateCSV.ToString());
            DateTime dt2 = DateTime.Parse(dateClient);
            return (DateTime.Compare(dt1, dt2) > 0) ? true : false;
        }
        public void processDataNull(Dictionary<string, object> objectCSV, DataTable objectClient)
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
                Memory.ClearError();
            }
        }
        //public void update(Dictionary<string, object> objectCSV, string nameTable, string fieldID1, string ID1, string fieldID2 = "", string ID2 = "")
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
        //public void insert(Dictionary<string, object> data, string nameTable)
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

        public List<Dictionary<string, object>> Data
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

        public List<Dictionary<string, object>> ListTracks
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
