using GxGlobal;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DongBoDuLieu
{
    public class SynJob
    {
        private static bool errorState = false;
        public void Synchronize(Form frm)
        {
            Thread thread = new Thread(() =>
            {
                Push();
                Pull();
                DeleteCSV();
                frm.Close();
                if (errorState == false)
                {
                    MessageBox.Show("Đồng bộ thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                errorState = false;
            });
            thread.Start();
        }

        private void DeleteCSV()
        {
            string giaoxusynPath = Memory.AppPath + "sync\\";
            foreach (string sFile in System.IO.Directory.GetFiles(giaoxusynPath, "*.csv"))
            {
                System.IO.File.Delete(sFile);
            }
        }

        public void Push()
        {
            try
            {
                string maGiaoXuRieng = Memory.getMaGiaoXuServer();
                WebClient cl = new WebClient();
                cl.Encoding = System.Text.Encoding.UTF8;
                //Kiểm tra giáo xứ có đồng ý cho phép đồng bộ không.
                int lockSync = Int32.Parse(cl.DownloadString(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/checkPermission/" + maGiaoXuRieng));
                if (lockSync != 1)
                {
                    if (lockSync == 0)
                    {
                        //Xử lý có giáo xứ khác đang đồng bộ
                        MessageBox.Show("Có máy nhập khác đang đồng bộ rồi", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    MessageBox.Show("Quá trình đồng bộ dữ liệu thất bại vui lòng liên hệ quản trị hệ thống qlgx.net để được hỗ trợ",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string pathFileSyn = this.createrFileSyn();
                if (pathFileSyn != null)
                {
                    byte[] response = cl.UploadFile(ConfigurationManager.AppSettings["SERVER"] + @"SynFileCL/getFileSyn/" + maGiaoXuRieng, pathFileSyn);
                    int rs = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(response, 0, response.Length));
                    if (rs == 1)
                    {
                        cl.DownloadStringCompleted += new DownloadStringCompletedEventHandler(handlerDowloadStringPush);
                        cl.DownloadStringAsync(new Uri(ConfigurationManager.AppSettings["SERVER"] + @"SynJobCL/executeByMaGiaoXu/" + maGiaoXuRieng));
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Quá trình xử lý dữ liệu dữ liệu khi đưa lên server bị lỗi (" + rs + ").\r\n" +
                                        "Vui lòng liên hệ quản trị hệ thông qlgx.net để được hỗ trợ.\r\n" +
                                        "Xin cảm ơn.\r\n", "Lỗi đồng bộ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        errorState = true;
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Quá trình đưa dữ liệu lên server bị lỗi.\r\n" +
                    "Vui lòng liên hệ quản trị hệ thông qlgx.net để được hỗ trợ.\r\n" +
                    "Xin cảm ơn.\r\n" + ex.Message, "Lỗi đồng bộ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorState = true;
                return;
            }
        }

        public void handlerDowloadStringPush(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message.ToString());
                return;
            }
            string messageRe = e.Result.ToString();
            MessageBox.Show(e.Result.ToString());
            Pull();
            return;
        }
        public void Pull()
        {
            try
            {
                WebClient cl = new WebClient();
                cl.Encoding = System.Text.Encoding.UTF8;
                string maGiaoXuRieng = Memory.getMaGiaoXuServer();
                cl.DownloadStringCompleted += new DownloadStringCompletedEventHandler(handlerDowloadStringPull);
                cl.DownloadStringAsync(new Uri(ConfigurationManager.AppSettings["SERVER"] + @"SynToClientCL/createrFileSyn/" + maGiaoXuRieng+"/"+GenerateUniqueCode.GetUniqueCode()));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Quá trình lấy dữ liệu từ server bị lỗi.\r\n" +
                    "Vui lòng liên hệ quản trị hệ thông qlgx.net để được hỗ trợ.\r\n" +
                    "Xin cảm ơn.\r\n" + ex.Message, "Lỗi đồng bộ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorState = true;
                return;
            }
        }
        private void ProcessPull()
        {
            WebClient cl = new WebClient();
            string pathDB = Memory.AppPath + "DBSV\\";
            if (!Directory.Exists(pathDB))
            {
                Directory.CreateDirectory(pathDB);
            }
            string maGiaoXuRieng = Memory.getMaGiaoXuServer();
            cl.DownloadFile(ConfigurationManager.AppSettings["SERVER_File"] + maGiaoXuRieng + "/" + maGiaoXuRieng + ".zip", pathDB + maGiaoXuRieng + ".zip");
            FastZip zip = new FastZip();
            zip.ExtractZip(pathDB + maGiaoXuRieng + ".zip", pathDB, null);
            Thread pullProcess = new Thread(()=> {
                compareGiaoXu(pathDB);
            });
            pullProcess.IsBackground = false;
            pullProcess.Start();
        }
        public void handlerDowloadStringPull(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message.ToString());
                return;
            }
            //string messageRe = e.Result.ToString();
            //MessageBox.Show(e.Result.ToString());
            ProcessPull();
            return;
        }
        
        public void compareGiaoXu(string dir)
        {
            DataSet ds = new DataSet();
            //dongboID
            DataTable tblDongBo = Memory.GetData(SqlConstants.SELECT_ALLDongBo);
            tblDongBo.TableName = DongBoConst.TableName;
            DongBoID dbid = new DongBoID(dir, "dongbo.csv", tblDongBo);
            dbid.import();
            tblDongBo.Columns.Add(new DataColumn("DaXoa", typeof(int)));
            ds.Tables.Add(tblDongBo);

            //Giáo họ
            DataTable tblGiaoHo = Memory.GetData(SqlConstants.SELECT_ALLGiaoHo);
            tblGiaoHo.TableName = GiaoHoConst.TableName;
            GiaoHoCompare giaoho = new GiaoHoCompare(dir, "GiaoHo.csv",tblGiaoHo);
            giaoho.TblDongBo = tblDongBo;
            giaoho.ExProcessData();
            ds.Tables.Add(tblGiaoHo);

            //Giáo dân
            DataTable tblGiaoDan = Memory.GetData(SqlConstants.SELECT_ALLGiaoDan);
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            GiaoDanCompare giaodan = new GiaoDanCompare(dir, "GiaoDan.csv",tblGiaoDan);
            giaodan.TblDongBo = tblDongBo;
            giaodan.ExProcessData();
            ds.Tables.Add(tblGiaoDan);

            //Gia đình
            DataTable tblGiaDinh = Memory.GetData(SqlConstants.SELECT_ALLGiaDinh);
            tblGiaDinh.TableName = GiaDinhConst.TableName;
            GiaDinhCompare giadinh = new GiaDinhCompare(dir, "GiaDinh.csv",tblGiaDinh);
            giadinh.TblDongBo = tblDongBo;
            giadinh.ExProcessData();
            ds.Tables.Add(tblGiaDinh);

            //Thành viên Gia đình
            DataTable tblTVGD = Memory.GetData(SqlConstants.SELECT_ALLThanhVienGiaDinh);
            tblTVGD.TableName = ThanhVienGiaDinhConst.TableName;
            ThanhVienGiaDinhCompare thanhviengiadinh = new ThanhVienGiaDinhCompare(dir, "ThanhVienGiaDinh.csv", tblTVGD);
            thanhviengiadinh.TblDongBo = tblDongBo;
            thanhviengiadinh.ExProcessData();
            ds.Tables.Add(tblTVGD);

            //Đợt bí tích
            DataTable tblDotBiTich= Memory.GetData(SqlConstants.SELECT_ALLDotBiTich);
            tblDotBiTich.TableName = DotBiTichConst.TableName;
            DotBiTichCompare dotbitich = new DotBiTichCompare(dir, "DotBiTich.csv",tblDotBiTich);
            dotbitich.TblDongBo = tblDongBo;
            dotbitich.ExProcessData();
            ds.Tables.Add(tblDotBiTich);

            //Bí tích chi tiết
            DataTable tblBiTichChiTiet = Memory.GetData(SqlConstants.SELECT_ALLBiTichChiTiet);
            tblBiTichChiTiet.TableName = BiTichChiTietConst.TableName;
            BiTichChiTietCompare bitichchitiet = new BiTichChiTietCompare(dir, "BiTichChiTiet.csv",tblBiTichChiTiet);
            bitichchitiet.TblDongBo = tblDongBo;
            bitichchitiet.ExProcessData();
            ds.Tables.Add(tblBiTichChiTiet);


            //Hôn phối
            DataTable tblHonPhoi = Memory.GetData(SqlConstants.SELECT_ALLHonPhoi);
            tblHonPhoi.TableName = HonPhoiConst.TableName;
            HonPhoiCompare honphoi = new HonPhoiCompare(dir, "HonPhoi.csv",tblHonPhoi);
            honphoi.TblDongBo = tblDongBo;
            honphoi.ExProcessData(); 
            ds.Tables.Add(tblHonPhoi);

            //Giáo dân hôn phối
            DataTable tblGDHP = Memory.GetData(SqlConstants.SELECT_ALLGiaoDanHonPhoi);
            tblGDHP.TableName = GiaoDanHonPhoiConst.TableName;
            GiaoDanHonPhoiCompare giaodanhonphoi = new GiaoDanHonPhoiCompare(dir, "GiaoDanHonPhoi.csv",tblGDHP);
            giaodanhonphoi.TblDongBo = tblDongBo;
            giaodanhonphoi.ExProcessData();
            ds.Tables.Add(tblGDHP);

            //Khối giáo lý
            DataTable tblKhoiGiaoLy = Memory.GetData(SqlConstants.SELECT_ALLKhoiGiaoLy);
            tblKhoiGiaoLy.TableName = KhoiGiaoLyConst.TableName;
            KhoiGiaoLyCompare khoigiaoly = new KhoiGiaoLyCompare(dir, "KhoiGiaoLy.csv",tblKhoiGiaoLy);
            khoigiaoly.TblDongBo = tblDongBo;
            khoigiaoly.ExProcessData(); 
            ds.Tables.Add(tblKhoiGiaoLy);

            //Lớp giáo lý
            DataTable tblLopGiaoLy = Memory.GetData(SqlConstants.SELECT_ALLLopGiaoLy);
            tblLopGiaoLy.TableName = LopGiaoLyConst.TableName;
            LopGiaoLyCompare lopgiaoly = new LopGiaoLyCompare(dir, "LopGiaoLy.csv", tblLopGiaoLy);
            lopgiaoly.TblDongBo = tblDongBo;
            lopgiaoly.ExProcessData();
            ds.Tables.Add(tblLopGiaoLy);

            //Giáo lý viên
            DataTable tblGiaoLyVien = Memory.GetData(SqlConstants.SELECT_ALLGiaoLyVien);
            tblGiaoLyVien.TableName = GiaoLyVienConst.TableName;
            GiaoLyVienCompare giaolyvien = new GiaoLyVienCompare(dir, "GiaoLyVien.csv", tblGiaoLyVien);
            giaolyvien.TblDongBo = tblDongBo;
            giaolyvien.ExProcessData();
            ds.Tables.Add(tblGiaoLyVien);

            //Chi tiết lớp giáo lý
            DataTable tblCTLGL = Memory.GetData(SqlConstants.SELECT_ALLChiTietLopGiaoLy);
            tblCTLGL.TableName = ChiTietLopGiaoLyConst.TableName;
            ChiTietLopGiaoLyCompare chitietlopgiaoly = new ChiTietLopGiaoLyCompare(dir, "ChiTietLopGiaoLy.csv", tblCTLGL);
            chitietlopgiaoly.TblDongBo = tblDongBo;
            chitietlopgiaoly.ExProcessData();
            ds.Tables.Add(tblCTLGL);

            //Hội đoàn
            DataTable tblHoiDoan = Memory.GetData(SqlConstants.SELECT_ALLHoiDoan);
            tblHoiDoan.TableName = HoiDoanConst.TableName;
            HoiDoanCompare hoidoan = new HoiDoanCompare(dir, "HoiDoan.csv", tblHoiDoan);
            hoidoan.TblDongBo = tblDongBo;
            hoidoan.ExProcessData();
            ds.Tables.Add(tblHoiDoan);

            //Chi tiết hội đoàn
            DataTable tblCTHD = Memory.GetData(SqlConstants.SELECT_ALLChiTietHoiDoan);
            tblCTHD.TableName = ChiTietHoiDoanConst.TableName;
            ChiTietHoiDoanCompare chitiethoidoan = new ChiTietHoiDoanCompare(dir,"ChiTietHoiDoan.csv",tblCTHD);
            chitiethoidoan.TblDongBo = tblDongBo;
            chitiethoidoan.ExProcessData();
            ds.Tables.Add(tblCTHD);

            //Rao hôn phối
            DataTable tblRaoHonPhoi = Memory.GetData(SqlConstants.SELECT_ALLRaoHonPhoi);
            tblRaoHonPhoi.TableName = RaoHonPhoiConst.TableName;
            RaoHonPhoiCompare raohonphoi = new RaoHonPhoiCompare(dir, "RaoHonPhoi.csv", tblRaoHonPhoi);
            raohonphoi.TblDongBo = tblDongBo;
            raohonphoi.ExProcessData();
            ds.Tables.Add(tblRaoHonPhoi);

            //Chuyển xứ
            DataTable tblChuyenXu = Memory.GetData(SqlConstants.SELECT_ALLChuyenXu);
            tblChuyenXu.TableName = ChuyenXuConst.TableName;
            ChuyenXuCompare chuyenxu = new ChuyenXuCompare(dir, "ChuyenXu.csv", tblChuyenXu);
            chuyenxu.TblDongBo = tblDongBo;
            chuyenxu.ExProcessData();
            ds.Tables.Add(tblChuyenXu);

            //Tận hiến
            DataTable tblTanHien = Memory.GetData(SqlConstants.SELECT_ALLTanHien);
            tblTanHien.TableName = TanHienConst.TableName;
            TanHienCompare tanhien = new TanHienCompare(dir, "TanHien.csv",tblTanHien);
            tanhien.TblDongBo = tblDongBo;
            tanhien.ExProcessData();
            ds.Tables.Add(tblTanHien);

            //Linh mục
            DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_ALLLinhMuc);
            tblLinhMuc.TableName = LinhMucConst.TableName;
            LinhMucCompare linhmuc = new LinhMucCompare(dir, "LinhMuc.csv", tblLinhMuc);
            linhmuc.TblDongBo = tblDongBo;
            linhmuc.ExProcessData();
            ds.Tables.Add(tblLinhMuc);

            //Cấu hình
            DataTable tblCauHinh = Memory.GetData(SqlConstants.SELECT_ALLCauHinh);
            tblCauHinh.TableName = CauHinhConst.TableName;
            CauHinhCompare cauhinh = new CauHinhCompare(dir, "CauHinh.csv", tblCauHinh);
            cauhinh.ExProcessData();
            ds.Tables.Add(tblCauHinh);
            //xóa row trong bảng đồng bộ
            dbid.deleteRow();
            Memory.UpdateDataSet(ds);
            MessageBox.Show("Thành Công");
        }
        //gán tên cho table
        public DataSet ListTable(string[,] stringListTable)
        {
            int CountTbl = (int)stringListTable.GetLongLength(0);  //Số table
            DataSet ds = new DataSet();
            DataTable tblex;
            string dieukien = " where UpdateDate >= ? ";
            DateTime? pulldate = Memory.getPullDate();
            for (int i = 0; i < CountTbl; i++)
            {
                if (pulldate != null)
                {
                    if (stringListTable[i, 1] == "GiaoDan")
                    {
                        tblex = Memory.GetData(stringListTable[i, 0] + dieukien + " order by MaGiaoDan ASC", new object[] { pulldate.ToString() });
                    }
                    else
                    {
                        tblex = Memory.GetData(stringListTable[i, 0] + dieukien, new object[] { pulldate.ToString() });
                    }
                }
                else
                {
                    if (stringListTable[i, 1] == "GiaoDan")
                    {
                        tblex = Memory.GetData(stringListTable[i, 0] + " order by MaGiaoDan ASC");
                    }
                    else
                    {
                        tblex = Memory.GetData(stringListTable[i, 0]);
                    }
                }
                tblex.TableName = stringListTable[i, 1];
                ds.Tables.Add(tblex);
            }
            return ds;
        }
        private string createrFileSyn()
        {

            string giaoxusynPath = Memory.AppPath + "sync\\";
            if (!Directory.Exists(giaoxusynPath))
            {
                Directory.CreateDirectory(giaoxusynPath);
            }
            string[,] stringListTable = new string[,]
            {
                {SqlConstants.SELECT_ALLBiTichChiTiet,BiTichChiTietConst.TableName },
                {SqlConstants.SELECT_ALLCauHinh,CauHinhConst.TableName },
                {SqlConstants.SELECT_ALLChiTietHoiDoan,ChiTietHoiDoanConst.TableName },
                {SqlConstants.SELECT_ALLChiTietLopGiaoLy,ChiTietLopGiaoLyConst.TableName },
                {SqlConstants.SELECT_ALLChuyenXu,ChuyenXuConst.TableName },
                {SqlConstants.SELECT_ALLDotBiTich,DotBiTichConst.TableName },
                {SqlConstants.SELECT_ALLGiaDinh,GiaDinhConst.TableName },
                {SqlConstants.SELECT_ALLGiaoDan,GiaoDanConst.TableName },
                {SqlConstants.SELECT_ALLGiaoDanHonPhoi,GiaoDanHonPhoiConst.TableName },
                {SqlConstants.SELECT_ALLGiaoHo,GiaoHoConst.TableName },
                {SqlConstants.SELECT_ALLGiaoLyVien,GiaoLyVienConst.TableName },
                {SqlConstants.SELECT_ALLHoiDoan,HoiDoanConst.TableName },
                {SqlConstants.SELECT_ALLHonPhoi,HonPhoiConst.TableName },
                {SqlConstants.SELECT_ALLKhoiGiaoLy,KhoiGiaoLyConst.TableName },
                {SqlConstants.SELECT_ALLLinhMuc,LinhMucConst.TableName },
                {SqlConstants.SELECT_ALLLopGiaoLy,LopGiaoLyConst.TableName },
                {SqlConstants.SELECT_ALLRaoHonPhoi,RaoHonPhoiConst.TableName },
                {SqlConstants.SELECT_ALLTanHien,TanHienConst.TableName },
                {SqlConstants.SELECT_ALLThanhVienGiaDinh,ThanhVienGiaDinhConst.TableName }
            };
            DataSet ds = ListTable(stringListTable);
            DataTable tblDongBo = Memory.GetData(String.Format("Select * from {0}", DongBoConst.TableName));
            string maDinhDanh = Memory.MaDinhDanh;
            string maGiaoXuRieng = Memory.getMaGiaoXuServer();

            if (ds.Tables.Count > 0)
            {
                foreach (DataTable tbl in ds.Tables)
                {
                    //Xử lý ID
                    Memory.changeValueID(tbl, tblDongBo,maDinhDanh,maGiaoXuRieng);
                    WriteFileCSV(tbl, 0);
                    //ghi xong 1 table;
                }

                string fileName = "gxsyn" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
                FastZip fzip = new FastZip();
                fzip.CreateZip(giaoxusynPath + fileName, giaoxusynPath, false, @"\.csv$");
                foreach (string sFile in System.IO.Directory.GetFiles(giaoxusynPath, "*.csv"))
                {
                    System.IO.File.Delete(sFile);
                }
                return giaoxusynPath + fileName;
            }
            return null;
        }



        private static void CatFileCSV(string pathFile, string nameTable)
        {
            using (StreamWriter sw = new StreamWriter(pathFile + nameTable + ".csv", true))
            {
                bool check = File.Exists(pathFile + nameTable + "Deleted" + ".csv");
                if (check)
                {
                    using (StreamReader sr = new StreamReader(pathFile + nameTable + "Deleted.csv"))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            sw.Write(line + "\n");
                        }
                    }
                    System.IO.File.Delete(pathFile + nameTable + "Deleted.csv");
                }
            }
        }
        public static void WriteFileCSV(DataTable tblRow, int isDelete)
        {
            if (tblRow != null)
            {
                string pathFile = Memory.AppPath + "sync\\";
                if (!Directory.Exists(pathFile))
                {
                    Directory.CreateDirectory(pathFile);
                }
                bool check = File.Exists(pathFile + tblRow.TableName + ".csv");
                using (StreamWriter sw = new StreamWriter(pathFile + tblRow.TableName + ".csv", true))
                {
                    if (!check)
                    {
                        sw.Write(createHeader(tblRow));
                    }
                    sw.Write(createRowValue(tblRow, isDelete));
                }
                CatFileCSV(pathFile, tblRow.TableName);
            }
        }
        public static string createRowValue(DataTable tblRow, int isDelete)
        {
            var result = new StringBuilder();
            foreach (DataRow row in tblRow.Rows)
            {
                for (int i = 0; i < tblRow.Columns.Count; i++)
                {
                    var value = row[i];
                    if (value.GetType().Name == "String")
                    {
                        value = Regex.Replace(value.ToString(), @"(\s{2,})|\n", " ");
                        value = Regex.Replace(value.ToString(), DongBoConst.Enclosure, " ");
                    }
                    if (value.GetType().Name == "DateTime")
                    {
                        value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", value);
                    }
                    result.Append(DongBoConst.Enclosure + value + DongBoConst.Enclosure);
                    result.Append(DongBoConst.Delimiter);
                }
                result.Append(DongBoConst.Enclosure + isDelete + DongBoConst.Enclosure + "\n");
            }
            return result.ToString();
        }
        public static string createHeader(DataTable tblRow)
        {
            var result = new StringBuilder();
            for (int i = 0; i < tblRow.Columns.Count; i++)
            {
                result.Append(DongBoConst.Enclosure + tblRow.Columns[i].ColumnName + DongBoConst.Enclosure);
                result.Append(DongBoConst.Delimiter);
            }
            result.Append(DongBoConst.Enclosure + "DeleteClient" + DongBoConst.Enclosure + "\n");
            return result.ToString();
        }
        private string ToCSV(DataTable table, bool fileExist)
        {
            var result = new StringBuilder();
            if (!fileExist)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(table.Columns[i].ColumnName);
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ";");
                }
            }
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var value = row[i];
                    if (value.GetType().Name == "String")
                    {
                        value = Regex.Replace(value.ToString(), @"(\s{2,})|\n", " ");

                    }
                    if (value.GetType().Name == "DateTime")
                    {
                        value = string.Format("{0:yyyy/MM/dd HH:mm:ss}", value);
                    }
                    result.Append(value);
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ";");
                }
            }
            return result.ToString();
        }
    }
}
