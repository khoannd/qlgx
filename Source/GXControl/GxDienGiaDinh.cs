
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    /// <summary>
    /// 2018-07-18 Gia add start
    /// </summary>
    public partial class GxDienGiaDinh : GxComboField
    {
        public GxDienGiaDinh()
        {
            InitializeComponent();
            this.Combo.Items.Add("Nghèo");
            this.Combo.Items.Add("Cận nghèo");
            this.Combo.Items.Add("Neo đơn");
            this.Combo.Items.Add("Khuyết tật");
        }
    }
}
