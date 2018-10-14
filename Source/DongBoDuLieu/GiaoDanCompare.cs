using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaoDanCompare : CompareMasterTable
    {
        public GiaoDanCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override void deleteObjectMaster()
        {
            throw new NotImplementedException();
        }

        public override void importCacObject()
        {
            if (this.Data.Count>0)
            {
                foreach (var item in Data)
                {
                    DataTable giaoDan = findGiaoDan();
                    //Xu ly ma giao ho
                    importObjectMaster(item,giaoDan,"MaGiaoDan",GiaoDanConst.TableName);
                   
                }
            }
        }
        private DataTable findGiaoDan()
        {
            DataTable tbl = new DataTable();
            return tbl;
        }
    }
}
