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
    public partial class GxListHistoryHoiDoan : GxGrid
    {
        public GxListHistoryHoiDoan()
        {
            InitializeComponent();
            FormatGrid();
        }
        public override void FormatGrid()
        {
            if (Memory.IsDesignMode) return;

            if (this.RootTable == null) this.RootTable = new GridEXTable();
            base.FormatGrid();

            this.RootTable.Columns.Clear();
            this.ColumnAutoResize = false;

            GridEXColumn col = this.RootTable.Columns.Add(HoiDoanConst.TenHoiDoan, ColumnType.Text);
            col.Width = 200;
            col.BoundMode = ColumnBoundMode.Bound;
            col.DataMember = HoiDoanConst.TenHoiDoan;
            col.Caption = "Tên hội đoàn";
            col.FilterEditType = FilterEditType.Combo;
            col.EditType = EditType.NoEdit;

            GridEXColumn col1 = this.RootTable.Columns.Add(ChiTietHoiDoanConst.NgayVaoHoiDoan,ColumnType.Text);
            col1.Width = 210;
            col1.BoundMode = ColumnBoundMode.Bound;
            col1.DataMember = ChiTietHoiDoanConst.NgayVaoHoiDoan;
            col1.Caption = "Ngày vào hội đoàn";
            col1.EditType = EditType.NoEdit;
          //  col1.FilterEditType = FilterEditType.Combo;
          //  col1.EditType = EditType.CalendarDropDown;
            //col1.FormatMode = FormatMode.UseIFormattable;
            //col1.DefaultGroupFormatMode = FormatMode.UseIFormattable;
            //col1.DefaultGroupFormatString = "dd/MM/yyyy";
            //col1.FormatString = "dd/MM/yyyy";


            GridEXColumn col2 = this.RootTable.Columns.Add(ChiTietHoiDoanConst.NgayRaHoiDoan,ColumnType.Text);
            col2.Width = 210;
            col2.BoundMode = ColumnBoundMode.Bound;
            col2.DataMember = ChiTietHoiDoanConst.NgayRaHoiDoan;
            col2.Caption = "Ngày ra hội đoàn";
            col2.EditType = EditType.NoEdit;
        //    col2.FilterEditType = FilterEditType.Combo;
          //  col2.EditType = EditType.CalendarDropDown;


            GridEXColumn col3 = this.RootTable.Columns.Add(ChiTietHoiDoanConst.VaiTro,ColumnType.Text);
            col3.HasValueList = true;
            col3.Width = 226;
            col3.BoundMode = ColumnBoundMode.Bound;
            col3.DataMember = ChiTietHoiDoanConst.VaiTro;
            col3.Caption = "Vai trò";
            col3.EditType = EditType.NoEdit;
         //   col3.FilterEditType = FilterEditType.Combo;
         //   col3.EditType = EditType.DropDownList;
         //   GridEXValueListItemCollection vl = col3.ValueList;
         //   GxListHoiDoan.LoadVaiTroHoiDoan(vl);

            SetGridColumnWidth();

        }
        public override void LoadData()
        {
            base.LoadData();
        }
        public override void LoadData(string query, object[] args)
        {
            base.LoadData(query, args);
        }
    }
}
