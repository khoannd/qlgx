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
    public partial class GxGiaDinhImportList : GxGiaDinhList
    {
        public override void FormatGrid()
        {
            if (Memory.IsDesignMode) return;

            base.FormatGrid();

            GridEXColumn col = null;
            col = new GridEXColumn(MergeData.MaGiaDinhMoi, Janus.Windows.GridEX.ColumnType.Text);
            col.Width = 60;
            col.BoundMode = ColumnBoundMode.Bound;
            col.DataMember = MergeData.MaGiaDinhMoi;
            col.Caption = "Mã gia đình mới";
            col.FilterEditType = FilterEditType.Combo;
            this.RootTable.Columns.Insert(0, col);

            col = new GridEXColumn(MergeData.KetQua, Janus.Windows.GridEX.ColumnType.Text);
            col.Width = 200;
            col.BoundMode = ColumnBoundMode.Bound;
            col.DataMember = MergeData.KetQua;
            col.Caption = "Kết quả";
            col.FilterEditType = FilterEditType.Combo;
            this.RootTable.Columns.Insert(0, col);

            SetGridColumnWidth();
        }

        protected override void SetFormData(frmGiaDinh frm)
        {
            DataRow row = (this.CurrentRow.DataRow as DataRowView).Row;
            frm.Id = (int)row[MergeData.MaGiaDinhMoi];
        }
    }
}
