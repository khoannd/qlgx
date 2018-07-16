using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxTrinhDoHocVan : GxComboField
    {
        public GxTrinhDoHocVan()
        {
            InitializeComponent();
            this.Combo.Items.Add("Trung cấp");
            this.Combo.Items.Add("Cao đẳng");
            this.Combo.Items.Add("Đại học");
            this.Combo.Items.Add("Trên đại học");
        }
    }
}
