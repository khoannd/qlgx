using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.EditControls;
using GxGlobal;
using Femiani.Forms.UI.Input;

namespace GxControl
{
    public partial class GxLinhMuc : GxTextField
    {
        public static DataTable TableLinhMuc = null;
        public static AutoCompleteEntryCollection Items = new AutoCompleteEntryCollection();
        private bool autoLoadData = true;

        public bool AutoLoadData
        {
            get { return autoLoadData; }
            set { autoLoadData = value; }
        }

        public GxLinhMuc()
        {
            InitializeComponent();
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //    if (autoLoadData)
        //    {
        //        LoadAutoCompleteData();
        //        TextBox.Items = Items;
        //    }
        //}

        //public static void LoadAutoCompleteData()
        //{
        //    try
        //    {
        //        if (Memory.IsDesignMode) return;
        //        if (Memory.Instance.GetMemory(GxConstants.LOAD_LINHMUC) != null) return;

        //        if (TableLinhMuc != null) TableLinhMuc.Dispose();
        //        TableLinhMuc = Memory.GetData("SELECT DISTINCT GiaoDan.ChaRuaToi AS TenLinhMuc FROM GiaoDan");
        //        if (!Memory.ShowError() && TableLinhMuc != null)
        //        {
        //            DataTable tbl1 = Memory.GetData("SELECT DISTINCT GiaoDan.ChaRuocLe AS TenLinhMuc FROM GiaoDan");
        //            if (!Memory.ShowError() && tbl1 != null)
        //            {
        //                MergeTables(TableLinhMuc, tbl1);
        //            }
        //            DataTable tbl2 = Memory.GetData("SELECT DISTINCT GiaoDan.ChaThemSuc AS TenLinhMuc FROM GiaoDan");
        //            if (!Memory.ShowError() && tbl2 != null)
        //            {
        //                MergeTables(TableLinhMuc, tbl2);
        //            }
        //            DataTable tbl3 = Memory.GetData("SELECT DISTINCT LinhMucChung AS TenLinhMuc FROM HonPhoi");
        //            if (!Memory.ShowError() && tbl3 != null)
        //            {
        //                MergeTables(TableLinhMuc, tbl3);
        //            }
        //            if (Items == null) Items = new AutoCompleteEntryCollection();
        //            Items.Clear();
        //            foreach (DataRow row in TableLinhMuc.Rows)
        //            {
        //                Items.Add(new AutoCompleteEntry(row["TenLinhMuc"].ToString(), row["TenLinhMuc"].ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
        //            }
        //        }
        //        Memory.Instance.SetMemory(GxConstants.LOAD_LINHMUC, true);
        //    }
        //    catch { }
        //}

        //private static void MergeTables(DataTable tbl1, DataTable tbl2)
        //{
        //    if (tbl1 == null || tbl2 == null) return;

        //    foreach (DataRow row in tbl2.Rows)
        //    {
        //        if (tbl1.Select(string.Format("TenLinhMuc='{0}'", row["TenLinhMuc"].ToString().Replace("'", "''"))).Length == 0)
        //        {
        //            tbl1.ImportRow(row);
        //        }
        //    }
        //}
    }
}
