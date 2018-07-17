using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxNgoaiNgu : GxComboField
    {
        public GxNgoaiNgu()
        {
            InitializeComponent();
            this.Combo.Items.Add("Tiếng Anh");
            this.Combo.Items.Add("Tiếng Pháp");
            this.Combo.Items.Add("Tiếng Nga");
            this.Combo.Items.Add("Tiếng Đức");
            this.Combo.Items.Add("Tiếng Trung Quốc");
            this.Combo.Items.Add("Tiếng Nhật");
            
        }
    }
}
