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

        private List<Dictionary<string, object>> ListGiaoDanTracks;


        public override void importCacObject()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable khoiGiaoLy = findKhoiGiaoLy(item);
                    if (deleteObjectMaster(item, khoiGiaoLy))
                    {
                        continue;
                    }
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
        public void getListGiaoDanTracks(List<Dictionary<string, object>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        public void getLopGiaoLyCompare(LopGiaoLyCompare lbl)
        {
            lopgiaoly = lbl;
        }


        private DataTable findKhoiGiaoLy(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT * FROM {0} WHERE TenKhoi=?", KhoiGiaoLyConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenKhoi"]);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    foreach (DataRow motKhoi in tbl.Rows)
                    {
                        if (compareLopGiaoLy(objectCSV, motKhoi))
                        {
                            DataTable temp = tbl.Clone();
                            temp.ImportRow(motKhoi);
                            return temp;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
            return null;
        }
        private bool compareLopGiaoLy(Dictionary<string, object> khoiGiaoLyCSV, DataRow khoiGiaoLyDB)
        {
            DataTable lopGiaoLyDB = Memory.GetData(@"Select * from LopGiaoLy Where MaKhoi=?", khoiGiaoLyDB["MaKhoi"]);
            List<Dictionary<string, object>> lopGiaoLyCSV = getListByID(lopgiaoly.Data, "MaKhoi", khoiGiaoLyCSV["MaKhoi"]);
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

                    if (lopgiaoly.compareHocVien(lopGiaoLyCSV[i], lopGiaoLyDB.Rows[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        

        public override bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item)
        {
            if (int.Parse(objectCSV["DeleteSV"].ToString()) == 1 && item != null && item.Rows.Count > 0
                                                            && compareDate(objectCSV[GxSyn.UpdateDate], item.Rows[0][GxSyn.UpdateDate].ToString()))
            {
                //xoa khoi
                delete(@"MaKhoi=?", KhoiGiaoLyConst.TableName, item.Rows[0][KhoiGiaoLyConst.MaKhoi]);
                DataTable tblLopGiaoLy = Memory.GetData("Select * from LopGiaoLy Where MaKhoi=?", item.Rows[0][KhoiGiaoLyConst.MaKhoi]);
                if (tblLopGiaoLy != null && tblLopGiaoLy.Rows.Count > 0)
                {
                    foreach (DataRow rowLopGiaoLy in tblLopGiaoLy.Rows)
                    {
                        //xoa lop
                        delete(@"MaLop=?", LopGiaoLyConst.TableName, rowLopGiaoLy[LopGiaoLyConst.MaLop]);
                        //xoa giao ly vien
                        delete(@"MaLop=?", GiaoLyVienConst.TableName, rowLopGiaoLy[GiaoLyVienConst.MaLop]);
                        //Xoa chi tiet lop giao ly
                        delete(@"MaLop=?", ChiTietLopGiaoLyConst.TableName, rowLopGiaoLy[ChiTietLopGiaoLyConst.MaLop]);
                    }
                }
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
