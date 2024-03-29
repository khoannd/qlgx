using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace ExcelReport
{
    public class ReportSoGiaDinh
    {
        private static Dictionary<int, string> vaiTro;
        public static string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
            }
        }
        private static int beginDetailRow = -1;
        private static string fileName; 
        private static void prepareThanhVienGiaDinhData(DataTable tblGiaoDan)
        {
            string lang = Memory.GetConfig(GxConstants.CF_LANGUAGE);
            bool isUSFormatName = Memory.GetConfig(GxConstants.CF_US_FORMAT_NAME) == GxConstants.CF_TRUE;

            //for(int i = tblGiaoDan.Rows.Count - 1; i >= 0; i--)
            int dem = 1;
            for(int i = 0; i < tblGiaoDan.Rows.Count ; i++)
            {
                DataRow row = tblGiaoDan.Rows[i];

                //xem xet giao dan hien tai co cho phep in khong - start
               
                //hiepdv begin edit
                if((bool)row[GiaoDanConst.QuaDoi])
                {
                    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LUUTRU) != GxConstants.CF_TRUE)
                    {
                        tblGiaoDan.Rows.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                if ((bool)row[GiaoDanConst.DaXoa])
                {
                    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_DAXOA) != GxConstants.CF_TRUE)
                    {
                        tblGiaoDan.Rows.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                if ((int)row[GiaoDanConst.DaChuyenXu]==-1)
                {
                    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_DACHUYENXU) != GxConstants.CF_TRUE)
                    {
                        tblGiaoDan.Rows.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                //if ((bool)row[GiaoDanConst.QuaDoi] || row[GiaoDanConst.DaChuyenXu].ToString() != GxConstants.CF_FALSE)
                //{
                //    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LUUTRU) != GxConstants.CF_TRUE)
                //    {
                //        tblGiaoDan.Rows.RemoveAt(i);
                //        continue;
                //    }
                //}
                //hiepdv end edit

                if ((int)row[ThanhVienGiaDinhConst.VaiTro] > 1 && (bool)row[GiaoDanConst.DaCoGiaDinh])
                {
                    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LAPGD) != GxConstants.CF_TRUE)
                    {
                        tblGiaoDan.Rows.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                //xem xet giao dan hien tai co cho phep in khong - end

                //row[GiaoDanConst.NgaySinh] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.SinhRa, row[GiaoDanConst.NgaySinh], row[GiaoDanConst.NoiSinh], "", "", "");
                //row[GiaoDanConst.NgayRuaToi] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.RuaToi, row[GiaoDanConst.NgayRuaToi], row[GiaoDanConst.NoiRuaToi], row[GiaoDanConst.SoRuaToi], row[GiaoDanConst.NguoiDoDauRuaToi], "");
                //row[GiaoDanConst.NgayRuocLe] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.RuaToi, row[GiaoDanConst.NgayRuocLe], row[GiaoDanConst.NoiRuocLe], row[GiaoDanConst.SoRuaToi], "", "");
                //row[GiaoDanConst.NgayThemSuc] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.ThemSuc, row[GiaoDanConst.NgayThemSuc], row[GiaoDanConst.NoiThemSuc], row[GiaoDanConst.SoThemSuc], row[GiaoDanConst.NguoiDoDauThemSuc], "");
                //if (!string.IsNullOrEmpty(row[HonPhoiConst.NguoiChung1].ToString()))
                //{
                //    row[HonPhoiConst.NguoiChung1] = string.Format("NC1:{0}", row[HonPhoiConst.NguoiChung1]);
                //}
                //if (!string.IsNullOrEmpty(row[HonPhoiConst.NguoiChung2].ToString()))
                //{
                //    row[HonPhoiConst.NguoiChung2] = string.Format("NC2:{0}", row[HonPhoiConst.NguoiChung2]);
                //}
                string nguoiChung = GetStringFromArray("\r\n", row[HonPhoiConst.NguoiChung1].ToString(), row[HonPhoiConst.NguoiChung2].ToString());
                //row[HonPhoiConst.NgayHonPhoi] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.HonPhoi, row[HonPhoiConst.NgayHonPhoi], row[HonPhoiConst.NoiHonPhoi], row[HonPhoiConst.SoHonPhoi], "", nguoiChung);


                //if (dicPos.ContainsKey(HonPhoiConst.NgayHonPhoi))
                //{
                //for hon phoi
                if (!Memory.IsNullOrEmpty(row[HonPhoiConst.NgayHonPhoi]))
                {
                }
                else
                {
                    if ((bool)row[GiaoDanConst.DaCoGiaDinh])
                    {
                        //honPhoi = "X";
                        row[HonPhoiConst.NgayHonPhoi] = "X";
                    }
                }
                //}

                //if (dicPos.ContainsKey(GiaoDanConst.NgayQuaDoi))
                //{
                //for qua doi
                if (!Memory.IsNullOrEmpty(row[GiaoDanConst.NgayQuaDoi]))
                {
                }
                else
                {
                    if ((bool)row[GiaoDanConst.QuaDoi])
                    {
                        row[GiaoDanConst.NgayQuaDoi] = "X";
                    }
                }
                // }

                //if (dicPos.ContainsKey(GiaoDanConst.HoTen))
                //{
                    if (lang == GxConstants.LANG_EN && isUSFormatName)
                    {
                        row[GiaoDanConst.HoTen] = Memory.GetHoTenByLangKhongTenThanh(row[GiaoDanConst.HoTen].ToString(), lang);
                    }
                //}
                if (Memory.GetConfig(GxConstants.CF_SOGIADINH_THAYSTT_MAGIAODAN) == GxConstants.CF_TRUE)
                {
                    row[GiaoDanConst.MaGiaoDan] = row[GiaoDanConst.MaGiaoDan];
                }
                else
                {
                    row[GiaoDanConst.MaGiaoDan] = dem++;
                }
            }
        }

        private static string getNgayVaNoi(ReportSoGiaDinhLoaiNgay objType, object ngay, object noi, object soBiTich, object nguoiDoDau, object nguoiChung)
        {
            if (Memory.IsNullOrEmpty(ngay))
            {
                ngay = "";
            }
            else
            {
                ngay = Memory.GetDateStringByLang(ngay);
            }

            if (Memory.IsNullOrEmpty(noi))
            {
                return ngay.ToString();
            }

            string info = "";
            List<string> infos = new List<string>();
            infos.Add(ngay.ToString());

            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INSOBITICH) == GxConstants.CF_TRUE && objType != ReportSoGiaDinhLoaiNgay.SinhRa)
            {
                if (soBiTich != null && !string.IsNullOrEmpty(soBiTich.ToString()))
                    infos.Add(string.Concat("Số: ", soBiTich.ToString()));
                else
                    infos.Add(" ");
            }
            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNGUOIDODAU) == GxConstants.CF_TRUE && objType != ReportSoGiaDinhLoaiNgay.SinhRa)
            {
                if (nguoiDoDau != null && !string.IsNullOrEmpty(nguoiDoDau.ToString()))
                    infos.Add(nguoiDoDau.ToString());
                else
                    infos.Add(" ");
            }
            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNGUOICHUNGHP) == GxConstants.CF_TRUE && objType == ReportSoGiaDinhLoaiNgay.HonPhoi)
            {
                if (nguoiChung != null && !string.IsNullOrEmpty(nguoiChung.ToString()))
                    infos.Add(nguoiChung.ToString());
                else
                    infos.Add(" ");
            }
            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNOISINH) == GxConstants.CF_TRUE)
            {
                if (noi != null && !string.IsNullOrEmpty(noi.ToString()))
                    infos.Add(noi.ToString());
                else
                    infos.Add(" ");
            }

            info = string.Join("\r\n", infos.ToArray()).Trim();
            return info;
        }

        public static int ExportExcel(DataSet ds)
        {
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(GiaDinhConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                string templatePath = "";
                string folder = Memory.GetConfig(GxConstants.CF_TEMPLATE_FOLDER);
                if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INCHAME) == GxConstants.CF_TRUE && folder != "Chung" && File.Exists(Memory.GetReportTemplatePath(GxConstants.REPORT_SOGIADINH1_FILENAME)))
                {
                    templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_SOGIADINH1_FILENAME);
                }
                else
                {
                    templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_SOGIADINH_FILENAME);
                }
                templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_SOGIADINH_FILENAME);
                string outputPath = Memory.GetTempPath(GxConstants.REPORT_SOGIADINH_FILENAME);

                ExcelEngine excel = new ExcelEngine();
                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaDinh = tblGiaDinh.Rows[0];

                if (excel.CreateObject(outputPath, templatePath))
                {
                    try
                    {
                        bool isMaGDRieng = (Memory.GetConfig(GxConstants.CF_TUNHAP_MAGIADINH) == "1");
                        excel.LoaiBaoCao = ReportType.SoGiaDinh;
                        excel.Write_to_excel(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu].ToString().ToUpper());
                        excel.Write_to_excel(GiaDinhConst.MaGiaDinh, isMaGDRieng ? rowGiaDinh[GiaDinhConst.MaGiaDinhRieng] : rowGiaDinh[GiaDinhConst.MaGiaDinh]);
                        excel.Write_to_excel(GiaDinhConst.TenGiaDinh, rowGiaDinh[GiaDinhConst.TenGiaDinh]);
                        excel.Write_to_excel(GiaoHoConst.TenGiaoHo, rowGiaDinh[GiaoHoConst.TenGiaoHo]);
                        //hiepdv begin add
                        excel.Write_to_excel(HonPhoiConst.CachThucHonPhoi, rowGiaDinh[HonPhoiConst.CachThucHonPhoi]);
                        excel.Write_to_excel(HonPhoiConst.NgayHonPhoi, rowGiaDinh[HonPhoiConst.NgayHonPhoi]);
                        excel.Write_to_excel(HonPhoiConst.SoHonPhoi, rowGiaDinh[HonPhoiConst.SoHonPhoi]);
                        excel.Write_to_excel(HonPhoiConst.NoiHonPhoi, rowGiaDinh[HonPhoiConst.NoiHonPhoi]);
                        //hiepdv end add
                        Dictionary<string, int> dicPos = getPositionsDic(excel, tblGiaoDan);
                        //hiepdv begin add
                        //check hide column
                        if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INCHAME) == GxConstants.CF_FALSE)
                        {
                            try
                            {
                                if(dicPos.ContainsKey("ChaMe"))
                                {
                                    excel.HideColumn(excel.MapColIndexToColName(dicPos["ChaMe"]));
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        //hiepdv end add
                        int maxColIndex = 0;
                        foreach (KeyValuePair<string, int> item in dicPos)
                        {
                            if (item.Value > maxColIndex) maxColIndex = item.Value;
                        }
                        string lasDetailColName= excel.MapColIndexToColName(maxColIndex);

                        int minColIndex = 20;
                        foreach (KeyValuePair<string, int> item in dicPos)
                        {
                            if (item.Value < minColIndex) minColIndex = item.Value;
                        }
                        string beginDetailColName = excel.MapColIndexToColName(minColIndex);

                        int i = beginDetailRow;
                        bool inNoiSinh = Memory.GetConfig(GxConstants.CF_SOGIADINH_INNOISINH) == GxConstants.CF_TRUE;

                        prepareThanhVienGiaDinhData(tblGiaoDan);

                        if (Memory.GetConfig(GxConstants.CF_SOGIADINH_THAYSTT_MAGIAODAN) == GxConstants.CF_TRUE)
                        {
                            excel.Write_to_excel(7, 2, "Mã GD");
                        }
                        string soHPChong = "";
                        foreach (DataRow row in tblGiaoDan.Rows)
                        {
                            //xem xet giao dan hien tai co cho phep in khong?
                            if ((bool)row[GiaoDanConst.QuaDoi] || row[GiaoDanConst.DaChuyenXu].ToString() != GxConstants.CF_FALSE)
                            {
                                if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LUUTRU) == GxConstants.CF_TRUE)
                                {
                                    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_GACHNGANG) == GxConstants.CF_TRUE)
                                    {
                                        System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Italic;
                                        System.Drawing.Font font = new System.Drawing.Font("Times New Roman", 9, fontStyle);
                                        if ((bool)row[GiaoDanConst.QuaDoi])
                                        {
                                            fontStyle = System.Drawing.FontStyle.Strikeout;
                                            font = new System.Drawing.Font("Times New Roman", 10, fontStyle);
                                        }
                                        excel.SetStrikeout("A" + i.ToString(), "Z" + i.ToString(), font);
                                    }
                                }
                                //else
                                //{
                                //    continue;
                                //}
                            }

                            if ((int)row[ThanhVienGiaDinhConst.VaiTro] > 1 && (bool)row[GiaoDanConst.DaCoGiaDinh])
                            {
                                if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LAPGD) == GxConstants.CF_TRUE)
                                {
                                    System.Drawing.Font font = new System.Drawing.Font("Times New Roman", 9, System.Drawing.FontStyle.Italic);

                                    excel.SetItalic("A" + i.ToString(), "Z" + i.ToString(), font);
                                }
                                //else
                                //{
                                //    continue;
                                //}
                            }

                            #region For nhung thuoc tinh dac biet, co nhieu truong hop
                            //if (dicPos.ContainsKey(GiaoDanConst.MaGiaoDan))
                            //{
                            //    if (Memory.GetConfig(GxConstants.CF_SOGIADINH_THAYSTT_MAGIAODAN) == GxConstants.CF_TRUE)
                            //    {
                            //        row[GiaoDanConst.MaGiaoDan] = row[GiaoDanConst.MaGiaoDan];
                            //    }
                            //    else
                            //    {
                            //        row[GiaoDanConst.MaGiaoDan] = (i - beginDetailRow + 1);
                            //    }
                            //}

                            //if (dicPos.ContainsKey(HonPhoiConst.NgayHonPhoi))
                            //{
                            //    //for hon phoi
                            //    if (!Memory.IsNullOrEmpty(row[HonPhoiConst.NgayHonPhoi]))
                            //    {
                            //    }
                            //    else
                            //    {
                            //        if ((bool)row[GiaoDanConst.DaCoGiaDinh])
                            //        {
                            //            //honPhoi = "X";
                            //            row[HonPhoiConst.NgayHonPhoi] = "X";
                            //        }
                            //    }
                            //}

                            //if (dicPos.ContainsKey(GiaoDanConst.NgayQuaDoi))
                            //{
                            //    //for qua doi
                            //    if (!Memory.IsNullOrEmpty(row[GiaoDanConst.NgayQuaDoi]))
                            //    {
                            //    }
                            //    else
                            //    {
                            //        if ((bool)row[GiaoDanConst.QuaDoi])
                            //        {
                            //            row[GiaoDanConst.NgayQuaDoi] = "X";
                            //        }
                            //    }
                            //}
                            //if (dicPos.ContainsKey(GiaoDanConst.HoTen))
                            //{
                            //    if (lang == GxConstants.LANG_EN && isUSFormatName)
                            //    {
                            //        row[GiaoDanConst.HoTen] = Memory.GetHoTenByLangKhongTenThanh(row[GiaoDanConst.HoTen].ToString(), lang);
                            //    }
                            //}

                            //if (inNoiSinh)
                            //{
                            //    row[GiaoDanConst.NgaySinh] = getNgayVaNoi(row[GiaoDanConst.NgaySinh], row[GiaoDanConst.NoiSinh]);
                            //    row[GiaoDanConst.NgayRuaToi] = getNgayVaNoi(row[GiaoDanConst.NgayRuaToi], row[GiaoDanConst.NoiRuaToi]);
                            //    row[GiaoDanConst.NgayRuocLe] = getNgayVaNoi(row[GiaoDanConst.NgayRuocLe], row[GiaoDanConst.NoiRuocLe]);
                            //    row[GiaoDanConst.NgayThemSuc] = getNgayVaNoi(row[GiaoDanConst.NgayThemSuc], row[GiaoDanConst.NoiThemSuc]);
                            //    row[HonPhoiConst.NgayHonPhoi] = getNgayVaNoi(row[HonPhoiConst.NgayHonPhoi], row[HonPhoiConst.NoiHonPhoi]);
                            //}

                            row[GiaoDanConst.NgaySinh] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.SinhRa, row[GiaoDanConst.NgaySinh], row[GiaoDanConst.NoiSinh], "", "", "");
                            row[GiaoDanConst.NgayRuaToi] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.RuaToi, row[GiaoDanConst.NgayRuaToi], row[GiaoDanConst.NoiRuaToi], row[GiaoDanConst.SoRuaToi], row[GiaoDanConst.NguoiDoDauRuaToi], "");
                            row[GiaoDanConst.NgayRuocLe] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.XTRL, row[GiaoDanConst.NgayRuocLe], row[GiaoDanConst.NoiRuocLe], row[GiaoDanConst.SoRuaToi], "", "");
                            row[GiaoDanConst.NgayThemSuc] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.ThemSuc, row[GiaoDanConst.NgayThemSuc], row[GiaoDanConst.NoiThemSuc], row[GiaoDanConst.SoThemSuc], row[GiaoDanConst.NguoiDoDauThemSuc], "");
                            string nguoiChung = GetStringFromArray(row[HonPhoiConst.NguoiChung1].ToString(), row[HonPhoiConst.NguoiChung2].ToString());
                            row[HonPhoiConst.NgayHonPhoi] = getNgayVaNoi(ReportSoGiaDinhLoaiNgay.HonPhoi, row[HonPhoiConst.NgayHonPhoi], row[HonPhoiConst.NoiHonPhoi], row[HonPhoiConst.SoHonPhoi], "", nguoiChung);

                            #endregion
                            //Xử lý dữ liệu cho phép in thêm nơi của các ngày
                            //if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNOISINH) == GxConstants.CF_TRUE)
                            //{
                            //    row[GiaoDanConst.NgaySinh]    = row[GiaoDanConst.NgaySinh]    + "\n" + row[GiaoDanConst.NoiSinh];
                            //    row[GiaoDanConst.NgayQuaDoi]  = row[GiaoDanConst.NgayQuaDoi]  + "\n" + row[GiaoDanConst.NoiQuaDoi];
                            //    row[GiaoDanConst.NgayRuaToi]  = row[GiaoDanConst.NgayRuaToi]  + "\n" + row[GiaoDanConst.NoiRuaToi];
                            //    row[GiaoDanConst.NgayRuocLe]  = row[GiaoDanConst.NgayRuocLe]  + "\n" + row[GiaoDanConst.NoiRuocLe];
                            //    row[GiaoDanConst.NgayThemSuc] = row[GiaoDanConst.NgayThemSuc] + "\n" + row[GiaoDanConst.NoiThemSuc];
                            //}

                            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNGUOICHUNGHP) == GxConstants.CF_TRUE
                                && !Memory.IsNullOrEmpty(row[HonPhoiConst.NgayHonPhoi]))
                            {
                                row[HonPhoiConst.NgayHonPhoi] = string.Format("{0}\r\n{1}{2}", row[HonPhoiConst.NgayHonPhoi],
                                                    row[HonPhoiConst.NguoiChung1].ToString() != "" ? row[HonPhoiConst.NguoiChung1].ToString() + "\r\n" : "",
                                                    row[HonPhoiConst.NguoiChung2].ToString() != "" ? row[HonPhoiConst.NguoiChung2].ToString() + "\r\n" : "");
                            }

                            //bắt đầu chèn 1 dòng
                            foreach (KeyValuePair<string, int> item in dicPos)
                            {
                                string key = item.Key.ToLower();
                                //if (key.StartsWith("ngay"))
                                //{
                                //    if (inNoiSinh &&
                                //            (key == GiaoDanConst.NgaySinh.ToLower() || key == GiaoDanConst.NgayRuaToi.ToLower() || key == GiaoDanConst.NgayRuocLe.ToLower()
                                //            || key == GiaoDanConst.NgayThemSuc.ToLower() || key == HonPhoiConst.NgayHonPhoi.ToLower()))
                                //    {
                                //        excel.Write_to_excel(i, item.Value, row[item.Key]);
                                //    }
                                //    else
                                //    {
                                //        row[item.Key] = Memory.GetDateStringByLang(row[item.Key]);
                                //        excel.Write_to_excel(i, item.Value, "'" + row[item.Key].ToString());
                                //    }

                                //}
                                //else
                                //{
                                //    if (Validator.IsNumber(row[item.Key].ToString()))
                                //    {
                                //        excel.Write_to_excel(i, item.Value, "'" + row[item.Key].ToString());
                                //    }
                                //    else
                                //    {
                                //        excel.Write_to_excel(i, item.Value, row[item.Key]);
                                //    }
                                //}

                                if (Validator.IsNumber(row[item.Key].ToString()) || item.Key.StartsWith("ngay",StringComparison.OrdinalIgnoreCase))
                                {
                                    excel.Write_to_excel(i, item.Value, "'" + row[item.Key].ToString());
                                }
                                else
                                {
                                    excel.Write_to_excel(i, item.Value, row[item.Key]);
                                }
                            }
                            //chèn xong 1 dòng
                            if((bool)row[GiaoDanConst.DaCoGiaDinh] && (int)row[ThanhVienGiaDinhConst.VaiTro] == 0)
                            {
                                soHPChong = row[HonPhoiConst.SoHonPhoi].ToString();
                            }
                            
                            if ((bool)row[GiaoDanConst.DaCoGiaDinh] 
                                && Memory.GetConfig(GxConstants.CF_SOGIADINH_INNGUOICHUNGHP) == GxConstants.CF_TRUE
                                && (int)row[ThanhVienGiaDinhConst.VaiTro] == 1 && soHPChong == row[HonPhoiConst.SoHonPhoi].ToString()
                                && (i - beginDetailRow) == 1
                                && dicPos.ContainsKey(HonPhoiConst.NgayHonPhoi))
                            {
                                int colIndexHP = dicPos[HonPhoiConst.NgayHonPhoi];
                                string columnNameHP = excel.MapColIndexToColName(colIndexHP);
                                excel.Merge(string.Concat(columnNameHP, (i - 1).ToString()), string.Concat(columnNameHP, i.ToString()));
                                excel.SetVerticalAlignment(string.Concat(columnNameHP, (i - 1).ToString()), string.Concat(columnNameHP, i.ToString()), Excel.XlVAlign.xlVAlignCenter);
                                excel.AutoFitRow(i - 1, i-1);
                                excel.AutoFitCol(colIndexHP, colIndexHP);
                            }
                            if (inNoiSinh && (!Memory.IsNullOrEmpty(row[GiaoDanConst.NoiSinh]) 
                                                || !Memory.IsNullOrEmpty(row[GiaoDanConst.NoiRuaToi]) || !Memory.IsNullOrEmpty(row[GiaoDanConst.NoiRuocLe])
                                                || !Memory.IsNullOrEmpty(row[GiaoDanConst.NoiThemSuc]) || !Memory.IsNullOrEmpty(row[HonPhoiConst.NoiHonPhoi]))
                               || (Memory.GetConfig(GxConstants.CF_SOGIADINH_INNGUOICHUNGHP) == GxConstants.CF_TRUE
                                        && !Memory.IsNullOrEmpty(row[HonPhoiConst.NgayHonPhoi]))
                                                )
                            {
                                excel.AutoFitRow(i, i);
                            }

                            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_INCHAME) == GxConstants.CF_TRUE
                                && dicPos.ContainsKey("ChaMe"))
                            {
                                int colIndexChaMe = dicPos["ChaMe"];
                                excel.AutoFitCol(colIndexChaMe, colIndexChaMe);
                            }

                            i++;
                            //Thực hiện insert thêm 1 dòng
                            excel.Range_Insert(i, 1);
                        }
                        //if(i - beginDetailRow < 8)
                        //{
                        //    i = beginDetailRow + 8;
                        //}
                        //luon dong khung 10 thanh vien
                        //neu chua du thi them cho du dong
                        //bat dau tinh toan
                        //int dis = i - beginDetailRow;
                        //dis = 10 - dis;
                        //i = i + dis;
                        //ket thuc tinh toan
                        excel.Border_Range(beginDetailColName + beginDetailRow.ToString(), lasDetailColName + (i-1).ToString());  //tô viền cho row mới chèn xong
                     
                        excel.Write_to_excel(GiaDinhConst.DienThoai, "'" + rowGiaDinh[GiaDinhConst.DienThoai].ToString());

                        excel.Write_to_excel(GiaDinhConst.DiaChi, "'" + rowGiaDinh[GiaDinhConst.DiaChi]);

                        //i++;
                        if (rowGiaDinh[GiaDinhConst.GhiChu].ToString().Trim() != "")
                        {
                            if (Memory.GetConfig(GxConstants.CF_LANGUAGE) == GxConstants.LANG_EN)
                            {
                                excel.Write_to_excel(i, minColIndex, "Note: " + rowGiaDinh[GiaDinhConst.GhiChu].ToString());
                            }
                            else
                            {
                                excel.Write_to_excel(i, minColIndex, "Ghi chú: " + rowGiaDinh[GiaDinhConst.GhiChu].ToString());
                            }
                        }
                        //else
                        //{
                        //    if (Memory.GetConfig(GxConstants.CF_LANGUAGE) == GxConstants.LANG_EN)
                        //    {
                        //        excel.Write_to_excel(i, minColIndex, "Note: ");
                        //    }
                        //    else
                        //    {
                        //        excel.Write_to_excel(i, minColIndex, "Ghi chú: ");
                        //    }
                        //}
                        //excel.SetPrintArea("$" + beginDetailColName + "$" + i.ToString(), "$" + lasDetailColName + "$" + i.ToString());
                        //excel.SetWrapText(beginDetailColName + i.ToString(), lasDetailColName + i.ToString(), true);

                        //excel.SetAlignment(beginDetailColName + i.ToString(), lasDetailColName + i.ToString(), XlHAlign.xlHAlignLeft);

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
                    Memory.Instance.Error = new Exception("Xuất giới thiệu thất bại." + Environment.NewLine +
                                                            "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                            "Có thể do tập tin \"SoGiaDinh.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
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

        public static string GetStringFromArray(string separator, params string[] ss)
        {
            List<string> list = new List<string>();
            foreach(string s in ss)
            {
                if(!string.IsNullOrEmpty(s))
                {
                    list.Add(s);
                }
            }

            return string.Join(separator, list.ToArray()).Trim();
        }

        //2018-07-31 Gia modify start
        public static WordEngine Export(DataSet ds, WordEngine mainDoc = null)
        {
            vaiTro = Memory.GetQuanHeList(true);
            WordEngine word = null;
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(GiaDinhConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return null;
                }
                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return null;

                string templatePath = Memory.GetReportTemplatePath(FileName);
                string outputPath = Memory.GetTempPath(FileName);

                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowGiaDinh = tblGiaDinh.Rows[0];

                string reportFormat = Memory.GetReportFormat();
                templatePath = string.Concat(templatePath, reportFormat);
                outputPath = string.Concat(outputPath, reportFormat);
                word = new WordEngine();
                if (reportFormat == GxConstants.DOC_FORMAT && word.CreateObject(outputPath, templatePath))
                {
                    // Chu Ho
                    //var chuHo = GetChuHo(tblGiaoDan);
                    //word.Replace(GetName(ThanhVienGiaDinhConst.VaiTro, 0), "Chủ hộ");
                    SetTableFimaly(tblGiaoDan, word);
                    SetInfor(rowGiaoXu, rowGiaDinh, word);
                    //  SetReplace(tblGiaoXuNhan, tblGiaoXu, tblGiaoDan);

                    //2018-07-31 Gia modify start
                    if (mainDoc == null)
                    {
                        //System.Diagnostics.Process.Start(outputPath);
                        word.Save();
                        //word.AllowVisible = true;
                    }
                    else
                    {
                        word.SelectAll();
                        word.Copy();
                        mainDoc.Paste();
                        mainDoc.Save();
                        word.End_Write();
                        System.IO.File.Delete(outputPath);
                    }
                    //2018-07-31 Gia modify end

                    //System.Diagnostics.Process.Start(outputPath);
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                //word = null;
                return null;
            }
            finally
            {
                if (word != null && mainDoc != null && System.IO.File.Exists(word.FileName))
                {
                    word.End_Write();
                }
            }
            return word;
        }

        public static DataRow GetChuHo(DataTable tbl)
        {
            foreach (var row in tbl.Rows)
            {
                var dataRow = (row as DataRow);
                if (dataRow[ThanhVienGiaDinhConst.VaiTro].Equals(true)) return dataRow;
            }
            return null;
        }
        private static Dictionary<string, int> getPositionsDic(ExcelEngine excel, DataTable tbl)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();

            if (excel != null && tbl != null)
            {
                foreach (DataColumn col in tbl.Columns)
                {
                    if (col.ColumnName == GiaoDanConst.DienThoai || col.ColumnName == GiaDinhConst.MaGiaDinh || col.ColumnName == GiaoDanConst.DiaChi) continue;
                    int idx = excel.FindColIndex(string.Format("[{0}]", col.ColumnName));
                    if (idx > -1)
                    {
                        if (beginDetailRow == -1)
                        {
                            beginDetailRow = excel.FindRowIndex(string.Format("[{0}]", col.ColumnName));
                        }
                        dic.Add(col.ColumnName, idx);
                    }
                }
            }
            return dic;
        }
        
        public static string GetName(string constan, int number)
        {
            return string.Concat(constan, number);
        }
        public static void SetTableFimaly(DataTable tbl, WordEngine word)
        {
            //word.CopyTableRows(1, 2, 1);
            //return;
            //word.CopyTableRowToClipboard(1, 2);
            //word.DeleteRow(1, 2);
            //word.AddRow(1);
            int i = 0;
            bool writeMaGiaoDan = Memory.GetConfig(GxConstants.CF_SOGIADINH_THAYSTT_MAGIAODAN) == GxConstants.CF_TRUE;
            prepareThanhVienGiaDinhData(tbl);
            const int ROW_HOLDER = 8;
            //const int ROW_MAX_TEMPLATE = 8;
            //index of table in document
            int indexTable = 1;

            int maxRow = (tbl.Rows.Count < ROW_HOLDER ? ROW_HOLDER : tbl.Rows.Count);

            #region không xóa dòng template dư

            //if (ROW_MAX_TEMPLATE > maxRow)
            //{
            //    for (int j = ROW_MAX_TEMPLATE; j > maxRow; j--)
            //    {
            //        word.DeleteRow(indexTable, j + 1);
            //    }
            //    //word.FormatTable(indexTable, Microsoft.Office.Interop.Word.WdPaperSize.wdPaperA4, Microsoft.Office.Interop.Word.WdOrientation.wdOrientLandscape);
            //}
            #endregion

            int maxTVGD = tbl.Rows.Count;
            //check số thành viên cần in
            if (maxTVGD > ROW_HOLDER) 
            {
                //Tiến hành add thêm row. số row cần add = maxrow - row_holder
                word.AddRowColumnFirstSTT(maxTVGD - ROW_HOLDER, indexTable);
                word.FormatTable(indexTable);
            }
            for (i = 0; i < maxRow; i++)
            {
                var tableRow = word.GetTableRow(indexTable, i + 2);
                if (tableRow != null)
                    if (i < tbl.Rows.Count)
                    {
                        //continue;
                        var row = (tbl.Rows[i] as DataRow);
                        //word.PasteRow(1);

                        //if (writeMaGiaoDan)
                        //{
                        //    word.ReplaceOne(tableRow.Range, GetName("STT", i), row[GiaoDanConst.MaGiaoDan]);
                        //}
                        //else
                        //{
                        //    word.ReplaceOne(tableRow.Range, GetName("STT", i), i + 1);
                        //}
                        word.ReplaceOne(tableRow.Range, GetName("STT", i), row[GiaoDanConst.MaGiaoDan]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.TenThanh, i), row[GiaoDanConst.TenThanh]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.HoTen, i), row[GiaoDanConst.HoTen]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgaySinh, i), row[GiaoDanConst.NgaySinh]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiSinh, i), row[GiaoDanConst.NoiSinh]);
                        //2020-05-12 hiepdv begin add
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.Phai, i), row[GiaoDanConst.Phai]);
                        //2020-05-12 hiepdv end add
                        if ((bool)row[ThanhVienGiaDinhConst.ChuHo] == true)
                        {
                            word.ReplaceOne(tableRow.Range, GetName(ThanhVienGiaDinhConst.VaiTro, i), "Chủ hộ");
                        }
                        else
                        {
                            word.ReplaceOne(tableRow.Range, GetName(ThanhVienGiaDinhConst.VaiTro, i), vaiTro[(int)row[ThanhVienGiaDinhConst.VaiTro]]);
                        }
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoRuaToi, i), row[GiaoDanConst.SoRuaToi]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiRuaToi, i), row[GiaoDanConst.NoiRuaToi]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayRuaToi, i), row[GiaoDanConst.NgayRuaToi]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoRuocLe, i), row[GiaoDanConst.SoRuocLe]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiRuocLe, i), row[GiaoDanConst.NoiRuocLe]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayRuocLe, i), row[GiaoDanConst.NgayRuocLe]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoThemSuc, i), row[GiaoDanConst.SoThemSuc]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiThemSuc, i), row[GiaoDanConst.NoiThemSuc]);
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayThemSuc, i), row[GiaoDanConst.NgayThemSuc]);
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.SoHonPhoi, i), row[HonPhoiConst.SoHonPhoi]);
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.NoiHonPhoi, i), row[HonPhoiConst.NoiHonPhoi]);
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.NgayHonPhoi, i), row[HonPhoiConst.NgayHonPhoi]);
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.GhiChu, i), row[HonPhoiConst.GhiChu]);

                        //xem xet giao dan hien tai co cho phep in khong?
                        if ((bool)row[GiaoDanConst.QuaDoi] || row[GiaoDanConst.DaChuyenXu].ToString() != GxConstants.CF_FALSE)
                        {
                            if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_LUUTRU) == GxConstants.CF_TRUE)
                            {
                                if (Memory.GetConfig(GxConstants.CF_SOGIADINH_IN_GACHNGANG) == GxConstants.CF_TRUE)
                                {
                                    if ((bool)row[GiaoDanConst.QuaDoi])
                                    {
                                        word.StrikeThroughRow(indexTable, i + 2);
                                    }
                                    else
                                    {
                                        word.ItalicRow(indexTable, i + 2);
                                    }
                                }
                            }
                        }
                        if ((int)row[ThanhVienGiaDinhConst.VaiTro] > 1 && (bool)row[GiaoDanConst.DaCoGiaDinh])
                        {
                            word.ItalicRow(indexTable, i + 2);
                        }

                        //if(j < tbl.Rows.Count - 1)
                        //{
                        //    word.PasteRow(1);
                        //}
                    }
                    else
                    {
                        //word.AddRow(1);
                        word.ReplaceOne(tableRow.Range, GetName("STT", i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.TenThanh, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.HoTen, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgaySinh, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiSinh, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(ThanhVienGiaDinhConst.VaiTro, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoRuaToi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiRuaToi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayRuaToi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoRuocLe, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiRuocLe, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayRuocLe, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.SoThemSuc, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NoiThemSuc, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.NgayThemSuc, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.SoHonPhoi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.NoiHonPhoi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.NgayHonPhoi, i), "");
                        word.ReplaceOne(tableRow.Range, GetName(HonPhoiConst.GhiChu, i), "");
                        //2020-05-12 hiepdv begin add
                        word.ReplaceOne(tableRow.Range, GetName(GiaoDanConst.Phai, i), "");
                        //2020-05-12 hiepdv end add
                    }

            }

            if (writeMaGiaoDan)
            {
                word.WriteTextInTable(indexTable, 1, 1, "Mã GD");
            }
            else
            {
                word.WriteTextInTable(indexTable, 1, 1, "STT");
            }
        }
        public static void SetInfor(DataRow rowGiaoXu, DataRow rowGiaDinh, WordEngine word)
        {
            word.Replace(GiaoXuConst.TenGiaoXu, ((string)rowGiaoXu[GiaoXuConst.TenGiaoXu]).ToUpper());
            word.Replace(GiaDinhConst.TenGiaDinh, rowGiaDinh[GiaDinhConst.TenGiaDinh]);
            word.Replace(GiaDinhConst.MaGiaDinh, rowGiaDinh[GiaDinhConst.MaGiaDinh]);
            word.Replace(GiaDinhConst.DiaChi, rowGiaDinh[GiaDinhConst.DiaChi]);
            word.Replace(GiaDinhConst.DienThoai, rowGiaDinh[GiaDinhConst.DienThoai]);
            word.Replace(GiaDinhConst.TenGiaoHo, rowGiaDinh[GiaDinhConst.TenGiaoHo]);
            
            //hiepdv begin add
            word.Replace(HonPhoiConst.CachThucHonPhoi, rowGiaDinh[HonPhoiConst.CachThucHonPhoi]);
            word.Replace(HonPhoiConst.NgayHonPhoi, rowGiaDinh[HonPhoiConst.NgayHonPhoi]);
            word.Replace(HonPhoiConst.SoHonPhoi, rowGiaDinh[HonPhoiConst.SoHonPhoi]);
            word.Replace(HonPhoiConst.NoiHonPhoi, rowGiaDinh[HonPhoiConst.NoiHonPhoi]);
            //if(rowGiaDinh[GiaDinhConst.GhiChu].ToString() != "")
            //{
            //    string ghiChu = string.Format("Ghi chú: {0}", rowGiaDinh[GiaDinhConst.GhiChu]);
            //    word.Replace(GiaDinhConst.GhiChu, ghiChu);
            //}
            //else
            //{
            //    word.Replace(GiaDinhConst.GhiChu, "");
            //}
            //hiepdv end add
        }
    }
}
