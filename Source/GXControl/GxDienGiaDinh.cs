using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxDienGiaDinh : GxComboField
    {
        public GxDienGiaDinh()
        {
            InitializeComponent();
            this.Combo.Items.Add("Nghèo");
            this.Combo.Items.Add("Cận Nghèo");
            this.Combo.Items.Add("Neo Đơn");
            this.Combo.Items.Add("Khuyết Tật");
        }
    }
}
