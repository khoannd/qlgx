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
    public partial class GxListAccout : GxGrid
    {
        public GxListAccout()
        {
            InitializeComponent();
            FormatGrid();
            QueryString = SqlConstants.SELECT_LIST_ACCCOUT_WITH_NAME;
            LoadData();
           
        }
        public override void LoadData()
        {
            QueryString = SqlConstants.SELECT_LIST_ACCCOUT_WITH_NAME;
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
                GridEXColumn col1 = this.RootTable.Columns.Add(AccountConst.HoTenNguoiDung, ColumnType.Text);
                col1.Width = 200;
                col1.BoundMode = ColumnBoundMode.Bound;
                col1.DataMember = AccountConst.HoTenNguoiDung;
                col1.Caption = "Tên người dùng";
                col1.FilterEditType = FilterEditType.Combo;

                GridEXColumn col2 = this.RootTable.Columns.Add(AccountConst.TenTaiKhoan, ColumnType.Text);
                col2.Width = 200;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = AccountConst.TenTaiKhoan;
                col2.Caption = "Tên tài khoản";
                col2.FilterEditType = FilterEditType.Combo;

                GridEXColumn col3 = this.RootTable.Columns.Add(AccountConst.Email, ColumnType.Text);
                col3.Width = 200;
                col3.BoundMode = ColumnBoundMode.Bound;
                col3.DataMember = AccountConst.Email;
                col3.Caption = "Email";
                col3.FilterEditType = FilterEditType.Combo;

                GridEXColumn col4 = this.RootTable.Columns.Add(AccountConst.SoDienThoai, ColumnType.Text);
                col4.Width = 150;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = AccountConst.SoDienThoai;
                col4.Caption = "Số điện thoại";
                col4.FilterEditType = FilterEditType.DropDownList;

                GridEXColumn col5 = this.RootTable.Columns.Add(AccountConst.TenLoai, ColumnType.Text);
                col5.Width = 100;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = AccountConst.TenLoai;
                col5.TextAlignment = TextAlignment.Far;
                col5.Caption = "Loại tài khoản";
                col5.FilterEditType = FilterEditType.Combo;
                SetGridColumnWidth();
            }
            catch
            {

            }
            }
        
    }
}
