using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GxGlobal;

namespace DongBoDuLieu
{
    class GiaoHoCompare : CompareMasterTable
    {
        public GiaoHoCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public override void importCacObject()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable giaoHo = findGiaoHo(item);
                    if (deleteObjectMaster(item["DeleteSV"], giaoHo))
                        continue;
                    importObjectMaster(item, giaoHo, GiaoHoConst.MaGiaoHo, GiaoHoConst.TableName);
                }
            }
            processMaGiaoHoCha();
        }
        private void processMaGiaoHoCha()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    if (item["DeleteSV"].ToString()=="1")
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(item["MaGiaoHoCha"].ToString()))
                    {
                        int idGiaoHoCha = findIdObjectClient(ListTracks, item["MaGiaoHoCha"]);
                        if (idGiaoHoCha != 0)
                        {
                            int idGiaoHo = findIdObjectClient(ListTracks, item["MaGiaoHo"]);
                            Memory.ExecuteSqlCommand(@"Update GiaoHo Set MaGiaoHoCha=? Where MaGiaoHo=?", idGiaoHoCha, idGiaoHo);
                            Memory.ClearError();
                        }
                    }
                }
            }
        }
        private DataTable findGiaoHo(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                if (!string.IsNullOrEmpty(objectCSV["MaNhanDang"].ToString()))
                {
                    string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", GiaoHoConst.TableName);
                    tbl = Memory.GetData(query, objectCSV["MaNhanDang"]);
                }

            }
            catch (System.Exception ex)
            {
                tbl = null;
            }
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return tbl;
            }
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE TenGiaoHo=?", GiaoHoConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenGiaoHo"]);
            }
            catch (Exception)
            {

                return null;
            }
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return tbl;
            }

            return null;
        }

        public override bool deleteObjectMaster(object CSVDelete, DataTable item)
        {
            if (int.Parse(CSVDelete.ToString()) == 1 && item != null && item.Rows.Count > 0)
            {
                //Xoa Giao Ho
                delete(@"MaGiaoHo=?", GiaoHoConst.TableName, item.Rows[0][GiaoHoConst.MaGiaoHo]);
                //Xoa Gia Dinh
                DataTable tblGiaDinh = Memory.GetData(@"Select * from GiaDinh where MaGiaoHo=?", item.Rows[0][GiaoHoConst.MaGiaoHo]);
                if (tblGiaDinh != null && tblGiaDinh.Rows.Count > 0)
                {
                    foreach (DataRow rowGiaDinh in tblGiaDinh.Rows)
                    {
                        //Xoa Gia Dinh
                        delete(@"MaGiaDinh=?", GiaDinhConst.TableName, rowGiaDinh[GiaDinhConst.MaGiaDinh]);
                        //Xoa Thanh Vien Gia Dinh
                        delete(@"MaGiaDinh=?", ThanhVienGiaDinhConst.TableName, rowGiaDinh[GiaDinhConst.MaGiaDinh]);
                    }
                }
                //Xoa giao dan co giao ho
                DataTable tblGiaoDan = Memory.GetData(@"Select * from GiaoDan where MaGiaoHo = ?", item.Rows[0][GiaoHoConst.MaGiaoHo]);
                if (tblGiaoDan != null && tblGiaoDan.Rows.Count > 0)
                {
                    foreach (DataRow rowGiaoDan in tblGiaoDan.Rows)
                    {
                        //Xoa GiaoDan
                        delete(@"MaGiaoDan=?", GiaoDanConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa ThanhVienGiaDinh co GiaoDan
                        delete(@"MaGiaoDan=?", ThanhVienGiaDinhConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa BiTichChiTiet
                        delete(@"MaGiaoDan=?", BiTichChiTietConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa GiaoDanHonPhoi
                        delete(@"MaGiaoDan=?", GiaoDanHonPhoiConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa ChuyenXu
                        delete(@"MaGiaoDan=?", ChuyenXuConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa TanHien
                        delete(@"MaGiaoDan=?", TanHienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa RaoHonPhoi
                        delete(@"MaGiaoDan1=? OR MaGiaoDan2=?", RaoHonPhoiConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan], rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa ChiTietLopGiaoLy
                        delete(@"MaGiaoDan=?", ChiTietLopGiaoLyConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                        //Xoa GiaoLyVien
                        delete(@"MaGiaoDan=?", GiaoLyVienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
