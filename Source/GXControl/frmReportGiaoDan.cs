using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GXGlobal;

namespace GXControl
{
    public partial class frmReportGiaoDan : frmBase
    {
        public frmReportGiaoDan()
        {
            InitializeComponent();
            gxCommand1.OKButton.Text = "Xuất chứng nhận";
            gxCommand1.ToolTipOK = "Xuất giấy chứng nhận bí tích cho giáo dân này ra excel";
            gxCommand1.OKButton.Left = gxCommand1.CancelButton.Left - GXConstants.CONTROL_DIS - gxCommand1.OKButton.Width;
            this.AcceptButton = gxCommand1.OKButton;
            //load linh muc combobox
            DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ");
            if (tblLinhMuc != null)
            {
                foreach (DataRow row in tblLinhMuc.Rows)
                {
                    cbChaGui.Combo.Items.Add(row[LinhMucConst.TenThanh].ToString() + " " + row[LinhMucConst.HoTen].ToString());
                }
                if (cbChaGui.Combo.Items.Count > 0) cbChaGui.Combo.SelectedIndex = 0;
            }
        }

        public string TenGiaoDan
        {
            get { return txtGiaoDan.Text; }
            set { txtGiaoDan.Text = value; }
        }

        private int maGiaoDan = -1;

        public int MaGiaoDan
        {
            get { return maGiaoDan; }
            set { maGiaoDan = value; }
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            if (maGiaoDan == -1 || DataObj == null || DataObj.Tables.Count == 0 || !DataObj.Tables.Contains(GiaoDanConst.TableName))
            {
                MessageBox.Show("Rất tiếc!\r\nCó lỗi không mong muốn xảy ra. Vui lòng liên hệ với người chịu trách nhiệm phần mềm", "Lỗi DataObj null");
                return;

            }
            if (txtChaNhan.Text == "")
            {
                MessageBox.Show("Hãy nhập cha nhận chứng nhận!");
                txtChaNhan.Focus();
                return;
            }

            if (txtGiaoXuNhan.Text == "")
            {
                MessageBox.Show("Hãy nhập giáo xứ nhận chứng nhận!");
                txtGiaoXuNhan.Focus();
                return;
            }

            DataTable tblGiaDinh = Memory.GetData(SqlConstants.SELECT_CHA_ME, new object[] { maGiaoDan });
            if (Memory.Instance.ShowError())
            {
                return;
            }
            tblGiaDinh.TableName = GiaDinhConst.TableName;

            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.Instance.ShowError())
            {
                return;
            }
            if (tblGiaoXu.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thông tin giáo xứ. Vui lòng nhập thông tin giáo xứ trước khi sử dụng chức năng này.");
                return;
            }
            tblGiaoXu.TableName = GiaoXuConst.TableName;
            if (DataObj.Tables.Contains(GiaDinhConst.TableName)) DataObj.Tables.Remove(GiaDinhConst.TableName);
            DataObj.Tables.Add(tblGiaDinh);
            if (DataObj.Tables.Contains(GiaoXuConst.TableName)) DataObj.Tables.Remove(GiaoXuConst.TableName);
            DataObj.Tables.Add(tblGiaoXu);

            //for linh muc cua giao xu hien tai
            tblGiaoXu.Columns.Add(ReportGiaoDanConst.TenLinhMucGui);
            tblGiaoXu.Rows[0][ReportGiaoDanConst.TenLinhMucGui] = cbChaGui.Text;

            //thong tin giao xu nhan chung nhan
            DataTable tblGiaoXuNhan = new DataTable();
            tblGiaoXuNhan.TableName = ReportGiaoDanConst.TableName;
            tblGiaoXuNhan.Columns.Add(ReportGiaoDanConst.TenLinhMucNhan);
            tblGiaoXuNhan.Columns.Add(ReportGiaoDanConst.GiaoXuNhan);
            tblGiaoXuNhan.Columns.Add(ReportGiaoDanConst.LyDo);
            DataRow rowGiaoXuNhan = tblGiaoXuNhan.NewRow();
            rowGiaoXuNhan[ReportGiaoDanConst.TenLinhMucNhan] = txtChaNhan.Text;
            rowGiaoXuNhan[ReportGiaoDanConst.GiaoXuNhan] = txtGiaoXuNhan.Text;
            rowGiaoXuNhan[ReportGiaoDanConst.LyDo] = txtLyDo.Text;
            tblGiaoXuNhan.Rows.Add(rowGiaoXuNhan);
            if (DataObj.Tables.Contains(ReportGiaoDanConst.TableName)) DataObj.Tables.Remove(ReportGiaoDanConst.TableName);
            DataObj.Tables.Add(tblGiaoXuNhan);

            int rs = ExcelReport.ReportGiaoDan.Export(DataObj);
            Memory.Instance.ShowError();
        }

    }
}