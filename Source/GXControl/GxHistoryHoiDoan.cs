using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;
using GxGlobal;

namespace GxControl
{
    public partial class GxHistoryHoiDoan : UserControl
    {
        DataTable tblDanhSachHoiDoan;
        DataTable tblDanhSachLichSuHoiDoan;
        
        private int maGiaoDan;

        private GxOperation operation = GxOperation.NONE;
        public GxOperation Operation
        {
            get
            {
                return operation;
            }
            set
            {
                operation = value;
            }
        }
        private bool isChanging = false;
        public bool IsChanging
        {
            get
            {
                return isChanging;
            }
            set
            {
                isChanging = value;
            }
        }
        public int MaGiaoDan
        {
            get
            {
                return maGiaoDan;
            }
            set
            {
                maGiaoDan = value;
            }
        }
        public GxHistoryHoiDoan()
        {
            InitializeComponent();
        }

        private void Combo_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            return;
        }

        private void Combo_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            return;
        }

        public void loaddata(int id)
        {
            string Query = SqlConstants.SELECT_LIST_HISTORY_HOIDOAN_BY_MAGIAODAN;
            gxListHistoryHoiDoan1.LoadData(Query, new object[] { id });
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            if(tblDanhSachHoiDoan.Rows.Count<=0)
            {
                DialogResult tl = MessageBox.Show("Hiện tại chưa có hội đoàn nào !", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
            if(operation==GxOperation.ADD)
            {
                EditEnableControl(false);
                IsChanging = false;
                return;
            }
            EditEnableControl(true);
            IsChanging = true;
        }
        private void EditEnableControl(bool enable)
        {
            if(enable)
            {
                panel1.Enabled = enable;
                gxAddEdit1.AddButton.Text = "&Thôi";
                gxAddEdit1.SelectButton.Enabled = enable;
                Operation = GxOperation.ADD;
                return;
            }
            panel1.Enabled = enable;
            gxAddEdit1.AddButton.Text = "&Thêm";
            gxAddEdit1.SelectButton.Enabled = enable;
            Operation = GxOperation.NONE;
        
            dtNgayRaHoiDoan.IsNullDate = true;
            dtNgayVaoHoiDoan.IsNullDate = true;
            return;

        }

        private void gxAddEdit1_SelectClick(object sender, EventArgs e)
        {
           bool rs= UpdateHoiDoan();
           if(rs)
           {
                EditEnableControl(false);
                IsChanging = false;
           }
           return;
        }
        //Thêm thành viên hội đoàn
        public bool UpdateHoiDoan()
        {
            DialogResult tl;
            if (cbTenHoiDoan.Combo.SelectedIndex<0)
            {
                tl = MessageBox.Show("Vui lòng chọn tên hội đoàn !! Bạn có muốn chọn tên hội đoàn!!!\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không lưu)", "Thông báo",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (tl == DialogResult.Yes)
                {
                    return false;
                }
                return true;
            }
            
            //Kiểm tra giáo dân đó đã tồn tại trong hội đoàn chưa
            tblDanhSachLichSuHoiDoan = (DataTable)gxListHistoryHoiDoan1.DataSource;
            DataRow[] rowds = tblDanhSachLichSuHoiDoan.Select(String.Concat("TenHoiDoan= '", cbTenHoiDoan.combo.Text, "' and NgayRaHoiDoan is null"));
            if (rowds != null && rowds.Length > 0)
            {
                tl=MessageBox.Show(String.Concat("Hiện tại giáo dân đã ở trong hội đoàn ", cbTenHoiDoan.combo.Text," rồi!! Bạn có muốn chỉnh sửa!!!\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không lưu)"),"Thông báo",
                    MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button1);
                if(tl==DialogResult.Yes)
                {
                    return false;
                }
                return true;
            }
            
            
            if (Memory.IsNullOrEmpty(dtNgayVaoHoiDoan.Value))
            {
               tl = MessageBox.Show("Chưa nhập ngày vào hội đoàn.Bạn có muốn tiếp tục không." +
                    "\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để quay lại nhập ngày vào hội đoàn." +
                    "\r\nChọn [Cancel] hoặc tắt hộp thoại để thoát không cập nhật",
                     "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (tl == DialogResult.Cancel)
                {
                    return true;
                }
                if (tl == DialogResult.No)
                {
                    return false;
                }
            }
           
            //Kiểm tra ngày vào có sau ngày ra không
            if (!Memory.IsNullOrEmpty(dtNgayVaoHoiDoan.Value.ToString())&& !Memory.IsNullOrEmpty(dtNgayRaHoiDoan.Value.ToString()))
            {
                if(Memory.CompareTwoStringDate(dtNgayVaoHoiDoan.Value.ToString(), dtNgayRaHoiDoan.Value.ToString()) >=0)
                {
                    tl = MessageBox.Show("Vui lòng kiểm tra lại. Ngày ra hội đoàn không thể trước ngày vào hội đoàn. Bạn có muốn quay lại chỉnh sửa không?.\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không lưu).", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if (tl == DialogResult.Yes)
                    {
                        return false;
                    }
                    return true;
                }
            }
            //Lấy hội đoàn để lấy ngày thành lập
            DataRow[] rows = tblDanhSachHoiDoan.Select(String.Concat("MaHoiDoan=", cbTenHoiDoan.combo.SelectedValue.ToString()));
            // Lấy danh sách các hội viên đã ra khỏi hội đoàn.
            DataTable tblcthd1 = Memory.GetData(String.Concat(SqlConstants.SELECT_CHITIETHOIDOAN_BY_MAHOIDOAN, " and NgayRaHoiDoan is not null order by NgayRaHoiDoan is null"), cbTenHoiDoan.combo.SelectedValue.ToString());
            DataRow[] rowls = tblcthd1.Select(String.Concat("MaGiaoDan=", MaGiaoDan));
            //Ngày vào
            if (!Memory.IsNullOrEmpty(dtNgayVaoHoiDoan.Value.ToString()))
            {
                //Kiểm tra ngày vào hội đoàn có trước ngày thành lập đoàn không
                if (!Memory.IsNullOrEmpty(rows[0][HoiDoanConst.NgayThanhLap].ToString()))
                {
                    if (Memory.CompareTwoStringDate(rows[0][HoiDoanConst.NgayThanhLap].ToString(), dtNgayVaoHoiDoan.Value.ToString()) > 0)
                    {
                        tl = MessageBox.Show("Vui lòng kiểm tra lại. Ngày vào hội đoàn không thể trước ngày thành lập hội đoàn "+ 
                            rows[0][HoiDoanConst.NgayThanhLap].ToString()+". Bạn có muốn quay lại chỉnh sửa không?." +
                            "\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không lưu).", 
                            "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                        if (tl == DialogResult.Yes)
                        {
                            return false;
                        }
                        return true;
                    }
                }
                //Kiểm tra xem ngày vào có sau ngày ra hội đoàn lần gần nhất không
                if (rowls!=null && rowls.Length>0)
                if (Memory.CompareTwoStringDate(rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(), dtNgayVaoHoiDoan.Value.ToString()) >= 0)
                {
                   tl= MessageBox.Show(String.Format("Giáo dân {0} đã từng ở trong hội đoàn và ngày gần nhất ra khỏi hội đoàn là {1}.!!!!\r\n" +
                        "Ngày vào hội đoàn không thể trước hoặc bằng ngày {2} được. " +
                        "Vui lòng kiểm tra lại.\r\nBạn có muốn tiếp tục không cập nhật (không lưu).\r\n" +
                        "Chọn [Yes] nếu có.\r\nChọn [No] để quay lại chỉnh sửa",
                       rowls[0][GiaoDanConst.HoTen].ToString(), rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(),
                       rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString()),
                       "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if(tl==DialogResult.No)
                    {
                        return false;
                    }
                    return true;
                }
            }
            //Ngày ra
            if (!Memory.IsNullOrEmpty(dtNgayRaHoiDoan.Value.ToString()))
            {
                //Kiểm tra ngày ra hội đoàn có trước ngày thành lập đoàn không
                if (!Memory.IsNullOrEmpty(rows[0][HoiDoanConst.NgayThanhLap].ToString()))
                {
                    if (Memory.CompareTwoStringDate(rows[0][HoiDoanConst.NgayThanhLap].ToString(), dtNgayRaHoiDoan.Value.ToString()) > 0)
                    {
                        tl = MessageBox.Show("Vui lòng kiểm tra lại. Ngày ra hội đoàn không thể trước ngày thành lập hội đoàn" + rows[0][HoiDoanConst.NgayThanhLap].ToString() + ". Bạn có muốn quay lại chỉnh sửa không?.\r\n Chọn [Yes] nếu có.\r\n Chọn [No] để không cập nhật (không lưu).", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                        if (tl == DialogResult.Yes)
                        {
                            return false;
                        }
                        return true;
                    }
                }
                //Kiểm tra xem ngày ra có sau ngày ra hội đoàn lần gần nhất không
                if (rowls!=null && rowls.Length>0)
                if (Memory.CompareTwoStringDate(rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(),dtNgayRaHoiDoan.Value.ToString())>=0)
                {
                    tl = MessageBox.Show(String.Format("Giáo dân {0} đã từng ở trong hội đoàn và ngày gần nhất ra khỏi hội đoàn là {1}.!!!!\r\n" +
                        "Ngày ra hội đoàn không thể trước hoặc bằng ngày {2} được. " +
                        "Vui lòng kiểm tra lại.\r\nBạn có muốn tiếp tục không cập nhật (không lưu).\r\n" +
                        "Chọn [Yes] nếu có.\r\nChọn [No] để quay lại chỉnh sửa",
                    rowls[0][GiaoDanConst.HoTen].ToString(), rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString(),
                    rowls[0][ChiTietHoiDoanConst.NgayRaHoiDoan].ToString()),
                    "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if (tl == DialogResult.No)
                    {
                        return false;
                    }
                    return true;
                }
            }


            // Đây là lệnh update
            DataRow row = tblDanhSachLichSuHoiDoan.NewRow();
            row[HoiDoanConst.TenHoiDoan] = cbTenHoiDoan.combo.Text;
            row[ChiTietHoiDoanConst.VaiTro] ="Hội viên";
            row[ChiTietHoiDoanConst.NgayVaoHoiDoan] =dtNgayVaoHoiDoan.Value;
            row[ChiTietHoiDoanConst.NgayRaHoiDoan] =dtNgayRaHoiDoan.Value;
            tblDanhSachLichSuHoiDoan.Rows.Add(row);
            DataTable dataTable = Memory.GetData("Select * from ChiTietHoiDoan");
            DataRow row1 = dataTable.NewRow();
            row1[HoiDoanConst.MaHoiDoan] = cbTenHoiDoan.combo.SelectedValue;
            row1[ChiTietHoiDoanConst.VaiTro] = "Hội viên";
            row1[ChiTietHoiDoanConst.NgayVaoHoiDoan] = dtNgayVaoHoiDoan.Value;
            row1[ChiTietHoiDoanConst.NgayRaHoiDoan] = dtNgayRaHoiDoan.Value;
            row1[GiaoDanConst.MaGiaoDan] =MaGiaoDan;
            dataTable.Rows.Add(row1);
            dataTable.TableName = ChiTietHoiDoanConst.TableName;
            DataSet ds = new DataSet();
            ds.Tables.Add(dataTable);
            Memory.UpdateDataSet(ds);
            if(Memory.ShowError())
            {
                return false;
            }
            EditEnableControl(false);
            IsChanging = false;
            return true;
        }

        private void GxHistoryHoiDoan_Load(object sender, EventArgs e)
        {
            dtNgayVaoHoiDoan.DateInput.ReadOnly = true;
            dtNgayRaHoiDoan.DateInput.ReadOnly = true;
            gxAddEdit1.DeleteButton.Visible = false;
            gxAddEdit1.ReloadButton.Visible = false;
            gxAddEdit1.EditButton.Visible = false;

            gxAddEdit1.SelectButton.Text = "&Lưu";
            //Load danh sách tên hội đoàn lên combobox
            tblDanhSachHoiDoan = Memory.GetData("Select * from HoiDoan");
            cbTenHoiDoan.combo.DataSource = tblDanhSachHoiDoan;
            cbTenHoiDoan.combo.ValueMember = HoiDoanConst.MaHoiDoan;
            cbTenHoiDoan.combo.DisplayMember = HoiDoanConst.TenHoiDoan;

            //Không cho phép chỉnh sửa combobox
            cbTenHoiDoan.combo.KeyPress += Combo_KeyPress;
            cbTenHoiDoan.combo.KeyDown += Combo_KeyDown;
            EditEnableControl(false);
        }
    }
}
