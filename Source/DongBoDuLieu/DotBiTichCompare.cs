﻿using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class DotBiTichCompare : CompareMasterTable
    {
        public DotBiTichCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1 && item != null && item.Rows.Count > 0
                                                           && compareDate(objectCSV[GxSyn.UpdateDate], item.Rows[0][GxSyn.UpdateDate].ToString()))
            {

                delete(@"MaDotBiTich=?", DotBiTichConst.TableName, item.Rows[0][DotBiTichConst.MaDotBiTich]);

                delete(@"MaDotBiTich=?", BiTichChiTietConst.TableName, item.Rows[0][BiTichChiTietConst.MaDotBiTich]);
                return true;
            }
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1)
            {
                return true;
            }
            return false;
        }

      

        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable dotbitich = findDotBiTich(item);
                    if (deleteObjectMaster(item,dotbitich))
                    {
                        continue;
                    }
                    importObjectMaster(item, dotbitich, DotBiTichConst.MaDotBiTich, DotBiTichConst.TableName);
                }
            }
        }

        private DataTable findDotBiTich(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                //MoTa,NgayBiTich,LoaiBiTich,LinhMuc
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MoTa=? AND NgayBiTich=? AND LoaiBiTich=? AND LinhMuc=?", DotBiTichConst.TableName);
                tbl = Memory.GetData(query, new object[] { objectCSV["MoTa"], objectCSV["NgayBiTich"], objectCSV["LoaiBiTich"], objectCSV["LinhMuc"] });
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl != null&&tbl.Rows.Count>0)
            {
                return tbl;
            }
            return null;
        }
    }
}
