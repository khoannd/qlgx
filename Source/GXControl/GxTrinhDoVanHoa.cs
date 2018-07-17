using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxTrinhDoVanHoa : GxComboField
    {
        public GxTrinhDoVanHoa()
        {
            InitializeComponent();
            this.Combo.Items.Add("(Trống)");
            for (int i = 0; i < 12; i++)
            {
                this.Combo.Items.Add("Lớp " + (i + 1));
            }
        }
    }
}
