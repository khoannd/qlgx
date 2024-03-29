using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportGioiThieuHP
    {
        public static int Export(DataSet ds)
        {
            ExcelEngine excel = null;
            WordEngine word = null;
            try
            {
                if (!ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(GiaoDanConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                //DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                DataTable tblGiaoXuNhan = ds.Tables[ReportHonPhoiConst.TableName];
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                string templatePath = Memory.GetReportTemplatePath(GxConstants.REPORT_HONPHOI_FILENAME);
                string outputPath = Memory.GetTempPath(GxConstants.REPORT_HONPHOI_FILENAME);

                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];

                string reportFormat = Memory.GetReportFormat();
                templatePath = string.Concat(templatePath, reportFormat);
                outputPath = string.Concat(outputPath, reportFormat);

                if (reportFormat == GxConstants.DOC_FORMAT)
                {
                    word = new WordEngine();
                    word.CreateObject(outputPath, templatePath);

                    try
                    {
                        word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                        word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                        word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                        //word.Replace(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                        word.Replace(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                        //word.Replace(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                        word.Replace(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);

                        word.Replace(ReportHonPhoiConst.TenGiaoXuNhan, rowGiaoXuNhan[ReportHonPhoiConst.TenLinhMucNhan]);
                        word.Replace(ReportHonPhoiConst.TenGiaoPhanNhan, rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoXuNhan]);
                        word.Replace(ReportHonPhoiConst.TenLinhMucGui, rowGiaoXu[ReportHonPhoiConst.TenLinhMucGui]);
                        //word.Replace(7, 19, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                        word.Replace(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString());
                        word.Replace(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh]);
                        word.Replace(ReportHonPhoiConst.Tuoi1, Memory.GetTuoi(rowGiaoDan[GiaoDanConst.NgaySinh].ToString()));
                        word.Replace(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh]);
                        word.Replace(GiaoDanConst.DienThoai, rowGiaoDan[GiaoDanConst.DienThoai]);
                        word.Replace(GiaoDanConst.DiaChi, rowGiaoDan[GiaoDanConst.DiaChi]);

                        word.Replace(ReportGiaoDanConst.TenCha, rowGiaoDan[GiaoDanConst.HoTenCha]);
                        word.Replace(ReportGiaoDanConst.TenMe, rowGiaoDan[GiaoDanConst.HoTenMe]);

                        word.Replace(GiaoDanConst.NgayRuaToi, rowGiaoDan[GiaoDanConst.NgayRuaToi]);
                        word.Replace(GiaoDanConst.NoiRuaToi, rowGiaoDan[GiaoDanConst.NoiRuaToi]);
                        word.Replace(GiaoDanConst.ChaRuaToi, rowGiaoDan[GiaoDanConst.ChaRuaToi]);
                        word.Replace(GiaoDanConst.NguoiDoDauRuaToi, rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi]);
                        word.Replace(GiaoDanConst.SoRuaToi, rowGiaoDan[GiaoDanConst.SoRuaToi]);
                        word.Replace(GiaoDanConst.NgayThemSuc, rowGiaoDan[GiaoDanConst.NgayThemSuc]);
                        word.Replace(GiaoDanConst.NoiThemSuc, rowGiaoDan[GiaoDanConst.NoiThemSuc]);
                        word.Replace(GiaoDanConst.ChaThemSuc, rowGiaoDan[GiaoDanConst.ChaThemSuc]);
                        word.Replace(GiaoDanConst.NguoiDoDauThemSuc, rowGiaoDan[GiaoDanConst.NguoiDoDauThemSuc]);
                        word.Replace(GiaoDanConst.SoThemSuc, rowGiaoDan[GiaoDanConst.SoThemSuc]);
                        word.Replace(ReportHonPhoiConst.Nguoi2, rowGiaoXuNhan[ReportHonPhoiConst.Nguoi2]);
                        word.Replace(ReportHonPhoiConst.Tuoi2, rowGiaoXuNhan[ReportHonPhoiConst.Tuoi2]);
                        word.Replace(ReportHonPhoiConst.TenCha2, rowGiaoXuNhan[ReportHonPhoiConst.TenCha2]);
                        word.Replace(ReportHonPhoiConst.TenMe2, rowGiaoXuNhan[ReportHonPhoiConst.TenMe2]);
                        word.Replace(ReportHonPhoiConst.TenGiaoXu2, rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoXu2]);
                        word.Replace(ReportHonPhoiConst.TenGiaoPhan2, rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoPhan2]);
                        word.Replace(ReportHonPhoiConst.NoiHocGLHN, rowGiaoXuNhan[ReportHonPhoiConst.NoiHocGLHN]);

                        if (Memory.GetConfig(GxConstants.CF_LANGUAGE) == GxConstants.LANG_EN)
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
                    excel = new ExcelEngine();
                    if (excel.CreateObject(outputPath, templatePath))
                    {
                        excel.LoaiBaoCao = ReportType.GioiThieuHonPhoi;
                        excel.Write_to_excel(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                        excel.Write_to_excel(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                        excel.Write_to_excel(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                        excel.Write_to_excel(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
                        excel.Write_to_excel(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
                        excel.Write_to_excel(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
                        excel.Write_to_excel(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);

                        excel.Write_to_excel(ReportHonPhoiConst.TenGiaoXuNhan, rowGiaoXuNhan[ReportHonPhoiConst.TenLinhMucNhan]);
                        excel.Write_to_excel(ReportHonPhoiConst.TenGiaoPhanNhan, rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoXuNhan]);
                        excel.Write_to_excel(ReportHonPhoiConst.TenLinhMucGui, rowGiaoXu[ReportHonPhoiConst.TenLinhMucGui]);
                        //excel.Write_to_excel(7, 19, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
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
                        excel.Write_to_excel(ReportHonPhoiConst.Nguoi2, rowGiaoXuNhan[ReportHonPhoiConst.Nguoi2]);
                        if (Memory.GetConfig(GxConstants.CF_LANGUAGE) == GxConstants.LANG_EN)
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
                    else
                    {
                        Memory.Instance.Error = new Exception("Xuất giới thiệu thất bại." + Environment.NewLine +
                                                                "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                                "Có thể do tập tin \"HonPhoi.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
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
