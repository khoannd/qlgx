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
    public partial class GxBiTichChiTiet : GxGrid
    {
        string soBiTichColumnText = "";

        public string SoBiTichColumnText
        {
            get { return soBiTichColumnText; }
            set { soBiTichColumnText = value; }
        }

        string soBiTichColumnName = "";

        public string SoBiTichColumnName
        {
            get { return soBiTichColumnName; }
            set { soBiTichColumnName = value; }
        }

        string nguoiDoDauColumnName = "";

        public string NguoiDoDauColumnName
        {
            get { return nguoiDoDauColumnName; }
            set { nguoiDoDauColumnName = value; }
        }

        string ngayBiTichColumnName = "";

        public string NgayBiTichColumnName
        {
            get { return ngayBiTichColumnName; }
            set { ngayBiTichColumnName = value; }
        }

        private string linhMucColumnName = "";

        public string LinhMucColumnName
        {
            get { return linhMucColumnName; }
            set { linhMucColumnName = value; }
        }

        private string noiBiTichColumnName = "";

        public string NoiBiTichColumnName
        {
            get { return noiBiTichColumnName; }
            set { noiBiTichColumnName = value; }
        }

        private string tenBiTich = "";

        public string TenBiTich
        {
            get { return tenBiTich; }
            set { tenBiTich = value; }
        }

        private int maDotBiTich = -1;

        /// <summary>
        /// Gets or sets Ma dot bi tich. When set value, data to be loaded automatically
        /// </summary>
        public int MaDotBiTich
        {
            get { return maDotBiTich; }
            set
            {
                maDotBiTich = value;

                if (!Memory.IsDesignMode && value > -1)
                {
                    LoadData();
                }
            }
        }

        private LoaiBiTich loaiBiTich = LoaiBiTich.RuaToi;

        public LoaiBiTich LoaiBiTich
        {
            get { return loaiBiTich; }
            set { loaiBiTich = value; }
        }

        private ContextMenu contextMenu = new ContextMenu();

        public GxBiTichChiTiet()
        {
            InitializeComponent();

            MenuItem item1 = new MenuItem("Xem chi tiết");
            item1.Click += new EventHandler(item1_Click);

            MenuItem item2 = new MenuItem("In chứng nhận bí tích");
            item2.Click += new EventHandler(item2_Click);

            contextMenu.MenuItems.Add(item1);
            contextMenu.MenuItems.Add(item2);

            this.ContextMenu = contextMenu;

            this.RowDoubleClick += new RowActionEventHandler(GxDotBiTichChiTiet_RowDoubleClick);
            
            this.UpdatingCell += new UpdatingCellEventHandler(GxBiTichChiTiet_UpdatingCell);
            this.AddingRecord += new CancelEventHandler(GxBiTichChiTiet_AddingRecord);
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
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            GxGiaoDanList.InChungNhanBiTich((int)row[GiaoDanConst.MaGiaoDan], loaiBiTich);
        }

        private void GxBiTichChiTiet_AddingRecord(object sender, CancelEventArgs e)
        {

        }

        private void GxBiTichChiTiet_UpdatingCell(object sender, UpdatingCellEventArgs e)
        {
            if (e.Column.DataMember == GiaDinhConst.MaGiaDinh && Validator.IsNumber(e.Value.ToString()))
            {
                if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null)
                {
                    return;
                }
                DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
                if (row[GiaDinhConst.MaGiaDinhCo] != DBNull.Value)
                {
                    MessageBox.Show("Giáo dân này đã thuộc về một gia đình\r\nKhông thể chỉnh sửa gia đình cho giáo dân này\r\nHãy nhấp phím [Esc] trên bàn phím để bỏ qua nếu có vấn đề khó khăn", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SendKeys.Send("{Esc}");
                    e.Cancel = true;
                }
                else
                {
                    if (!isValidGiaDinh((int)e.Value, (int)row[GiaoDanConst.MaGiaoDan]))
                    {
                        e.Cancel = true;
                        return;
                    }
                    DataTable tblGiaDinh = Memory.GetData(SqlConstants.SELECT_GIADINH_LIST + string.Format(" AND MaGiaDinh={0}", e.Value));
                    if (!Memory.ShowError() && tblGiaDinh != null)
                    {
                        if (tblGiaDinh.Rows.Count > 0)
                        {
                            row[GiaDinhConst.TenGiaDinh] = tblGiaDinh.Rows[0][GiaDinhConst.TenGiaDinh];
                            row[GiaoDanConst.HoTenCha] = tblGiaDinh.Rows[0][GiaDinhConst.TenChong];
                            row[GiaoDanConst.HoTenMe] = tblGiaDinh.Rows[0][GiaDinhConst.TenVo];
                        }
                        else
                        {
                            MessageBox.Show("Mã gia đình bạn nhập không tồn tại. Xin vui lòng xem lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                    }
                    
                }
            }
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

        private void GxDotBiTichChiTiet_RowDoubleClick(object sender, RowActionEventArgs e)
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
                    //check gia đình đã xóa hoặc chuyển xứ
                    string warningMessage = "";
                    //Check giáo dân trong hồ sơ lưu trữ
                    if ((bool)frm.DataReturn[GiaDinhConst.DaChuyenXu])
                    {
                        warningMessage = "đã chuyển xứ";
                    }
                    if ((bool)frm.DataReturn[GiaDinhConst.DaXoa])
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
                        DialogResult tl= MessageBox.Show(string.Format("Gia đình {0} hiện tại {1}. Bạn có muốn tiếp tục chọn gia đình này." +
                            "\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để hủy.", frm.DataReturn[GiaDinhConst.TenGiaDinh], warningMessage), 
                            "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (tl == DialogResult.No)
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

                this.RootTable.Columns.Clear();

                switch (loaiBiTich)
                {
                    case LoaiBiTich.RuocLe:
                        soBiTichColumnName = GiaoDanConst.SoRuocLe;
                        ngayBiTichColumnName = GiaoDanConst.NgayRuocLe;
                        linhMucColumnName = GiaoDanConst.ChaRuocLe;
                        noiBiTichColumnName = GiaoDanConst.NoiRuocLe;
                        soBiTichColumnText = "Số XTRL";
                        tenBiTich = "XTRL lần đầu";
                        break;
                    case LoaiBiTich.ThemSuc:
                        soBiTichColumnName = GiaoDanConst.SoThemSuc;
                        nguoiDoDauColumnName = GiaoDanConst.NguoiDoDauThemSuc;
                        ngayBiTichColumnName = GiaoDanConst.NgayThemSuc;
                        linhMucColumnName = GiaoDanConst.ChaThemSuc;
                        noiBiTichColumnName = GiaoDanConst.NoiThemSuc;
                        soBiTichColumnText = "Số thêm sức";
                        tenBiTich = "Thêm sức";
                        break;
                    default: 
                        soBiTichColumnName = GiaoDanConst.SoRuaToi;
                        nguoiDoDauColumnName = GiaoDanConst.NguoiDoDauRuaToi;
                        ngayBiTichColumnName = GiaoDanConst.NgayRuaToi;
                        linhMucColumnName = GiaoDanConst.ChaRuaToi;
                        noiBiTichColumnName = GiaoDanConst.NoiRuaToi;
                        soBiTichColumnText = "Số rửa tội";
                        tenBiTich = "Rửa tội";
                        break;
                }

                GridEXColumn col1 = this.RootTable.Columns.Add(soBiTichColumnName, ColumnType.Text);
                col1.Width = 80;
                col1.BoundMode = ColumnBoundMode.Bound;
                col1.DataMember = soBiTichColumnName;
                col1.Caption = soBiTichColumnText;
                col1.EditType = EditType.TextBox;
                col1.FilterEditType = FilterEditType.TextBox;

                GridEXColumn col2 = this.RootTable.Columns.Add(GiaoDanConst.TenThanh, ColumnType.Text);
                col2.Width = 100;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = GiaoDanConst.TenThanh;
                col2.Caption = "Tên thánh";
                if ((int)loaiBiTich > 0)
                {
                    col2.EditType = EditType.NoEdit;
                }
                else
                {
                    col1.FilterEditType = FilterEditType.TextBox;
                }

                GridEXColumn col3 = this.RootTable.Columns.Add(GiaoDanConst.HoTen, ColumnType.Text);
                col3.Width = 150;
                col3.BoundMode = ColumnBoundMode.Bound;
                col3.DataMember = GiaoDanConst.HoTen;
                col3.Caption = "Họ tên";
                col3.EditType = EditType.NoEdit;

                GridEXColumn col4 = this.RootTable.Columns.Add(GiaoDanConst.Phai, ColumnType.Text);
                col4.Width = 50;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = GiaoDanConst.Phai;
                col4.Caption = "Phái";
                col4.EditType = EditType.NoEdit;

                GridEXColumn col5 = this.RootTable.Columns.Add(GiaoDanConst.NgaySinh, ColumnType.Text);
                col5.Width = 80;
                col5.EditButtonDisplayMode = CellButtonDisplayMode.EditingCell;
                col5.ButtonDisplayMode = CellButtonDisplayMode.CurrentCell;
                col5.EditType = EditType.NoEdit;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = GiaoDanConst.NgaySinh;
                col5.TextAlignment = TextAlignment.Far;
                col5.Caption = "Ngày sinh";
                col5.FormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatMode = FormatMode.UseIFormattable;
                col5.DefaultGroupFormatString = "dd/MM/yyyy";
                col5.FormatString = "dd/MM/yyyy";

                GridEXColumn col7 = this.RootTable.Columns.Add(GiaDinhConst.MaGiaDinh, ColumnType.Text);
                col7.Width = 50;
                col7.BoundMode = ColumnBoundMode.Bound;
                col7.DataMember = GiaDinhConst.MaGiaDinh;
                col7.TextAlignment = TextAlignment.Far;
                col7.Caption = "Mã GĐ";
                col7.EditType = EditType.TextBox;

                GridEXColumn col8 = this.RootTable.Columns.Add(GiaDinhConst.TenGiaDinh, ColumnType.Text);
                col8.Width = 80;
                col8.BoundMode = ColumnBoundMode.Bound;
                col8.DataMember = GiaDinhConst.TenGiaDinh;
                col8.Caption = "Tên GĐ";
                col8.EditType = EditType.NoEdit;

                if (loaiBiTich == LoaiBiTich.RuaToi || loaiBiTich == LoaiBiTich.ThemSuc)
                {
                    GridEXColumn colDoDau = new GridEXColumn(nguoiDoDauColumnName);
                    colDoDau.Caption = "Người đỡ đầu";
                    colDoDau.Width = 150;
                    colDoDau.BoundMode = ColumnBoundMode.Bound;
                    colDoDau.DataMember = nguoiDoDauColumnName;
                    colDoDau.ColumnType = ColumnType.Text;
                    this.RootTable.Columns.Add(colDoDau);
                }

                GridEXColumn col6 = this.RootTable.Columns.Add(BiTichChiTietConst.GhiChu1, ColumnType.Text);
                col6.Width = 200;
                col6.BoundMode = ColumnBoundMode.Bound;
                col6.DataMember = BiTichChiTietConst.GhiChu1;
                col6.Caption = "Ghi chú";
                col6.EditType = EditType.TextBox;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxBiTichChiTiet, FormatGrid)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void LoadData()
        {
            try
            {
                string orderby = "";
                switch (loaiBiTich)
                {
                    case LoaiBiTich.RuocLe:
                        orderby = GiaoDanConst.SoRuocLe;
                        break;
                    case LoaiBiTich.ThemSuc:
                        orderby = GiaoDanConst.SoThemSuc;
                        break;
                    default:
                        orderby = GiaoDanConst.SoRuaToi;
                        break;
                }
                LoadData(string.Format(SqlConstants.SELECT_BITICH_CHITIET_THEODOT+ " ORDER BY {0} ASC",orderby), new object[] { maDotBiTich });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxBiTichChiTiet, LoadData)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void EditRow()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            frmGiaoDan frm = new frmGiaoDan();
            frm.Operation = GxOperation.EDIT;//show for view only
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
     