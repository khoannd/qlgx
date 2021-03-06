using System;
using System.Collections.Generic;
using System.Text;
using GXGlobal;
using System.Data;

namespace ExcelReport
{
    public class ReportHonPhoi
    {
        public static int Export1(DataSet ds)
        {
            ExcelEngine excel = new ExcelEngine();
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(GiaoDanConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                DataTable tblGiaoXuNhan = ds.Tables[ReportHonPhoiConst.TableName];
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                string templatePath = Memory.GetReportTemplatePath(GXConstants.REPORT_HONPHOI_FILENAME);
                string outputPath = Memory.GetReportTempPath(GXConstants.REPORT_HONPHOI_FILENAME);

                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];
                DataRow rowGiaDinh = null;
                if (tblGiaDinh.Rows.Count > 0)
                {
                    rowGiaDinh = tblGiaDinh.Rows[0];
                }
                if (excel.CreateObject(outputPath, templatePath))
                {
                    excel.Write_to_excel(1, 5, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                    //excel.Write_to_excel(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                    excel.Write_to_excel(2, 5, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                    excel.Write_to_excel(3, 5, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(6, 7, rowGiaoXuNhan[ReportHonPhoiConst.TenLinhMucNhan]);
                    excel.Write_to_excel(6, 17, rowGiaoXuNhan[ReportHonPhoiConst.GiaoXuNhan]);
                    excel.Write_to_excel(7, 6, rowGiaoXu[ReportHonPhoiConst.TenLinhMucGui]);
                    excel.Write_to_excel(7, 19, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(9, 6, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString());
                    excel.Write_to_excel(10, 6, rowGiaoDan[GiaoDanConst.NgaySinh]);
                    excel.Write_to_excel(10, 15, rowGiaoDan[GiaoDanConst.NoiSinh]);
                    if (rowGiaDinh != null)
                    {
                        excel.Write_to_excel(11, 6, rowGiaDinh[ReportHonPhoiConst.TenCha]);
                        excel.Write_to_excel(12, 6, rowGiaDinh[ReportHonPhoiConst.TenMe]);
                    }
                    excel.Write_to_excel(13, 8, rowGiaoDan[GiaoDanConst.NgayRuaToi]);
                    excel.Write_to_excel(13, 15, rowGiaoDan[GiaoDanConst.NoiRuaToi]);
                    excel.Write_to_excel(14, 7, rowGiaoDan[GiaoDanConst.ChaRuaToi]);
                    excel.Write_to_excel(15, 7, rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi]);
                    excel.Write_to_excel(16, 7, rowGiaoDan[GiaoDanConst.SoRuaToi]);
                    excel.Write_to_excel(17, 9, rowGiaoDan[GiaoDanConst.NgayThemSuc]);
                    excel.Write_to_excel(17, 15, rowGiaoDan[GiaoDanConst.NoiThemSuc]);
                    excel.Write_to_excel(18, 8, rowGiaoDan[GiaoDanConst.ChaThemSuc]);
                    excel.Write_to_excel(19, 7, rowGiaoDan[GiaoDanConst.NguoiDoDauThemSuc]);
                    excel.Write_to_excel(20, 7, rowGiaoDan[GiaoDanConst.SoThemSuc]);
                    excel.Write_to_excel(21, 7, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(22, 7, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                    excel.Write_to_excel(22, 17, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);                
                    excel.Write_to_excel(24, 6, rowGiaoXuNhan[ReportHonPhoiConst.Nguoi2]);
                    excel.Write_to_excel(29, 11, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(29, 15, string.Format("Ngày {0} tháng {1} năm {2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year));
                    excel.End_Write();
                    System.Diagnostics.Process.Start(outputPath);
                }
                else
                {
                    Memory.Instance.Error = new Exception("Xuất giới thiệu thất bại." + Environment.NewLine +
                                                            "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine + 
                                                            "Có thể do tập tin \"HonPhoi.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                            "Xin vui lòng đóng tập tin này và thử lại lần nữa");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                if(excel!=null) excel.End_Write();
                Memory.Instance.Error = ex;
                return -1;
            }
            return 0;
        }

        public static int Export(DataSet ds)
        {
            ExcelEngine excel = new ExcelEngine();
            try
            {
                if (!ds.Tables.Contains(GiaoDanConst.TableName) || !ds.Tables.Contains(GiaoXuConst.TableName) || !ds.Tables.Contains(GiaoDanConst.TableName))
                {
                    Memory.Instance.Error = new Exception("Không có dữ liệu làm việc!");
                    return -1;
                }

                DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];
                DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
                DataTable tblGiaDinh = ds.Tables[GiaDinhConst.TableName];
                DataTable tblGiaoXuNhan = ds.Tables[ReportHonPhoiConst.TableName];
                if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
                string templatePath = Memory.GetReportTemplatePath(GXConstants.REPORT_HONPHOI_FILENAME);
                string outputPath = Memory.GetReportTempPath(GXConstants.REPORT_HONPHOI_FILENAME);

                DataRow rowGiaoXu = tblGiaoXu.Rows[0];
                DataRow rowGiaoDan = tblGiaoDan.Rows[0];
                DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];
                DataRow rowGiaDinh = null;
                if (tblGiaDinh.Rows.Count > 0)
                {
                    rowGiaDinh = tblGiaDinh.Rows[0];
                }
                if (excel.CreateObject(outputPath, templatePath))
                {
                    excel.Write_to_excel(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                    excel.Write_to_excel(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                    excel.Write_to_excel(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(ReportHonPhoiConst.TenLinhMucNhan, rowGiaoXuNhan[ReportHonPhoiConst.TenLinhMucNhan]);
                    excel.Write_to_excel(ReportHonPhoiConst.GiaoXuNhan, rowGiaoXuNhan[ReportHonPhoiConst.GiaoXuNhan]);
                    excel.Write_to_excel(ReportHonPhoiConst.TenLinhMucGui, rowGiaoXu[ReportHonPhoiConst.TenLinhMucGui]);
                    //excel.Write_to_excel(7, 19, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(GiaoDanConst.HoTen, rowGiaoDan[GiaoDanConst.TenThanh].ToString() + " " + rowGiaoDan[GiaoDanConst.HoTen].ToString());
                    excel.Write_to_excel(GiaoDanConst.NgaySinh, rowGiaoDan[GiaoDanConst.NgaySinh]);
                    excel.Write_to_excel(GiaoDanConst.NoiSinh, rowGiaoDan[GiaoDanConst.NoiSinh]);
                    if (rowGiaDinh != null)
                    {
                        excel.Write_to_excel(ReportHonPhoiConst.TenCha, rowGiaDinh[ReportHonPhoiConst.TenCha]);
                        excel.Write_to_excel(ReportHonPhoiConst.TenMe, rowGiaDinh[ReportHonPhoiConst.TenMe]);
                    }
                    else
                    {
                        excel.Write_to_excel(ReportHonPhoiConst.TenCha, "");
                        excel.Write_to_excel(ReportHonPhoiConst.TenMe, "");
                    }
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
                    //excel.Write_to_excel(21, 7, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    //excel.Write_to_excel(22, 7, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
                    //excel.Write_to_excel(22, 17, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
                    excel.Write_to_excel(ReportHonPhoiConst.Nguoi2, rowGiaoXuNhan[ReportHonPhoiConst.Nguoi2]);
                    //excel.Write_to_excel(29, 11, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
                    excel.Write_to_excel(ReportHonPhoiConst.NgayThangNam, string.Format("Ngày {0} tháng {1} năm {2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year));
                    excel.End_Write();
                    System.Diagnostics.Process.Start(outputPath);
                }
                else
                {
                    Memory.Instance.Error = new Exception("Xuất giới thiệu thất bại." + Environment.NewLine +
                                                            "Có thể bạn chưa cài MS Office 2003 trở lên" + Environment.NewLine +
                                                            "Có thể do tập tin \"HonPhoi.xls\" trong thư mục Template của chương trình đang được mở" + Environment.NewLine +
                                                            "Xin vui lòng đóng tập tin này và thử lại lần nữa");
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
    }
}
