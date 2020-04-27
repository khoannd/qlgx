/*
 * Created by: Khoan
 * Created date: 10/03/2008
 * Updated:
 * 
 */
using System;
using System.Data;
using System.Collections.Generic;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GxGlobal
{
    public class DataProvider
    {
        #region DECLARE
        private String m_commText = "";

        private object[] m_Parameters;
        private String m_connString = "";

        public String ConnectionString
        {
            get { return m_connString; }
            set { m_connString = value; }
        }

        private OleDbConnection Conn;
        private OleDbTransaction transaction;
        /// <summary>
        /// Indicating the transaction is external transaction
        /// </summary>
        private bool isExTransaction = false;
        #endregion

        #region PUBLIC

        public DataProvider(string connectionString)
        {
            m_connString = connectionString;
        }

        public DataProvider(string dbFullPath, string user, string password)
        {
            try
            {
                m_connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};User ID={1}; Jet OLEDB:Database Password={2}", dbFullPath, user, password);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
        }

        public bool IsOpen
        {
            get
            {
                if (Conn == null) return false;
                return Conn.State == ConnectionState.Open;
            }
        }

        /// <summary>
        /// Begin new transaction
        /// </summary>
        /// <returns>true is success, otherwise false</returns>
        public bool BeginTransaction()
        {
            isExTransaction = true;
            try
            {
                if (Conn == null)
                {
                    Conn = new OleDbConnection(m_connString);
                }
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                Conn.Open();
                transaction = Conn.BeginTransaction();
                return true;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                isExTransaction = false;
                return false;
            }

        }

        /// <summary>
        /// Commit data to data source
        /// </summary>
        /// <returns>true is success, otherwise false</returns>
        public bool CommitTransaction()
        {
            isExTransaction = false;
            if (Conn == null || transaction == null)
            {
                Memory.Instance.Error = new Exception("Can't found connection or transaction to commit");
                return false;
            }
            bool result = false;
            try
            {
                transaction.Commit();
                result = true;
            }
            catch (OleDbException ex)
            {
                transaction.Rollback();
                Memory.Instance.Error = ex;
                result = false;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Rollback data from data source
        /// </summary>
        /// <returns>true is success, otherwise false</returns>
        public bool RollbackTransaction()
        {
            isExTransaction = false;
            if (Conn == null || transaction == null)
            {
                Memory.Instance.Error = new Exception("Can't found connection or transaction to commit");
                return false;
            }
            bool result = false;
            try
            {
                transaction.Rollback();
                result = true;
            }
            catch (OleDbException ex)
            {
                Memory.Instance.Error = ex;
                result = false;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return result;
        }

        public DataTable GetData(string commandText, string tableName = "Table1")
        {
            m_commText = commandText;
            return GetData(tableName);
        }

        public DataTable GetData(string commandText, object[] lstParameters)
        {
            m_commText = commandText;
            m_Parameters = lstParameters;
            return GetData();
        }
       
        
        private DataTable GetData(string tableName = "Table1")
        {
            DataSet ds = null;
            DataTable tbl = null;
            try
            {
                if (Conn == null)
                {
                    Conn = new OleDbConnection(m_connString);
                }
                OleDbDataAdapter adapter = new OleDbDataAdapter(m_commText, Conn);
                OleDbCommand command = null;
                try
                {
                    command = CreateCommand(m_Parameters);
                    command.CommandText = m_commText;
                    command.Connection = Conn;
                    if (Conn.State != ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    //trans = command.Connection.BeginTransaction();
                    //command.Transaction = trans;

                    adapter.SelectCommand = command;

                    ds = new DataSet();
                    adapter.Fill(ds, tableName);
                    tbl = ds.Tables[0];
                    ds.Tables.Clear();
                    //trans.Commit();
                }
                catch (OleDbException ex)
                {
                    Memory.Instance.Error = ex;
                }
                finally
                {
                    command.Connection.Close();
                }
            }
            catch (Exception e)
            {
                Memory.Instance.Error = e;
                ds = null;
            }

            return tbl;
        }

        public Int32 Execute(string commandText)
        {
            m_commText = commandText;
            return Execute();
        }
        

        public Int32 Execute(string commandText, params object[] lstParameters)
        {
            m_commText = commandText;
            m_Parameters = lstParameters;
            return Execute();
        }

        public Int32 Execute(DataSet ds)
        {
            int rs = 0;
            if (ds == null)
            {
                Memory.Instance.Error = new Exception("DataSet can't null");
                return -1;
            }
            try
            {
                OleDbDataAdapter da = null;
                if (!isExTransaction && Conn == null)
                {
                    Conn = new OleDbConnection(m_connString);
                }
                try
                {
                    if (!isExTransaction && Conn.State != ConnectionState.Open)
                    {
                        Conn.Open();
                        transaction = Conn.BeginTransaction();
                    }
                    da = new OleDbDataAdapter();
                    foreach (DataTable tbl in ds.Tables)
                    {
                        if (tbl.TableName == CauHinhConst.TableName)
                        {
                            da.SelectCommand = new OleDbCommand(string.Format("SELECT MaCauHinh,GiaTri,Mota,UpdateDate FROM {0}", tbl.TableName), Conn, transaction);
                        }
                        else
                        {
                            da.SelectCommand = new OleDbCommand(string.Format("SELECT * FROM {0}", tbl.TableName), Conn, transaction);

                        }
                        OleDbCommandBuilder cmdBd = new OleDbCommandBuilder(da);
                        cmdBd.ConflictOption = ConflictOption.OverwriteChanges;
                        cmdBd.QuotePrefix = "[";
                        cmdBd.QuoteSuffix = "]";
                        da.DeleteCommand = cmdBd.GetDeleteCommand();
                        da.InsertCommand = cmdBd.GetInsertCommand();
                        da.UpdateCommand = cmdBd.GetUpdateCommand();
                        da.Update(tbl);
                    }
                    if (!isExTransaction)
                    {
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (!isExTransaction)
                    {
                        transaction.Rollback();
                    }
                    Memory.Instance.Error = ex;
                    rs = -1;
                }
                finally
                {
                    if (!isExTransaction && Conn.State == ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Map server exception to meaningful client error message
                Memory.Instance.Error = e;
                rs = 1;
            }
            return rs;
        }

        public Exception GetError()
        {
            return Memory.Instance.Error;
        }

        #endregion

        #region PRIVATE

        private Int32 Execute()
        {
            int rs = 0;
            try
            {
                if (!isExTransaction && Conn == null)
                {
                    Conn = new OleDbConnection(m_connString);
                }
                OleDbCommand command = null;
                try
                {

                    command = CreateCommand(m_Parameters);
                    command.CommandText = m_commText;
                    command.Connection = Conn;
                    if (!isExTransaction && Conn.State != ConnectionState.Open)
                    {
                        command.Connection.Open();
                        if (!isExTransaction)
                        {
                            transaction = command.Connection.BeginTransaction();
                        }
                    }
                    command.Transaction = transaction;
                    rs = command.ExecuteNonQuery();
                    if (!isExTransaction)
                    {
                        transaction.Commit();
                    }
                    //rs = 0;
                }
                catch (OleDbException ex)
                {
                    //MessageBox.Show("Can't execute the query: " + m_commText + Environment.NewLine + "Error: " + ex.Message);
                    try
                    {
                        if (!isExTransaction)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch { }
                    Memory.Instance.Error = ex;
                    //m_Parameters.Clear();
                    rs = 1;
                }
                finally
                {
                    if (!isExTransaction && command.Connection.State != ConnectionState.Closed)
                    {
                        command.Connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Map server exception to meaningful client error message
                Memory.Instance.Error = e;
                rs = 1;
                //m_Parameters.Clear();
                //throw e;
            }
            return rs;
        }

        private OleDbCommand CreateCommand(object[] lstData)
        {
            OleDbCommand cmd = new OleDbCommand();
            {
                if (lstData != null)
                {
                    for (int i = 0; i < lstData.Length; i++)
                    {

                        OleDbParameter param = new OleDbParameter();
                        param.Value = lstData[i];
                        param.ParameterName = string.Format("Param{0}", i + 1);
                        cmd.Parameters.Add(param);
                    }
                }
            }
            return cmd;
        }

        public DataTable GetTableSchema()
        {
            DataTable schemaTable = null;
            try
            {
                //T oConn = new T();
                BeginTransaction();
                schemaTable = Conn.GetSchema("Columns", new string[] { });
                CommitTransaction();
                return schemaTable;

            }
            catch { }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return schemaTable;
        }

        public DataTable GetViewSchema()
        {
            DataTable schemaTable = null;
            try
            {
                BeginTransaction();
                schemaTable = Conn.GetSchema(
                    "TABLES",
                    new string[] { null, null, null, "VIEW" });
                CommitTransaction();
                return schemaTable;

            }
            catch { }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return schemaTable;
        }
        #endregion
    }
}