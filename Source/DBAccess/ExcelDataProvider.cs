using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace GxGlobal
{
    public class ExcelDataProvider
    {
        public static DataSet GetDataSet(string path, string tableName)
        {
            return GetDataSet(path, tableName, "");
        }
        public static DataSet GetDataSet(string path, string tableName, string sql)
        {
            string[] conns = new string[]{ 
                "provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + path + "';Extended Properties='Excel 8.0;HDR=YES;IMEX=1'",
                "provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'",
                "provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + "';Extended Properties='Excel 8.0 Xml;HDR=YES;IMEX=1'"
                };
            int i = 0;
            retry:
            OleDbConnection connect = null;
            try
            {
                connect = new OleDbConnection();
                OleDbDataAdapter adapter = new OleDbDataAdapter();

                connect = new OleDbConnection(conns[i]);
                if (string.IsNullOrEmpty(sql))
                {
                    sql = string.Format("select * from [{0}$]", tableName);
                }
                adapter = new OleDbDataAdapter(sql, connect);
                adapter.TableMappings.Add("Table", tableName);
                DataSet ds = new DataSet();
                adapter.Fill(ds, tableName);
                adapter.Dispose();

                connect.Close();
                return ds;
            }
            catch (System.Exception ex)
            {
                //Write log
                //Temporary solution
                if(i == 0) Memory.Instance.Error = ex; 

                if(connect != null && connect.State != ConnectionState.Closed)
                {
                    try
                    {
                        connect.Close();
                    }
                    catch 
                    {
                    }
                }

                if(i < conns.Length - 1)
                {
                    i++;
                    goto retry;
                }
                
                return null;
            }
        }

        public static DataTable GetDataTable(string path, string sql)
        {
            try
            {
                OleDbConnection connect = new OleDbConnection();
                OleDbDataAdapter adapter = new OleDbDataAdapter();

                connect = new OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + "';Extended Properties='Excel 8.0;HDR=YES;IMEX=1'");

                adapter = new OleDbDataAdapter(sql, connect);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                adapter.Dispose();

                connect.Close();

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable tbl = ds.Tables[0];
                    tbl.DataSet.Tables.Remove(tbl);
                    return tbl;
                }
                return null;
            }
            catch (System.Exception ex)
            {
                //Write log
                //Temporary solution
                Memory.Instance.Error = ex;
                return null;
            }
        }
    }
}
