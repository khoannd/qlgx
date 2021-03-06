using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxControl;
using GxGlobal;

namespace GiaoXu
{
    public partial class frmBiTichChiTiet : frmBase
    {
        private Dictionary<int, int> dicDeleteInfo = new Dictionary<int, int>();

        private LoaiBiTich loaiBiTich = LoaiBiTich.RuaToi;

        /// <summary>
        /// 0: rửa tội
        /// 1: XTRL lần đầu
        /// 2: Thêm sức
        /// </summary>
        public LoaiBiTich LoaiBiTich
        {
            get { return loaiBiTich; }
            set
            {
                loaiBiTich = value;
                if (gxBiTichChiTiet1 != null)
                {
                }
                gxBiTichChiTiet1.LoaiBiTich = value;
            }
        }

        public int MaDotBiTich
        {
            get { return int.Parse(txtMaDotBiTich.Text); }
            set
            {
                txtMaDotBiTich.Text = value.ToString();
                id = value;
            }
        }

        public frmBiTichChiTiet()
        {
            InitializeComponent();
            this.HelpKey = "dot_bi_tich";

            gxAddEdit1.Button1.Text = "Chọn &gia đình";
            gxAddEdit1.Button1.Visible = true;
            gxAddEdit1.EditButton.Text = "&Xem chi tiết";
            gxAddEdit1.ToolTipButton1 = "Chọn gia đình trong danh sách gia đình có sẵn cho giáo dân đang được chọn trên lưới";
            gxAddEdit1.Button1.Enabled = false;
            //gxAddEdit1.PrintButton.Text = "In danh sách bí tích";
            gxAddEdit1.PrintButton.Click += new EventHandler(PrintButton_Click);
            gxAddEdit1.PrintButton.Visible = true;
            gxAddEdit1.ReloadButton.Visible = true;
            gxAddEdit1.Button2.Visible = true;
            gxAddEdit1.Button2.Text = "In chứng &nhận";
            gxAddEdit1.ToolTipButton2 = "In chứng nhận bí tích cho giáo dân được chọn trên lưới";
            gxAddEdit1.Button2.Click += new EventHandler(Button2_Click);
            gxCommand1.OKButton.Text = "&Cập nhật";
            this.AcceptButton = gxCommand1.OKButton;
            dtNgayBiTich.DateInput.IsNullDate = true;
            dtNgayBiTich.DateInput.OnOK += new EventHandler(DateInput_OnOK);

            //gxBiTichChiTiet1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            gxBiTichChiTiet1.ColumnAutoResize = false;
            if (Memory.GiaoXuInfo != null)
            {
                txtNoiBiTich.Text = Memory.GiaoXuInfo[GiaoXuConst.TenGiaoXu].ToString();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            gxBiTichChiTiet1.InChungNhan();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.operation == GxOperation.ADD)
            {
                MaDotBiTich = Memory.Instance.GetNextId(DotBiTichConst.TableName, DotBiTichConst.MaDotBiTich, true);
            }
            else txtMaDotBiTich.ReadOnly = true;
            gxBiTichChiTiet1.FormatGrid();
            gxBiTichChiTiet1.MaDotBiTich = MaDotBiTich;
            //if (loaiBiTich == LoaiBiTich.ThemSuc)
            //{
            //    txtLinhMuc.Label = "Đức Giám Mục";
            //}
            this.Text += " - " + gxBiTichChiTiet1.TenBiTich;
            //if (this.operation == GxOperation.ADD && loaiBiTich != LoaiBiTich.ThemSuc)
            //{
            //    //load linh muc combobox
            //    DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ");
            //    if (tblLinhMuc != null)
            //    {
            //        foreach (DataRow row in tblLinhMuc.Rows)
            //        {
            //            txtLinhMuc.Combo.Items.Add(row[LinhMucConst.TenThanh].ToString() + " " + row[LinhMucConst.HoTen].ToString());
            //        }
            //        if (txtLinhMuc.Combo.Items.Count > 0) txtLinhMuc.Combo.SelectedIndex = 0;
            //    }
            //}
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            gxBiTichChiTiet1.Print();
        }

        private void gxAddEdit1_SelectClick(object sender, EventArgs e)
        {
            frmChonDuLieu frm = new frmChonDuLieu();
            frm.LoaiTimKiem = LoaiTimKiem.GiaoDan;
            if (Memory.GetConfig(GxConstants.CF_SOBITICH_HIENTATCAGIAODAN) == GxConstants.CF_TRUE)
            {
                frm.WhereSQL = string.Format(" AND ({0} IS NULL OR {0} = \"\") ", gxBiTichChiTiet1.NgayBiTichColumnName);
            }
            addGiaoDan(frm);
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            frmGiaoDan frm = new frmGiaoDan();
            addGiaoDan(frm);
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            gxBiTichChiTiet1.EditRow();
        }

        private void addGiaoDan(frmBase frm)
        {
            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.DataReturn != null)
                {
                    DataTable tbl = (DataTable)gxBiTichChiTiet1.DataSource;
                    if (tbl != null)
                    {
                        DataRow newRow = null;
                        if (frm is frmGiaoDan)
                        {
                            DataTable tblTmp = Memory.GetData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + " AND DaXoa=0 AND DaChuyenXu=0 AND MaGiaoDan = " + frm.DataReturn[GiaoDanConst.MaGiaoDan].ToString());
                            if (!Memory.ShowError() && tblTmp != null)
                            {
                                newRow = tblTmp.Rows[0];
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            newRow = frm.DataReturn;
                        }
                        //Check existance start
                        //Check in current list
                        DataRow[] rowsCheck = tbl.Select(string.Format("MaGiaoDan={0}", newRow[GiaoDanConst.MaGiaoDan]));
                        if (rowsCheck != null && rowsCheck.Length > 0)
                        {
                            MessageBox.Show("Đã tồn tại giáo dân này trong danh sách. Vui lòng nhập giáo dân khác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        //check in database in other DotBiTich
                        DataTable tblCheck = Memory.GetData(string.Format(SqlConstants.SELECT_BITICH_CHITIET_THEOLOAI + " AND MaGiaoDan={0} AND LoaiBiTich={1}", newRow[GiaoDanConst.MaGiaoDan], (int)loaiBiTich));
                        if (Memory.ShowError()) return;
                        if (tblCheck != null && tblCheck.Rows.Count > 0)
                        {
                            MessageBox.Show("Đã tồn tại giáo dân này trong đợt bí tích " + gxBiTichChiTiet1.TenBiTich + " khác. Vui lòng nhập giáo dân khác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;                            
                        }

                        //Check existance end


                        //Check giáo dân trong hồ sơ lưu trữ
                        string warningMessage = "";
                        if ((bool)newRow[GiaoDanConst.QuaDoi])
                        {
                            warningMessage = "đã qua đời";
                        }
                        if (Convert.ToInt32(newRow[GiaoDanConst.DaChuyenXu].ToString()) == -1)
                        {
                            if(string.IsNullOrEmpty(warningMessage))
                            {
                                warningMessage = "đã chuyển xứ";
                            }    
                            else
                            {
                                warningMessage += ", đã chuyển xứ";
                            }    
                        }
                        if ((bool)newRow[GiaoDanConst.DaXoa])
                        {
                            if (string.IsNullOrEmpty(warningMessage))
                            {
                                warningMessage = "đã xóa";
                            }
                            else
                            {
                                warningMessage += ", đã xóa";
                            }
                        }
                        if(!string.IsNullOrEmpty(warningMessage.Trim()))
                        {
                           DialogResult tl= MessageBox.Show(string.Format("Giáo dân {0} hiện tại {1}. Bạn có muốn tiếp tục thêm giáo dân này." +
                                "\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để hủy.",newRow[GiaoDanConst.HoTen],warningMessage), 
                                "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                            if (tl == DialogResult.No)
                                return;
                        }    

                        newRow.Table.Columns.Add(GiaDinhConst.MaGiaDinh, typeof(int));
                        newRow.Table.Columns.Add(GiaDinhConst.MaGiaDinhCo, typeof(int));
                        newRow.Table.Columns.Add(GiaDinhConst.TenGiaDinh, typeof(string));
                        DataTable tblConCai = Memory.GetData(SqlConstants.SELECT_THANHVIEN_GIADINH_LIST + " AND MaGiaoDan=? AND VaiTro = ?", new object[] { newRow[GiaoDanConst.MaGiaoDan], GxConstants.VAITRO_CON });
                        if (!Memory.ShowError() && tblConCai != null)
                        {
                            if (tblConCai.Rows.Count > 0)
                            {
                                newRow[GiaDinhConst.MaGiaDinh] = newRow[GiaDinhConst.MaGiaDinhCo] = tblConCai.Rows[0][ThanhVienGiaDinhConst.MaGiaDinh];
                                newRow[GiaDinhConst.TenGiaDinh] = tblConCai.Rows[0][GiaDinhConst.TenGiaDinh];
                            }
                            tbl.ImportRow(newRow);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dùng trong trường hợp xóa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            if (gxBiTichChiTiet1.CurrentRow != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn loại bỏ giáo dân này ra khỏi danh sách không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                int maGiaoDan = (int)(gxBiTichChiTiet1.CurrentRow.DataRow as DataRowView).Row[GiaoDanConst.MaGiaoDan];
                gxBiTichChiTiet1.CurrentRow.Delete();
                if (MessageBox.Show(string.Format("Bạn có muốn xóa cả thông tin [{0}] của giáo dân này không?", gxBiTichChiTiet1.TenBiTich), "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (!dicDeleteInfo.ContainsKey(maGiaoDan))
                    {
                        dicDeleteInfo.Add(maGiaoDan, maGiaoDan);
                    }
                }
            }
        }

        private void DateInput_OnOK(object sender, EventArgs e)
        {
            if (txtMoTa.Text.Trim() == "" && gxBiTichChiTiet1.TenBiTich != "")
            {
                txtMoTa.Text = string.Format("Đợt bí tích {0}{1}{2}{3}", gxBiTichChiTiet1.TenBiTich,
                                            dtNgayBiTich.DateInput.Day != "" ? " ngày " + dtNgayBiTich.DateInput.Day : "",
                                            dtNgayBiTich.DateInput.Month != "" ? " tháng " + dtNgayBiTich.DateInput.Month : "",
                                            dtNgayBiTich.DateInput.Year != "" ? " năm " + dtNgayBiTich.DateInput.Year : "");
            }
        }

        private bool checkInput()
        {
            if (!Validator.IsNumber(txtMaDotBiTich.Text))
            {
                MessageBox.Show("Mã gia đình phải được nhập số");
                txtMaDotBiTich.Focus();
                return false;
            }

            if (txtMoTa.Text.Trim() == "")
            {
                MessageBox.Show("Hãy mô tả cho đợt bí tích này!");
                txtMoTa.Focus();
                return false;
            }

            if (gxBiTichChiTiet1.RowCount == 0)
            {
                MessageBox.Show("Hãy nhập ít nhất 1 giáo dân trong danh sách những người chịu bí tích");
                return false;
            }

            //check ma dot bi tich
            if (operation == GxOperation.ADD && Memory.IsExistedData(SqlConstants.SELECT_DOTBITICH_LIST + " AND  DotBiTich.MaDotBiTich = ?", new object[] { txtMaDotBiTich.Text }))
            {
                MessageBox.Show("Mã gia đình này đã tồn tại. Hãy nhập mã khác!");
                txtMaDotBiTich.Focus();
                return false;
            }

            return true; ;
        }


        private void AssignDataSource(DataRow row)
        {
            row[DotBiTichConst.MaDotBiTich] = MaDotBiTich;
            row[DotBiTichConst.LinhMuc] = txtLinhMuc.Text;
            row[DotBiTichConst.NgayBiTich] = dtNgayBiTich.Value;
            row[DotBiTichConst.MoTa] = txtMoTa.Text;
            row[DotBiTichConst.NoiBiTich] = txtNoiBiTich.Text;
        }

        public void AssignControlData()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_DOTBITICH_LIST + " AND  DotBiTich.MaDotBiTich=?", new object[] { Id });
            if (tbl != null && tbl.Rows.Count > 0)
            {
                AssignControlData(tbl.Rows[0]);
            }
        }

        public void AssignControlData(DataRow row)
        {
            MaDotBiTich = (int)row[DotBiTichConst.MaDotBiTich];
            txtLinhMuc.Text = row[DotBiTichConst.LinhMuc].ToString();
            txtMoTa.Text = row[DotBiTichConst.MoTa].ToString();
            dtNgayBiTich.Value = row[DotBiTichConst.NgayBiTich].ToString();
            txtNoiBiTich.Text = row[DotBiTichConst.NoiBiTich].ToString();
            this.CurrentRow = row;
        }

        private DataTable[] getGridData()
        {
            DataTable tblGiaoDan = (DataTable)gxBiTichChiTiet1.DataSource;
            DataTable tblConCai = null;
            DataTable tblBiTichChiTiet = null;
            if (tblGiaoDan != null)
            {
                tblGiaoDan.TableName = GiaoDanConst.TableName;
                //Get table structure only
                tblBiTichChiTiet = Memory.GetTable(BiTichChiTietConst.TableName, " AND MaDotBiTich=" + this.MaDotBiTich.ToString());
                tblConCai = Memory.GetTableStruct(ThanhVienGiaDinhConst.TableName);
                if (!Memory.ShowError() && tblConCai != null && tblBiTichChiTiet != null)
                {
                    tblConCai.TableName = ThanhVienGiaDinhConst.TableName;
                    tblBiTichChiTiet.TableName = BiTichChiTietConst.TableName;
                    foreach (DataRow row in tblGiaoDan.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted)
                        {
                            row.RejectChanges();
                            string sql = string.Format("DELETE FROM BiTichChiTiet WHERE MaDotBiTich={0} AND MaGiaoDan={1}", txtMaDotBiTich.Text, row[GiaoDanConst.MaGiaoDan]);
                            Memory.ExecuteSqlCommand(sql);
                            Memory.ShowError();
                            if (dicDeleteInfo.ContainsKey((int)row[GiaoDanConst.MaGiaoDan]))
                            {
                                //delete bi tich info
                                sql = "UPDATE GiaoDan SET ";
                                if (loaiBiTich == LoaiBiTich.RuaToi)
                                {
                                    sql += string.Format("{0}=NULL, {1}=NULL, {2}=NULL, {3}=NULL, {4}=NULL", GiaoDanConst.NgayRuaToi, GiaoDanConst.SoRuaToi, GiaoDanConst.NguoiDoDauRuaToi, GiaoDanConst.ChaRuaToi, GiaoDanConst.NoiRuaToi);
                                }
                                else if (loaiBiTich == LoaiBiTich.RuocLe)
                                {
                                    sql += string.Format("{0}=NULL, {1}=NULL, {2}=NULL, {3}=NULL", GiaoDanConst.NgayRuocLe, GiaoDanConst.SoRuocLe, GiaoDanConst.ChaRuocLe, GiaoDanConst.NoiRuocLe);
                                }
                                else if (loaiBiTich == LoaiBiTich.ThemSuc)
                                {
                                    sql += string.Format("{0}=NULL, {1}=NULL, {2}=NULL, {3}=NULL, {4}=NULL", GiaoDanConst.NgayThemSuc, GiaoDanConst.SoThemSuc, GiaoDanConst.NguoiDoDauThemSuc, GiaoDanConst.ChaThemSuc, GiaoDanConst.NoiThemSuc);
                                }
                                sql += string.Format(" WHERE MaGiaoDan={0}", row[GiaoDanConst.MaGiaoDan]);
                                Memory.ExecuteSqlCommand(sql);
                                Memory.ShowError();
                            }                            
                            continue;
                        }
                        #region for con cai
                        row[gxBiTichChiTiet1.NgayBiTichColumnName] = dtNgayBiTich.Value;
                        row[gxBiTichChiTiet1.LinhMucColumnName] = txtLinhMuc.Text.Trim();
                        row[gxBiTichChiTiet1.NoiBiTichColumnName] = txtNoiBiTich.Text.Trim();
                        //if has changed
                        if (row[GiaDinhConst.MaGiaDinh].ToString() != row[GiaDinhConst.MaGiaDinhCo].ToString())
                        {
                            //if is edit
                            if (row[GiaDinhConst.MaGiaDinh].ToString() != "" && row[GiaDinhConst.MaGiaDinhCo].ToString() != "")
                            {
                                tblConCai.ImportRow(row);
                            }
                            else //else is add
                            {
                                DataRow newRow = tblConCai.NewRow();
                                newRow[ThanhVienGiaDinhConst.MaGiaDinh] = row[GiaDinhConst.MaGiaDinh];
                                newRow[ThanhVienGiaDinhConst.MaGiaoDan] = row[GiaoDanConst.MaGiaoDan];
                                newRow[ThanhVienGiaDinhConst.VaiTro] = GxConstants.VAITRO_CON;
                                newRow[ThanhVienGiaDinhConst.ChuHo] = false;
                                tblConCai.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        #region for bi tich chi tiet
                        //if is add new
                        if (row[GxConstants.EXISTED_COLUMN] is DBNull)
                        {
                            DataRow newRow = tblBiTichChiTiet.NewRow();
                            newRow[BiTichChiTietConst.MaDotBiTich] = this.MaDotBiTich;
                            newRow[BiTichChiTietConst.MaGiaoDan] = row[GiaoDanConst.MaGiaoDan];
                            newRow[BiTichChiTietConst.GhiChu] = row[BiTichChiTietConst.GhiChu1];
                            tblBiTichChiTiet.Rows.Add(newRow);
                        }
                        else 
                        {
                            DataRow[] rows = tblBiTichChiTiet.Select(string.Format("MaDotBiTich={0} AND MaGiaoDan={1}", this.MaDotBiTich, row[GiaoDanConst.MaGiaoDan]));
                            if (rows.Length > 0) rows[0][BiTichChiTietConst.GhiChu] = row[BiTichChiTietConst.GhiChu1];
                        }
                        #endregion

                    }
                }
            }
            //foreach (DataRow row in tblGiaoDan.Rows)
            //{ 
                
            //}
            return new DataTable[] { tblGiaoDan, tblConCai, tblBiTichChiTiet };
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            try
            {
                if (!checkInput()) return;

                //Kiem tra so bi tich da nhap het chua?
                DataTable tblTmp = (DataTable)gxBiTichChiTiet1.DataSource;
                bool hasEmpty = false;
                if (tblTmp != null)
                {
                    for (int i = 0; i < tblTmp.Rows.Count; i++)
                    {
                        if (tblTmp.Rows[i].RowState != DataRowState.Deleted)
                        {
                            if (Memory.IsNullOrEmpty(tblTmp.Rows[i][gxBiTichChiTiet1.SoBiTichColumnName]))
                            {
                                hasEmpty = true;
                                gxBiTichChiTiet1.Row = i;
                                if (gxBiTichChiTiet1.CurrentRow.Cells[gxBiTichChiTiet1.SoBiTichColumnName].FormatStyle == null)
                                    gxBiTichChiTiet1.CurrentRow.Cells[gxBiTichChiTiet1.SoBiTichColumnName].FormatStyle = new Janus.Windows.GridEX.GridEXFormatStyle();
                                gxBiTichChiTiet1.CurrentRow.Cells[gxBiTichChiTiet1.SoBiTichColumnName].FormatStyle.BackColor = Color.Red;
                            }
                        }
                    }
                }

                if (hasEmpty)
                {
                    DialogResult rs = MessageBox.Show("Có giáo dân chưa được nhập [" + gxBiTichChiTiet1.SoBiTichColumnText + "].\r\nBạn có muốn tiếp tục cập nhật danh sách bí tích không?\r\n" +
                        "Nhấp nút [Yes] để cập nhật và đóng mà hình\r\n" +
                        "Nhấp nút [No] để đóng mà hình mà không cập nhật danh sách\r\n" +
                        "Nhấp nút [Cancel] để quay lại nhập dữ liệu, không cập nhật và cũng không đóng màn hình\r\n", "Xác nhận", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (rs == DialogResult.No)
                    {
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                        return;
                    }
                    else if (rs == DialogResult.Cancel)
                    {
                        return;
                    }

                }
                //

                DataTable tblDotBiTich = Memory.GetData(SqlConstants.SELECT_DOTBITICH_LIST + " AND DotBiTich.MaDotBiTich=?", new object[] { MaDotBiTich });

                if (!Memory.ShowError() && tblDotBiTich != null)
                {
                    tblDotBiTich.TableName = DotBiTichConst.TableName;
                    if (operation == GxOperation.EDIT)
                    {
                        if (tblDotBiTich.Rows.Count == 0)
                        {
                            MessageBox.Show("Thành thật xin rỗi. Có lỗi khi cập nhật đợt bí tích này\r\nCập nhật không thành công");
                            this.DialogResult = DialogResult.Cancel;
                            return;
                        }

                        dataReturn = tblDotBiTich.Rows[0];
                        dataReturn[DotBiTichConst.LinhMuc] = txtLinhMuc.Text;
                        dataReturn[DotBiTichConst.NgayBiTich] = dtNgayBiTich.Value;
                        dataReturn[DotBiTichConst.MoTa] = txtMoTa.Text;
                        dataReturn[DotBiTichConst.NoiBiTich] = txtNoiBiTich.Text;
                        dataReturn[DotBiTichConst.UpdateDate] = Memory.Instance.GetServerDateTime();
                        dataReturn[DotBiTichConst.SoLuong] = gxBiTichChiTiet1.RecordCount;
                    }
                    else if(operation == GxOperation.ADD)
                    {
                        dataReturn = tblDotBiTich.NewRow();
                        dataReturn[DotBiTichConst.MaDotBiTich] = MaDotBiTich;
                        dataReturn[DotBiTichConst.LinhMuc] = txtLinhMuc.Text;
                        dataReturn[DotBiTichConst.NgayBiTich] = dtNgayBiTich.Value;
                        dataReturn[DotBiTichConst.MoTa] = txtMoTa.Text;
                        dataReturn[DotBiTichConst.NoiBiTich] = txtNoiBiTich.Text;
                        dataReturn[DotBiTichConst.LoaiBiTich] = (int)loaiBiTich;
                        dataReturn[DotBiTichConst.UpdateDate] = Memory.Instance.GetServerDateTime();
                        dataReturn[DotBiTichConst.SoLuong] = gxBiTichChiTiet1.RecordCount;
                        tblDotBiTich.Rows.Add(dataReturn);
                    }
                }

                DataSet ds = new DataSet();

                DataTable[] tbls = getGridData();
                if (tbls != null && tbls.Length > 0)
                {
                    foreach (DataTable table in tbls)
                    {
                        if (table != null) ds.Tables.Add(table);
                    }
                }

                if (tblDotBiTich != null) ds.Tables.Add(tblDotBiTich);
                Memory.UpdateDataSet(ds);
                if (!Memory.ShowError())
                {
                    this.DialogResult = DialogResult.OK;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (frmBiTichChiTiet, gxCommand1_OnOK)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.DialogResult = DialogResult.Cancel;
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void gxAddEdit1_Button1Click(object sender, EventArgs e)
        {
            gxBiTichChiTiet1.SelectGiaDinh();
        }

        private void gxDotBiTichChiTiet1_SelectionChanged(object sender, EventArgs e)
        {
            if (gxBiTichChiTiet1.CurrentRow == null || (gxBiTichChiTiet1.CurrentRow.DataRow as DataRowView) == null)
            {
                gxAddEdit1.Button1.Enabled = false;
                return;
            }
            
            DataRow row = (gxBiTichChiTiet1.CurrentRow.DataRow as DataRowView).Row;
            if (row[GiaDinhConst.MaGiaDinhCo] != DBNull.Value)
            {
                gxAddEdit1.Button1.Enabled = false;
            }
            else
            {
                gxAddEdit1.Button1.Enabled = true;
            }
        }
    }
}