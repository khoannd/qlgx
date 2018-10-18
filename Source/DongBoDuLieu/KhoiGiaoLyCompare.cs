using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class KhoiGiaoLyCompare : CompareMasterTable
    {
        public KhoiGiaoLyCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        LopGiaoLyCompare lopgiaoly;

        private List<Dictionary<string, string>> ListGiaoDanTracks;

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(KhoiGiaoLyConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[KhoiGiaoLyConst.MaKhoi].ToString());
                    if (idCSV == 0)
                    {
                        //xoa khoi
                        delete(@"MaKhoi=?", KhoiGiaoLyConst.TableName, item[KhoiGiaoLyConst.MaKhoi]);
                        DataTable tblLopGiaoLy = Memory.GetData("Select * from LopGiaoLy Where MaKhoi=?", item[KhoiGiaoLyConst.MaKhoi]);
                        if (tblLopGiaoLy!=null&&tblLopGiaoLy.Rows.Count>0)
                        {
                            //xoa lop
                            delete(@"MaLop=?", LopGiaoLyConst.TableName, item[LopGiaoLyConst.MaLop]);
                            //xoa giao ly vien
                            delete(@"MaLop=?", GiaoLyVienConst.TableName, item[GiaoLyVienConst.MaLop]);
                            //Xoa chi tiet lop giao ly
                            delete(@"MaLop=?", ChiTietLopGiaoLyConst.TableName, item[ChiTietLopGiaoLyConst.MaLop]);

                        }
                       
                    }

                }

            }
        }

        public override void importCacObject()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable khoiGiaoLy = findKhoiGiaoLy(item);
                    int idGiaoDan = findIdObjectClient(ListGiaoDanTracks, item["NguoiQuanLy"]);
                    if (idGiaoDan == 0)
                    {
                        continue;
                    }
                    else
                    {
                        item["NguoiQuanLy"] = idGiaoDan.ToString();
                    }
                    importObjectMaster(item, khoiGiaoLy, "MaKhoi", KhoiGiaoLyConst.TableName);
                }
            }
        }
        public void getListGiaoDanTracks(List<Dictionary<string, string>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        public void getLopGiaoLyCompare(LopGiaoLyCompare lbl)
        {
            lopgiaoly = lbl;
        }


        private DataTable findKhoiGiaoLy(Dictionary<string, string> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE TenKhoi=?", KhoiGiaoLyConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenKhoi"]);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    if (compareLopGiaoLy(objectCSV, tbl))
                    {
                        return tbl;
                    }
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
            return null;
        }
        private bool compareLopGiaoLy(Dictionary<string, string> khoiGiaoLyCSV, DataTable khoiGiaoLyDB)
        {
            DataTable lopGiaoLyDB = Memory.GetData(@"Select * from LopGiaoLy Where MaKhoi=?", khoiGiaoLyDB.Rows[0]["MaKhoi"]);
            List<Dictionary<string, string>> lopGiaoLyCSV = getListByID(lopgiaoly.Data, "MaKhoi", khoiGiaoLyCSV["MaKhoi"]);
            if (lopGiaoLyDB != null && lopGiaoLyDB.Rows.Count == 0 && lopGiaoLyCSV != null && lopGiaoLyCSV.Count == 0)
            {
                return true;
            }
            if (lopGiaoLyDB != null && lopGiaoLyDB.Rows.Count == 0 || lopGiaoLyCSV != null && lopGiaoLyCSV.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < lopGiaoLyCSV.Count; i++)
            {
                for (int j = 0; j < lopGiaoLyDB.Rows.Count; j++)
                {

                    if (lopgiaoly.compareHocVien(lopGiaoLyCSV[i], lopGiaoLyDB.Rows[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
