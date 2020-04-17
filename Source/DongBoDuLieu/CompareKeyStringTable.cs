using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public abstract class CompareKeyStringTable : Compare
    {
        private string khoaChinh;
        public CompareKeyStringTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public abstract void ExProcessData();
        public override void ProcessData()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataRow[] row = Tbl.Select(string.Format("{0} = '{1}'", khoaChinh, item[khoaChinh]));
                    if (row != null && row.Length > 0)
                    {
                        if (Convert.ToInt32(item["DeleteSV"].ToString()) == 1)
                        {
                            row[0].Delete();
                        }
                        else
                        {
                            processDataNull(item, row[0]);
                            assignData(item, row[0], khoaChinh);
                        }
                        continue;
                    }
                    if (Convert.ToInt32(item["DeleteSV"].ToString()) == 0)
                    {
                        assignDataAdd(Tbl, item, khoaChinh, item[khoaChinh]);
                    }
                }
            }
        }
        public string KhoaChinh
        {
            get { return khoaChinh; }
            set { khoaChinh = value; }
        }
    }
}
