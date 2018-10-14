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
        public override void importCacObject()
        {
            if (this.Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable giaDinh = findGiaDinh(item);
                    item["MaGiaoHo"] = findIdObjectClient(ListGiaoHoTracks,item["MaGiaoHo"]).ToString();
                    importObjectMaster(item, giaDinh, "MaGiaDinh", GiaDinhConst.TableName);
                }
            }
        }
        public void getListGiaoHoTracks(List<Dictionary<string, string>> giaoHoTracks)
        {
            ListGiaoHoTracks = giaoHoTracks;
        }
        public void getListGiaoDanTracks(List<Dictionary<string, string>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        private DataTable findGiaDinh(Dictionary<string,string>objectCSV)
        {
            //Tim Ma Nhan dang
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang='{1}'", GiaDinhConst.TableName, objectCSV["MaNhanDang"]);
                tbl = Memory.GetData(query);
            }
            catch (System.Exception ex)
            {
                tbl = null;
            }
            if (tbl!=null&&tbl.Rows.Count>0)
            {
                return tbl;
            }
            //Ten gia dinh, dia chi, sdt , ghi chu
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE TenGiaDinh='{1}',DiaChi='{2}',DienThoai='{3}',GhiChu='{4}'", GiaDinhConst.TableName, objectCSV["TenGiaDinh"], objectCSV["DiaChi"], objectCSV["DienThoai"], objectCSV["GhiChu"]);
                tbl = Memory.GetData(query);
            }
            catch (System.Exception ex)
            {
                tbl = null;
            }
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return tbl;
            }
            return null;
        }

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(GiaDinhConst.TableName);
            if (rsDB!=null&&rsDB.Rows.Count>0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[GiaDinhConst.MaGiaDinh].ToString());
                    if (idCSV==0)
                    {
                        //Xoa gia dinh
                        //Xoa Gia Dinh
                        delete(@"MaGiaDinh=" + item[GiaDinhConst.MaGiaDinh], GiaDinhConst.TableName);
                        //Xoa Thanh Vien Gia Dinh
                        delete(@"MaGiaDinh=" + item[GiaDinhConst.MaGiaDinh], ThanhVienGiaDinhConst.TableName);
                    }

                }

            }
        }
    }
}
