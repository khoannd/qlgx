using System;
using System.Collections.Generic;
using System.Text;
using GXGlobal;
using System.Data;

namespace GiaoXu
{
    public class TreeObject
    {
        public event EventHandler OnLoadGiaoHoFinished;
        public event EventHandler OnLoadGiaDinhFinished;
        public event EventHandler OnLoadGiaoDanFinished;

        public void LoadGiaoHo()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_GIAOHO);
            if (!Memory.Instance.ShowError() && tbl != null)
            {
                if (OnLoadGiaoHoFinished != null) OnLoadGiaoHoFinished(tbl, EventArgs.Empty);
            }
        }

        public void LoadGiaDinh(int maGiaoHo)
        {
            DataTable tbl = Memory.GetData(string.Format(SqlConstants.SELECT_GIADINH_LIST, maGiaoHo));
            if (!Memory.Instance.ShowError() && tbl != null)
            {
                if (OnLoadGiaDinhFinished != null) OnLoadGiaDinhFinished(tbl, EventArgs.Empty);
            }
        }

        public void LoadGiaoDan(int maGiaDinh)
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_GIAODAN_THEO_HONPHOI, new object[] { maGiaDinh });
            if (!Memory.Instance.ShowError() && tbl != null)
            {
                if (OnLoadGiaoDanFinished != null) OnLoadGiaoDanFinished(tbl, EventArgs.Empty);
            }
        }
    }
}
