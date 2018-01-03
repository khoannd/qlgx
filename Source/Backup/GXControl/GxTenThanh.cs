using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;
using GxGlobal;

namespace GxControl
{
    public partial class GxTenThanh : GxBaseField
    {

        public GxTenThanh()
        {
            InitializeComponent();
            InitControl();
            if (!Memory.IsDesignMode)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_TENTHANH);
            if (!Memory.ShowError())
            {
                uiComboBox1.DataSource = tbl;
                uiComboBox1.DisplayMember = GiaoDanConst.TenThanh;
                uiComboBox1.SelectedIndex = -1;
            }
        }

        protected override void InitControl()
        {
            uiComboBox1.AutoComplete = true;
            editBase = uiComboBox1;
            base.InitControl();
        }
    }
}
