using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class GxThongKeChung : UserControl
    {
        string totalText = "";
        public GxThongKeChung()
        {
            InitializeComponent();
            gxGiaoDanList1.Visible = true;
            gxGiaoDanList1.Dock = DockStyle.Fill;
            gxGiaDinhList1.Visible = false;
            lblTotal.Text = "";
            dtDateFrom.Text = DateTime.Now.Year.ToString();
            gxGiaoDanList1.LoadDataFinished += new EventHandler(gxGiaoDanList1_LoadDataFinished);
            gxGiaDinhList1.LoadDataFinished += new EventHandler(gxGiaDinhList1_LoadDataFinished);
            //txtDateFrom.TextBox.KeyUp += new KeyEventHandler(TextBox_KeyUp);
            SetCbCondition();
            SetAge();
            SetCombobox();
        }
        
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (dtDateFrom.Text.Length >= 4 && ((e.KeyValue >= 96 && e.KeyValue <= 105) || (e.KeyValue >= 48 && e.KeyValue <= 57)))
            {
                dtDateTo.Focus();
            }
        }

        private void gxGiaDinhList1_LoadDataFinished(object sender, EventArgs e)
        {
            lblTotal.Text = "Tổng cộng: " + gxGiaDinhList1.RowCount.ToString() + totalText;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.Crimson;
        }

        private void gxGiaoDanList1_LoadDataFinished(object sender, EventArgs e)
        {
            lblTotal.Text = "Tổng cộng: " + gxGiaoDanList1.RowCount.ToString() + totalText;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.Crimson;
        }

        private void gxHonPhoiList1_LoadDataFinished(object sender, EventArgs e)
        {
            lblTotal.Text = "Tổng cộng: " + gxHonPhoiList1.RowCount.ToString() + totalText;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.Crimson;
        }

        private bool checkYear()
        {
            Condition con = (Condition)(gxCbCondition).SelectedIndex;
            if (dtDateFrom.IsNullDate && !con.Equals(Condition.TONGSOGIADINH))
            {
                Memory.ShowError("Hãy nhập từ ngày");
                return false;
            }
            return true;
        }

        //2018-01-09 SonVc  Delete start
     /*   private bool checkRadio()
        {
            foreach (Control ctl in uiGroupBox1.Controls)
            {
                if (ctl is GxRadioBox && (ctl as GxRadioBox).Checked)
                {
                    return true;
                }
            }
            MessageBox.Show("Hãy chọn một điều kiện thống kê");
            return false;

        }*/
        //2018-01-09 SonVc  Delete end
        //2018-01-08 SonVc Add start
        public void EnableAge()
        {
            gxCbAgeFrom.Visible = true;
            gxCbToAge.Visible = true;
            dtDateFrom.Visible = false;
            dtDateTo.Visible = false;
            label1.Visible = false;
        }
        public void EnableDate()
        {
            gxCbAgeFrom.Visible = false;
            gxCbToAge.Visible = false;
            dtDateFrom.Visible = true;
            dtDateTo.Visible = true;
            label1.Visible = true;
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            Condition con = (Condition)(gxCbCondition).SelectedIndex;
            if (con.Equals(Condition.GIATRUONG) || con.Equals(Condition.HIENMAU)
                 || con.Equals(Condition.CHUHO))
            {
                if (!CheckAge()) return;
                if (!CheckCondition()) return;
                // Get Age
                Extract ex = new Extract(cbGiaoHo, gxGiaoDanList1)
                {
                    FromYear = (int)gxCbAgeFrom.Combo.SelectedItem,
                    ToYear = (int)gxCbToAge.combo.SelectedItem,
                    IsLuuTru = chkLuuTru.Checked ? true : false,
                    AllowNull = chkNullAccept.Checked ? true : false
                };
               
                if (con.Equals(Condition.CHUHO))
                {   
                    ex.SelectHeadByAge();
                    totalText = " chủ hộ";

                }
                else if (con.Equals(Condition.GIATRUONG))
                {
                    ex.SelectByVaiTro(true);
                    totalText = " gia trưởng";
                }
                else if (con.Equals(Condition.HIENMAU))
                {
                    ex.SelectByVaiTro(false);
                    totalText = " hiền mẫu";
                }
                gxGiaoDanList1.Dock = DockStyle.Fill;
                gxGiaoDanList1.Visible = true;
                gxGiaDinhList1.Visible = false;
                gxHonPhoiList1.Visible = false;
                btnPrint.Enabled = true;
                btnFilter.Enabled = true;
            }
            else if(con.Equals(Condition.CAONIEN) || con.Equals(Condition.THIEUNHI) || con.Equals(Condition.GIOITRE))
            {
                Extract ex = new Extract(cbGiaoHo, gxGiaoDanList1)
                {
                    FromYear = (int)gxCbAgeFrom.combo.SelectedItem,
                    ToYear = gxCbToAge.combo.SelectedItem != null ? (int)gxCbToAge.combo.SelectedItem : 0,
                };
                if (con.Equals(Condition.CAONIEN))
                {
                    ex.SelectByTuoi(TypeByAge.CAO_NIEN);
                    totalText = " cao niên";
                }
                else if (con.Equals(Condition.THIEUNHI))
                {
                    ex.SelectByTuoi(TypeByAge.THIEU_NHI);
                    totalText = " thiếu nhi";
                }
                else
                {
                    ex.SelectByTuoi(TypeByAge.GIOI_TRE);
                    totalText = " giới trẻ";
                }
                gxGiaoDanList1.Dock = DockStyle.Fill;
                gxGiaoDanList1.Visible = true;
                gxGiaDinhList1.Visible = false;
                gxHonPhoiList1.Visible = false;
            }
            else
            {

                if (dtDateTo.IsNullDate)
                {
                    dtDateTo.Value = dtDateFrom.Value;
                }
                if (!checkYear() || !CheckCondition())
                    return;
                int iDateFrom = int.Parse(Memory.GetIntOfDateFrom(dtDateFrom.DateInput.Day.Trim(), dtDateFrom.DateInput.Month.Trim(), dtDateFrom.DateInput.Year.Trim()));
                int iDateTo = int.Parse(Memory.GetIntOfDateTo(dtDateTo.DateInput.Day.Trim(), dtDateTo.DateInput.Month.Trim(), dtDateTo.DateInput.Year.Trim()));
                if (iDateFrom > iDateTo)
                {
                    Memory.ShowError("Từ ngày không thể lớn hơn đến ngày");
                    return;
                }

                string noDateSql = "";
                if (chkNullAccept.Checked)
                {
                    noDateSql = " OR {0} IS NULL OR {0} = \"\"";
                }
                //TODO: review again GET MONTH
                string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                    + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                    + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
                string dateSql = string.Concat(" AND (", convertDateToInt, " BETWEEN ? AND ?) ");

                //nếu không phải search theo hôn phối
                if (!con.Equals(Condition.HONPHOI) && !con.Equals(Condition.TONGSOGIADINH))
                {
                    string where = " AND DaXoa=0 ";
                    where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 AND QuaDoi=0 ";
                    if (con.Equals(Condition.SINHRA))
                    {
                        where += string.Format(dateSql + noDateSql, GiaoDanConst.NgaySinh);
                        totalText = " người được sinh ra";
                    }
                    else if (con.Equals(Condition.RUATOI))
                    {
                        where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuaToi);
                        totalText = " người được rửa tội";
                    }
                    else if (con.Equals(Condition.RUOCLELANDAU))
                    {
                        where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuocLe);
                        totalText = " người được xưng tội rước lễ lần đầu";
                    }
                    else if (con.Equals(Condition.THEMSUC))
                    {
                        where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayThemSuc);
                        totalText = " người được thêm sức";
                    }
                    else if (con.Equals(Condition.QUADOI))
                    {
                        where = " AND QuaDoi<>0 ";
                        if (!chkLuuTru.Checked)
                        {
                            where += " AND DaXoa=0 AND DaChuyenXu=0 ";
                        }                  
                        if (chkNullAccept.Checked)
                        {
                            string date = dateSql.Substring(0, dateSql.LastIndexOf(')'));
                            where += string.Format(date + " OR ({0} IS NULL OR {0} = \"\")) ", GiaoDanConst.NgayQuaDoi);
                        }
                        else
                        {
                            where += string.Format(dateSql, GiaoDanConst.NgayQuaDoi);
                        }
                        totalText = " người qua đời";
                    }
                    else if (con.Equals(Condition.TONGSOGIAODAN))
                    {
                        dtDateTo.Text = dtDateFrom.Text;
                        //where = " AND DaXoa=0 ";
                        //where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 AND QuaDoi=0 ";
                        where += string.Format(" AND (" + convertDateToInt + " <= ? " + noDateSql + ")", GiaoDanConst.NgaySinh);
                        iDateFrom = iDateTo;
                        totalText = " giáo dân đến thời điểm được nhập";
                    }
                    else if (con.Equals(Condition.TANTONG))
                    {
                        where = " AND TanTong<>0 ";
                        if (!chkLuuTru.Checked)
                        {
                            where += " AND DaXoa=0 AND DaChuyenXu=0 ";
                        }
                        if (chkNullAccept.Checked)
                        {
                            string date = dateSql.Substring(0, dateSql.LastIndexOf(')'));
                            where += string.Format(date + " OR ({0} IS NULL OR {0} = \"\")) ", GiaoDanConst.NgayRuaToi);
                        }
                        else
                        {
                            where += string.Format(dateSql, GiaoDanConst.NgayRuaToi);
                        }
                        totalText = " tân tòng được rửa tội";
                    }
                    if (cbGiaoHo.MaGiaoHo > -1)
                    {
                        where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                    }
                    if (cbGiaoHo.MaGiaoHo > 0 || con.Equals(Condition.TONGSOGIAODAN)) //neu thong ke 1 giao ho cu the hoac thong ke tong giao dan
                    {
                        where += " AND GiaoDanAo=0 ";
                    }
                    gxGiaoDanList1.LoadData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + where, new object[] { iDateFrom, iDateTo });
                    gxGiaoDanList1.Dock = DockStyle.Fill;
                    gxGiaoDanList1.Visible = true;
                    gxGiaDinhList1.Visible = false;
                    gxHonPhoiList1.Visible = false;
                    btnPrint.Enabled = true;
                    btnFilter.Enabled = true;
                }
                else // Hon phoi
                {
                    string where = "";
                    if (con.Equals(Condition.HONPHOI))
                    {
                        if (!Memory.CreateSELECT_HONPHOI_VIEW())
                        {
                            MessageBox.Show("Không thể lấy thông tin hôn phối", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        if (!checkStatus()) return;
                        totalText = " đôi chịu phép hôn phối";
                        if (chkNullAccept.Checked)
                        {
                            string date = dateSql.Substring(0, dateSql.LastIndexOf(')'));
                            where += string.Format(date + " OR ({0} IS NULL OR {0} = \"\")) ", HonPhoiConst.NgayHonPhoi);
                        }
                        else
                        {
                            where += string.Format(dateSql, HonPhoiConst.NgayHonPhoi);
                        }
                        //if (cbGiaoHo.MaGiaoHo > -1)
                        //{
                        //    where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                        //}

                        SetWhereForHonPhoi(ref where);
                        if (cbGiaoHo.MaGiaoHo > -1)
                        {
                            where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                        }
                        gxHonPhoiList1.LoadData(string.Concat(SqlConstants.SELECT_HONPHOI_LIST, where), new object[] { iDateFrom, iDateTo });
                        gxHonPhoiList1.Dock = DockStyle.Fill;
                        gxGiaDinhList1.Visible = false;
                        gxGiaoDanList1.Visible = false;
                        gxHonPhoiList1.Visible = true;
                    }
                    else if (con.Equals(Condition.TONGSOGIADINH))
                    {
                        where = " AND DaXoa=0 ";
                        if (cbGiaoHo.MaGiaoHo > 0) //neu thong ke 1 giao ho cu the
                        {
                            where += " AND GiaDinhAo=0 ";
                        }
                        where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 ";
                        totalText = " gia đình";

                        if (cbGiaoHo.MaGiaoHo > -1)
                        {
                            where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                        }
                        gxGiaDinhList1.LoadData(string.Concat(SqlConstants.SELECT_GIADINH_LIST, where), null);
                        gxGiaDinhList1.Dock = DockStyle.Fill;
                        gxGiaDinhList1.Visible = true;
                        gxGiaoDanList1.Visible = false;
                        gxHonPhoiList1.Visible = false;
                    }
                }
                btnPrint.Enabled = true;
                btnFilter.Enabled = true;

            }
        }

        public void SetWhereForHonPhoi(ref string where)
        {
            
            StatusMarried status = (StatusMarried)(gxCbMarried.SelectedIndex);
            if (status == StatusMarried.CHUAN)
            {
                where = string.Concat(where, " AND CachThucHonPhoi = 'Chuẩn'");
            }
            else if (status == StatusMarried.HOP_PHAP)
            {
                where = string.Concat(where, " AND CachThucHonPhoi = 'Hợp pháp'");
            }
            else if (status == StatusMarried.HOP_THUC_HOA)
            {
                where = string.Concat(where, " AND CachThucHonPhoi = 'Hợp thức hóa'");
            }
            else if (status == StatusMarried.LY_DI)
            {
                where = string.Concat(where, " AND CachThucHonPhoi = 'Ly dị'");
            }
            else if(status == StatusMarried.LY_THAN)
            {
                where = string.Concat(where, " AND CachThucHonPhoi = 'Ly thân'");
            }
        }
        private bool checkStatus()
        {
            if(gxCbMarried.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn 1 trạng thái hôn nhân","Nhắc nhở",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                gxCbMarried.Focus();
                return false;
            }
            return true;
        }

        public bool CheckAge()
        {
            if(gxCbAgeFrom.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Từ tuổi", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gxCbAgeFrom.Focus();
                return false;
            }
            if (gxCbToAge.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn Đến tuổi", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gxCbToAge.Focus();
                return false;
            }
            // 2018-11-24 Khoan add start
            if (int.Parse(gxCbAgeFrom.combo.SelectedItem.ToString()) > int.Parse(gxCbToAge.combo.SelectedItem.ToString()))
            {
                MessageBox.Show("Từ tuổi phải nhỏ hơn hoặc bằng đến tuổi", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gxCbToAge.Focus();
                return false;
            }
            // 2018-11-24 Khoan add start
            return true;
        }
        public bool CheckCondition()
        {
            if (gxCbCondition.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn điều kiện trích xuất", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gxCbAgeFrom.Focus();
                return false;
            }
            return true;
        }
        private void gxComboField1_SelectedIndexChanged(object sender, EventArgs e)
        {

            Condition con = (Condition)(gxCbCondition).SelectedIndex;
            if (con.Equals(Condition.GIATRUONG) || con.Equals(Condition.HIENMAU)
                 || con.Equals(Condition.CHUHO))
            {
                EnableAge();
                gxCbMarried.Visible = false;

            }
            else if (con.Equals(Condition.CAONIEN) || con.Equals(Condition.GIOITRE) || con.Equals(Condition.THIEUNHI))
            {
                gxCbMarried.Visible = false;
                dtDateFrom.Visible = false;
                gxCbToAge.Visible = false;
                dtDateTo.Visible = false;
                label1.Visible = false;
                if (con.Equals(Condition.CAONIEN))
                {
                    gxCbAgeFrom.Visible = true;
                    gxCbAgeFrom.combo.SelectedItem = GxConstants.TUOI_CAO_NIEN;
                }
                else if (con.Equals(Condition.GIOITRE))
                {
                    gxCbAgeFrom.Visible = true;
                    gxCbToAge.Visible = true;
                    string condi = GxConstants.TUOI_TRE;
                    gxCbAgeFrom.combo.SelectedItem = int.Parse(condi.Substring(0, condi.IndexOf('-')));
                    gxCbToAge.combo.SelectedItem = int.Parse(condi.Substring(condi.IndexOf('-') + 1));
                }
                else
                {
                    gxCbAgeFrom.Visible = true;
                    gxCbToAge.Visible = true;
                    string condi = GxConstants.TUOI_THIEU_NHI;
                    gxCbAgeFrom.combo.SelectedItem = int.Parse(condi.Substring(0, condi.IndexOf('-')));
                    gxCbToAge.combo.SelectedItem = int.Parse(condi.Substring(condi.IndexOf('-') + 1));

                }

            }
            else if (con.Equals(Condition.HONPHOI))
            {
                EnableDate();
                gxCbMarried.Visible = true;

            }
            else
            {
                EnableDate();
                gxCbMarried.Visible = false;
            }
            //ẩn hiện checkbox
            EnableChk();
        }
        public void SetCombobox()
        {
            gxCbAgeFrom.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            gxCbToAge.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            gxCbMarried.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            gxCbCondition.combo.DropDownStyle = ComboBoxStyle.DropDownList;

        }
        public void SetCbCondition()
        {
            gxCbCondition.combo.Items.Add("Sinh ra");
            gxCbCondition.combo.Items.Add("Rửa tội");
            gxCbCondition.combo.Items.Add("Xưng tội - rước lễ lần đầu");
            gxCbCondition.combo.Items.Add("Thêm sức");
            gxCbCondition.combo.Items.Add("Hôn phối");
            gxCbCondition.combo.Items.Add("Qua đời");
            gxCbCondition.combo.Items.Add("Tổng số giáo dân");
            // 2018-11-24 Khoan mod start
            //gxCbCondition.combo.Items.Add("Tổng số gia đình(không phụ thuộc năm thống kê)");
            gxCbCondition.combo.Items.Add("Tổng số gia đình (không phụ thuộc năm thống kê)");
            // 2018-11-24 Khoan mod end
            gxCbCondition.combo.Items.Add("Tân tòng (thống kê theo ngày rửa tội)");
            gxCbCondition.combo.Items.Add("Chủ hộ");
            gxCbCondition.combo.Items.Add("Gia trưởng");
            gxCbCondition.combo.Items.Add("Hiền mẫu");
            gxCbCondition.combo.Items.Add("Cao niên");
            gxCbCondition.combo.Items.Add("Giới trẻ");
            gxCbCondition.combo.Items.Add("Thiếu nhi");

            gxCbMarried.combo.Items.Add("Không phân loại");
            gxCbMarried.combo.Items.Add("Chuẩn");
            gxCbMarried.combo.Items.Add("Hợp thức hóa");
            gxCbMarried.combo.Items.Add("Hợp pháp");
            gxCbMarried.combo.Items.Add("Ly dị");
            gxCbMarried.combo.Items.Add("Ly thân");
        }
        private void SetAge()
        {
            for (int i = 1; i < 150; i++)
            {
                gxCbAgeFrom.combo.Items.Add(i);
                gxCbToAge.combo.Items.Add(i);
            }
        }

        //2018-01-08 SonVc Add ennd

        //2018-01-08 SonVc Delete start
        /*   private void btnSearch_Click1(object sender, EventArgs e)
           {


               if (dtDateTo.IsNullDate)
               {
                   dtDateTo.Value = dtDateFrom.Value;
               }

               if (!checkYear() || !checkRadio())
                   return;

               int iDateFrom = int.Parse(Memory.GetIntOfDateFrom(dtDateFrom.DateInput.Day.Trim(), dtDateFrom.DateInput.Month.Trim(), dtDateFrom.DateInput.Year.Trim()));
               int iDateTo = int.Parse(Memory.GetIntOfDateTo(dtDateTo.DateInput.Day.Trim(), dtDateTo.DateInput.Month.Trim(), dtDateTo.DateInput.Year.Trim()));
               if (iDateFrom > iDateTo)
               {
                   Memory.ShowError("Từ ngày không thể lớn hơn đến ngày");
                   return;
               }

               string noDateSql = "";
               if (chkNullAccept.Checked)
               {
                   noDateSql = " OR {0} IS NULL OR {0} = \"\"";
               }
               //TODO: review again GET MONTH
               string convertDateToInt = " INT(IIF(LEN([{0}])>=1,RIGHT([{0}], 4),\"0000\") " //year
                                   + "& IIF(LEN([{0}])>=7,MID([{0}],4,2),IIF(LEN([{0}])=7,MID([{0}],1,2),\"00\")) "  //month
                                   + "& IIF(LEN([{0}])=10,MID([{0}],1,2),\"00\")) ";//day
               string dateSql = string.Concat(" AND (", convertDateToInt, " BETWEEN ? AND ?) ");

               //nếu không phải search theo hôn phối
               if (!rdHonPhoi.Checked && !rdGiaDinh.Checked)
               {
                   string where = " AND DaXoa=0 ";
                   where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 AND QuaDoi=0 ";
                   if (rdSinhRa.Checked)
                   {
                       where += string.Format(dateSql + noDateSql, GiaoDanConst.NgaySinh);
                       totalText = " người được sinh ra";
                   }
                   else if (rdRuaToi.Checked)
                   {
                       where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuaToi);
                       totalText = " người được rửa tội";
                   }
                   else if (rdRuocLe.Checked)
                   {
                       where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayRuocLe);
                       totalText = " người được xưng tội rước lễ lần đầu";
                   }
                   else if (rdThemSuc.Checked)
                   {
                       where += string.Format(dateSql + noDateSql, GiaoDanConst.NgayThemSuc);
                       totalText = " người được thêm sức";
                   }
                   else if (rdQuaDoi.Checked)
                   {
                       where = " AND QuaDoi<>0 ";
                       if (!chkLuuTru.Checked)
                       {
                           where += " AND DaXoa=0 AND DaChuyenXu=0 ";
                       }
                       if (chkNullAccept.Checked)
                       {
                           where += string.Format(dateSql + " OR ({0} IS NULL OR {0} = \"\")) ", GiaoDanConst.NgayQuaDoi);
                       }
                       else
                       {
                           where += string.Format(dateSql, GiaoDanConst.NgayQuaDoi);
                       }
                       totalText = " người qua đời";
                   }
                   else if (rdConSong.Checked)
                   {
                       dtDateTo.Text = dtDateFrom.Text;
                       //where = " AND DaXoa=0 ";
                       //where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 AND QuaDoi=0 ";
                       where += string.Format(" AND (" + convertDateToInt + " <= ? " + noDateSql + ")", GiaoDanConst.NgaySinh);
                       iDateFrom = iDateTo;
                       totalText = " giáo dân đến thời điểm được nhập";
                   }
                   else if (rdTanTong.Checked)
                   {
                       where = " AND TanTong<>0 ";
                       if (!chkLuuTru.Checked)
                       {
                           where += " AND DaXoa=0 AND DaChuyenXu=0 ";
                       }
                       if (chkNullAccept.Checked)
                       {
                           where += string.Format(dateSql + " OR ({0} IS NULL OR {0} = \"\")) ", GiaoDanConst.NgayRuaToi);
                       }
                       else
                       {
                           where += string.Format(dateSql, GiaoDanConst.NgayRuaToi);
                       }
                       totalText = " tân tòng được rửa tội";
                   }
                   if (cbGiaoHo.MaGiaoHo > -1)
                   {
                       where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                   }
                   if (cbGiaoHo.MaGiaoHo > 0 || rdConSong.Checked) //neu thong ke 1 giao ho cu the hoac thong ke tong giao dan
                   {
                       where += " AND GiaoDanAo=0 ";
                   }
                   gxGiaoDanList1.LoadData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + where, new object[] { iDateFrom, iDateTo });
                   gxGiaoDanList1.Dock = DockStyle.Fill;
                   gxGiaoDanList1.Visible = true;
                   gxGiaDinhList1.Visible = false;
                   gxHonPhoiList1.Visible = false;
               }
               else // Hon phoi
               {
                   string where = "";
                   if (rdHonPhoi.Checked)
                   {
                       if (!Memory.CreateSELECT_HONPHOI_VIEW())
                       {
                           MessageBox.Show("Không thể lấy thông tin hôn phối", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                           return;
                       }
                       totalText = " đôi chịu phép hôn phối";
                       where += string.Format(dateSql + noDateSql, HonPhoiConst.NgayHonPhoi);
                       //if (cbGiaoHo.MaGiaoHo > -1)
                       //{
                       //    where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                       //}
                       gxHonPhoiList1.LoadData(string.Concat(SqlConstants.SELECT_HONPHOI_LIST, where), new object[] { iDateFrom, iDateTo });
                       gxHonPhoiList1.Dock = DockStyle.Fill;
                       gxGiaDinhList1.Visible = false;
                       gxGiaoDanList1.Visible = false;
                       gxHonPhoiList1.Visible = true;
                   }
                   else if (rdGiaDinh.Checked)
                   {
                       where = " AND DaXoa=0 ";
                       if (cbGiaoHo.MaGiaoHo > 0) //neu thong ke 1 giao ho cu the
                       {
                           where += " AND GiaDinhAo=0 ";
                       }
                       where += chkLuuTru.Checked ? "" : " AND DaChuyenXu=0 ";
                       totalText = " gia đình";

                       if (cbGiaoHo.MaGiaoHo > -1)
                       {
                           where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", cbGiaoHo.MaGiaoHo);
                       }
                       gxGiaDinhList1.LoadData(string.Concat(SqlConstants.SELECT_GIADINH_LIST, where), null);
                       gxGiaDinhList1.Dock = DockStyle.Fill;
                       gxGiaDinhList1.Visible = true;
                       gxGiaoDanList1.Visible = false;
                       gxHonPhoiList1.Visible = false;
                   }
               }
               btnPrint.Enabled = true;
               btnFilter.Enabled = true;
           }*/

        // 2018-01-08  SonVc Delete end

        private void txtYear_Leave(object sender, System.EventArgs e)
        {
            //txtDateFrom.Text = Memory.GetYear(txtDateFrom.Text).ToString();
        }

        private void gxGiaDinhList1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {
            //EditRow();
        }

        private void frmThongKeChung_Load(object sender, EventArgs e)
        {
            cbGiaoHo.SelectedValue = -1;
            gxGiaoDanList1.FormatGrid();
            gxGiaDinhList1.FormatGrid();
            gxHonPhoiList1.ListMode = 2;
            gxHonPhoiList1.FormatGrid();
            
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (gxGiaDinhList1.Visible && gxGiaDinhList1.RowCount > 0) gxGiaDinhList1.Print();
            else if (gxGiaoDanList1.Visible && gxGiaoDanList1.RowCount > 0) gxGiaoDanList1.Print();
            else if (gxHonPhoiList1.Visible && gxHonPhoiList1.RowCount > 0) gxHonPhoiList1.Print();
        }

        
        private void EnableChk()
        {
            Condition con = (Condition)(gxCbCondition).SelectedIndex;
            if (con.Equals(Condition.QUADOI) || con.Equals(Condition.SINHRA) || con.Equals(Condition.HONPHOI) || con.Equals(Condition.TONGSOGIAODAN) || con.Equals(Condition.TANTONG) || con.Equals(Condition.CHUHO )|| con.Equals(Condition.GIATRUONG) ||
                con.Equals(Condition.HIENMAU))
            {
                chkNullAccept.Enabled = true;
            }
            else
            {
                chkNullAccept.Checked = false;
                chkNullAccept.Enabled = false;
            }
            if (con.Equals(Condition.HONPHOI))
            {
                cbGiaoHo.SelectedValue = -1;
            }
        }
        private void btnFilter_Click(object sender, EventArgs e)
        {
            frmFilter frm = new frmFilter();
            Dictionary<object, object> dic = new Dictionary<object, object>();
            if (gxGiaoDanList1.Visible)
            {
                dic.Add(GiaoDanConst.HoTen, "");
                frm.GrdData = gxGiaoDanList1;
            }
            else if (gxGiaDinhList1.Visible)
            {
                dic.Add(GiaDinhConst.TenGiaDinh, "");
                frm.GrdData = gxGiaDinhList1;
            }
            else if (gxHonPhoiList1.Visible)
            {
                dic.Add(GxConstants.NAM, "");
                frm.GrdData = gxHonPhoiList1;
            }
            frm.FilterParameter = dic;
            frm.ShowDialog();
        }
        
        
    }
    //2018-01-8 SonVc add start
    /// <summary>
    /// Biểu diễn các lựa chọn cho điều kiện lọc dữ liệu
    /// </summary>
    enum Condition
    {
        SINHRA,
        RUATOI,
        RUOCLELANDAU,
        THEMSUC,
        HONPHOI,
        QUADOI,
        TONGSOGIAODAN,
        TONGSOGIADINH,
        TANTONG,
        CHUHO,
        GIATRUONG,
        HIENMAU,//2018-11-24 Khoan mod - switch pos between GIATRUONG & HIENMAU
        CAONIEN,
        GIOITRE,
        THIEUNHI
    }
    public enum TypeByAge
    {
        THIEU_NHI,
        GIOI_TRE,
        CAO_NIEN
    }
    public enum StatusMarried
    {
        KHONG_PHAN_LOAI,
        CHUAN,
        HOP_THUC_HOA,
        HOP_PHAP,
        LY_DI,
        LY_THAN
    }
    //2018-01-8 SonVc add end
}