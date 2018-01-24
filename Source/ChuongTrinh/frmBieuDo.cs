using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using GxControl;

namespace GiaoXu
{
    public partial class frmBieuDo : frmBase
    {
        public frmBieuDo()
        {
            InitializeComponent();
            chkLuuTru.Checked = false;
            chkLuuTru.Enabled = false;
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            if (rdTongGiaoDan.Checked == false && rdDoTuoi.Checked == false && rdBiTich.Checked == false && rdGiaoHo.Checked == false && rdHonPhoi.Checked == false)
            {
                MessageBox.Show("Xin vui lòng chọn một loại biểu đồ cần xem", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dtDateFrom.Value == DBNull.Value)
            {
                MessageBox.Show("Xin vui lòng chọn ngày bắt đầu", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dtDateTo.Value == DBNull.Value)
            {
                MessageBox.Show("Xin vui lòng chọn ngày kết thúc", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int iDateFrom = int.Parse(Memory.GetIntOfDateFrom(dtDateFrom.DateInput.Day.Trim(), dtDateFrom.DateInput.Month.Trim(), dtDateFrom.DateInput.Year.Trim()));
            int iDateTo = int.Parse(Memory.GetIntOfDateTo(dtDateTo.DateInput.Day.Trim(), dtDateTo.DateInput.Month.Trim(), dtDateTo.DateInput.Year.Trim()));
            if (iDateFrom > iDateTo)
            {
                Memory.ShowError("Từ ngày không thể lớn hơn đến ngày");
                return;
            }
            
            if (rdTongGiaoDan.Checked)
            {
                exportTongGiaoDan(iDateFrom, iDateTo);
            }
            else if (rdBiTich.Checked)
            {
                exportBiTich(iDateFrom, iDateTo);
            }
            else if (rdDoTuoi.Checked)
            {
                exportDoTuoi();
            }
            else if (rdGiaoHo.Checked)
            {
                exportGiaoHo();
            }
            else if (rdHonPhoi.Checked)
            {
                exportTongHonPhoi(iDateFrom, iDateTo);
            }
        }

        private void exportTongGiaoDan(int iDateFrom, int iDateTo)
        {
            string noDateSql = "";
            if (chkNullAccept.Checked)
            {
                noDateSql = " OR {0} IS NULL OR {0} = \"\"";
            }
            string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
            string dateSql = string.Concat(" AND (", convertDateToInt, " <= ?) ");

            string where = " DaXoa=0 AND GiaoDanAo=0 ";
            where += chkLuuTru.Checked ? "" : " AND QuaDoi=0 ";
            where += string.Format(dateSql + noDateSql, GiaoDanConst.NgaySinh);
            string sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where;


            int fromYear = (int)(iDateFrom / 10000);
            int toYear = (int)(iDateTo / 10000);
            DataTable tbl = new DataTable();
            //lỗi
            for (int i = fromYear; i <= toYear; i++)
            {
                tbl.Columns.Add(i.ToString(), typeof(int));
            }
            DataRow row = tbl.NewRow();
            tbl.Rows.Add(row);
            for (int i = fromYear; i <= toYear; i++)
            {
                DataTable tblTmp = Memory.GetData(sql, new object[] { int.Parse(string.Concat(i,"3112")) });
                if (Memory.ShowError())
                {
                    return;
                }
                row[i.ToString()] = tblTmp.Rows[0][0];
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(tbl);
            ExcelReport.ChartTongGiaoDan.Export(ds);
        }

        private void exportTongHonPhoi(int iDateFrom, int iDateTo)
        {
            if (!Memory.CreateSELECT_HONPHOI_VIEW())
            {
                MessageBox.Show("Không thể lấy thông tin hôn phối", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string noDateSql = "";
            if (chkNullAccept.Checked)
            {
                noDateSql = " OR {0} IS NULL OR {0} = \"\"";
            }
            string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
            string dateSql = string.Concat(" AND (", convertDateToInt, "BETWEEN ? AND ?) ");

            string where = string.Format(dateSql + noDateSql, HonPhoiConst.NgayHonPhoi);
            //string.Concat(SqlConstants.SELECT_HONPHOI_LIST, where), new object[] { iDateFrom, iDateTo }
            string sql = "SELECT COUNT(MaHonPhoi) FROM (" + string.Concat(SqlConstants.SELECT_HONPHOI_LIST, where)+")";


            int fromYear = (int)(iDateFrom / 10000);
            int toYear = (int)(iDateTo / 10000);
            DataTable tbl = new DataTable();
            for (int i = fromYear; i <= toYear; i++)
            {
                tbl.Columns.Add(i.ToString(), typeof(int));
            }
            DataRow row = tbl.NewRow();
            tbl.Rows.Add(row);
            for (int i = fromYear; i <= toYear; i++)
            {
                DataTable tblTmp = Memory.GetData(sql, new object[] { int.Parse(string.Concat(i, "0000")), int.Parse(string.Concat(i, "3112")) });
                if (Memory.ShowError())
                {
                    return;
                }
                row[i.ToString()] = tblTmp.Rows[0][0];
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(tbl);
            ExcelReport.ChartTongHonPhoi.Export(ds);
        }

        private void exportBiTich(int iDateFrom, int iDateTo)
        {
            string noDateSql = "";
            //if (chkNullAccept.Checked)
            //{
            //    noDateSql = " OR {0} IS NULL OR {0} = \"\"";
            //}
            string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
            string dateSql = string.Concat(" AND (", convertDateToInt, " BETWEEN ? AND ?) ");

            string where = " DaXoa=0 AND GiaoDanAo=0 ";
            where += chkLuuTru.Checked ? "" : " AND QuaDoi=0 ";

            int fromYear = (int)(iDateFrom / 10000);
            int toYear = (int)(iDateTo / 10000);
            DataTable tbl = new DataTable();
            tbl.Columns.Add("Label", typeof(string));
            for (int i = fromYear; i <= toYear; i++)
            {
                tbl.Columns.Add(i.ToString(), typeof(int));
            }
            DataRow rowSinhRa = tbl.NewRow();
            rowSinhRa[0] = "Sinh ra";
            tbl.Rows.Add(rowSinhRa);
            DataRow rowRuaToi = tbl.NewRow();
            rowRuaToi[0] = "Rửa tội";
            tbl.Rows.Add(rowRuaToi);
            DataRow rowXTRL = tbl.NewRow();
            rowXTRL[0] = "XTRL lần đầu";
            tbl.Rows.Add(rowXTRL);
            DataRow rowThemSuc = tbl.NewRow();
            rowThemSuc[0] = "Thêm sức";
            tbl.Rows.Add(rowThemSuc);

            for (int i = fromYear; i <= toYear; i++)
            {
                object[] prm = new object[] { int.Parse(string.Concat(i, "0000")), int.Parse(string.Concat(i, "3112")) };
                //Get Sinh ra
                string sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where + string.Format(dateSql + noDateSql, GiaoDanConst.NgaySinh);
                DataTable tblTmp = Memory.GetData(sql, prm);
                if (Memory.ShowError())
                {
                    return;
                }
                rowSinhRa[i.ToString()] = tblTmp.Rows[0][0];

                //Get rua toi
                sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where + string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuaToi);
                tblTmp = Memory.GetData(sql, prm);
                if (Memory.ShowError())
                {
                    return;
                }
                rowRuaToi[i.ToString()] = tblTmp.Rows[0][0];

                //Get XTRL
                sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where + string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuocLe);
                tblTmp = Memory.GetData(sql, prm);
                if (Memory.ShowError())
                {
                    return;
                }
                rowXTRL[i.ToString()] = tblTmp.Rows[0][0];

                //Get rua toi
                sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where + string.Format(dateSql + noDateSql, GiaoDanConst.NgayThemSuc);
                tblTmp = Memory.GetData(sql, prm);
                if (Memory.ShowError())
                {
                    return;
                }
                rowThemSuc[i.ToString()] = tblTmp.Rows[0][0];

            }
            DataSet ds = new DataSet();
            ds.Tables.Add(tbl);
            ExcelReport.ChartBiTich.Export(ds);
        }

        private void exportDoTuoi()
        {
            string noDateSql = "";
            string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
            string dateSql = string.Concat(" AND (", convertDateToInt, " BETWEEN ? AND ?) ");

            string where = " DaXoa=0 AND GiaoDanAo=0 AND NgaySinh IS NOT NULL AND NgaySinh <> \"\" ";
            where += chkLuuTru.Checked ? "" : " AND QuaDoi=0 ";
            where += string.Format(dateSql + noDateSql, GiaoDanConst.NgaySinh);
            string sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE " + where;

            DataTable tbl = new DataTable();
            tbl.Columns.Add("Dưới 7 tuổi");
            tbl.Columns.Add("7 đến 12");
            tbl.Columns.Add("13 đến 16");
            tbl.Columns.Add("17 đến 25");
            tbl.Columns.Add("26 đến 30");
            tbl.Columns.Add("31 đến 50");
            tbl.Columns.Add("Trên 50");


            DataRow row = tbl.NewRow();
            tbl.Rows.Add(row);

            //khuc 1
            int fromYear = DateTime.Now.Year - 6;
            int toYear = DateTime.Now.Year;
            object[] prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            DataTable tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[0] = tblTmp.Rows[0][0];

            //khuc 2
            fromYear = DateTime.Now.Year - 12;
            toYear = DateTime.Now.Year - 7;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[1] = tblTmp.Rows[0][0];

            //khuc 3
            fromYear = DateTime.Now.Year - 16;
            toYear = DateTime.Now.Year - 13;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[2] = tblTmp.Rows[0][0];

            //khuc 4
            fromYear = DateTime.Now.Year - 25;
            toYear = DateTime.Now.Year - 17;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[3] = tblTmp.Rows[0][0];

            //khuc 5
            fromYear = DateTime.Now.Year - 30;
            toYear = DateTime.Now.Year - 26;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[4] = tblTmp.Rows[0][0];

            //khuc 6
            fromYear = DateTime.Now.Year - 50;
            toYear = DateTime.Now.Year - 31;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[5] = tblTmp.Rows[0][0];

            //khuc 7
            fromYear = 1990;
            toYear = DateTime.Now.Year - 51;
            prm = new object[] { int.Parse(string.Concat(fromYear, "0000")), int.Parse(string.Concat(toYear, "3112")) };

            tblTmp = Memory.GetData(sql, prm);
            if (Memory.ShowError())
            {
                return;
            }
            row[6] = tblTmp.Rows[0][0];

            DataSet ds = new DataSet();
            ds.Tables.Add(tbl);
            ExcelReport.ChartDoTuoi.Export(ds);
        }

        private void exportGiaoHo()
        {
            string sql = "SELECT COUNT(MaGiaoDan) FROM GiaoDan WHERE DaXoa=0 AND GiaoDanAo=0 AND MaGiaoHo=?";

            DataTable tbl = new DataTable();
            DataTable tblGiaoHo = Memory.GetData("SELECT * FROM GiaoHo");
            foreach (DataRow rowGH in tblGiaoHo.Rows)
            {
                tbl.Columns.Add(rowGH[GiaoHoConst.TenGiaoHo].ToString());
            }

            DataRow row = tbl.NewRow();
            tbl.Rows.Add(row);
            for (int i = 0; i < tblGiaoHo.Rows.Count; i++)
            {
                DataTable tblTmp = Memory.GetData(sql, new object[] { tblGiaoHo.Rows[i][GiaoHoConst.MaGiaoHo] });
                if (Memory.ShowError())
                {
                    return;
                }
                row[tblGiaoHo.Rows[i][GiaoHoConst.TenGiaoHo].ToString()] = tblTmp.Rows[0][0];
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(tbl);
            ExcelReport.ChartGiaoHo.Export(ds);
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rdTongGiaoDan_CheckedChanged(object sender, EventArgs e)
        {
            chkLuuTru.Visible = rdTongGiaoDan.Checked;
            chkLuuTru.Enabled = rdTongGiaoDan.Checked;
            if (!rdTongGiaoDan.Checked)
            {
                chkLuuTru.Checked = false;
            }
        }

        private void frmBieuDo_Load(object sender, EventArgs e)
        {
            gxCommand1.OKButton.Text = "&Xem";
        }
    }
}
