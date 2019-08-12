using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxGroupBox : Janus.Windows.EditControls.UIGroupBox
    {
        public GxGroupBox()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e) 
        {
            try
            {
                base.OnPaint(e);
            }
            catch
            {
            }
        }
    }
}
