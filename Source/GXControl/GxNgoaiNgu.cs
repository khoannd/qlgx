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
            this.Combo.Items.Add("Tiếng anh");
            this.Combo.Items.Add("Tiếng nhật");
            
        }
    }
}
