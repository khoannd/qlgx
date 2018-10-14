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
        public int findIdObjectCSV(List<Dictionary<string, string>> listTracks, string idDB)
        {
            if (listTracks != null && listTracks.Count > 0)
            {
                foreach (var item in ListTracks)
                {
                    if (compareString(item["oldIdIsCsv"],"true"))
                    {
                        if (compareString(item["newId"],idDB))
                        {
                            return int.Parse(item["oldId"]);
                        }
                    }
                    else
                    {
                        if (compareString(item["oldId"],idDB))
                        {
                            return int.Parse(item["newId"]);
                        }
                    }
                }
            }
            return 0;
        }
        public  bool compareString(string a,string b)
        {
            if (string.Compare(a,b)==0)
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
                    if (compareString(item["oldIdIsCsv"],"true"))
                    {
                        if (compareString(item["oldId"],idCSV))
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
        public void update(Dictionary<string, string> objectCSV, string nameTable, string fieldID1, string ID1, string fieldID2 = "", string ID2 = "")
        {
            string temp = "";
            objectCSV["UpdateDate"] = DateTime.Now.ToString();
            foreach (var item in objectCSV)
            {
                string value = processTypeValue(item.Value);
                temp += item.Key + "=" + value + ",";
            }
            temp = temp.Remove(temp.Length - 1, 1);
            string query = "";
            if (!string.IsNullOrEmpty(fieldID2) && !string.IsNullOrEmpty(ID2))
            {
                query = string.Format(@"UPDATE {0} SET {1} WHERE {2}={3} AND {4}={5}", nameTable, temp, fieldID1, ID1, fieldID2, ID2);
            }
            else
            {
                query = string.Format(@"UPDATE {0} SET {1} WHERE {2}={3}", nameTable, temp, fieldID1, ID1);
            }

            Memory.ExecuteSqlCommand(query);
            Memory.ClearError();
        }

        private string processTypeValue(string value)
        {
            int rs;
            DateTime rs2;
            bool check = int.TryParse(value, out rs);
            if (check)
            {
                return value;
            }
            check = DateTime.TryParse(value, out rs2);
            if (check)
            {
                return "#" + value + "#";
            }
            return "'" + value + "'";
        }
        public void delete(string condition,string nameTable)
        {
            string query = string.Format(@"DELETE FROM {0} WHERE {1}", nameTable, condition);
            Memory.ExecuteSqlCommand(query);
            Memory.ClearError();
        }
        public void insert(Dictionary<string, string> data, string nameTable)
        {
            string field = "";
            string value = "";
            data["UpdateDate"] = DateTime.Now.ToString();
            foreach (var item in data)
            {
                field += item.Key + ",";
                value += processTypeValue(item.Value) + ",";
            }
            string query = string.Format(@"INSERT INTO {0} ({1}) VALUES ({2})", nameTable, field, value);
            Memory.ExecuteSqlCommand(query);
            Memory.ClearError();
        }

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
