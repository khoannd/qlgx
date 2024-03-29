using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GxGlobal;
using System.IO;
using System.ComponentModel;

namespace GxControl
{
    public class ImportData: IGxProcess
    {
        private DataProvider provider = null;
        private DataTable tblGiaoHo = null;
        private DataTable tblGiaDinh = null;
        private DataTable tblGiaoDan = null;
        private DataTable tblThanhVienGiaDinh = null;
        private DataTable tblHonPhoi = null;
        private DataTable tblGiaoDanHonPhoi = null;
        private int giaDinhHienTai = 0;

        private StreamWriter sw = null;
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;
        private ProcessOptions processOptions = ProcessOptions.ImportData;
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

        private string filePathExcel = "";
        private string sheetNameExcel = "";

        ConvertDB.vnConvert.ConvertFont convert = new ConvertDB.vnConvert.ConvertFont();
        public ImportData(string dbName, string user, string pass)
        {
            provider = new DataProvider(dbName, user, pass);
        }

        public ImportData(string filePath, string sheetName)
        {
            filePathExcel = filePath;
            sheetNameExcel = sheetName;
        }

        public void Execute()
        {
            if (processOptions == GxGlobal.ProcessOptions.ImportData)
            {
                if (!string.IsNullOrEmpty(sheetNameExcel))
                {
                    importExcelFile();
                }
                else
                {
                    importMDBFile();
                }
            }
            else if(processOptions == GxGlobal.ProcessOptions.ImportHocVien)
            {
                ImportGiaoLy();
            }
        }

        #region Import Lop Giao Ly
        private void ImportGiaoLy()
        {
            try
            {
                if (!File.Exists(filePathExcel))
                {
                    if (OnError != null) OnError("Tập tin Excel không tồn tại", new CancelEventArgs());
                    return;
                }
                DataSet ds = ExcelDataProvider.GetDataSet(filePathExcel, sheetNameExcel);
                if (Memory.HasError())
                {
                    if (OnError != null) OnError("Không thể lấy dữ liệu từ tập tin được chọn. Tập tin này phải là tập tin được xuất ra từ chương trình, không được chỉnh sửa dòng đầu tiên và tên sheet.\r\nXin vui lòng kiểm tra lại hoặc liên hệ tác giả để biết thêm thông tin chi tiết.", new CancelEventArgs());
                    return;
                }
                Dictionary<string, string> dicColumnText;
                Dictionary<string, int> dicColumnWidth;
                ExportDataToExcel.GetColumnInfo(out dicColumnText, out dicColumnWidth);

                string sql = string.Format("SELECT a.*, b.TenThanh, b.HoTen, b.Phai, b.NgaySinh FROM ChiTietLopGiaoLy a  INNER JOIN GiaoDan b ON a.MaGiaoDan = b.MaGiaoDan WHERE (a.Malop = ?) AND (b.DaXoa=0)");
                tblGiaoDan = Memory.GetData(sql, new object[] { processData });

                if (tblGiaoDan == null)
                {
                    if (Memory.Instance.Error != null)
                    {
                        if (OnError != null) OnError(Memory.Instance.Error.Message, new CancelEventArgs());
                    }
                    if (OnFinished != null) OnFinished(tblGiaoDan, EventArgs.Empty);
                    return;
                }
                tblGiaoDan.TableName = "ChiTietGiaoLy";

                object oID = tblGiaoDan.Compute("MAX(SoThuTu)", string.Empty);
                int maxID = (oID != DBNull.Value ? Convert.ToInt32(oID) : 0);
                maxID++;

                int duLieuHienTai = 0;
                Memory.Instance.SetMemory("TongDuLieu", ds.Tables[0].Rows.Count);
                if (OnStart != null) OnStart("started", EventArgs.Empty);
                int importCount = 0;
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    duLieuHienTai++;
                    Memory.Instance.SetMemory("DuLieuHienTai", duLieuHienTai);
                    //check user abort
                    bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                    //if user abort progress, return
                    if (userStop) return;

                    if (Memory.IsNullOrEmpty(r["Họ tên"]) || Memory.IsNullOrEmpty(r["Phái"]) || Memory.IsNullOrEmpty(r["Ngày sinh"]))
                    {
                        if (OnError != null) OnError(string.Format("Không nhập được học viên dòng số {0} vì không được nhập đủ thông tin Họ tên, Phái, Ngày sinh", duLieuHienTai + 1), new CancelEventArgs());
                        continue;
                    }
                    int maGiaoDan = 0;
                    if (Memory.IsNullOrEmpty(r["Mã GD"]))
                    {
                        //Find giao dan
                        sql = string.Format("SELECT MaGiaoDan FROM GiaoDan WHERE HoTen LIKE '{0}'", r["Họ tên"].ToString().Trim().Replace("'", "''"));
                        if (!Memory.IsNullOrEmpty(r["Tên thánh"]))
                        {
                            sql = string.Format("{0} AND TenThanh LIKE '{1}'", sql, r["Tên thánh"].ToString().Trim().Replace("'", "''"));
                        }
                        if (!Memory.IsNullOrEmpty(r["Phái"]))
                        {
                            sql = string.Format("{0} AND Phai LIKE '{1}'", sql, r["Phái"].ToString().Trim().Replace("'", "''"));
                        }
                        if (!Memory.IsNullOrEmpty(r["Ngày sinh"]))
                        {
                            sql = string.Format("{0} AND NgaySinh LIKE '{1}'", sql, r["Ngày sinh"].ToString().Trim().Replace("-", "/").Replace(" ", "").Replace("'", "''"));
                        }
                        DataTable tblTmp = Memory.GetData(sql);
                        if (tblTmp != null && tblTmp.Rows.Count > 0)
                        {
                            if (tblTmp.Rows.Count > 1)
                            {
                                if (OnError != null) OnError(string.Format("Có {0} người [{0}] trùng thông tin. Hãy kiểm tra lại.", tblTmp.Rows.Count, r["Họ tên"]), new CancelEventArgs());
                            }
                            maGiaoDan = Memory.GetInt(tblTmp.Rows[0]["MaGiaoDan"]);
                        }
                        else if (Memory.Instance.Error != null)
                        {
                            if (OnError != null) OnError(Memory.Instance.Error.Message, new CancelEventArgs());
                        }
                    }
                    else
                    {
                        maGiaoDan = Memory.GetInt(r["Mã GD"]);
                    }
                    if (maGiaoDan > 0)
                    {
                        DataRow[] rows = tblGiaoDan.Select(string.Format("MaGiaoDan={0}", maGiaoDan));
                        if (rows.Length > 0)
                        {
                            if (OnError != null) OnError(string.Format("Không nhập [{0}] vì đã tồn tại trong danh sách lớp", r["Họ tên"]), new CancelEventArgs());
                            continue;
                        }
                    }
                    else
                    {
                        int maGiaoHo = 0;
                        if (!Memory.IsNullOrEmpty(r["Giáo họ"]))
                        {
                            maGiaoHo = getMaGiaoHo(r["Giáo họ"].ToString(), true);
                        }
                        if (maGiaoHo == 0)
                        {
                            if (OnError != null) OnError(string.Format("Không tìm thấy giáo họ của [{0}]", r["Họ tên"]), new CancelEventArgs());
                        }
                        sql = string.Format("INSERT INTO GiaoDan(MaGiaoDan, TenThanh, HoTen, Phai, NgaySinh, MaGiaoHo, DaCoGiaDinh) VALUES(?, ?, ?, ?, ?, ?, 0)");
                        maGiaoDan = Memory.Instance.GetNextId("GiaoDan", "MaGiaoDan", false);
                        Memory.ExecuteSqlCommand(sql, maGiaoDan, r["Tên thánh"], r["Họ tên"], r["Phái"], r["Ngày sinh"], maGiaoHo);
                        if (Memory.Instance.Error != null)
                        {
                            if (OnError != null) OnError(Memory.Instance.Error.Message, new CancelEventArgs());
                        }
                        else
                        {
                            if (OnExecuting != null) OnExecuting(string.Format("Đã thêm mới [{0}] vì thông tin giáo dân này chưa có trong chương trình", r["Họ tên"]), new CancelEventArgs());
                        }
                    }
                    importCount++;
                    DataRow row = tblGiaoDan.NewRow();
                    tblGiaoDan.Rows.Add(row);
                    if (processData != null) row["MaLop"] = processData;
                    row["SoThuTu"] = maxID++;
                    row["MaGiaoDan"] = maGiaoDan;
                    
                    row["TenThanh"] = r["Tên thánh"];
                    row["HoTen"] = r["Họ tên"];
                    row["Phai"] = r["Phái"];
                    row["NgaySinh"] = r["Ngày sinh"];
                    row["GhiChuGLy"] = r["Ghi chú"];
                    row["HoanThanh"] = !Memory.IsNullOrEmpty(r["Đã học xong"]) ? -1 : 0;
                    
                }
                if (OnExecuting != null) OnExecuting("Có tất cả " + importCount.ToString() + " học viên được nhập", EventArgs.Empty);
                if (OnFinished != null) OnFinished(tblGiaoDan, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (OnError != null) OnError("Có lỗi không mong muốn xảy ra khi xem xét thông tin giáo dân. Thông tin lỗi: " + ex.Message, new CancelEventArgs());
            }
            finally
            {
                Memory.ClearError();
                filePathExcel = "";
                sheetNameExcel = "";
            }
        }
        #endregion

        #region Import Excel File
        private void importExcelFile()
        {
            try
            {
                if (!File.Exists(filePathExcel))
                {
                    if (OnError != null) OnError("Tập tin Excel không tồn tại", new CancelEventArgs());
                    return;
                }
                DataSet ds = ExcelDataProvider.GetDataSet(filePathExcel, sheetNameExcel);
                if (Memory.HasError())
                {
                    if (OnError != null) OnError("Không thể lấy dữ liệu từ tập tin được chọn. Tập tin này phải là tập tin được xuất ra từ chương trình, không được chỉnh sửa dòng đầu tiên và tên sheet.\r\nXin vui lòng kiểm tra lại hoặc liên hệ tác giả để biết thêm thông tin chi tiết. \r\nThông tin chi tiết: " + Memory.Instance.Error?.Message, new CancelEventArgs());
                    return;
                }

                bool hasGiaDinh = false;
                Dictionary<string, string> dicColumnText;
                Dictionary<string, int> dicColumnWidth;
                ExportDataToExcel.GetColumnInfo(out dicColumnText, out dicColumnWidth);

                if (ds.Tables[0].Columns.Contains(dicColumnText[GiaDinhConst.MaGiaDinhRieng]))
                {
                    hasGiaDinh = true;
                }
                
                tblGiaoDan = Memory.GetData("SELECT * FROM GiaoDan");
                if (hasGiaDinh)
                {
                    tblThanhVienGiaDinh = Memory.GetData("SELECT * FROM ThanhVienGiaDinh ");
                    tblGiaoDan.Columns.Add("QuanHe", typeof(string));
                    tblGiaoDan.Columns.Add("SoHonPhoi", typeof(string));
                    tblGiaoDan.Columns.Add("NgayHonPhoi", typeof(string));
                    tblGiaoDan.Columns.Add("NoiHonPhoi", typeof(string));
                    tblGiaoDan.Columns.Add("LinhMucChung", typeof(string));
                    tblGiaoDan.Columns.Add("NguoiChung1", typeof(string));
                    tblGiaoDan.Columns.Add("NguoiChung2", typeof(string));
                    tblGiaoDan.Columns.Add("CachThucHonPhoi", typeof(string));
                    tblGiaDinh = Memory.GetData("SELECT * FROM GiaDinh");
                    tblHonPhoi = Memory.GetData("SELECT * FROM HonPhoi");
                    tblGiaoDanHonPhoi = Memory.GetData("SELECT * FROM GiaoDanHonPhoi");
                }

                object oID = tblGiaoDan.Compute("MAX(MaGiaoDan)", string.Empty);
                int maxID = (oID != DBNull.Value ? Convert.ToInt32(oID) : 0);

                oID = ds.Tables[0].Compute("MAX([Mã GD])", string.Empty);
                int maxExcelID = (oID != DBNull.Value ? Convert.ToInt32(oID) : 0);

                maxID = Math.Max(maxID, maxExcelID);

                int duLieuHienTai = 0;
                Memory.Instance.SetMemory("TongDuLieu", ds.Tables[0].Rows.Count);
                if (OnStart != null) OnStart("started", EventArgs.Empty);
                DataTable tblTmp = tblGiaoDan.Clone();
                int lastGiaoHo = 0;
                string preMaGiaDinh = "";

                if (hasGiaDinh)
                {
                    createMaGiaDinh(ds.Tables[0], dicColumnText);
                }
                
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    bool isInvalidDate = false;
                    duLieuHienTai++;
                    Memory.Instance.SetMemory("DuLieuHienTai", duLieuHienTai);
                    if (OnExecuting != null) OnExecuting(string.Concat("Đang nhập giáo dân ", r["Họ Tên"]), EventArgs.Empty);
                    //check user abort
                    bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                    //if user abort progress, return
                    if (userStop) return;

                    string maGiaDinhRieng = "";
                    bool isExist = false;
                    string hoTen = r[dicColumnText[GiaoDanConst.HoTen]].ToString().Trim();
                    if (hoTen == "")
                    {
                        continue;
                    }
                    //format date
                    List<string> lstError = new List<string>(); // 2019-07-27 Khoan add
                    foreach (var col in r.Table.Columns)
                    {

                        var column = (col as DataColumn);
                        if (column.ColumnName.Contains("Ngày") && column.ColumnName != "Ngày Cập Nhật")
                        {
                            var rs = Memory.ReplaceDate(r[column.ColumnName].ToString());
                           // 2019-07-27 Khoan mod start
                            //if (rs != "-1" && rs != "")
                            if (rs != "-1")
                            // 2019-07-27 Khoan mod end
                            {
                                r[column.ColumnName] = rs;
                            }
                            else
                            {
                                 lstError.Add(string.Format("{0}={1}", column.ColumnName, r[column.ColumnName])); // 2019-07-27 Khoan add
                                isInvalidDate = true;
                                continue;
                            }
                        }
                    }
                    if (isInvalidDate)
                    {
                        // 2019-07-27 Khoan mod start
                        //if (OnError != null) OnError(string.Format("{0} có ngày tháng không hợp lệ", hoTen), new CancelEventArgs());
                        if (OnError != null) OnError(string.Format("{0} có ngày tháng không hợp lệ: {1}", hoTen, string.Join(", ", lstError.ToArray())), new CancelEventArgs());
                        // 2019-07-27 Khoan mod end
                        continue;
                    }
                    if (hasGiaDinh)
                    {
                        maGiaDinhRieng = r[dicColumnText[GiaDinhConst.MaGiaDinhRieng]].ToString().Trim();
                        if (preMaGiaDinh == "")
                        {
                            preMaGiaDinh = maGiaDinhRieng;
                        }

                        if (preMaGiaDinh != maGiaDinhRieng)
                        {
                            if (tblTmp.Rows.Count > 0)
                            {
                                importGiaDinhExcel(lastGiaoHo, tblTmp, preMaGiaDinh);
                                tblTmp.Clear();
                            }
                            preMaGiaDinh = maGiaDinhRieng;
                        }
                    }

                    if (r[dicColumnText[GiaoDanConst.Phai]].ToString().Trim() == "")
                    {
                        if (OnError != null) OnError(string.Format("{0} không có thông tin [Phái]", hoTen), new CancelEventArgs());
                        continue;
                    }
                    if (hasGiaDinh && maGiaDinhRieng != "" && r[dicColumnText["QuanHe"]].ToString().Trim() == "")
                    {
                        if (OnError != null) OnError(string.Format("{0} không có thông tin [Quan hệ]", hoTen), new CancelEventArgs());
                        continue;
                    }

                    DataRow row = tblGiaoDan.NewRow();
                    foreach (KeyValuePair<string, string> col in dicColumnText)
                    {
                        string colName = col.Key.Trim('_');
                        if (!ds.Tables[0].Columns.Contains(col.Value)) continue;
                        
                        if (colName == GiaoDanConst.MaGiaoDan)
                        {
                            if (r[col.Value].ToString().Trim() != "")
                            {
                                DataRow[] rows = tblGiaoDan.Select(string.Format("MaGiaoDan={0}", r[col.Value]));
                                if (rows.Length > 0)
                                {
                                    row = rows[0];
                                    isExist = true;
                                }
                                else
                                {
                                    row[GiaoDanConst.MaGiaoDan] = r[col.Value];
                                }
                            }
                            else
                            {
                                maxID++;
                                row[GiaoDanConst.MaGiaoDan] = maxID;
                            }
                        }
                        else if (colName == GiaoHoConst.TenGiaoHo)
                        {
                            lastGiaoHo = getMaGiaoHo(r[col.Value].ToString());
                            row["MaGiaoHo"] = lastGiaoHo;
                        }
                        else if (col.Key.StartsWith("_"))
                        {
                            if (!string.IsNullOrEmpty(r[col.Value].ToString().Trim()))
                            {
                                row[colName] = true;
                            }
                            else
                            {
                                row[colName] = false;
                            }
                        }
                        else if (colName.ToLower().StartsWith("ngay") && (r[col.Value] is DateTime))
                        {
                            row[colName] = ((DateTime)r[col.Value]).ToString("dd/MM/yyyy");
                        }
                        else if (colName == GiaoDanConst.UpdateDate)
                        {
                            if (r[col.Value].ToString() == "")
                            {
                                row[colName] = DateTime.Now;
                            }
                        }
                        //27/11/2019 hiepdv begin add
                        else if (colName == GiaoDanConst.MaNhanDang)
                        {
                            if (r[col.Value].ToString().Trim()!="")
                            {
                                row[colName] = r[col.Value];
                            }
                            else if (!isExist)
                            {
                                row[GiaoDanConst.MaNhanDang] = Memory.GetGiaoDanKey(row[GiaoDanConst.MaGiaoDan]);
                            }
                        }
                        //27/11/2019 hiepdv end add
                        else if (tblGiaoDan.Columns.Contains(colName))
                        {
                            row[colName] = r[col.Value];
                            //ConvertDB.vnConvert.FontIndex vnType = ConvertDB.vnConvert.FontIndex.iUNI;
                            //if (convert.isVietnamese(row[colName].ToString(), ref vnType) && vnType == ConvertDB.vnConvert.FontIndex.iVNI)
                            //{
                            //    row[colName] = Memory.ConvertVNI2UNI(row[colName].ToString());
                            //}
                        }
                    }
                    if (!isExist)
                    {
                        tblGiaoDan.Rows.Add(row);
                    }
                    if (maGiaDinhRieng != "")
                    {
                        tblTmp.ImportRow(row);
                    }
                }
                if (hasGiaDinh && tblTmp.Rows.Count > 0)
                {
                    importGiaDinhExcel(lastGiaoHo, tblTmp, preMaGiaDinh);
                    tblTmp.Clear();
                }

                ds = new DataSet();
                tblGiaoDan.TableName = GiaoDanConst.TableName;
                // Chuan hoa data
                for (int i = tblGiaoDan.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = tblGiaoDan.Rows[i];

                    if (row.RowState == DataRowState.Unchanged)
                    {
                        tblGiaoDan.Rows.Remove(row);
                    }
                }
                Memory.AutoUpperCaseFirstCharGiaoDan(tblGiaoDan);

                if (OnExecuting != null) OnExecuting("Đang lưu thông tin giáo dân vào chương trình...", EventArgs.Empty);

                if (hasGiaDinh)
                {
                    tblGiaDinh.TableName = GiaDinhConst.TableName;
                    tblThanhVienGiaDinh.TableName = ThanhVienGiaDinhConst.TableName;
                    ds.Tables.Add(tblGiaDinh);
                    ds.Tables.Add(tblThanhVienGiaDinh);
                    if (tblHonPhoi.GetChanges() != null)
                    {
                        tblHonPhoi.TableName = HonPhoiConst.TableName;
                        ds.Tables.Add(tblHonPhoi);
                    }
                    if (tblGiaoDanHonPhoi.GetChanges() != null)
                    {
                        tblGiaoDanHonPhoi.TableName = GiaoDanHonPhoiConst.TableName;
                        ds.Tables.Add(tblGiaoDanHonPhoi);
                    }
                }
                
                ds.Tables.Add(tblGiaoDan);
                Memory.UpdateDataSet(ds);
                if (Memory.HasError())
                {
                    if (OnError != null) OnError("Có lỗi khi cập nhật thông tin giáo dân. Thông tin lỗi: " + Memory.Instance.Error.Message, new CancelEventArgs());
                    return;
                }
                if (OnFinished != null) OnFinished("finished", EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (OnError != null) OnError("Có lỗi không mong muốn xảy ra khi xem xét thông tin giáo dân. Thông tin lỗi: " + ex.Message, new CancelEventArgs());
            }
            finally
            {
                Memory.ClearError();
                filePathExcel = "";
                sheetNameExcel = "";
            }
        }
        private void importGiaDinhExcel(int maGiaoho, DataTable tblGiaoDanTmp, string maGiaDinhRieng)
        {

            Dictionary<string, DataRow> dicVoChong = new Dictionary<string, DataRow>();
            Dictionary<int, string> dicQuanHe = Memory.GetQuanHeList(true);
            foreach (DataRow row in tblGiaoDanTmp.Rows)
            {
                string quanHe = row["QuanHe"].ToString().Trim().ToLower();
                if (quanHe.Contains("chủ") || quanHe.Contains("chồng") || quanHe.Contains("vợ"))
                {
                    if (quanHe.Contains("chủ"))
                    {
                        if (dicVoChong.ContainsKey("chủ"))
                        {
                            if (dicVoChong["chủ"]["Phai"].ToString().ToLower() == "nữ" && row["Phai"].ToString().ToLower() == "nam")
                            {
                                dicVoChong["chủ"] = row;
                            }
                        }
                        else
                        {
                            dicVoChong.Add("chủ", row);
                        }
                    }
                    else
                    {
                        if(!dicVoChong.ContainsKey(quanHe))
                        {
                            dicVoChong.Add(quanHe, row);
                        }
                    }
                        
                }
            }

            if (dicVoChong.Count > 0)
            {
                string tenGiaDinh = "";
                string tenChong = "", tenChongDayDu = "";
                string tenVo = "", tenVoDayDu = "";
                int nguoiChong = 0;
                int nguoiVo = 0;
                int chuHo = 0;
                if (dicVoChong.Count == 1)
                {
                    foreach (KeyValuePair<string, DataRow> item in dicVoChong)
                    {
                        if (item.Value[GiaoDanConst.Phai].ToString().ToLower() == "nam")
                        {
                            tenChong = Memory.GetName(item.Value[GiaoDanConst.HoTen].ToString()).Trim();
                            tenChongDayDu = Memory.GetTenThanhHoTen(item.Value[GiaoDanConst.TenThanh].ToString().Trim(), item.Value[GiaoDanConst.HoTen].ToString().Trim());
                            nguoiChong = Convert.ToInt32(item.Value[GiaoDanConst.MaGiaoDan]);
                            chuHo = nguoiChong;
                        }
                        else
                        {
                            tenVo = Memory.GetName(item.Value[GiaoDanConst.HoTen].ToString()).Trim();
                            tenVoDayDu = Memory.GetTenThanhHoTen(item.Value[GiaoDanConst.TenThanh].ToString().Trim(), item.Value[GiaoDanConst.HoTen].ToString().Trim());
                            nguoiVo = Convert.ToInt32(item.Value[GiaoDanConst.MaGiaoDan]);
                            chuHo = nguoiVo;
                        }
                    }
                }
                else
                {
                    if (dicVoChong.ContainsKey("vợ"))
                    {
                        tenVo = Memory.GetName(dicVoChong["vợ"][GiaoDanConst.HoTen].ToString()).Trim();
                        tenVoDayDu = Memory.GetTenThanhHoTen(dicVoChong["vợ"][GiaoDanConst.TenThanh].ToString().Trim(), dicVoChong["vợ"][GiaoDanConst.HoTen].ToString().Trim());
                        nguoiVo = Convert.ToInt32(dicVoChong["vợ"][GiaoDanConst.MaGiaoDan]);
                    }
                    if (dicVoChong.ContainsKey("chồng"))
                    {
                        tenChong = Memory.GetName(dicVoChong["chồng"][GiaoDanConst.HoTen].ToString()).Trim();
                        tenChongDayDu = Memory.GetTenThanhHoTen(dicVoChong["chồng"][GiaoDanConst.TenThanh].ToString().Trim(), dicVoChong["chồng"][GiaoDanConst.HoTen].ToString().Trim());
                        nguoiChong = Convert.ToInt32(dicVoChong["chồng"][GiaoDanConst.MaGiaoDan]);
                    }
                    if (dicVoChong.ContainsKey("chủ"))
                    {
                        //Neu da tim thay nguoi vo thi nguoi con lai se la nguoi chong
                        if (nguoiVo > 0) //da tim thay nguoi vo
                        {
                            tenChong = Memory.GetName(dicVoChong["chủ"][GiaoDanConst.HoTen].ToString()).Trim();
                            tenChongDayDu = Memory.GetTenThanhHoTen(dicVoChong["chủ"][GiaoDanConst.TenThanh].ToString().Trim(), dicVoChong["chủ"][GiaoDanConst.HoTen].ToString().Trim());
                            nguoiChong = Convert.ToInt32(dicVoChong["chủ"][GiaoDanConst.MaGiaoDan]);
                            chuHo = nguoiChong;
                        }
                        else //chua thay nguoi vo
                        {
                            tenVo = Memory.GetName(dicVoChong["chủ"][GiaoDanConst.HoTen].ToString()).Trim();
                            tenVoDayDu = Memory.GetTenThanhHoTen(dicVoChong["chủ"][GiaoDanConst.TenThanh].ToString().Trim(), dicVoChong["chủ"][GiaoDanConst.HoTen].ToString().Trim());
                            nguoiVo = Convert.ToInt32(dicVoChong["chủ"][GiaoDanConst.MaGiaoDan]);
                            chuHo = nguoiVo;
                        }
                    }
                }
                tenGiaDinh = tenChong;
                if (tenGiaDinh != "" && tenVo != "") tenGiaDinh += " - " + tenVo;
                else if (tenVo != "") tenGiaDinh = tenVo;

                DataRow rowGiaDinh = null;
                DataRow[] rows = tblGiaDinh.Select(string.Format("MaGiaDinhRieng='{0}'", maGiaDinhRieng));
                if (rows.Length == 0)
                {
                    //Them Gia Dinh
                    rowGiaDinh = tblGiaDinh.NewRow();
                    tblGiaDinh.Rows.Add(rowGiaDinh);
                    rowGiaDinh[GiaDinhConst.MaGiaDinh] = Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, false);
                    rowGiaDinh[GiaDinhConst.MaNhanDang] = Memory.GetGiaDinhKey(rowGiaDinh[GiaDinhConst.MaGiaDinh]);
                }
                else
                {
                    rowGiaDinh = rows[0];
                }
                rowGiaDinh[GiaDinhConst.MaGiaoHo] = maGiaoho;
                rowGiaDinh[GiaDinhConst.TenGiaDinh] = tenGiaDinh;
                rowGiaDinh[GiaDinhConst.UpdateDate] = Memory.Instance.GetServerDateTime();
                
                if (maGiaDinhRieng != "")
                {
                    rowGiaDinh[GiaDinhConst.MaGiaDinhRieng] = maGiaDinhRieng;
                }
                //Them Thanh Vien Gia Dinh
                foreach (DataRow row in tblGiaoDanTmp.Rows)
                {
                    int maGiaoDan = Convert.ToInt32(row[GiaoDanConst.MaGiaoDan]);
                    string quanHe = row["QuanHe"].ToString().Trim().ToLower();

                    DataRow rowTV = null;
                    DataRow[] rowsTV = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND MaGiaoDan={1}", rowGiaDinh[GiaDinhConst.MaGiaDinh], maGiaoDan));
                    if (rowsTV.Length == 0)
                    {
                        rowTV = tblThanhVienGiaDinh.NewRow();
                        tblThanhVienGiaDinh.Rows.Add(rowTV);
                    }
                    else
                    {
                        rowTV = rowsTV[0];
                    }

                    int vaiTro = 100;

                    if (maGiaoDan == nguoiVo)
                    {
                        vaiTro = 1;
                    }
                    else if (maGiaoDan == nguoiChong)
                    {
                        vaiTro = 0;
                    }
                    else
                    {
                        foreach (KeyValuePair<int, string> item in dicQuanHe)
                        {
                            if (quanHe.Contains(item.Value.ToLower()))
                            {
                                vaiTro = item.Key;
                                break;
                            }
                        }
                    }
                    rowTV[ThanhVienGiaDinhConst.MaGiaDinh] = rowGiaDinh[GiaDinhConst.MaGiaDinh];
                    rowTV[ThanhVienGiaDinhConst.MaGiaoDan] = maGiaoDan;
                    rowTV[ThanhVienGiaDinhConst.VaiTro] = vaiTro;
                    if (maGiaoDan == chuHo) rowTV[ThanhVienGiaDinhConst.ChuHo] = true;
                    else rowTV[ThanhVienGiaDinhConst.ChuHo] = false;
                    //Neu gia dinh co ca vo chong va nguoi hien tai la vo hoac chong thi tu chon cho nguoi do da lap gia dinh
                    if ((maGiaoDan == nguoiChong || maGiaoDan == nguoiVo) && (nguoiChong > 0 && nguoiVo > 0))
                    {
                        DataRow[] rowTmps = tblGiaoDan.Select("MaGiaoDan=" + maGiaoDan);
                        if (rowTmps.Length > 0) rowTmps[0][GiaoDanConst.DaCoGiaDinh] = true;
                    }

                    //cap nhat ten cha me
                    if (quanHe == "con")
                    {
                        DataRow[] rowTmps = tblGiaoDan.Select("MaGiaoDan=" + maGiaoDan);
                        if (rowTmps.Length > 0)
                        {
                            if (Memory.IsNullOrEmpty(rowTmps[0][GiaoDanConst.HoTenCha])) rowTmps[0][GiaoDanConst.HoTenCha] = tenChongDayDu;
                            if (Memory.IsNullOrEmpty(rowTmps[0][GiaoDanConst.HoTenMe])) rowTmps[0][GiaoDanConst.HoTenMe] = tenVoDayDu;
                        }
                    }
                }
                Dictionary<int, DataRow> dicHonPhoi = new Dictionary<int, DataRow>();
                foreach (var item in dicVoChong)
                {
                    dicHonPhoi.Add(Memory.GetInt(item.Value[GiaoDanConst.MaGiaoDan]), item.Value);
                }
                importHonPhoiExcel(dicHonPhoi, tenGiaDinh, nguoiVo, nguoiChong);
            }
        }
        private void importHonPhoiExcel(Dictionary<int, DataRow> dicVoChong, string tenHonPhoi, int nguoiVo, int nguoiChong)
        {
            if (dicVoChong == null || dicVoChong.Count != 2)
            {
                return;
            }
            
            DataRow rowHPExcel = null;
            if(!Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["SoHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["NgayHonPhoi"])
                || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["NoiHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["LinhMucChung"])
                || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["SoHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["NguoiChung1"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiChong]["NguoiChung2"]))
            {
                rowHPExcel = dicVoChong[nguoiChong];
            }
            else if (!Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["SoHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["NgayHonPhoi"])
                || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["NoiHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["LinhMucChung"])
                || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["SoHonPhoi"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["NguoiChung1"]) || !Memory.IsNullOrEmpty(dicVoChong[nguoiVo]["NguoiChung2"]))
            {
                rowHPExcel = dicVoChong[nguoiVo];
            }
            if (rowHPExcel == null)
            {
                return;
            }

            DataRow[] rows = tblGiaoDanHonPhoi.Select(string.Format("MaGiaoDan={0} OR MaGiaoDan={1}", nguoiVo, nguoiChong));
            DataRow rowHP = null;
            if (rows.Length > 0)
            {
                DataRow[] rowsHP = tblHonPhoi.Select(string.Format("MaHonPhoi={0}", rows[0][GiaoDanHonPhoiConst.MaHonPhoi]));
                if (rowsHP.Length > 0)
                {
                    rowHP = rowsHP[0];
                }
                for (int i = rows.Length - 1; i >= 0; i--)
                {
                    rows[i].Delete();
                }
            }
            if (rowHP == null)
            {
                rowHP = tblHonPhoi.NewRow();
                tblHonPhoi.Rows.Add(rowHP);
                rowHP[HonPhoiConst.MaHonPhoi] = Memory.Instance.GetNextId(HonPhoiConst.TableName, HonPhoiConst.MaHonPhoi, false);
                rowHP[HonPhoiConst.MaNhanDang] = Memory.GetHonPhoiKey(rowHP[HonPhoiConst.MaHonPhoi]);
            }
            rowHP[HonPhoiConst.SoHonPhoi] = rowHPExcel[HonPhoiConst.SoHonPhoi];
            rowHP[HonPhoiConst.NgayHonPhoi] = rowHPExcel[HonPhoiConst.NgayHonPhoi];
            rowHP[HonPhoiConst.NoiHonPhoi] = rowHPExcel[HonPhoiConst.NoiHonPhoi];
            rowHP[HonPhoiConst.LinhMucChung] = rowHPExcel[HonPhoiConst.LinhMucChung];
            rowHP[HonPhoiConst.NguoiChung1] = rowHPExcel[HonPhoiConst.NguoiChung1];
            rowHP[HonPhoiConst.NguoiChung2] = rowHPExcel[HonPhoiConst.NguoiChung2];
            rowHP[HonPhoiConst.CachThucHonPhoi] = rowHPExcel[HonPhoiConst.CachThucHonPhoi];

            tblGiaoDanHonPhoi.Rows.Add(nguoiChong, rowHP[HonPhoiConst.MaHonPhoi], 1);
            tblGiaoDanHonPhoi.Rows.Add(nguoiVo, rowHP[HonPhoiConst.MaHonPhoi], 1);
            
        }

        private void createMaGiaDinh(DataTable tbl, Dictionary<string, string> dicColumnText)
        {
            if (dicColumnText == null)
            {
                ExportDataToExcel.GetColumnInfo(out dicColumnText, out _);
            }

            for(int i = 0; i < tbl.Rows.Count; i++)
            {
                DataRow r = tbl.Rows[i];

                // get ma gia dinh
                if(string.IsNullOrEmpty(r[dicColumnText[GiaDinhConst.MaGiaDinhRieng]].ToString().Trim()))
                {
                    if (r[dicColumnText["QuanHe"]].ToString().ToLower().Trim().Contains("chủ hộ"))
                    {
                        r[dicColumnText[GiaDinhConst.MaGiaDinhRieng]] =
                            Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, false);
                    }
                    else
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (tbl.Rows[j][dicColumnText["QuanHe"]].ToString().ToLower().Trim().Contains("chủ hộ"))
                            {
                                r[dicColumnText[GiaDinhConst.MaGiaDinhRieng]] =
                                    tbl.Rows[j][dicColumnText[GiaDinhConst.MaGiaDinhRieng]].ToString();
                                break;
                            }
                        }
                    }
                }

                // correct quan he
                if (r[dicColumnText[GiaoDanConst.HoTen]].ToString().Trim().ToLower().Contains("thị "))
                {
                    r[dicColumnText[GiaoDanConst.Phai]] = "Nữ";
                }
                if(string.IsNullOrEmpty(r[dicColumnText["QuanHe"]].ToString().Trim()) && i > 0)
                {
                    if (tbl.Rows[i - 1][dicColumnText["QuanHe"]].ToString().ToLower().Trim().Contains("chủ hộ"))
                    {
                        if (tbl.Rows[i - 1][dicColumnText[GiaoDanConst.Phai]].ToString().ToLower().Trim() == "nam"
                            && tbl.Rows[i][dicColumnText[GiaoDanConst.Phai]].ToString().ToLower().Trim() == "nữ")
                        {
                            r[dicColumnText["QuanHe"]] = "Vợ";
                        }
                        else
                        {
                            r[dicColumnText["QuanHe"]] = "Chưa rõ";
                        }
                    }
                    else
                    {
                        r[dicColumnText["QuanHe"]] = "Con";
                    }
                }
            }
        }
        
        private int maGiaoHo = 0;

        private int getMaGiaoHo(string tenGiaoHo)
        {
            return getMaGiaoHo(tenGiaoHo, false);
        }
        private int getMaGiaoHo(string tenGiaoHo, bool convertVNI)
        {
            tenGiaoHo = Memory.AutoUpperFirstChar(tenGiaoHo).Trim();
            if (convertVNI && convert.isVietnamese(tenGiaoHo))
            {
                tenGiaoHo = Memory.ConvertVNI2UNI(tenGiaoHo);
            }
            if (tenGiaoHo == "" || tenGiaoHo.ToLower() == "ngoài xứ")
            {
                return 0;
            }
            if (tblGiaoHo == null)
            {
                tblGiaoHo = Memory.GetData("SELECT * FROM GiaoHo");
                DataSet ds = new DataSet();
                tblGiaoHo.TableName = GiaoHoConst.TableName;
                ds.Tables.Add(tblGiaoHo);

                object oID = tblGiaoHo.Compute("MAX(MaGiaoHo)", "");
                maGiaoHo = (oID != DBNull.Value ? Convert.ToInt32(oID) : 0);
            }
            if (tblGiaoHo != null)
            {
                DataRow[] rows = tblGiaoHo.Select(string.Format("TenGiaoHo='{0}'", tenGiaoHo));
                if (rows.Length > 0)
                {
                    return (int)rows[0][GiaoHoConst.MaGiaoHo];
                }
                else
                {

                    DataRow row = tblGiaoHo.NewRow();
                    maGiaoHo++;
                    row[GiaoHoConst.MaGiaoHo] = maGiaoHo;
                    row[GiaoHoConst.TenGiaoHo] = tenGiaoHo;
                    row[GiaoHoConst.MaNhanDang] = Memory.GetGiaoHoKey(maGiaoHo);
                    row[GiaoHoConst.UpdateDate] = DateTime.Now;
                    tblGiaoHo.Rows.Add(row);

                    Memory.UpdateDataSet(tblGiaoHo.DataSet);
                    tblGiaoHo.AcceptChanges();
                    if (Memory.HasError())
                    {
                        throw new Exception("Tạo giáo họ mới bị lỗi. Thông tin lỗi: " + Memory.Instance.Error.Message);
                    }
                    return maGiaoHo;
                }
            }
            return 0;
        }
        #endregion

        #region Import MDB

        private void importMDBFile()
        {
            Memory.ClearError();
            sw = new StreamWriter(Memory.AppPath + "ImportLog.txt", true, Encoding.UTF8);
            //check valid db
            if (!IsValidDb())
            {
                sw.WriteLine();
                sw.WriteLine("START - " + DateTime.Now.ToString() + " -----------------------");
                sw.WriteLine(GxConstants.MSG_INVALID_FILE_IMPORT);
                sw.Write("END - " + DateTime.Now.ToString() + " -----------------------");
                sw.Close();
                if (OnError != null) OnError(GxConstants.MSG_INVALID_FILE_IMPORT, new CancelEventArgs());
                if (OnFinished != null) OnFinished("finished", EventArgs.Empty);
                return;
            }
            string sql = "SELECT COUNT(A.GiaDinh) AS Tong FROM (SELECT giaoxu.[GIA DINH] AS GiaDinh FROM giaoxu GROUP BY giaoxu.[GIA DINH]) AS A";
            DataTable tblTmp = provider.GetData(sql);
            if (tblTmp != null && tblTmp.Rows.Count > 0)
            {
                Memory.Instance.SetMemory("TongDuLieu", tblTmp.Rows[0][0]);
            }
            if (OnStart != null) OnStart("started", EventArgs.Empty);
            try
            {
                sw.WriteLine();
                sw.WriteLine("START - " + DateTime.Now.ToString() + " -----------------------");
                if (ImportGiaoHo())
                {
                    ImportCacGiaDinh();
                }
                sw.WriteLine("END - " + DateTime.Now.ToString() + " -----------------------");
                if (OnFinished != null) OnFinished("finished", EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                sw.WriteLine("Import: " + ex.Message);
                if (OnError != null) OnError("Import: " + ex.Message, new CancelEventArgs());
            }
            finally
            {
                sw.Close();
            }
        }

        private bool ImportGiaoHo()
        {
            try
            {
                if (OnExecuting != null) OnExecuting("Bắt đầu nhập các giáo họ", EventArgs.Empty);
                DataTable tbl = provider.GetData("SELECT * FROM tenho");
                int currentMaGiaoHo = Memory.Instance.GetNextId("GiaoHo", "MaGiaoHo", true);
                tblGiaoHo = Memory.GetData("SELECT * FROM GiaoHo");
                tblGiaoHo.TableName = "GiaoHo";
                tblGiaoHo.Columns.Add("MaGiaoHoCu", typeof(int));
                if (!Memory.HasError())
                {
                    foreach (DataRow row in tbl.Rows)
                    {
                        //check user abort
                        bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                        //if user abort progress, return
                        if (userStop) return false;

                        string tenGiaoHo = Memory.ConvertVNI2UNIUpperFirstChar(row["TEN HO"].ToString());
                        //int maGiaoHo = -1;
                        if (OnExecuting != null) OnExecuting("Đang nhập giáo họ: " + tenGiaoHo.Replace("'", "''"), EventArgs.Empty);
                        DataRow[] rows = tblGiaoHo.Select("TenGiaoHo='" + tenGiaoHo + "'");
                        if (rows != null && rows.Length > 0)
                        {
                            rows[0]["TenGiaoHo"] = tenGiaoHo;
                            continue;
                        }
                        //if (tenGiaoHo.ToLower() != "ngoài xứ") maGiaoHo = currentMaGiaoHo++;
                        //if (maGiaoHo == -1)//ngoai xu
                        //{
                        //    rows = tblGiaoHo.Select("MaGiaoHo=-1");
                        //    if (rows != null && rows.Length > 0)
                        //    {
                        //        rows[0]["MaGiaoHoCu"] = row["HO NAO"];
                        //        continue;
                        //    }
                        //}
                        DataRow newRow = tblGiaoHo.NewRow();
                        newRow["MaGiaoHo"] = currentMaGiaoHo++;
                        newRow["TenGiaoHo"] = tenGiaoHo;
                        newRow["MaGiaoHoCu"] = row["HO NAO"];
                        newRow["UpdateDate"] = Memory.Instance.GetServerDateTime();
                        tblGiaoHo.Rows.Add(newRow);
                    }
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tblGiaoHo);
                    Memory.UpdateDataSet(ds);
                    if (OnExecuting != null) OnExecuting("Kết thúc nhập các giáo họ", EventArgs.Empty);
                    if (Memory.HasError())
                    {
                        if (OnError != null) OnError("Lỗi khi cập nhật các giáo họ", new CancelEventArgs());
                        return false;
                    }
                    ds.Tables.Remove(tblGiaoHo);
                }
                else
                {
                    if (OnError != null) OnError("Lỗi khi lấy dữ liệu giáo họ", new CancelEventArgs());
                    return false;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                sw.WriteLine("ImportGiaoHo: " + ex.Message);
                if (OnError != null) OnError("Có lỗi khi nhập dữ liệu giáo họ", new CancelEventArgs());
                return false;
            }
            return true;
        }

        private bool ImportCacGiaDinh()
        {
            try
            {
                if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu gia đình và giáo dân...", EventArgs.Empty);
                tblGiaoDan = Memory.GetData("SELECT * FROM GiaoDan");
                tblGiaDinh = Memory.GetData("SELECT * FROM GiaDinh");
                tblGiaDinh.Columns.Add("MaGiaDinhCu", typeof(int));

                DataTable tblGiaoXuCu = provider.GetData("SELECT giaoxu.*, tenho.[TEN HO] FROM giaoxu INNER JOIN tenho ON (giaoxu.[HO NAO] = tenho.[HO NAO])");
                tblThanhVienGiaDinh = Memory.GetData("SELECT * FROM ThanhVienGiaDinh");
                if (tblGiaoXuCu == null)
                {
                    Memory.Instance.Error = new Exception("Không tìm thấy dữ liệu gia đình và giáo dân");
                    if (OnError != null) OnError(Memory.Instance.Error.Message, new CancelEventArgs());
                    return false;
                }
                string tenGiaDinh = "";
                int maGiaDinh = -1;
                giaDinhHienTai = 0;
                int maxMaGiaDinh = 0;
                for (int i = 0; i < tblGiaoXuCu.Rows.Count; i++)
                {
                    //check user abort
                    bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                    //if user abort progress, return
                    if (userStop) return false;
                    DataRow row = tblGiaoXuCu.Rows[i];
                    string tmp = Memory.ConvertVNI2UNI(row["GIA DINH"].ToString());
                    string quanhe = Memory.ConvertVNI2UNI(row["QUAN HE"].ToString()).ToLower();
                    if (tenGiaDinh != tmp)
                    {
                        tenGiaDinh = Memory.ConvertVNI2UNI(row["GIA DINH"].ToString());
                        maGiaDinh = ImportGiaDinh(row);
                        if (maGiaDinh > maxMaGiaDinh)
                        {
                            maxMaGiaDinh = maGiaDinh;
                            giaDinhHienTai++;
                            Memory.Instance.SetMemory("DuLieuHienTai", giaDinhHienTai);
                        }
                        if (OnExecuting != null) OnExecuting(string.Format("Đang nhập gia đình: {0}", tenGiaDinh), EventArgs.Empty);
                    }
                    
                    int maThanhVienGiaDinh = ImportGiaoDan(row);
                    int vaitro = getVaiTro(quanhe, row["PHAI"].ToString());

                    assignThanhVienGiaDinh(maGiaDinh, maThanhVienGiaDinh, vaitro);
                }
                if (OnExecuting != null) OnExecuting("Đang hoàn tất việc cập nhật dữ liệu vào chương trình...", EventArgs.Empty);
                DataSet ds = new DataSet();
                tblGiaDinh.TableName = GiaDinhConst.TableName;
                ds.Tables.Add(tblGiaDinh);
                tblGiaoDan.TableName = GiaoDanConst.TableName;
                ds.Tables.Add(tblGiaoDan);
                tblThanhVienGiaDinh.TableName = ThanhVienGiaDinhConst.TableName;
                ds.Tables.Add(tblThanhVienGiaDinh);
                Memory.UpdateDataSet(ds);
                if (!Memory.HasError())
                {
                    return true;
                }
                else
                {
                    sw.WriteLine("ImportCacGiaDinh: " + Memory.Instance.Error.Message);
                    if (OnError != null) OnError("Cập nhật dữ liệu xuống database của chương trình bị thất bại. \r\nXin vui lòng xem file ImportLog.txt trong thư mục của chương trình để biết thêm chi tiết lỗi xảy ra.", new CancelEventArgs());
                    return false;
                }

            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                sw.WriteLine("ImportCacGiaDinh: " + ex.Message);
                if (OnError != null) OnError("Có lỗi chung xảy ra khi nhập dữ liệu các gia đình. \r\nXin vui lòng xem file ImportLog.txt trong thư mục của chương trình để biết thêm chi tiết lỗi xảy ra.", new CancelEventArgs());
                return false;
            }
        }

        private void reviewData()
        {
            foreach (DataRow giaoDan in tblGiaoDan.Rows)
            {
                string strCheck = string.Format("MaGiaoDan={0} AND (VaiTro=0 OR VaiTro=1)", giaoDan["MaGiaoDan"]);
                DataRow[] rows = tblThanhVienGiaDinh.Select(strCheck);
                if (rows != null && rows.Length > 0)
                {
                    giaoDan[GiaoDanConst.DaCoGiaDinh] = true;
                }
            }
        }

        private int getVaiTro(string quanhe, string phai)
        {
            int vaitro = 2;
            if (quanhe == "chồng" || quanhe == "cha") vaitro = 0;
            else if (quanhe == "vợ" || quanhe == "mẹ" || quanhe == "độc thân") vaitro = 1;
            //tim vo, truong hop gia dinh co den 2 vo -> chang hieu tai sao
            if (vaitro == 0 || vaitro == 1)
            {
                phai = Memory.ConvertVNI2UNI(phai);
                if (phai.ToLower() == "nam") vaitro = 0; else vaitro = 1;
            }
            else
            {
                switch (quanhe)
                {
                    case "chị":
                        vaitro = 17;
                        break;
                    case "em":
                        vaitro = 18;
                        break;
                    case "cháu":
                        vaitro = 3;
                        break;
                }
            }
            return vaitro;
        }

        private void assignThanhVienGiaDinh(int maGiaDinh, int maThanhVienGiaDinh, int vaiTro)
        {
            if (maGiaDinh > -1 && tblThanhVienGiaDinh != null)
            {
                DataRow[] checkThanhVienGiaDinh = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh = {0} AND MaGiaoDan = {1}", maGiaDinh, maThanhVienGiaDinh));
                if (checkThanhVienGiaDinh != null && checkThanhVienGiaDinh.Length > 0)
                {
                    //If existed
                    return;
                }
                DataRow rowThanhVienGiaDinh = tblThanhVienGiaDinh.NewRow();
                rowThanhVienGiaDinh["MaGiaDinh"] = maGiaDinh;
                rowThanhVienGiaDinh["MaGiaoDan"] = maThanhVienGiaDinh;
                rowThanhVienGiaDinh["VaiTro"] = vaiTro;
                tblThanhVienGiaDinh.Rows.Add(rowThanhVienGiaDinh);
            }
        }

        private DataRow FindGiaDinh(DataRow row)
        {
            string s = "";
            //string ngayHPQuery = row["NGAY HON PHOI"] == DBNull.Value ? " NgayHonPhoi IS NULL " : string.Format(" NgayHonPhoi = '{0}'", row["NGAY HON PHOI"]);
            //string soHPQuery = row["SO HP"] == DBNull.Value ? " SoHonPhoi IS NULL " : string.Format(" SoHonPhoi = {0}", row["SO HP"]);
            s = string.Format("MaGiaDinhCu = {0} OR TenGiaDinh = '{1}'", row["NOCGIA"], Memory.ConvertVNI2UNI(row["GIA DINH"].ToString()).Replace("'", "''"));
            DataRow[] rows = tblGiaDinh.Select(s);
            if (rows != null && rows.Length > 0)
            {
                return rows[0];
            }
            return null;
        }

        private int ImportGiaoDan(DataRow row)
        {
            try
            {
                string s = "";
                if (row["NGAY SINH"] != DBNull.Value)
                {
                    s = string.Format("TenThanh = '{0}' AND HoTen='{1} {2}' AND NgaySinh = '{3}'",
                                                 Memory.ConvertVNI2UNI(row["TEN THANH"].ToString()),
                                                 Memory.ConvertVNI2UNIUpperFirstChar(row["HO"].ToString()),
                                                 Memory.ConvertVNI2UNIUpperFirstChar(row["TEN"].ToString()),
                                                 Memory.GetDateString(row["NGAY SINH"]));
                }
                else
                {
                    s = string.Format("TenThanh = '{0}' AND HoTen='{1} {2}' AND NgaySinh IS NULL",
                                                 Memory.ConvertVNI2UNI(row["TEN THANH"].ToString().Replace("'", "''")),
                                                 Memory.ConvertVNI2UNIUpperFirstChar(row["HO"].ToString().Replace("'", "''")),
                                                 Memory.ConvertVNI2UNIUpperFirstChar(row["TEN"].ToString().Replace("'", "''")));
                }

                DataRow[] rows = tblGiaoDan.Select(s);
                if (rows != null && rows.Length > 0)
                {
                    return (int)rows[0][GiaoDanConst.MaGiaoDan];
                }
                int maGiaoDan = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, false);
                DataRow row1 = tblGiaoDan.NewRow();
                assignGiaoDan(maGiaoDan, row1, row);
                tblGiaoDan.Rows.Add(row1);
                return maGiaoDan;

            }
            catch (Exception ex)
            {
                sw.WriteLine("ImportGiaoDan: " + ex.Message);
                Memory.Instance.Error = ex;
                if (OnError != null)
                {
                    OnError(string.Format("Lỗi khi nhập dữ liệu giáo dân: Tên thánh: {0}, Họ Tên:{1} {2}, Ngày sinh: {3}",
                        Memory.ConvertVNI2UNI(row["TEN THANH"].ToString()),
                        Memory.ConvertVNI2UNIUpperFirstChar(row["HO"].ToString()),
                        Memory.ConvertVNI2UNIUpperFirstChar(row["TEN"].ToString()),
                        row["NGAY SINH"] == DBNull.Value ? "Chưa xác định" : ((DateTime)row["NGAY SINH"]).ToString("dd/mm/yyyy")),
                    new CancelEventArgs());
                }
            }
            return -1;
        }

        private void assignGiaoDan(int maGiaoDan, DataRow newRow, DataRow oldRow)
        {
            newRow[GiaoDanConst.MaGiaoDan] = maGiaoDan;
            newRow[GiaoDanConst.ChaRuaToi] = Memory.ConvertVNI2UNI(oldRow["LINH MUC RUA TOI"].ToString());
            //row[GiaoDanConst.ChaRuocLe] = row1[""];
            //row[GiaoDanConst.ChaThemSuc] = row1[""];
            newRow[GiaoDanConst.ConHoc] = (oldRow["Hoc"].ToString() == "x");
            newRow[GiaoDanConst.HoTen] = Memory.ConvertVNI2UNIUpperFirstChar(oldRow["HO"].ToString() + " " + oldRow["TEN"].ToString());
            newRow[GiaoDanConst.NgayRuaToi] = Memory.GetDateString(oldRow["NG RUA TOI"]);
            newRow[GiaoDanConst.NgayRuocLe] = Memory.GetDateString(oldRow["NGAY RUOC LE"]);
            newRow[GiaoDanConst.NgaySinh] = Memory.GetDateString(oldRow["NGAY SINH"]);
            newRow[GiaoDanConst.NgayThemSuc] =Memory.GetDateString(oldRow["NGAY THEM SUC"]);
            newRow[GiaoDanConst.NgheNghiep] = Memory.ConvertVNI2UNI(oldRow["NgheNghiep"].ToString());
            //row[GiaoDanConst.NguoiDoDauRuaToi] = row1[""];
           // row[GiaoDanConst.NguoiDoDauThemSuc] = row1[""];
            newRow[GiaoDanConst.NoiRuaToi] = Memory.ConvertVNI2UNI(oldRow["NOI RUA TOI"].ToString());
            newRow[GiaoDanConst.NoiRuocLe] = Memory.ConvertVNI2UNI(oldRow["NOI RUOC LE"].ToString());
            newRow[GiaoDanConst.NoiSinh] = Memory.ConvertVNI2UNI(oldRow["NOI SINH"].ToString());
            newRow[GiaoDanConst.NoiThemSuc] = Memory.ConvertVNI2UNI(oldRow["NOI THEM SUC"].ToString());
            newRow[GiaoDanConst.Phai] = oldRow["PHAI"].ToString() == "nam" ? "Nam" : "Nữ";
            //row[GiaoDanConst.QuaDoi] = row1[""];
            newRow[GiaoDanConst.TenThanh] = Memory.ConvertVNI2UNI(oldRow["TEN THANH"].ToString());
            newRow[GiaoDanConst.TrinhDoVanHoa] = Memory.ConvertVNI2UNI(oldRow["VANHOA"].ToString());
            //row[GiaoDanConst.DienThoai] = row1[""];
            //row[GiaoDanConst.Email] = row1[""];
            newRow[GiaoDanConst.MaGiaoHo] = getMaGiaoHo(oldRow["HO NAO"], Memory.ConvertVNI2UNIUpperFirstChar(oldRow["TEN HO"].ToString()));
            newRow[GiaoDanConst.UpdateDate] = Memory.Instance.GetServerDateTime();
            newRow[GiaoDanConst.SoRuaToi] = oldRow["SO RUA TOI"];
            //row[GiaoDanConst.SoThemSuc] = row1[""];
            newRow[GiaoDanConst.GhiChu] = Memory.ConvertVNI2UNI(oldRow["GHI CHU"].ToString());
            newRow[GiaoDanConst.HoTenCha] = Memory.ConvertVNI2UNIUpperFirstChar(oldRow["CHA"].ToString());
            newRow[GiaoDanConst.HoTenMe] = Memory.ConvertVNI2UNIUpperFirstChar(oldRow["ME"].ToString());
        }

        private bool IsValidDb()
        {
            //check table giaoxu
            DataTable tbl = provider.GetData("SELECT * FROM giaoxu");
            string[] strChecks = new string[] { "LINH MUC RUA TOI", "Hoc", "HO", "NG RUA TOI", 
                                "NGAY RUOC LE", "NGAY SINH", "NGAY THEM SUC", "NgheNghiep", "NOI RUA TOI", "NOI RUOC LE", 
                                "NOI SINH", "NOI THEM SUC", "PHAI", "TEN THANH", "VANHOA","HO NAO","SO RUA TOI","GHI CHU" };
            if (tbl != null)
            {
                for (int i = 0; i < strChecks.Length; i++)
                {
                    if (!tbl.Columns.Contains(strChecks[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            tbl = provider.GetData("SELECT * FROM tenho");
            strChecks = new string[] { "xuho", "HO NAO", "TEN HO", "TENTAT", "BONMANGHO" };
            if (tbl != null)
            {
                for (int i = 0; i < strChecks.Length; i++)
                {
                    if (!tbl.Columns.Contains(strChecks[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            //check table tenho
            return true;
        }

        private int getMaGiaoHo(object maGiaoHoCu, object tenGiaoHo)
        {
            DataRow[] ho = tblGiaoHo.Select(string.Format("MaGiaoHoCu = {0} OR TenGiaoHo = '{1}'", maGiaoHoCu, tenGiaoHo.ToString().Replace("'", "''")));
            if (ho != null && ho.Length > 0)
            {
                return (int)ho[0][GiaoHoConst.MaGiaoHo];
            }

            ho = tblGiaoHo.Select("TenGiaoHo = 'Ngoài Xứ'");
            if (ho != null && ho.Length > 0)
            {
                return (int)ho[0][GiaoHoConst.MaGiaoHo];
            }

            return -1;
        }

        private int ImportGiaDinh(DataRow row)
        {
            try
            {
                DataRow rowTmp = FindGiaDinh(row);
                if (rowTmp != null)
                {
                    return (int)rowTmp[GiaDinhConst.MaGiaDinh];
                }
                int maGiaDinh = Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, false);
                DataRow row1 = tblGiaDinh.NewRow();
                assignGiaDinh(maGiaDinh, row1, row);
                tblGiaDinh.Rows.Add(row1);
                return maGiaDinh;

            }
            catch (Exception ex)
            {
                sw.WriteLine("ImportGiaDinh: " + ex.Message);
                Memory.Instance.Error = ex;
                if (OnError != null) OnError("Có lỗi khi nhập dữ liệu gia đình" + Memory.ConvertVNI2UNI(row["GIA DINH"].ToString()), new CancelEventArgs());
            }
            return -1;
        }

        private void assignGiaDinh(int maGiaDinh, DataRow newRow, DataRow oldRow)
        {
            newRow[GiaDinhConst.MaGiaDinh] = maGiaDinh;
            newRow["MaGiaDinhCu"] = oldRow["NOCGIA"];
            newRow[GiaDinhConst.MaGiaoHo] = getMaGiaoHo(oldRow["HO NAO"], Memory.ConvertVNI2UNIUpperFirstChar(oldRow["TEN HO"].ToString()));
            newRow[GiaDinhConst.TenGiaDinh] = Memory.ConvertVNI2UNI(oldRow["GIA DINH"].ToString());
            newRow[HonPhoiConst.SoHonPhoi] = oldRow["SO HP"];
            //newRow[GiaDinhConst.NguoiVo] = maVo;
            //newRow[GiaDinhConst.NguoiChong] = maChong;
            string tthp = Memory.ConvertVNI2UNI(oldRow["TinhTrangHp"].ToString());
            if (tthp.ToLower() == "hth")
            {
                tthp = "Hợp thức hóa";
            }
            //else if (tthp == "chuẩn")
            //{
            //    tthp = "Chuẩn";
            //}
            else
            {
                tthp = "Hợp pháp";
            }
            //newRow[GiaDinhConst.CachThucHonPhoi] = tthp;
        }
        #endregion
    }
}
