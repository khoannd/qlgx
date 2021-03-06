using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using GxControl;

namespace GiaoXu
{
    public partial class frmDotBiTichList : frmBase
    {
        public frmDotBiTichList()
        {
            InitializeComponent();
            this.HelpKey = "dot_bi_tich";
            //cbLoaiBiTich.Combo.SelectedIndex = 0;
            gxAddEdit1.Enabled = false;
            //cbLoaiBiTich.Combo.SelectedIndexChanged += new EventHandler(Combo_SelectedIndexChanged);
            gxAddEdit1.FindButton.Visible = true;
            gxHonPhoiList1.Visible = false;
            gxHonPhoiList1.Dock = DockStyle.Fill;
            gxDotBiTichList1.Visible = false;
            btnSearch.Click += SearchDotBiTich;
            for(int y = int.Parse(DateTime.Now.ToString("yyyy")); y >1900; y--)
            {
                cbTuNam.Combo.Items.Add(y);
                cbDenNam.Combo.Items.Add(y);
            }
            
        }

        private void SearchDotBiTich(object sender, EventArgs e)
        {
            if (cbLoaiBiTich.SelectedValue == null)
            {
                MessageBox.Show("Xin vui lòng chọn một loại bí tích cần xem");
                cbLoaiBiTich.Focus();
                return;
            }

            gxAddEdit1.Enabled = true;
            uiGroupBox1.Visible = true;
            
            if ((int)cbLoaiBiTich.SelectedValue == (int)LoaiBiTich.HonPhoi)
            {
                gxHonPhoiList1.Visible = true;
                gxDotBiTichList1.Visible = false;
                string sql = SqlConstants.SELECT_HONPHOI_LIST;
                List<object> args = new List<object>();
                if (cbTuNam.Text != "")
                {
                    sql += " AND INT(IIF(LEN([NgayHonPhoi])>=1,RIGHT([NgayHonPhoi], 4),\"0000\")) >= ? ";
                    args.Add(cbTuNam.Text);
                }
                if (cbDenNam.Text != "")
                {
                    sql += " AND INT(IIF(LEN([NgayHonPhoi])>=1,RIGHT([NgayHonPhoi], 4),\"0000\")) <= ? ";
                    args.Add(cbDenNam.Text);
                }
                sql += " ORDER BY " + Memory.ConvertDateToInt("NgayHonPhoi") + " ASC ";
                gxHonPhoiList1.LoadData(sql, args.ToArray());
            }
            else
            {
                gxDotBiTichList1.Visible = true;
                gxHonPhoiList1.Visible = false;
                gxDotBiTichList1.TuNam = Memory.GetInt(cbTuNam.Text, 0);
                gxDotBiTichList1.DenNam = Memory.GetInt(cbDenNam.Text, 0);
                gxDotBiTichList1.LoaiBiTich = (int)cbLoaiBiTich.SelectedValue;
                gxDotBiTichList1.LoadData();
            }
        }

        private void frmDotBiTichList_Load(object sender, EventArgs e)
        {
            gxDotBiTichList1.FormatGrid();
            gxHonPhoiList1.FormatGrid();
            cbDenNam.Text = DateTime.Now.ToString("yyyy");
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            if ((int)cbLoaiBiTich.SelectedValue == (int)LoaiBiTich.HonPhoi)
            {
                gxHonPhoiList1.AddRow();
                return;
            }
            frmBiTichChiTiet frm = new frmBiTichChiTiet();
            frm.Operation = GxOperation.ADD;
            frm.LoaiBiTich = (LoaiBiTich)cbLoaiBiTich.SelectedValue;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                DataTable tbl = (DataTable)gxDotBiTichList1.DataSource;
                if (tbl != null)
                {
                    if (frm.DataReturn != null)
                    {
                        tbl.ImportRow(frm.DataReturn);
                    }
                }
            }
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            EditRow();
        }

        /// <summary>
        /// Truong hop xoa 1 gia dinh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            if (gxDotBiTichList1.CurrentRow != null && (gxDotBiTichList1.CurrentRow.DataRow as DataRowView) != null)
            {
                DataRow currRow = (gxDotBiTichList1.CurrentRow.DataRow as DataRowView).Row;
                if (MessageBox.Show("Bạn có chắc muốn loại bỏ đợt bí tích này ra khỏi danh sách?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
                Memory.DeleteRows(BiTichChiTietConst.TableName, BiTichChiTietConst.MaDotBiTich, currRow[DotBiTichConst.MaDotBiTich]);
                Memory.DeleteRows(DotBiTichConst.TableName, DotBiTichConst.MaDotBiTich, currRow[DotBiTichConst.MaDotBiTich]);
                gxDotBiTichList1.CurrentRow.Delete();
            }
        }

        private void gxDotBiTichList1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {
            EditRow();
        }

        private void EditRow()
        {
            if((int)cbLoaiBiTich.SelectedValue == (int)LoaiBiTich.HonPhoi)
            {
                gxHonPhoiList1.EditRow();
                return;
            }

            if (gxDotBiTichList1.CurrentRow == null || (gxDotBiTichList1.CurrentRow.DataRow as DataRowView) == null)
            {
                return;
            }
            frmBiTichChiTiet frm = new frmBiTichChiTiet();
            frm.Operation = GxOperation.EDIT;
            frm.LoaiBiTich = (LoaiBiTich)cbLoaiBiTich.SelectedValue;
            DataRow row = (gxDotBiTichList1.CurrentRow.DataRow as DataRowView).Row;
            frm.MaDotBiTich = (int)row[DotBiTichConst.MaDotBiTich];
            frm.AssignControlData();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                row[DotBiTichConst.LinhMuc] = frm.DataReturn[DotBiTichConst.LinhMuc];
                row[DotBiTichConst.NgayBiTich] = frm.DataReturn[DotBiTichConst.NgayBiTich];
                row[DotBiTichConst.MoTa] = frm.DataReturn[DotBiTichConst.MoTa];
                row[DotBiTichConst.NoiBiTich] = frm.DataReturn[DotBiTichConst.NoiBiTich];
                row[DotBiTichConst.SoLuong] = frm.DataReturn[DotBiTichConst.SoLuong];
            }
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }
    }
}