using GxGlobal;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DongBoDuLieu
{
    public class SynJob
    {
        public void Push()
        {
           
           try
           {
                WebClient cl = new WebClient();
                string pathFileSyn = this.createrFileSyn();
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                int maGiaoXuRieng;
                int.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString(), out maGiaoXuRieng);

                cl.UploadFile(ConfigurationManager.AppSettings["SERVER"] + @"/SynFileCL/getFileSyn/" + maGiaoXuRieng, pathFileSyn);
                cl.DownloadString(ConfigurationManager.AppSettings["SERVER"] + @"/SynJobCL/excuteByMaGiaoXu/" + maGiaoXuRieng);
            }
           catch (System.Exception ex)
           {
                return;
           }

        }
        public void Pull()
        {
            
            try
            { 
                WebClient cl = new WebClient();
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                int maGiaoXuRieng;
                int.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString(), out maGiaoXuRieng);
                byte[] rs = cl.DownloadData(ConfigurationManager.AppSettings["SERVER"] + @"SynToClientCL/createrFileSyn/" + maGiaoXuRieng);
                string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                if (string.Compare(temp, "1") == 0)
                {
                    string pathDB = Memory.AppPath + "DBSV\\";
                    if (!Directory.Exists(pathDB))
                    {
                        Directory.CreateDirectory(pathDB);
                    }
                    cl.DownloadFile(ConfigurationManager.AppSettings["SERVER_File"] + maGiaoXuRieng + "/" + maGiaoXuRieng + ".zip", pathDB + maGiaoXuRieng + ".zip");
                    FastZip zip = new FastZip();
                    zip.ExtractZip(pathDB + maGiaoXuRieng + ".zip", pathDB, null);
                    compareGiaoXu(pathDB);
                }

            }
            catch (System.Exception ex)
            {
                return;	
            }
        }
        public void compareGiaoXu(string dir)
        {
            GiaoHoCompare giaoho = new GiaoHoCompare(dir, "GiaoHo.csv");
            giaoho.importCacObject();
            giaoho.deleteObjectMaster();

            GiaoDanCompare giaodan = new GiaoDanCompare(dir, "GiaoDan.csv");
            giaodan.getListGiaoHoTracks(giaoho.ListTracks);
            giaodan.importCacObject();//Chua Lam
            giaodan.deleteObjectMaster();



            GiaDinhCompare giadinh = new GiaDinhCompare(dir, "GiaDinh.csv");
            ThanhVienGiaDinhCompare thanhviengiadinh = new ThanhVienGiaDinhCompare(dir, "ThanhVienGiaDinh.csv");
            giadinh.getListGiaoHoTracks(giaoho.ListTracks);
            giadinh.getListGiaoDanTracks(giaodan.ListTracks);
            giadinh.getListThanhVienGiaDinhCSV(thanhviengiadinh.Data);
            giadinh.importCacObject();
            giadinh.deleteObjectMaster();

            thanhviengiadinh.getListTracksGiaDinh(giadinh.ListTracks);
            thanhviengiadinh.getListTracksGiaoDan(giaodan.ListTracks);
            thanhviengiadinh.importCacObject();
            thanhviengiadinh.deleteObjectRelation();

            DotBiTichCompare dotbitich = new DotBiTichCompare(dir,"DotBiTich.csv");
            dotbitich.importCacObject();
            dotbitich.deleteObjectMaster();

            BiTichChiTietCompare bitichchitiet = new BiTichChiTietCompare(dir,"BiTichChiTiet.csv");
            bitichchitiet.getlistTracksDotBiTich(dotbitich.ListTracks);
            bitichchitiet.getListTracksGiaoDan(giaodan.ListTracks);
            bitichchitiet.importCacObject();
            bitichchitiet.deleteObjectRelation();



            HonPhoiCompare honphoi = new HonPhoiCompare(dir, "HonPhoi.csv");
            honphoi.importCacObject();
            honphoi.deleteObjectMaster();

            GiaoDanHonPhoiCompare giaodanhonphoi = new GiaoDanHonPhoiCompare(dir, "GiaoDanHonPhoi.csv");
            giaodanhonphoi.getListTracksGiaoDan(giaodan.ListTracks);
            giaodanhonphoi.getListTracksHonPhoi(honphoi.ListTracks);
            giaodanhonphoi.importCacObject();
            giaodanhonphoi.deleteObjectRelation();

            KhoiGiaoLyCompare khoigiaoly = new KhoiGiaoLyCompare(dir, "KhoiGiaoLy.csv");
            LopGiaoLyCompare lopgiaoly = new LopGiaoLyCompare(dir, "LopGiaoLy.csv");
            ChiTietLopGiaoLyCompare chitietlopgiaoly = new ChiTietLopGiaoLyCompare(dir, "ChiTietLopGiaoLy.csv");
            lopgiaoly.getListChiTietLopGiaoLyCSV(chitietlopgiaoly.Data);
            lopgiaoly.getListGiaoDanTracks(giaodan.ListTracks);
            khoigiaoly.getListGiaoDanTracks(giaodan.ListTracks);
            khoigiaoly.getLopGiaoLyCompare(lopgiaoly);
            khoigiaoly.importCacObject();
            khoigiaoly.deleteObjectMaster();


            lopgiaoly.getListKhoiGiaoLy(khoigiaoly.ListTracks);
            lopgiaoly.importCacObject();
            lopgiaoly.deleteObjectMaster();

            chitietlopgiaoly.getListTracksGiaoDan(giaodan.ListTracks);
            chitietlopgiaoly.getListTracksLopGiaoLy(lopgiaoly.ListTracks);
            chitietlopgiaoly.importCacObject();
            chitietlopgiaoly.deleteObjectRelation();



        }
        private string createrFileSyn()
        {
            string giaoxusynPath = Memory.AppPath + "sync\\";
            if (!Directory.Exists(giaoxusynPath))
            {
                Directory.CreateDirectory(giaoxusynPath);
            }
            DataSet ds = new DataSet();
            ds.Tables.AddRange(new DataTable[] {
                Memory.GetData(SqlConstants.SELECT_ALLBiTichChiTiet,BiTichChiTietConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLCauHinh,CauHinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLChiTietLopGiaoLy,ChiTietLopGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLChuyenXu,ChuyenXuConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLDotBiTich,DotBiTichConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLDuLieuChung,DuLieuChungConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaDinh,GiaDinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoDan,GiaoDanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoDanHonPhoi,GiaoDanHonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoHat,GiaoHatConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoHo,GiaoHoConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoLyVien,GiaoLyVienConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoPhan,GiaoPhanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoXu,GiaoXuConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLHonPhoi,HonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLKhoiGiaoLy,KhoiGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLLinhMuc,LinhMucConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLLopGiaoLy,LopGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLRaoHonPhoi,RaoHonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTanHien,TanHienConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLThanhVienGiaDinh,ThanhVienGiaDinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLVaiTro,VaiTroConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTaiKhoan,TaiKhoanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTenLoaiTaiKhoan,TenLoaiTaiKhoanConst.TableName),
            });
            if (ds.Tables.Count > 0)
            {
                foreach (DataTable item in ds.Tables)
                {
                    string temp = this.ToCSV(item);
                    using (StreamWriter sw = new StreamWriter(giaoxusynPath + item.TableName + ".csv"))
                    {
                        sw.Write(temp);
                    }

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
        private string ToCSV(DataTable table)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ";");
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
