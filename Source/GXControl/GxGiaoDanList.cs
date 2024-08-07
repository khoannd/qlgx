using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;
using System.Threading;
using GxGlobal;
using System.Diagnostics;

namespace GxControl
{
    public partial class GxGiaoDanList : GxGrid
    {
        private ContextMenu contextMenu = new ContextMenu();
        private bool allowShowForm = true;
        static DataSet ds = new DataSet();
        static Dictionary<int, string> dicQuanHe = null;
        public bool AllowShowForm
        {
            get { return allowShowForm; }
            set { allowShowForm = value; }
        }

        private bool allowContextMenu = true;

        public bool AllowContextMenu
        {
            get { return allowContextMenu; }
            set { allowContextMenu = value; }
        }

        private bool allowEditGiaoDan = true;

        public bool AllowEditGiaoDan
        {
            get { return allowEditGiaoDan; }
            set { allowEditGiaoDan = value; }
        }

        public GxGiaoDanList()
        {
            try
            {
                InitializeComponent();
                if (allowContextMenu)
                {
                    MenuItem item1 = new MenuItem("Xem chi tiết");
                    item1.Click += new EventHandler(item1_Click);

                    MenuItem item2 = new MenuItem("In chứng nhận bí tích");
                    item2.Click += new EventHandler(item2_Click);

                    MenuItem item3 = new MenuItem("In giới thiệu hôn phối");
                    item3.Click += new EventHandler(item3_Click);

                    MenuItem item4 = new MenuItem("In chứng nhận rửa tội");
                    item4.Click += new EventHandler(item4_Click);

                    MenuItem item5 = new MenuItem("In chứng nhận xưng tội - rước lễ");
                    item5.Click += new EventHandler(item5_Click);

                    MenuItem item6 = new MenuItem("In chứng nhận thêm sức");
                    item6.Click += new EventHandler(item6_Click);

                    //MenuItem item7 = new MenuItem("In điều tra và rao hôn phối");
                    //item7.Click += new EventHandler(item7_Click);

                    MenuItem item8 = new MenuItem("Xem gia đình");
                    item8.Click += new EventHandler(item8_Click);

                    MenuItem item9 = new MenuItem("In lý lịch cá nhân");
                    item9.Click += new EventHandler(item9_Click);
                    MenuItem item10 = new MenuItem("In giấy giới thiệu chứng nhận rửa tội");
                    MenuItem item11 = new MenuItem("In giấy giới thiệu giáo lý hôn phối");
                    MenuItem item12 = new MenuItem("In giấy giới thiệu chứng nhận thêm sức");
                    MenuItem item13 = new MenuItem("Xem vị trí");
                    item13.Click += Item13_Click;
                    item12.Click += Item12_Click;
                    item11.Click += Item11_Click;
                    item10.Click += Item10_Click;
                    contextMenu.MenuItems.Add(item1);
                    contextMenu.MenuItems.Add(item9);
                    contextMenu.MenuItems.Add(item2);
                    //contextMenu.MenuItems.Add(item7);
                    contextMenu.MenuItems.Add(item3);
                    contextMenu.MenuItems.Add(item4);
                    contextMenu.MenuItems.Add(item5);
                    contextMenu.MenuItems.Add(item6);
                    contextMenu.MenuItems.Add(item8);
                    contextMenu.MenuItems.Add(item10);
                    contextMenu.MenuItems.Add(item11);
                    contextMenu.MenuItems.Add(item12);
                    contextMenu.MenuItems.Add(item13);
                    this.ContextMenu = contextMenu;
                }
                this.RowDoubleClick += new RowActionEventHandler(GXGiaoDanList_RowDoubleClick);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxGiaoDanList, Constructor)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Item13_Click(object sender, EventArgs e)
        {
            ViewMapGiaoDan();
        }

        private void Item12_Click(object sender, EventArgs e)
        {
            if (this.CurrentRow.RowIndex == -1) return;
            frmReport frm = new frmReport();
            frm.EType = TypeExport.GioiThieuThemSuc;
            frm.RowInfor = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.ShowDialog();
        }
        private void Item11_Click(object sender, EventArgs e)
        {
            if (this.CurrentRow.RowIndex == -1) return;
            frmReport frm = new frmReport();
            frm.EType = TypeExport.GioiThieuGiaoLyPhonPhoi;
            frm.RowInfor = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.ShowDialog();
        }

        private void Item10_Click(object sender, EventArgs e)
        {
            if (this.CurrentRow.RowIndex == -1) return;
            frmReport frm = new frmReport();
            frm.EType = TypeExport.GioiThieuRuaToi;
            frm.RowInfor = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.ShowDialog();
        }

        public void XuatChungNhanBiTich()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            GetDataChungNhanBiTich((int)row[GiaoDanConst.MaGiaoDan]);
            frmGoiChungNhan frm = new frmGoiChungNhan();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ds.Tables.Add(frm.GetTableNguoiNhan());
                int rs = ExcelReport.ReportChungNhanBT.Export(ds);
                Memory.ShowError();
            }
        }

        public void XuatLyLichCaNhan()
        {
            //2018-07-31 Gia modify start
            if (this.SelectedItems.Count==1)
            {
                if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
                DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
                XuatLyLichCaNhan((int)row[GiaoDanConst.MaGiaoDan], -1);
                return;
            }
           
            //mess chon kieu print
            int option = -1;
            frmPrint print = new frmPrint();
            DialogResult rs = print.ShowDialog();
            if (rs == DialogResult.OK)
            {
                option = print.getOption();
            }
            print.Dispose();
            if (option != -1)
            {
                switch ((PrintOperation)option)
                {
                    case PrintOperation.SINGLE:
                        foreach (GridEXSelectedItem item in this.SelectedItems)
                        {
                            DataRow row = (item.GetRow().DataRow as DataRowView).Row;
                            XuatLyLichCaNhan((int)row[GiaoDanConst.MaGiaoDan], -1);
                        }
                        break;
                    case PrintOperation.MULTI:
                        Dictionary<int, int> dicMaGiaoDan = new Dictionary<int, int>();
                        foreach (GridEXSelectedItem item in this.SelectedItems)
                        {
                            DataRow row = (item.GetRow().DataRow as DataRowView).Row;
                            dicMaGiaoDan.Add((int)row[GiaoDanConst.MaGiaoDan], -1);
                        }
                        XuatLyLichCaNhan(dicMaGiaoDan);
                        break;
                }
            }
            //2018-07-31 Gia modify end
        }

        public static string GetVaiTroText(int vaiTro)
        {
            if (dicQuanHe == null)
            {
                dicQuanHe = Memory.GetQuanHeList(true);
            }
            if (dicQuanHe.ContainsKey(vaiTro))
            {
                return dicQuanHe[vaiTro];
            }
            return "";
        }
        public static void XuatLyLichCaNhan(int maGiaoDan, int vaiTro)
        {
            GetDataChungNhanBiTich(maGiaoDan);
            string strVaiTro = GetVaiTroText(vaiTro);
            if (!string.IsNullOrEmpty(strVaiTro))
            {
                strVaiTro = string.Format("Quan hệ GĐ: {0}", strVaiTro);
            }
            WordEngine word = ExcelReport.ReportLyLichCaNhan.Export(ds, strVaiTro, null);
            word.End_Write();
            if (System.IO.File.Exists(word.FileName))
            {
                System.Diagnostics.Process.Start(word.FileName);
            }

            Memory.ShowError();
        }

        public static void XuatLyLichCaNhan(Dictionary<int, int> dicMaGiaoDan)
        {
            if (dicMaGiaoDan != null && dicMaGiaoDan.Count > 0)
            {
                WordEngine word = null;
                int i = 0;
                foreach (var item in dicMaGiaoDan)
                {
                    var maGiaoDan = item.Key;
                    var vaiTro = item.Value;
                    GetDataChungNhanBiTich(maGiaoDan);
                    string strVaiTro = GetVaiTroText(vaiTro);
                    if (!string.IsNullOrEmpty(strVaiTro))
                    {
                        strVaiTro = string.Format("Quan hệ GĐ: {0}", strVaiTro);
                    }
                    if (word == null)
                    {
                        word = ExcelReport.ReportLyLichCaNhan.Export(ds, strVaiTro, word);
                    }
                    else
                    {
                        ExcelReport.ReportLyLichCaNhan.Export(ds, strVaiTro, word);
                    }
                    Memory.ShowError();
                    if (i < dicMaGiaoDan.Count - 1)
                    {
                        word.MoveEnd();
                        word.InsertPage();
                        word.MoveEnd();
                    }
                    i++;
                }
                word.End_Write();
                if (System.IO.File.Exists(word.FileName))
                {
                    System.Diagnostics.Process.Start(word.FileName);
                }
            }
        }

        public void XuatChungNhanMotBiTich(LoaiBiTich loaiBiTich)
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            InChungNhanBiTich((int)row[GiaoDanConst.MaGiaoDan], loaiBiTich);
        }

        public static void InChungNhanBiTich(int maGiaoDan, LoaiBiTich loaiBiTich)
        {
            Memory.Instance.SetMemory(GxConstants.CURRENT_REPORT, loaiBiTich);
            GetDataChungNhanBiTich(maGiaoDan);
            int rs = ExcelReport.ReportChungNhanBT.Export(ds);
            Memory.ShowError();
            Memory.Instance.SetMemory(GxConstants.CURRENT_REPORT, null);
        }

        private void item8_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
                DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
                int maGiaoDan = (int)row[GiaoDanConst.MaGiaoDan];
                showGiaDinh(maGiaoDan);
            }
            catch (Exception ex)
            {
                Memory.ShowError(ex.Message);
            }
        }
        public void showGiaDinh(int maGiaoDan)
        {
            try
            {
                DataTable tblTVGD = Memory.GetTable(ThanhVienGiaDinhConst.TableName, " AND MaGiaoDan=" + maGiaoDan.ToString());
                if (!Memory.ShowError())
                {
                    //neu khong tim thay
                    if (tblTVGD.Rows.Count == 0)
                    {
                        MessageBox.Show("Giáo dân này không thuộc gia đình nào");
                        return;
                    }
                    //neu chi co 1 gia dinh thoi thi show gia dinh len luon
                    if (tblTVGD.Rows.Count == 1)
                    {
                        frmGiaDinh frmGD = new frmGiaDinh();
                        frmGD.Id = (int)tblTVGD.Rows[0][ThanhVienGiaDinhConst.MaGiaDinh];
                        frmGD.AssignControlData();
                        frmGD.Operation = GxOperation.EDIT;
                        frmGD.ShowDialog();
                        return;
                    }
                    //neu co nhieu hon 1 gia đinh thi show danh sach cho nguoi ta chon
                    List<int> lstMaGiaDinh = new List<int>();
                    foreach (DataRow rowTV in tblTVGD.Rows)
                    {
                        lstMaGiaDinh.Add((int)rowTV[ThanhVienGiaDinhConst.MaGiaDinh]);
                    }
                    frmXemGiaDinhGiaoDan frmXem = new frmXemGiaDinhGiaoDan();
                    frmXem.MaGiaDinhList = lstMaGiaDinh;
                    frmXem.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Memory.ShowError(ex.Message);
            }
        }

        private void item9_Click(object sender, EventArgs e)
        {
            XuatLyLichCaNhan();
        }

        private void item1_Click(object sender, EventArgs e)
        {
            EditRow();
        }

        private void item2_Click(object sender, EventArgs e)
        {
            Memory.Instance.SetMemory(GxConstants.CURRENT_REPORT, LoaiBiTich.TatCa);
            XuatChungNhanBiTich();
            Memory.Instance.SetMemory(GxConstants.CURRENT_REPORT, null);
        }

        public void item3_Click(object sender, EventArgs e)
        {
            XuatGioiThieuHonPhoi();
        }

        private void item4_Click(object sender, EventArgs e)
        {
            XuatChungNhanMotBiTich(LoaiBiTich.RuaToi);
        }

        private void item5_Click(object sender, EventArgs e)
        {
            XuatChungNhanMotBiTich(LoaiBiTich.RuocLe);
        }

        private void item6_Click(object sender, EventArgs e)
        {
            XuatChungNhanMotBiTich(LoaiBiTich.ThemSuc);
        }

        private void item7_Click(object sender, EventArgs e)
        {
            XuatRaoHonPhoi();
        }

        public void XuatRaoHonPhoi()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            if ((bool)row[GiaoDanConst.DaCoGiaDinh])
            {
                if (MessageBox.Show("Giáo dân này đã từng lập gia đình.\r\nBạn có chắc muốn rao  hôn phối cho giáo dân này không?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }
            frmRaoHonPhoi frm = new frmRaoHonPhoi();
            frm.CurrentRow = row;
            frm.UsePrint = true;
            frm.ShowDialog();
        }

        public void XuatGioiThieuHonPhoi()
        {
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            if ((bool)row[GiaoDanConst.DaCoGiaDinh])
            {
                if (MessageBox.Show("Giáo dân này đã từng lập gia đình.\r\nBạn có chắc muốn giới thiệu hôn phối cho giáo dân này không?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }
            GetDataGioiThieuHonPhoi((int)row[GiaoDanConst.MaGiaoDan]);
            frmReportGioiThieuHP frm = new frmReportGioiThieuHP();
            frm.MaGiaoDan = (int)row[GiaoDanConst.MaGiaoDan];
            frm.TenGiaoDan = row[GiaoDanConst.TenThanh].ToString() + " " + row[GiaoDanConst.HoTen].ToString();
            //2018-01-12 SonVc add start
            frm.GioiTinh = row[GiaoDanConst.Phai].ToString();
            //2018-01-12 SonVc add end
            frm.DataObj = ds;
            frm.ShowDialog();
        }

        public static void GetDataChungNhanBiTich(int maGiaoDan)
        {
            try
            {
                ds = new DataSet();
                //Get Giaoxu info
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                if (Memory.ShowError())
                {
                    return;
                }
                if (tblGiaoXu.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thông tin giáo xứ. Vui lòng nhập thông tin giáo xứ trước khi sử dụng chức năng này.");
                    return;
                }
                tblGiaoXu.Columns.Add(ReportGiaoDanConst.TenLinhMucGui);
                //get linh muc info
                DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND ChucVu='Chánh xứ' AND DenNgay IS NULL ");
                if (!Memory.ShowError() && tblLinhMuc != null && tblLinhMuc.Rows.Count > 0)
                {
                    tblGiaoXu.Rows[0][ReportGiaoDanConst.TenLinhMucGui] = tblLinhMuc.Rows[0][LinhMucConst.TenThanh].ToString() + " " + tblLinhMuc.Rows[0][LinhMucConst.HoTen].ToString();
                }
                tblGiaoXu.TableName = GiaoXuConst.TableName;
                ds.Tables.Add(tblGiaoXu);

                //get giao dan info
                DataTable tblTmp = Memory.GetData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + " AND MaGiaoDan=?", new object[] { maGiaoDan });
                if (Memory.ShowError() || tblTmp == null || tblTmp.Rows.Count == 0)
                {
                    MessageBox.Show("Rất tiếc! Có lỗi không mong muốn xảy ra.\r\nVui lòng liên hệ với người chịu trách nhiệm phần mềm", "Lỗi không tìm thấy dữ liệu giáo dân", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                tblTmp.Columns.Add("TenGiaoHoCha", typeof(string));
                DataRow row = tblTmp.Rows[0];

                //select GiaoHo, GiaoXom
                if (!Memory.IsNullOrEmpty(row[GiaoHoConst.MaGiaoHoCha])) //if is GiaoXom, them Select GiaoHo 
                {
                    DataTable tblGiaoHo = Memory.GetData("SELECT * FROM GiaoHo WHERE MaGiaoHo=" + row[GiaoHoConst.MaGiaoHoCha].ToString());
                    if (!Memory.ShowError() && tblGiaoHo != null && tblGiaoHo.Rows.Count != 0)
                    {
                        row["TenGiaoHoCha"] = tblGiaoHo.Rows[0]["TenGiaoHo"].ToString();
                    }
                }
                else
                {
                    row["TenGiaoHoCha"] = row[GiaoHoConst.TenGiaoHo];
                }

                DataTable tblGiaoDan = tblTmp;
                Dictionary<object, object> dicChaMe = GxGiaDinhList.GetTenChaMe((int)row[GiaoDanConst.MaGiaoDan], row[GiaoDanConst.HoTenCha], row[GiaoDanConst.HoTenMe]);
                row[GiaoDanConst.HoTenCha] = dicChaMe[GxConstants.VAITRO_CHONG];
                row[GiaoDanConst.HoTenMe] = dicChaMe[GxConstants.VAITRO_VO];

                tblGiaoDan.TableName = GiaoDanConst.TableName;
                ds.Tables.Add(tblGiaoDan);

                //get hon phoi
                if ((bool)row[GiaoDanConst.DaCoGiaDinh])
                {
                    DataTable tblHonPhoi = Memory.GetData(SqlConstants.SELECT_HONPHOI_THEO_MAGIAODAN + " AND GiaoDanHonPhoi_1.MaGiaoDan <> ?  ORDER BY HP.NgayHonPhoi DESC", maGiaoDan, maGiaoDan);
                    if (tblHonPhoi != null && tblHonPhoi.Rows.Count > 0)
                    {
                        tblHonPhoi.TableName = HonPhoiConst.TableName;
                        ds.Tables.Add(tblHonPhoi);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void GetDataGioiThieuHonPhoi(int maGiaoDan)
        {
            try
            {
                ds = new DataSet();
                //Get Giaoxu info
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                if (Memory.ShowError())
                {
                    return;
                }
                if (tblGiaoXu.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thông tin giáo xứ. Vui lòng nhập thông tin giáo xứ trước khi sử dụng chức năng này.");
                    return;
                }
                tblGiaoXu.Columns.Add(ReportGiaoDanConst.TenLinhMucGui);
                //get linh muc info
                DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ");
                if (!Memory.ShowError() && tblLinhMuc != null && tblLinhMuc.Rows.Count > 0)
                {
                    tblGiaoXu.Rows[0][ReportGiaoDanConst.TenLinhMucGui] = tblLinhMuc.Rows[0][LinhMucConst.TenThanh].ToString() + " " + tblLinhMuc.Rows[0][LinhMucConst.HoTen].ToString();
                }
                tblGiaoXu.TableName = GiaoXuConst.TableName;
                ds.Tables.Add(tblGiaoXu);

                //get giao dan info
                DataTable tblTmp = Memory.GetData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + " AND MaGiaoDan=?", new object[] { maGiaoDan });
                if (Memory.ShowError() || tblTmp == null || tblTmp.Rows.Count == 0)
                {
                    MessageBox.Show("Rất tiếc! Có lỗi không mong muốn xảy ra.\r\nVui lòng liên hệ với người chịu trách nhiệm phần mềm", "Lỗi không tìm thấy dữ liệu giáo dân", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DataRow row = tblTmp.Rows[0];

                DataTable tblGiaoDan = tblTmp;
                Dictionary<object, object> dicChaMe = GxGiaDinhList.GetTenChaMe((int)row[GiaoDanConst.MaGiaoDan], row[GiaoDanConst.HoTenCha], row[GiaoDanConst.HoTenMe]);
                row[GiaoDanConst.HoTenCha] = dicChaMe[GxConstants.VAITRO_CHONG];
                row[GiaoDanConst.HoTenMe] = dicChaMe[GxConstants.VAITRO_VO];

                tblGiaoDan.TableName = GiaoDanConst.TableName;
                ds.Tables.Add(tblGiaoDan);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GXGiaoDanList_RowDoubleClick(object sender, RowActionEventArgs e)
        {
            if (allowShowForm)
            {
                EditRow();
            }
        }

        public virtual void EditRow()
        {
            if (!allowShowForm) return;
            if (this.CurrentRow == null || (this.CurrentRow.DataRow as DataRowView) == null) return;
            frmGiaoDan frm = new frmGiaoDan();
            if (allowEditGiaoDan)
            {
                frm.Operation = GxOperation.EDIT;
            }
            else
            {
                frm.Operation = GxOperation.VIEW;
            }
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            if (this.RootTable.Columns.Contains(ThanhVienGiaDinhConst.VaiTro))
            {
                Memory.Instance.SetMemory(ThanhVienGiaDinhConst.VaiTro, this.CurrentRow.Cells[0].Value);
            }
            SetFormData(frm);
            frm.AssignControlData();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.DataReturn != null)
                {
                    //row[GiaoDanConst.TenThanh] = frm.DataReturn[GiaoDanConst.TenThanh];
                    //row[GiaoDanConst.HoTen] = frm.DataReturn[GiaoDanConst.HoTen];
                    //row[GiaoDanConst.NgaySinh] = frm.DataReturn[GiaoDanConst.NgaySinh];
                    //row[GiaoDanConst.Phai] = frm.DataReturn[GiaoDanConst.Phai];
                    //row[GiaoHoConst.TenGiaoHo] = frm.DataReturn[GiaoHoConst.TenGiaoHo];
                    foreach (DataColumn col in frm.DataReturn.Table.Columns)
                    {
                        if (row.Table.Columns.Contains(col.ColumnName))
                        {
                            row[col.ColumnName] = frm.DataReturn[col.ColumnName];
                        }
                    }
                }
            }
        }

        protected virtual void SetFormData(frmGiaoDan frm)
        {
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.Id = (int)row[GiaoDanConst.MaGiaoDan];
        }

        public override void FormatGrid()
        {
            try
            {
                if (Memory.IsDesignMode) return;

                if (this.RootTable == null) this.RootTable = new GridEXTable();
                base.FormatGrid();

                this.RootTable.Columns.Clear();
                this.ColumnAutoResize = false;
                GridEXColumn col1 = this.RootTable.Columns.Add(GiaoDanConst.MaGiaoDan, ColumnType.Text);
                col1.Width = 50;
                col1.BoundMode = ColumnBoundMode.Bound;
                col1.DataMember = GiaoDanConst.MaGiaoDan;
                col1.Caption = "Mã GD";
                col1.FilterEditType = FilterEditType.Combo;

                GridEXColumn col2 = this.RootTable.Columns.Add(GiaoDanConst.TenThanh, ColumnType.Text);
                col2.Width = 80;
                col2.BoundMode = ColumnBoundMode.Bound;
                col2.DataMember = GiaoDanConst.TenThanh;
                col2.Caption = "Tên thánh";
                col2.FilterEditType = FilterEditType.Combo;

                GridEXColumn col3 = this.RootTable.Columns.Add(GiaoDanConst.HoTen, ColumnType.Text);
                col3.Width = 150;
                col3.BoundMode = ColumnBoundMode.Bound;
                col3.DataMember = GiaoDanConst.HoTen;
                col3.Caption = "Họ tên";
                col3.FilterEditType = FilterEditType.Combo;

                GridEXColumn col4 = this.RootTable.Columns.Add(GiaoDanConst.Phai, ColumnType.Text);
                col4.Width = 50;
                col4.BoundMode = ColumnBoundMode.Bound;
                col4.DataMember = GiaoDanConst.Phai;
                col4.Caption = "Phái";
                col4.FilterEditType = FilterEditType.DropDownList;

                GridEXColumn col5 = this.RootTable.Columns.Add(GiaoDanConst.NgaySinh, ColumnType.Text);
                col5.Width = 80;
                col5.BoundMode = ColumnBoundMode.Bound;
                col5.DataMember = GiaoDanConst.NgaySinh;
                col5.TextAlignment = TextAlignment.Far;
                col5.Caption = "Ngày sinh";
                col5.FilterEditType = FilterEditType.Combo;

                GridEXColumn col6 = this.RootTable.Columns.Add(GiaoDanConst.NgayRuaToi, ColumnType.Text);
                col6.Width = 80;
                col6.BoundMode = ColumnBoundMode.Bound;
                col6.DataMember = GiaoDanConst.NgayRuaToi;
                col6.TextAlignment = TextAlignment.Far;
                col6.Caption = "Ngày rửa tội";
                col6.FilterEditType = FilterEditType.Combo;

                GridEXColumn col7 = this.RootTable.Columns.Add(GiaoDanConst.NgayRuocLe, ColumnType.Text);
                col7.Width = 80;
                col7.BoundMode = ColumnBoundMode.Bound;
                col7.DataMember = GiaoDanConst.NgayRuocLe;
                col7.TextAlignment = TextAlignment.Far;
                col7.Caption = "Ngày XTRL";
                col7.FilterEditType = FilterEditType.Combo;

                GridEXColumn col8 = this.RootTable.Columns.Add(GiaoDanConst.NgayThemSuc, ColumnType.Text);
                col8.Width = 80;
                col8.BoundMode = ColumnBoundMode.Bound;
                col8.DataMember = GiaoDanConst.NgayThemSuc;
                col8.TextAlignment = TextAlignment.Far;
                col8.Caption = "Ngày Th.Sức";
                col8.FilterEditType = FilterEditType.Combo;

                GridEXColumn col9 = this.RootTable.Columns.Add(GiaoDanConst.DaCoGiaDinh, ColumnType.CheckBox);
                //col9.CheckBoxFalseValue = 0;
                //col9.CheckBoxTrueValue = -1;

                col9.Width = 50;
                col9.BoundMode = ColumnBoundMode.Bound;
                col9.DataMember = GiaoDanConst.DaCoGiaDinh;
                col9.Caption = "Lập GĐ";
                col9.FilterEditType = FilterEditType.CheckBox;



                GridEXColumn col13 = this.RootTable.Columns.Add(GiaoDanConst.HoTenCha, ColumnType.Text);
                col13.Width = 100;
                col13.BoundMode = ColumnBoundMode.Bound;
                col13.DataMember = GiaoDanConst.HoTenCha;
                col13.Caption = "Cha";
                col13.FilterEditType = FilterEditType.Combo;

                GridEXColumn col14 = this.RootTable.Columns.Add(GiaoDanConst.HoTenMe, ColumnType.Text);
                col14.Width = 100;
                col14.BoundMode = ColumnBoundMode.Bound;
                col14.DataMember = GiaoDanConst.HoTenMe;
                col14.Caption = "Mẹ";
                col14.FilterEditType = FilterEditType.Combo;

                GridEXColumn col15 = this.RootTable.Columns.Add(GiaoDanConst.TanTong, ColumnType.CheckBox);
                col15.Width = 50;
                col15.BoundMode = ColumnBoundMode.Bound;
                col15.DataMember = GiaoDanConst.TanTong;
                col15.Caption = "Tân tòng";
                col15.FilterEditType = FilterEditType.CheckBox;

                GridEXColumn col16 = this.RootTable.Columns.Add(GiaoDanConst.ConHoc, ColumnType.CheckBox);
                col16.Width = 50;
                col16.BoundMode = ColumnBoundMode.Bound;
                col16.DataMember = GiaoDanConst.ConHoc;
                col16.Caption = "Còn học";
                col16.FilterEditType = FilterEditType.CheckBox;

                GridEXColumn col18 = this.RootTable.Columns.Add(GiaoDanConst.NgheNghiep, ColumnType.Text);
                col18.Width = 100;
                col18.BoundMode = ColumnBoundMode.Bound;
                col18.DataMember = GiaoDanConst.NgheNghiep;
                col18.Caption = "Nghề nghiệp";
                col18.FilterEditType = FilterEditType.Combo;

                GridEXColumn col12 = this.RootTable.Columns.Add(GiaoDanConst.GhiChu, ColumnType.Text);
                col12.Width = 200;
                col12.BoundMode = ColumnBoundMode.Bound;
                col12.DataMember = GiaoDanConst.GhiChu;
                col12.Caption = "Ghi chú";
                col12.FilterEditType = FilterEditType.Combo;

                
                GridEXColumn col11 = this.RootTable.Columns.Add(GiaoDanConst.DienThoai, ColumnType.Text);
                col11.Width = 80;
                col11.BoundMode = ColumnBoundMode.Bound;
                col11.DataMember = GiaoDanConst.DienThoai;
                col11.Caption = "Điện thoại";
                col11.FilterEditType = FilterEditType.Combo;

                GridEXColumn colDiaChi = this.RootTable.Columns.Add(GiaoDanConst.DiaChi, ColumnType.Text);
                colDiaChi.Width = 200;
                colDiaChi.BoundMode = ColumnBoundMode.Bound;
                colDiaChi.DataMember = GiaoDanConst.DiaChi;
                colDiaChi.Caption = "Địa chỉ";
                colDiaChi.FilterEditType = FilterEditType.Combo;

                GridEXColumn col10 = this.RootTable.Columns.Add(GiaDinhConst.TenGiaoHo, ColumnType.Text);
                col10.Width = 80;
                col10.BoundMode = ColumnBoundMode.Bound;
                col10.DataMember = GiaDinhConst.TenGiaoHo;
                col10.Caption = "Giáo họ";
                col10.FilterEditType = FilterEditType.Combo;

                //Temporary column
                GridEXColumn colChuyenXu = this.RootTable.Columns.Add(GiaoDanConst.DaChuyenXu, ColumnType.CheckBox);
                colChuyenXu.Width = 50;
                colChuyenXu.BoundMode = ColumnBoundMode.Bound;
                colChuyenXu.DataMember = GiaoDanConst.DaChuyenXu;
                colChuyenXu.Caption = "Đã chuyển đi";
                colChuyenXu.CheckBoxTrueValue = -1;
                colChuyenXu.CheckBoxFalseValue = 0;
                colChuyenXu.FilterEditType = FilterEditType.CheckBox;

                GridEXColumn col17 = this.RootTable.Columns.Add(GiaoDanConst.TrinhDoVanHoa, ColumnType.Text);
                col17.Width = 100;
                col17.BoundMode = ColumnBoundMode.Bound;
                col17.DataMember = GiaoDanConst.TrinhDoVanHoa;
                col17.Caption = "Văn hóa";
                col17.FilterEditType = FilterEditType.Combo;

                //2018-07-17 Gia add start
                //reason:Thêm cột 
                GridEXColumn col20 = this.RootTable.Columns.Add(GiaoDanConst.TrinhDoChuyenMon, ColumnType.Text);
                col20.Width = 80;
                col20.BoundMode = ColumnBoundMode.Bound;
                col20.DataMember = GiaoDanConst.TrinhDoChuyenMon;
                col20.Caption = "Chuyên môn";
                col20.FilterEditType = FilterEditType.Combo;

                GridEXColumn col21 = this.RootTable.Columns.Add(GiaoDanConst.BietNgoaiNgu, ColumnType.Text);
                col21.Width = 50;
                col21.BoundMode = ColumnBoundMode.Bound;
                col21.DataMember = GiaoDanConst.BietNgoaiNgu;
                col21.Caption = "Ngoại ngữ";
                col21.FilterEditType = FilterEditType.Combo;
                //2018-07-17 Gia add end

                GridEXColumn colQuaDoi = this.RootTable.Columns.Add(GiaoDanConst.QuaDoi, ColumnType.CheckBox);
                colQuaDoi.Width = 50;
                colQuaDoi.BoundMode = ColumnBoundMode.Bound;
                colQuaDoi.DataMember = GiaoDanConst.QuaDoi;
                colQuaDoi.Caption = "Qua đời";
                colQuaDoi.FilterEditType = FilterEditType.CheckBox;
                colQuaDoi.Visible = true;

                GridEXColumn colNgayQuaDoi = this.RootTable.Columns.Add(GiaoDanConst.NgayQuaDoi, ColumnType.Text);
                colNgayQuaDoi.Width = 80;
                colNgayQuaDoi.BoundMode = ColumnBoundMode.Bound;
                colNgayQuaDoi.DataMember = GiaoDanConst.NgayQuaDoi;
                colNgayQuaDoi.TextAlignment = TextAlignment.Far;
                colNgayQuaDoi.Caption = "Ngày qua đời";
                colNgayQuaDoi.FilterEditType = FilterEditType.Combo;

                GridEXColumn colNoiAnTang = this.RootTable.Columns.Add(GiaoDanConst.NoiAnTang, ColumnType.Text);
                colNoiAnTang.Width = 80;
                colNoiAnTang.BoundMode = ColumnBoundMode.Bound;
                colNoiAnTang.DataMember = GiaoDanConst.NoiAnTang;
                colNoiAnTang.Caption = "Nơi an táng";

                AddColumn(GiaoDanConst.NoiSinh, ColumnType.Text, 60, ColumnBoundMode.Bound, "Nơi sinh", FilterEditType.Combo);
                AddColumn(GiaoDanConst.NoiRuaToi, ColumnType.Text, 60, ColumnBoundMode.Bound, "Nơi rửa tội", FilterEditType.Combo);
                AddColumn(GiaoDanConst.NoiRuocLe, ColumnType.Text, 60, ColumnBoundMode.Bound, "Nơi XTRL", FilterEditType.Combo);
                AddColumn(GiaoDanConst.NoiThemSuc, ColumnType.Text, 60, ColumnBoundMode.Bound, "Nơi thêm sức", FilterEditType.Combo);

                SetGridColumnWidth();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxGiaoDanList, FormatGrid)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xét trong danh sách gia đình của giáo xứ và cả trong thông tin cá nhân
        /// </summary>
        /// <param name="maGiaoDan"></param>
        /// <returns></returns>
        public static bool DaCoGiaDinh(int maGiaoDan)
        {
            //DataTable tblTV = Memory.GetTable(HonPhoiConst.TableName, string.Format(" AND MaGiaoDan={0}", maGiaoDan));
            DataTable tblTV = GetHonPhoi(maGiaoDan);
            if (!Memory.ShowError())
            {
                if (tblTV != null && tblTV.Rows.Count > 0)
                {
                    return true;
                }
                DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, string.Format(" AND MaGiaoDan={0}", maGiaoDan));
                if (!Memory.ShowError() && tblGiaoDan != null && tblGiaoDan.Rows.Count > 0 && (bool)tblGiaoDan.Rows[0][GiaoDanConst.DaCoGiaDinh])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Chi xét trong danh sách gia đình của giáo xứ
        /// </summary>
        /// <param name="maGiaoDan"></param>
        /// <returns></returns>
        public static bool DaCoThongTinHonPhoi(int maGiaoDan)
        {
            DataTable tblTV = GetHonPhoi2(maGiaoDan);
            if (!Memory.ShowError())
            {
                if (tblTV != null && tblTV.Rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 2018-07-18 Gia add start
        /// </summary>
        /// <param name="maGiaoDan"></param>
        /// <returns></returns>
        public static DataTable GetHonPhoi2(int maGiaoDan)
        {
            //select ma hon phoi
            DataTable tblCheck = Memory.GetData(SqlConstants.SELECT_MAHONPHOI_HOPLE, new object[] { maGiaoDan, maGiaoDan, 0 });
            if (!Memory.ShowError() && tblCheck != null && tblCheck.Rows.Count > 0)
            {
                int MaHonPhoi = (int)tblCheck.Rows[0][HonPhoiConst.MaHonPhoi];
                return GetData(MaHonPhoi);
            }
            //this.operation = GxOperation.ADD;
            return null;
        }
        public static DataTable GetHonPhoi(int maGiaoDan)
        {
            //select ma hon phoi
            DataTable tblCheck = Memory.GetData("SELECT MaHonPhoi FROM GiaoDanHonPhoi WHERE MaGiaoDan=?", new object[] { maGiaoDan });
            if (!Memory.ShowError() && tblCheck != null && tblCheck.Rows.Count > 0)
            {
                int MaHonPhoi = (int)tblCheck.Rows[0][HonPhoiConst.MaHonPhoi];
                return GetData(MaHonPhoi);
            }
            //this.operation = GxOperation.ADD;
            return null;
        }

        public static DataTable GetData(int maHonPhoi)
        {
            //select vo chong
            DataTable tblCheck = Memory.GetData(SqlConstants.SELECT_VOCHONG_THEO_HONPHOI, new object[] { maHonPhoi });
            if (!Memory.ShowError() && tblCheck != null && tblCheck.Rows.Count > 1)
            {
                //select thong tin hon phoi
                tblCheck = Memory.GetData(SqlConstants.SELECT_HONPHOI_THEO_ID, new object[] { maHonPhoi });
                if (!Memory.ShowError() && tblCheck != null && tblCheck.Rows.Count > 0)
                {
                    return tblCheck;
                }
            }
            //this.operation = GxOperation.ADD;
            return null;
        }
        public void ViewMapGiaoDan()
        {
            var giaoDanSelect = this.SelectedItems[0];
            var address = (giaoDanSelect.GetRow().DataRow as DataRowView).Row[GiaoDanConst.DiaChi];

            if (address != null && address.ToString() != "")
            {
                Memory.ViewMap(address.ToString());
            }
            else
            {
                MessageBox.Show("Giáo dân này không có địa chỉ để xem bản đồ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //hiepdv begin add
        public bool checkGiaoDanTrongGiaDinh(int maGiaoDan, string tenGiaoDan)
        {
            DataTable tblListGiaDinh = Memory.GetData(SqlConstants.SELECT_LIST_GIADINH_BY_MAGIAODAN, maGiaoDan.ToString());
            string message = String.Concat("Giáo dân ", tenGiaoDan, "\r\n");
            if (tblListGiaDinh != null && tblListGiaDinh.Rows.Count > 0)
            {
                for (int i = 0; i < tblListGiaDinh.Rows.Count; i++)
                {
                    int vaitro = Int32.Parse(tblListGiaDinh.Rows[i][ThanhVienGiaDinhConst.VaiTro].ToString());
                    string tenGiaDinh = tblListGiaDinh.Rows[i][GiaDinhConst.TenGiaDinh].ToString();
                    string maGiaDinh = tblListGiaDinh.Rows[i][GiaDinhConst.MaGiaDinhRieng].ToString().Trim() != "" ?
                        tblListGiaDinh.Rows[i][GiaDinhConst.MaGiaDinhRieng].ToString() :
                        tblListGiaDinh.Rows[i][GiaDinhConst.MaGiaDinh].ToString();

                    if (vaitro < 2)
                    {
                        message += String.Concat("Giữ vai trò là ", vaitro == 0 ? "người nam" : "người nữ",
                            " trong gia đình [", tenGiaDinh, "] có mã gia đình là [", maGiaDinh, "] \r\n");
                    }
                    else
                    {
                        message += String.Concat("Giữ vai trò là thành viên gia đình trong gia đình [",
                            tenGiaDinh, "] có mã gia đình là [", maGiaDinh, "] \r\n");
                    }
                }
                message += String.Concat("Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này");
                MessageBox.Show(message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        //hiepdv end add

    }
}
