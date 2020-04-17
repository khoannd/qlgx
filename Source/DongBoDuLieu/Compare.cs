using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public abstract class Compare
    {
        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
        private DataTable tblDongBo;
        private DataTable tbl;
        private string MaGiaoXuRieng= Memory.getMaGiaoXuServer();
        public Compare(string dir, string nameCSV)
        {
            ReadFileCSV readFile = new ReadFileCSV(dir + nameCSV);    
            this.Data = readFile.Data;
        }
        public abstract void ProcessData();

        public DataRow findRowDongBo(int IDServer,string tableName)
        {
            string query = String.Format("{0} = '{1}' and {2} = {3} ",DongBoConst.TenBang,tableName,DongBoConst.MaIDMayChu,IDServer);
            DataRow[] rs = TblDongBo.Select(query);
            if(rs!=null && rs.Length>0)
            {
                return rs[0];
            }
             return null;
        }
        public int findValueDongBoClient(int IDServer, string tableName)
        {
            DataRow rs = findRowDongBo(IDServer, tableName);
            if(rs!=null)
            {
                return Convert.ToInt32(rs[DongBoConst.MaIDMayKhach].ToString());
            } 
            return -1;
        }
        public void insertDongBoID(int MaIDMayKhach,int MaIDMayChu,string TenBang)
        {
            DataRow newRow = tblDongBo.NewRow();
            newRow[DongBoConst.TenBang] = TenBang;
            newRow[DongBoConst.MaIDMayChu] = MaIDMayChu;
            newRow[DongBoConst.MaIDMayKhach] = MaIDMayKhach;
            newRow[DongBoConst.UpdateDate] = Memory.Instance.GetServerDateTime().ToString();
            newRow[GxSyn.MaGiaoXuRieng] = MaGiaoXuRieng;
            tblDongBo.Rows.Add(newRow);
        }
        public void assignDataAdd(DataTable tbl, Dictionary<string, object> objectCSV, string fieldID1, object ID1, string fieldID2 = null, object ID2 = null)
        {
            DataRow row = tbl.NewRow();
            foreach (var item in objectCSV)
            {
                if (item.Key == fieldID1)
                {
                    row[fieldID1] = ID1;
                    continue;
                }
                if (item.Key == fieldID2)
                {
                    row[fieldID2] = ID2;
                    continue;
                }
                if (item.Key == "DeleteSV")
                {
                    continue;
                }
                row[item.Key] = item.Value;
            }
            tbl.Rows.Add(row);
        }
        public void assignData(Dictionary<string, object> objectCSV, DataRow rowClient,string fieldID1=null,string fieldID2=null)
        {
            foreach (var item in objectCSV)
            {
                if (item.Key == fieldID1||item.Key==fieldID2||item.Key=="DeleteSV")
                    continue;
                rowClient[item.Key] = item.Value;
            }
        }
        
        public bool changeValueKey(Dictionary<string, object> objectCSV, string fieldID1,string tableNameObject1, string fieldID2 = null,string tableNameObject2=null)
        {
            int maFieldID1 = findValueDongBoClient(Convert.ToInt32(objectCSV[fieldID1]), tableNameObject1);
            if (maFieldID1 == -1)
                return false;
            objectCSV[fieldID1] = maFieldID1;
            if(fieldID2!=null)
            {
                int maFieldID2 = findValueDongBoClient(Convert.ToInt32(objectCSV[fieldID2]), tableNameObject2);
                if (maFieldID2 == -1)
                    return false;
                objectCSV[fieldID2] = maFieldID2;
            }    
            return true;
        }
        
        public bool compareString(object a, object b)
        {
            if (string.Compare(a.ToString(), b.ToString()) == 0)
            {
                return true;
            }
            return false;
        }
      
        public bool compareDate(object dateCSV, string dateClient)
        {
            DateTime dt1 = DateTime.Parse(dateCSV.ToString());
            DateTime dt2 = DateTime.Parse(dateClient);
            return (DateTime.Compare(dt1, dt2) > 0) ? true : false;
        }
        public void processDataNull(Dictionary<string, object> objectCSV, DataRow objectClient)
        {
            List<string> keys = new List<string>(objectCSV.Keys);
            foreach (string item in keys)
            {
                if (item=="DeleteSV")
                {
                    continue;
                }
                if (objectClient[item].ToString() != "" && objectCSV[item].ToString() == "")
                {
                    objectCSV[item] = objectClient[item];
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
            return DBNull.Value;
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
        public DataTable TblDongBo
        {
            get
            {
                return tblDongBo;
            }
            set
            {
                tblDongBo = value;
            }
        }
        public DataTable Tbl
        {
            get
            {
                return tbl;
            }
            set
            {
                tbl = value;
            }
        }
    }
}
