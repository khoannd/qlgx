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
        private List<Dictionary<string, object>> ListGiaoHoTracks;
        public void getListGiaoHoTracks(List<Dictionary<string, object>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public override void importCacObject()
        {
                foreach (var item in Data)
                {
                    DataTable giaoDan = findGiaoDan(item);
                    //delete giaoDan
                    if (deleteObjectMaster(item, giaoDan))
                    {
                        continue;
                    }
                    item["MaGiaoHo"] = findIdObjectClient(ListGiaoHoTracks, item["MaGiaoHo"]).ToString();
                    importObjectMaster(item,giaoDan,"MaGiaoDan",GiaoDanConst.TableName);           
                }
        }
        private DataTable findGiaoDan(Dictionary<string, object> objectCSV)
        {
            //Tim Ma Nhan dang
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", GiaoDanConst.TableName);
                tbl = Memory.GetData(query, objectCSV["MaNhanDang"]);
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl!=null&&tbl.Rows.Count>0)
            {
                return tbl;
            }

            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE HoTen=? AND TenThanh=? AND NgaySinh=?", GiaoDanConst.TableName);
                tbl = Memory.GetData(query, objectCSV["HoTen"], objectCSV["TenThanh"], objectCSV["NgaySinh"]);
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl!=null&&tbl.Rows.Count>0)
            {
                return tbl;
            }
            return null;
        }

  
        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (objectCSV["DeleteSV"].ToString() == "1" && item != null && item.Rows.Count > 0)
            {
                if (compareDate(objectCSV["UpdateDate"], item.Rows[0][GiaoDanConst.UpdateDate].ToString()))
                {
                    //delete giaodan
                    delete(@"MaGiaoDan = ?",GiaoDanConst.TableName,item.Rows[0][GiaoDanConst.MaGiaoDan]);
                    //delete ThanhVienGiaDinh
                    delete(@"MaGiaoDan = ?", ThanhVienGiaDinhConst.TableName, item.Rows[0][GiaoDanConst.MaGiaoDan]);
                    //delete BiTichChiTiet
                    delete(@"MaGiaoDan = ?", BiTichChiTietConst.TableName, item.Rows[0][GiaoDanConst.MaGiaoDan]);
                    //delete ChuyenXu
                    delete(@"MaGiaoDan = ?", ChuyenXuConst.TableName, item.Rows[0][GiaoDanConst.MaGiaoDan]);
                    //delete RaoHonPhoi
                    delete(@"MaGiaoDan = ?", RaoHonPhoiConst.TableName, item.Rows[0][GiaoDanConst.MaGiaoDan]);
                    //delete ChiTietLopGiaoLy
                    delete(@"MaGiaoDan = ?", ChiTietLopGiaoLyConst.TableName, item.Rows[0][GiaoDanConst.MaGiaoDan]);
                }

                return true;
            }

            if (objectCSV["DeleteSV"].ToString() == "1")
            {
                return true;
            }
            return false;
        }
    }
}
