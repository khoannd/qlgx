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
                        delete(@"MaGiaoHo=" + item[GiaoHoConst.MaGiaoHo], GiaoHoConst.TableName);
                        //Xoa Gia Dinh
                        DataTable tblGiaDinh = Memory.GetData(@"Select * from GiaDinh where MaGiaoHo=" + item[GiaoHoConst.MaGiaoHo]);
                        if (tblGiaDinh!=null&&tblGiaDinh.Rows.Count>0)
                        {
                            foreach (DataRow rowGiaDinh in tblGiaDinh.Rows)
                            {
                                //Xoa Gia Dinh
                                delete(@"MaGiaDinh=" + rowGiaDinh[GiaDinhConst.MaGiaDinh], GiaDinhConst.TableName);
                                //Xoa Thanh Vien Gia Dinh
                                delete(@"MaGiaDinh="+ rowGiaDinh[GiaDinhConst.MaGiaDinh], ThanhVienGiaDinhConst.TableName);
                            }
                        }
                        //Xoa giao dan co giao ho
                        DataTable tblGiaoDan = Memory.GetData(@"Select * from GiaoDan where MaGiaoHo=" + item[GiaoHoConst.MaGiaoHo]);
                        if (tblGiaoDan!=null&&tblGiaoDan.Rows.Count>0)
                        {
                            foreach (DataRow rowGiaoDan in tblGiaoDan.Rows)
                            {
                                //Xoa GiaoDan
                                delete(@"MaGiaoDan="+rowGiaoDan[GiaoDanConst.MaGiaoDan], GiaoDanConst.TableName);
                                //Xoa ThanhVienGiaDinh co GiaoDan
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], ThanhVienGiaDinhConst.TableName);
                                //Xoa BiTichChiTiet
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], BiTichChiTietConst.TableName);
                                //Xoa GiaoDanHonPhoi
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], GiaoDanHonPhoiConst.TableName);
                                //Xoa ChuyenXu
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], ChuyenXuConst.TableName);
                                //Xoa TanHien
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], TanHienConst.TableName);
                                //Xoa RaoHonPhoi
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], RaoHonPhoiConst.TableName);
                                //Xoa ChiTietLopGiaoLy
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], ChiTietLopGiaoLyConst.TableName);
                                //Xoa GiaoLyVien
                                delete(@"MaGiaoDan=" + rowGiaoDan[GiaoDanConst.MaGiaoDan], GiaoLyVienConst.TableName);
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

        private DataTable findGiaoHo(string maNhanDang)
        {
            DataTable tbl = null;
            try
            {
                string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE MaNhanDang='{1}'", GiaoHoConst.TableName, maNhanDang);
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
            return null;
        }
    }
}
