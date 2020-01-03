﻿using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class GiaDinhCompare : CompareMasterTable
    {
        public GiaDinhCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        private List<Dictionary<string, object>> ListGiaoHoTracks;
        private List<Dictionary<string, object>> ListGiaoDanTracks;
        private List<Dictionary<string, object>> ListThanhVienGiaDinhCSV;
        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable giaDinh = findGiaDinh(item);
                    if (deleteObjectMaster(item, giaDinh))
                        continue;
                    item["MaGiaoHo"] = findIdObjectClient(ListGiaoHoTracks, item["MaGiaoHo"]).ToString();
                    importObjectMaster(item, giaDinh, "MaGiaDinh", GiaDinhConst.TableName);
                }
            }
        }
        public void getListThanhVienGiaDinhCSV(List<Dictionary<string, object>> thanhVienGiaDinhCSV)
        {
            ListThanhVienGiaDinhCSV = thanhVienGiaDinhCSV;
        }
        public void getListGiaoHoTracks(List<Dictionary<string, object>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public void getListGiaoDanTracks(List<Dictionary<string, object>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        private DataTable findGiaDinh(Dictionary<string, object> objectCSV)
        {
            //Tim Ma Nhan dang
            DataTable tbl = null;

            try
            {
                if (!string.IsNullOrEmpty(objectCSV["MaNhanDang"].ToString()))
                {
                    string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", GiaDinhConst.TableName);
                    tbl = Memory.GetData(query, objectCSV["MaNhanDang"]);
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return tbl;
            }
            //Ten gia dinh, dia chi, sdt , ghi chu
            try
            {
                string query = string.Format(@"SELECT * FROM {0} WHERE TenGiaDinh=? AND DiaChi=? AND DienThoai=? AND GhiChu=?", GiaDinhConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenGiaDinh"], objectCSV["DiaChi"], objectCSV["DienThoai"], objectCSV["GhiChu"]);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow giaDinhDB in tbl.Rows)
                    {
                        //Check ton tai 1 thanh vien gia dinh
                        List<Dictionary<string, object>> thanhVienGiaDinhCSV = getListByID(ListThanhVienGiaDinhCSV, "MaGiaDinh", objectCSV["MaGiaDinh"]);
                        DataTable thanhVienGiaDinhDB = Memory.GetData("Select * from ThanhVienGiaDinh where MaGiaDinh=?", giaDinhDB["MaGiaDinh"]);
                        //Xu ly ma giao dan thanh vien gia dinh

                        foreach (var thanhVienCSV in thanhVienGiaDinhCSV)
                        {
                            int idThanhVienDB = findIdObjectClient(ListGiaoDanTracks, thanhVienCSV["MaGiaoDan"]);
                            foreach (DataRow thanhVienDB in thanhVienGiaDinhDB.Rows)
                            {
                                if (int.Parse(thanhVienDB["MaGiaoDan"].ToString()) == idThanhVienDB && compareString(thanhVienDB["VaiTro"].ToString(), thanhVienCSV["VaiTro"]))
                                {
                                    DataTable rs = new DataTable();
                                    rs.Rows.Add(giaDinhDB);
                                    return rs;
                                }
                            }
                        }
                        return null;
                    }

                }

            }
            catch (System.Exception ex)
            {
                return null;
            }

            return null;
        }

        public override bool deleteObjectMaster(Dictionary<string,object> objectCSV, DataTable item)
        {
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1 && item != null && item.Rows.Count > 0 
                                                            &&compareDate(objectCSV[GxSyn.UpdateDate],item.Rows[0][GxSyn.UpdateDate].ToString()))
            {
                //Xoa Gia Dinh
                delete(@"MaGiaDinh=?", GiaDinhConst.TableName, item.Rows[0][GiaDinhConst.MaGiaDinh]);
                //Xoa Thanh Vien Gia Dinh
                delete(@"MaGiaDinh=?", ThanhVienGiaDinhConst.TableName, item.Rows[0][GiaDinhConst.MaGiaDinh]);
                return true;
            }
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1)
            {
                return true;
            }
            return false;

        }
    }
}