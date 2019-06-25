using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using Janus.Windows.GridEX;

namespace GxControl
{
    public partial class GxListHoiDoan : GxGrid
    {

        public GxListHoiDoan()
        {
            InitializeComponent();
            FormatGrid();
            QueryString = SqlConstants.SELECT_LIST_HOIDOAN;
            LoadData();
        }
        public override void LoadData(string query, object[] args)
        {
            base.LoadData(query, args);
        }

        public override void LoadData()
        {
            base.LoadData();
        }

        public override void FormatGrid()
        {
            try
            {
                if (Memory.IsDesignMode) return;

                if (this.RootTable == null) this.RootTable = new GridEXTable();
                base.FormatGrid();

                this.RootTable.Columns.Clear();
                this.ColumnAutoResize = false;

                GridEXColumn col1 = this.RootTable.Columns.Add(HoiDoanConst.MaHoiDoan, ColumnType.Text);
                col1.Width = 100;
                col1.BoundMode = ColumnBoundMode.Bound;
                col1.DataMember = HoiDoanConst.MaHoiDoan;
                col1.Caption = "Mã hội đoàn";
                col1.FilterEditType = FilterEditType.Combo;

                GridEXColumn col2 = this.RootTable.Columns.Add(HoiDoanConst.TenHoiDoan,ColumnType.Text);
                col2.Width = 200;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = HoiDoanConst.TenHoiDoan;
                col2.Caption = "Tên hội đoàn";
                col2.FilterEditType = FilterEditType.Combo;

                GridEXColumn col4 = this.RootTable.Columns.Add(HoiDoanConst.ThanhBonMang, ColumnType.Text);
                col4.Width = 200;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = HoiDoanConst.ThanhBonMang;
                col4.Caption = "Thánh bổn mạng";
                col4.FilterEditType = FilterEditType.Combo;

                GridEXColumn col5 = this.RootTable.Columns.Add(HoiDoanConst.NgayBonMang, ColumnType.Text);
                col5.Width =180;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = HoiDoanConst.NgayBonMang;
                col5.Caption = "Ngày bổn mạng";
                col5.FilterEditType = FilterEditType.Combo;

                GridEXColumn col6 = this.RootTable.Columns.Add(HoiDoanConst.NgayThanhLap, ColumnType.Text);
                col6.Width = 180;
                col6.BoundMode = ColumnBoundMode.Bound;
                col6.DataMember = HoiDoanConst.NgayThanhLap;
                col6.Caption = "Ngày thành lập";
                col6.FilterEditType = FilterEditType.Combo;

                GridEXColumn col7 = this.RootTable.Columns.Add(HoiDoanConst.GhiChu, ColumnType.Text);
                col7.Width = 200;
                col7.BoundMode = ColumnBoundMode.Bound;
                col7.DataMember = HoiDoanConst.GhiChu;
                col7.Caption = "Ghi chú";
                col7.FilterEditType = FilterEditType.Combo;
                SetGridColumnWidth();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void gxListHoiDoan_RowDoubleClick(object sender, RowActionEventArgs e)
        {
            EditRow();
        }
        public void EditRow()
        {
            frmHoiDoan f = new frmHoiDoan();
            f.oldrow = (this.CurrentRow.DataRow as DataRowView).Row;
            f.ShowDialog();
            QueryString = SqlConstants.SELECT_LIST_HOIDOAN;
            this.LoadData();
        }

      
    }
}
