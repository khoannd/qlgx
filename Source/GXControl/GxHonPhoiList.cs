using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;
using GxGlobal;

namespace GxControl
{
    public partial class GxHonPhoiList : GxGrid
    {
        private ContextMenu contextMenu = new ContextMenu();
        public GxHonPhoiList()
        {
            InitializeComponent();
            this.AllowAddNew = InheritableBoolean.False;
            this.AllowDelete = InheritableBoolean.False;
            this.AllowEdit = InheritableBoolean.False;
            MenuItem item1 = new MenuItem("&Xem chi tiết");
            item1.Click += new EventHandler(item1_Click);
            contextMenu.MenuItems.Add(item1);
            MenuItem item2 = new MenuItem("&Chứng nhận hôn phối");
            item2.Click += new EventHandler(item2_Click);
            contextMenu.MenuItems.Add(item2);

            this.ContextMenu = contextMenu;

        }

        private int maGiaoDan = -1;

        public int MaGiaoDan
        {
            get { return maGiaoDan; }
            set { maGiaoDan = value; }
        }

        private string phai = "";

        public string Phai
        {
            get { return phai; }
            set { phai = value; }
        }

        private int listMode = 1;
        /// <summary>
        /// 1: used for giao dan form (only vo or chong)
        /// 2: used for thong ke form (include vo and chong)
        /// </summary>
        public int ListMode
        {
            get { return listMode; }
            set { listMode = value; }
        }

        public override void FormatGrid()
        {
            if (Memory.IsDesignMode) return;
            
            if (this.RootTable == null) this.RootTable = new GridEXTable();
            base.FormatGrid();

            this.RootTable.Columns.Clear();
            this.ColumnAutoResize = false;
            if (listMode == 1)
            {
                AddColumn(GiaoDanHonPhoiConst.SoThuTu, ColumnType.Text, 60, ColumnBoundMode.Bound, "STT", FilterEditType.Combo, TextAlignment.Far);
            }
            else if (listMode == 2)
            {
                AddColumn(HonPhoiConst.MaHonPhoi, ColumnType.Text, 60, ColumnBoundMode.Bound, "Mã HP", FilterEditType.Combo);
            }
            if (listMode == 1)
            {
                string vochong = "Chồng";
                if (phai.ToLower() == GxConstants.NAM.ToLower())
                {
                    vochong = "Vợ";
                }
                AddColumn(HonPhoiConst.VoChong, ColumnType.Text, 100, ColumnBoundMode.Bound, vochong, FilterEditType.Combo);
            }
            else if (listMode == 2)
            {
                AddColumn("Nam", ColumnType.Text, 100, ColumnBoundMode.Bound, "Chồng", FilterEditType.Combo);
                AddColumn("Nữ", ColumnType.Text, 100, ColumnBoundMode.Bound, "Vợ", FilterEditType.Combo);
            }
            AddColumn(HonPhoiConst.SoHonPhoi, ColumnType.Text, 80, ColumnBoundMode.Bound, "Số HP", FilterEditType.Combo);
            AddColumn(HonPhoiConst.NoiHonPhoi, ColumnType.Text, 100, ColumnBoundMode.Bound, "Nơi HP", FilterEditType.Combo);
            AddColumn(HonPhoiConst.NgayHonPhoi, ColumnType.Text, 80, ColumnBoundMode.Bound, "Ngày HP", FilterEditType.Combo, TextAlignment.Far);
            AddColumn(HonPhoiConst.SoNamHonPhoi, ColumnType.Text, 80, ColumnBoundMode.Bound, "Số năm HP", FilterEditType.Combo);
            AddColumn(HonPhoiConst.CachThucHonPhoi, ColumnType.Text, 100, ColumnBoundMode.Bound, "Cách thức HP", FilterEditType.Combo);
            AddColumn(HonPhoiConst.LinhMucChung, ColumnType.Text, 100, ColumnBoundMode.Bound, "Linh mục chứng", FilterEditType.Combo);
            AddColumn(HonPhoiConst.NguoiChung1, ColumnType.Text, 100, ColumnBoundMode.Bound, "Người chứng 1", FilterEditType.Combo);
            AddColumn(HonPhoiConst.NguoiChung2, ColumnType.Text, 100, ColumnBoundMode.Bound, "Người chứng 2", FilterEditType.Combo);
            AddColumn(HonPhoiConst.GhiChu, ColumnType.Text, 200, ColumnBoundMode.Bound, "Ghi chú", FilterEditType.Combo);
            AddColumn(GiaoDanConst.QuaDoi, ColumnType.Text, 50, ColumnBoundMode.Bound, "Qua Đời", FilterEditType.CheckBox);
            if (listMode == 2)
                AddColumn(GiaoHoConst.TenGiaoHo, ColumnType.Text, 80, ColumnBoundMode.Bound, "Giáo Họ", FilterEditType.Combo);

            //addcondition
            GridEXFormatCondition con1 = new GridEXFormatCondition(this.RootTable.Columns[GiaoDanConst.QuaDoi], ConditionOperator.Equal, -1);
            con1.FormatStyle = new GridEXFormatStyle();
            con1.FormatStyle.ForeColor = Color.Red;
            con1.FormatStyle.FontStrikeout = TriState.True;
            this.RootTable.FormatConditions.Add(con1);
            SetGridColumnWidth();
        }

        private void item1_Click(object sender, EventArgs e)
        {
            EditRow();
        }

        private void item2_Click(object sender, EventArgs e)
        {
            XuatChungNhanHonPhoi();
        }

        public void XuatChungNhanHonPhoi()
        {
            if (this.SelectedItems.Count > 1)
            {
                foreach (Janus.Windows.GridEX.GridEXSelectedItem item in this.SelectedItems)
                {
                    GxGiaDinhList.ChungNhanHonPhoi((int)(item.GetRow().DataRow as DataRowView).Row[HonPhoiConst.MaHonPhoi], false);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
            }
            else if (this.CurrentRow != null && (this.CurrentRow.DataRow is DataRowView))
            {
                GxGiaDinhList.ChungNhanHonPhoi((int)(this.CurrentRow.DataRow as DataRowView).Row[HonPhoiConst.MaHonPhoi], false);
            }
        }
        public bool LoadData2()
        {
            //select ma hon phoi
            LoadData(SqlConstants.SELECT_HONPHOI_THEO_MAGIAODAN2 + " AND GiaoDanHonPhoi_1.MaGiaoDan <> ?", new object[] { maGiaoDan, maGiaoDan });
            return true;
        }

        public bool LoadData()
        {
            //select ma hon phoi
            LoadData(SqlConstants.SELECT_HONPHOI_THEO_MAGIAODAN2 + " AND GiaoDanHonPhoi_1.MaGiaoDan <> ?", new object[] { maGiaoDan, maGiaoDan });

            return true;
        }

        private void GxHonPhoiList_RowDoubleClick(object sender, RowActionEventArgs e)
        {
            EditRow();
        }
        public void EditRow()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            frmHonPhoi frmHP = new frmHonPhoi();
            frmHP.Operation = GxOperation.EDIT;
            frmHP.MaHonPhoi = (int)row[HonPhoiConst.MaHonPhoi];
            frmHP.AssignControlData();
            if (frmHP.ShowDialog() == DialogResult.OK && frmHP.DataReturn != null)
            {
                string sql = SqlConstants.SELECT_HONPHOI_LIST + " AND MaHonPhoi = ?";
                DataTable tbl = Memory.GetData(sql, frmHP.DataReturn[HonPhoiConst.MaHonPhoi]);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    DataRow row2 = tbl.Rows[0];
                    foreach(DataColumn col in row.Table.Columns)
                    {
                        if(row2.Table.Columns.Contains(col.ColumnName))
                        {
                            row[col] = row2[col.ColumnName];
                        }
                    }
                    this.Refresh();
                }
            }
        }
        public void AddRow()
        {
            frmHonPhoi frmHP = new frmHonPhoi();
            frmHP.Operation = GxOperation.ADD;
            if(frmHP.ShowDialog() == DialogResult.OK && frmHP.DataReturn != null)
            {
                string sql = SqlConstants.SELECT_HONPHOI_LIST + " AND MaHonPhoi = ?";
                DataTable tbl = Memory.GetData(sql, frmHP.DataReturn[HonPhoiConst.MaHonPhoi]);
                if(tbl != null && tbl.Rows.Count > 0)
                {
                    ((DataTable)this.DataSource).ImportRow(tbl.Rows[0]);
                    this.FindAll(this.RootTable.Columns[HonPhoiConst.MaHonPhoi], Janus.Windows.GridEX.ConditionOperator.Equal, frmHP.DataReturn[HonPhoiConst.MaHonPhoi]);
                }
            }
        }

        private void GxHonPhoiList_FormattingRow(object sender, RowLoadEventArgs e)
        {
            //DataTable tbl = (DataTable)this.DataSource;
            //if (tbl!=null&&tbl.Rows.Count>0)
            //{
            //    for (int i = 0; i < tbl.Rows.Count; i++)
            //    {
            //        bool temp =(bool) tbl.Rows[i][GiaoDanConst.QuaDoi];
            //        if (temp==true)
            //        {
            //            GridEXFormatStyle style = new GridEXFormatStyle();
            //            style.FontStrikeout = TriState.True;
            //            this.GetRow(i).RowStyle = style;  
            //        }
            //    }
            //}
           
        }
    }
}
