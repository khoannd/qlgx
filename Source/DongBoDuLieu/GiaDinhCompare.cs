using GxGlobal;
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
        private List<Dictionary<string, string>> ListGiaoHoTracks;
        private List<Dictionary<string, string>> ListGiaoDanTracks;
        private List<Dictionary<string, string>> ListThanhVienGiaDinhCSV;
        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable giaDinh = findGiaDinh(item);
                    item["MaGiaoHo"] = findIdObjectClient(ListGiaoHoTracks, item["MaGiaoHo"]).ToString();
                    importObjectMaster(item, giaDinh, "MaGiaDinh", GiaDinhConst.TableName);
                }
            }
        }
        public void getListThanhVienGiaDinhCSV(List<Dictionary<string, string>> thanhVienGiaDinhCSV)
        {
            ListThanhVienGiaDinhCSV = thanhVienGiaDinhCSV;
        }
        public void getListGiaoHoTracks(List<Dictionary<string, string>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public void getListGiaoDanTracks(List<Dictionary<string, string>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        private DataTable findGiaDinh(Dictionary<string, string> objectCSV)
        {
            //Tim Ma Nhan dang
            DataTable tbl = null;
           
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", GiaDinhConst.TableName);
                tbl = Memory.GetData(query, objectCSV["MaNhanDang"]);
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
                        List<Dictionary<string, string>> thanhVienGiaDinhCSV = getListByID(ListThanhVienGiaDinhCSV, "MaGiaDinh", objectCSV["MaGiaDinh"]);
                        DataTable thanhVienGiaDinhDB = Memory.GetData("Select * from ThanhVienGiaDinh where MaGiaDinh=?", giaDinhDB["MaGiaDinh"]);
                        //Xu ly ma giao dan thanh vien gia dinh

                        foreach (var thanhVienCSV in thanhVienGiaDinhCSV)
                        {
                            int idThanhVienDB = findIdObjectClient(ListGiaoDanTracks, thanhVienCSV["MaGiaoDan"]);
                            foreach (DataRow thanhVienDB in thanhVienGiaDinhDB.Rows)
                            {
                                if (int.Parse(thanhVienDB["MaGiaoDan"].ToString()) == idThanhVienDB && compareString(thanhVienDB["VaiTro"].ToString(), thanhVienCSV["VaiTro"]))
                                {
                                    DataTable rs=new DataTable();
                                    rs.Rows.Add(giaDinhDB);
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

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(GiaDinhConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[GiaDinhConst.MaGiaDinh].ToString());
                    if (idCSV == 0)
                    {
                        //Xoa gia dinh
                        //Xoa Gia Dinh
                        delete(@"MaGiaDinh=?", GiaDinhConst.TableName, item[GiaDinhConst.MaGiaDinh]);
                        //Xoa Thanh Vien Gia Dinh
                        delete(@"MaGiaDinh=?", ThanhVienGiaDinhConst.TableName, item[GiaDinhConst.MaGiaDinh]);
                    }

                }

            }
        }
    }
}
