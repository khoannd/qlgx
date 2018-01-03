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
    public partial class GxBaseField : UserControl
    {
        public GxBaseField()
        {
            InitializeComponent();
        }

        protected int boxLeft = 0;

        public int BoxLeft
        {
            get { return boxLeft; }
            set
            {
                boxLeft = value;
                if (value > 0 && Memory.IsDesignMode)
                {
                    ChangeSize();
                }
            }
        }

        private bool autoUpperFirstChar = false;
        private bool allowAutoUpperFirstChar = Memory.GetConfig(GxConstants.CF_CHUANHOA_TUCHUANHOA) == GxConstants.CF_TRUE;

        public bool AutoUpperFirstChar
        {
            get { return autoUpperFirstChar; }
            set
            {
                allowAutoUpperFirstChar = !(Memory.GetConfig(GxConstants.CF_CHUANHOA_TUCHUANHOA) == GxConstants.CF_FALSE);
                if (!allowAutoUpperFirstChar)
                {
                    autoUpperFirstChar = false;
                }
                else
                {
                    autoUpperFirstChar = value;
                }
            }
        }

        public bool EditEnabled
        {
            get { return editBase != null ? editBase.Enabled : false; }
            set { editBase.Enabled = value; }
        }

        protected virtual void InitControl()
        {
            if(editBase == null) editBase = new Control();
            this.editBase.Location = new System.Drawing.Point(41, 3);
            this.editBase.Name = "editBase";
            //if (!Memory.IsDesignMode && (editBase is TextBox) && !((TextBox)editBase).Multiline)
            //{
            //    this.editBase.Size = new System.Drawing.Size(133, 25);
            //}
            this.editBase.TabIndex = 3;
            this.Controls.Add(this.editBase);
            this.label1.TextChanged += new EventHandler(label1_TextChanged);
            this.Resize += new EventHandler(CtlBase_Resize);
        }

        public string Label
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public override string Text
        {
            get { return editBase.Text.Trim(); }
            set { editBase.Text = value.Trim(); }
        }

        //public int BoxLeft
        //{
        //    get { return editBase.Left; }
        //    set { editBase.Left = value; }
        //}

        //public int BoxWidth
        //{
        //    get { return editBase.Width; }
        //    set { editBase.Width = value; }
        //}

        private void CtlBase_Resize(object sender, EventArgs e)
        {
            ChangeSize();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            editBase.Focus();
        }

        protected virtual void ChangeSize()
        {
            int spaceEditCtlLeft = 0;
            if (boxLeft == 0)
            {
                spaceEditCtlLeft = label1.Left + label1.Width;
            }
            else
            {
                label1.AutoSize = false;
                label1.Left = 0;
                label1.Width = boxLeft;
                spaceEditCtlLeft = boxLeft;
            }
            editBase.Left = spaceEditCtlLeft + GxConstants.CONTROL_DIS;
            editBase.Width = this.Width - label1.Width - 2 * GxConstants.CONTROL_DIS;
            //if (!Memory.IsDesignMode)
            //{
            //    if ((editBase is TextBox) && !((TextBox)editBase).Multiline)
            //    {
            //        this.Height = editBase.Height + GXConstants.CONTROL_DIS + 2;
            //    }
            //    else
            //    {
            //        editBase.Height = this.Height - (GXConstants.CONTROL_DIS + 2);
            //        this.Height = editBase.Height + GXConstants.CONTROL_DIS + 2;
            //    }
            //}
            editBase.Height = this.Height - (GxConstants.CONTROL_DIS + 2);
        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
            ChangeSize();
        }

        private void GxBaseField_Load(object sender, EventArgs e)
        {
            if(editBase!=null)
                editBase.Leave += new EventHandler(editBase_Leave);
        }

        private void editBase_Leave(object sender, EventArgs e)
        {
            if (autoUpperFirstChar)
            {
                editBase.Text = Memory.AutoUpperFirstChar(editBase.Text);
            }
        }
    }
}
