using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    class ReadFileCSV
    {
        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
        public ReadFileCSV(string fileName)
        {
            if (File.Exists(fileName))
            {
                string line = "";
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string[] header = sr.ReadLine().Split(';');
                    string[] path = fileName.Split('\\');
                    string nameTable = path[path.Length - 1].Split('.')[0];
                    DataTable tbl = Memory.GetData(string.Format("Select * from {0} Where UpdateDate=#01/01/0001#", nameTable));
                    if (tbl != null)
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] data = line.Split(';');
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            for (int i = 0; i < header.Length-1; i++)
                            {
                                if (header[i]=="DeleteSV")
                                {
                                    dic.Add(header[i], data[i]);
                                    continue;
                                }
                                dic.Add(header[i], processTypeValue(data[i], tbl.Columns[header[i]].DataType));
                            }
                            this.Data.Add(dic);
                        }
                    }
                }
            }
        }
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
            if (typeValue.Name=="Int32")
            {
                return DBNull.Value;
            }
            return string.Empty;
        }
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
    }
}
