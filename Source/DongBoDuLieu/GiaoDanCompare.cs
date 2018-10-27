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

        //public override void deleteObjectMaster()
        //{
        //    DataTable rsDB = getAll(GiaoDanConst.TableName);
        //    if (rsDB != null && rsDB.Rows.Count > 0)
        //    {
        //        foreach (DataRow rowGiaoDan in rsDB.Rows)
        //        {
        //            int idCSV = findIdObjectCSV(ListTracks, rowGiaoDan[GiaoDanConst.MaGiaoDan].ToString());
        //            if (idCSV == 0)
        //            {
        //                //Xoa GiaoDan
        //                delete(@"MaGiaoDan=?", GiaoDanConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa ThanhVienGiaDinh co GiaoDan
        //                delete(@"MaGiaoDan=?", ThanhVienGiaDinhConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa BiTichChiTiet
        //                delete(@"MaGiaoDan=?", BiTichChiTietConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa GiaoDanHonPhoi
        //                delete(@"MaGiaoDan=?", GiaoDanHonPhoiConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa ChuyenXu
        //                delete(@"MaGiaoDan=?", ChuyenXuConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa TanHien
        //                delete(@"MaGiaoDan=?", TanHienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa RaoHonPhoi
        //                delete(@"MaGiaoDan1=? OR MaGiaoDan2=?", RaoHonPhoiConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan], rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa ChiTietLopGiaoLy
        //                delete(@"MaGiaoDan=?", ChiTietLopGiaoLyConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //                //Xoa GiaoLyVien
        //                delete(@"MaGiaoDan=?", GiaoLyVienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
        //            }

        //        }

        //    }
        //}
        public void getListGiaoHoTracks(List<Dictionary<string, object>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public override void importCacObject()
        {
            if (this.Data.Count>0)
            {
                foreach (var item in Data)
                {
                    DataTable giaoDan = findGiaoDan(item);
                    item["MaGiaoHo"] = findIdObjectClient(ListGiaoHoTracks, item["MaGiaoHo"]).ToString();
                    importObjectMaster(item,giaoDan,"MaGiaoDan",GiaoDanConst.TableName);
                   
                }
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
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE HoTen=?,TenThanh=?,NgaySinh=?", GiaoDanConst.TableName);
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
            throw new NotImplementedException();
        }
    }
}
