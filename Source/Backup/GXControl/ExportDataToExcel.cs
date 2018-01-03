using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GxGlobal;
using System.IO;
using System.ComponentModel;

namespace GxControl
{
    public class ExportDataToExcel : IGxProcess
    {
        private DataProvider provider = null;
        private DataTable tblGiaoHo = null;
        private DataTable tblGiaDinh = null;
        private DataTable tblGiaoDan = null;
        private DataTable tblThanhVienGiaDinh = null;
        private int giaDinhHienTai = 0;

        private StreamWriter sw = null;
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;
        private ProcessOptions processOptions = ProcessOptions.ExportGiaoDanToExcel;
        public ProcessOptions ProcessOptions
        {
            get { return processOptions; }
            set { processOptions = value; }
        }

        private object processData = null;
        public object ProcessData
        {
            get { return processData; }
            set { processData = value; }
        }

        public void Execute()
        {
            if (OnStart != null) OnStart(this, EventArgs.Empty);
            try
            {
                switch (processOptions)
                {
                    case ProcessOptions.ExportGiaoDanToExcel:
                        exportGiaoDan();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                if (OnError != null) OnError(this, new CancelEventArgs());
            }
            finally
            {
                if (OnFinished != null) OnFinished(this, EventArgs.Empty);
            }
        }
        private void exportGiaoDan()
        {
            Memory.ClearError();

            provider = new DataProvider(Path.Combine(Memory.AppPath, Memory.DbName), Memory.DbUser, Memory.DbPassword);

            tblGiaoDan = provider.GetData("SELECT g.*, gh.TenGiaoHo FROM GiaoDan g LEFT JOIN GiaoHo gh ON g.MaGiaoHo=gh.MaGiaoHo ORDER BY g.MaGiaoHo ASC, g.MaGiaoDan ASC");
            for (int i = 0; i < tblGiaoDan.Columns.Count; i++)
            {
                DataColumn col = tblGiaoDan.Columns[i];
                if (col.DataType == typeof(bool))
                {
                    tblGiaoDan.Columns.Add(string.Concat("_", col.ColumnName));
                }
            }
            foreach (DataRow row in tblGiaoDan.Rows)
            {
                foreach (DataColumn col in tblGiaoDan.Columns)
                {
                    if (col.DataType == typeof(bool))
                    {
                        row[string.Concat("_", col.ColumnName)] = (bool)row[col] ? "X" : "";
                    }
                }
            }
            GxGrid grid = new GxGrid();
            grid.DataSource = tblGiaoDan;
            formatGrid(grid, tblGiaoDan);
            grid.Print();
        }

        private void formatGrid(GxGrid grid, DataTable tbl)
        {
            grid.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            grid.RootTable = new Janus.Windows.GridEX.GridEXTable();
            grid.RootTable.RowHeight = 20;
            grid.ColumnAutoResize = false;

            #region Define column

            Dictionary<string, string> dicColumnText;
            Dictionary<string, int> dicColumnWidth;
            GetColumnInfo(out dicColumnText, out dicColumnWidth);
            #endregion

            foreach (KeyValuePair<string, string> col in dicColumnText)
            {
                Janus.Windows.GridEX.GridEXColumn column = grid.AddColumn(col.Key, Janus.Windows.GridEX.ColumnType.Text, dicColumnWidth[col.Key], Janus.Windows.GridEX.ColumnBoundMode.Bound, col.Value, Janus.Windows.GridEX.FilterEditType.Combo);
            }
            
        }

        public static void GetColumnInfo(out Dictionary<string, string> dicColumnText, out Dictionary<string, int> dicColumnWidth)
        {
            dicColumnText = new Dictionary<string, string>();
            dicColumnText.Add("MaGiaoDan", "Mã GD");
            dicColumnText.Add("TenGiaoHo", "Giáo Họ");
            dicColumnText.Add("TenThanh", "Tên Thánh");
            dicColumnText.Add("HoTen", "Họ Tên");
            dicColumnText.Add("Phai", "Phái");
            dicColumnText.Add("NgaySinh", "Ngày Sinh");
            dicColumnText.Add("NoiSinh", "Nơi Sinh");
            dicColumnText.Add("HoTenCha", "Tên Cha");
            dicColumnText.Add("HoTenMe", "Tên Mẹ");
            dicColumnText.Add("SoRuaToi", "Số Rửa Tội");
            dicColumnText.Add("NgayRuaToi", "Ngày Rửa Tội");
            dicColumnText.Add("NoiRuaToi", "Nơi Rửa Tội");
            dicColumnText.Add("ChaRuaToi", "Người Rửa Tội");
            dicColumnText.Add("NguoiDoDauRuaToi", "Người Đỡ Đầu RT");
            dicColumnText.Add("SoRuocLe", "Số XTRL");
            dicColumnText.Add("NgayRuocLe", "Ngày XTRL");
            dicColumnText.Add("NoiRuocLe", "Nơi XTRL");
            dicColumnText.Add("ChaRuocLe", "Người ban XTRL");
            dicColumnText.Add("SoThemSuc", "Số Thêm Sức");
            dicColumnText.Add("NgayThemSuc", "Ngày Thêm Sức");
            dicColumnText.Add("NoiThemSuc", "Nơi Thêm Sức");
            dicColumnText.Add("ChaThemSuc", "Người ban Thêm Sức");
            dicColumnText.Add("NguoiDoDauThemSuc", "Người Đỡ Đầu Thêm Sức");
            dicColumnText.Add("TrinhDoVanHoa", "Trình Độ");
            dicColumnText.Add("NgheNghiep", "Nghề Nghiệp");
            dicColumnText.Add("_ConHoc", "Còn Học");
            dicColumnText.Add("_QuaDoi", "Qua Đời");
            dicColumnText.Add("NgayQuaDoi", "Ngày Qua Đời");
            dicColumnText.Add("DienThoai", "Điện Thoại");
            dicColumnText.Add("Email", "Email");
            dicColumnText.Add("DiaChi", "Địa Chỉ");
            dicColumnText.Add("GhiChu", "Ghi Chú");
            dicColumnText.Add("UpdateDate", "Ngày Cập Nhật");
            dicColumnText.Add("_DaCoGiaDinh", "Đã Lập Gia Đình");
            dicColumnText.Add("_GiaoDanAo", "Không Thống Kê");
            dicColumnText.Add("_TanTong", "Tân Tòng");
            dicColumnText.Add("MaNhanDang", "Mã Nhận Dạng");
            dicColumnText.Add("ThuocGiaoXu", "Thuộc Giáo Xứ");
            dicColumnText.Add("_DaXoa", "Đã Xóa");

            dicColumnWidth = new Dictionary<string, int>();
            dicColumnWidth.Add("MaGiaoDan", 80);
            dicColumnWidth.Add("TenGiaoHo", 100);
            dicColumnWidth.Add("TenThanh", 80);
            dicColumnWidth.Add("HoTen", 150);
            dicColumnWidth.Add("Phai", 50);
            dicColumnWidth.Add("NgaySinh", 80);
            dicColumnWidth.Add("NoiSinh", 100);
            dicColumnWidth.Add("HoTenCha", 150);
            dicColumnWidth.Add("HoTenMe", 150);
            dicColumnWidth.Add("SoRuaToi", 60);
            dicColumnWidth.Add("NgayRuaToi", 80);
            dicColumnWidth.Add("NoiRuaToi", 100);
            dicColumnWidth.Add("ChaRuaToi", 150);
            dicColumnWidth.Add("NguoiDoDauRuaToi", 150);
            dicColumnWidth.Add("SoRuocLe", 60);
            dicColumnWidth.Add("NgayRuocLe", 80);
            dicColumnWidth.Add("NoiRuocLe", 100);
            dicColumnWidth.Add("ChaRuocLe", 150);
            dicColumnWidth.Add("SoThemSuc", 60);
            dicColumnWidth.Add("NgayThemSuc", 80);
            dicColumnWidth.Add("NoiThemSuc", 100);
            dicColumnWidth.Add("ChaThemSuc", 150);
            dicColumnWidth.Add("NguoiDoDauThemSuc", 150);
            dicColumnWidth.Add("TrinhDoVanHoa", 100);
            dicColumnWidth.Add("NgheNghiep", 100);
            dicColumnWidth.Add("_ConHoc", 60);
            dicColumnWidth.Add("_QuaDoi", 60);
            dicColumnWidth.Add("NgayQuaDoi", 80);
            dicColumnWidth.Add("DienThoai", 80);
            dicColumnWidth.Add("Email", 100);
            dicColumnWidth.Add("DiaChi", 100);
            dicColumnWidth.Add("GhiChu", 150);
            dicColumnWidth.Add("UpdateDate", 50);
            dicColumnWidth.Add("_DaCoGiaDinh", 80);
            dicColumnWidth.Add("_GiaoDanAo", 80);
            dicColumnWidth.Add("_TanTong", 80);
            dicColumnWidth.Add("MaNhanDang", 50);
            dicColumnWidth.Add("ThuocGiaoXu", 100);
            dicColumnWidth.Add("_DaXoa", 50);
        }

        private void exportGiaoDan1()
        {
            Memory.ClearError();

            provider = new DataProvider(Path.Combine(Memory.AppPath, Memory.DbName), Memory.DbUser, Memory.DbPassword);

            tblGiaoDan = provider.GetData("SELECT * FROM GiaoDan");
            if (Memory.HasError())
            {
                return;
            }
            int count = tblGiaoDan.Rows.Count;
            Memory.Instance.SetMemory("TongDuLieu", count);

            string fileName = Memory.GetTempPath("GiaoDan.xls");
            ExcelEngine excel = new ExcelEngine();
            excel.CreateNewObject(fileName);

            for (int i = 0; i < tblGiaoDan.Columns.Count; i++)
            {
                excel.Write_to_excel(1, i + 1, tblGiaoDan.Columns[i].Caption);
            }
            int rowIndex = 2;
            int currentProgress = 0;
            foreach (DataRow row in tblGiaoDan.Rows)
            {
                currentProgress++;
                Memory.Instance.SetMemory("DuLieuHienTai", currentProgress);

                for (int i = 0; i < tblGiaoDan.Columns.Count; i++)
                {
                    excel.Write_to_excel(rowIndex, i + 1, row[i]);
                }
                rowIndex++;
                if (OnExecuting != null) OnExecuting(string.Concat(((int)(currentProgress * 100 / count)).ToString(), "%"), EventArgs.Empty);
            }
            excel.End_Write();
            System.Diagnostics.Process.Start(fileName);
        }
    }
}
