using GxControl;
using GxGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmHoiDoanList : frmBase
    {
       
        public frmHoiDoanList()
        {
            InitializeComponent();
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            gxListHoiDoan1.EditRow();
        }

        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            MessageBox.Show("Xóa hội đoàn");
        }

        private void frmHoiDoanList_Load(object sender, EventArgs e)
        {
            gxAddEdit1.SelectButton.Visible = false;
            gxAddEdit1.ReloadButton.Enabled = true;
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            frmHoiDoan f = new frmHoiDoan();
            f.ShowDialog();
            gxListHoiDoan1.LoadData(SqlConstants.SELECT_LIST_HOIDOAN, null);
        }

        private void gxAddEdit1_ReloadClick(object sender, EventArgs e)
        {
            gxListHoiDoan1.LoadData(SqlConstants.SELECT_LIST_HOIDOAN, null);
        }

        private void gxListHoiDoan1_SelectionChanged(object sender, EventArgs e)
        {
            if (gxListHoiDoan1.CurrentRow == null || (gxListHoiDoan1.CurrentRow.DataRow as DataRowView) == null)
            {
                gxAddEdit1.DeleteButton.Enabled = false;
                gxAddEdit1.EditButton.Enabled = false;
                return;
            }
            gxAddEdit1.DeleteButton.Enabled = true;
            gxAddEdit1.EditButton.Enabled = true;
        }
    }
}
