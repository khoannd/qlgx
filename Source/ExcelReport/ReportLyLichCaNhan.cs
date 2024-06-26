using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportLyLichCaNhan
    {
        public static WordEngine Export(DataSet ds, string vaiTro, WordEngine mainDoc)
        {
            WordEngine word = null;
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return null;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaoXuNhan = ds.Tables.Contains(ReportChungNhanBTConst.TableName) ? ds.Tables[ReportChungNhanBTConst.TableName] : null;
                //DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];

                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0)
                {
                    return null;
                }
                LoaiBiTich loaiBiTich = Memory.Instance.GetMemory(GxConstants.CURRENT_REPORT) == null ? LoaiBiTich.TatCa : (LoaiBiTich)Memory.Instance.GetMemory(GxConstants.CURRENT_REPORT);
                string fileName = GxConstants.REPORT_LYLICH_CANHAN_FILENAME;

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
                word = new WordEngine();
                if (word.CreateObject(outputPath, templatePath))
                {
                    try
                    {
                        //Giao xu
                        word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                        word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                        word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                        word.Replace(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                        word.Replace(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                        word.Replace(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                        word.Replace(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);

                        word.Replace(ReportGiaoDanConst.TenLinhMucGui, rowGiaoXu[ReportGiaoDanConst.TenLinhMucGui]);

                        //Giao ho
                        string tenGiaoHo = rowGiaoDan[GiaoHoConst.TenGiaoHo].ToString();
                        if (!Memory.IsNullOrEmpty(rowGiaoDan["TenGiaoHoCha"])
                            && rowGiaoDan["TenGiaoHoCha"].ToString().ToLower().CompareTo(tenGiaoHo.ToLower()) != 0)
                        {
                            tenGiaoHo = string.Format("{0} - {1}", rowGiaoDan["TenGiaoHoCha"], tenGiaoHo);
                        }

                        word.Replace(GiaoHoConst.TenGiaoHo, tenGiaoHo);

                        //Giao dan - thong tin co ban
                        word.Replace(GiaoDanConst.MaGiaoDan, rowGiaoDan[GiaoDanConst.MaGiaoDan], defaultValue);
                        word.Replace(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString(), defaultValue);
                        word.Replace(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh], defaultValue);
                        word.Replace(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh], defaultValue);
                        word.Replace(GiaoDanConst.Phai, rowGiaoDan[GiaoDanConst.Phai], defaultValue);
                        word.Replace(ThanhVienGiaDinhConst.VaiTro, vaiTro, "");

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

                        //Thong tin khac
                        word.Replace("DiaChiGiaoDan", rowGiaoDan[GiaoDanConst.DiaChi], defaultValue);
                        word.Replace("DienThoaiGiaoDan", rowGiaoDan[GiaoDanConst.DienThoai], defaultValue);
                        word.Replace("EmailGiaoDan", rowGiaoDan[GiaoDanConst.Email], defaultValue);
                        word.Replace("DanToc", rowGiaoDan[GiaoDanConst.DanToc], defaultValue);
                        word.Replace("NgheNghiep", rowGiaoDan[GiaoDanConst.NgheNghiep], defaultValue);
                        word.Replace("TrinhDoVanHoa", rowGiaoDan[GiaoDanConst.TrinhDoVanHoa], defaultValue);
                        //2018-08-03 Gia add start
                        word.Replace("TrinhDoChuyenMon", rowGiaoDan[GiaoDanConst.TrinhDoChuyenMon], defaultValue);
                        word.Replace("BietNgoaiNgu", rowGiaoDan[GiaoDanConst.BietNgoaiNgu], defaultValue);
                        //2018-08-30 Gia add end
                        word.Replace("ConHoc", (bool)rowGiaoDan[GiaoDanConst.ConHoc] ? "[x]" : "[  ]", "");
                        word.Replace("TanTong", (bool)rowGiaoDan[GiaoDanConst.TanTong] ? "[x]" : "[  ]", "");
                        word.Replace("DaCoGiaDinh", (bool)rowGiaoDan[GiaoDanConst.DaCoGiaDinh] ? "[x]" : "[  ]", "");
                        if ((bool)rowGiaoDan[GiaoDanConst.QuaDoi])
                        {
                            word.Replace("QuaDoi", (bool)rowGiaoDan[GiaoDanConst.QuaDoi] ? "[x]" : "[  ]", "");
                            word.Replace("NgayQuaDoi", Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NgayQuaDoi]) ? "" : string.Concat("Ngày: ", rowGiaoDan[GiaoDanConst.NgayQuaDoi]), "");
                            word.Replace("NoiAnTang", Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NoiAnTang]) ? "" : string.Concat("An táng tại: ", rowGiaoDan[GiaoDanConst.NoiAnTang]), "");
                            word.Replace("SoAnTang", Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.SoAnTang]) ? "" : string.Concat(" Số: ", rowGiaoDan[GiaoDanConst.SoAnTang]), "");

                        }
                        else
                        {
                            word.Replace("QuaDoi", "[  ]", "");
                            word.Replace("NoiAnTang", "", "");
                            word.Replace("SoAnTang", "", "");
                            word.Replace("NgayQuaDoi", "", "");
                        }

                        word.Replace(GiaoDanConst.ChaRuocLe, rowGiaoDan[GiaoDanConst.ChaRuocLe], defaultValue);

                        //Hon Phoi
                        word.Replace(HonPhoiConst.SoHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.SoHonPhoi] : "", defaultValue1);
                        word.Replace(HonPhoiConst.VoChong, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.VoChong] : "", defaultValue);
                        word.Replace(HonPhoiConst.NgayHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NgayHonPhoi] : "", defaultValue);
                        if (rowHonPhoi != null && rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length >= 4)
                        {
                            word.Replace("NamHonPhoi", rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Substring(rowHonPhoi[HonPhoiConst.NgayHonPhoi].ToString().Length - 4), defaultValue);
                        }
                        word.Replace(HonPhoiConst.NoiHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NoiHonPhoi] : "", defaultValue);
                        word.Replace("ChaHonPhoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.LinhMucChung] : "", defaultValue);
                        word.Replace(HonPhoiConst.CachThucHonPhoi, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.CachThucHonPhoi] : "", defaultValue);
                        word.Replace(HonPhoiConst.NguoiChung1, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NguoiChung1] : "", defaultValue);
                        word.Replace(HonPhoiConst.NguoiChung2, rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.NguoiChung2] : "", defaultValue);
                        word.Replace("GhiChuHonPHoi", rowHonPhoi != null ? rowHonPhoi[HonPhoiConst.GhiChu] : "", defaultValue);


                        if (Memory.GetConfig(GxConstants.CF_LANGUAGE).ToString() == GxConstants.LANG_EN)
                        {
                            word.Replace(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamEn());
                        }
                        else
                        {
                            word.Replace(ReportGiaoDanConst.NgayThangNam, Memory.GetReportNgayThangNamVn());
                        }

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
                                                            "Xin vui lòng đóng tập tin này và thử lại lần nữa." + Environment.NewLine +
                                                                "Chi tiết lỗi: " + Memory.Instance.Error?.Message);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                if (word != null) word.End_Write();
                word = null;
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
    }
}
