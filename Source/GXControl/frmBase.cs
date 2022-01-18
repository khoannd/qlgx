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
    public partial class frmBase : Form
    {
        public Keys HK_UPDATE = Keys.F6;
        public Keys HK_CLOSE = Keys.F11;

        public frmBase()
        {
            InitializeComponent();
        }

        protected DataSet dataObj = null;

        public DataSet DataObj
        {
            get { return dataObj; }
            set { dataObj = value; }
        }

        protected GxOperation operation = GxOperation.ADD;

        public GxOperation Operation
        {
            get { return operation; }
            set
            {
                operation = value;
                //if (value == GxOperation.VIEW)
                //{
                //    foreach (Control ctl in this.Controls)
                //    {
                //        if (ctl is GxControl.GxGroupBox)
                //        {
                //            ctl.Enabled = false;
                //        }
                //    }
                //}
            }
        }

        protected int parentId = -1;

        public int ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        protected int id = -1;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        protected DataRow dataReturn = null;

        public DataRow DataReturn
        {
            get { return dataReturn; }
            set { dataReturn = value; }
        }

        protected DataRow currentRow = null;

        public DataRow CurrentRow
        {
            get { return currentRow; }
            set { currentRow = value; }
        }

        private string helpKey = "index";

        public string HelpKey
        {
            get { return helpKey; }
            set { helpKey = value; }
        }

        private void frmBase_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyCode == Keys.F1)
            //{
            //    frmHelp frm = new frmHelp();
            //    frm.SetHelp("");
            //    frm.ShowDialog();
            //}
        }

        private void frmBase_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            if (!Memory.IsDesignMode)
            {
                setControlSettings(this);
            }
        }

        private void frmBase_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            if (this.Modal)
            {
                frmHelp frm = new frmHelp();
                frm.SetHelp(HelpKey);
                frm.ShowDialog();
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.Modal && e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            switch (e.KeyValue)
            {
                case 120:
                    foreach (Control c in this.Controls)
                    {
                        GxCommand cmd = findGxCommand(c);
                        if (cmd != null && cmd.AllowHotkey)
                        {
                            cmd.OnCancelClick(this, e);
                        }
                    }
                    break;
                case 117:
                    foreach (Control c in this.Controls)
                    {
                        if (c is GxCommand && (c as GxCommand).AllowHotkey)
                        {
                            (c as GxCommand).OnOKClick(this, e);
                        }
                    }
                    break;
                default:
                    break;
            }
            base.OnKeyDown(e);
        }

        private GxCommand findGxCommand(Control parent)
        {
            if (parent.Controls.Count == 0) return null;
            foreach (Control c in parent.Controls)
            {
                if (c is GxCommand)
                {
                    return (GxCommand)c;
                }
                GxCommand cmd = findGxCommand(c);
                if (cmd != null) return cmd;
            }
            return null;
        }

        /// <summary>
        /// Load caption for each control in form
        /// </summary>
        /// <param name="ctl"></param>
        private static void setControlSettings(System.Windows.Forms.Control ctl)
        {
            foreach (Control control in ctl.Controls)
            {
                setControlSettings(control);
            }
            ctl.Font = new Font(Memory.GetFontName(), Memory.GetFontSize());
        }

        public void GetGridControlsCollection(Control root, ref List<Control> AllControls)
        {
            foreach (Control child in root.Controls)
            {
                if (child is GxGrid)
                {
                    AllControls.Add(child);
                }
                GetGridControlsCollection(child, ref AllControls);
            }
        }

        public void GetControlsCollection<T>(Control root, ref List<Control> AllControls)
        {
            foreach (Control child in root.Controls)
            {
                if (child is T)
                {
                    AllControls.Add(child);
                }
                GetControlsCollection<T>(child, ref AllControls);
            }
        }
        
        private void frmBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<Control> gridControls = new List<Control>();
            GetGridControlsCollection(this, ref gridControls);
            if (gridControls.Count > 0)
            {
                foreach (Control grd in gridControls)
                {
                    ((GxGrid)grd).UpdateColumnWidthToMemory();
                }
            }
        }

        public void setMinDate(GxDateField[] setForDateFields, string dateString, string dateLabel)
        {
            if (string.IsNullOrEmpty(dateString) || !Memory.IsValidDateString(dateString))
            {
                return;
            }
            foreach (var field in setForDateFields)
            {
                field.MinDate = dateString;
                field.MinDateName = dateLabel;
            }
        }

        public void setMinDate(GxDateField setForDateField, GxDateField dateFieldCheckMin, string dateLabel = null)
        {
            if (dateFieldCheckMin.IsNullDate || !dateFieldCheckMin.IsValidDate)
            {
                return;
            }
            setForDateField.MinDate = dateFieldCheckMin.DateString;
            setForDateField.MinDateName = dateLabel != null ? dateLabel : dateFieldCheckMin.Label;
        }

        public void setMinDate(GxDateField setForDateField, string dateString, string dateLabel)
        {
            if (string.IsNullOrEmpty(dateString) || !Memory.IsValidDateString(dateString))
            {
                return;
            }
            setForDateField.MinDate = dateString;
            setForDateField.MinDateName = dateLabel;
        }

        public void setMaxDate(GxDateField[] setForDateFields, string dateString, string dateLabel)
        {
            if (string.IsNullOrEmpty(dateString) || !Memory.IsValidDateString(dateString))
            {
                return;
            }
            foreach (var field in setForDateFields)
            {
                field.MaxDate = dateString;
                field.MaxDateName = dateLabel;
            }
        }

        public void setMaxDate(GxDateField setForDateField, string dateString, string dateLabel)
        {
            if (string.IsNullOrEmpty(dateString) || !Memory.IsValidDateString(dateString))
            {
                return;
            }
            setForDateField.MaxDate = dateString;
            setForDateField.MaxDateName = dateLabel;
        }

        public void setMaxDate(GxDateField setForDateField, GxDateField dateFieldCheckMax, string dateLabel = null)
        {
            if (dateFieldCheckMax.IsNullDate || !dateFieldCheckMax.IsValidDate)
            {
                return;
            }
            setForDateField.MaxDate = dateFieldCheckMax.DateString;
            setForDateField.MaxDateName = dateLabel != null ? dateLabel : dateFieldCheckMax.Label;
        }

        public bool checkDateConstraint(List<Control> dateFields)
        {
            foreach (var field in dateFields)
            {
                if (field is GxDateField dateField && !dateField.CheckDateConstraint())
                {
                    Control parent = findParentTab(field);
                    if (parent != null && parent.Parent is TabControl control)
                    {
                        control.SelectedTab = (TabPage)parent;
                    }
                    dateField.Focus();
                    return false;
                }
            }

            return true;
        }

        public TabPage findParentTab(Control ctl)
        {
            if(ctl == null || ctl.Parent == null) return null;
            
            if (ctl.Parent is TabPage page)
            {
                return page;
            }

            return findParentTab(ctl.Parent);
        }
    }
}