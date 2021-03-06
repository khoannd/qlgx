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
    public partial class frmRaoHonPhoiList : frmBase
    {
        private object[] arguments = null;

        public object[] Arguments
        {
            get { return arguments; }
            set
            {
                arguments = value;
                gxRaoHonPhoiList1.Arguments = value;
            }
        }

        private string queryString = "";

        public string QueryString
        {
            get { return queryString; }
            set
            {
                queryString = value;
                gxRaoHonPhoiList1.QueryString = value;
            }
        }

        public frmRaoHonPhoiList()
        {
            InitializeComponent();

            this.HelpKey = "rao_hon_phoi";
            gxAddEdit1.DisplayMode = DisplayMode.Full;

            //gxAddEdit1.PrintButton.Text = "&In danh sách giáo dân";
            gxAddEdit1.PrintButton.Click += new EventHandler(btnInDanhSach_Click);

            gxAddEdit1.Button1.Text = "In điều &tra";
            gxAddEdit1.Button1.Click += new EventHandler(btnInRaoHonPhoi_Click);

            //gxAddEdit1.Button2.Visible = false;

            gxAddEdit1.Button2.Text = "In kết quả &rao hôn phối";
            gxAddEdit1.Button2.Click += new EventHandler(btnInKQRaoHonPhoi_Click);


            gxAddEdit1.ReloadButton.Visible = true;
            gxAddEdit1.GridData = gxRaoHonPhoiList1;

            gxAddEdit1.SelectButton.Visible = false;

            gxCommand1.OKButton.Visible = false;
            gxCommand1.CancelButton.Text = "Đó&ng";

            gxRaoHonPhoiList1.LoadDataFinished += new EventHandler(gxRaoHonPhoiList1_LoadDataFinished);
            gxRaoHonPhoiList1.FilterApplied += new EventHandler(gxRaoHonPhoiList1_FilterApplied);
            gxRaoHonPhoiList1.RowCountChanged += new EventHandler(gxRaoHonPhoiList1_RowCountChanged);

            lblTotal.Text = "";
            cbOption.Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOption.Combo.Items.Add("Chỉ xem những đôi rao chưa hoàn tất");
            cbOption.Combo.Items.Add("Xem tất cả");
            cbOption.Combo.SelectedIndexChanged += new EventHandler(Combo_SelectedIndexChanged);
            cbOption.SelectedIndex = 0;
        }

        public void LoadRaoHonPhoiList(string sql)
        {
            gxRaoHonPhoiList1.LoadData(sql, null);
        }

        public void LoadRaoHonPhoiList(string sql, object[] arguments)
        {
            gxRaoHonPhoiList1.LoadData(sql, arguments);
        }

        private void gxRaoHonPhoiList1_RowCountChanged(object sender, EventArgs e)
        {
            lblTotal.Text = gxRaoHonPhoiList1.RowCount.ToString() + " đôi rao";
        }

        public GxRaoHonPhoiList RaoHonPhoiList
        {
            get { return gxRaoHonPhoiList1; }
            set { gxRaoHonPhoiList1 = value; }
        }

        private void gxRaoHonPhoiList1_FilterApplied(object sender, EventArgs e)
        {
            
        }

        private void Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gxAddEdit1.ReloadButton.Enabled = true;
            gxAddEdit1.FindButton.Enabled = true;
            if (cbOption.SelectedIndex == 0)
            {
                gxRaoHonPhoiList1.ShowAll = false;
            }
            else
            {
                gxRaoHonPhoiList1.ShowAll = true;
            }
            
            gxRaoHonPhoiList1.LoadData();
        }

        private void gxRaoHonPhoiList1_LoadDataFinished(object sender, EventArgs e)
        {
            lblTotal.Text = gxRaoHonPhoiList1.RowCount.ToString() + " giáo dân";
        }


        private void btnInRaoHonPhoi_Click(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.EditRow(true);
        }

        private void btnInKQRaoHonPhoi_Click(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.EditRow(true,true);
        }

        private void frmRaoHonPhoiList_Load(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.FormatGrid();
            //gxRaoHonPhoiList1.LoadData();
            gxAddEdit1.ReloadButton.Enabled = true;
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.AddRow();
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.EditRow(false);
        }

        /// <summary>
        /// Truong hop xoa 1 gia dinh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.DeleteRow();
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnInDanhSach_Click(object sender, EventArgs e)
        {
            gxRaoHonPhoiList1.Print();
        }
    }
}