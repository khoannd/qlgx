using System;
using System.Collections.Generic;
using System.Text;
using GxControl;
using GxGlobal;
using System.Data;
using System.Xml;
using System.ComponentModel;

namespace GxControl
{
    public class GenerateDotBiTichProcess : IGxProcess
    {
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;

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

        private LoaiBiTich loaiBiTich = LoaiBiTich.TatCa;

        public LoaiBiTich LoaiBiTich
        {
            get { return loaiBiTich; }
            set { loaiBiTich = value; }
        }
        private string nguoiBanBiTich = "";

        public string NguoiBanBiTich
        {
            get { return nguoiBanBiTich; }
            set { nguoiBanBiTich = value; }
        }
        private string tuNgay = "";

        public string TuNgay
        {
            get { return tuNgay; }
            set { tuNgay = value; }
        }
        private string denNgay = "";

        public string DenNgay
        {
            get { return denNgay; }
            set { denNgay = value; }
        }

        private string noiBiTich = "";

        public string NoiBiTich
        {
            get { return noiBiTich; }
            set { noiBiTich = value; }
        }

        private int tongGiaoDan = 0;

        public int TongGiaoDan
        {
            get { return tongGiaoDan; }
            set { tongGiaoDan = value; }
        }
        private int tongDotBiTich = 0;

        public int TongDotBiTich
        {
            get { return tongDotBiTich; }
            set { tongDotBiTich = value; }
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
            if (OnFinished != null) OnFinished(tongGiaoDan, EventArgs.Empty);
        }

        private void reViewData()
        {
            if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu để tạo lập...", EventArgs.Empty);
            int total = 0;
            tongDotBiTich = 0;
            tongGiaoDan = 0;
            //select all dot bi tich
            DataTable tblDotBiTich = Memory.GetData("SELECT * FROM DotBiTich WHERE LoaiBiTich = ?", new object[] { loaiBiTich });
            Memory.ShowError();
            if (tblDotBiTich != null)
            {
                string colNameNgay = "";
                string colNameLinhMuc = "";
                string colNameNoiBitich = "";

                switch (loaiBiTich)
                {
                    case LoaiBiTich.RuaToi:
                        colNameNgay = GiaoDanConst.NgayRuaToi;
                        colNameLinhMuc = GiaoDanConst.ChaRuaToi;
                        colNameNoiBitich = GiaoDanConst.NoiRuaToi;
                        break;
                    case LoaiBiTich.RuocLe:
                        colNameNgay = GiaoDanConst.NgayRuocLe;
                        colNameLinhMuc = GiaoDanConst.ChaRuocLe;
                        colNameNoiBitich = GiaoDanConst.NoiRuocLe;
                        break;
                    case LoaiBiTich.ThemSuc:
                        colNameNgay = GiaoDanConst.NgayThemSuc;
                        colNameLinhMuc = GiaoDanConst.ChaThemSuc;
                        colNameNoiBitich = GiaoDanConst.NoiThemSuc;
                        break;
                }
                //TODO: review again
                string convertDateToInt = " INT(IIF(LEN([{0}])>=4,RIGHT([{0}], 4),\"0000\") " //year
                                                + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                                + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
                string selectGiaoDan = string.Concat("SELECT * FROM GiaoDan WHERE ", convertDateToInt, " BETWEEN ? AND ? ");

                if (noiBiTich != "")
                {
                    selectGiaoDan = string.Concat(selectGiaoDan, " AND ", colNameNoiBitich, " LIKE \"", noiBiTich, "\"");
                }

                if (nguoiBanBiTich != "")
                {
                    selectGiaoDan = string.Concat(selectGiaoDan, " AND ", colNameLinhMuc, " LIKE \"", nguoiBanBiTich, "\"");
                }

                selectGiaoDan = string.Concat(selectGiaoDan, "ORDER BY RIGHT([{0}], 4) ASC");
                if (OnExecuting != null) OnExecuting("Đang lấy dữ liệu để tạo lập các đợt bí tích...", EventArgs.Empty);
                DataTable tblGiaoDan = Memory.GetData(string.Format(selectGiaoDan, colNameNgay), new object[] { tuNgay, denNgay });
                Memory.ShowError();
                total = tblGiaoDan.Rows.Count;
                DataTable tblBiTichChiTiet = Memory.GetTable(BiTichChiTietConst.TableName, "");
                Memory.ShowError();

                if (OnExecuting != null) OnExecuting("Đang tạo các đợt bí tích...", EventArgs.Empty);

                if (tblGiaoDan.Rows.Count > 0)
                {
                    foreach (DataRow row in tblGiaoDan.Rows)
                    {
                        try {
                            DataRow rowDotBitich = GetDotBiTich(tblDotBiTich, loaiBiTich, row[colNameLinhMuc].ToString(),
                                                        row[colNameNgay].ToString(), row[colNameNoiBitich].ToString());
                            DataRow[] rowChiTiet = tblBiTichChiTiet.Select(string.Format("MaDotBiTich={0} AND MaGiaoDan={1}",
                                                                            rowDotBitich[DotBiTichConst.MaDotBiTich],
                                                                            row[GiaoDanConst.MaGiaoDan]));
                            if (rowChiTiet.Length == 0)
                            {
                                DataRow newRowChiTiet = tblBiTichChiTiet.NewRow();
                                newRowChiTiet[BiTichChiTietConst.MaDotBiTich] = rowDotBitich[DotBiTichConst.MaDotBiTich];
                                newRowChiTiet[BiTichChiTietConst.MaGiaoDan] = row[GiaoDanConst.MaGiaoDan];
                                newRowChiTiet[BiTichChiTietConst.UpdateDate] = DateTime.Now;
                                tblBiTichChiTiet.Rows.Add(newRowChiTiet);
                                tongGiaoDan++;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (OnError != null) OnError(string.Concat("Error at GiaoDan: ", row[GiaoDanConst.MaGiaoDan], ". Error: ", ex.Message), new CancelEventArgs());
                        }
                    }
                }
                DataSet dsUpdate = new DataSet();
                //tblGiaoDan.TableName = GiaoDanConst.TableName;
                tblBiTichChiTiet.TableName = BiTichChiTietConst.TableName;
                tblDotBiTich.TableName = DotBiTichConst.TableName;
                //dsUpdate.Tables.Add(tblGiaoDan);
                dsUpdate.Tables.Add(tblBiTichChiTiet);
                dsUpdate.Tables.Add(tblDotBiTich);
                Memory.UpdateDataSet(dsUpdate);
                Memory.ShowError();
            }
        }

        private DataRow GetDotBiTich(DataTable tblDotBiTich, LoaiBiTich loaiBitich, string tenLinhMuc, string ngay, string noiBitich)
        {
            string tenLMLower = tenLinhMuc.ToLower();
            string ngaySo = Memory.GetIntOfDateFrom(ngay);
            foreach (DataRow row in tblDotBiTich.Rows)
            {
                string sdate = Memory.GetIntOfDateFrom(row[DotBiTichConst.NgayBiTich].ToString());
                if (row[DotBiTichConst.LinhMuc].ToString().ToLower().Equals(tenLMLower)
                    && (int)row[DotBiTichConst.LoaiBiTich] == (int)loaiBitich
                    && sdate.CompareTo(ngaySo) == 0
                    )
                {
                    return row;
                }
            }
            DataRow newRow = tblDotBiTich.NewRow();
            newRow[DotBiTichConst.LinhMuc] = tenLinhMuc;
            newRow[DotBiTichConst.LoaiBiTich] = (int)loaiBitich;
            newRow[DotBiTichConst.MaDotBiTich] = Memory.Instance.GetNextId(DotBiTichConst.TableName, DotBiTichConst.MaDotBiTich, false);
            newRow[DotBiTichConst.NgayBiTich] = ngay;
            newRow[DotBiTichConst.NoiBiTich] = noiBitich;
            newRow[DotBiTichConst.UpdateDate] = DateTime.Now;
            //tao mo ta cho dot bi tich
            DateDTO dateDTO = Memory.GetDatePart(ngay);
            string mota = string.Format("Đợt bí tích{0}{1}{2}",
                dateDTO.Day != "" ? " ngày " + dateDTO.Day : "",
                dateDTO.Month != "" ? " tháng " + dateDTO.Month : "",
                dateDTO.Year != "" ? " năm " + dateDTO.Year : "");
            newRow[DotBiTichConst.MoTa] = mota;

            tblDotBiTich.Rows.Add(newRow);

            tongDotBiTich++;

            return newRow;
        }
    }
}
