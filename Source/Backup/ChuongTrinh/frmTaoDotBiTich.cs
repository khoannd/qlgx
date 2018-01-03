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
    public partial class frmTaoDotBiTich : frmBase
    {
        private GenerateDotBiTichProcess ProcessClass = new GenerateDotBiTichProcess();

        public frmTaoDotBiTich()
        {
            InitializeComponent();
            gxCommand1.OKButton.Text = "&Bắt đầu";
            txtKetQua.TextBox.ScrollBars = ScrollBars.Vertical;
        }

        private void frmTaoDotBiTich_Load(object sender, EventArgs e)
        {

        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            if(cbLoaiBiTich.Text == "")
            {
                Memory.ShowError("Xin vui lòng chọn loại bí tích cần tạo tự động.");
                return;
            }
            string from = Memory.GetIntOfDateFrom(dtDateFrom.Value.ToString()); ;
            string to = Memory.GetIntOfDateTo(dtDateTo.Value.ToString());
            if (from.CompareTo(to) == 1)
            {
                Memory.ShowError("Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
                return;
            }

            frmProcess frmUpdate = new frmProcess();
            frmUpdate.LabelStart = "Đang kiểm tra dữ liệu...";
            frmUpdate.Text = "Đang thực hiện việc tạo sổ bí tích tự động. Có thể mất vài phút. Xin vui lòng đợi...";
            frmUpdate.LableFinished = "Đã thực hiện xong!";

            ProcessClass = new GenerateDotBiTichProcess();
            ProcessClass.LoaiBiTich = (LoaiBiTich)cbLoaiBiTich.SelectedValue;
            ProcessClass.NguoiBanBiTich = txtLinhMuc.Text;
            ProcessClass.NoiBiTich = txtNoiBiTich.Text;
            ProcessClass.TuNgay = from;
            ProcessClass.DenNgay = to;

            frmUpdate.ProcessClass = ProcessClass;
            frmUpdate.FinishedFunction = new EventHandler(generateFinished);
            frmUpdate.ShowDialog();
        }

        private void generateFinished(object sender, EventArgs e)
        {
            string tongGiaoDan = ProcessClass.TongGiaoDan.ToString();
            string tongDotBitich = ProcessClass.TongDotBiTich.ToString();
            StringBuilder str = new StringBuilder();
            str.AppendLine(string.Format("Tổng số đợt bí tích được tạo: {0}", tongDotBitich));
            str.AppendLine(string.Format("Tổng số giáo dân được cho vào sổ bí tích: {0}", tongGiaoDan));
            txtKetQua.Text = str.ToString();
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
