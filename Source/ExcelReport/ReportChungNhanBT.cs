using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportChungNhanBT
    {
        public static int Export(DataSet ds)
        {
            ExcelEngine excel = null;
            WordEngine word = null;
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaoXuNhan = ds.Tables.Contains(ReportChungNhanBTConst.TableName) ? ds.Tables[ReportChungNhanBTConst.TableName] : null;
                //DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                LoaiBiTich loaiBiTich = Memory.Instance.GetMemory(GxConstants.CURRENT_REPORT) == null ? LoaiBiTich.TatCa : (LoaiBiTich)Memory.Instance.GetMemory(GxConstants.CURRENT_REPORT);
                string fileName = "";
                switch (loaiBiTich)
                {
                    case LoaiBiTich.RuaToi:
                        fileName = GxConstants.REPORT_RUATOI_FILENAME;
                        break;
                    case LoaiBiTich.RuocLe:
                        fileName = GxConstants.REPORT_XTRL_FILENAME;
                        break;
                    case LoaiBiTich.ThemSuc:
                        fileName = GxConstants.REPORT_THEMSUC_FILENAME;
                        break;
                    default:
                        fileName = GxConstants.REPORT_BITICH_FILENAME;
                        break;
                }

                string templatePath = Memory.GetReportTemplatePath(fileName);
                string outputPath = Memory.GetTempPath(fileName);
                
                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowHonPhoi = ds.Tables.Contains(HonPhoiConst.TableName) ? ds.Tables[HonPhoiConst.TableName].Rows[0] : null;
                DataRow rowGiaoXuNhan = tblGiaoXuNhan != null && tblGiaoXuNhan.Rows.Count != 0 ? tblGiaoXuNhan.Rows[0] : null;

                string reportFormat = Memory.GetReportFormat();
                templatePath = string.Concat(templatePath, reportFormat);
                outputPath = string.Concat(outputPath, reportFormat);
                string defaultValue = ".................................";
                string defaultValue1 = ".............";
                if (reportFormat == GxConstants.DOC_FORMAT)
                {
                    word = new WordEngine();
                    if (word.CreateObject(outputPath, templatePath))
                    {
                        try
                        {
                            word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                            word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                            word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                            word.Replace(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                            word.Replace(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                            word.Replace(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                            word.Replace(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);

                            word.Replace(GiaoHoConst.TenGiaoHo, rowGiaoDan[GiaoHoConst.TenGiaoHo]);
                            word.Replace("TenGiaoHoCha", rowGiaoDan["TenGiaoHoCha"]);
                            word.Replace(ReportChungNhanBTConst.TenLinhMucGui, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenLinhMucGui] : rowGiaoXu[ReportGiaoDanConst.TenLinhMucGui], defaultValue);

                            word.Replace(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString(), defaultValue);
                            word.Replace(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh], defaultValue);
                            word.Replace(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh], defaultValue);

                            word.Replace(ReportGiaoDanConst.TenCha, rowGiaoDan[GiaoDanConst.HoTenCha], defaultValue);
                            word.Replace(ReportGiaoDanConst.TenMe, rowGiaoDan[GiaoDanConst.HoTenMe], defaultValue);

                            word.Replace(GiaoDanConst.NgayRuaToi, rowGiaoDan[GiaoDanConst.NgayRuaToi], defaultValue);
                            if (rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Length >= 4)
                            {
                                word.Replace("NamRuaToi", rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Length - 4), defaultValue);
                            }
                            word.Replace(GiaoDanConst.NoiRuaToi, rowGiaoDan[GiaoDanConst.NoiRuaToi], defaultValue);
                            word.Replace(GiaoDanConst.ChaRuaToi, rowGiaoDan[GiaoDanConst.ChaRuaToi], defaultValue);
                            word.Replace(GiaoDanConst.NguoiDoDauRuaToi, rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi], defaultValue);
                            word.Replace(GiaoDanConst.SoRuaToi, rowGiaoDan[GiaoDanConst.SoRuaToi], defaultValue1);
                            word.Replace(GiaoDanConst.NgayThemSuc, rowGiaoDan[GiaoDanConst.NgayThemSuc], defaultValue);
                            if (rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Length >= 4)
                            {
                                word.Replace("NamThemSuc", rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Length - 4), defaultValue);
                            }
                            word.Replace(GiaoDanConst.NoiThemSuc, rowGiaoDan[GiaoDanConst.NoiThemSuc], defaultValue);
                            word.Replace(GiaoDanConst.ChaThemSuc, rowGiaoDan[GiaoDanConst.ChaThemSuc], defaultValue);
                            word.Replace(GiaoDanConst.NguoiDoDauThemSuc, rowGiaoDan[GiaoDanConst.NguoiDoDauThemSuc], defaultValue);
                            word.Replace(GiaoDanConst.SoThemSuc, rowGiaoDan[GiaoDanConst.SoThemSuc], defaultValue1);
                            word.Replace(GiaoDanConst.SoRuocLe, rowGiaoDan[GiaoDanConst.SoRuocLe], defaultValue1);
                            word.Replace(GiaoDanConst.NgayRuocLe, rowGiaoDan[GiaoDanConst.NgayRuocLe], defaultValue);
                            if (rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Length >= 4)
                            {
                                word.Replace("NamRuocLe", rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Length - 4), defaultValue);
                            }
                            word.Replace(GiaoDanConst.NoiRuocLe, rowGiaoDan[GiaoDanConst.NoiRuocLe], defaultValue);
                            word.Replace(GiaoDanConst.ChaRuocLe, rowGiaoDan[GiaoDanConst.ChaRuocLe], defaultValue);

                            word.Replace(HonPhoiConst.SoHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.SoHonPhoi] : "", defaultValue1);
                            word.Replace(HonPhoiConst.NgayHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NgayHonPhoi] : "", defaultValue);
                            if (rowHonPhoi != null && rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length >= 4)
                            {
                                word.Replace("NamHonPhoi", rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Substring(rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length - 4), defaultValue);
                            }
                            word.Replace(HonPhoiConst.NoiHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NoiHonPhoi] : "", defaultValue);
                            word.Replace("ChaHonPhoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.LinhMucChung] : "", defaultValue);
                            word.Replace(HonPhoiConst.CachThucHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.CachThucHonPhoi] : "", defaultValue);
                            word.Replace(HonPhoiConst.VoChong, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.VoChong] : "", defaultValue);
                            word.Replace("GhiChuHonPHoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.GhiChu] : "", defaultValue);
                            word.Replace("NgaySinhVoChong", rowHonPhoi != null ? rowHonPhoi["NgaySinhVoChong"] : "", defaultValue);

                            word.Replace(ReportChungNhanBTConst.LyDo, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.LyDo] : "");
                            word.Replace(ReportChungNhanBTConst.TenLinhMucNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenLinhMucNhan] : ".................................");
                            word.Replace(ReportChungNhanBTConst.TenGiaoXuNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenGiaoXuNhan] : ".................................");
                            word.Replace(ReportChungNhanBTConst.TenGiaoPhanNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenGiaoPhanNhan] : ".................................");

                            if (Memory.GetConfig(GxConstants.CF_LANGUAGE).ToString() == GxConstants.LANG_EN)
                            {
                                word.Replace(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamEn());
                            }
                            else
                            {
                                word.Replace(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamVn());
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
                        Memory.Instance.Error = new Exception("Xuất giới thiệu thất bại." + Environment.NewLine +
                                                                "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                                "Có thể do tập tin \"BiTich.doc\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                                "Xin vui lòng đóng tập tin này và thử lại lần nữa");
                        return -1;
                    }
                }
                else
                {
                    excel = new ExcelEngine();
                    if (excel.CreateObject(outputPath, templatePath))
                    {
                        try
                        {
                            excel.Write_to_excel(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                            excel.Write_to_excel(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                            excel.Write_to_excel(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                            excel.Write_to_excel(ReportChungNhanBTConst.TenLinhMucGui, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenLinhMucGui] : rowGiaoXu[ReportGiaoDanConst.TenLinhMucGui]);
                            excel.Write_to_excel(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString());
                            excel.Write_to_excel(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh]);
                            excel.Write_to_excel(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh]);

                            excel.Write_to_excel(ReportGiaoDanConst.TenCha, rowGiaoDan[GiaoDanConst.HoTenCha]);
                            excel.Write_to_excel(ReportGiaoDanConst.TenMe, rowGiaoDan[GiaoDanConst.HoTenMe]);

                            excel.Write_to_excel(GiaoDanConst.NgayRuaToi, rowGiaoDan[GiaoDanConst.NgayRuaToi]);
                            excel.Write_to_excel(GiaoDanConst.NoiRuaToi, rowGiaoDan[GiaoDanConst.NoiRuaToi]);
                            excel.Write_to_excel(GiaoDanConst.ChaRuaToi, rowGiaoDan[GiaoDanConst.ChaRuaToi]);
                            excel.Write_to_excel(GiaoDanConst.NguoiDoDauRuaToi, rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi]);
                            excel.Write_to_excel(GiaoDanConst.SoRuaToi, rowGiaoDan[GiaoDanConst.SoRuaToi]);
                            excel.Write_to_excel(GiaoDanConst.NgayThemSuc, rowGiaoDan[GiaoDanConst.NgayThemSuc]);
                            excel.Write_to_excel(GiaoDanConst.NoiThemSuc, rowGiaoDan[GiaoDanConst.NoiThemSuc]);
                            excel.Write_to_excel(GiaoDanConst.ChaThemSuc, rowGiaoDan[GiaoDanConst.ChaThemSuc]);
                            excel.Write_to_excel(GiaoDanConst.NguoiDoDauThemSuc, rowGiaoDan[GiaoDanConst.NguoiDoDauThemSuc]);
                            excel.Write_to_excel(GiaoDanConst.SoThemSuc, rowGiaoDan[GiaoDanConst.SoThemSuc]);
                            excel.Write_to_excel(GiaoDanConst.SoRuocLe, rowGiaoDan[GiaoDanConst.SoRuocLe]);
                            excel.Write_to_excel(GiaoDanConst.NgayRuocLe, rowGiaoDan[GiaoDanConst.NgayRuocLe]);
                            excel.Write_to_excel(GiaoDanConst.NoiRuocLe, rowGiaoDan[GiaoDanConst.NoiRuocLe]);
                            excel.Write_to_excel(GiaoDanConst.ChaRuocLe, rowGiaoDan[GiaoDanConst.ChaRuocLe]);

                            excel.Write_to_excel(HonPhoiConst.SoHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.SoHonPhoi] : "");
                            excel.Write_to_excel(HonPhoiConst.NgayHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NgayHonPhoi] : "");
                            excel.Write_to_excel(HonPhoiConst.NoiHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NoiHonPhoi] : "");
                            excel.Write_to_excel("ChaHonPhoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.LinhMucChung] : "");
                            excel.Write_to_excel(HonPhoiConst.CachThucHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.CachThucHonPhoi] : "");
                            excel.Write_to_excel(HonPhoiConst.VoChong, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.VoChong] : "");
                            excel.Write_to_excel("GhiChuHonPHoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.GhiChu] : "");
                            excel.Write_to_excel("NgaySinhVoChong", rowHonPhoi != null ? rowHonPhoi["NgaySinhVoChong"] : "");

                            if (rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Length >= 4)
                            {
                                excel.Write_to_excel("NamRuaToi", rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString().Length - 4));
                            }
                            if (rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Length >= 4)
                            {
                                excel.Write_to_excel("NamThemSuc", rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayThemSuc].ToString().Length - 4));
                            }
                            if (rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Length >= 4)
                            {
                                excel.Write_to_excel("NamRuocLe", rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Substring(rowGiaoDan[GiaoDanConst.NgayRuocLe].ToString().Length - 4));
                            }
                            if (rowHonPhoi != null && rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length >= 4)
                            {
                                excel.Write_to_excel("NamHonPhoi", rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Substring(rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length - 4));
                            }

                            excel.Write_to_excel(ReportChungNhanBTConst.LyDo, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.LyDo] : "");
                            excel.Write_to_excel(ReportChungNhanBTConst.TenLinhMucNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenLinhMucNhan] : "");
                            excel.Write_to_excel(ReportChungNhanBTConst.TenGiaoXuNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenGiaoXuNhan] : "");
                            excel.Write_to_excel(ReportChungNhanBTConst.TenGiaoPhanNhan, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportChungNhanBTConst.TenGiaoPhanNhan] : "");

                            if (Memory.GetConfig(GxConstants.CF_LANGUAGE).ToString() == GxConstants.LANG_EN)
                            {
                                excel.Write_to_excel(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamEn());
                            }
                            else
                            {
                                excel.Write_to_excel(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamVn());
                            }
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
                                                                "Có thể do tập tin \"BiTich.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                                "Xin vui lòng đóng tập tin này và thử lại lần nữa");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return -1;
            }
            finally
            {
                if (excel != null) excel.End_Write();
                if (word != null) word.End_Write();
            }
            return 0;
        }
    }
}
