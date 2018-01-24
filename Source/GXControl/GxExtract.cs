using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using GxControl;

namespace GxControl
{
    public partial class GxExtract : UserControl
    {
        public GxExtract()
        {
            InitializeComponent();
            SetCombobox();
            cbCondition.SelectedIndexChanged += CbCondition_SelectedIndexChanged;
            
        }
        
        private void CbCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbCondition.Combo.SelectedItem!=null && cbCondition.Combo.SelectedItem.Equals("Chủ hộ"))
            {
                cbAgeTo.Visible = true;
                cbAgeFrom.Visible = true;
            }
            else
            {
                cbAgeTo.Visible = false;
                cbAgeFrom.Visible = false;
            }
           
        }

        public void SetCombobox()
        {
            for(int i = 18;i < 100; i++)
            {
                cbAgeFrom.Combo.Items.Add(i);
                cbAgeTo.Combo.Items.Add(i);
            }
            cbCondition.combo.Items.Add("Chủ hộ");
            cbCondition.combo.Items.Add("Hiền mẫu");
            cbCondition.combo.Items.Add("Gia trưởng");
            cbCondition.Combo.Items.Add("Cao niên");
            cbCondition.combo.Items.Add("Giới trẻ");
            cbCondition.Combo.Items.Add("Thiếu nhi");
          
        }
        private void gxComboField2_Load(object sender, EventArgs e)
        {

        }

        private void gxBtnTrichXuat_Click(object sender, EventArgs e)
        {
           if(cbCondition.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn điều kiện trích xuất", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbCondition.Focus();
                return;
            }
            if (cbCondition.Combo.SelectedItem.Equals("Chủ hộ"))
            {
                SelectHeadByAge();
            }
            else if (cbCondition.Combo.SelectedItem.Equals("Gia trưởng") )
            {
                SelectByVaiTro(true);
            }
            else if(cbCondition.Combo.SelectedItem.Equals("Hiền mẫu"))
            {
                SelectByVaiTro(false);
            }
        }
        public void SelectHeadByAge()
        {
            if (!checkAge()) return;
            
            int fromAge = int.Parse(cbAgeFrom.Combo.SelectedItem.ToString());
            int toAge = int.Parse(cbAgeTo.Combo.SelectedItem.ToString());
            
        }
        public void SelectByVaiTro(bool sex)
        {
            string where;
            if (sex == true)
            {
                 where = "AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 AND ThanhVienGiaDinh.VaiTro = 0 ";
            }
            else
            {
                where = "AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 AND ThanhVienGiaDinh.VaiTro = 1 ";
            }
            if (gxGiaoHo1.MaGiaoHo > -1)
            {
                where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", gxGiaoHo1.MaGiaoHo, gxGiaoHo1.MaGiaoHo);
            }
            gxGiaoDanList1.LoadData(string.Concat(SqlConstants.SELECT_LISTGIAODAN_BYGIADINH, where), null);
        }
        private bool checkAge()
        {
            if(cbAgeFrom.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng nhập từ tuổi", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbAgeFrom.Focus();
                return false;
            }
            if(cbAgeTo.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng nhập đến tuổi", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbAgeTo.Focus();
                return false;
            }
            return true;
        }
        private void GxExtract_Load(object sender, EventArgs e)
        {
            gxGiaoHo1.GridGiaoDan = gxGiaoDanList1;
          
            gxGiaoHo1.IsAo = Janus.Windows.GridEX.TriState.False;
            gxGiaoDanList1.FormatGrid();
        }

        private void gxButton2_Click(object sender, EventArgs e)
        {
            if (gxGiaoDanList1.Visible && gxGiaoDanList1.RowCount > 0) gxGiaoDanList1.Print();
        }
    }
}
