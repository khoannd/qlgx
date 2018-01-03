using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxComboBox : ComboBox
    {
        public GxComboBox()
        {
            InitializeComponent();
        }

        public new string SelectedText
        {
            get { return base.SelectedText; }
            set
            {
                try
                {
                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        if (this.Items[i].ToString().Equals(value))
                        {
                            this.SelectedIndex = i;
                            return;
                        }
                    }
                    this.Text = value;
                }
                catch { }
            }
        }

        /*
        private List<object> lstValue = new List<object>();

        protected override void AddItemsCore(object[] value)
        {
            foreach (object v in value)
            {
                lstValue.Add(v);
            }
            base.AddItemsCore(value);
        }

        public void AddItem(object value, object text)
        {

            lstValue.Add(value);
            AddItemsCore(new object[] { text });
        }

        public object SelectedValue
        {
            get {
                return lstValue[this.SelectedIndex];
            }
            set {
                for (int i = 0; i < lstValue.Count; i++)
                {
                    if (lstValue[i].Equals(value))
                    {
                        this.SelectedIndex = i;
                    }
                }
            }
        }
         * */

    }
}
