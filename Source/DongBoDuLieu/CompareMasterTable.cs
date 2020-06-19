using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public abstract class CompareMasterTable : Compare
    {
        private string khoaChinh;
        private int newIDMayKhach;
        private string khoaNgoai = null;
        private string tableNameKhoaNgoai = null;
        private string khoaNgoai2 = null;
        private string tableNameKhoaNgoai2 = null;
        public CompareMasterTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public abstract void ExProcessData();
       
        public override void ProcessData()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataRow dongboRow = findRowDongBo(Convert.ToInt32(item[khoaChinh].ToString()), Tbl.TableName);
                    if (dongboRow != null)
                    {
                        int IDClient = Convert.ToInt32(dongboRow[DongBoConst.MaIDMayKhach].ToString());
                        DataRow[] row = Tbl.Select(string.Format("{0} = {1}", khoaChinh, IDClient));
                        if (row != null && row.Length > 0)
                        {
                            if (Convert.ToInt32(item["DeleteSV"].ToString()) == 1)
                            {
                                row[0].Delete();
                                dongboRow["DaXoa"] = 1;
                            }
                            else
                            {
                                if (khoaNgoai != null)
                                {
                                    if (!changeValueKey(item, khoaNgoai, tableNameKhoaNgoai))
                                        continue;
                                    if (khoaNgoai2 != null)
                                    {
                                        if (!changeValueKey(item, khoaNgoai2, tableNameKhoaNgoai2))
                                            continue;
                                    }
                                }
                                processDataNull(item, row[0]);
                                assignData(item, row[0], khoaChinh);
                            }
                        }
                        continue;
                    }
                    if (Convert.ToInt32(item["DeleteSV"].ToString()) == 0)
                    {
                        if (khoaNgoai != null)
                        {
                            if (!changeValueKey(item, khoaNgoai, tableNameKhoaNgoai))
                                continue;
                            if (khoaNgoai2 != null)
                            {
                                if (!changeValueKey(item, khoaNgoai2, tableNameKhoaNgoai2))
                                    continue;
                            }
                        }
                        if(NewIDMayKhach==-1)
                        {
                            assignDataAdd(Tbl, item, khoaChinh, newIDMayKhach);
                            DataSet ds = new DataSet();
                            ds.Tables.Add(Tbl);
                            Memory.UpdateDataSet(ds,false);
                            ds.Tables.Remove(Tbl);
                            newIDMayKhach = Memory.Instance.GetNextId(ChiTietHoiDoanConst.TableName, ChiTietHoiDoanConst.ID, true);
                            int idTemp = newIDMayKhach - 1;
                            insertDongBoID(idTemp, Convert.ToInt32(item[khoaChinh]), Tbl.TableName);
                            continue;
                        }    
                        assignDataAdd(Tbl, item, khoaChinh, newIDMayKhach);
                        insertDongBoID(newIDMayKhach++, Convert.ToInt32(item[khoaChinh]), Tbl.TableName);
                        continue;
                    }
                }
            }
        }
        public string KhoaChinh
        {
            get { return khoaChinh; }
            set { khoaChinh = value; }
        }
        public int NewIDMayKhach
        {
            get { return newIDMayKhach; }
            set { newIDMayKhach = value; }
        }
        public string KhoaNgoai
        {
            get { return khoaNgoai; }
            set { khoaNgoai = value; }
        }
        public string TableNameKhoaNgoai
        {
            get { return tableNameKhoaNgoai; }
            set { tableNameKhoaNgoai = value; }
        }
        public string KhoaNgoai2
        {
            get { return khoaNgoai2; }
            set { khoaNgoai2 = value; }
        }
        public string TableNameKhoaNgoai2
        {
            get { return tableNameKhoaNgoai2; }
            set { tableNameKhoaNgoai2 = value; }
        }
    }
}
