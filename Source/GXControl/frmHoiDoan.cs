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
            GxListHoiDoan.LoadVaiTroHoiDoan(vl);

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
     

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            try
            {
                //Check input data
                if (!CheckInputData()) return;
             
                //Check fill input data
                DataTable tblCTHDGrid = (DataTable)gxGiaoDanList1.DataSource;
                bool hasEmpty = false;
                int CountRow = gxGiaoDanList1.RecordCount;
                if (tblCTHDGrid != null && CountRow>0)
                {
                    for (int i=0;i< CountRow; i++)
                    {
                        gxGiaoDanList1.Row = i;
                        if (Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text))
                        {
                            hasEmpty = true;
                            if (gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle == null)
                                gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle = new Janus.Windows.GridEX.GridEXFormatStyle();
                            gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].FormatStyle.BackColor = Color.Red;
                        }
                    }
                }

                if(hasEmpty)
                {
                    DialogResult rs = MessageBox.Show("Có hội viên chưa nhập ngày vào hội." +
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
                DataTable tblcthd1 = Memory.GetData(String.Concat(SqlConstants.SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN," and NgayRaHoiDoan is not null and NgayVaoHoiDoan is not null"), MaHoiDoan);
               
                //Kiểm tra dữ liệu ngày vào ngày ra của hội viên.
                for (int i=0;i<CountRow;i++)
                {
                    gxGiaoDanList1.Row = i;
                  
                    //Kiểm tra ngày ra hội đứng trước ngày vào hội
                    if (!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text) && !Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text))
                    {
                        if (Memory.CompareTwoStringDate(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text.ToString(), gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text.ToString()) >= 0)
                        {
                            MessageBox.Show(String.Format("Vui lòng nhập lại ngày ra và ngày vào hội đoàn của giáo dân {0}.\r\nNgày ra hội đoàn không thể trước hoặc bằng ngày vào hội đoàn.", gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.HoTen].Text), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    //Kiểm tra trong database
                    //Kiểm tra ngày vào hội đoàn của hội viên có nằm giữa ngày vào ngày ra của hội viên đó trong lịch sử của họ tham gia hội đoàn
                    if (!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text))
                    {
                        //Kiểm tra ngày vào hội đoàn có trước ngày thành lập hội đoàn không.
                        if (!Memory.IsNullOrEmpty(dtNgayThanhLap.Value.ToString()))
                        {
                            if(Memory.CompareTwoStringDate(dtNgayThanhLap.Value.ToString(), gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text)>0)
                            {
                                MessageBox.Show(String.Format("Vui lòng kiểm tra lại ngày vào hội đoàn của giáo dân {0} ngày vào hội đoàn không thể trước ngày thành lập hội đoàn {1}", gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.HoTen].Text,dtNgayThanhLap.Value.ToString()), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                return;
                            }
                        }

                        if (!CheckDate(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayVaoHoiDoan].Text.ToString(),tblcthd1, gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.MaGiaoDan].Text.ToString())) return;
                    }
                    //Tương tự kiểm tra ngày vào ta kiểm tra ngày ra
                    if (!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text))
                    {
                        //Kiểm tra ngày vào hội đoàn có trước ngày thành lập hội đoàn không.
                        if (!Memory.IsNullOrEmpty(dtNgayThanhLap.Value.ToString()))
                        {
                            if (Memory.CompareTwoStringDate(dtNgayThanhLap.Value.ToString(), gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text) > 0)
                            {
                                MessageBox.Show(String.Format("Vui lòng kiểm tra lại ngày ra hội đoàn của giáo dân {0} ngày ra hội đoàn không thể trước ngày thành lập hội đoàn {1}", gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.HoTen].Text, dtNgayThanhLap.Value.ToString()), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                                return;
                            }
                        }

                        if (!CheckDate(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text.ToString(), tblcthd1, gxGiaoDanList1.CurrentRow.Cells[GiaoDanConst.MaGiaoDan].Text.ToString())) return;
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
                        row.RejectChanges();
                        Memory.ExecuteSqlCommand(String.Concat("Delete from ChiTietHoiDoan where NgayVaoHoiDoan is null " +
                            "and NgayRaHoiDoan is null and MaGiaoDan = ", row[GiaoDanConst.MaGiaoDan]));
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
                        DataRow[] rows = tblcthd.Select(String.Concat("MaGiaoDan = " , row[GiaoDanConst.MaGiaoDan] + " and NgayRaHoiDoan is null"));
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
                this.Close();
            }
        }
        //Compare between two string date
        /// <summary>
        /// date1(dd/MM/yyyy): Ngày trước.
        /// date2(dd/MM/yyyy): Ngày sau.
        /// Nếu date1 trước date2 return -1 nếu bằng return 0 nếu date1 sau date2 return 1.
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int CompareTwoStringDateExample(string date1,string date2)
        {
            DateTime datetime1 = Memory.FormatStringToDateTime(date1);
            DateTime datetime2 = Memory.FormatStringToDateTime(date2);
            if (DateTime.Compare(datetime1, datetime2) < 0)
            {
                return -1;
            }
            if (DateTime.Compare(datetime1, datetime2) == 0)
            {
                return 0;
            }
                return 1;
        }
        //Check date

        public static bool CheckDate(string date, DataTable tblcthd,string MGD)
        {
            DataRow[] rows = tblcthd.Select(String.Concat("MaGiaoDan = ", MGD.ToString()));

            if (rows != null && rows.Length > 0)
            {
                for (int j = 0; j < rows.Length; j++)
                {
                    string oldNgayVaoHoiDoan = rows[j][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString();
                    string oldNgayRaHoiDoan = rows[j][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString();
                    if (Memory.CompareTwoStringDate(oldNgayVaoHoiDoan, date) <= 0 && Memory.CompareTwoStringDate(date, oldNgayRaHoiDoan) <= 0)
                    {
                        MessageBox.Show(String.Format("Từ ngày [{0}] đến ngày [{1}] giáo dân {2} đã ở trong hội đoàn rồi!!!!\r\n Vui lòng kiểm tra lại.",
                            rows[j][ChiTietHoiDoanConst.NgayVaoHoiDoan].ToString(), rows[j][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(), rows[j][GiaoDanConst.HoTen].ToString()),
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            return true;
        }

        //Check input data
        private bool CheckInputData()
        {
            if (txtTenHoiDoan.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập tên hội đoàn","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button1);
                txtTenHoiDoan.Focus();
                return false;
            }
            if(gxGiaoDanList1.RowCount==0)
            {
                MessageBox.Show("Cần có ít nhất 1 hội viên trong hội đoàn", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return false;
            }
            if (!ktHoiTruong()) return false;
            return true;
        }
        
       
        public void AssignDataSourceHoiDoan(DataRow row)
        {
            row[HoiDoanConst.MaHoiDoan] = Convert.ToInt32(MaHoiDoan);
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
                r1[HoiDoanConst.MaHoiDoan] = MaHoiDoan;
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
            try
            {
                DataTable tbl = (DataTable)gxGiaoDanList1.DataSource;
                if (tbl != null)
                {
                    //Check input data

                    DataRow[] rows = tbl.Select(String.Concat("MaGiaoDan= " , frm.DataReturn[GiaoDanConst.MaGiaoDan].ToString()) );
                    if (rows != null && rows.Length > 0)
                    {
                        if(!Memory.IsNullOrEmpty(rows[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString()))
                        {
                            MessageBox.Show(String.Format("Giáo dân {0} đang được sửa đổi vui lòng cập nhật rồi làm việc tiếp!!!", frm.DataReturn[GiaoDanConst.HoTen].ToString()),
                           "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            return;
                        }

                        MessageBox.Show(String.Format("Giáo dân {0} đã tồn tại trong hội đoàn rồi!!!", frm.DataReturn[GiaoDanConst.HoTen].ToString()),
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        return;
                    }
                    frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.NgayVaoHoiDoan, typeof(string));
                    frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.NgayRaHoiDoan, typeof(string));
                    frm.DataReturn.Table.Columns.Add(ChiTietHoiDoanConst.VaiTro, typeof(string));
                    frm.DataReturn[ChiTietHoiDoanConst.VaiTro] = "Hội viên";

                    frm.DataReturn.AcceptChanges();
                    frm.DataReturn.SetAdded();
                    tbl.ImportRow(frm.DataReturn);
                    gxGiaoDanList1.Row = gxGiaoDanList1.RowCount - 1;
                    return;
                }
            }catch
            {
                MessageBox.Show("Thêm giáo dân thất bại");
            }
        }

        //Kiểm tra xem danh sách có hội trưởng chưa hoặc hội trưởng có nhiều hơn một người không.

        public bool ktHoiTruong()
        {
           DataTable tbl = (DataTable)gxGiaoDanList1.DataSource;
           if(tbl != null && tbl.Rows.Count > 0)
            {
                DataRow[] rows = tbl.Select(String.Concat("VaiTro='", HoiDoanConst.TruongHoiDoan, "' and NgayRaHoiDoan is null"));
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
                }
                else
                {
                    DialogResult tl = MessageBox.Show("Mỗi hội đoàn cần có một hội trưởng." +
                       "\r\nChọn [Yes] để tiếp tục." +
                       "\r\nChọn [No] để đóng màn hình và không cập nhập." +
                       "\r\nChọn [Cancel] để quay lại chỉnh sửa dữ liệu.",
                       "Nhắc nhở", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);
                    if (tl == DialogResult.Yes)
                    {
                        return true;
                    }
                    if (tl == DialogResult.No)
                    {
                        this.Close();
                        return false;
                    }
                    return false;
                }
            }
            return true;
        }

        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            try
            {
                if (gxGiaoDanList1.Row < 0)
                {
                    MessageBox.Show("Vui lòng chọn hội viên để xóa");
                }
                else
                {
                    if (gxGiaoDanList1.CurrentRow.DataRow != null)
                    {
                        if (!Memory.IsNullOrEmpty(gxGiaoDanList1.CurrentRow.Cells[ChiTietHoiDoanConst.NgayRaHoiDoan].Text.ToString()))
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
                                gxGiaoDanList1.Refetch();
                                gxGiaoDanList1.Refresh();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa thất bại!");
                this.Close();
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
