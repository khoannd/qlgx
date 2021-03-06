using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxControl;
using GxGlobal;
using Janus.Windows.GridEX;

namespace GiaoLy
{
    public partial class frmChuyenLop : frmBase
    {
        public frmChuyenLop()
        {
            InitializeComponent();
            gxCommand1.OKButton.Text = "&Chuyển";
            this.AcceptButton = gxCommand1.OKButton;
        }
        int malop = -1;
        public int MaLop
        {
            get { return malop; }
            set { malop = value; }
        }
        private int namGLy = 0;

        public int NamGiaoLy
        {
            get { return namGLy; }
            set
            {
                namGLy = value;
            }
        }
        private void frmChuyenLop_Load(object sender, EventArgs e)
        {
            string sql = string.Format("SELECT aa.SoThuTu, aa.TenThanh, aa.HoTen, aa.Phai, aa.NgaySinh, GiaoHo.TenGiaoHo, aa.HoanThanh, aa.GhiChuGLy,1 AS Chon2 FROM (SELECT b.*,a.HoanThanh,a.GhiChuGLy,a.SoThuTu FROM ChiTietLopGiaoLy a  INNER JOIN GiaoDan b ON a.MaGiaoDan = b.MaGiaoDan WHERE (a.Malop = ?)) aa LEFT OUTER JOIN GiaoHo ON aa.MaGiaoHo = GiaoHo.MaGiaoHo WHERE (aa.DaXoa = 0)");
            gxHocSinhList1.AllowEdit = InheritableBoolean.True;
            gxHocSinhList1.FormatGrid2();
            gxHocSinhList1.MaLop = MaLop;
            gxHocSinhList1.LoadData();
            
            loadCmbDefaul();
            btnCheckAll.Visible = true;
            btnUncheckAll.Visible = true;
        }

        private void loadCmbDefaul()
        {
            int i;
            for (i = 2000; i < 2050; i++)
            {
                cbNam.Items.Add(i.ToString());
                if (i == NamGiaoLy+1)
                {
                    cbNam.SelectedIndex = i - 2000;
                }
            }
            cbNam.Enabled = true;
            DataTable tblKhoi = Memory.GetData("select makhoi,tenkhoi from KhoiGiaoLy ");
            cbKhoi.ValueMember = "makhoi";
            cbKhoi.DisplayMember = "tenkhoi";
            Janus.Windows.EditControls.UIComboBoxItem it = new Janus.Windows.EditControls.UIComboBoxItem();
            it.Text = " - - Chọn khối - - ";
            it.Value = -1;
            cbKhoi.Items.Add(it);
            if (tblKhoi.Rows.Count > 0)
            {
                foreach (DataRow dr in tblKhoi.Rows)
                {
                    it = new Janus.Windows.EditControls.UIComboBoxItem();
                    it.Value = dr["makhoi"];
                    it.Text = dr["tenkhoi"].ToString();
                    cbKhoi.Items.Add(it);
                }
            }
           
            cbKhoi.SelectedIndex = 0;
            cbLop.Items.Add(" - - Chọn lớp - - ");
            cbLop.SelectedIndex = 0;
            this.cbKhoi.SelectedIndexChanged += new System.EventHandler(this.cbKhoi_SelectedIndexChanged);
            this.cbNam.SelectedIndexChanged += new System.EventHandler(this.cbNam_SelectedIndexChanged);
        }

        private void cbNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadComboLop();
        }

        private void cbKhoi_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadComboLop();
        }

        private void loadComboLop()
        {
            DataTable tblLop = Memory.GetData("select malop,tenlop from LopGiaoLy where makhoi = ? and Nam = ?", new object[] { cbKhoi.SelectedItem.Value, cbNam.Text });
            cbLop.Items.Clear();
            Janus.Windows.EditControls.UIComboBoxItem it = new Janus.Windows.EditControls.UIComboBoxItem();
            it.Text = " - - Chọn lớp - - ";
            it.Value = -1;
            cbLop.Items.Add(it);
            if (tblLop.Rows.Count > 0)
            {
                foreach (DataRow dr in tblLop.Rows)
                {
                    it = new Janus.Windows.EditControls.UIComboBoxItem();
                    it.Value = dr["malop"];
                    it.Text = dr["tenlop"].ToString();
                    cbLop.Items.Add(it);
                }
            }

            cbLop.SelectedIndex = 0;
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (Janus.Windows.GridEX.GridEXRow item in gxHocSinhList1.GetRows())
            {
                item.BeginEdit();
                item.Cells["Chon"].Value = true;
                item.EndEdit();
            }
        }

        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            foreach (Janus.Windows.GridEX.GridEXRow item in gxHocSinhList1.GetRows())
            {
                item.BeginEdit();
                item.Cells["Chon"].Value = false;
                item.EndEdit();
            }
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            try{
                if (cbKhoi.SelectedIndex == 0)
                {
                    MessageBox.Show("Hãy chọn khối giáo lý");
                    return;
                }
                if (cbLop.SelectedIndex == 0)
                {
                    MessageBox.Show("Hãy chọn lớp giáo lý");
                    return;
                }
                if (MessageBox.Show("Danh sách chọn sẽ chuyển lên lớp " + cbLop.SelectedItem.Text + " khối " + cbKhoi.SelectedItem.Text + " năm học " + cbNam.SelectedItem.Text, "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DataTable tblHS = Memory.GetData("select * from chitietlopgiaoly where malop = ?", new object[] { cbLop.SelectedValue });
                    bool flg = false;
                    int soThuTuNext = 0;
                    foreach (Janus.Windows.GridEX.GridEXRow item in gxHocSinhList1.GetRows())
                    {
                        
                        if ((bool)item.Cells["Chon"].Value == true)
                        {
                            if (Memory.GetData("select * from chitietlopgiaoly where malop = ? and magiaodan = ?", new object[] { cbLop.SelectedValue, item.Cells["MaGiaoDan"].Value }).Rows.Count > 0)
                            {
                                MessageBox.Show("Giáo dân " + item.Cells["HoTen"].Value + " đã tồn tại trong lớp " + cbLop.SelectedItem.Text);
                            }
                            else
                            {
                                DataRow dr = tblHS.NewRow();
                                if (!flg)
                                {
                                    DataTable tblTmp = Memory.GetData("select max(sothutu) from chitietlopgiaoly where malop = ?", new object[] { cbLop.SelectedValue });
                                    if (tblTmp.Rows[0][0] != System.DBNull.Value)
                                    {
                                        soThuTuNext = Convert.ToInt32(tblTmp.Rows[0][0].ToString());
                                        flg = true;
                                    }
                                }
                                soThuTuNext = soThuTuNext + 1;
                                dr["MaLop"] = cbLop.SelectedValue;
                                dr["MaGiaoDan"] = item.Cells["MaGiaoDan"].Value;
                                dr["SoThuTu"] = soThuTuNext;
                                dr["HoanThanh"] = false;
                                dr["GhiChuGLy"] = "";
                                tblHS.Rows.Add(dr);
                            }
                            
                        }
                        
                    }
                    DataSet ds = new DataSet();
                    tblHS.TableName = "chitietlopgiaoly";
                    ds.Tables.Add(tblHS);
                    Memory.UpdateDataSet(ds);
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (frmLopGiaoLy, gxCommand1_OnOK)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}