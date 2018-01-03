using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class frmChonGiaoDan : frmBase
    {
        private string lastSearch = "";
        private bool isSearching = false;
        private string whereSQL = "";

        public string WhereSQL
        {
            get { return whereSQL; }
            set { whereSQL = value; }
        }

        public frmChonGiaoDan()
        {
            InitializeComponent();
            txtHoTen.TextBox.Leave += new EventHandler(txtHoTen_Leave);
            txtHoTen.TextBox.KeyPress += new KeyPressEventHandler(TextBox_KeyPress);
            gxGiaoDanList1.AllowAddNew = Janus.Windows.GridEX.InheritableBoolean.False;
            gxGiaoDanList1.AllowDelete = Janus.Windows.GridEX.InheritableBoolean.False;
            gxGiaoDanList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            gxGiaoDanList1.AllowEditGiaoDan = false;
            gxGiaoDanList1.AllowShowForm = false;
            gxGiaoDanList1.LoadDataFinished += new EventHandler(gxGiaoDanList1_LoadDataFinished);
        }

        private void gxGiaoDanList1_LoadDataFinished(object sender, EventArgs e)
        {
            isSearching = false;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                txtHoTen.Text = Memory.AutoUpperFirstChar(txtHoTen.Text);
                search();
            }
        }

        private void txtHoTen_Leave(object sender, EventArgs e)
        {
            txtHoTen.Text = Memory.AutoUpperFirstChar(txtHoTen.Text);
            if (lastSearch.ToLower() == txtHoTen.Text.ToLower())
            {
                return;
            }
            if (this.ActiveControl.Equals(gxButton1))
            {
                search();
            }
        }

        private void search()
        {
            if (!isSearching)
            {
                if (txtHoTen.Text.Trim().Length >= 2)
                {
                    isSearching = true;
                    lastSearch = txtHoTen.Text;
                    gxGiaoDanList1.LoadData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + whereSQL + " AND HoTen LIKE ?", new object[] { "%" + txtHoTen.Text.Trim() });
                }
            }
        }

        private void gxGiaoDanList1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void gxGiaoDanList1_SelectionChanged(object sender, EventArgs e)
        {
            if (gxGiaoDanList1.CurrentRow != null && gxGiaoDanList1.CurrentRow.DataRow != null)
            {
                dataReturn = (gxGiaoDanList1.CurrentRow.DataRow as DataRowView).Row;
            }
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void frmChonGiaoDan_Load(object sender, EventArgs e)
        {
            gxCommand1.OKButton.Text = "Chọ&n";
            gxGiaoDanList1.FormatGrid();
            txtHoTen.TextBox.Focus();
            gxAddEdit1.AddButton.Visible = false;
            gxAddEdit1.EditButton.Visible = false;
            gxAddEdit1.SelectButton.Visible = false;
            gxAddEdit1.PrintButton.Visible = false;
            gxAddEdit1.DeleteButton.Visible = false;
            gxAddEdit1.ReloadButton.Visible = false;
            gxAddEdit1.FindButton.Visible = true;
            gxAddEdit1.Width = gxAddEdit1.FindButton.Width;

            gxAddEdit1.GridData = gxGiaoDanList1;

            this.ActiveControl = txtHoTen;
            txtHoTen.TextBox.Focus();
        }

        private void gxButton1_Click(object sender, EventArgs e)
        {
            search();
        }

        private void frmChonGiaoDan_VisibleChanged(object sender, EventArgs e)
        {
            
        }
    }
}
