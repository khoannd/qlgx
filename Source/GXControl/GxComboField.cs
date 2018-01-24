using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class GxComboField : GxBaseField
    {
        public event EventHandler SelectedIndexChanged;
        public GxComboField()
        {
            InitializeComponent();
            InitControl();
            combo.SelectedValueChanged += new EventHandler(combo_SelectedValueChanged);
            //combo.VisibleChanged += Combo_VisibleChanged;
        }

        private void Combo_VisibleChanged(object sender, EventArgs e)
        {
            combo.SelectedIndex = -1;
        }

        public int SelectedIndex
        {
            get { return combo.SelectedIndex; }
            set { combo.SelectedIndex = value; }
        }

        private void combo_SelectedValueChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null) SelectedIndexChanged(sender, e);
        }

        protected override void InitControl()
        {
            editBase = combo;
            base.InitControl();
        }

        public GxComboBox Combo
        {
            get { return combo; }
        }

        public string SelectedText
        {
            get { return combo.SelectedText; }
            set
            {
                combo.SelectedText = value;
            }
        }

        public object SelectedValue
        {
            get { return combo.SelectedValue; }
            set { combo.SelectedValue = value; }
        }

        public int MaxLength
        {
            get { return combo.MaxLength; }
            set { combo.MaxLength = value; }
        }
    }
}