using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    class LopGiaoLyCompare : CompareMasterTable
    {
        public LopGiaoLyCompare(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        private List<Dictionary<string, object>> ListGiaoDanTracks;
        private List<Dictionary<string, object>> ListKhoiGiaoLyTracks;
        List<Dictionary<string, object>> chiTietLopGiaoLy;
        public void getListChiTietLopGiaoLyCSV(List<Dictionary<string, object>> ChiTietLopGiaoLyCSV)
        {
            chiTietLopGiaoLy = ChiTietLopGiaoLyCSV;
        }
        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(LopGiaoLyConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[LopGiaoLyConst.MaLop].ToString());
                    if (idCSV == 0)
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
        public void getListGiaoDanTracks(List<Dictionary<string, object>> giaoDanTracks)
        {
            ListGiaoDanTracks = giaoDanTracks;
        }
        public void getListKhoiGiaoLy(List<Dictionary<string, object>> khoiGiaoLyTracks)
        {
            ListKhoiGiaoLyTracks = khoiGiaoLyTracks;
        }

        public override void importCacObject()
        {
            if (Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    DataTable lopGiaoLy = findLopGiaoLy(item);
                    int idMaKhoi = findIdObjectClient(ListKhoiGiaoLyTracks, item["MaKhoi"]);
                    if (idMaKhoi == 0)
                    {
                        continue;
                    }
                    else
                    {
                        item["MaKhoi"] = idMaKhoi.ToString();
                    }
                    importObjectMaster(item, lopGiaoLy, "MaLop", LopGiaoLyConst.TableName);
                }
            }
        }

        private DataTable findLopGiaoLy(Dictionary<string, object> objectCSV)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE TenLop=? AND Nam=? AND MaKhoi=?", LopGiaoLyConst.TableName);
                tbl = Memory.GetData(query, objectCSV["TenLop"], objectCSV["Nam"], objectCSV["MaKhoi"]);
                if (tbl!=null&&tbl.Rows.Count>0)
                {
                    if (compareHocVien(objectCSV,tbl.Rows[0]))
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

        public bool compareHocVien(Dictionary<string, object> lopCSV, DataRow lopDB)
        {
            DataTable hocVienDB = Memory.GetData(@"Select * from LopGiaoLy Where MaLop=?",lopDB["MaLop"]);
            List<Dictionary<string, object>> hocvienCSV = getListByID(chiTietLopGiaoLy, "MaLop", lopCSV["MaLop"]);
            if (hocVienDB != null && hocVienDB.Rows.Count == 0 && hocvienCSV != null && hocvienCSV.Count == 0)
            {
                return true;
            }
            if (hocVienDB != null && hocVienDB.Rows.Count == 0 || hocvienCSV != null && hocvienCSV.Count == 0)
            {
                return false;
            }
            foreach (var hocvienInCSV in hocvienCSV)
            {
                int idHocVieDB = findIdObjectClient(ListGiaoDanTracks, hocvienInCSV["MaGiaoDan"]);
                foreach (DataRow hocvienInDB in hocVienDB.Rows)
                {
                    if (int.Parse(hocvienInDB["MaGiaoDan"].ToString())==idHocVieDB&&int.Parse(hocvienInDB["SoThuTu"].ToString())==int.Parse(hocvienInCSV["SoThuTu"].ToString()))
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }
}
