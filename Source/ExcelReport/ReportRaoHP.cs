using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportRaoHP
    {
        private static int beginDetailRow = -1;
        public static int Export(DataSet ds, bool printRS = false)
        {
            ExcelEngine excel = new ExcelEngine();
            try
            {
                if (!ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(ReportRaoHonPhoiConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblReportRaoHonPhoi = ds.Tables[ReportRaoHonPhoiConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                //Get template and output path
                if (tblGiaoXu.Rows.Count == 0 || tblReportRaoHonPhoi.Rows.Count == 0) return -1;
                string templatePath;
                string outputPath;
                if (printRS)
                {
                    templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_KQRAOHONPHOI_FILENAME);
                    outputPath = Memory.GetTempPath(GxConstants.REPORT_KQRAOHONPHOI_FILENAME);
                }
                else
                {
                    templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_RAOHONPHOI_FILENAME);
                    outputPath = Memory.GetTempPath(GxConstants.REPORT_RAOHONPHOI_FILENAME);
                }
                //Get working row
                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowData = tblReportRaoHonPhoi.Rows[0];

                string reportFormat = Memory.GetReportFormat();
                if (!System.IO.File.Exists(string.Concat(templatePath, reportFormat)))
                {
                    reportFormat = ".xls";
                }
                templatePath = string.Concat(templatePath, reportFormat);
                outputPath = string.Concat(outputPath, reportFormat);

                if (reportFormat == ".xls" && excel.CreateObject(outputPath, templatePath))
                {
                    try
                    {
                        excel.LoaiBaoCao = ReportType.RaoHonPhoi;
                        excel.Write_to_excel(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                        excel.Write_to_excel(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                        excel.Write_to_excel(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                        excel.Write_to_excel(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                        excel.Write_to_excel(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                        excel.Write_to_excel(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                        excel.Write_to_excel(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);
                        excel.Write_to_excel(ReportChungNhanBTConst.TenGiaoXuNhan, rowData[ReportRaoHonPhoiConst.TenLinhMucNhan]);
                        excel.Write_to_excel(ReportChungNhanBTConst.TenGiaoPhanNhan, rowData[ReportRaoHonPhoiConst.GiaoXuNhan]);

                        foreach (DataColumn col in tblReportRaoHonPhoi.Columns)
                        {
                            excel.Write_to_excel(col.ColumnName, rowData[col]);
                        }

                        excel.End_Write();
                        System.Diagnostics.Process.Start(outputPath);
                    }
                    catch (Exception ex)
                    {
                        Memory.Instance.Error = ex;
                    }
                }
                else if (reportFormat == ".doc")
                {
                    WordEngine word = new WordEngine();
                    word.CreateObject(outputPath, templatePath);

                    try
                    {
                        word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                        word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                        word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu].ToString().Replace("GIÁO XỨ ", "").Replace("Giáo xứ ", "").Replace("Giáo Xứ ", ""));
                        word.Replace(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                        word.Replace(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                        word.Replace(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                        word.Replace(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);
                        word.Replace(ReportChungNhanBTConst.TenGiaoXuNhan, rowData[ReportRaoHonPhoiConst.TenLinhMucNhan]);
                        word.Replace(ReportChungNhanBTConst.TenGiaoPhanNhan, rowData[ReportRaoHonPhoiConst.GiaoXuNhan]);

                        foreach (DataColumn col in tblReportRaoHonPhoi.Columns)
                        {
                            word.Replace(col.ColumnName, rowData[col]);
                        }

                        word.End_Write();
                        System.Diagnostics.Process.Start(outputPath);
                    }
                    catch (Exception ex)
                    {
                        Memory.Instance.Error = ex;
                    }
                }
                else
                {
                    Memory.Instance.Error = new Exception("Xuất rao hôn phối thất bại." + Environment.NewLine +
                                                            "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                            "Có thể do tập tin \"RaoHonPhoi.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                            "Xin vui lòng đóng tập tin này và thử lại lần nữa." + Environment.NewLine +
                                                                "Chi tiết lỗi: " + Memory.Instance.Error?.Message);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                if (excel != null) excel.End_Write();
                Memory.Instance.Error = ex;
                return -1;
            }
            return 0;
        }

        public static int ExportList(DataSet ds)
        {
            ExcelEngine excel = new ExcelEngine();
            try
            {
                if (!ds.Tables.Contains(RaoHonPhoiTMPConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblReportRaoHonPhoi = ds.Tables[RaoHonPhoiTMPConst.TableName];
                //Get template and output path
                if (tblReportRaoHonPhoi.Rows.Count == 0) return -1;
                string templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_RAOHONPHOILIST_FILENAME);
                string outputPath = Memory.GetTempPath(GxConstants.REPORT_RAOHONPHOILIST_FILENAME);
                //Get working row
                DataRow rowData = tblReportRaoHonPhoi.Rows[0];

                if (excel.CreateObject(outputPath, templatePath))
                {
                    try
                    {
                        excel.LoaiBaoCao = ReportType.RaoHonPhoiList;
                        Dictionary<string, int> dicPos = getPositionsDic(excel, tblReportRaoHonPhoi);
                        int maxColIndex = 0;
                        foreach (KeyValuePair<string, int> item in dicPos)
                        {
                            if (item.Value > maxColIndex) maxColIndex = item.Value;
                        }
                        string lasDetailColName = excel.MapColIndexToColName(maxColIndex);

                        int minColIndex = 20;
                        foreach (KeyValuePair<string, int> item in dicPos)
                        {
                            if (item.Value < minColIndex) minColIndex = item.Value;
                        }
                        string beginDetailColName = excel.MapColIndexToColName(minColIndex);

                        int i = beginDetailRow;
                        bool inNoiSinh = Memory.GetConfig(GxConstants.CF_SOGIADINH_INNOISINH) == GxConstants.CF_TRUE;
                        string lang = Memory.GetConfig(GxConstants.CF_LANGUAGE);
                        bool isUSFormatName = Memory.GetConfig(GxConstants.CF_US_FORMAT_NAME) == GxConstants.CF_TRUE;
                        int tmp = 0;
                        int stt = 0;
                        foreach (DataRow row in tblReportRaoHonPhoi.Rows)
                        {
                            //if (tmp != (int)row[RaoHonPhoiTMPConst.MaRaoHonPhoi])
                            //{
                            //    tmp = (int)row[RaoHonPhoiTMPConst.MaRaoHonPhoi];
                            //    stt++;
                            //}
                            //excel.Write_to_excel(i, 2, stt);//in so thu tu

                            foreach (KeyValuePair<string, int> item in dicPos)
                            {
                                string key = item.Key.ToLower();
                                if (key.StartsWith("ngay"))
                                {
                                    row[item.Key] = Memory.GetDateStringByLang(row[item.Key]);
                                    excel.Write_to_excel(i, item.Value, "'" + row[item.Key].ToString());

                                }
                                if (item.Key == GiaoDanConst.TanTong)
                                {
                                    if ((bool)row[GiaoDanConst.TanTong] == false)
                                    {
                                        excel.Write_to_excel(i, item.Value, "");
                                    }
                                    else
                                    {
                                        excel.Write_to_excel(i, item.Value, "X");
                                    }
                                }
                                else
                                {
                                    if (Validator.IsNumber(row[item.Key].ToString()))
                                    {
                                        excel.Write_to_excel(i, item.Value, "'" + row[item.Key].ToString());
                                    }
                                    else
                                    {
                                        excel.Write_to_excel(i, item.Value, row[item.Key]);
                                    }
                                }
                            }
                            //Merge
                            if ((i - beginDetailRow) % 2 != 0)
                            {
                                stt++;
                                excel.Write_to_excel(i, 1, stt);
                                excel.Merge("A" + (i - 1).ToString(), "A" + i.ToString());//STT
                                excel.Merge("B" + (i - 1).ToString(), "B" + i.ToString());//Doi rao
                                //excel.Merge("C" + (i - 1).ToString(), "C" + i.ToString());//Lan rao
                            }
                            i++;
                        }
                        excel.Border_Range(beginDetailColName + beginDetailRow.ToString(), lasDetailColName + (i - 1).ToString());

                        i++;

                        excel.SetWrapText(beginDetailColName + i.ToString(), lasDetailColName + i.ToString(), false);
                        DateTime d = DateTime.Now;
                        if (Memory.Instance.GetMemory(ReportRaoHonPhoiConst.ThoiGianRao) != null && Memory.Instance.GetMemory(ReportRaoHonPhoiConst.ThoiGianRao) is DateTime)
                        {
                            d = (DateTime)Memory.Instance.GetMemory(ReportRaoHonPhoiConst.ThoiGianRao);
                        }
                        excel.Write_to_excel(ReportRaoHonPhoiConst.ThoiGianRao, d.ToString("dd/MM/yyyy"));

                        excel.End_Write();

                        System.Diagnostics.Process.Start(outputPath);
                    }
                    catch (Exception ex)
                    {
                        Memory.Instance.Error = ex;
                    }
                }
                else
                {
                    Memory.Instance.Error = new Exception("Xuất danh sách điều tra hôn phối." + Environment.NewLine +
                                                            "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                            "Có thể do tập tin \"DanhSachRaoHonPhoi.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                            "Xin vui lòng đóng tập tin này và thử lại lần nữa." + Environment.NewLine +
                                                                "Chi tiết lỗi: " + Memory.Instance.Error?.Message);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return -1;
            }
            beginDetailRow = -1;
            return 0;
        }

        private static Dictionary<string, int> getPositionsDic(ExcelEngine excel, DataTable tbl)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();

            if (excel != null && tbl != null)
            {
                foreach (DataColumn col in tbl.Columns)
                {
                    if (col.ColumnName == GiaoDanConst.DienThoai) continue;
                    int idx = excel.FindColIndex(string.Format("[{0}]", col.ColumnName));
                    if (idx > -1)
                    {
                        if (beginDetailRow == -1 && col.ColumnName == GiaoDanConst.Phai)
                        {
                            beginDetailRow = excel.FindRowIndex(string.Format("[{0}]", col.ColumnName));
                        }
                        dic.Add(col.ColumnName, idx);
                    }
                }
            }
            return dic;
        }
    }
}
