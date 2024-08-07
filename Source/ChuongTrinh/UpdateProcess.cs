using System;
using System.Collections.Generic;
using System.Text;
using GxControl;
using GxGlobal;
using System.Data;
using System.Xml;
using System.ComponentModel;

namespace GiaoXu
{
    public class UpdateProcess: IGxProcess
    {
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;
        private ProcessOptions processOptions = ProcessOptions.UpgradeVersion;
        public ProcessOptions ProcessOptions
        {
            get { return processOptions; }
            set { processOptions = value; }
        }

        private object processData = null;
        public object ProcessData
        {
            get { return processData; }
            set { processData = value; }
        }

        #region public
        /// <summary>
        /// Execute action based on option and data
        /// </summary>
        public void Execute()
        {
            if (OnStart != null) OnStart("started", EventArgs.Empty);
            try
            {
                Memory.DbVersion = Memory.GetConfig(GxConstants.CF_CURRENT_DB_VERSION);
                switch (processOptions)
                {
                    case ProcessOptions.AutoUpperFirstCharGiaDinh:
                        AutoUpperCaseFirstCharGiaDinh();
                        break;
                    case ProcessOptions.AutoUpperFirstCharGiaoDan:
                        AutoUpperCaseFirstCharGiaoDan();
                        break;
                    case ProcessOptions.UpgradeVersion:
                        if (CompareVersion(Memory.DbVersion, "1.0.0.3") < 0)
                        {
                            updateTo1_0_0_3();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.0.0.0") < 0)
                        {
                            updateTo2_0_0_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.0.0.1") < 0)
                        {
                            updateTo2_0_0_1();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.0.0.2") < 0)
                        {
                            updateTo2_0_0_2();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.0.0.3") < 0)
                        {
                            updateTo2_0_0_3();
                        }
                        //reviewDateData_2_0_0_4();
                        if (CompareVersion(Memory.DbVersion, "2.0.0.4") < 0)
                        {
                            updateTo2_0_0_4();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.0.0.5") < 0)
                        {
                            updateTo2_0_0_5();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.0.0") < 0)
                        {
                            updateTo2_1_0_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.0.1") < 0)
                        {
                            updateTo2_1_0_1();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.0.4") < 0)
                        {
                            updateTo2_1_0_4();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.0.6") < 0)
                        {
                            updateTo2_1_0_6();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.0") < 0)
                        {
                            updateTo2_1_1_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.2") < 0)
                        {
                            updateTo2_1_1_2();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.3") < 0)
                        {
                            updateTo2_1_1_3();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.4") < 0)
                        {
                            updateTo2_1_1_4();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.6") < 0)
                        {
                            updateTo2_1_1_6();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.8") < 0)
                        {
                            updateTo2_1_1_8();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.1.9") < 0)
                        {
                            updateTo2_1_1_9();
                        }
                        if(CompareVersion(Memory.DbVersion,"2.1.2.0") < 0)
                        {
                            updateTo_2_1_2_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "2.1.3.0") < 0)
                        {
                            updateTo_2_1_3_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.2.0.1") < 0)
                        {
                            updateTo3_2_0_1();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.2.0.2") < 0)
                        {
                            updateTo3_2_0_2();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.2.0.3") < 0)
                        {
                            updateTo3_2_0_3();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.0.1") < 0)
                        {
                            updateTo3_3_0_1();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.0.2") < 0)
                        {
                            updateTo3_3_0_2();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.3.3") < 0)
                        {
                            updateTo3_3_3_3();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.3.4") < 0)
                        {
                            updateTo3_3_3_4();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.4.0") < 0)
                        {
                            updateTo3_3_4_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.5.0") < 0)
                        {
                            // the same update with previous version
                            updateTo3_3_4_0();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.5.1") < 0)
                        {
                            updateTo3_3_5_1();
                        }
                        if (CompareVersion(Memory.DbVersion, "3.3.5.2") < 0)
                        {
                            updateTo3_3_5_2();
                        }

                        if (CompareVersion(Memory.DbVersion, "3.3.6.0") < 0)
                        {
                            updateTo3_3_6_0();
                        }

                        #region send giao xu infor
                        sendGiaoXuInfo();
                        #endregion
                        //Update version info
                        Memory.ClearError();
                        Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, Memory.CurrentVersion, CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
                        
                        Memory.ClearError();
                        break;
                    case ProcessOptions.ChuyenHoGiaDinh:
                        if (processData is Dictionary<string, object>)
                        {
                            Dictionary<string, object> dicData = (Dictionary<string, object>)processData;
                            if (dicData.ContainsKey(GiaoHoConst.MaGiaoHo) && dicData.ContainsKey(GiaDinhConst.TableName))
                            {
                                chuyenHoGiaDinh((DataTable)dicData[GiaDinhConst.TableName], (int)dicData[GiaoHoConst.MaGiaoHo]);
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                if (OnError != null) OnError("Update: " + ex.Message, new CancelEventArgs());
            }
            if (OnFinished != null) OnFinished("finished", EventArgs.Empty);
        }
        #endregion

        #region chuyen ho gia dinh
        DataTable tblThanhVienSeChuyen = null;
        DataTable tblGiaDinhSeChuyen = null;
        private bool chuyenHoGiaDinh(DataTable tbl, int maGiaoHo)
        {
            try
            {
                if (tbl != null)
                {
                    tblGiaDinhSeChuyen = tbl.Clone();
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {
                        DataRow row = tbl.Rows[i];
                        if ((bool)row[frmChuyenHoGiaDinh.SELECT_COL] == true)
                        {
                            string tenGiaDinh = row[GiaDinhConst.TenGiaDinh].ToString();
                            if (OnExecuting != null) OnExecuting(string.Format("Đang chuyển họ cho gia đình [{0}]...", tenGiaDinh), EventArgs.Empty);
                            row[GiaDinhConst.MaGiaoHo] = maGiaoHo;
                            if (!chuyenHoThanhVienGiaDinh((int)row[GiaDinhConst.MaGiaDinh], maGiaoHo))
                            {
                                return false;
                            }
                            tblGiaDinhSeChuyen.ImportRow(row);
                        }
                    }
                    //update to database
                    if (OnExecuting != null) OnExecuting("Đang cập nhật thông tin gia đình và các thành viên...", EventArgs.Empty);

                    DataSet ds = new DataSet();

                    if (tblThanhVienSeChuyen != null)
                    {
                        tblThanhVienSeChuyen.TableName = GiaoDanConst.TableName;
                        ds.Tables.Add(tblThanhVienSeChuyen);
                    }

                    tblGiaDinhSeChuyen.TableName = GiaDinhConst.TableName;
                    ds.Tables.Add(tblGiaDinhSeChuyen);
                    Memory.UpdateDataSet(ds);
                    if (Memory.ShowError())
                    {
                        raiseError("Cập nhật dữ liệu bị lỗi. Chuyển họ thất bại." + Environment.NewLine + Memory.Instance.Error.Message);
                        return false;
                    }
                    tblThanhVienSeChuyen.Dispose();
                    tblGiaDinhSeChuyen.Dispose();
                }
            }
            catch (Exception ex)
            {
                raiseError("Chuyển họ thất bại" + Environment.NewLine + ex.Message);
                return false;
            }
            return true;
        }
        private bool chuyenHoThanhVienGiaDinh(int maGiaDinh, int maGiaoHo)
        {
            string sql = string.Format(SqlConstants.SELECT_THANHVIEN_GIADINH, SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO);
            DataTable tblThanhVien = Memory.GetData(sql + " AND ThanhVienGiaDinh.MaGiaDinh = ? ", new object[] { maGiaDinh });
            if (Memory.HasError())
            {
                raiseError("Chuyển họ cho các thành viên trong gia đình bị lỗi" + Environment.NewLine + Memory.Instance.Error);
                return false;
            }
            tblThanhVien.TableName = GiaoDanConst.TableName;
            if (tblThanhVienSeChuyen == null)
            {
                tblThanhVienSeChuyen = tblThanhVien.Clone();
            }

            foreach (DataRow row in tblThanhVien.Rows)
            {
                row[GiaoDanConst.MaGiaoHo] = maGiaoHo;
                DataRow[] rows = tblThanhVienSeChuyen.Select(string.Format("MaGiaoDan={0}", row[GiaoDanConst.MaGiaoDan]));
                if (rows != null && rows.Length > 0)
                {
                    //if existed, no add again
                    continue;
                }
                tblThanhVienSeChuyen.ImportRow(row);
            }
            return true;
        }

        //private bool chuyenHoGiaoDan(DataTable tbl)
        //{

        //}

        #endregion

        /// <summary>
        /// Compare between two version.
        /// Return:
        /// 0 if equal .
        /// -1 if ver1 greater than ver2. 
        /// 1 if ver2 less than ver1
        /// </summary>
        private int CompareVersion(string ver1, string ver2)
        {
            return string.Compare(ver1, ver2);
        }

        private void raiseError(string message)
        {
            if (OnError != null) OnError(message, new CancelEventArgs());
        }

        #region Tools
        public void AutoUpperCaseFirstCharGiaoDan()
        {
            try
            {
                if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu giáo dân...", EventArgs.Empty);
                DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, "");
                if (!Memory.ShowError() && tblGiaoDan != null && tblGiaoDan.Rows.Count > 0)
                {
                    if (OnExecuting != null) OnExecuting("Đang chuẩn hóa dữ liệu giáo dân...", EventArgs.Empty);
                    tblGiaoDan.TableName = GiaoDanConst.TableName;
                    Memory.AutoUpperCaseFirstCharGiaoDan(tblGiaoDan);
					Memory.FormatDate(tblGiaoDan);
                    if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu giáo dân...", EventArgs.Empty);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tblGiaoDan);
                    Memory.UpdateDataSet(ds);
                    if (Memory.HasError())
                    {
                        if (OnError != null) OnError(this, new CancelEventArgs());
                        Memory.ClearError();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Exception AutoUpperCaseFirstCharGiaoDan", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                if (OnError != null) OnError(ex.Message, new CancelEventArgs());
            }
        }

        public void AutoUpperCaseFirstCharGiaDinh()
        {
            try
            {
                if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu gia đình...", EventArgs.Empty);
                DataTable tblHonPhoi = Memory.GetTable(HonPhoiConst.TableName, "");
                DataTable tblGiaDinh = Memory.GetTable(GiaDinhConst.TableName, "");                
                if (!Memory.ShowError() && tblGiaDinh != null && tblGiaDinh.Rows.Count > 0 && tblHonPhoi != null)
                {
                    if (OnExecuting != null) OnExecuting("Đang chuẩn hóa dữ liệu gia đình...", EventArgs.Empty);
                    //Gia Dinh
                    tblGiaDinh.TableName = GiaDinhConst.TableName;
                    Memory.AutoUpperCaseFirstCharGiaDinh(tblGiaDinh);
                    //Hon phoi
                    tblHonPhoi.TableName = HonPhoiConst.TableName;
                    Memory.AutoUpperCaseFirstCharGiaDinh(tblHonPhoi);
                    if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu gia đình...", EventArgs.Empty);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tblGiaDinh);
                    Memory.UpdateDataSet(ds);
                    if (Memory.HasError())
                    {
                        if (OnError != null) OnError(this, new CancelEventArgs());
                        Memory.ClearError();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Exception AutoUpperCaseFirstCharGiaDinh", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                if (OnError != null) OnError(ex.Message, new CancelEventArgs());
            }
        }
        #endregion

        #region For version 3.3.6.0
        //2018-07-17 Gia add start
        private void updateTo3_3_6_0()
        {
            string alterSQL = @"ALTER TABLE GiaoDan ADD NgayXucDau Text(10), NguoiXucDau Text(100), TinhTrangXucDau Text(100), GhiChuXucDau Text(255),
NgayBD1 Text(10), NoiBD1 Text(100), NgayBD2 Text(10), NoiBD2 Text(100), 
NgayTHVaoDoi Text(10), NoiTHVaoDoi Text(100), 
NgayGLHN1 Text(10), NgayGLHN2 Text(10), NoiGLHN Text(100), NguoiChungNhanGLHN Text(100), XepLoaiGLHN Text(100);";
            Memory.ExecuteSqlCommand(alterSQL);
            Memory.ClearError();

        }
        ///2018-07-17 Gia add end
        #endregion

        #region For version 3.3.5.2
        private void updateTo3_3_5_2()
        {
            //Update  SELECT_HONPHOI_LIST
            string dropProcedureListHonPhoi = @"DROP PROCEDURE SELECT_HONPHOI_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListHonPhoi);
            Memory.ClearError();

            string createProcudureListHonPhoi = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS TRANSFORM First(A.MaGiaoDan) AS MaGiaoDan
SELECT IIF( HP.NgayHonPhoi = '','',(Year(Now())-Right(HP.NgayHonPhoi,4))) as SoNamHonPhoi , HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate, First(HP.GhiChu) AS GhiChu
FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate
PIVOT GDHP.Phai;
 ";
            if (Memory.HasError())
                Memory.ShowError();
            Memory.ExecuteSqlCommand(createProcudureListHonPhoi);
            Memory.ClearError();
        }
        #endregion

        #region For version 3.3.5.1
        private void updateTo3_3_5_1()
        {
            //Update  SELECT_GIADINH_LIST
            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT (IIF(
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true and
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,2,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,1,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true
,0,-1)))) as GACH,
 GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, 
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong,
                                                (SELECT TOP 1 GiaoDan.DienThoai FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS DTChong,
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, 
                                                (SELECT TOP 1 GiaoDan.DienThoai FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS DTVo, 
                                                (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD 
                                                    WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND GiaoDan.DaXoa = false AND GiaoDan.MaGiaoHo <> 0 and GiaoDan.GiaoDanAo=false  
                                                    AND(VaiTro =0 OR VaiTro=1 Or TVGD.MaGiaoDan not in 
                                                    (select ThanhVienGiaDinh.MaGiaoDan from ThanhVienGiaDinh 
                                                    where ThanhVienGiaDinh.MaGiaoDan=TVGD.MaGiaoDan and ThanhVienGiaDinh.MaGiaDinh<>TVGD.MaGiaDinh ) )  
                                                    and TVGD.MaGiaoDan Not in (select CX.MaGiaoDan from ChuyenXu CX where CX.MaGiaoDan=TVGD.MaGiaoDan and  CX.LoaiChuyen<> 1 ) ) AS SoLuong
                                                FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();
        }
        #endregion
        #region For version 3.3.4.0
        private void updateTo3_3_4_0()
        {
            string dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST_1";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);

            string createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST_1 AS TRANSFORM First(A.MaGiaoDan) AS MaGiaoDan
SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate, First(HP.GhiChu) AS GhiChu
FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate
PIVOT GDHP.Phai;
";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if ((Memory.Instance.Error != null))
            {
                return;
            }

            dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);
            Memory.ShowError();
            //Memory.ClearError();

            createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS TRANSFORM First([A.TenThanh]+' '+[A.HoTen]) AS TenDayDu 
                SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate 
                FROM((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan = TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi 
                GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate 
                PIVOT GDHP.Phai;";

            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            Memory.ShowError();
            //Memory.ClearError();
        }
        #endregion
        #region For version 3.3.3.4
        private void updateTo3_3_3_4()
        {
            //Update  SELECT_GIADINH_LIST
            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT (IIF(
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true and
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,2,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,1,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true
,0,-1)))) as GACH,
 GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, 
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong,
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, 
                                                (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD 
                                                    WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND GiaoDan.DaXoa = false AND GiaoDan.MaGiaoHo <> 0 and GiaoDan.GiaoDanAo=false  
                                                    AND(VaiTro =0 OR VaiTro=1 Or TVGD.MaGiaoDan not in 
                                                    (select ThanhVienGiaDinh.MaGiaoDan from ThanhVienGiaDinh 
                                                    where ThanhVienGiaDinh.MaGiaoDan=TVGD.MaGiaoDan and ThanhVienGiaDinh.MaGiaDinh<>TVGD.MaGiaDinh ) )  
                                                    and TVGD.MaGiaoDan Not in (select CX.MaGiaoDan from ChuyenXu CX where CX.MaGiaoDan=TVGD.MaGiaoDan and  CX.LoaiChuyen<> 1 ) ) AS SoLuong
                                                FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();

            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_DACOGIADINH_THEOHONPHOI);
            Memory.ClearError();

            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_CHUACOGIADINH_THEOHONPHOI);
            Memory.ClearError();
        }
        #endregion

        #region For version 3.3.3.3
        private void updateTo3_3_3_3()
        {
            string dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST_1";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);

            string createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST_1 AS TRANSFORM First(A.MaGiaoDan) AS MaGiaoDan
SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, First(HP.GhiChu) AS GhiChu, HP.MaNhanDang, HP.UpdateDate
FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate
PIVOT GDHP.Phai
 ";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if ((Memory.Instance.Error != null))
            {
                return;
            }

            dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);
            Memory.ClearError();

            createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS SELECT SELECT_HONPHOI_LIST_1.*, GiaoDan.TenThanh + ' ' + GiaoDan.HoTen AS NguoiNam,  GiaoDan_1.TenThanh + ' ' + GiaoDan_1.HoTen AS NguoiNu, GiaoDan.MaGiaoHo, GiaoHo.MaGiaoHoCha, GiaoHo.TenGiaoHo
            FROM ((SELECT_HONPHOI_LIST_1 INNER JOIN GiaoDan ON SELECT_HONPHOI_LIST_1.Nam = GiaoDan.MaGiaoDan) INNER JOIN GiaoDan AS GiaoDan_1 ON SELECT_HONPHOI_LIST_1.Nữ = GiaoDan_1.MaGiaoDan) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo;
            ";

            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            Memory.ClearError();
        }
        #endregion

        //hiepdv update
        #region For version 3.3.0.2
        private void updateTo3_3_0_2()
        {
            Memory.SetConfig(GxConstants.CF_SOGIADINH_IN_DAXOA, 0);
            Memory.SetConfig(GxConstants.CF_SOGIADINH_IN_DACHUYENXU, 0);
        }
        #endregion
        #region For version 3.3.0.1
        private void updateTo3_3_0_1()
        {

            //Update  SELECT_GIADINH_LIST
            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT (IIF(
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true and
(SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,2,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,1,
IIF((SELECT TOP 1 GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true
,0,-1)))) as GACH,
 GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, 
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong,
                                                (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, 
                                                (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD 
                                                    WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND GiaoDan.DaXoa = false  
                                                    AND(VaiTro =0 OR VaiTro=1 Or TVGD.MaGiaoDan not in 
                                                    (select ThanhVienGiaDinh.MaGiaoDan from ThanhVienGiaDinh 
                                                    where ThanhVienGiaDinh.MaGiaoDan=TVGD.MaGiaoDan and ThanhVienGiaDinh.MaGiaDinh<>TVGD.MaGiaDinh ) )  
                                                    and TVGD.MaGiaoDan Not in (select CX.MaGiaoDan from ChuyenXu CX where CX.MaGiaoDan=TVGD.MaGiaoDan and  CX.LoaiChuyen<> 1 ) ) AS SoLuong
                                                FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();
        }
        #endregion

        #region For version 3.2.0.3
        private void updateTo3_2_0_3()
        {

            //Update  SELECT_GIADINH_LIST
            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT (IIF(
(SELECT GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true and
(SELECT GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,2,
IIF((SELECT GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1)=true
,1,
IIF((SELECT GiaoDan.QuaDoi FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro =0)=true
,0,-1)))) as GACH,
 GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, 
                                                    (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD 
                                                    WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND GiaoDan.DaXoa = false  
                                                    AND(VaiTro =0 OR VaiTro=1 Or TVGD.MaGiaoDan not in 
                                                    (select ThanhVienGiaDinh.MaGiaoDan from ThanhVienGiaDinh 
                                                    where ThanhVienGiaDinh.MaGiaoDan=TVGD.MaGiaoDan and ThanhVienGiaDinh.MaGiaDinh<>TVGD.MaGiaDinh ) )  
                                                    and TVGD.MaGiaoDan Not in (select CX.MaGiaoDan from ChuyenXu CX where CX.MaGiaoDan=TVGD.MaGiaoDan and  CX.LoaiChuyen<> 1 ) ) AS SoLuong
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();
        }
        #endregion

        #region For version 3.2.0.2
        private void updateTo3_2_0_2()
        {
            //Create Table HoiDoan
            string createTableHoiDoan = @"
                                            Create Table HoiDoan(
                                            MaHoiDoan integer primary key,
                                            TenHoiDoan text(255),
                                            ThanhBonMang text(255),
                                            NgayBonMang text(255),
                                            NgayThanhLap text(255),
                                            GhiChu text(255)
                                            )";
            Memory.ExecuteSqlCommand(createTableHoiDoan);
            Memory.ClearError();

            //Create Table ChiTietHoiDoan
            string createTableChiTietHoiDoan = @"
                                            Create Table ChiTietHoiDoan(
                                            ID autoincrement primary key,
                                            MaHoiDoan integer,
                                            MaGiaoDan integer,
                                            NgayVaoHoiDoan text(255),
                                            NgayRaHoiDoan text(255),
                                            VaiTro text(255)
                                            )";

            Memory.ExecuteSqlCommand(createTableChiTietHoiDoan);
            Memory.ClearError();

            //Update  SELECT_GIADINH_LIST
            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan     
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, 
                                                    (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD 
                                                    WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND GiaoDan.DaXoa = false  
                                                    AND(VaiTro =0 OR VaiTro=1 Or TVGD.MaGiaoDan not in 
                                                    (select ThanhVienGiaDinh.MaGiaoDan from ThanhVienGiaDinh 
                                                    where ThanhVienGiaDinh.MaGiaoDan=TVGD.MaGiaoDan and ThanhVienGiaDinh.MaGiaDinh<>TVGD.MaGiaDinh ) )  
                                                    and TVGD.MaGiaoDan Not in (select CX.MaGiaoDan from ChuyenXu CX where CX.MaGiaoDan=TVGD.MaGiaoDan and  CX.LoaiChuyen<> 1 ) ) AS SoLuong
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();
            //update create index in table ChuyenXu
            string createIndexChuyenXu = @"CREATE INDEX IndexMaGiaoDan ON ChuyenXu (MaGiaoDan)";
            Memory.ExecuteSqlCommand(createIndexChuyenXu);
            Memory.ClearError();

        }
       //hiepdv end update
        #endregion

        #region For version 3.2.0.1
        private void updateTo3_2_0_1()
        {
            DataTable tbl = Memory.GetData(string.Format("SELECT * FROM CauHinh WHERE {0}='{1}'", CauHinhConst.MaCauHinh, GxConstants.CF_MAU_SOGIADINH));
            if(tbl != null && tbl.Rows.Count > 0)
            {
                //nothing
            }
            else
            {
                Memory.ExecuteSqlCommand(string.Format("INSERT INTO CauHinh VALUES('{0}', '{1}', '{2}', '{3}')", GxConstants.CF_MAU_SOGIADINH, GxConstants.CF_MAU_SOGIADINH_EXCEL, "Mẫu in phiếu gia đình", Memory.Instance.GetServerDateTime()));
            }
            
            Memory.ClearError();
        }
        #endregion
        #region For version 2.1.3.0
        //2018-07-17 Gia add start
        private void updateTo_2_1_3_0()
        {
            string alterGiaDinhTable = @"ALTER TABLE GiaDinh ADD DienGiaDinh Text(255)";
            Memory.ExecuteSqlCommand(alterGiaDinhTable);
            Memory.ClearError();

            string alterGiaoDanTable = @"ALTER TABLE GiaoDan ADD TrinhDoChuyenMon Text(255),BietNgoaiNgu Text(100)";
            Memory.ExecuteSqlCommand(alterGiaoDanTable);
            Memory.ClearError();

            //2018-08-08 Gia add start
            string alterGiaoXuTable = @"ALTER TABLE GiaoXu ADD LastUpload DATETIME";
            Memory.ExecuteSqlCommand(alterGiaoXuTable);
            Memory.ClearError();
            //2018-08-08 Gia add end
        }
        ///2018-07-17 Gia add end
        #endregion


        #region For version 2.1.2.0
        private void updateTo_2_1_2_0()
        {
            string createTaiKhoanTable =
                @"
CREATE TABLE TaiKhoan(
HoTenNguoiDung Text(255),
TenTaiKhoan Text(255) PRIMARY KEY,
MatKhau Text(255),
Email Text(255),
SoDienThoai Text(50),
LoaiTaiKhoan INTEGER,
CauHoiGoiY Text(255),
DaXoa BIT,
CauTraLoiGoiY Text(255))
";
            Memory.ExecuteSqlCommand(createTaiKhoanTable);
            Memory.ClearError();
            string createLoaiTaiKhoanTable =
                @"
CREATE TABLE TenLoaiTaiKhoan(
ID INTEGER PRIMARY KEY,
TenLoai Text(255)
)
";
            Memory.ExecuteSqlCommand(createLoaiTaiKhoanTable);
            Memory.ClearError();
            string insertLoaiTaiKhoan1 = @"INSERT INTO TenLoaiTaiKhoan (ID,TenLoai) 
VALUES (0,'Quản trị viên')";
            Memory.ExecuteSqlCommand(insertLoaiTaiKhoan1);
            Memory.ClearError();

            string insertLoaiTaiKhoan2 = @"INSERT INTO TenLoaiTaiKhoan (ID,TenLoai) 
VALUES (1,'Người nhập 1')";
            Memory.ExecuteSqlCommand(insertLoaiTaiKhoan2);
            Memory.ClearError();

            string insertLoaiTaiKhoan3 = @"INSERT INTO TenLoaiTaiKhoan (ID,TenLoai) 
VALUES (2,'Người nhập 2')";
            Memory.ExecuteSqlCommand(insertLoaiTaiKhoan3);
            Memory.ClearError();

            string alterGiaoDanTable = @"ALTER TABLE GiaoDan ADD AnhDaiDien Text(255), CMND TEXT(50)";
            Memory.ExecuteSqlCommand(alterGiaoDanTable);
            Memory.ClearError();

            string alterGiaDinhTable = @"ALTER TABLE GiaDinh ADD AnhDaiDien Text(255), SoHoKhau TEXT(50)";
            Memory.ExecuteSqlCommand(alterGiaDinhTable);
            Memory.ClearError();

            string dropProcedureListGiaDinh = @"DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropProcedureListGiaDinh);
            Memory.ClearError();

            string createProcudureListGiaDinh = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS

SELECT GD.*, GiaoHo.MaGiaoHoCha, IIf(GD.MaGiaoHo=0,'Ngoài xứ',GiaoHo.TenGiaoHo) AS TenGiaoHo, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 0) AS TenChong, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD, GiaoDan
      
                                                          WHERE GiaoDan.MaGiaoDan = TVGD.MaGiaoDan AND TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.VaiTro = 1) AS TenVo, (SELECT COUNT(TVGD.MaGiaoDan) FROM GiaoDan, ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh = GD.MaGiaDinh AND TVGD.MaGiaoDan = GiaoDan.MaGiaoDan  AND GiaoDan.QuaDoi = 0 AND((GiaoDan.DaCoGiaDinh = 0 AND VaiTro = 2) OR(VaiTro = 0 OR VaiTro = 1)) ) AS SoLuong
FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo = GiaoHo.MaGiaoHo;
            ";
            Memory.ExecuteSqlCommand(createProcudureListGiaDinh);
            Memory.ClearError();

        }
        #endregion
        #region For version 2.1.1.9
        private void updateTo2_1_1_9()
        {
            string sqlCreateTable = @"CREATE TABLE DuLieuChung (
                                        ID INTEGER PRIMARY KEY,
                                        LoaiDuLieu INTEGER NOT NULL,
                                        MaDuLieu TEXT(50) NOT NULL,
                                        DuLieu1 TEXT(255),
                                        DuLieu2 TEXT(255)
                                        )                   
                                     ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            Memory.ClearError();

            string sqlAlterTable = @"ALTER TABLE GiaoXu 
                                    ADD COLUMN GhiChu MEMO,
                                    MaGiaoXuRieng INTEGER
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            sqlAlterTable = @"ALTER TABLE GiaoPhan 
                                    ADD COLUMN MaGiaoPhanRieng INTEGER
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            sqlAlterTable = @"ALTER TABLE GiaoHat
                                    ADD COLUMN MaGiaoHatRieng INTEGER
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();


          
        }
        #endregion
        #region For version 2.1.1.8
        private void updateTo2_1_1_8()
        {
            string sqlCreateTable = @"CREATE TABLE DuLieuChung (
                                        ID INTEGER PRIMARY KEY,
                                        LoaiDuLieu INTEGER NOT NULL,
                                        MaDuLieu TEXT(50) NOT NULL,
                                        DuLieu1 TEXT(255),
                                        DuLieu2 TEXT(255)
                                        )                   
                                     ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            Memory.ClearError();

            string sqlAlterTable = @"ALTER TABLE GiaoDan 
                                    ADD COLUMN DanToc TEXT(50),
                                    NoiQuaDoi TEXT(255),
                                    SoAnTang TEXT(50),
                                    NoiAnTang TEXT(255)
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            sqlAlterTable = @"ALTER TABLE RaoHonPhoi 
                                    ADD COLUMN GiaoXuNQ1 TEXT(255),
                                    GiaoPhanNQ1 TEXT(255),
                                    GiaoXuNQ2 TEXT(255),
                                    GiaoPhanNQ2 TEXT(255)
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            #region DROP VIEW SELECT_HONPHOI_LIST_1
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST_1";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_HONPHOI_LIST_1
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST_1 AS TRANSFORM First(A.MaGiaoDan) AS MaGiaoDan
SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, First(HP.GhiChu) AS GhiChu, HP.MaNhanDang, HP.UpdateDate
FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.MaNhanDang, HP.UpdateDate
PIVOT GDHP.Phai ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            Memory.ClearError();
            #endregion

            #region Import ten thanh
            DataTable tblTenThanh = Memory.GetData("SELECT * FROM DuLieuChung WHERE LoaiDuLieu=1 ");
            if (System.IO.File.Exists(Memory.AppPath + "Resources\\TenThanh.txt") && tblTenThanh != null && tblTenThanh.Rows.Count == 0)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(Memory.AppPath + "Resources\\TenThanh.txt");
                while(!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim();
                    if (s == string.Empty) continue;
                    Memory.InsertRow(tblTenThanh, new string[] { DuLieuChungConst.ID, DuLieuChungConst.LoaiDuLieu, DuLieuChungConst.MaDuLieu, DuLieuChungConst.DuLieu1 }, 
                        new object[] { Memory.Instance.GetNextId(DuLieuChungConst.TableName, DuLieuChungConst.ID, false), LoaiDuLieuChung.TenThanh, s, s });
                }
                DataSet ds = new DataSet();
                tblTenThanh.TableName = DuLieuChungConst.TableName;
                ds.Tables.Add(tblTenThanh);
                Memory.UpdateDataSet(ds);
                sr.Close();
            }
            Memory.ClearError();
            #endregion
        }
        #endregion

        #region For version 2.1.1.6
        private void updateTo2_1_1_6()
        {
            string sqlAlterTable = @"ALTER TABLE GiaoDan 
                                    ADD COLUMN ThuocGiaoPhan TEXT(255)
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "Chung", CauHinhConst.MaCauHinh, GxConstants.CF_TEMPLATE_FOLDER));
            Memory.ExecuteSqlCommand(string.Format("INSERT INTO CauHinh VALUES('{0}', '{1}', '{2}', '{3}')", GxConstants.CF_LANGUAGE, GxConstants.LANG_VN, "Ngôn ngữ báo cáo", Memory.Instance.GetServerDateTime()));
            Memory.ClearError();
        }
        #endregion
        #region For version 2.1.1.4
        private void updateTo2_1_1_4()
        {
            string sqlAlterTable = @"ALTER TABLE GiaoDan 
                                    ALTER COLUMN DaCoGiaDinh BIT DEFAULT 0
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();
            /*
            string dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);
            Memory.ClearError();

            string createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS SELECT SELECT_HONPHOI_LIST_1.*, GiaoDan.TenThanh + ' ' + GiaoDan.HoTen AS NguoiNam,  GiaoDan_1.TenThanh + ' ' + GiaoDan_1.HoTen AS NguoiNu, GiaoDan.MaGiaoHo, GiaoHo.MaGiaoHoCha, GiaoHo.TenGiaoHo
FROM ((SELECT_HONPHOI_LIST_1 INNER JOIN GiaoDan ON SELECT_HONPHOI_LIST_1.Nam = GiaoDan.MaGiaoDan) INNER JOIN GiaoDan AS GiaoDan_1 ON SELECT_HONPHOI_LIST_1.Nữ = GiaoDan_1.MaGiaoDan) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo;
";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
             */
            return;
        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.1.1.3
        private void updateTo2_1_1_3()
        {
            /*
            string dropViewSELECT_HONPHOI_LIST = "DROP PROCEDURE SELECT_HONPHOI_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_HONPHOI_LIST);
            Memory.ClearError();

            string createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST_1 AS TRANSFORM First(A.MaGiaoDan) AS MaGiaoDan
SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate
FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate
PIVOT GDHP.Phai;
 ";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS SELECT SELECT_HONPHOI_LIST_1.*, GiaoDan.TenThanh + ' ' + GiaoDan.HoTen AS NguoiNam,  GiaoDan_1.TenThanh + ' ' + GiaoDan_1.HoTen AS NguoiNu, GiaoDan.MaGiaoHo, GiaoHo.TenGiaoHo
FROM ((SELECT_HONPHOI_LIST_1 INNER JOIN GiaoDan ON SELECT_HONPHOI_LIST_1.Nam = GiaoDan.MaGiaoDan) INNER JOIN GiaoDan AS GiaoDan_1 ON SELECT_HONPHOI_LIST_1.Nữ = GiaoDan_1.MaGiaoDan) LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo;
";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            return;
        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
             * */
        }
        #endregion

        #region For version 2.1.1.2
        private void updateTo2_1_1_2()
        {
            string sqlAlterTable = @"ALTER TABLE GiaoHo ADD COLUMN MaGiaoHoCha INTEGER";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            Memory.ClearError();

            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, GiaoHo.MaGiaoHoCha, IIF(GD.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) AS TenChong, (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS TenVo, (SELECT COUNT(MaGiaoDan) FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh = GD.MaGiaDinh) AS SoLuong
FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo=GiaoHo.MaGiaoHo;
 ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            return;
        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion
        #region For version 2.1.1.0
        private void updateTo2_1_1_0()
        {
            //fix bug of vesion 2.1.0.8
            bool isLatestDB = true;
            DataTable tbl = Memory.GetData("SELECT DiaChi FROM GiaoDan WHERE 1=2");
            if (Memory.Instance.Error != null)
            {
                Memory.ClearError();
                isLatestDB = false;
            }
            if (isLatestDB)
            {
                tbl = Memory.GetData("SELECT ChuHo FROM ThanhVienGiaDinh WHERE 1=2");
                if (Memory.Instance.Error != null)
                {
                    Memory.ClearError();
                    isLatestDB = false;
                }
            }
            if (!isLatestDB)
            {
                //*************************************************
                //update db in v2.1.0.4
                //*************************************************
                //create HonPhoi table
                string sqlCreateTable = @"ALTER TABLE GiaoDan 
                                    ADD COLUMN DiaChi TEXT(255)
                                     ";
                Memory.ExecuteSqlCommand(sqlCreateTable);

                if (Memory.ShowError())
                {
                    goto errorHandler;
                }
                string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
                Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);

                string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, IIF(GD.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo,
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) AS TenChong, 
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS TenVo,
                                                    (SELECT COUNT(MaGiaoDan) FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh = GD.MaGiaDinh) as SoLuong
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo=GiaoHo.MaGiaoHo ";
                Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
                if (Memory.ShowError())
                {
                    goto errorHandler;
                }

                //*************************************************
                // update db v2.1.0.6
                //*************************************************

                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_CHUANHOA_TUCHUANHOA, GxConstants.CF_TRUE, "Cho phép chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_CHUANHOA_TUDOIDAU, GxConstants.CF_TRUE, "Cho phép tự đổi cách bỏ dấu sang kiểu cũ khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_SOBITICH_HIENTATCAGIAODAN, GxConstants.CF_FALSE, "Cho phép hiện tất cả giáo dân tìm được khi chọn vào sổ bí tích", Memory.Instance.GetServerDateTime() });
                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_CHUANHOA_TUCHUYENMA, GxConstants.CF_TRUE, "Cho phép tự chuyển sang mã unicode dựng sẵn khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_MAX_BACKUP, "40", "Số lượng tối đa tập tin backup được giữ lại", DateTime.Now });
                Memory.ClearError();
                Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                        new object[] { GxConstants.CF_CHUANHOA_TRONGNGOAC, "3", "Cách để từ trong ngoại khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
                Memory.ClearError();
                //fix bug khi update rao hon phoi bi loi
                string sqlAlterTable = @"ALTER TABLE RaoHonPhoi 
                                    ALTER COLUMN GiaoXu1 TEXT(255)
                                     ";
                Memory.ExecuteSqlCommand(sqlAlterTable);
                Memory.ClearError();

                sqlAlterTable = @"ALTER TABLE ThanhVienGiaDinh 
                                    ADD COLUMN ChuHo BIT
                                     ";
                Memory.ExecuteSqlCommand(sqlAlterTable);
                Memory.ClearError();
            }
            return;
        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.1.0.6
        private void updateTo2_1_0_6()
        {
            #region Update version info
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_CHUANHOA_TUCHUANHOA, GxConstants.CF_TRUE, "Cho phép chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_CHUANHOA_TUDOIDAU, GxConstants.CF_TRUE, "Cho phép tự đổi cách bỏ dấu sang kiểu cũ khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_SOBITICH_HIENTATCAGIAODAN, GxConstants.CF_FALSE, "Cho phép hiện tất cả giáo dân tìm được khi chọn vào sổ bí tích", Memory.Instance.GetServerDateTime() });
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_CHUANHOA_TUCHUYENMA, GxConstants.CF_TRUE, "Cho phép tự chuyển sang mã unicode dựng sẵn khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_MAX_BACKUP, "40", "Số lượng tối đa tập tin backup được giữ lại", DateTime.Now });
            Memory.ClearError();
            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)",
                                    new object[] { GxConstants.CF_CHUANHOA_TRONGNGOAC, "3", "Cách để từ trong ngoại khi chuẩn hóa dữ liệu nhập một cách tự động", Memory.Instance.GetServerDateTime() });
            Memory.ClearError();
            //fix bug khi update rao hon phoi bi loi
            string sqlAlterTable = @"ALTER TABLE RaoHonPhoi 
                                    ALTER COLUMN GiaoXu1 TEXT(255)
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);
            Memory.ClearError();

            sqlAlterTable = @"ALTER TABLE ThanhVienGiaDinh 
                                    ADD COLUMN ChuHo BIT
                                     ";
            Memory.ExecuteSqlCommand(sqlAlterTable);

            //Chinh sua data ThanhVienGiaDinh
            DataTable tblThanhVienGiaDinh = Memory.GetData("SELECT TVGD.*, GD.QuaDoi, GD.Phai FROM ThanhVienGiaDinh TVGD INNER JOIN GiaoDan GD ON TVGD.MaGiaoDan=GD.MaGiaoDan WHERE TVGD.VaiTro=0 OR TVGD.VaiTro=1");
            tblThanhVienGiaDinh.TableName = ThanhVienGiaDinhConst.TableName;
            foreach (DataRow row in tblThanhVienGiaDinh.Rows)
            {
                DataRow[] tvgd = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND MaGiaoDan <> {1}", row[ThanhVienGiaDinhConst.MaGiaDinh], row[ThanhVienGiaDinhConst.MaGiaoDan]));
                if (tvgd.Length == 0)
                {
                    row[ThanhVienGiaDinhConst.ChuHo] = true;
                }
                else
                {
                    DataRow rowNam = null;
                    DataRow rowNu = null;
                    if (row[GiaoDanConst.Phai].ToString().ToLower() == GxConstants.NAM.ToLower())
                    {
                        rowNam = row;
                        rowNu = tvgd[0];
                    }
                    else
                    {
                        rowNu = row;
                        rowNam = tvgd[0];
                    }
                    if ((bool)rowNam[GiaoDanConst.QuaDoi] == true
                        && (bool)rowNu[GiaoDanConst.QuaDoi] == false) //Neu nguoi nu con song ma nguoi nam qua doi thi nguoi nu la chu ho
                    {
                        rowNu[ThanhVienGiaDinhConst.ChuHo] = true;
                        rowNam[ThanhVienGiaDinhConst.ChuHo] = false;
                    }
                    else //Neu nguoi nam con song hoac ca 2 cung qua doi thi nguoi nam la chu ho
                    {
                        rowNam[ThanhVienGiaDinhConst.ChuHo] = true;
                        rowNu[ThanhVienGiaDinhConst.ChuHo] = false;
                    }
                }
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(tblThanhVienGiaDinh);
            Memory.UpdateDataSet(ds);

            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            #endregion
            Memory.ClearError();
            return;

        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.1.0.4
        private void updateTo2_1_0_4()
        {
            if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu...", EventArgs.Empty);
            #region UPDATE Database struct
            //create HonPhoi table
            string sqlCreateTable = @"ALTER TABLE GiaoDan 
                                    ADD COLUMN DiaChi TEXT(255)
                                     ";
            Memory.ExecuteSqlCommand(sqlCreateTable);

            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, "");
            tblGiaoDan.PrimaryKey = new DataColumn[] { tblGiaoDan.Columns[GiaoDanConst.MaGiaoDan] };

            DataTable tblGiaDinh = Memory.GetTable(GiaDinhConst.TableName, "");
            tblGiaDinh.PrimaryKey = new DataColumn[] { tblGiaDinh.Columns[GiaDinhConst.MaGiaDinh] };

            DataTable tblTVGD = Memory.GetTable(ThanhVienGiaDinhConst.TableName, "");

            //Tim va update dia chi cua tung thanh vien trong gia dinh
            foreach (DataRow rowTV in tblTVGD.Rows)
            {
                //tim gia dinh cua thanh vien
                int maGiaDinh = (int)rowTV[ThanhVienGiaDinhConst.MaGiaDinh];
                DataRow rowGiaDinh = tblGiaDinh.Rows.Find(maGiaDinh);
                if (maGiaDinh != null)
                {
                    //tim row giao dan
                    int maGiaoDan = (int)rowTV[ThanhVienGiaDinhConst.MaGiaoDan];
                    DataRow rowGiaoDan = tblGiaoDan.Rows.Find(maGiaoDan);
                    if (rowGiaoDan != null)
                    {
                        //neu chua tung co dia chi
                        if (rowGiaoDan[GiaoDanConst.DiaChi].ToString().Trim() == "")
                        {
                            rowGiaoDan[GiaoDanConst.DiaChi] = rowGiaDinh[GiaDinhConst.DiaChi];
                        }
                        else if (rowTV[ThanhVienGiaDinhConst.VaiTro].ToString() == "0" || rowTV[ThanhVienGiaDinhConst.VaiTro].ToString() == "1")
                        {
                            //Neu la vo hoac chong, nghia la gia dinh rieng, thi update dia chi gia dinh rieng cho vo hoac chong, mac du co the 
                            // da co dia chi theo dia chi cua gia dinh cha me
                            rowGiaoDan[GiaoDanConst.DiaChi] = rowGiaDinh[GiaDinhConst.DiaChi];
                        }
                    }
                }
            }

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, IIF(GD.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo,
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) AS TenChong, 
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS TenVo,
                                                    (SELECT COUNT(MaGiaoDan) FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh = GD.MaGiaDinh) as SoLuong
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo=GiaoHo.MaGiaoHo ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            #endregion

            if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu cho phù hợp với phiên bản mới...", EventArgs.Empty);
            DataSet ds = new DataSet();
            ds.Tables.Add(tblGiaoDan);
            Memory.UpdateDataSet(ds);

            Memory.ClearError();
            #endregion

        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.1.0.1
        private void updateTo2_1_0_1()
        {
            #region Update version info
            Memory.ClearError();
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.1.0.1", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            #endregion
        }
        #endregion

        #region For version 2.1.0.0
        private void updateTo2_1_0_0()
        {
            if (OnExecuting != null) OnExecuting("Đang cập nhật cấu trúc dữ liệu...", EventArgs.Empty);

            //backup database 
            string backupPath = "";
            //frmRestore.BackupDatabase(out backupPath);

            #region UPDATE Database struct
            //create HonPhoi table
            string sqlCreateTable = @"ALTER TABLE TanHien 
                                    ADD COLUMN NgayVaoDCV TEXT(10),
                                     NgayVaoNhaThu TEXT(10),
                                     NgayVaoNhaTap TEXT(10),
                                     NgayVaoKhanLanDau TEXT(10),
                                     NgayVaoKhanTronDoi TEXT(10),
                                     NgayPhoTe TEXT(10),
                                     NgayThuPhongLM TEXT(10),
                                     NgayBonMang TEXT(10)
                                     ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            //if (Memory.ShowError())
            //{
            //    goto errorHandler;
            //}
            Memory.ClearError();
            
            #endregion

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, IIF(GD.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo,
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) AS TenChong, 
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS TenVo
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo=GiaoHo.MaGiaoHo ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            #endregion

            #region Giao ly
            string createKhoiGiaoLy = @"CREATE TABLE KhoiGiaoLy (
                                                            MaKhoi INTEGER PRIMARY KEY,
                                                            TenKhoi TEXT(255),
                                                            NguoiQuanLy INTEGER,      
                                                            GhiChu TEXT(255)
                                                            )";
            string createLopGiaoLy = @"CREATE TABLE LopGiaoLy (
                                                            MaLop INTEGER PRIMARY KEY,
                                                            TenLop TEXT(255),
                                                            MaKhoi INTEGER,
                                                            NamHoc INTEGER,          
                                                            PhongHoc TEXT(255),
                                                            GhiChu TEXT(255)
                                                            )";
            string createChiTietLopGiaoLy = @"CREATE TABLE ChiTietLopGiaoLy (
                                                            MaLop INTEGER,
                                                            MaGiaoDan INTEGER,
                                                            SoThuTu INTEGER,      
                                                            HoanThanh Bit,
                                                            GhiChuGLy TEXT(255),
                                                            CONSTRAINT PK_ChiTietLopGiaoLy PRIMARY KEY (MaLop, MaGiaoDan)
                                                            )";
            string createGiaoLyVien = @"CREATE TABLE GiaoLyVien (
                                                            MaLop INTEGER,
                                                            MaGiaoDan INTEGER,
                                                            CONSTRAINT PK_GiaoLyVien PRIMARY KEY (MaLop, MaGiaoDan)
                                                            )";
            Memory.ExecuteSqlCommand(createKhoiGiaoLy);
            //if (Memory.ShowError())
            //{
            //    goto errorHandler;
            //}

            Memory.ExecuteSqlCommand(createLopGiaoLy);
            //if (Memory.ShowError())
            //{
            //    goto errorHandler;
            //}

            Memory.ExecuteSqlCommand(createChiTietLopGiaoLy);
            //if (Memory.ShowError())
            //{
            //    goto errorHandler;
            //}

            Memory.ExecuteSqlCommand(createGiaoLyVien);
            //if (Memory.ShowError())
            //{
            //    goto errorHandler;
            //}

            #endregion

            #region Review data
            Memory.ClearError();
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.1.0.0", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            #endregion

            return;

        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.0.0.5
        private void updateTo2_0_0_5()
        {
            if (OnExecuting != null) OnExecuting("Đang cập nhật cấu trúc dữ liệu...", EventArgs.Empty);

            //backup database 
            string backupPath = "";
            //frmRestore.BackupDatabase(out backupPath);

            #region UPDATE Database struct
            //create HonPhoi table
            string sqlCreateTable = @"
            CREATE TABLE HonPhoi (
                                    MaHonPhoi INTEGER PRIMARY KEY,
                                    TenHonPhoi TEXT(255),
                                    SoHonPhoi TEXT(50),
                                    NoiHonPhoi TEXT(255), 
                                    NgayHonPhoi TEXT(10), 
                                    LinhMucChung TEXT(255), 
                                    NguoiChung1 TEXT(255), 
                                    NguoiChung2 TEXT(255), 
                                    CachThucHonPhoi TEXT(255), 
                                    GhiChu MEMO, 
                                    MaNhanDang TEXT(255),
                                    UpdateDate DATETIME
                                    ) 
            ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //Append HonPhoi table with more information from current GiaDinh table
            sqlCreateTable = @"
            INSERT INTO HonPhoi (MaHonPhoi, TenHonPhoi, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung, NguoiChung1, NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang, UpdateDate )
            SELECT MaGiaDinh, TenGiaDinh, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung, NguoiChung1, NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang, UpdateDate
            FROM (
            SELECT MaGiaDinh, TenGiaDinh, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung, NguoiChung1, NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang, UpdateDate,
            (SELECT TOP 1 MaGiaoDan FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) As NguoiNam,
            (SELECT TOP 1 MaGiaoDan FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS NguoiNu
            FROM GiaDinh GD
            )
            WHERE NguoiNam IS NOT NULL AND NguoiNu IS NOT NULL
            ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //create table link between GiaoDan and HonPhoi
            sqlCreateTable = @"
            CREATE TABLE GiaoDanHonPhoi (
                                    MaGiaoDan INTEGER,
                                    MaHonPhoi INTEGER,
                                    SoThuTu INTEGER, 
                                    CONSTRAINT PK_GiaoDan_HonPhoi PRIMARY KEY (MaGiaoDan, MaHonPhoi)
                                    ) 
            ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //copy giadinh to temporary table
            sqlCreateTable = @"
            SELECT GiaDinh.MaGiaDinh, GiaDinh.MaGiaoHo, GiaDinh.TenGiaDinh, GiaDinh.GhiChu, GiaDinh.DienThoai, 
            GiaDinh.DiaChi, GiaDinh.DaXoa, GiaDinh.UpdateDate, GiaDinh.DaChuyenXu, GiaDinh.NgayChuyen, GiaDinh.NoiChuyen, GiaDinh.GiaDinhAo, 
            GiaDinh.MaNhanDang, GiaDinh.MaGiaDinhRieng 
            INTO tam
            FROM GiaDinh";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //drop current GiaDinh table
            string sqlDrop = "DROP TABLE GiaDinh";
            Memory.ExecuteSqlCommand(sqlDrop);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //create new GiaDinh table
            sqlCreateTable = @"
            SELECT tam.MaGiaDinh, tam.MaGiaoHo, tam.TenGiaDinh, tam.GhiChu, tam.DienThoai, 
            tam.DiaChi, tam.DaXoa, tam.UpdateDate, tam.DaChuyenXu, tam.NgayChuyen, tam.NoiChuyen, tam.GiaDinhAo, 
            tam.MaNhanDang, tam.MaGiaDinhRieng 
            INTO GiaDinh
            FROM tam";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            Memory.ExecuteSqlCommand("DROP TABLE tam");

            sqlCreateTable = "ALTER TABLE GiaDinh ADD CONSTRAINT PK_GiaDinh PRIMARY KEY (MaGiaDinh)";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            sqlCreateTable = @"CREATE TABLE TanHien (
                                    MaTanHien INTEGER,
                                    MaGiaoDan INTEGER,
                                    NgayBatDau TEXT(10), 
                                    ChucVu TEXT(255), 
                                    NoiTu TEXT(255), 
                                    DongTu TEXT(255), 
                                    NoiPhucVu TEXT(255), 
                                    DiaChiPhucVu TEXT(255), 
                                    DienThoaiPhucVu TEXT(255), 
                                    EmailPhucVu TEXT(255), 
                                    GhiChu TEXT(255),
                                    DaHoiTuc BIT NOT NULL DEFAULT FALSE,
                                    CONSTRAINT PK_TanHien PRIMARY KEY (MaTanHien)
                                    ) ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            
            #endregion

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS SELECT GD.*, IIF(GD.MaGiaoHo = 0, ""Ngoài xứ"", GiaoHo.TenGiaoHo) AS TenGiaoHo,
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) AS TenChong, 
                                                    (SELECT TOP 1 GiaoDan.TenThanh + ' ' + GiaoDan.HoTen FROM ThanhVienGiaDinh TVGD,GiaoDan 
                                                    WHERE GiaoDan.MaGiaoDan=TVGD.MaGiaoDan AND TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS TenVo
                                                    FROM GiaDinh AS GD LEFT JOIN GiaoHo ON GD.MaGiaoHo=GiaoHo.MaGiaoHo ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            //Create VIEW select hon phoi list
            string createViewSELECT_HONPHOI_LIST = @"CREATE PROCEDURE SELECT_HONPHOI_LIST AS TRANSFORM First([A.TenThanh]+"" ""+[A.HoTen]) AS TenDayDu
                                SELECT HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate
                                FROM ((SELECT GiaoDanHonPhoi.*, TbPhai.Phai FROM GiaoDanHonPhoi INNER JOIN GiaoDan AS TbPhai ON GiaoDanHonPhoi.MaGiaoDan=TbPhai.MaGiaoDan)  AS GDHP INNER JOIN GiaoDan AS A ON GDHP.MaGiaoDan = A.MaGiaoDan) INNER JOIN HonPhoi AS HP ON GDHP.MaHonPhoi = HP.MaHonPhoi
                                GROUP BY HP.MaHonPhoi, HP.TenHonPhoi, HP.SoHonPhoi, HP.NoiHonPhoi, HP.NgayHonPhoi, HP.LinhMucChung, HP.NguoiChung1, HP.NguoiChung2, HP.CachThucHonPhoi, HP.GhiChu, HP.MaNhanDang, HP.UpdateDate
                                PIVOT GDHP.Phai ";
            Memory.ExecuteSqlCommand(createViewSELECT_HONPHOI_LIST);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            #endregion

            #region Giao ly
            string createKhoiGiaoLy = @"CREATE TABLE KhoiGiaoLy (
                                                            MaKhoi INTEGER PRIMARY KEY,
                                                            TenKhoi TEXT(255),
                                                            NguoiQuanLy INTEGER,      
                                                            GhiChu TEXT(255)
                                                            )";
            string createLopGiaoLy = @"CREATE TABLE LopGiaoLy (
                                                            MaLop INTEGER PRIMARY KEY,
                                                            TenLop TEXT(255),
                                                            MaKhoi INTEGER,
                                                            Nam INTEGER,          
                                                            PhongHoc TEXT(255),
                                                            GhiChu TEXT(255)
                                                            )";
            string createChiTietLopGiaoLy = @"CREATE TABLE ChiTietLopGiaoLy (
                                                            MaLop INTEGER,
                                                            MaGiaoDan INTEGER,
                                                            SoThuTu INTEGER,      
                                                            HoanThanh Bit,
                                                            GhiChuGLy TEXT(255),
                                                            CONSTRAINT PK_ChiTietLopGiaoLy PRIMARY KEY (MaLop, MaGiaoDan)
                                                            )";
            string createGiaoLyVien = @"CREATE TABLE GiaoLyVien (
                                                            MaLop INTEGER,
                                                            MaGiaoDan INTEGER,
                                                            CONSTRAINT PK_GiaoLyVien PRIMARY KEY (MaLop, MaGiaoDan)
                                                            )";
            Memory.ExecuteSqlCommand(createKhoiGiaoLy);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            Memory.ExecuteSqlCommand(createLopGiaoLy);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            Memory.ExecuteSqlCommand(createChiTietLopGiaoLy);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            Memory.ExecuteSqlCommand(createGiaoLyVien);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            #endregion

            #region Review data
            Memory.ClearError();
            reViewData_2_0_0_5();
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.0.0.5", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            #endregion

            return;

        errorHandler:
            //frmRestore.RestoreDatabase(backupPath);
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        private void reViewData_2_0_0_5()
        {
            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu hôn phối để cập nhật cho phù hợp với phiên bản mới...", EventArgs.Empty);
            //create temporary table
            string sqlCreateTable = @"
            CREATE TABLE HonPhoiTemp (
                                    MaHonPhoi INTEGER PRIMARY KEY,
                                    NguoiNam INTEGER,
                                    NguoiNu INTEGER
                                    ) 
            ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }

            //Append HonPhoi table with more information from current GiaDinh table
            sqlCreateTable = @"
            INSERT INTO HonPhoiTemp (MaHonPhoi, NguoiNam, NguoiNu)
            SELECT GD.MaGiaDinh, (SELECT TOP 1 MaGiaoDan FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=0) As NguoiNam, 
            (SELECT TOP 1 MaGiaoDan FROM ThanhVienGiaDinh TVGD WHERE TVGD.MaGiaDinh=GD.MaGiaDinh AND TVGD.VaiTro=1) AS NguoiNu
            FROM GiaDinh GD
            ";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            //get data
            DataTable tblHonPhoi = Memory.GetData("SELECT * FROM HonPhoiTemp");
            if (Memory.ShowError())
            {
                goto errorHandler;
            }
            DataTable tblGiaoDan = Memory.GetData("SELECT * FROM GiaoDan");
            if (Memory.ShowError() ||tblGiaoDan == null)
            {
                goto errorHandler;
            }
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            if (tblHonPhoi != null)
            {
                if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu hôn phối của giáo dân cho phù hợp với phiên bản mới...", EventArgs.Empty);
                DataTable tblGiaoDanHP = Memory.GetTableStruct(GiaoDanHonPhoiConst.TableName);
                foreach (DataRow row in tblHonPhoi.Rows)
                {
                    if (!Memory.IsNullOrEmpty(row[HonPhoiConst.NguoiNam]) && !Memory.IsNullOrEmpty(row[HonPhoiConst.NguoiNu]))
                    {
                        DataRow newRow = tblGiaoDanHP.NewRow();
                        newRow[GiaoDanHonPhoiConst.MaHonPhoi] = row[HonPhoiConst.MaHonPhoi];
                        newRow[GiaoDanHonPhoiConst.MaGiaoDan] = row[HonPhoiConst.NguoiNam];
                        newRow[GiaoDanHonPhoiConst.SoThuTu] = 1;
                        tblGiaoDanHP.Rows.Add(newRow);

                        newRow = tblGiaoDanHP.NewRow();
                        newRow[GiaoDanHonPhoiConst.MaHonPhoi] = row[HonPhoiConst.MaHonPhoi];
                        newRow[GiaoDanHonPhoiConst.MaGiaoDan] = row[HonPhoiConst.NguoiNu];
                        newRow[GiaoDanHonPhoiConst.SoThuTu] = 1;
                        tblGiaoDanHP.Rows.Add(newRow);
                    }
                    else
                    {
                        int maGiaoDan = -1;
                        if (!Memory.IsNullOrEmpty(row[HonPhoiConst.NguoiNam])) maGiaoDan = (int)row[HonPhoiConst.NguoiNam];
                        else if (!Memory.IsNullOrEmpty(row[HonPhoiConst.NguoiNu])) maGiaoDan = (int)row[HonPhoiConst.NguoiNu];
                        if (maGiaoDan > -1)
                        {
                            DataRow[] rows = tblGiaoDan.Select(string.Concat("MaGiaoDan=", maGiaoDan));
                            if (rows != null && rows.Length > 0)
                            {
                                rows[0][GiaoDanConst.DaCoGiaDinh] = false;
                            }
                        }
                    }
                }
                if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu hôn phối của giáo dân...", EventArgs.Empty);
                DataSet ds = new DataSet();
                ds.Tables.Add(tblGiaoDanHP);
                ds.Tables.Add(tblGiaoDan);
                tblGiaoDanHP.TableName = GiaoDanHonPhoiConst.TableName;
                Memory.UpdateDataSet(ds);
                if (Memory.ShowError())
                {
                    if (OnError != null) OnError(this, new CancelEventArgs());
                    Memory.ClearError();
                    return;
                }
            }
            //drop temporary table
            string dropSql = "DROP Table HonPhoiTemp";
            Memory.ExecuteSqlCommand(dropSql);
            if (Memory.ShowError())
            {
                if (OnError != null) OnError(this, new CancelEventArgs());
                Memory.ClearError();
                return;
            }

            //update ma giao ho "ngoai xu" = -1 cho tat ca
            //cho giao dan
            sqlCreateTable = "UPDATE GiaoDan SET MaGiaoHo=0 WHERE MaGiaoHo=-1";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                if (OnError != null) OnError(this, new CancelEventArgs());
                Memory.ClearError();
                return;
            }

            //cho gia dinh
            sqlCreateTable = "UPDATE GiaDinh SET MaGiaoHo=0 WHERE MaGiaoHo=-1";
            Memory.ExecuteSqlCommand(sqlCreateTable);
            if (Memory.ShowError())
            {
                if (OnError != null) OnError(this, new CancelEventArgs());
                Memory.ClearError();
                return;
            }
            return;

            errorHandler:
            //drop temporary table
            dropSql = "DROP Table HonPhoiTemp";
            Memory.ExecuteSqlCommand(dropSql);
            if (Memory.ShowError())
            {
                if (OnError != null) OnError(this, new CancelEventArgs());
                Memory.ClearError();
                return;
            }
            if (OnError != null) OnError(this, new CancelEventArgs());
            Memory.ClearError();
        }
        #endregion

        #region For version 2.0.0.4
        private void updateTo2_0_0_4()
        {
            string[] alterColumns = null;
            string[] alterColumnType = null;
            string sql = "";
            if (OnExecuting != null) OnExecuting("Đang cập nhật cấu trúc dữ liệu...", EventArgs.Empty);
            alterColumns = new string[] { "MaGiaDinhRieng" };
            alterColumnType = new string[] { "TEXT(255)" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaDinh ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS TRANSFORM First([TenThanh]+"" ""+[HoTen]) AS TenDayDu 
                                SELECT GD.MaGiaDinh, GD.MaGiaDinhRieng, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo, GD.MaNhanDang
                                FROM ((SELECT * FROM ThanhVienGiaDinh INNER JOIN VaiTro ON ThanhVienGiaDinh.VaiTRo=VaiTro.ID)  AS TVGD INNER JOIN (SELECT * FROM GiaoDan)  AS A ON TVGD.MaGiaoDan = A.MaGiaoDan) INNER JOIN (SELECT GiaDinh.*, GiaoHo.TenGiaoHo FROM GiaDinh LEFT JOIN GiaoHo ON GiaDinh.MaGiaoHo=GiaoHo.MaGiaoHo)  AS GD ON TVGD.MaGiaDinh = GD.MaGiaDinh
                                GROUP BY GD.MaGiaDinh, GD.MaGiaDinhRieng, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo, GD.MaNhanDang
                                PIVOT TVGD.Value
                                ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            #endregion

            Memory.ClearError();

            Memory.ExecuteSqlCommand("INSERT INTO CauHinh VALUES(?,?,?,?)", GxConstants.CF_TUNHAP_MAGIADINH, GxConstants.CF_FALSE, "Cho phép tự nhập mã gia đình", Memory.Instance.GetServerDateTime());
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.0.0.4", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            Memory.ClearError();
            //reviewDateData_2_0_0_4();
        }

        #endregion

        #region For version 2.0.0.3
        private void updateTo2_0_0_3()
        {
            string[] alterColumns = null;
            string[] alterColumnType = null;
            string sql = "";

            if (OnExecuting != null) OnExecuting("Đang cập nhật cấu trúc dữ liệu...", EventArgs.Empty);
            alterColumns = new string[] { "ThuocGiaoXu" };
            alterColumnType = new string[] { "TEXT(255)" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaoDan ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }

            string createRaoHonPhoi = @"CREATE TABLE RaoHonPhoi (
                                                            MaRaoHonPhoi INTEGER PRIMARY KEY,
                                                            TenRaoHonPhoi TEXT(255),
                                                            MaGiaoDan1 INTEGER,
                                                            MaGiaoDan2 INTEGER,
                                                            NgayRaoLan1 TEXT(10),
                                                            NgayRaoLan2 TEXT(10),
                                                            NgayRaoLan3 TEXT(10),
                                                            GiaoXu1 TEXT(10),
                                                            GiaoPhan1 TEXT(255),
                                                            GiaoXuTruoc1 TEXT(255),
                                                            GiaoPhanTruoc1 TEXT(255),
                                                            GiaoXu2 TEXT(255),
                                                            GiaoPhan2 TEXT(255),
                                                            GiaoXuTruoc2 TEXT(255),
                                                            GiaoPhanTruoc2 TEXT(255),
                                                            LinhMucNhan TEXT(255),
                                                            GiaoXuNhan TEXT(255),
                                                            GhiChu TEXT(255),
                                                            Tam1 TEXT(255),
                                                            Tam2 TEXT(255),
                                                            Tam3 TEXT(255),
                                                            UpdateDate DATETIME
                                                            ) 
                                                        ";
            Memory.ExecuteSqlCommand(createRaoHonPhoi);

            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.0.0.3", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            Memory.ClearError();
        }
        #endregion

        #region For version 2.0.0.2
        private void updateTo2_0_0_2()
        {
            string[] alterColumns = null;
            string[] alterColumnType = null;
            string sql = "";

            #region FOR TABLE STRUCTURE
            if (OnExecuting != null) OnExecuting("Đang cập nhật cấu trúc dữ liệu...", EventArgs.Empty);
            alterColumns = new string[] { "MaNhanDang" };
            alterColumnType = new string[] { "TEXT(255)" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaoDan ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }

            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaDinh ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }

            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaoHo ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }
            Memory.ClearError();
            #endregion

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS TRANSFORM First([TenThanh]+"" ""+[HoTen]) AS TenDayDu 
                                SELECT GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo, GD.MaNhanDang
                                FROM ((SELECT * FROM ThanhVienGiaDinh INNER JOIN VaiTro ON ThanhVienGiaDinh.VaiTRo=VaiTro.ID)  AS TVGD INNER JOIN (SELECT * FROM GiaoDan)  AS A ON TVGD.MaGiaoDan = A.MaGiaoDan) INNER JOIN (SELECT GiaDinh.*, GiaoHo.TenGiaoHo FROM GiaDinh LEFT JOIN GiaoHo ON GiaDinh.MaGiaoHo=GiaoHo.MaGiaoHo)  AS GD ON TVGD.MaGiaDinh = GD.MaGiaDinh
                                GROUP BY GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo, GD.MaNhanDang
                                PIVOT TVGD.Value
                                ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            #endregion

            Memory.ClearError();

            addMaNhanDang();

            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, "2.0.0.2", CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            Memory.ClearError();
        }
        private void addMaNhanDang()
        {
            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu giáo dân để cập nhật cho phù hợp với phiên bản mới...", EventArgs.Empty);
            DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, "");
            if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu giáo dân cho phù hợp với phiên bản mới...", EventArgs.Empty);
            if (!Memory.HasError() && tblGiaoDan != null)
            {
                foreach (DataRow row in tblGiaoDan.Rows)
                {
                    if (Memory.IsNullOrEmpty(row[GiaoDanConst.MaNhanDang]))
                    {
                        row[GiaoDanConst.MaNhanDang] = Memory.GetGiaoDanKey(row[GiaoDanConst.MaGiaoDan]);
                    }
                }
                if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu giáo dân...", EventArgs.Empty);
                DataSet ds = new DataSet();
                tblGiaoDan.TableName = GiaoDanConst.TableName;
                ds.Tables.Add(tblGiaoDan);
                Memory.UpdateDataSet(ds);
                if (Memory.ShowError())
                {
                    return;
                }
            }

            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu gia đình để cập nhật cho phù hợp với phiên bản mới...", EventArgs.Empty);
            DataTable tblGiaDinh = Memory.GetTable(GiaDinhConst.TableName, "");
            if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu gia đình cho phù hợp với phiên bản mới...", EventArgs.Empty);
            if (!Memory.HasError() && tblGiaDinh != null)
            {
                foreach (DataRow row in tblGiaDinh.Rows)
                {
                    if (Memory.IsNullOrEmpty(row[GiaDinhConst.MaNhanDang]))
                    {
                        row[GiaDinhConst.MaNhanDang] = Memory.GetGiaDinhKey(row[GiaDinhConst.MaGiaDinh]);
                    }
                }
                if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu gia đình...", EventArgs.Empty);
                DataSet ds = new DataSet();
                tblGiaDinh.TableName = GiaDinhConst.TableName;
                ds.Tables.Add(tblGiaDinh);
                Memory.UpdateDataSet(ds);
                if (Memory.ShowError())
                {
                    return;
                }
            }

            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu giáo họ để cập nhật cho phù hợp với phiên bản mới...", EventArgs.Empty);
            DataTable tblGiaoHo = Memory.GetTable(GiaoHoConst.TableName, "");
            if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu giáo họ cho phù hợp với phiên bản mới...", EventArgs.Empty);
            if (!Memory.HasError() && tblGiaoHo != null)
            {
                foreach (DataRow row in tblGiaoHo.Rows)
                {
                    if (Memory.IsNullOrEmpty(row[GiaoHoConst.MaNhanDang]))
                    {
                        row[GiaoHoConst.MaNhanDang] = Memory.GetGiaoHoKey(row[GiaoHoConst.MaGiaoHo]);
                    }
                }
                if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu giáo họ...", EventArgs.Empty);
                DataSet ds = new DataSet();
                tblGiaoHo.TableName = GiaoHoConst.TableName;
                ds.Tables.Add(tblGiaoHo);
                Memory.UpdateDataSet(ds);
                if (Memory.ShowError())
                {
                    return;
                }
            }
        }
        #endregion

        #region For version 2.0.0.1
        private void updateTo2_0_0_1()
        {
            string[] alterColumns = null;
            string[] alterColumnType = null;
            string sql = "";
            #region for GiaoDan
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho giáo dân...", EventArgs.Empty);
            alterColumns = new string[] { "TanTong" };
            alterColumnType = new string[] { "BIT NOT NULL DEFAULT FALSE" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaoDan ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }
            #endregion
            Memory.ExecuteSqlCommand(string.Format("UPDATE CauHinh SET {0}='{1}', {4}='{5}' WHERE {2}='{3}'", CauHinhConst.GiaTri, Memory.CurrentVersion, CauHinhConst.MaCauHinh, GxConstants.CF_CURRENT_DB_VERSION, CauHinhConst.UpdateDate, Memory.Instance.GetServerDateTime()));
            Memory.ClearError();
        }
        #endregion

        #region For version 2.0.0.0
        private bool hasUpdateDatabaseFor2_0_0_0()
        {
            DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, "");
            if (!Memory.ShowError() && tblGiaoDan != null)
            {
                if (tblGiaoDan.Columns.Contains(GiaoDanConst.SoRuocLe))
                {
                    return true;
                }
            }
            return false;
        }
        private void updateTo2_0_0_0()
        {
            //if (hasUpdateDatabaseFor2_0_0_0()) return;

            string[] alterColumns = null;
            string[] alterColumnType = null;
            string sql = "";
            #region for GiaoDan
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho giáo dân...", EventArgs.Empty);
            alterColumns = new string[] { "SoRuocLe", "HoTenCha", "HoTenMe", "DaCoGiaDinh", "GiaoDanAo" };
            alterColumnType = new string[] { "TEXT(255)", "TEXT(255)", "TEXT(255)", "BIT NOT NULL DEFAULT TRUE", "BIT" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaoDan ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
                //if (Memory.ShowError())
                //{
                //    return;
                //}
            }
            #endregion

            #region For GiaDinh
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho gia đình...", EventArgs.Empty);
            alterColumns = new string[] { "DaChuyenXu", "NgayChuyen", "NoiChuyen", "GiaDinhAo" };
            alterColumnType = new string[] { "BIT", "TEXT(10)", "TEXT(255)", "BIT" };
            for (int i = 0; i < alterColumns.Length; i++)
            {
                sql = string.Format("ALTER TABLE GiaDinh ADD COLUMN {0} {1}", alterColumns[i], alterColumnType[i]);
                Memory.ExecuteSqlCommand(sql);
            }

            sql = string.Format("ALTER TABLE GiaDinh ALTER COLUMN {0} TEXT(50)", HonPhoiConst.SoHonPhoi);
            Memory.ExecuteSqlCommand(sql);
            #endregion

            #region For DotBiTich
            string createDotBiTich = @"CREATE TABLE DotBiTich (
                                                            MaDotBiTich INTEGER PRIMARY KEY,
                                                            NgayBiTich TEXT(10),
                                                            MoTa TEXT(255),
                                                            LinhMuc TEXT(255),
                                                            LoaiBiTich INTEGER,
                                                            NoiBiTich TEXT(255),
                                                            UpdateDate DATETIME
                                                            ) 
                                                        ";
            Memory.ExecuteSqlCommand(createDotBiTich);
            //if (Memory.ShowError())
            //{
            //    return;
            //}
            #endregion

            #region For BiTichChiTiet
            string createBiTichChiTiet = @"CREATE TABLE BiTichChiTiet (
                                                            MaDotBiTich INTEGER,
                                                            MaGiaoDan INTEGER,
                                                            GhiChu TEXT(255),
                                                            UpdateDate DATETIME,
                                                            CONSTRAINT PK_MaDotBiTich_MaGiaoDan PRIMARY KEY (MaDotBiTich, MaGiaoDan)
                                                            ) 
                                                        ";
            Memory.ExecuteSqlCommand(createBiTichChiTiet);
            //if (Memory.ShowError())
            //{
            //    return;
            //}
            #endregion

            #region For ChuyenXu
            string createChuyenXu = @"CREATE TABLE ChuyenXu (
                                                            MaChuyenXu INTEGER PRIMARY KEY,
                                                            MaGiaoDan INTEGER,
                                                            NgayChuyen TEXT(10),
                                                            NoiChuyen TEXT(255),
                                                            LoaiChuyen INTEGER,
                                                            GhiChuChuyen TEXT(255),
                                                            UpdateDate DATETIME
                                                            ) 
                                                        ";
            Memory.ExecuteSqlCommand(createChuyenXu);
            //if (Memory.ShowError())
            //{
            //    return;
            //}
            #endregion

            #region For CauHinh
            string createCauHinh = @"CREATE TABLE CauHinh (
                                                            MaCauHinh TEXT(255) PRIMARY KEY,
                                                            GiaTri MEMO,
                                                            MoTa TEXT(255),
                                                            UpdateDate DATETIME
                                                            ) 
                                                        ";
            Memory.ExecuteSqlCommand(createCauHinh);
            Memory.ClearError();
            //if (Memory.ShowError())
            //{
            //    return;
            //}
            #endregion

            #region DROP VIEW SELECT_GIADINH_LIST
            string dropViewSELECT_GIADINH_LIST = "DROP PROCEDURE SELECT_GIADINH_LIST";
            Memory.ExecuteSqlCommand(dropViewSELECT_GIADINH_LIST);
            #endregion

            #region CREATE VIEW SELECT_GIADINH_LIST
            string createViewSELECT_GIADINH_LIST = @"CREATE PROCEDURE SELECT_GIADINH_LIST AS TRANSFORM First([TenThanh]+"" ""+[HoTen]) AS TenDayDu 
                                SELECT GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo
                                FROM ((SELECT * FROM ThanhVienGiaDinh INNER JOIN VaiTro ON ThanhVienGiaDinh.VaiTRo=VaiTro.ID)  AS TVGD INNER JOIN (SELECT * FROM GiaoDan)  AS A ON TVGD.MaGiaoDan = A.MaGiaoDan) INNER JOIN (SELECT GiaDinh.*, GiaoHo.TenGiaoHo FROM GiaDinh LEFT JOIN GiaoHo ON GiaDinh.MaGiaoHo=GiaoHo.MaGiaoHo)  AS GD ON TVGD.MaGiaDinh = GD.MaGiaDinh
                                WHERE (((TVGD.VaiTro)=0)) OR (((TVGD.VaiTro)=1))
                                GROUP BY GD.MaGiaDinh, GD.TenGiaDinh, GD.MaGiaoHo, GD.SoHonPhoi, GD.NgayHonPhoi, GD.NoiHonPhoi, GD.LinhMucChung, GD.NguoiChung1, GD.NguoiChung2, GD.CachThucHonPhoi, GD.GhiChu, GD.DienThoai, GD.DiaChi, GD.DaXoa, GD.UpdateDate, GD.TenGiaoHo, GD.DaChuyenXu, GD.NgayChuyen, GD.NoiChuyen, GD.GiaDinhAo
                                PIVOT TVGD.Value
                                ";
            Memory.ExecuteSqlCommand(createViewSELECT_GIADINH_LIST);
            #endregion

            Memory.ClearError();

            #region Review Data
            reviewDateData();
            #endregion

            #region for linh muc
            string updatehuVuLinhMuc = "UPDATE LinhMuc SET ChucVu=? WHERE ChucVu=? ";
            Memory.ExecuteSqlCommand(updatehuVuLinhMuc, new object[] { "Phó xứ", "Phụ tá" });
            #endregion

            #region Insert Default Config
            createCauHinh = "INSERT INTO CauHinh VALUES(?,?,?,?)";
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_CURRENT_DB_VERSION, Memory.CurrentVersion, "Phiên bản hiện tại", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_AUTO_UPDATE, 1, "Cho phép tự động cập nhật", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_LANGUAGE, GxConstants.LANG_VN, "Thư mục chứa các mẫu báo cáo", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_SOGIADINH_IN_GACHNGANG, 1, "In gạch ngang trong sổ gia đình với những người thuộc hồ sơ lưu trữ", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_SOGIADINH_IN_LAPGD, 1, "In cả những người đã lập gia đình trong sổ gia đình", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_SOGIADINH_IN_LUUTRU, 1, "In cả những người thuộc hồ sơ lưu trữ trong sổ gia đình", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_US_FORMAT_NAME, 1, "In họ tên theo định dạng kiểu Mỹ (tên trước họ sau)", Memory.Instance.GetServerDateTime() });
            Memory.ExecuteSqlCommand(createCauHinh, new object[] { GxConstants.CF_SOGIADINH_INNOISINH, 1, "In nơi sinh và nơi rửa tội", Memory.Instance.GetServerDateTime() });
            Memory.LoadConfig();
            #endregion
            Memory.ClearError();

        }

        public static void sendGiaoXuInfo()
        {
            try
            {
                if (Memory.ServerUrl != "" && Memory.IsConnectionAvailable())
                {
                    DataTable tblGiaoXuInfo = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                    if (tblGiaoXuInfo != null && tblGiaoXuInfo.Rows.Count > 0)
                    {
                        string giaoPhan = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoPhanConst.TenGiaoPhan].ToString());
                        string giaoHat = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoHatConst.TenGiaoHat].ToString());
                        string giaoXu = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoXuConst.TenGiaoXu].ToString());
                        string diaChi = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoXuConst.DiaChi].ToString());
                        string website = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoXuConst.Website].ToString());
                        string dienThoai = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoXuConst.DienThoai].ToString());
                        string email = System.Web.HttpUtility.UrlEncode(tblGiaoXuInfo.Rows[0][GiaoXuConst.Email].ToString());
                        string tongGiaoDan = "0";
                        string tongGiaDinh = "0";
                        DataTable tblTmp = Memory.GetData("SELECT COUNT(*) FROM GiaoDan");
                        if (!Memory.HasError() && tblTmp != null && tblTmp.Rows.Count > 0)
                        {
                            tongGiaoDan = tblTmp.Rows[0][0].ToString();
                        }
                        tblTmp = Memory.GetData("SELECT COUNT(*) FROM GiaDinh");
                        if (!Memory.HasError() && tblTmp != null && tblTmp.Rows.Count > 0)
                        {
                            tongGiaDinh = tblTmp.Rows[0][0].ToString();
                        }

                        string queryString = string.Format("?giaophan={0}&giaohat={1}&giaoxu={2}&diachi={3}&website={4}&dienthoai={5}&email={6}&tonggiadinh={7}&tonggiaodan={8}",
                                                giaoPhan, giaoHat, giaoXu, diaChi, website, dienThoai, email, tongGiaDinh, tongGiaoDan);

                        DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ");
                        if (tblLinhMuc != null && tblLinhMuc.Rows.Count > 0)
                        {
                            queryString += string.Format("&linhmuc={0}&dtlinhmuc={1}&emaillinhmuc={2}",
                                                    System.Web.HttpUtility.UrlEncode(tblLinhMuc.Rows[0][LinhMucConst.TenThanh].ToString() + " " + tblLinhMuc.Rows[0][LinhMucConst.HoTen].ToString()),
                                                    System.Web.HttpUtility.UrlEncode(tblLinhMuc.Rows[0][LinhMucConst.DienThoai].ToString()),
                                                    System.Web.HttpUtility.UrlEncode(tblLinhMuc.Rows[0][LinhMucConst.Email].ToString())
                                                    );
                        }
                        System.Net.WebClient web = new System.Net.WebClient();
                        web.OpenRead(Memory.ServerUrl + "themgiaoxu.aspx" + queryString);                            
                    }
                }
            }
            catch
            { }
        }

        private void reviewDateData()
        {
            try
            {
                #region for GiaoDan
                if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu để cập nhật cho phù hợp với phiên bản mới...", EventArgs.Empty);
                DataTable tblGiaoDan = Memory.GetTable(GiaoDanConst.TableName, "");
                DataTable tblGiaDinh = Memory.GetData(SqlConstants.SELECT_GIADINH_LIST);
                DataTable tblThanhVien = Memory.GetTable(ThanhVienGiaDinhConst.TableName, " ");//AND VaiTro=2
                if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu giáo dân cho phù hợp với phiên bản mới...", EventArgs.Empty);
                if (!Memory.ShowError() && tblGiaoDan != null && tblGiaoDan.Rows.Count > 0)
                {
                    tblGiaoDan.TableName = GiaoDanConst.TableName;
                    foreach (DataRow row in tblGiaoDan.Rows)
                    {
                        row[GiaoDanConst.NgaySinh] = Memory.GetStandardDateString(row[GiaoDanConst.NgaySinh]);
                        row[GiaoDanConst.NgayRuaToi] = Memory.GetStandardDateString(row[GiaoDanConst.NgayRuaToi]);
                        row[GiaoDanConst.NgayRuocLe] = Memory.GetStandardDateString(row[GiaoDanConst.NgayRuocLe]);
                        row[GiaoDanConst.NgayThemSuc] = Memory.GetStandardDateString(row[GiaoDanConst.NgayThemSuc]);
                        row[GiaoDanConst.NgayQuaDoi] = Memory.GetStandardDateString(row[GiaoDanConst.NgayQuaDoi]);
                        //Check Cha me
                        DataRow[] rowsTV = tblThanhVien.Select(string.Format("MaGiaoDan={0} AND VaiTro=2", row[GiaoDanConst.MaGiaoDan]));
                        if (rowsTV.Length > 0)
                        {
                            DataRow[] rowsGiaDinh = tblGiaDinh.Select(string.Format("MaGiaDinh={0}", rowsTV[0][GiaDinhConst.MaGiaDinh]));
                            if (rowsGiaDinh.Length > 0)
                            {
                                row[GiaoDanConst.HoTenCha] = rowsGiaDinh[0][GiaDinhConst.TenChong];
                                row[GiaoDanConst.HoTenMe] = rowsGiaDinh[0][GiaDinhConst.TenVo];
                            }
                        }
                        //Check DaCoGiaDinh
                        rowsTV = tblThanhVien.Select(string.Format("MaGiaoDan={0} AND (VaiTro=0 OR VaiTro=1)", row[GiaoDanConst.MaGiaoDan]));
                        if (rowsTV.Length > 0)
                        {
                            row[GiaoDanConst.DaCoGiaDinh] = true;
                        }
                    }
                    if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu giáo dân...", EventArgs.Empty);
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tblGiaoDan);
                    Memory.UpdateDataSet(ds);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
                #endregion

                #region for GiaDinh
                if (OnExecuting != null) OnExecuting("Đang xem xét dữ liệu gia đình cho phù hợp với phiên bản mới...", EventArgs.Empty);
                //DataTable tblGiaDinh = Memory.GetTable(GiaDinhConst.TableName, "");
                if (!Memory.ShowError() && tblGiaDinh != null && tblGiaDinh.Rows.Count > 0)
                {
                    tblGiaDinh.TableName = GiaDinhConst.TableName;
                    foreach (DataRow row in tblGiaDinh.Rows)
                    {
                        row[HonPhoiConst.NgayHonPhoi] = Memory.GetStandardDateString(row[HonPhoiConst.NgayHonPhoi]);
                        row[GiaDinhConst.NgayChuyen] = Memory.GetStandardDateString(row[GiaDinhConst.NgayChuyen]);
                        if (row[GiaDinhConst.TenGiaDinh].ToString() != "")
                        {
                            string[] arr = row[GiaDinhConst.TenGiaDinh].ToString().Split('-');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                arr[i] = arr[i].Trim();
                            }
                            row[GiaDinhConst.TenGiaDinh] = arr.Length > 0 ? string.Join(" - ", arr) : arr[0];
                        }
                    }
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tblGiaDinh);
                    if (OnExecuting != null) OnExecuting("Đang cập nhật dữ liệu gia đình...", EventArgs.Empty);
                    Memory.UpdateDataSet(ds);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Exception ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                if (OnError != null) OnError(ex.Message, new CancelEventArgs());
            }
        }

        #endregion

        #region For version 1.0.0.3
        private void updateTo1_0_0_3()
        {
            //for GiaoDan
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho giáo dân...", EventArgs.Empty);
            string[] alterColumns = new string[] { "NgaySinh", "NgayRuaToi", "NgayRuocLe", "NgayThemSuc", "NgayQuaDoi" };
            if (!hasDateTimeUpdated("GiaoDan", "NgaySinh"))
            {
                foreach (string col in alterColumns)
                {
                    string sql = string.Format("ALTER TABLE GiaoDan ALTER COLUMN {0} TEXT(10)", col);
                    Memory.ExecuteSqlCommand(sql);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
                convertDateData(alterColumns, GiaoDanConst.TableName);
            }
            //For GiaDinh
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho gia đình...", EventArgs.Empty);
            alterColumns = new string[] { "NgayHonPhoi" };
            if (!hasDateTimeUpdated("GiaDinh", "NgayHonPhoi"))
            {
                foreach (string col in alterColumns)
                {
                    string sql = string.Format("ALTER TABLE GiaDinh ALTER COLUMN {0} TEXT(10)", col);
                    Memory.ExecuteSqlCommand(sql);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
                convertDateData(alterColumns, GiaDinhConst.TableName);
            }
            //For LinhMuc
            if (OnExecuting != null) OnExecuting("Đang cập nhật cho linh mục...", EventArgs.Empty);
            alterColumns = new string[] { "NgaySinh", "TuNgay", "DenNgay" };
            if (!hasDateTimeUpdated("LinhMuc", "NgaySinh"))
            {
                foreach (string col in alterColumns)
                {
                    string sql = string.Format("ALTER TABLE LinhMuc ALTER COLUMN {0} TEXT(10)", col);
                    Memory.ExecuteSqlCommand(sql);
                    if (Memory.ShowError())
                    {
                        return;
                    }
                }
                convertDateData(alterColumns, LinhMucConst.TableName);
            }
        }

        private void convertDateData(string[] alterColumns, string tableName)
        {
            if (!isVietnameseDateTimeFormat())
            {
                DataTable tbl = Memory.GetData("SELECT * FROM " + tableName);
                if (tbl != null)
                {
                    System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
                    foreach (DataRow row in tbl.Rows)
                    {
                        foreach (string col in alterColumns)
                        {
                            object value = row[col];
                            if (value != null && value != DBNull.Value)
                            {
                                DateTime d = DateTime.Parse(value.ToString(), cul.DateTimeFormat);
                                value = Memory.GetDateString(d);
                                row[col] = value;
                            }
                        }
                    }
                }
                tbl.TableName = tableName;
                DataSet ds = new DataSet();
                ds.Tables.Add(tbl);
                Memory.UpdateDataSet(ds);
                Memory.ShowError();
            }
        }

        private bool isVietnameseDateTimeFormat()
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            if (cul.DateTimeFormat.LongDatePattern == "dddd d MMMM yyyy")
            {
                return true;
            }
            return false;
        }

        private bool hasDateTimeUpdated(string tableName, string colCheck)
        {
            DataTable tbl = Memory.GetData("SELECT * FROM " + tableName + " WHERE 0=1");
            if (tbl != null)
            {
                if (tbl.Columns[colCheck].DataType == typeof(DateTime))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region For check
        private bool hasUpdateDatabase(string tableName, string colName)
        {
            DataTable tbl = Memory.GetTableStruct(tableName);
            if (!Memory.ShowError() && tbl != null)
            {
                if (tbl.Columns.Contains(tableName))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

    }
}
