using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GXGlobal;

namespace GXControl
{
    public partial class GridGiaoDanTemplate : frmBase
    {
        public GridGiaoDanTemplate()
        {
            InitializeComponent();
            //gridEX1.LoadData();
        }

        private void gridEX1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {
            gxCommand1_OnOK(sender, e);
        }

        private void gridEX1_SelectionChanged(object sender, EventArgs e)
        {
            dataReturn = (gridEX1.CurrentRow.DataRow as DataRowView).Row;
        }

        //private void LoadData()
        //{
        //    try
        //    {
        //        DataTable tbl = Memory.Instance.Provider.GetData("SELECT MaGiaoDan, HoTen, TenThanh, NgaySinh, GiaoHo.TenGiaoHo FROM GiaoDan LEFT JOIN GiaoHo ON GiaoDan.MaGiaoHo = GiaoHo.MaGiaoHo");
        //        if (tbl != null)
        //        {
        //            gridEX1.DataSource = tbl;
        //        }
        //    }
        //    catch { }
        //}

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}