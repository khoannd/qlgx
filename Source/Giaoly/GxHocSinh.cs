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
    public partial class GxHocSinh : GxGrid
    {
        int maLop = 0;

        public int MaLop
        {
            get { return maLop; }
            set { maLop = value; }
        }

        int soThuTu = 0;

        public int SoThuTu
        {
            get { return soThuTu; }
            set { soThuTu = value; }
        }

        string tenThanh = "";

        public string TenThanh
        {
            get { return tenThanh; }
            set { tenThanh = value; }
        }

        string hoTen = "";

        public string HoTen
        {
            get { return hoTen; }
            set { hoTen = value; }
        }

        string phai = "";

        public string Phai
        {
            get { return phai; }
            set { phai = value; }
        }

        private string ngaySinh = "";

        public string NgaySinh
        {
            get { return ngaySinh; }
            set { ngaySinh = value; }
        }

        private bool hoanThanh = false;

        public bool HoanThanh
        {
            get { return hoanThanh; }
            set { hoanThanh = value; }
        }

        private string ghiChu = "";

        public string GhiChu
        {
            get { return ghiChu; }
            set { ghiChu = value; }
        }


        public GxHocSinh()
        {
            InitializeComponent();

            MenuItem item1 = new MenuItem("Xem chi tiết");
            item1.Click += new EventHandler(item1_Click);

            MenuItem item2 = new MenuItem("In chứng nhận bí tích");
            item2.Click += new EventHandler(item2_Click);



            this.RowDoubleClick += new RowActionEventHandler(GxHocSinh_RowDoubleClick);
            
            this.UpdatingCell += new UpdatingCellEventHandler(GxHocSinh_UpdatingCell);
            this.AddingRecord += new CancelEventHandler(GxHocSinh_AddingRecord);
        }

        private void item1_Click(object sender, EventArgs e)
        {
            EditRow();
        }

        private void item2_Click(object sender, EventArgs e)
        {
            InChungNhan();
        }

        public void InChungNhan()
        {

        }

        private void GxHocSinh_AddingRecord(object sender, CancelEventArgs e)
        {

        }

        private void GxHocSinh_UpdatingCell(object sender, UpdatingCellEventArgs e)
        {
            //hiepdv begin add
            if(e.Column.DataMember.ToString()=="SoThuTu")
            {
                if(e.Value.ToString()=="")
                {
                    e.Value = e.InitialValue;
                    MessageBox.Show("Vui lòng nhập số thứ tự là số","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
            //hiepdv end add
            //if (e.Column.DataMember == GiaDinhConst.MaGiaDinh && Validator.IsNumber(e.Value.ToString()))
            //{
            //    if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null)
            //    {
            //        return;
            //    }
            //    DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            //    if (row[GiaDinhConst.MaGiaDinhCo] != DBNull.Value)
            //    {
            //        MessageBox.Show("Giáo dân này đã thuộc về một gia đình\r\nKhông thể chỉnh sửa gia đình cho giáo dân này\r\nHãy nhấp phím [Esc] trên bàn phím để bỏ qua nếu có vấn đề khó khăn", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        SendKeys.Send("{Esc}");
            //        e.Cancel = true;
            //    }
            //    else
            //    {
            //        if (!isValidGiaDinh((int)e.Value, (int)row[GiaoDanConst.MaGiaoDan]))
            //        {
            //            e.Cancel = true;
            //            return;
            //        }
            //        DataTable tblGiaDinh = Memory.GetData(SqlConstants.SELECT_GIADINH_LIST + string.Format(" AND MaGiaDinh={0}", e.Value));
            //        if (!Memory.ShowError() && tblGiaDinh != null)
            //        {
            //            if (tblGiaDinh.Rows.Count > 0)
            //            {
            //                row[GiaDinhConst.TenGiaDinh] = tblGiaDinh.Rows[0][GiaDinhConst.TenGiaDinh];
            //                row[GiaoDanConst.HoTenCha] = tblGiaDinh.Rows[0][GiaDinhConst.TenChong];
            //                row[GiaoDanConst.HoTenMe] = tblGiaDinh.Rows[0][GiaDinhConst.TenVo];
            //            }
            //            else
            //            {
            //                MessageBox.Show("Mã gia đình bạn nhập không tồn tại. Xin vui lòng xem lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                e.Cancel = true;
            //            }
            //        }
                    
            //    }
            //}
        }

        private bool isValidGiaDinh(int maGiaDinh, int maGiaoDan)
        {
            DataRow row = GxGiaDinhList.GetRowGiaDinhVoChong(maGiaoDan);
            if (row != null)
            {
                if ((int)row[GiaDinhConst.MaGiaDinh] == maGiaDinh)
                {
                    MessageBox.Show("Giáo dân được chọn đã là [vợ] hoặc [chồng] trong gia đình bạn nhập. Xin vui lòng xem lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private void GxHocSinh_RowDoubleClick(object sender, RowActionEventArgs e)
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            //if select gia dinh
            if (this.CurrentColumn != null && (this.CurrentColumn.DataMember == GiaDinhConst.MaGiaDinh ||
                this.CurrentColumn.DataMember == GiaDinhConst.TenGiaDinh))
            {
                //if select gia dinh
                if (this.CurrentColumn != null && (this.CurrentColumn.DataMember == GiaDinhConst.MaGiaDinh ||
                    this.CurrentColumn.DataMember == GiaDinhConst.TenGiaDinh))
                {
                    SelectGiaDinh();
                    return;
                }                
            }
            //if edit giao dan
            EditRow();   
        }

        public bool SelectGiaDinh()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return false;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            if (row[GiaDinhConst.MaGiaDinhCo] != DBNull.Value)
            {
                return false;
            }
            frmChonDuLieu frm = new frmChonDuLieu();
            frm.LoaiTimKiem = LoaiTimKiem.GiaDinh;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.DataReturn != null)
                {
                    if (!isValidGiaDinh((int)frm.DataReturn[GiaDinhConst.MaGiaDinh], (int)row[GiaoDanConst.MaGiaoDan]))
                    {
                        return false;
                    }
                    row[GiaDinhConst.MaGiaDinh] = frm.DataReturn[GiaDinhConst.MaGiaDinh];
                    row[GiaDinhConst.TenGiaDinh] = frm.DataReturn[GiaDinhConst.TenGiaDinh];
                    row[GiaoDanConst.HoTenCha] = frm.DataReturn[GiaDinhConst.TenChong];
                    row[GiaoDanConst.HoTenMe] = frm.DataReturn[GiaDinhConst.TenVo];
                }
            }
            return true;
        }

        public override void FormatGrid()
        {
            try
            {
                if (Memory.IsDesignMode) return;

                if (this.RootTable == null) this.RootTable = new GridEXTable();
                base.FormatGrid();
                
                GridEXColumn col1 = this.RootTable.Columns.Add("SoThuTu", ColumnType.Text);
                col1.Width = 50;
                col1.BoundMode = ColumnBoundMode.Bound;
                //col1.DataMember = soBiTichColumnName;
                col1.Caption = "Số thứ tự";                
                //col1.EditType = EditType.NoEdit;
                col1.FilterEditType = FilterEditType.TextBox;

                GridEXColumn col2 = this.RootTable.Columns.Add("TenThanh", ColumnType.Text);
                col2.Width = 90;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = "TenThanh";
                col2.Caption = "Tên thánh";
                col2.EditType = EditType.NoEdit;

                GridEXColumn col3 = this.RootTable.Columns.Add("HoTen", ColumnType.Text);
                col3.Width = 150;
                col3.BoundMode = ColumnBoundMode.Bound;
                col3.DataMember = "HoTen";
                col3.Caption = "Họ tên";
                col3.EditType = EditType.NoEdit;

                GridEXColumn col4 = this.RootTable.Columns.Add("Phai", ColumnType.Text);
                col4.Width = 50;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = "Phai";
                col4.Caption = "Phái";
                col4.EditType = EditType.NoEdit;

                GridEXColumn col5 = this.RootTable.Columns.Add("NgaySinh", ColumnType.Text);
                col5.Width = 80;
                col5.EditType = EditType.NoEdit;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = "NgaySinh";
                col5.Caption = "Ngày sinh";
                col5.FormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatString = "dd/MM/yyyy";
                col5.FormatString = "dd/MM/yyyy";

                GridEXColumn col7 = this.RootTable.Columns.Add("HoanThanh", ColumnType.CheckBox);
                col7.Width = 100;
                col7.BoundMode = ColumnBoundMode.Bound;
                col7.DataMember = "HoanThanh";
                col7.TextAlignment = TextAlignment.Far;
                col7.Caption = "Hoàn thành khóa học";
                col7.EditType = EditType.CheckBox;

                GridEXColumn col8 = this.RootTable.Columns.Add("GhiChuGLy", ColumnType.Text);
                col8.Width = 180;
                col8.BoundMode = ColumnBoundMode.Bound;
                col8.DataMember = "GhiChuGLy";
                col8.Caption = "Ghi chú";
                col8.EditType = EditType.TextBox;

                GridEXColumn col9 = this.RootTable.Columns.Add("MaLop", ColumnType.Text);
                col9.Width = 0;
                col9.BoundMode = ColumnBoundMode.Bound;
                col9.DataMember = "MaLop";
                col9.Visible = false;
                //hiepdv begin add
                this.RootTable.SortKeys.Add(new GridEXSortKey(col1,Janus.Windows.GridEX.SortOrder.Ascending));
                //hiepdv end add
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxHocSinh, FormatGrid)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void FormatGrid2()
        {
            try
            {
                if (Memory.IsDesignMode) return;

                if (this.RootTable == null) this.RootTable = new GridEXTable();
                base.FormatGrid();

                GridEXColumn col1 = this.RootTable.Columns.Add("SoThuTu", ColumnType.Text);
                col1.Width = 50;
                col1.BoundMode = ColumnBoundMode.Bound;
                //col1.DataMember = soBiTichColumnName;
                col1.Caption = "Số thứ tự";
                col1.EditType = EditType.NoEdit;
                col1.FilterEditType = FilterEditType.TextBox;

                GridEXColumn col2 = this.RootTable.Columns.Add("TenThanh", ColumnType.Text);
                col2.Width = 90;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = "TenThanh";
                col2.Caption = "Tên thánh";
                col2.EditType = EditType.NoEdit;

                GridEXColumn col3 = this.RootTable.Columns.Add("HoTen", ColumnType.Text);
                col3.Width = 150;
                col3.BoundMode = ColumnBoundMode.Bound;
                col3.DataMember = "HoTen";
                col3.Caption = "Họ tên";
                col3.EditType = EditType.NoEdit;

                GridEXColumn col4 = this.RootTable.Columns.Add("Phai", ColumnType.Text);
                col4.Width = 50;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = "Phai";
                col4.Caption = "Phái";
                col4.EditType = EditType.NoEdit;

                GridEXColumn col5 = this.RootTable.Columns.Add("NgaySinh", ColumnType.Text);
                col5.Width = 80;
                col5.EditType = EditType.NoEdit;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = "NgaySinh";
                col5.Caption = "Ngày sinh";
                col5.FormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatString = "dd/MM/yyyy";
                col5.FormatString = "dd/MM/yyyy";

                GridEXColumn col7 = this.RootTable.Columns.Add("HoanThanh", ColumnType.CheckBox);
                col7.Width = 100;
                col7.BoundMode = ColumnBoundMode.Bound;
                col7.DataMember = "HoanThanh";
                col7.TextAlignment = TextAlignment.Far;
                col7.Caption = "Hoàn thành khóa học";
                col7.EditType = EditType.NoEdit;

                GridEXColumn col8 = this.RootTable.Columns.Add("GhiChuGLy", ColumnType.Text);
                col8.Width = 130;
                col8.BoundMode = ColumnBoundMode.Bound;
                col8.DataMember = "GhiChuGLy";
                col8.Caption = "Ghi chú";
                col8.EditType = EditType.NoEdit;

                GridEXColumn col9 = this.RootTable.Columns.Add("Chon", ColumnType.CheckBox);
                col9.Width = 50;
                col9.BoundMode = ColumnBoundMode.Bound;
                col9.DataMember = "Chon";
                col9.TextAlignment = TextAlignment.Far;
                col9.Caption = "Chọn";
                col9.EditType = EditType.CheckBox;

                GridEXColumn col10 = this.RootTable.Columns.Add("MaGiaoDan", ColumnType.Text);
                col10.BoundMode = ColumnBoundMode.Bound;
                col10.DataMember = "MaGiaoDan";
                col10.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxHocSinh, FormatGrid)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public override void LoadData()
        {
            if (Memory.IsDesignMode) return;
            try
            {
                //LoadData(QueryString, Arguments);
                string sql = string.Format("SELECT aa.SoThuTu, aa.TenThanh, aa.HoTen, aa.Phai, aa.NgaySinh, GiaoHo.TenGiaoHo, aa.HoanThanh, aa.GhiChuGLy,aa.MaGiaoDan FROM (SELECT b.*,a.HoanThanh,a.GhiChuGLy,a.SoThuTu FROM ChiTietLopGiaoLy a  INNER JOIN GiaoDan b ON a.MaGiaoDan = b.MaGiaoDan WHERE (a.Malop = ?)) aa LEFT OUTER JOIN GiaoHo ON aa.MaGiaoHo = GiaoHo.MaGiaoHo WHERE (aa.DaXoa = 0)");
                DataTable tbl = Memory.GetData(sql, new object[] { MaLop });
                tbl.Columns.Add("Chon", System.Type.GetType("System.Boolean"));
                foreach (DataRow dr in tbl.Rows)
                {
                    dr["Chon"] = false;                  
                }
                if (!Memory.ShowError() && tbl != null)
                {
                    //tbl.TableName = "LopGiaoLy";
                    this.DataSource = tbl;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxLopGiaoLyList, LoadData)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void EditRow()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            frmGiaoDan frm = new frmGiaoDan();
            //
            frm.Operation = GxOperation.NONE;//show for view only
            //
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.Id = (int)row[GiaoDanConst.MaGiaoDan];
            frm.AssignControlData();
            
            if (frm.ShowDialog() == DialogResult.OK)
            {
                //if (frm.DataReturn != null)
                //{
                //    row[GiaoDanConst.TenThanh] = frm.DataReturn[GiaoDanConst.TenThanh];
                //    row[GiaoDanConst.HoTen] = frm.DataReturn[GiaoDanConst.HoTen];
                //    row[GiaoDanConst.NgaySinh] = frm.DataReturn[GiaoDanConst.NgaySinh];
                //    row[GiaoDanConst.Phai] = frm.DataReturn[GiaoDanConst.Phai];
                //    row[GiaoDanConst.SoRuaToi] = frm.DataReturn[GiaoDanConst.SoRuaToi];
                //    row[GiaoDanConst.SoRuocLe] = frm.DataReturn[GiaoDanConst.SoRuocLe];
                //    row[GiaoDanConst.SoThemSuc] = frm.DataReturn[GiaoDanConst.SoThemSuc];
                //    row[GiaoDanConst.NguoiDoDauRuaToi] = frm.DataReturn[GiaoDanConst.NguoiDoDauRuaToi];
                //    row[GiaoDanConst.NguoiDoDauThemSuc] = frm.DataReturn[GiaoDanConst.NguoiDoDauThemSuc];
                //}
            }
        }
    }
}
     