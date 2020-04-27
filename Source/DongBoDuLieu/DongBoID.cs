using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GxGlobal;

namespace DongBoDuLieu
{
    public class DongBoID
    {
        private List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
        private DataTable tblDongBo;
        public List<Dictionary<string, object>> Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
        public DongBoID(string dir,string nameCSV, DataTable tblDongBo)
        {
            this.tblDongBo = tblDongBo;
            ReadFileCSV readFile = new ReadFileCSV(dir + nameCSV);
            this.Data = readFile.Data;
        }
        public void import()
        {
            if(Data!=null && Data.Count>0)
            {
                foreach (var item in Data)
                {
                    DataRow newRow = tblDongBo.NewRow();
                    AssignData(newRow, item);
                    tblDongBo.Rows.Add(newRow);
                }
            }
        }
        public void deleteRow()
        {
            DataRow[] rs = tblDongBo.Select("DaXoa=1");
            foreach (var item in rs)
            {
                item.Delete();
            }
            tblDongBo.Columns.Remove("DaXoa");
        }
        public void AssignData(DataRow newRow,Dictionary<string,object> item)
        {
            newRow[DongBoConst.TenBang] = item[DongBoConst.TenBang];
            newRow[DongBoConst.MaIDMayChu] = item[DongBoConst.MaIDMayChu];
            newRow[DongBoConst.MaIDMayKhach] = item[DongBoConst.MaIDMayKhach];
            newRow[DongBoConst.UpdateDate] = item[DongBoConst.UpdateDate];
        }
    }
}
