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
            
            DialogResult tl = MessageBox.Show("Bạn có thật sự muốn xóa hội đoàn này!\r\nNếu có chọn [Yes].\r\nKhông chọn [No]","Cảnh báo",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2);
            if(tl==DialogResult.Yes)
            {
                if(gxListHoiDoan1.CurrentRow.DataRow!=null)
                {
                    string MaHoiDoan = gxListHoiDoan1.CurrentRow.Cells[HoiDoanConst.MaHoiDoan].Text.ToString();
                    Memory.ExecuteSqlCommand(String.Concat("Delete from ChiTietHoiDoan Where MaHoiDoan = ",MaHoiDoan));
                    Memory.ExecuteSqlCommand(String.Concat("Delete from HoiDoan Where MaHoiDoan = ",MaHoiDoan));
                    gxListHoiDoan1.CurrentRow.Delete();
                    gxListHoiDoan1.Refetch();
                    gxListHoiDoan1.Refresh();
                }
            }
            if(Memory.ShowError())
            {
                MessageBox.Show("Lỗi xóa hội đoàn");
                this.Close();
            }
        }

        private void frmHoiDoanList_Load(object sender, EventArgs e)
        {
            gxAddEdit1.SelectButton.Visible = false;
            gxAddEdit1.ReloadButton.Enabled = true;
            reloadGrid();
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            frmHoiDoan f = new frmHoiDoan();
            f.ShowDialog();
            gxListHoiDoan1.LoadData(SqlConstants.SELECT_LIST_HOIDOAN, null);
        }

        private void gxAddEdit1_ReloadClick(object sender, EventArgs e)
        {
            loaddata();
        }

        private void gxListHoiDoan1_SelectionChanged(object sender, EventArgs e)
        {
            reloadGrid();
        }
        private void reloadGrid()
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
        private void loaddata()
        {
            gxListHoiDoan1.LoadData(SqlConstants.SELECT_LIST_HOIDOAN, null);
            reloadGrid();
        }
    }
}
