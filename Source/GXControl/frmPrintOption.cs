using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class frmPrint : frmBase
    {
        public frmPrint()
        {
            InitializeComponent();
            gxCommand1.OKButton.DialogResult = DialogResult.OK;
            gxCommand1.CancelButton.DialogResult = DialogResult.Cancel;
           
        }
        public int getOption()
        {
            foreach (RadioButton item in pnRadio.Controls)
            {
                if (item.Checked==true)
                {
                    switch (item.Name)
                    {
                        case "SING":
                            return 0;
                        case "MUL":
                            return 1;
                    }
                }
            }
            return -1;
        }
    }
}
