using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public abstract class CompareRelationTable : Compare
    {
        private string khoaChinh1;
        private string tableNameKhoaChinh1;
        private string khoaChinh2;
        private string tableNameKhoaChinh2;
        public CompareRelationTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public abstract void ExProcessData();
        public override void ProcessData()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    int ID1 = findValueDongBoClient(Convert.ToInt32(item[khoaChinh1]), tableNameKhoaChinh1);
                    int ID2 = findValueDongBoClient(Convert.ToInt32(item[khoaChinh2]), tableNameKhoaChinh2);
                    if (ID1 == -1 || ID2 == -1)
                        continue;
                    DataRow[] row = Tbl.Select(string.Format(string.Format("{0} = {1} and {2} = {3}", khoaChinh1, ID1, khoaChinh2, ID2)));
                    if (row != null && row.Length > 0)
                    {
                        if (Convert.ToInt32(item["DeleteSV"].ToString()) == 1)
                        {
                            row[0].Delete();
                        }
                        else
                        {
                            assignData(item, row[0], khoaChinh1, khoaChinh2);
                        }
                        continue;
                    }
                    if (Convert.ToInt32(item["DeleteSV"].ToString()) == 0)
                    {
                        assignDataAdd(Tbl, item, khoaChinh1, ID1, khoaChinh2, ID2);
                    }
                }
            }
        }
        public string KhoaChinh1
        {
            get { return khoaChinh1; }
            set { khoaChinh1 = value; }
        }
        public string TableNameKhoaChinh1
        {
            get { return tableNameKhoaChinh1; }
            set { tableNameKhoaChinh1 = value; }
        }
        public string KhoaChinh2
        {
            get { return khoaChinh2; }
            set { khoaChinh2 = value; }
        }
        public string TableNameKhoaChinh2
        {
            get { return tableNameKhoaChinh2; }
            set { tableNameKhoaChinh2 = value; }
        }

    }
}
