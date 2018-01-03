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
            try
            {
                OleDbConnection connect = new OleDbConnection();
                OleDbDataAdapter adapter = new OleDbDataAdapter();

                connect = new OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + "';Extended Properties='Excel 8.0;HDR=YES;IMEX=1'");

                adapter = new OleDbDataAdapter(string.Format("select * from [{0}$]", tableName), connect);
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
                Memory.Instance.Error = ex;
                return null;
            }
        }
    }
}
