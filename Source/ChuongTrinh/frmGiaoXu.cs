using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using GxControl;
using GxGlobal;
using Newtonsoft.Json;

namespace GiaoXu
{
    public partial class frmGiaoXu : frmBase
    {
        //cờ cho biết là giáo xứ có được request lên server không
        public int hasRequest = 0;
        DataTable tbl=null;
        public object ListGiaoHat
        {
            set
            {
                if(value != null)
                {
                    cbGiaoHat.combo.DataSource = value;
                    cbGiaoHat.combo.ValueMember = "MaGiaoHat";
                    cbGiaoHat.combo.DisplayMember = "TenGiaoHat";
                    if(tbl!=null && tbl.Rows.Count>0)
                    {
                        cbGiaoPhan.Text= tbl.Rows[0][GiaoPhanConst.TenGiaoPhan].ToString();
                        cbGiaoHat.Text= tbl.Rows[0][GiaoHatConst.TenGiaoHat].ToString();
                        txtTenGiaoXu.Text = tbl.Rows[0][GiaoXuConst.TenGiaoXu].ToString();
                        txtDiaChi.Text = tbl.Rows[0][GiaoXuConst.DiaChi].ToString();
                        txtWebsite.Text = tbl.Rows[0][GiaoXuConst.Website].ToString();
                        txtDienThoai.Text = tbl.Rows[0][GiaoXuConst.DienThoai].ToString();
                        txtEmail.Text = tbl.Rows[0][GiaoXuConst.Email].ToString();
                        txtHinh.Text = tbl.Rows[0][GiaoXuConst.Hinh].ToString();
                    }
                }
            }
            get
            {
                return cbGiaoHat.combo.DataSource;
            }
        }
        public object ListGiaoPhan
        {
            set
            {
                if(value != null)
                {
                    cbGiaoPhan.combo.DataSource = value;
                    cbGiaoPhan.combo.DisplayMember = "TenGiaoPhan";
                    cbGiaoPhan.combo.ValueMember = "MaGiaoPhan";
                    cbGiaoPhan.SelectedIndex = 0;
                }
                cbGiaoHat.Visible = true;
                cbGiaoPhan.Visible = true;
                linkLbGiaoHat.Visible = true;
                linkLbGiaoPhan.Visible = true;
                txtTenGiaoPhan.Size = new Size(365, txtTenGiaoPhan.Size.Height);
                txtTenGiaoHat.Size = new Size(365, txtTenGiaoHat.Size.Height);
            }
            get
            {
                return cbGiaoPhan.combo.DataSource;
            }
        }
        public frmGiaoXu()
        {
            InitializeComponent();

            this.HelpKey = "giao_xu";

            gxAddEdit1.SelectButton.Visible = false;
            this.AcceptButton = gxAddEdit1.AddButton;
            this.cbGiaoPhan.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbGiaoHat.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbGiaoPhan.combo.SelectedIndexChanged += Combo_SelectedIndexChanged;
        }

        private void Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbGiaoPhan.SelectedValue;
            if (item == null) return;
            if (item is GiaoPhan)
            {
                item = (cbGiaoPhan.SelectedValue as GiaoPhan).MaGiaoPhan;
            }
            var giaoHats = GetGiaoHatsInGiaoPhan(item.ToString());
            ListGiaoHat = giaoHats;
        }

        public List<GiaoHat> GetGiaoHatsInGiaoPhan(string giaoPhanId)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_LIST_GIAO_HAT_THEO_GIAO_PHAN, giaoPhanId));
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers["PassWord"] = "admin";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                var rs = responds.GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<GiaoHat>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void frmGiaoXu_Load(object sender, EventArgs e)
        {
            tbl = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.ShowError()) return;
            if (tbl.Rows.Count > 0)
            {
                txtTenGiaoPhan.Text = tbl.Rows[0][GiaoPhanConst.TenGiaoPhan].ToString();
                txtTenGiaoHat.Text = tbl.Rows[0][GiaoHatConst.TenGiaoHat].ToString();
                cbGiaoHat.SelectedText = tbl.Rows[0][GiaoHatConst.TenGiaoHat].ToString();
                cbGiaoPhan.SelectedText = tbl.Rows[0][GiaoPhanConst.TenGiaoPhan].ToString();
                txtTenGiaoXu.Text = tbl.Rows[0][GiaoXuConst.TenGiaoXu].ToString();
                txtDiaChi.Text = tbl.Rows[0][GiaoXuConst.DiaChi].ToString();
                txtWebsite.Text = tbl.Rows[0][GiaoXuConst.Website].ToString();
                txtDienThoai.Text = tbl.Rows[0][GiaoXuConst.DienThoai].ToString();
                txtEmail.Text = tbl.Rows[0][GiaoXuConst.Email].ToString();
                txtHinh.Text = tbl.Rows[0][GiaoXuConst.Hinh].ToString();
                txtGhiChu.Rtf = tbl.Rows[0][GiaoXuConst.GhiChu].ToString();
            }
            gxLinhMucList1.FormatGrid();
            gxLinhMucList1.LoadData();
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            EditLinhMucRow(GxOperation.ADD);
        }

        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            if (gxLinhMucList1.CurrentRow != null && (gxLinhMucList1.CurrentRow.DataRow as DataRowView) != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa thông tin Linh mục này?", "Xác nhận xóa", MessageBoxButtons.YesNo) == DialogResult.No) return;
                Memory.ExecuteSqlCommand(SqlConstants.DELETE_LINHMUC_THEO_ID,
                                                    new object[] { (gxLinhMucList1.CurrentRow.DataRow as DataRowView).Row[LinhMucConst.MaLinhMuc] });
                if (Memory.ShowError())
                {
                    return;
                }
                gxLinhMucList1.CurrentRow.Delete();

            }
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            EditLinhMucRow(GxOperation.EDIT);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenGiaoPhan.Text) && !cbGiaoPhan.Visible)
            {
                MessageBox.Show("Hãy nhập tên giáo phận!");
                txtTenGiaoPhan.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtTenGiaoHat.Text) && !cbGiaoHat.Visible)
            {
                MessageBox.Show("Hãy nhập tên giáo hạt!");
                txtTenGiaoHat.Focus();
                return;
            }
            if (cbGiaoPhan.Visible && cbGiaoPhan.SelectedIndex < 0)
            {
                MessageBox.Show("Hãy chọn giáo phận!");
                cbGiaoPhan.Focus();
                return;
            }
            if (cbGiaoHat.Visible && cbGiaoHat.SelectedIndex < 0)
            {
                MessageBox.Show("Hãy chọn giáo hạt!");
                cbGiaoHat.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtTenGiaoXu.Text))
            {
                MessageBox.Show("Hãy nhập tên giáo xứ!");
                txtTenGiaoXu.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtDiaChi.Text))
            {
                MessageBox.Show("Hãy nhập địa chỉ giáo xứ!");
                txtDiaChi.Focus();
                return;
            }
            DataSet ds = new DataSet();

            //for giao phan
            DataTable tblGiaoPhan = Memory.GetData(SqlConstants.SELECT_GIAOPHAN);
            if (Memory.ShowError()) return;
            if (tblGiaoPhan.Rows.Count > 0)
            {
                tblGiaoPhan.Rows[0][GiaoPhanConst.TenGiaoPhan] = txtTenGiaoPhan.Text;
            }
            else
            {
                DataRow row = tblGiaoPhan.NewRow();
                row[GiaoPhanConst.MaGiaoPhan] = Memory.Instance.GetNextId(GiaoPhanConst.TableName, GiaoPhanConst.MaGiaoPhan, true);
                row[GiaoPhanConst.TenGiaoPhan] = txtTenGiaoPhan.Text;
                if (cbGiaoPhan.Visible)
                {
                    row[GiaoPhanConst.TenGiaoPhan] = cbGiaoPhan.Text;
                    row[GiaoPhanConst.MaGiaoPhanRieng] = cbGiaoPhan.SelectedValue;
                }
                tblGiaoPhan.Rows.Add(row);
            }
            tblGiaoPhan.TableName = GiaoPhanConst.TableName;
            ds.Tables.Add(tblGiaoPhan);

            //for giao hat
            DataTable tblGiaoHat = Memory.GetData(SqlConstants.SELECT_GIAOHAT);
            if (Memory.ShowError()) return;
            if (tblGiaoHat.Rows.Count > 0)
            {
                tblGiaoHat.Rows[0][GiaoHatConst.TenGiaoHat] = txtTenGiaoHat.Text;
            }
            else
            {
                DataRow row = tblGiaoHat.NewRow();
                row[GiaoHatConst.MaGiaoHat] = Memory.Instance.GetNextId(GiaoHatConst.TableName, GiaoHatConst.MaGiaoHat, true);
                row[GiaoHatConst.TenGiaoHat] = txtTenGiaoHat.Text;
                if (cbGiaoHat.Visible)
                {
                    row[GiaoHatConst.MaGiaoHatRieng] = cbGiaoHat.SelectedValue;
                    row[GiaoHatConst.TenGiaoHat] = cbGiaoHat.Text;
                }
                row[GiaoHatConst.MaGiaoPhan] = tblGiaoPhan.Rows[0][GiaoPhanConst.MaGiaoPhan];
                tblGiaoHat.Rows.Add(row);
            }            
            tblGiaoHat.TableName = GiaoHatConst.TableName;
            ds.Tables.Add(tblGiaoHat);

            //for giao xu
            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.ShowError()) return;
            if (tblGiaoXu.Rows.Count > 0)
            {
                tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXu] = Memory.Instance.GetNextId(GiaoXuConst.TableName, GiaoXuConst.MaGiaoXu, true);
                AssignGXData(tblGiaoXu.Rows[0]);
            }
            else
            {
                DataRow row = tblGiaoXu.NewRow();
                AssignGXData(row);
                row[GiaoXuConst.MaGiaoHat] = tblGiaoHat.Rows[0][GiaoHatConst.MaGiaoHat];
                tblGiaoXu.Rows.Add(row);                
            }
            
            tblGiaoXu.TableName = GiaoXuConst.TableName;
            ds.Tables.Add(tblGiaoXu);
            Memory.UpdateDataSet(ds);
            if (!Memory.ShowError())
            {
                this.Text += txtTenGiaoXu.Text;
                MessageBox.Show("Đã cập nhật thông tin giáo xứ!");
            }
            //request yêu cầu lên 
            if (Memory.TestConnectToServer()&&Memory.GetConfig(GxConstants.BACKUP_DATA_TO_SERVER)=="1" && hasRequest==1)
            {
                if(tblGiaoXu!=null && tblGiaoXu.Rows.Count>0 && tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString()=="")
                insertInfoGXInFirstTime();
            }
        }
        private int insertInfoGXInFirstTime()
        {
            try
            {
                WebClient cl = new WebClient();
                NameValueCollection infoGX = new NameValueCollection();
                infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOXU), GiaoXuConst.TableName));
                //infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOHAT), GiaoHatConst.TableName, GiaoHatConst.MaGiaoHatRieng));
                //infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOPHAN), GiaoPhanConst.TableName, GiaoPhanConst.MaGiaoPhanRieng));
                if (infoGX.Count > 0)
                {
                    byte[] rs = cl.UploadValues(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/insertGiaoXuDoi", "POST", infoGX);
                    string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                    Dictionary<string, int> maID = JsonConvert.DeserializeObject<Dictionary<string, int>>(temp);
                    if (maID.Count > 0)  
                    {
                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOXUDOI, new object[] { maID["MaGiaoXuDoi"] });

                        //if (maID.ContainsKey("IDGP"))
                        //{
                        //    if (int.Parse(maID["IDGP"].ToString()) >= 0)
                        //        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOPHANRIENG, new object[] { maID["IDGP"] });
                        //}
                        //if (maID.ContainsKey("IDGH"))
                        //{
                        //    if (int.Parse(maID["IDGH"].ToString()) >= 0)
                        //        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOHATRIENG, new object[] { maID["IDGH"] });
                        //}
                        //if (maID.ContainsKey("IDGX"))
                        //{
                        //    Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOXURIENG, new object[] { maID["IDGX"] });
                        //}
                        //return maID["IDGX"];
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "frmGiaoXu");
                throw;
            }

        }
        private NameValueCollection createrInfoTable(DataTable tbl, string nameTable)
        {
            if (tbl != null && tbl.Rows.Count > 0)
            {
                NameValueCollection temp = new NameValueCollection();
                //int maRieng;

                //bool check = int.TryParse(tbl.Rows[0][nameCotRieng].ToString(), out maRieng);
                //if (!check)
                temp.Add(nameTable, "");
                foreach (DataColumn item in tbl.Columns)
                {
                    temp.Add(nameTable + item.ColumnName, tbl.Rows[0][item].ToString());
                }
                return temp;
            }
            return null;
        }
        private void AssignGXData(DataRow row)
        {
            row[GiaoXuConst.TenGiaoXu] = txtTenGiaoXu.Text;
            row[GiaoXuConst.DiaChi] = txtDiaChi.Text;
            row[GiaoXuConst.Website] = txtWebsite.Text;
            row[GiaoXuConst.DienThoai] = txtDienThoai.Text;
            row[GiaoXuConst.Email] = txtEmail.Text;
            row[GiaoXuConst.Hinh] = txtHinh.Text;
            row[GiaoXuConst.GhiChu] = txtGhiChu.Rtf;
            try
            {
                if (System.IO.File.Exists(openFileDialog1.FileName))
                {
                    System.IO.FileInfo img = new System.IO.FileInfo(openFileDialog1.FileName);
                    string dest = Memory.AppPath + txtHinh.Text;
                    System.IO.File.Copy(img.FullName, dest, true);
                    Form main = Application.OpenForms["frmMain"];
                    ((frmMain)main).pictureBox1.BackgroundImage = Image.FromFile(img.FullName);
                }
            }
            catch { }
        }

        private void gxLinhMucList1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {

            EditLinhMucRow(GxOperation.EDIT);
        }

        private void EditLinhMucRow(GxOperation op)
        {
            if (op == GxOperation.EDIT && (gxLinhMucList1.CurrentRow == null || (gxLinhMucList1.CurrentRow.DataRow as DataRowView) == null)) return;
            frmLinhMuc frm = new frmLinhMuc();
            frm.Operation = op;
            DataRow row = null;
            if (op == GxOperation.EDIT)
            {
                row = (gxLinhMucList1.CurrentRow.DataRow as DataRowView).Row;
                frm.Id = (int)row[LinhMucConst.MaLinhMuc];
                frm.AssignControlData();
                showForm(row, frm);
            }
            else
            {
                DataTable tbl = gxLinhMucList1.DataSource as DataTable;
                if (tbl == null)
                {
                    tbl = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST);
                    if (Memory.ShowError())
                    {
                        return;
                    }                    
                    gxLinhMucList1.DataSource = tbl;
                }
                row = tbl.NewRow();
                DialogResult rs = showForm(row, frm);
                if (rs == DialogResult.OK)
                {
                    tbl.Rows.Add(row);
                }
            }
        }

        private DialogResult showForm(DataRow row, frmBase frm)
        {
            DialogResult rs = DialogResult.Cancel;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.DataReturn != null)
                {
                    row[LinhMucConst.MaLinhMuc] = frm.DataReturn[LinhMucConst.MaLinhMuc];
                    row[LinhMucConst.TenThanh] = frm.DataReturn[LinhMucConst.TenThanh];
                    row[LinhMucConst.HoTen] = frm.DataReturn[LinhMucConst.HoTen];
                    row[LinhMucConst.ChucVu] = frm.DataReturn[LinhMucConst.ChucVu];
                    row[LinhMucConst.NgaySinh] = frm.DataReturn[LinhMucConst.NgaySinh];
                    row[LinhMucConst.TuNgay] = frm.DataReturn[LinhMucConst.TuNgay];
                    row[LinhMucConst.DenNgay] = frm.DataReturn[LinhMucConst.DenNgay];
                    row[LinhMucConst.GhiChu] = frm.DataReturn[LinhMucConst.GhiChu];
                    rs = DialogResult.OK;
                }
            }
            return rs;
        }

        private void gxLinhMucList1_RowCountChanged(object sender, System.EventArgs e)
        {
            if (gxLinhMucList1.DataSource != null && gxLinhMucList1.RowCount > 0)
            {
                gxAddEdit1.EditButton.Enabled = true;
                gxAddEdit1.DeleteButton.Enabled = true;
            }
            else
            {
                gxAddEdit1.EditButton.Enabled = false;
                gxAddEdit1.DeleteButton.Enabled = false;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtHinh.Text = System.IO.Path.GetFileName(openFileDialog1.FileName);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UpdateProcess.sendGiaoXuInfo();
            base.OnClosing(e);
        }

        private void gxAddEdit1_Load(object sender, EventArgs e)
        {

        }

        private void linkLbGiaoPhan_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(linkLbGiaoPhan.Text == "Tạo mới")
            {
                cbGiaoPhan.Visible = false;
                linkLbGiaoPhan.Text = "Chọn từ danh sách";
                cbGiaoPhan.SelectedIndex = -1;
                cbGiaoHat.combo.DataSource = null;
            }
            else
            {
                cbGiaoPhan.Visible = true;
                linkLbGiaoPhan.Text = "Tạo mới";
            }
        }

        private void linkLbGiaoHat_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (linkLbGiaoHat.Text == "Tạo mới")
            {
                cbGiaoHat.Visible = false;
                linkLbGiaoHat.Text = "Chọn từ danh sách";
                cbGiaoHat.SelectedIndex = -1;
            }
            else
            {
                cbGiaoHat.Visible = true;
                linkLbGiaoHat.Text = "Tạo mới";
            }
        }
    }
}