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

        public override void deleteObjectMaster()
        {
            DataTable rsDB = getAll(GiaoHoConst.TableName);
            if (rsDB != null && rsDB.Rows.Count > 0)
            {
                foreach (DataRow item in rsDB.Rows)
                {
                    int idCSV = findIdObjectCSV(ListTracks, item[GiaoHoConst.MaGiaoHo].ToString());
                    if (idCSV == 0)
                    {
                        //Xoa Giao Ho
                        delete(@"MaGiaoHo=?", GiaoHoConst.TableName, item[GiaoHoConst.MaGiaoHo]);
                        //Xoa Gia Dinh
                        DataTable tblGiaDinh = Memory.GetData(@"Select * from GiaDinh where MaGiaoHo=?", item[GiaoHoConst.MaGiaoHo]);
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
                        DataTable tblGiaoDan = Memory.GetData(@"Select * from GiaoDan where MaGiaoHo = ?", item[GiaoHoConst.MaGiaoHo]);
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
                                delete(@"MaGiaoDan=?" , TanHienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                                //Xoa RaoHonPhoi
                                delete(@"MaGiaoDan1=? OR MaGiaoDan2=?", RaoHonPhoiConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan], rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                                //Xoa ChiTietLopGiaoLy
                                delete(@"MaGiaoDan=?" , ChiTietLopGiaoLyConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                                //Xoa GiaoLyVien
                                delete(@"MaGiaoDan=?", GiaoLyVienConst.TableName, rowGiaoDan[GiaoDanConst.MaGiaoDan]);
                            }
                        }

                    }
                }
            }
        }

        public override void importCacObject()
        {
            foreach (var item in Data)
            {
                DataTable giaoHo = findGiaoHo(item["MaNhanDang"]);
                importObjectMaster(item, giaoHo, GiaoHoConst.MaGiaoHo, GiaoHoConst.TableName);
            }
        }

        private DataTable findGiaoHo(object maNhanDang)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang=?", GiaoHoConst.TableName);
                tbl = Memory.GetData(query, maNhanDang);
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
    }
}
