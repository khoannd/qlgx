using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GxGlobal;
using System.IO;
using System.ComponentModel;

namespace GxControl
{
    public class ImportDataMGC: IGxProcess
    {
        private DataProvider provider = null;

        private Dictionary<int, string> dicGiaoXu = new Dictionary<int,string>();
        private Dictionary<int, string> dicLienHe = null;
        private Dictionary<int, string> dicLienHeCu = null;
        private DataTable tblGiaoHo = null;
        private DataTable tblGiaDinh = null;
        private DataTable tblGiaoDan = null;
        private DataTable tblGiaDinhCu = null;
        private DataTable tblGiaoDanCu = null;
        private DataTable tblThanhVienGiaDinh = null;
        private DataTable tblHonPhoi = null;
        private DataTable tblHonPhoiCu = null;
        private DataTable tblBtHonPhoiCu = null;
        private DataTable tblGiaoDanHP = null;
        private int giaDinhHienTai = 0;
        private int maGiaoXuImport = -1;

        private int DataVersion = 1;
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
            set { 
                processData = value;
                maGiaoXuImport = (int)value;
            }
        }

        private string filePathExcel = "";
        private string sheetNameExcel = "";

        ConvertDB.vnConvert.ConvertFont convert = new ConvertDB.vnConvert.ConvertFont();
        public ImportDataMGC(string dbName, string user, string pass)
        {
            provider = new DataProvider(dbName, user, pass);
        }

        public ImportDataMGC(string filePath, string sheetName)
        {
            filePathExcel = filePath;
            sheetNameExcel = sheetName;
        }

        public void Execute()
        {
            importMDBFile();
        }

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
            string sql = "SELECT COUNT(SoDanhBo) AS Tong FROM GiaDinh";
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
                DataTable tblGiaoXuCu = provider.GetData(@"
SELECT gx.MaGiaoPhan, gx.MaGiaoXu, gp.TenGiaoPhan, gh.TenHat, gxl.TenGiaoXu, gx.DiaChi, gx.XaHuyenQuan, gx.TinhTP, gx.DienThoai, gx.EMail
FROM ((GiaoXu AS gx INNER JOIN GXListAct AS gxl ON gx.MaGiaoXu = gxl.MaGiaoXu) INNER JOIN GPList AS gp ON gx.MaGiaoPhan = gp.MaGiaoPhan)  INNER JOIN HatList AS gh ON gx.MaHat = gh.MaHat AND gh.MaGiaoPhan=gx.MaGiaoPhan
GROUP BY gx.MaGiaoPhan, gx.MaGiaoXu, gp.TenGiaoPhan, gh.TenHat, gxl.TenGiaoXu, gx.DiaChi, gx.XaHuyenQuan, gx.TinhTP, gx.DienThoai, gx.EMail
");
                foreach (DataRow row in tblGiaoXuCu.Rows)
                {
                    dicGiaoXu.Add(Memory.GetInt(row["MaGiaoXu"]), Memory.ConvertVNI2UNI(row["TenGiaoXu"].ToString()));
                    if (-1 != maGiaoXuImport && Memory.GetInt(row["MaGiaoXu"]) == maGiaoXuImport)
                    {
                        DataTable tblGiaoXu = Memory.GetData("SELECT * FROM GiaoXu");
                        if (tblGiaoXu.Rows.Count == 0)
                        {
                            int maGiaoHat = Memory.Instance.GetNextId(GiaoHatConst.TableName, GiaoHatConst.MaGiaoHat, true);
                            
                            DataRow newRowGiaoXu = tblGiaoXu.NewRow();
                            tblGiaoXu.Rows.Add(newRowGiaoXu);
                            newRowGiaoXu[GiaoXuConst.MaGiaoHat] = maGiaoHat;
                            newRowGiaoXu[GiaoXuConst.MaGiaoXu] = row["MaGiaoXu"];
                            newRowGiaoXu[GiaoXuConst.TenGiaoXu] = Memory.ConvertVNI2UNI(row["TenGiaoXu"].ToString());
                            newRowGiaoXu[GiaoXuConst.DiaChi] = getDiaChi(row);
                            newRowGiaoXu[GiaoXuConst.DienThoai] = row["DienThoai"];
                            newRowGiaoXu[GiaoXuConst.Email] = row["EMail"];
                            DataSet dsTmp = new DataSet();
                            tblGiaoXu.TableName = GiaoXuConst.TableName;
                            dsTmp.Tables.Add(tblGiaoXu);
                            int rs = (int)Memory.UpdateDataSet(dsTmp);
                            if (rs != -1)
                            {
                                string sql = "INSERT INTO GiaoHat VALUES(?, ?, ?, ?, ?)";
                                rs = (int)Memory.ExecuteSqlCommand(sql, maGiaoHat, row["MaGiaoPhan"], Memory.ConvertVNI2UNI(row["TenHat"].ToString()), "", 0);
                                sql = "INSERT INTO GiaoPhan VALUES(?, ?, ?, ?)";
                                rs = (int)Memory.ExecuteSqlCommand(sql, row["MaGiaoPhan"], Memory.ConvertVNI2UNI(row["TenGiaoPhan"].ToString()), "", 0);
                            }
                        }
                    }
                }
                DataTable tbl = provider.GetData("SELECT * FROM ListKhuXom");
                if(tbl != null)
                {
                    DataVersion = 2;
                }
                else
                {
                    Memory.ClearError();
                    tbl = provider.GetData("SELECT KhuXom FROM GiaDinh WHERE KhuXom IS NOT NULL AND KhuXom<>'' GROUP BY KhuXom");
                }
                int currentMaGiaoHo = Memory.Instance.GetNextId("GiaoHo", "MaGiaoHo", true);
                tblGiaoHo = Memory.GetData("SELECT * FROM GiaoHo");
                tblGiaoHo.TableName = "GiaoHo";
                if (!Memory.HasError())
                {
                    foreach (DataRow row in tbl.Rows)
                    {
                        //check user abort
                        bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                        //if user abort progress, return
                        if (userStop) return false;

                        string tenGiaoHo = Memory.ConvertVNI2UNIUpperFirstChar(row["KhuXom"].ToString());
                        tenGiaoHo = Memory.AutoUpperFirstChar(tenGiaoHo, ConvertDB.vnConvert.FontCase.UpperCaseFirstChar, false, false);
                        //int maGiaoHo = -1;
                        if (OnExecuting != null) OnExecuting("Đang nhập giáo họ: " + tenGiaoHo.Replace("'", "''"), EventArgs.Empty);
                        DataRow[] rows = tblGiaoHo.Select("TenGiaoHo='" + tenGiaoHo.Replace("'", "''") + "'");
                        if (rows != null && rows.Length > 0)
                        {
                            rows[0]["TenGiaoHo"] = tenGiaoHo;
                            continue;
                        }
                        
                        DataRow newRow = tblGiaoHo.NewRow();
                        newRow["MaGiaoHo"] = currentMaGiaoHo++;
                        newRow["TenGiaoHo"] = tenGiaoHo;
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
                DataTable tblLienHe = provider.GetData("SELECT * FROM ListLienhe");
                dicLienHeCu = new Dictionary<int, string>();
                foreach (DataRow rowLH in tblLienHe.Rows)
                {
                    dicLienHeCu.Add(Memory.GetInt(rowLH["IDLienHe"]), Memory.ConvertVNI2UNI(rowLH["LienHe"].ToString().Trim()).ToLower());
                }
                dicLienHe = Memory.GetQuanHeList(true);

                tblGiaoDan = Memory.GetData("SELECT * FROM GiaoDan");
                
                tblGiaoDan.Columns.Add("MaGiaoDanCu", typeof(int));
                tblGiaDinh = Memory.GetData("SELECT * FROM GiaDinh");
                tblGiaDinh.Columns.Add("MaGiaDinhCu", typeof(int));
                tblThanhVienGiaDinh = Memory.GetData("SELECT * FROM ThanhVienGiaDinh");

                tblGiaoDanCu = provider.GetData("SELECT  a.*, b.DanToc AS TenDanToc FROM LyLich AS a LEFT JOIN ListDanToc b ON a.IDDanToc=b.IDDanToc ");
                if(DataVersion == 2)
                {
                    tblGiaDinhCu = provider.GetData("SELECT a.*, b.KhuXom FROM GiaDinh a LEFT JOIN ListKhuXom b ON a.MaKhuXom=b.MaKhuXom");
                }
                else
                {
                    tblGiaDinhCu = provider.GetData("SELECT * FROM GiaDinh");
                }
                
                //DataTable tblBtRuaToi = provider.GetData("SELECT BtRuaToi.MaGiaoPhan, BtRuaToi.MaHat, BtRuaToi.SoDanhBo, BtRuaToi.MaGiaoXu, BtRuaToi.TenThanh, BtRuaToi.HoVaTen, BtRuaToi.SinhNgay, BtRuaToi.SinhTai, BtRuaToi.Phai, BtRuaToi.TenCha, BtRuaToi.TenMe, BtRuaToi.DiaChi, BtRuaToi.SoRuaToi, BtRuaToi.NgayRuaToi, BtRuaToi.BuCacPhep, BtRuaToi.LinhMucRuaToi, BtRuaToi.DoDauRuaToi, BtRuaToi.NgayThemSuc, BtRuaToi.NoiThemSuc, BtRuaToi.NgayRuocLe, BtRuaToi.NgayHonPhoi, BtRuaToi.KetHonVoi, BtRuaToi.NoiHonPhoi, BtRuaToi.NguoiChung1, BtRuaToi.NguoiChung2, BtRuaToi.NgayThanhHien, BtRuaToi.NgayKhanTron FROM BtRuaToi ");
                DataTable tblBtRuaToi = provider.GetData("SELECT * FROM BtRuaToi");
                DataTable tblBtRuocLe = provider.GetData("SELECT * FROM BtRuocLe");
                DataTable tblBtThemSuc = provider.GetData("SELECT * FROM BtThemSuc");
                chuyenMaTableBiTich(tblBtRuaToi);
                chuyenMaTableBiTich(tblBtRuocLe);
                chuyenMaTableBiTich(tblBtThemSuc);
                
                if (tblGiaoDanCu == null)
                {
                    Memory.Instance.Error = new Exception("Không tìm thấy dữ liệu gia đình và giáo dân");
                    if (OnError != null) OnError(Memory.Instance.Error.Message, new CancelEventArgs());
                    return false;
                }
                string soGiaDinh = "";
                int maGiaDinh = -1;
                int maGiaoHo = -1;
                giaDinhHienTai = 0;
                int maxMaGiaDinh = 0;
                for (int i = 0; i < tblGiaoDanCu.Rows.Count; i++)
                {
                    //check user abort
                    bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                    //if user abort progress, return
                    if (userStop) return false;
                    DataRow row = tblGiaoDanCu.Rows[i];
                    string tmp = row["DanhBoGiaDinh"].ToString();
                    int quanhe = -1;
                    int.TryParse(row["IDLienHe"].ToString(), out quanhe);
                    if (soGiaDinh != tmp)
                    {
                        soGiaDinh = tmp;
                        DataRow rowGiaDinh = ImportGiaDinh(row);
                        if (rowGiaDinh != null)
                        {
                            maGiaDinh = Memory.GetInt(rowGiaDinh[GiaDinhConst.MaGiaDinh]);
                            maGiaoHo = Memory.GetInt(rowGiaDinh[GiaDinhConst.MaGiaoHo]);
                            if (maGiaDinh > maxMaGiaDinh)
                            {
                                maxMaGiaDinh = maGiaDinh;
                                giaDinhHienTai++;
                                Memory.Instance.SetMemory("DuLieuHienTai", giaDinhHienTai);
                            }
                            if (OnExecuting != null) OnExecuting(string.Format("Đang nhập gia đình: {0}", rowGiaDinh["TenGiaDinh"]), EventArgs.Empty);
                        }
                        else
                        {
                            maGiaoHo = -1;
                            maGiaDinh = -1;
                        }
                    }
                    DataRow rowGiaoDan = ImportGiaoDan(row, maGiaoHo);

                    if (maGiaDinh != -1 && rowGiaoDan != null)
                    {
                        DataRow rowBtRuaToi = findSoBiTich(rowGiaoDan, tblBtRuaToi);
                        if (rowBtRuaToi != null)
                        {
                            rowGiaoDan[GiaoDanConst.SoRuaToi] = rowBtRuaToi["SoRuaToi"];
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NgayRuaToi]) && !Memory.IsNullOrEmpty(rowBtRuaToi["NgayRuaToi"]))
                            {
                                rowGiaoDan[GiaoDanConst.NgayRuaToi] = Memory.FormatDateString(rowBtRuaToi["NgayRuaToi"].ToString());
                            }
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.ChaRuaToi]) && !Memory.IsNullOrEmpty(rowBtRuaToi["LinhMucRuaToi"]))
                            {
                                rowGiaoDan[GiaoDanConst.ChaRuaToi] = rowBtRuaToi["LinhMucRuaToi"];
                            }
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi]) && !Memory.IsNullOrEmpty(rowBtRuaToi["DoDauRuaToi"]))
                            {
                                rowGiaoDan[GiaoDanConst.NguoiDoDauRuaToi] = rowBtRuaToi["DoDauRuaToi"];
                            }
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NgayThemSuc]) && !Memory.IsNullOrEmpty(rowBtRuaToi["NgayThemSuc"]))
                            {
                                rowGiaoDan[GiaoDanConst.NgayThemSuc] = Memory.FormatDateString(rowBtRuaToi["NgayThemSuc"].ToString());
                            }
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NoiThemSuc]) && !Memory.IsNullOrEmpty(rowBtRuaToi["NoiThemSuc"]))
                            {
                                rowGiaoDan[GiaoDanConst.NoiThemSuc] = rowBtRuaToi["NoiThemSuc"];
                            }
                            if (Memory.IsNullOrEmpty(rowGiaoDan[GiaoDanConst.NgayRuocLe]) && !Memory.IsNullOrEmpty(rowBtRuaToi["NgayRuocLe"]))
                            {
                                rowGiaoDan[GiaoDanConst.NgayRuocLe] = Memory.FormatDateString(rowBtRuaToi["NgayRuocLe"].ToString());
                            }
                            if (!Memory.IsNullOrEmpty(rowBtRuaToi["NgayHonPhoi"]) || !Memory.IsNullOrEmpty(rowBtRuaToi["KetHonVoi"]) || !Memory.IsNullOrEmpty(rowBtRuaToi["NoiHonPhoi"]))
                            {
                                rowGiaoDan[GiaoDanConst.DaCoGiaDinh] = -1;
                            }
                            if (!Memory.IsNullOrEmpty(rowBtRuaToi["GhiChu"]))
                            {
                                string ghichu = Memory.ConvertVNI2UNI(rowBtRuaToi["GhiChu"].ToString());
                                ghichu = string.Join("\r\n", new string[] { rowGiaoDan[GiaoDanConst.GhiChu].ToString(), rowBtRuaToi["GhiChu"].ToString() });
                                rowGiaoDan[GiaoDanConst.GhiChu] = ghichu;
                            }
                        }
                        DataRow rowBtRuocLe = findSoBiTich(rowGiaoDan, tblBtRuocLe);
                        if (rowBtRuocLe != null)
                        {
                            rowGiaoDan[GiaoDanConst.SoThemSuc] = rowBtRuocLe["SoRuocLe"];
                        }
                        DataRow rowBtThemSuc = findSoBiTich(rowGiaoDan, tblBtThemSuc);
                        if (rowBtThemSuc != null)
                        {
                            rowGiaoDan[GiaoDanConst.SoThemSuc] = rowBtThemSuc["SoThemSuc"];
                        }
                        int vaitro = getVaiTro(quanhe, Memory.GetInt(row["Phai"]));
                        assignThanhVienGiaDinh(maGiaDinh, Memory.GetInt(rowGiaoDan[GiaoDanConst.MaGiaoDan]), vaitro);
                    }
                }
                if (OnExecuting != null) OnExecuting("Đang nhập thông tin hôn phối...", EventArgs.Empty);
                importCacHonPhoi();

                if (OnExecuting != null) OnExecuting("Đang hoàn tất việc cập nhật dữ liệu vào chương trình...", EventArgs.Empty);
                DataSet ds = new DataSet();
                tblGiaDinh.TableName = GiaDinhConst.TableName;
                ds.Tables.Add(tblGiaDinh);
                tblGiaoDan.TableName = GiaoDanConst.TableName;
                ds.Tables.Add(tblGiaoDan);
                tblThanhVienGiaDinh.TableName = ThanhVienGiaDinhConst.TableName;
                ds.Tables.Add(tblThanhVienGiaDinh);
                tblHonPhoi.TableName = HonPhoiConst.TableName;
                ds.Tables.Add(tblHonPhoi);
                tblGiaoDanHP.TableName = GiaoDanHonPhoiConst.TableName;
                ds.Tables.Add(tblGiaoDanHP);
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

        private int getVaiTro(int quanhe, int phai)
        {
            int vaitro = 100;
            if(DataVersion == 2 && quanhe == 1)
            {
                vaitro = 0; //chủ gia đình, không chắc là chồng hay vợ mà còn tùy thuộc vào Phái
            }
            else if (quanhe == 15 || quanhe == 3) vaitro = 0; //15 = chồng
            else if (quanhe == 16 || quanhe == 4) vaitro = 1; //16 = vợ
            //tim vo, truong hop gia dinh co den 2 vo -> chang hieu tai sao
            if (vaitro == 0 || vaitro == 1)
            {
                if (phai == 1) vaitro = 0; else vaitro = 1;
            }
            else
            {
                foreach (var lh in dicLienHeCu)
                {
                    if (lh.Key == quanhe)
                    {
                        foreach (var item in dicLienHe)
                        {
                            if (item.Value.ToLower() == lh.Value)
                            {
                                vaitro = item.Key;
                                return vaitro;
                            }
                        }
                    }
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

        private DataRow findGiaDinh(DataRow row)
        {
            string s = string.Format("MaGiaDinhCu = {0} OR MaGiaDinhRieng = '{0}'", row["DanhBoGiaDinh"]);
            DataRow[] rows = tblGiaDinh.Select(s);
            if (rows != null && rows.Length > 0)
            {
                return rows[0];
            }
            return null;
        }

        private string getGhiChu(DataRow row)
        {
            string ghichu = "";
            if (row["NangKhieu"].ToString() != "")
            {
                ghichu = string.Concat("Năng khiếu: ", row["NangKhieu"].ToString());
            }
            if (row["GhiChu"].ToString() != "")
            {
                ghichu = string.Concat(ghichu, "\r\n", row["GhiChu"].ToString());
            }
            return Memory.ConvertVNI2UNI(ghichu);
        }

        private bool IsValidDb()
        {
            //check table giaoxu
            DataTable tbl = provider.GetData("SELECT * FROM LyLich WHERE 1=2");
            return tbl != null;
        }

        private int getMaGiaoHo(object maGiaoXuCu, object maGiaoHoCu, object tenGiaoHo)
        {
            if (-1 != maGiaoXuImport && int.Parse(maGiaoXuCu.ToString()) != maGiaoXuImport) //ngoài xứ
            {
                return 0;
            }
            DataRow[] ho = tblGiaoHo.Select(string.Format("TenGiaoHo = '{0}'", tenGiaoHo.ToString().Replace("'", "''")));
            if (ho != null && ho.Length > 0)
            {
                return Memory.GetInt(ho[0][GiaoHoConst.MaGiaoHo]);
            }

            ho = tblGiaoHo.Select("TenGiaoHo = 'Ngoài Xứ'");
            if (ho != null && ho.Length > 0)
            {
                return Memory.GetInt(ho[0][GiaoHoConst.MaGiaoHo]);
            }

            return -1;
        }

        private DataRow ImportGiaDinh(DataRow row)
        {
            try
            {
                if(row["DanhBoGiaDinh"].ToString() == "" || row["DanhBoGiaDinh"].ToString() == "0")
                {
                    return null;
                }
                DataRow rowTmp = findGiaDinh(row);
                if (rowTmp != null)
                {
                    return rowTmp;
                }
                //find row gia dinh cu
                string select = string.Format("SoDanhBo={0}", row["DanhBoGiaDinh"]);
                DataRow[] rows = tblGiaDinhCu.Select(select);
                if (rows != null && rows.Length > 0)
                {
                    row = rows[0];
                    int maGiaDinh = Memory.Instance.GetNextId(GiaDinhConst.TableName, GiaDinhConst.MaGiaDinh, false);
                    DataRow row1 = tblGiaDinh.NewRow();
                    assignGiaDinh(maGiaDinh, row1, row);
                    tblGiaDinh.Rows.Add(row1);
                    return row1;
                }
                

            }
            catch (Exception ex)
            {
                sw.WriteLine("ImportGiaDinh: " + ex.Message);
                Memory.Instance.Error = ex;
                if (OnError != null) OnError("Có lỗi khi nhập dữ liệu gia đình" + Memory.ConvertVNI2UNI(row["GIA DINH"].ToString()), new CancelEventArgs());
            }
            return null;
        }

        private DataRow findGiaoDanByName(DataRow rowGiaoDanCu)
        {
            string s = "";

            if (rowGiaoDanCu["SinhNgay"] != DBNull.Value)
            {
                s = string.Format("TenThanh = '{0}' AND HoTen='{1}' AND NgaySinh = '{2}'",
                                             Memory.ConvertVNI2UNI(rowGiaoDanCu["TenThanh"].ToString()),
                                             Memory.ConvertVNI2UNI(rowGiaoDanCu["HoVaTen"].ToString()),
                                             Memory.FormatDateString(rowGiaoDanCu["SinhNgay"].ToString()));
            }
            else
            {
                if (rowGiaoDanCu["NgayRuaToi"] != DBNull.Value)
                {
                    s = string.Format("TenThanh = '{0}' AND HoTen='{1}' AND NgaySinh = '{2}' AND NgayRuaToi IS NULL",
                                                 Memory.ConvertVNI2UNI(rowGiaoDanCu["TenThanh"].ToString()),
                                                 Memory.ConvertVNI2UNI(rowGiaoDanCu["HoVaTen"].ToString()),
                                                 Memory.GetDateString(rowGiaoDanCu["NgayRuaToi"]));
                }
                else
                {
                    s = string.Format("TenThanh = '{0}' AND HoTen='{1}' AND NgayRuaToi IS NULL AND NgaySinh IS NULL",
                                             Memory.ConvertVNI2UNI(rowGiaoDanCu["TenThanh"].ToString().Replace("'", "''")),
                                             Memory.ConvertVNI2UNI(rowGiaoDanCu["HoVaTen"].ToString().Replace("'", "''")),
                                             Memory.FormatDateString(rowGiaoDanCu["NgayRuaToi"].ToString())
                                             );
                }

            }

            DataRow[] rows = tblGiaoDan.Select(s);
            if (rows != null && rows.Length > 0)
            {
                return rows[0];
            }
            return null;
        }
        private DataRow findGiaoDanByID(string soDanhBo)
        {
            string s = string.Format("MaGiaoDanCu={0} OR MaNhanDang='{0}'", soDanhBo);
            DataRow[] rows = tblGiaoDan.Select(s);
            if (rows.Length > 0)
            {
                return rows[0];
            }
            return null;
        }
        /// <summary>
        /// ImportGiaoDan
        /// </summary>
        /// <param name="row">Row MGC.GiaoDan</param>
        /// <returns></returns>
        private DataRow ImportGiaoDan(DataRow row, int maGiaoHo)
        {
            try
            {
                DataRow row1 = findGiaoDanByID(row["soDanhBo"].ToString());
                int maGiaoDan = -1;
                if (row1 == null)
                {
                    row1 = tblGiaoDan.NewRow();
                    maGiaoDan = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, false);
                    assignGiaoDan(maGiaoDan, maGiaoHo, row1, row);
                    tblGiaoDan.Rows.Add(row1);
                }
                
                return row1;
            }
            catch (Exception ex)
            {
                sw.WriteLine("ImportGiaoDan: " + ex.Message);
                Memory.Instance.Error = ex;
                if (OnError != null)
                {
                    OnError(string.Format("Lỗi khi nhập dữ liệu giáo dân: Tên thánh: {0}, Họ Tên:{1}, Ngày sinh: {2}",
                        Memory.ConvertVNI2UNI(row["TenThanh"].ToString()),
                        Memory.ConvertVNI2UNIUpperFirstChar(row["HoVaTen"].ToString()),
                        row["SinhNgay"] == DBNull.Value ? "Chưa xác định" : Memory.FormatDateString(row["SinhNgay"].ToString())),
                    new CancelEventArgs());
                }
            }
            return null;
        }

        private void assignGiaoDan(int maGiaoDan, int maGiaoHo, DataRow newRow, DataRow oldRow)
        {
            newRow[GiaoDanConst.MaGiaoDan] = maGiaoDan;
            newRow[GiaoDanConst.MaGiaoHo] = maGiaoXuImport == -1 ? maGiaoHo : Memory.GetInt(oldRow["MaGiaoXu"]) != maGiaoXuImport ? 0 : maGiaoHo;
            newRow[GiaoDanConst.HoTen] = Memory.ConvertVNI2UNI(oldRow["HoVaTen"].ToString());
            newRow[GiaoDanConst.Phai] = oldRow["Phai"].ToString() == "1" ? GxConstants.NAM : GxConstants.NU;
            newRow[GiaoDanConst.TenThanh] = Memory.ConvertVNI2UNI(oldRow["TenThanh"].ToString());
            newRow[GiaoDanConst.NgaySinh] = Memory.FormatDateString(oldRow["SinhNgay"].ToString());
            newRow[GiaoDanConst.NoiSinh] = Memory.ConvertVNI2UNI(oldRow["SinhTai"].ToString());
            //newRow[GiaoDanConst.SoRuaToi] = oldRow[""];
            newRow[GiaoDanConst.NgayRuaToi] = Memory.FormatDateString(oldRow["NgayRuaToi"].ToString());
            newRow[GiaoDanConst.NoiRuaToi] = Memory.ConvertVNI2UNI(oldRow["NoiRuaToi"].ToString());
            newRow[GiaoDanConst.ChaRuaToi] = Memory.ConvertVNI2UNI(oldRow["LinhMucRuaToi"].ToString());
            newRow[GiaoDanConst.NguoiDoDauRuaToi] = Memory.ConvertVNI2UNI(oldRow["DoDauRuaToi"].ToString());
            //newRow[GiaoDanConst.SoRuocLe] = oldRow[""];
            newRow[GiaoDanConst.NgayRuocLe] = Memory.FormatDateString(oldRow["NgayRuocLe"].ToString());
            newRow[GiaoDanConst.NoiRuocLe] = Memory.ConvertVNI2UNI(oldRow["NoiRuocLe"].ToString());
            //newRow[GiaoDanConst.ChaRuocLe] = Memory.ConvertVNI2UNI(oldRow[""].ToString());
            //newRow[GiaoDanConst.SoThemSuc] = oldRow[""];
            newRow[GiaoDanConst.NgayThemSuc] = Memory.FormatDateString(oldRow["NgayThemSuc"].ToString());
            newRow[GiaoDanConst.NoiThemSuc] = Memory.ConvertVNI2UNI(oldRow["NoiThemSuc"].ToString());
            newRow[GiaoDanConst.ChaThemSuc] = Memory.ConvertVNI2UNI(oldRow["DucChaThemSuc"].ToString());
            newRow[GiaoDanConst.NguoiDoDauThemSuc] = Memory.ConvertVNI2UNI(oldRow["DoDauThemSuc"].ToString());
            //newRow[GiaoDanConst.TrinhDoVanHoa] = Memory.ConvertVNI2UNI(oldRow[""].ToString());
            //newRow[GiaoDanConst.NgheNghiep] = Memory.ConvertVNI2UNI(oldRow[""].ToString());
            //newRow[GiaoDanConst.ConHoc] = oldRow[""];
            newRow[GiaoDanConst.QuaDoi] = oldRow["NgayTuTran"].ToString() != "" ? true : false; //NoiTuTran chua co
            newRow[GiaoDanConst.NgayQuaDoi] = oldRow["NgayTuTran"].ToString() != "" && oldRow["NgayTuTran"].ToString() != "X" ? Memory.FormatDateString(oldRow["NgayTuTran"].ToString()) : "";
            newRow[GiaoDanConst.DienThoai] = oldRow["DienThoai"];
            newRow[GiaoDanConst.Email] = oldRow["EMail"];
            //newRow[GiaoDanConst.DaXoa] = oldRow[""];
            newRow[GiaoDanConst.GhiChu] = getGhiChu(oldRow);
            newRow[GiaoDanConst.UpdateDate] = DateTime.Now;
            newRow[GiaoDanConst.HoTenCha] = Memory.ConvertVNI2UNI(oldRow["TenCha"].ToString());
            newRow[GiaoDanConst.HoTenMe] = Memory.ConvertVNI2UNI(oldRow["TenMe"].ToString());
            //newRow[GiaoDanConst.DaCoGiaDinh] = oldRow[""];
            //newRow[GiaoDanConst.GiaoDanAo] = oldRow[""];
            //newRow[GiaoDanConst.TanTong] = oldRow[""];
            newRow[GiaoDanConst.MaNhanDang] = oldRow["SoDanhBo"];
            int maGiaoXuCu = Memory.GetInt(oldRow["MaGiaoXu"]);
            newRow[GiaoDanConst.ThuocGiaoXu] = dicGiaoXu.ContainsKey(maGiaoXuCu) ? dicGiaoXu[maGiaoXuCu] : "";
            newRow[GiaoDanConst.DiaChi] = getDiaChi(oldRow);
            newRow[GiaoDanConst.NoiQuaDoi] = Memory.ConvertVNI2UNI(oldRow["NoiTuTran"].ToString());
            newRow[GiaoDanConst.NoiAnTang] = Memory.ConvertVNI2UNI(oldRow["NoiAnTang"].ToString());
            newRow[GiaoDanConst.DanToc] = Memory.ConvertVNI2UNI(oldRow["TenDanToc"].ToString());
        }

        private void assignGiaDinh(int maGiaDinh, DataRow newRow, DataRow oldRow)
        {
            newRow[GiaDinhConst.MaGiaDinh] = maGiaDinh;
            newRow[GiaDinhConst.MaGiaoHo] = getMaGiaoHo(Memory.GetInt(oldRow["MaGiaoXu"]), oldRow["KhuXom"], Memory.ConvertVNI2UNIUpperFirstChar(oldRow["KhuXom"].ToString()));
            newRow[GiaDinhConst.TenGiaDinh] = Memory.ConvertVNI2UNI(oldRow["TenChuGiaDinh"].ToString());
            //newRow[GiaDinhConst.GhiChu] = oldRow[""];
            newRow[GiaDinhConst.DienThoai] = oldRow["DienThoai"];
            newRow[GiaDinhConst.DiaChi] = getDiaChi(oldRow);
            newRow[GiaDinhConst.UpdateDate] = DateTime.Now;
            newRow[GiaDinhConst.MaNhanDang] = Memory.GetGiaDinhKey(maGiaDinh);
            newRow[GiaDinhConst.MaGiaDinhRieng] = oldRow["SoDanhBo"];
            newRow["MaGiaDinhCu"] = oldRow["SoDanhBo"];
        }
        private string getDiaChi(DataRow row)
        {
            List<string> lst = new List<string>();
            if (!string.IsNullOrEmpty(row["DiaChi"].ToString())) lst.Add(Memory.ConvertVNI2UNI(row["DiaChi"].ToString()));
            if (!string.IsNullOrEmpty(row["XaHuyenQuan"].ToString())) lst.Add(Memory.ConvertVNI2UNI(row["XaHuyenQuan"].ToString()));
            if (!string.IsNullOrEmpty(row["TinhTP"].ToString())) lst.Add(Memory.ConvertVNI2UNI(row["TinhTP"].ToString()));

            return string.Join(" - ", lst.ToArray());
        }

        private void importCacHonPhoi()
        {
            try
            {
                string sql = @"SELECT LyLich.TenThanh, LyLich.HoVaTen, HonPhoi.*
FROM LyLich INNER JOIN HonPhoi ON (LyLich.SoDanhBo = HonPhoi.SoDanhBo)
";
                tblHonPhoiCu = provider.GetData(sql);

                //Import from BtHonPhoi
                tblBtHonPhoiCu = provider.GetData("SELECT * FROM BtHonPhoi");
                tblHonPhoi = Memory.GetData("SELECT * FROM HonPhoi ");
                //tblHonPhoi.Columns.Add("MaHonPhoiCu", typeof(int));
                tblGiaoDanHP = Memory.GetData("SELECT * FROM GiaoDanHonPhoi ");
                if (tblBtHonPhoiCu != null && tblBtHonPhoiCu.Rows.Count > 0)
                {
                    foreach (DataRow row in tblBtHonPhoiCu.Rows)
                    {
                        List<DataRow> lstGiaoDan = findGiaoDanHonHoi(row);
                        
                        if (lstGiaoDan.Count > 1)
                        {
                            DataRow rowHonPhoi = importBtHonPhoi(row);
                            //kiem tra hon phoi da ton tai chua
                            bool existed = kiemTraHonPhoiTonTai(Memory.GetInt(lstGiaoDan[0]["MaGiaoDan"]), Memory.GetInt(lstGiaoDan[1]["MaGiaoDan"]));
                            //neu ton tai roi thi thoi
                            if (existed)
                            {
                                continue;
                            }
                            foreach (var item in lstGiaoDan)
                            {
                                if (rowHonPhoi[HonPhoiConst.CachThucHonPhoi].ToString() == "")
                                {
                                    rowHonPhoi[HonPhoiConst.CachThucHonPhoi] = getCachThucHonPhoi(item[GiaoDanConst.MaNhanDang].ToString());
                                }
                                item[GiaoDanConst.DaCoGiaDinh] = -1;
                                assignGiaoDanHonPhoiRow(item[GiaoDanConst.MaGiaoDan], rowHonPhoi[HonPhoiConst.MaHonPhoi], 1);
                            }
                        }

                    }
                }

                //Import from HonPhoi
                if (tblHonPhoiCu != null && tblHonPhoiCu.Rows.Count > 0)
                {
                    foreach (DataRow row1 in tblHonPhoiCu.Rows)
                    {
                        DataRow row2 = findHonPhoi(row1);
                        if (row2 != null)
                        {
                            DataRow giaoDan1 = findGiaoDanByID(row1["SoDanhBo"].ToString());
                            DataRow giaoDan2 = findGiaoDanByID(row2["SoDanhBo"].ToString());
                            if (giaoDan1 != null && giaoDan2 != null)
                            {
                                //kiem tra hon phoi da ton tai chua
                                bool existed = kiemTraHonPhoiTonTai(Memory.GetInt(giaoDan1["MaGiaoDan"]), Memory.GetInt(giaoDan2["MaGiaoDan"]));
                                //neu ton tai roi thi thoi
                                if (existed)
                                {
                                    continue;
                                }
                                //neu chua ton tai
                                int maHonPhoi = Memory.Instance.GetNextId(HonPhoiConst.TableName, HonPhoiConst.MaHonPhoi, false);
                                DataRow row = tblHonPhoi.NewRow();
                                tblHonPhoi.Rows.Add(row);
                                row[HonPhoiConst.NoiHonPhoi] = Memory.ConvertVNI2UNI(row1["NoiHonPhoi"].ToString());
                                row[HonPhoiConst.NgayHonPhoi] = Memory.FormatDateString(row1["NgayHonPhoi"].ToString());
                                row[HonPhoiConst.LinhMucChung] = Memory.ConvertVNI2UNI(row1["LinhMucHonPhoi"].ToString());
                                row[HonPhoiConst.NguoiChung1] = Memory.ConvertVNI2UNI(row1["NguoiChung1"].ToString());
                                row[HonPhoiConst.NguoiChung2] = Memory.ConvertVNI2UNI(row1["NguoiChung2"].ToString());
                                row[HonPhoiConst.CachThucHonPhoi] = getCachThucHonPhoi(row1["SoDanhBo"].ToString());
                                row[HonPhoiConst.MaNhanDang] = Memory.GetHonPhoiKey(maHonPhoi);
                                row[HonPhoiConst.UpdateDate] = DateTime.Now;
                                row[HonPhoiConst.MaHonPhoi] = maHonPhoi;

                                assignGiaoDanHonPhoiRow(giaoDan1["MaGiaoDan"], maHonPhoi, 1);
                                assignGiaoDanHonPhoiRow(giaoDan2["MaGiaoDan"], maHonPhoi, 1);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sw.WriteLine("ImportGiaoDanHonPhoi: " + ex.Message);
                Memory.Instance.Error = ex;
                if (OnError != null)
                {
                    OnError("Lỗi khi nhập dữ liệu hôn phối", new CancelEventArgs());
                }
            }
            
        }

        private bool kiemTraHonPhoiTonTai(int maGiaoDan1, int maGiaoDan2)
        {
            string s = string.Format("MaGiaoDan={0} OR MaGiaoDan={1}", maGiaoDan1, maGiaoDan2);
            DataView view = new DataView(tblGiaoDanHP, s, "MaHonPhoi ASC", DataViewRowState.CurrentRows);
            DataTable tmp = view.ToTable();
            bool existed = false;
            foreach (DataRow r in tmp.Rows)
            {
                DataRow[] rows = tmp.Select(string.Format("MaHonPhoi={0}", r["MaHonPhoi"]));
                if (rows.Length > 1)
                {
                    existed = true;
                    break;
                }
            }
            return existed;
        }

        private void assignGiaoDanHonPhoiRow(object maGiaoDan, object maHonPhoi, int soTT)
        {
            DataRow[] rows = tblGiaoDanHP.Select(string.Format("MaGiaoDan={0} AND MaHonPhoi={1}", maGiaoDan, maHonPhoi));
            if (rows.Length > 0) return;

            DataRow newRow = tblGiaoDanHP.NewRow();
            tblGiaoDanHP.Rows.Add(newRow);
            newRow[GiaoDanHonPhoiConst.MaGiaoDan] = maGiaoDan;
            newRow[GiaoDanHonPhoiConst.MaHonPhoi] = maHonPhoi;
            newRow[GiaoDanHonPhoiConst.SoThuTu] = soTT;
        }

        private DataRow importBtHonPhoi(DataRow rowHonPhoiCu)
        {
            DataRow[] rows = tblHonPhoi.Select(string.Format("MaNhanDang='{0}'", rowHonPhoiCu["SoDanhBo"].ToString().Replace("'", "''")));
            if (rows.Length > 0)
            {
                return rows[0];
            }
            int maHonPhoi = Memory.Instance.GetNextId(HonPhoiConst.TableName, HonPhoiConst.MaHonPhoi, false);

            DataRow row = tblHonPhoi.NewRow();
            tblHonPhoi.Rows.Add(row);
            row[HonPhoiConst.SoHonPhoi] = rowHonPhoiCu["SoHonPhoi"];
            row[HonPhoiConst.NoiHonPhoi] = Memory.ConvertVNI2UNI(rowHonPhoiCu["NoiHonPhoi"].ToString());
            row[HonPhoiConst.NgayHonPhoi] = Memory.FormatDateString(rowHonPhoiCu["NgayHonPhoi"].ToString());
            row[HonPhoiConst.LinhMucChung] = Memory.ConvertVNI2UNI(rowHonPhoiCu["LinhMucHonPhoi"].ToString());
            row[HonPhoiConst.NguoiChung1] = Memory.ConvertVNI2UNI(rowHonPhoiCu["NguoiChung1"].ToString());
            row[HonPhoiConst.NguoiChung2] = Memory.ConvertVNI2UNI(rowHonPhoiCu["NguoiChung2"].ToString());
            row[HonPhoiConst.CachThucHonPhoi] = "";
            List<string> lstGhichu=new List<string>();
            if(rowHonPhoiCu["NaGhiChu"].ToString()!="")lstGhichu.Add(rowHonPhoiCu["NaGhiChu"].ToString());
            if(rowHonPhoiCu["NuGhiChu"].ToString()!="")lstGhichu.Add(rowHonPhoiCu["NuGhiChu"].ToString());
            row[HonPhoiConst.GhiChu] = Memory.ConvertVNI2UNI(string.Join("; ", lstGhichu.ToArray()));
            row[HonPhoiConst.MaNhanDang] = rowHonPhoiCu["SoDanhBo"];
            row[HonPhoiConst.UpdateDate] = DateTime.Now;
            row[HonPhoiConst.MaHonPhoi] = maHonPhoi;

            return row;
        }

        private string getCachThucHonPhoi(string soDanhBo)
        {
            DataRow[] rows = tblHonPhoiCu.Select(string.Format("SoDanhBo='{0}'", soDanhBo));
            if (rows.Length > 0)
            {
                int tinhtrang = Memory.GetInt(rows[0]["TinhTrangHP"]);
                switch (tinhtrang)
                {
                    case 1: return "Hợp pháp";
                    case 2: return "Ly Thân";
                    case 3: return "Ly dị";
                    case 4: return "Không theo phép đạo";
                    default:
                        return "";
                }
            }
            return "";
        }

        private List<DataRow> findGiaoDanHonHoi(DataRow rowHonPhoiCu)
        {
            List<DataRow> lst = new List<DataRow>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //tim nguoi chong
            dic.Add(GiaoDanConst.NgayRuaToi, Memory.FormatDateString(rowHonPhoiCu["NaNgayRuaToi"].ToString()));
            //dic.Add(GiaoDanConst.NoiRuaToi, Memory.ConvertVNI2UNI(rowHonPhoiCu["NaNoiRuaToi"].ToString()));
            dic.Add(GiaoDanConst.HoTenCha, Memory.ConvertVNI2UNI(rowHonPhoiCu["NaTenCha"].ToString()));
            dic.Add(GiaoDanConst.HoTenMe, Memory.ConvertVNI2UNI(rowHonPhoiCu["NaTenMe"].ToString()));

            DataRow rowGiaoDan1 = findMotNguoiHonPhoi(rowHonPhoiCu, dic, rowHonPhoiCu["NaTenThanh"].ToString(), rowHonPhoiCu["NaHoVaTen"].ToString(), rowHonPhoiCu["NaSinhNgay"].ToString());
            if (rowGiaoDan1 != null)
            {
                lst.Add(rowGiaoDan1);
            }
            dic = new Dictionary<string, string>();
            //tim nguoi chong
            dic.Add(GiaoDanConst.NgayRuaToi, Memory.FormatDateString(rowHonPhoiCu["NuNgayRuaToi"].ToString()));
            //dic.Add(GiaoDanConst.NoiRuaToi, Memory.ConvertVNI2UNI(rowHonPhoiCu["NuNoiRuaToi"].ToString()));
            dic.Add(GiaoDanConst.HoTenCha, Memory.ConvertVNI2UNI(rowHonPhoiCu["NuTenCha"].ToString()));
            dic.Add(GiaoDanConst.HoTenMe, Memory.ConvertVNI2UNI(rowHonPhoiCu["NuTenMe"].ToString()));

            DataRow rowGiaoDan2 = findMotNguoiHonPhoi(rowHonPhoiCu, dic, rowHonPhoiCu["NuTenThanh"].ToString(), rowHonPhoiCu["NuHoVaTen"].ToString(), rowHonPhoiCu["NuSinhNgay"].ToString());
            if (rowGiaoDan2 != null)
            {
                lst.Add(rowGiaoDan2);
            }
            return lst;
        }

        private DataRow findMotNguoiHonPhoi(DataRow rowHonPhoiCu, Dictionary<string, string> dic, string tenThanh, string hoTen, object ngaySinh)
        {
            string s = "";
            DataRow rowGiaoDan1 = null;
            if (ngaySinh != DBNull.Value)
            {
                s = string.Format("TenThanh = '{0}' AND HoTen='{1}' AND NgaySinh = '{2}' ",
                                             Memory.ConvertVNI2UNI(tenThanh.Replace("'", "''")),
                                             Memory.ConvertVNI2UNI(hoTen.Replace("'", "''")),
                                             ngaySinh);
                DataRow[] rows = tblGiaoDan.Select(s);
                if (rows.Length > 0)
                {
                    rowGiaoDan1 = rows[0];
                }
            }
            if (rowGiaoDan1 == null)
            {
                s = string.Format("TenThanh='{0}' AND HoTen='{1}' ",
                                                 Memory.ConvertVNI2UNI(tenThanh),
                                                 Memory.ConvertVNI2UNI(hoTen));
                bool hasValue = false;
                foreach (var item in dic)
                {
                    if (item.Value != "" && item.Value != "X")
                    {
                        s = string.Format("{0} AND ({1} IS NOT NULL AND {1}='{2}')", s, item.Key, Memory.ConvertVNI2UNI(item.Value).Replace("'", "''"));
                        hasValue = true;
                    }
                }
                if (hasValue)
                {
                    DataRow[] rows = tblGiaoDan.Select(s);
                    if (rows.Length > 0)
                    {
                        rowGiaoDan1 = rows[0];
                    }
                }

            }
            return rowGiaoDan1;
        }

        private DataRow findHonPhoi(DataRow rowHonPhoiCu)
        {
            DataRow rowHP = null;

            //Tim nguoi con lai theo ten vo / ten chong va ten nguoi chung, linh muc,...
            string tenVoChong = string.Concat(rowHonPhoiCu["TenThanh"].ToString().Replace("'", "''"), " ", rowHonPhoiCu["HoVaTen"]).ToString().Replace("'", "''");
            string s = "";
            if (rowHonPhoiCu["NgayHonPhoi"].ToString() != "")
            {
                s = string.Format("KetHonVoi='{0}' AND NgayHonPhoi='{1}'", tenVoChong, rowHonPhoiCu["NgayHonPhoi"]);
            }
            else
            {
                s = string.Format("KetHonVoi='{0}' AND (NgayHonPhoi IS NULL OR NgayHonPhoi='') AND (NguoiChung1='{1}' OR NguoiChung2='{2}' OR LinhMucHonPhoi='{3}')",
                    tenVoChong, rowHonPhoiCu["NguoiChung1"].ToString().Replace("'", "''"), rowHonPhoiCu["NguoiChung2"].ToString().Replace("'", "''"), rowHonPhoiCu["LinhMucHonPhoi"].ToString().Replace("'", "''"));
            }
            DataRow[] rows = tblHonPhoiCu.Select(s);
            if (rows.Length > 0)
            {
                rowHP = rows[0];
            }
            return rowHP;
        }

        private void chuyenMaTableBiTich(DataTable tbl) {
            foreach (DataRow row in tbl.Rows)
            {
                foreach (DataColumn col in tbl.Columns)
                {
                    if (col.DataType == typeof(int) || col.ColumnName.Contains("Ngay") || col.ColumnName.Contains("Ma") || col.ColumnName.Contains("So") || row[col].ToString() == "") continue;
                    row[col] = Memory.ConvertVNI2UNI(row[col].ToString());
                }
            }
        }

        private DataRow findSoBiTich(DataRow rowGiaoDan, DataTable tblBiTich)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("NgayRuaToi", rowGiaoDan[GiaoDanConst.NgayRuaToi].ToString());
            dic.Add("TenCha", rowGiaoDan[GiaoDanConst.HoTenCha].ToString());
            dic.Add("TenMe", rowGiaoDan[GiaoDanConst.HoTenMe].ToString());
            string tenThanh = rowGiaoDan[GiaoDanConst.TenThanh].ToString();
            string hoTen = rowGiaoDan[GiaoDanConst.HoTen].ToString();
            string ngaySinh = rowGiaoDan[GiaoDanConst.NgaySinh].ToString();
            string s = "";
            DataRow row = null;
            if (ngaySinh != "")
            {
                s = string.Format("TenThanh = '{0}' AND HoVaTen='{1}' AND SinhNgay = '{2}' ",
                                             Memory.ConvertVNI2UNI(tenThanh.Replace("'", "''")),
                                             Memory.ConvertVNI2UNI(hoTen.Replace("'", "''")),
                                             ngaySinh);
                DataRow[] rows = tblBiTich.Select(s);
                if (rows.Length > 0)
                {
                    row = rows[0];
                }
            }
            if (row == null)
            {
                s = string.Format("TenThanh='{0}' AND HoVaTen='{1}' ",
                                                 Memory.ConvertVNI2UNI(tenThanh.Replace("'", "''")),
                                                 Memory.ConvertVNI2UNI(hoTen.Replace("'", "''")));
                bool hasValue = false;
                foreach (var item in dic)
                {
                    if (item.Value != "" && item.Value != "X")
                    {
                        s = string.Format("{0} AND ({1} IS NOT NULL AND {1}='{2}')", s, item.Key, Memory.ConvertVNI2UNI(item.Value).Replace("'", "''"));
                        hasValue = true;
                    }
                }
                if (hasValue)
                {
                    DataRow[] rows = tblBiTich.Select(s);
                    if (rows.Length > 0)
                    {
                        row = rows[0];
                    }
                }

            }
            return row;
        }
        #endregion
    }
}
