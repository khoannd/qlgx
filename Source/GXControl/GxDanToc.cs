using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.EditControls;
using GxGlobal;

namespace GxControl
{
    public partial class GxDanToc : GxComboField
    {

        public GxDanToc()
        {
            InitializeComponent();
            label1.Text = "Dân tộc";
            uiComboBox1.ComboStyle = ComboStyle.DropDownList;
            InitControl();
            LoadData();
        }

        
        private void LoadData()
        {
            if (Memory.IsDesignMode) return;
            string[] list = new string[] { "", "Kinh", "Tày", "Thái", "Mường", "Khơ Me", "H'Mông", "Nùng", "Hoa", "Dao", "Gia Rai", "Ê Đê", "Ba Na", "Xơ Đăng", "Sán Chay", "Cơ Ho", "Chăm", "Sán Dìu", "Hrê", "Ra Glai", "M'Nông", "X’Tiêng", "Bru-Vân Kiều", "Thổ", "Khơ Mú", "Cơ Tu", "Giáy", "Giẻ Triêng", "Tà Ôi", "Mạ", "Co", "Chơ Ro", "Xinh Mun", "Hà Nhì", "Chu Ru", "Lào", "Kháng", "La Chí", "Phú Lá", "La Hủ", "La Ha", "Pà Thẻn", "Chứt", "Lự", "Lô Lô", "Mảng", "Cờ Lao", "Bố Y", "Cống", "Ngái", "Si La", "Pu Péo", "Rơ măm", "Brâu", "Ơ Đu" };
            this.Combo.Items.AddRange(list);
        }
        
    }
}
