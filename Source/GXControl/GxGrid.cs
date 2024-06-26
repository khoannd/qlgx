using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;
using System.Threading;
using GxGlobal;

namespace GxControl
{
    public partial class GxGrid : GridEX
    {
        protected Label LblLoadData = new Label();
        protected System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private int counter = 0;
        private const string GRID_COL_FILENAME = "GridColumns.xml";
        public static string XmlPath = Memory.AppPath + GRID_COL_FILENAME;
        protected Dictionary<string, int> ColumnWidths = null;
       
        public event EventHandler LoadDataFinished;

        private bool disableParentOnLoadData = true;

        public bool DisableParentOnLoadData
        {
            get { return disableParentOnLoadData; }
            set { disableParentOnLoadData = value; }
        }

        private string queryString = "";

        public string QueryString
        {
            get { return queryString; }
            set { queryString = value; }
        }

        private object[] arguments = null;

        public object[] Arguments
        {
            get { return arguments; }
            set { arguments = value; }
        }

        private string whereSQL = "";

        public string WhereSQL
        {
            get { return whereSQL; }
            set { whereSQL = value; }
        }

        public GxGrid()
        {
            InitializeComponent();
            LblLoadData.BackColor = Color.Yellow;
            LblLoadData.ForeColor = Color.Black;
            FilterMode = FilterMode.Automatic;
            AllowColumnDrag = true;
            AllowRemoveColumns = InheritableBoolean.True;
            GroupByBoxVisible = false;
            //AlternatingColors = true;
            ColumnAutoResize = true;
            DynamicFiltering = true;
            HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            VisualStyle = VisualStyle.Office2003;
            
            
            LblLoadData.Text = "Đang tải dữ liệu, xin vui lòng đợi...";
            LblLoadData.Font = new Font(this.Font.FontFamily, 11, FontStyle.Bold);
            LblLoadData.AutoSize = true;
            //LblLoadData.Location = new Point((int)(this.Width / 2 - LblLoadData.Width / 2), (int)(this.Height / 2 - LblLoadData.Height / 2));
            LblLoadData.Location = new Point((int)(this.Width / 2 - LblLoadData.Width / 2), 100);
            LblLoadData.BringToFront();

            LblLoadData.Visible = false;
            this.Controls.Add(LblLoadData);
            timer.Interval = 500;
            timer.Tick += new EventHandler(timer_Tick);

            if (!Memory.IsDesignMode && Memory.Instance.GetMemory("GridColumns") == null)
            {
                DataTable tbl = null;
                if (System.IO.File.Exists(XmlPath))
                {
                    try
                    {
                        tbl = new DataTable();
                        DataSet ds = new DataSet();
                        ds.ReadXml(XmlPath);
                        if (ds.Tables.Count > 0)
                        {
                            tbl = ds.Tables[0];
                            tbl.DataSet.Tables.Remove(tbl);
                            Memory.Instance.SetMemory("GridColumns", tbl);
                        }
                        
                    }
                    catch (Exception)
                    {
                        tbl = null;
                    }
                    
                }
                if (tbl == null)
                {
                    tbl = new DataTable();
                    tbl.TableName = "tblColumnWidth";
                    tbl.Columns.Add("key", typeof(string));
                    tbl.Columns.Add("columns", typeof(string));
                    tbl.Columns.Add("widths", typeof(string));
                    Memory.Instance.SetMemory("GridColumns", tbl);
                }
            }
            
        }

        protected void GetGridColumnWithData()
        {
            if (ColumnWidths != null) return;

            try
            {
                ColumnWidths = new Dictionary<string, int>();
                DataTable tbl = (DataTable)Memory.Instance.GetMemory("GridColumns");
                if (tbl != null)
                {
                    string key = GetGridKey();
                    DataRow[] rows = tbl.Select(string.Format("key='{0}'", key));
                    if (rows.Length > 0)
                    {
                        string colName = rows[0]["columns"].ToString();
                        string[] names = colName.Split(',');
                        string colwidth = rows[0]["widths"].ToString();
                        string[] widths = colwidth.Split(',');
                        for (int i = 0; i < names.Length; i++)
                        {
                            if (!ColumnWidths.ContainsKey(names[i]))
                            {
                                ColumnWidths.Add(names[i], Memory.GetInt(widths[i]));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        protected void SetGridColumnWidth()
        {
            this.GetGridColumnWithData();
            try
            {
                if (ColumnWidths != null && ColumnWidths.Count > 0)
                {
                    foreach (GridEXColumn col in this.RootTable.Columns)
                    {
                        if (ColumnWidths.ContainsKey(col.DataMember) && ColumnWidths[col.DataMember] > 0)
                        {
                            col.Width = ColumnWidths[col.DataMember];
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        public string GetGridKey()
        {
            return this.FindForm() != null ? this.FindForm().Name + "_" + this.Name : this.Name;
        }

        public void UpdateColumnWidthToMemory()
        {
            try
            {
                if (this.RootTable == null)
                {
                    return;
                }
                List<string> lstColumns = new List<string>();
                List<string> lstWidths = new List<string>();
                foreach (GridEXColumn col in this.RootTable.Columns)
                {
                    lstColumns.Add(col.DataMember);
                    lstWidths.Add(col.Width.ToString());
                }
                string key = GetGridKey();
                DataTable tbl = (DataTable)Memory.Instance.GetMemory("GridColumns");
                if (tbl != null)
                {
                    DataRow[] rows = tbl.Select(string.Format("key='{0}'", key));
                    DataRow row = null;
                    if (rows.Length > 0)
                    {
                        row = rows[0];
                    }
                    else
                    {
                        row = tbl.NewRow();
                        tbl.Rows.Add(row);
                        row["key"] = key;
                    }
                    row["columns"] = string.Join(",", lstColumns.ToArray());
                    row["widths"] = string.Join(",", lstWidths.ToArray());
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tbl);
                    ds.WriteXml(GxGrid.XmlPath);
                    tbl.AcceptChanges();
                    tbl.DataSet.Tables.Remove(tbl);
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            string s = "Đang tải dữ liệu, xin vui lòng đợi";
            LblLoadData.Text = s.PadRight(s.Length + counter, '.');
            if (counter == 5) counter = 0;
            counter++;
        }

        public virtual void FormatGrid()
        {
            this.VisualStyle = VisualStyle.Office2003;
            this.RootTable.RowHeight = 20;
            RowHeaderContent = RowHeaderContent.RowHeaderText;
        }

        private bool autoLoadGridFormat = true;
        private string selectDataQuery;

        public bool AutoLoadGridFormat
        {
            get { return autoLoadGridFormat; }
            set { autoLoadGridFormat = value; }
        }

        public virtual void ShowLoadDataControls()
        {

            if (disableParentOnLoadData)
            {
                Form main = this.FindForm();
                if (main != null && main.Visible)
                {
                    LblLoadData.Visible = true;
                    timer.Enabled = true;
                    this.Enabled = false;

                    if (main.Parent != null && main.Parent.Parent != null && main.Parent.Parent.Parent != null)
                    {
                        main.Parent.Parent.Parent.Enabled = false;
                    }
                    else if (main.Parent != null && main.Parent.Parent != null)
                    {
                        main.Parent.Parent.Enabled = false;
                    }
                    else if (main.Parent != null)
                    {
                        main.Parent.Enabled = false;
                    }
                    else
                    {
                        main.Enabled = false;
                    }
                }
            }
        }

        public virtual void HideLoadDataControls(object sender, EventArgs e)
        {
            if (LblLoadData.InvokeRequired || this.InvokeRequired)
            {
                EventHandler d = new EventHandler(HideLoadDataControls);
                this.Invoke(d, new object[] { sender, e });
                return;
            }
            if (disableParentOnLoadData)
            {
                Form main = this.FindForm();

                if (main != null)
                {
                    if (main.InvokeRequired)
                    {
                        EventHandler d = new EventHandler(HideLoadDataControls);
                        this.Invoke(d, new object[] { sender, e });
                        return;
                    }

                    if (main.Parent != null && main.Parent.Parent != null && main.Parent.Parent.Parent != null)
                    {
                        main.Parent.Parent.Parent.Enabled = true;
                    }
                    else if (main.Parent != null && main.Parent.Parent != null)
                    {
                        main.Parent.Parent.Enabled = true;
                    }
                    else if (main.Parent != null)
                    {
                        main.Parent.Enabled = true;
                    }
                    else
                    {
                        main.Enabled = true;
                    }
                }
            }
            this.Enabled = true;
            LblLoadData.Visible = false;
            timer.Enabled = false;           
        }

        public virtual void LoadData()
        {
            if (Memory.IsDesignMode) return;
            try
            {
                LoadData(QueryString, Arguments);

                //if (!Memory.ShowError() && tbl != null)
                //{
                //    tbl.TableName = GiaDinhConst.TableName;
                //    this.DataSource = tbl;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxGrid, LoadData)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public virtual void LoadData(string query, object[] args)
        {
            queryString = query ;
            arguments = args;
            LoadDataToGrid();
        }

        public void LoadDataToGrid()
        {
            if (Memory.IsDesignMode) return;

            ShowLoadDataControls();
            this.RemoveFilters();
            frmLoadDataProcess frm = new frmLoadDataProcess();
            frm.Query = queryString + whereSQL;
            frm.Parameters = arguments;
            frm.OnError += new EventHandler(frm_OnError);
            frm.OnFinished += new EventHandler(frm_OnFinished);
            ThreadStart threadStart = new ThreadStart(frm.Execute);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        private void frm_OnFinished(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler d = new EventHandler(frm_OnFinished);
                this.Invoke(d, new object[] { sender, e });
                return;
            }
            if (sender != null)
            {
                DataTable tbl = (DataTable)sender;
               
                if (this.DataSource != null && this.DataSource is DataTable)
                    ((DataTable)this.DataSource).Dispose();
                this.DataSource = tbl;
               

            }
            
            if (LoadDataFinished != null) LoadDataFinished(sender, e);
            HideLoadDataControls(sender, e);
        }

      

        private void frm_OnError(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                EventHandler d = new EventHandler(frm_OnError);
                this.Invoke(d, new object[] { sender, e });
                return;
            }
            MessageBox.Show("Tải dữ liệu thất bại\r\nXin vui lòng liên hệ người chịu trách nhiệm phần mềm để được giải quyết", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            HideLoadDataControls(sender, e);
        }

        public virtual void Print()
        {
            //Janus.Windows.GridEX.GridEXPrintDocument exp = new GridEXPrintDocument();
            //PrintPreviewDialog prd = new PrintPreviewDialog();
            //exp.GridEX = this;
            //exp.FitColumns = FitColumnsMode.SizingColumns;
            //exp.ExpandFarColumn = true;
            //exp.PrintHierarchical = true;
            //exp.DefaultPageSettings.Landscape = true;
            //prd.Document = exp;
            //prd.ShowDialog();

            Janus.Windows.GridEX.Export.GridEXExporter ex = new Janus.Windows.GridEX.Export.GridEXExporter();
            ex.GridEX = this;
            ex.IncludeFormatStyle = true;
            ex.IncludeHeaders = true;
            ex.SheetName = (this.DataSource as DataTable).TableName;
            
            string filePath = System.IO.Path.GetRandomFileName() + ".xls";
            filePath = Memory.GetTempPath(filePath);
            System.IO.FileInfo file = new System.IO.FileInfo(filePath);
            System.IO.FileStream stream = file.OpenWrite();
            ex.IncludeCollapsedRows = true;
            ex.Export((System.IO.Stream)stream);
            stream.Close();
            System.Diagnostics.Process.Start(filePath);

            //string templatePath = Memory.GetReportTemplatePath("common.xls");
            //string outputPath = Memory.GetTempPath("common.xls");
            //ExcelEngine excel = new ExcelEngine();
            //if (excel.CreateObject(outputPath, templatePath))
            //{
            //    try
            //    {
            //        //write header
            //        for (int i = 0; i < this.RootTable.Columns.Count; i++)
            //        {
            //            excel.Write_to_excel(1, i + 1, this.RootTable.Columns[i].Caption);
            //            excel.SetWidth(i + 1, this.RootTable.Columns[i].Width);
            //        }
            //        for (int i = 0; i < this.RowCount; i++)
            //        {
            //            for (int j = 0; j < this.RootTable.Columns.Count; j++)
            //            {
            //                object value = this.GetRow(i).Cells[j].Value;
            //                //xu ly 1 so truong hop dac biet
            //                if (this.RootTable.Columns[j].Key.StartsWith("Ngay") && !Memory.IsNullOrEmpty(value))
            //                {
            //                    value = string.Format("'{0}", value);
            //                }
            //                else if (this.RootTable.Columns[j].DataTypeCode == TypeCode.Boolean)
            //                {
            //                    value = (bool)value ? "X" : "";
            //                }
            //                excel.Write_to_excel(i + 2, j + 1, value);
            //            }
            //        }
            //        excel.End_Write();
            //        System.Diagnostics.Process.Start(outputPath);
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Có lỗi xảy ra khi in dữ liệu trên lưới" + Environment.NewLine +
            //            e.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //    finally
            //    {
                    
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Xuất báo cáo thất bại." + Environment.NewLine +
            //                                            "Có thể do bạn chưa cài MS Office 2003 trở lên", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        public GridEXColumn AddColumn(string colName, ColumnType colType, int width, ColumnBoundMode boundMode, string caption, FilterEditType editType)
        {
            return AddColumn(colName, colType, width, boundMode, caption, editType, TextAlignment.Near);
        }
        public GridEXColumn AddColumn(string colName, ColumnType colType, int width, ColumnBoundMode boundMode, string caption, FilterEditType editType, TextAlignment align)
        {
            if (this.RootTable == null) this.RootTable = new GridEXTable();
            GridEXColumn col = this.RootTable.Columns.Add(colName, colType);
            col.Width = width;
            col.BoundMode = boundMode;
            col.DataMember = colName;
            col.Caption = caption;
            col.FilterEditType = editType;
            col.TextAlignment = align;
            return col;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
