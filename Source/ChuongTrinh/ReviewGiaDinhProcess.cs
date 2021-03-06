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
    public class ReviewGiaDinhProcess: IGxProcess
    {
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;
        private DataTable tblGiaDinhReviewed = null;
        private DataTable tblThanhVienGiaDinh = null;
        private ProcessOptions processOptions = ProcessOptions.ReviewGiaDinhProcess;
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

        private DataTable tblGiaDinh = null;
        private int maGiaoHo = -1;

        public int MaGiaoHo
        {
            get { return maGiaoHo; }
            set { maGiaoHo = value; }
        }

        private bool kiemTraKhongNgayHP = true;

        public bool KiemTraKhongNgayHP
        {
            get { return kiemTraKhongNgayHP; }
            set { kiemTraKhongNgayHP = value; }
        }

        private bool kiemTraHonPhoiTruocTuoi = true;

        public bool KiemTraHonPhoiTruocTuoi
        {
            get { return kiemTraHonPhoiTruocTuoi; }
            set { kiemTraHonPhoiTruocTuoi = value; }
        }

        private bool kiemTraKhoangCachTuoiConCai = true;

        public bool KiemTraKhoangCachTuoiConCai
        {
            get { return kiemTraKhoangCachTuoiConCai; }
            set { kiemTraKhoangCachTuoiConCai = value; }
        }

        private bool cacVanDeKhac = true;

        public bool CacVanDeKhac
        {
            get { return cacVanDeKhac; }
            set { cacVanDeKhac = value; }
        }

        public void Execute()
        {
            if (OnStart != null) OnStart("started", EventArgs.Empty);            
            try
            {
                reViewData();
            }
            catch (Exception ex)
            {
               if (OnError != null) OnError("Update: " + ex.Message, new CancelEventArgs());
            }
            if (OnFinished != null) OnFinished(tblGiaDinhReviewed, EventArgs.Empty);
        }

        private void reViewData()
        {
            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu gia đình để kiểm tra...", EventArgs.Empty);
            string where = " AND DaXoa=0 ";
            if (MaGiaoHo > -1)
            {
                where += string.Format(" AND (MaGiaoHo={0}) ", MaGiaoHo);
            }
            tblGiaDinh = Memory.GetData(SqlConstants.SELECT_GIADINH_LIST_CO_HONPHOI + where);
            tblGiaDinh.Columns.Add(GxConstants.NGUYEN_NHAN, typeof(string));
            tblGiaDinh.Columns.Add(GxConstants.KETQUA_KIEMRA, typeof(int));

            tblThanhVienGiaDinh = Memory.GetData(SqlConstants.SELECT_THANHVIENGIADINH_GIAODAN);

            if (!Memory.ShowError() && tblGiaDinh != null)
            {
                if (OnExecuting != null) OnExecuting("Đang kiểm tra dữ liệu gia đình...", EventArgs.Empty);
                tblGiaDinhReviewed = tblGiaDinh.Clone();
                List<int> lstMaGiaDinh = new List<int>();
                foreach (DataRow row in tblGiaDinh.Rows)
                {
                    bool hasError = false;
                    int ketQua = coNgayThangLoi(row);
                    if (ketQua == -1)
                    {
                        return;
                    }
                    if (ketQua > 0)
                    {
                        hasError = true;
                    }
                    if (!lstMaGiaDinh.Contains((int)row[GiaDinhConst.MaGiaDinh]))
                    {
                        ketQua = nhieuVoChong(row);
                        lstMaGiaDinh.Add((int)row[GiaDinhConst.MaGiaDinh]);
                        if (ketQua == -1)
                        {
                            return;
                        }
                        if (ketQua > 0)
                        {
                            hasError = true;
                        }
                    }
                    
                    if (hasError)
                    {
                        tblGiaDinhReviewed.ImportRow(row);
                    }
                }
                tblThanhVienGiaDinh.Dispose();
                tblGiaDinh.Dispose();
            }
        }

        private int coNgayThangLoi(DataRow row)
        {
            int ketQua = 0;
            StringBuilder str = new StringBuilder();
            DataRow[] rows = null;
            try
            {
                if (kiemTraKhongNgayHP)
                {
                    if (Memory.IsNullOrEmpty(row[HonPhoiConst.NgayHonPhoi]))
                    {
                        str.Append("- Không có ngày hôn phối\n");
                        ketQua += (int)ReviewGiaDinhType.KhongCoNgayHonPhoi;
                    }
                }

                if (kiemTraHonPhoiTruocTuoi)
                {
                    rows = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND VaiTro<=1", row[GiaDinhConst.MaGiaDinh]));
                    if (rows != null && rows.Length > 0)
                    {
                        int khoangCach = GxConstants.TUOI_HON_PHOI;
                        foreach (DataRow rowP in rows)
                        {
                            if (rowP[GiaoDanConst.Phai].ToString().ToLower() == GxConstants.NAM.ToLower())
                            {
                                khoangCach = GxConstants.TUOI_HON_PHOI_NAM;
                            }
                            else
                            {
                                khoangCach = GxConstants.TUOI_HON_PHOI_NU;
                            }
                            if (Memory.KiemTraTuoiKhongHopLe(rowP[GiaoDanConst.NgaySinh], row[HonPhoiConst.NgayHonPhoi], khoangCach))
                            {
                                str.Append(string.Format("- Người {0} hôn phối trước {1} tuổi\n", rowP[GiaoDanConst.Phai].ToString().ToLower(), khoangCach));
                                ketQua += (int)ReviewGiaDinhType.HonPhoiTruocTuoi;
                                break;
                            }
                        }
                    }
                }

                if (kiemTraKhoangCachTuoiConCai)
                {
                    rows = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND VaiTro<=1", row[GiaDinhConst.MaGiaDinh]));
                    DataRow[] rowsConCai = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND VaiTro=2", row[GiaDinhConst.MaGiaDinh]));
                    if (rows != null && rows.Length > 0)
                    {
                        bool isHopLy = true;
                        int khoangCach = GxConstants.TUOI_HON_PHOI;
                        string chame = "";
                        foreach (DataRow rowP in rows)
                        {
                            if (rowP[GiaoDanConst.Phai].ToString().ToLower() == GxConstants.NAM.ToLower())
                            {
                                khoangCach = GxConstants.TUOI_HON_PHOI_NAM;
                                chame = "người cha";
                            }
                            else
                            {
                                khoangCach = GxConstants.TUOI_HON_PHOI_NU;
                                chame = "người mẹ";
                            }
                            isHopLy = true;
                            foreach (DataRow rowC in rowsConCai)
                            {
                                if (Memory.KiemTraTuoiKhongHopLe(rowP[GiaoDanConst.NgaySinh], rowC[GiaoDanConst.NgaySinh], khoangCach))
                                {
                                    str.Append(string.Format("- Khoảng cách tuổi giữa {0} và con cái không hợp lý (nhỏ hơn {1} tuổi)\n", chame, khoangCach));
                                    ketQua += (int)ReviewGiaDinhType.KhoangCachTuoiKhongHopLe;
                                    isHopLy = false;
                                    break;
                                }
                            }
                            if (!isHopLy) break;
                        }
                    }
                }

                row[GxConstants.NGUYEN_NHAN] = str.ToString().Trim('\n');
                row[GxConstants.KETQUA_KIEMRA] = row[GxConstants.KETQUA_KIEMRA] != DBNull.Value ? (int)row[GxConstants.KETQUA_KIEMRA] + ketQua : ketQua;
            }
            catch (Exception ex)
            {
                if (OnError != null) OnError(ex.Message, new CancelEventArgs());
                return -1;
            }
            return ketQua;
        }

        private int nhieuVoChong(DataRow row)
        {
            int ketQua = 0;
            StringBuilder str = new StringBuilder("- ");
            DataRow[] rows = null;
            if (cacVanDeKhac)
            {
                 rows = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND VaiTro=0", row[GiaDinhConst.MaGiaDinh]));
                 if (rows != null && rows.Length > 1)
                 {
                     str.Append("Gia đình có nhiều chồng. ");
                     ketQua += (int)ReviewGiaDinhType.NhieuVoChong;
                 }
                 rows = tblThanhVienGiaDinh.Select(string.Format("MaGiaDinh={0} AND VaiTro=1", row[GiaDinhConst.MaGiaDinh]));
                 if (rows != null && rows.Length > 1)
                 {
                     str.Append("Gia đình có nhiều vợ. ");
                     if (ketQua == 0)
                     {
                         ketQua += (int)ReviewGiaDinhType.NhieuVoChong;
                     }
                 }
                 if (ketQua > 0)
                 {
                     str.Append("(do lỗi phiên bản trước. Hãy mở gia đình này lên, xem lại thông tin và bấm nút cập nhật để sửa lỗi)\n");
                     row[GxConstants.NGUYEN_NHAN] = str.ToString().Trim('\n');
                     row[GxConstants.KETQUA_KIEMRA] = row[GxConstants.KETQUA_KIEMRA] != DBNull.Value ? (int)row[GxConstants.KETQUA_KIEMRA] + ketQua : ketQua;
                 }
            }
            return ketQua;
        }
    }
}
