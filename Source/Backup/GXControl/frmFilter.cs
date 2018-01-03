using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class frmFilter : frmBase
    {
        public GxGrid GrdData
        {
            get { return filterEditor1.GrdData; }
            set
            {
                filterEditor1.GrdData = value;
            }
        }

        public Dictionary<object, object> FilterParameter
        {
            get { return filterEditor1.FilterParameter; }
            set
            {
                filterEditor1.FilterParameter = value;
            }
        }
        public frmFilter()
        {
            InitializeComponent();
            gxCommand1.OKIsAccept = true;
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            filterEditor1.AutoApply(true);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}