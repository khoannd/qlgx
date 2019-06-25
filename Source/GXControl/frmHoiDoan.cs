using GxGlobal;
using Janus.Windows.GridEX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class frmHoiDoan : frmBase
    {
      
      
        //row chọn để xem hoặc sửa từ danh sách hội đoàn
        public DataRow oldrow = null;

        //Lấy database chi tiết hội đoàn.
        DataTable tblcthd;

        public int MaHoiDoan
        {
            get
            {
                return Convert.ToInt32(txtMaHoiDoan.Text);
            }
            set
            {
                txtMaHoiDoan.Text = value.ToString();
                id = value;
            }
        }

        public frmHoiDoan()
        {
            InitializeComponent();
        }

        private void frmHoiDoan_Load(object sender, EventArgs e)
        {
            //dt input null
            dtNgayBonMang.DateInput.IsNullDate = true;
            dtNgayThanhLap.DateInput.IsNullDate = true;
            //enable button Reload 
            gxAddEdit1.ReloadButton.Enabled = true;
            //Bắt đầu format lại Grid
            gxGiaoDanList1.FormatGrid();

            //Cho phép sửa các cell trong grid
            gxGiaoDanList1.AllowEdit = InheritableBoolean.True;


            //Đóng tất cả các cột trong List Giáo dân không cho sửa.
            foreach (GridEXColumn col in gxGiaoDanList1.RootTable.Columns)
            {
                col.EditType = EditType.NoEdit;
            }

            GridEXColumn col1 = new GridEXColumn(ChiTietHoiDoanConst.NgayVaoHoiDoan);
            col1.Width = 150;
            col1.BoundMode = ColumnBoundMode.Bound;
            col1.DataMember = ChiTietHoiDoanConst.NgayVaoHoiDoan;
            col1.Caption = "Ngày vào hội đoàn";
            col1.FilterEditType = FilterEditType.Combo;
            col1.EditType = EditType.CalendarDropDown;
            //col1.FormatMode = FormatMode.UseIFormattable;
            //col1.DefaultGroupFormatMode = FormatMode.UseIFormattable;
            //col1.DefaultGroupFormatString = "dd/MM/yyyy";
            //col1.FormatString = "dd/MM/yyyy";


            GridEXColumn col2 = new GridEXColumn(ChiTietHoiDoanConst.NgayRaHoiDoan);
            col2.Width = 150;
            col2.BoundMode = ColumnBoundMode.Bound;
            col2.DataMember = ChiTietHoiDoanConst.NgayRaHoiDoan;
            col2.Caption = "Ngày ra hội đoàn";
            col2.FilterEditType = FilterEditType.Combo;
            col2.EditType = EditType.CalendarDropDown;

            
            GridEXColumn col3 = new GridEXColumn(ChiTietHoiDoanConst.VaiTro);
            col3.HasValueList = true;
            col3.Width = 150;
            col3.BoundMode = ColumnBoundMode.Bound;
            col3.DataMember = ChiTietHoiDoanConst.VaiTro;
            col3.Caption = "Vai trò";
            col3.FilterEditType = FilterEditType.Combo;
            col3.EditType = EditType.DropDownList;
            GridEXValueListItemCollection vl = col3.ValueList;
            LoadDanhSachVaiTro(vl);

            gxGiaoDanList1.RootTable.Columns.Insert(0, col1);
            gxGiaoDanList1.RootTable.Columns.Insert(1, col2);
            gxGiaoDanList1.RootTable.Columns.Insert(2, col3);

            // row này được lấy từ GxListHoiDoan. nếu thêm mới thì oldrow bằng null
            if (oldrow != null)
            {
                //Cho biết là đang sửa hội đoàn chứ không phải thêm mới
                this.operation = GxOperation.EDIT;
                
                //Điền dữ liệu vào các ô bên form frmHoiDoan
                txtMaHoiDoan.TextBox.Text = oldrow["MaHoiDoan"].ToString();
                txtTenHoiDoan.TextBox.Text = oldrow["TenHoiDoan"].ToString();
                txtThanhBonMang.TextBox.Text = oldrow["ThanhBonMang"].ToString();
                dtNgayThanhLap.Value = oldrow["NgayThanhLap"].ToString();
                dtNgayBonMang.Value = oldrow["NgayBonMang"].ToString();
                txtGhiChu.TextBox.Text = oldrow["GhiChu"].ToString();
            }
            else
            {
                 id= Memory.Instance.GetNextId(HoiDoanConst.TableName, HoiDoanConst.MaHoiDoan, true);
                 txtMaHoiDoan.TextBox.Text = id.ToString();
            }
            //Load data lên grid
            reloaddata();
            tblcthd = Memory.GetData(SqlConstants.SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN, MaHoiDoan);
            reloadGrid();
        }
        public void reloaddata()
        {
            //Câu truy vấn lấy danh sách thành viên hội đoàn
            string QueryString = SqlConstants.SELECT_LIST_HOIVIEN_HOIDOAN;
            //Điều kiện 
            object[] argument = { Convert.ToInt32(txtMaHoiDoan.TextBox.Text) };

            //Load data lên grid
            gxGiaoDanList1.LoadData(QueryString, argument);
        }
        //Load vai trò
        public void LoadDanhSachVaiTro(GridEXValueListItemCollection vl)
        {
            vl.Add("Trưởng hội đoàn", "Trưởng hội đoàn");
            vl.Add("Phó hội đoàn", "Phó hội đoàn");
            vl.Add("Thư ký", "Thư ký");
            vl.Add("Hội viên", "Hội viên");
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            try
            {
                //Check input data
                if (!CheckInputData()) return;
             
                //Check fill input data
                DataTable tblCTHDGrid = (DataTable)gxGiaoDanList1.DataSource;
                bool hasEmpty = false;
                int CountRow = tblCTHDGrid.Rows.Count;
                if (tblCTHDGrid != null && CountRow>0)
                {
                    for (int i=0;i< CountRow;i++)
                    {
                        if (tblCTHDGrid.Rows[i].RowState!=DataRowState.Deleted)
                        {
                            gxGiaoDanList1.Row = i;
                            if (Memory.IsNullOrEmpty(tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayVaoHoiDoan]))
                            {
                                hasEmpty = true;
                                if (gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle == null)
                                    gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle = new Janus.Windows.GridEX.GridEXFormatStyle();
                                gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle.BackColor = Color.Red;
                            }
                            if (Memory.IsNullOrEmpty(tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.VaiTro]))
                            {
                                hasEmpty = true;
                                if (gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.VaiTro].FormatStyle == null)
                                    gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.VaiTro].FormatStyle = new Janus.Windows.GridEX.GridEXFormatStyle();
                                gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.VaiTro].FormatStyle.BackColor = Color.Red;
                            }
                        }
                    }
                    gxGiaoDanList1.Row = -3;
                }

                if(hasEmpty)
                {
                    DialogResult rs = MessageBox.Show("Có hội viên chưa nhập ngày vào hội hoặc vai trò." +
                        "\r\nBạn có muốn tiếp tục cập nhật danh sách hội đoàn không?\r\n" +
                        "Nhấp nút [Yes] để tiếp tục.\r\n" +
                        "Nhấp nút [No] để đóng màn hình mà không cập nhật danh sách\r\n" +
                        "Nhấp nút [Cancel] để quay lại nhập dữ liệu, không cập nhật và cũng không đóng màn hình\r\n", "Xác nhận", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (rs == DialogResult.No)
                    {
                        this.Close();
                        return;
                    }
                    if (rs == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                // Lấy danh sách các hội viên đã ra khỏi hội đoàn.
                DataTable tblcthd1 = Memory.GetData(SqlConstants.SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN+" and NgayRaHoiDoan is not null", MaHoiDoan);
               
                //Kiểm tra dữ liệu ngày vào ngày ra của hội viên.
                for (int i=0;i<CountRow;i++)
                {
                    if (tblCTHDGrid.Rows[i].RowState != DataRowState.Deleted)
                    {
                        gxGiaoDanList1.Row = i;
                        
                        if (!Memory.IsNullOrEmpty(tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayVaoHoiDoan]))
                        {
                            DataRow[] rows = tblcthd1.Select("MaGiaoDan = "+ tblCTHDGrid.Rows[i]["MaGiaoDan"].ToString(),"ID desc");
                            
                            if (rows!=null && rows.Length>0)
                            {
                                string a = tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString();
                                DateTime newNgayVaoHoiDoan = Memory.FormatStringToDateTime(a);

                                for (int j=0;j<rows.Length;j++)
                                {
                                    a = rows[j][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString();
                                    DateTime oldNgayVaoHoiDoan = Memory.FormatStringToDateTime(a);
                                    a = rows[j][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString();
                                    DateTime oldNgayRaHoiDoan = Memory.FormatStringToDateTime(a);
                                    if (DateTime.Compare(oldNgayVaoHoiDoan, newNgayVaoHoiDoan) <=0 && DateTime.Compare(newNgayVaoHoiDoan, oldNgayRaHoiDoan) <=0)
                                    {
                                        MessageBox.Show(String.Format("Từ ngày {0} đến ngày {1} giáo dân {2} đã ở trong hội đoàn rồi!!!!", 
                                            rows[j][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString(), rows[j][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(), rows[j][GiaoDanConst.HoTen].ToString()),
                                            "Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                        return;
                                    }
                                }
                            }
                        }
                        if (!Memory.IsNullOrEmpty(tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayRaHoiDoan]) && !Memory.IsNullOrEmpty(tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayVaoHoiDoan]))
                        {
                            string a = tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString();
                            DateTime newNgayVaoHoiDoan = Memory.FormatStringToDateTime(a);
                            a = tblCTHDGrid.Rows[i][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString();
                            DateTime newNgayRaHoiDoan = Memory.FormatStringToDateTime(a);
                            if (DateTime.Compare(newNgayVaoHoiDoan,newNgayRaHoiDoan) >= 0)
                            {
                                 MessageBox.Show(String.Format("Vui lòng nhập lại ngày ra vào đoàn của giáo dân {0}.Ngày ra không thể trước ngày vào", tblCTHDGrid.Rows[i][GiaoDanConst.HoTen]), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }
                }
                
                //Update data table Hội Đoàn

                DataTable tblHoiDoan = Memory.GetData(SqlConstants.SELECT_HOIDOAN_BY_MAHOIDOAN, new object[] { MaHoiDoan });
              
                if(!Memory.ShowError() && tblHoiDoan!=null)
                {
                    tblHoiDoan.TableName = HoiDoanConst.TableName;
                    if (operation == GxOperation.EDIT)
                    {
                        if(tblHoiDoan.Rows.Count==0)
                        {
                            MessageBox.Show("Xin lỗi!!!\r\nCập nhập hội đoàn thất bại!!!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        dataReturn = tblHoiDoan.Rows[0];
                        AssignDataSourceHoiDoan(dataReturn);
                    }
                    else
                    {
                        if (operation == GxOperation.ADD)
                        {
                            dataReturn = tblHoiDoan.NewRow();
                            AssignDataSourceHoiDoan(dataReturn);
                            tblHoiDoan.Rows.Add(dataReturn);
                        }
                    }
                }
                //Update Chi tiết hội đoàn
                foreach(DataRow row in tblCTHDGrid.Rows)
                {
                    if(row.RowState==DataRowState.Deleted)
                    {
                        Memory.ExecuteSqlCommand("Delete form ChiTietHoiDoan where NgayVaoHoiDoan is null " +
                            "and NgayRaHoiDoan is null and MaGiaoDan = "+row[GiaoDanConst.MaGiaoDan]);
                        continue;
                    }
                    //Thêm vào bảng chi tiết
                    if(row[GxConstants.EXISTED_COLUMN] is DBNull)   //Thêm mới
                    {
                        DataReturn = tblcthd.NewRow();
                        UpdateHoiVien(DataReturn,row);
                        tblcthd.Rows.Add(DataReturn);

                    }else
                    {
                        DataRow[] rows = tblcthd.Select("MaGiaoDan = " + row[GiaoDanConst.MaGiaoDan] + " and NgayRaHoiDoan is null");
                        UpdateHoiVien(rows[0], row);
                    }
                }

                DataSet ds = new DataSet();
                tblcthd.TableName = ChiTietHoiDoanConst.TableName;
                ds.Tables.Add(tblcthd);
                ds.Tables.Add(tblHoiDoan);
                Memory.UpdateDataSet(ds);
                if (Memory.ShowError())
                {
                    MessageBox.Show("Update thất bại");
                }
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Update thất bại");
                throw;
            }
        }
       //Check input data
       private bool CheckInputData()
        {
            if (txtTenHoiDoan.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tên hội đoàn");
                return false;
            }
            if(gxGiaoDanList1.RowCount==0)
            {
                MessageBox.Show("Cần có ít nhất 1 hội viên trong hội đoàn");
                return false;
            }
            if (!ktHoiTruong()) return false;
            return true;
        }
        
       
        public void AssignDataSourceHoiDoan(DataRow row)
        {
            row[HoiDoanConst.MaHoiDoan] = Convert.ToInt32(txtMaHoiDoan.TextBox.Text.ToString());
            row[HoiDoanConst.TenHoiDoan] = txtTenHoiDoan.TextBox.Text.ToString();
            row[HoiDoanConst.ThanhBonMang] = txtThanhBonMang.TextBox.Text.ToString();
            row[HoiDoanConst.NgayBonMang] = dtNgayBonMang.Value.ToString();
            row[HoiDoanConst.NgayThanhLap] = dtNgayThanhLap.Value.ToString();
            row[HoiDoanConst.GhiChu] = txtGhiChu.TextBox.Text.ToString();
        }
        
        public void UpdateHoiVien(DataRow r1,DataRow r2)
        {
            try
            {
                r1[HoiDoanConst.MaHoiDoan] = txtMaHoiDoan.TextBox.Text.ToString();
                r1[GiaoDanConst.MaGiaoDan] = r2[GiaoDanConst.MaGiaoDan];
                r1[ChiTietHoiDoanConst.NgayVaoHoiDoan] = r2[ChiTietHoiDoanConst.NgayVaoHoiDoan];
                r1[ChiTietHoiDoanConst.NgayRaHoiDoan] = r2[ChiTietHoiDoanConst.NgayRaHoiDoan];
                r1[ChiTietHoiDoanConst.VaiTro] = r2[ChiTietHoiDoanConst.VaiTro];
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            frmGiaoDan frm = new frmGiaoDan();
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
                insertDataGrid(frm);
        }

        private void gxAddEdit1_SelectClick(object sender, EventArgs e)
        {
            frmChonGiaoDan frm = new frmChonGiaoDan();
            frm.ShowDialog();
            if(frm.DialogResult==DialogResult.OK)
                insertDataGrid(frm);
        }

        public void insertDataGrid(frmBase frm)
        {
            DataTable tbl = (DataTable)gxGiaoDanList1.DataSource;
            if(tbl!=null)
            {
                //Check input data

                DataRow[] rows = tbl.Select("MaGiaoDan=" + frm.DataReturn[GiaoDanConst.MaGiaoDan].ToString()+" and NgayRaHoiDoan is null");
                if(rows!=null && rows.Length>0)
                {
                    MessageBox.Show(String.Format("Giáo dân {0} đã tồn tại trong hội đoàn rồi !!!", frm.DataReturn[GiaoDanConst.HoTen].ToString()),
                        "Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
                    return;
                }
                frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.NgayVaoHoiDoan, typeof(string));
                frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.NgayRaHoiDoan, typeof(string));
                frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.VaiTro, typeof(string));


                frm.DataReturn.AcceptChanges();
                frm.DataReturn.SetAdded();
                tbl.ImportRow(frm.DataReturn);
                gxGiaoDanList1.Row = gxGiaoDanList1.RowCount - 1;
                return;
            }
        }

        //Kiểm tra xem danh sách có hội trưởng chưa hoặc nhóm trưởng có nhiều hơn một người không.

        public bool ktHoiTruong()
        {
           DataTable tbl = (DataTable)gxGiaoDanList1.DataSource;
           if(tbl != null && tbl.Rows.Count > 0)
            {
                string query = String.Concat("VaiTro='", HoiDoanConst.TruongHoiDoan, "' and NgayRaHoiDoan is null");
                DataRow[] rows = tbl.Select(query);
                if (rows != null && rows.Length > 0)
                {
                    if (rows.Length > 1)
                    {
                       DialogResult tl= MessageBox.Show("Mỗi hội đoàn chỉ được có một hội trưởng." +
                            "\r\nChọn [Yes] để đóng màn hình và không cập nhập." +
                            "\r\nChọn [No] để quay lại nhập dữ liệu, không đóng màn hình và không cập nhập." +
                            "\r\nChọn [Cancel] để thoát.", 
                            "Nhắc nhở", MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                       if(tl==DialogResult.Yes)
                       {
                            this.Close();
                            return false;
                       }
                        return false;
                    }
                    if(!Memory.IsNullOrEmpty(rows[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString()))
                    {
                        DialogResult tl = MessageBox.Show("Mỗi hội đoàn cần có một hội trưởng." +
                      "\r\nChọn [Yes] để đóng màn hình và không cập nhập." +
                      "\r\nChọn [No] để quay lại nhập dữ liệu, không đóng màn hình và không cập nhập." +
                      "\r\nChọn [Cancel] để thoát.",
                      "Nhắc nhở", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                        if (tl == DialogResult.Yes)
                        {
                            this.Close();
                            return false;
                        }
                        return false;
                    }
                }
                else
                {
                    DialogResult tl = MessageBox.Show("Mỗi hội đoàn cần có một hội trưởng." +
                       "\r\nChọn [Yes] để đóng màn hình và không cập nhập." +
                       "\r\nChọn [No] để quay lại nhập dữ liệu, không đóng màn hình và không cập nhập." +
                       "\r\nChọn [Cancel] để thoát.",
                       "Nhắc nhở", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                    if (tl == DialogResult.Yes)
                    {
                        this.Close();
                        return false;
                    }
                    return false;
                }
            }
            else
            {
                DialogResult tl = MessageBox.Show("Mỗi hội đoàn cần có một hội trưởng." +
                       "\r\nChọn [Yes] để đóng màn hình và không cập nhập." +
                       "\r\nChọn [No] để quay lại nhập dữ liệu, không đóng màn hình và không cập nhập." +
                       "\r\nChọn [Cancel] để thoát.",
                       "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button3);
                if (tl == DialogResult.Yes)
                {
                    this.Close();
                    return true;
                }
                return false;
            }
            return true;
        }

        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
           
           if(gxGiaoDanList1.Row<0)
            {
                MessageBox.Show("Vui lòng chọn hội viên để xóa");
            }
           else
            {
                if (gxGiaoDanList1.CurrentRow.DataRow != null)
                {
                    if(!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text.ToString()))
                    {
                        MessageBox.Show("Giáo dân này đã được xóa khỏi hội đoàn.");
                        return;
                    }
                    if (!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text.ToString()))
                    {
                        DialogResult tl = MessageBox.Show("Bạn có thực sự muốn giáo dân ["
                            + gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.HoTen].Text.ToString() + "] ra khỏi hội đoàn.\r\n" +
                            "Chọn [Yes] để lấy ngày hiện tại làm ngày ra khỏi đoàn.\r\n" +
                            "Chọn [No] để nhập ngày ra khỏi đoàn.\r\n" +
                            "Chọn [Cancel] để thoát.",
                            "Nhắc nhở", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                        if (tl == DialogResult.Yes)
                        {
                            gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Value = DateTime.Now.ToString("dd/MM/yyyy");
                            gxGiaoDanList1.Row = -3;
                            return;
                        }
                    }
                    else
                    {
                        DialogResult tl = MessageBox.Show("Bạn có thực sự muốn XÓA giáo dân ["
                            + gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.HoTen].Text.ToString() + "] ra khỏi hội đoàn.\r\n" +
                            "Chọn [Yes] để xóa.\r\n" +
                            "Chọn [No] để nhập ngày vào và ra khỏi đoàn.\r\n" +
                            "Chọn [Cancel] để thoát.",
                            "Nhắc nhở", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                        if (tl == DialogResult.Yes)
                        {
                            gxGiaoDanList1.CurrentRow.Delete();
                            return;
                        }
                    }
                }
            }
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            gxGiaoDanList1.EditRow();
        }

        private void gxAddEdit1_ReloadClick(object sender, EventArgs e)
        {
            reloaddata();
        }

        private void gxGiaoDanList1_SelectionChanged(object sender, EventArgs e)
        {
            reloadGrid();
        }
        private void reloadGrid()
        {
            if (gxGiaoDanList1.CurrentRow == null || (gxGiaoDanList1.CurrentRow.DataRow as DataRowView) == null || gxGiaoDanList1.RowCount <= 0)
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
