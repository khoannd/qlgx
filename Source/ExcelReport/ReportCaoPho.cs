using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportCaoPho
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
                DataTable tblCaoPho = ds.Tables.Contains(ReportCaoPhoConst.TableName) ? ds.Tables[ReportCaoPhoConst.TableName] : null;
                //DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];

                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                string fileName = GxConstants.REPORT_CAOPHO_FILENAME;

                string templatePath = Memory.GetReportTemplatePath(fileName);
                string outputPath = Memory.GetTempPath(fileName);

                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowHonPhoi = ds.Tables.Contains(HonPhoiConst.TableName) ? ds.Tables[HonPhoiConst.TableName].Rows[0] : null;
                DataRow rowGiaoXuNhan = tblCaoPho != null && tblCaoPho.Rows.Count != 0 ? tblCaoPho.Rows[0] : null;

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



                            word.Replace("CauNguyenLinhHon", rowGiaoDan[GiaoDanConst.TenThanh].ToString(), defaultValue);

                            word.Replace(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString(), defaultValue);
                            word.Replace(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh], defaultValue);
                            if (rowGiaoDan[GiaoDanConst.NgaySinh].ToString().Length >= 4)
                            {
                                int namSinh = Int32.Parse(rowGiaoDan[GiaoDanConst.NgaySinh].ToString().Substring(rowGiaoDan[GiaoDanConst.NgaySinh].ToString().Length - 4));
                                int tuoi = DateTime.Now.Year - namSinh;
                                word.Replace("Tuoi", tuoi, defaultValue);
                            }



                            word.Replace("TenGiaoKhu", rowGiaoXuNhan != null ? "..................." : rowGiaoXuNhan[GiaoXuConst.TenGiaoXu]);
                            word.Replace(ReportCaoPhoConst.NoiAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NoiAnTang] : ".....................");
                            word.Replace(ReportCaoPhoConst.SDTNhaHieu, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.SDTNhaHieu] : ".....................");
                            //Ngày qua đời
                            word.Replace(ReportCaoPhoConst.NgayQuaDoi, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NgayQuaDoi] : "......");
                            word.Replace(ReportCaoPhoConst.ThangQuaDoi, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.ThangQuaDoi] : "");
                            word.Replace(ReportCaoPhoConst.NamQuaDoi, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NamQuaDoi] : "");
                            word.Replace(ReportCaoPhoConst.GioQuaDoi, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.GioQuaDoi] : "");
                            word.Replace(ReportCaoPhoConst.PhutQuaDoi, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.PhutQuaDoi] : "");
                            //Ngày tẩn liệm
                            word.Replace(ReportCaoPhoConst.NgayTanLiem, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NgayTanLiem] : "");
                            word.Replace(ReportCaoPhoConst.ThangTanLiem, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.ThangTanLiem] : "");
                            word.Replace(ReportCaoPhoConst.NamTanLiem, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NamTanLiem] : "");
                            word.Replace(ReportCaoPhoConst.GioTanLiem, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.GioTanLiem] : "");
                            word.Replace(ReportCaoPhoConst.PhutTanLiem, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.PhutTanLiem] : "");
                            //Ngày TLe An táng
                            word.Replace(ReportCaoPhoConst.NgayTLAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NgayTLAnTang] : "");
                            word.Replace(ReportCaoPhoConst.ThangTLAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.ThangTLAnTang] : "");
                            word.Replace(ReportCaoPhoConst.NamTLAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NamTLAnTang] : "");
                            word.Replace(ReportCaoPhoConst.GioTLAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.GioTLAnTang] : "");
                            word.Replace(ReportCaoPhoConst.PhutTLAnTang, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.PhutTLAnTang] : "");
                            //Ngày Tle Tại Gia
                            word.Replace(ReportCaoPhoConst.NgayTLTaiGia, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NgayTLTaiGia] : "");
                            word.Replace(ReportCaoPhoConst.ThangTLTaiGia, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.ThangTLTaiGia] : "");
                            word.Replace(ReportCaoPhoConst.NamTLTaiGia, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.NamTLTaiGia] : "");
                            word.Replace(ReportCaoPhoConst.GioTLTaiGia, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.GioTLTaiGia] : "");
                            word.Replace(ReportCaoPhoConst.PhutTLTaiGia, rowGiaoXuNhan != null ? rowGiaoXuNhan[ReportCaoPhoConst.PhutTLTaiGia] : "");

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
                                                                "Xin vui lòng đóng tập tin này và thử lại lần nữa." + Environment.NewLine +
                                                                "Chi tiết lỗi: " + Memory.Instance.Error?.Message);
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
                                                                "Xin vui lòng đóng tập tin này và thử lại lần nữa." + Environment.NewLine +
                                                                "Chi tiết lỗi: " + Memory.Instance.Error?.Message);
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
