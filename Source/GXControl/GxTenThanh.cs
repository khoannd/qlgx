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
        public Janus.Windows.EditControls.UIComboBox Combobox
        {
            get { return uiCombobox; }
            set { uiCombobox = value; }
        }
        protected override void OnGotFocus(EventArgs e)
        {
            uiCombobox.Focus();
            base.OnGotFocus(e);
        }
        protected override void OnEnter(EventArgs e)
        {
            uiCombobox.Focus();
            base.OnEnter(e);
        }

        private void LoadData()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_TENTHANH);
            if (!Memory.ShowError())
            {
                uiCombobox.DataSource = tbl;
                uiCombobox.DisplayMember = GiaoDanConst.TenThanh;
                uiCombobox.SelectedIndex = -1;
                uiCombobox.Focus();
            }
        }

        protected override void InitControl()
        {
            uiCombobox.AutoComplete = true;
            editBase = uiCombobox;
            base.InitControl();
        }
    }
}
