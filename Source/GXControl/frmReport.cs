using GxGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class frmReport : frmBase
    {
        public DataRow RowInfor
        {
            get;set;
        }
        public TypeExport EType
        {
            get;set;
        }
        public frmReport()
        {
            InitializeComponent();
            gxCommand1.OnOK += GxCommand1_OnOK;
            this.AcceptButton = gxCommand1.OKButton;
            gxCommand1.OKButton.Text = "Xuất phiếu giới thiệu";
            cbLinhMuc.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            DataObj = new DataSet();
        }
        

        private void GxCommand1_OnOK(object sender, EventArgs e)
        {
            DataTable tblGiaoXuNhan = new DataTable();
            SetForGiaoXuNhan(tblGiaoXuNhan);
            DataTable tblGiaoXu = new DataTable();
            SetForGiaoXu(tblGiaoXu);
            DataTable tblGiaoDan = new DataTable();
            if(EType == TypeExport.GioiThieuRuaToi)
            {
                SetGiaoDanForRuaToi(tblGiaoDan);
                ReportGioiThieuRuaToi rp = new ReportGioiThieuRuaToi();
                rp.TenLinhMuc = cbLinhMuc.Combo.SelectedItem != null ? cbLinhMuc.combo.SelectedItem.ToString() : "";
                rp.FileName = GxConstants.REPORT_GIOITHIEU_RUATOI_FILENAME; 
                rp.Export(dataObj);
            }
            else if(EType == TypeExport.GioiThieuGiaoLyPhonPhoi)
            {
                SetGiaoDanForHonPhoi(tblGiaoDan);   
                RpGThieuGlyHPhoi rp = new RpGThieuGlyHPhoi();
                rp.TenLinhMuc = cbLinhMuc.Combo.SelectedItem != null ? cbLinhMuc.combo.SelectedItem.ToString() : "";
                rp.FileName = GxConstants.REPORT_GIOITHIEU_GIAOLYHONPHOI_FILENAME;
                rp.Export(dataObj);
            }
            else if(EType == TypeExport.GioiThieuThemSuc)
            {
                SetGiaoDanForThemSuc(tblGiaoDan);
                RpGioiThieuThemSuc rp = new RpGioiThieuThemSuc();
                rp.TenLinhMuc = cbLinhMuc.Combo.SelectedItem != null ? cbLinhMuc.combo.SelectedItem.ToString() : "";
                rp.FileName = GxConstants.REPORT_GIOITHIEU_THEMSUC_FILENAME;
                rp.Export(dataObj);
            } 
            else if(EType == TypeExport.GioiThieuChuyenXu)
            {
                SetGiaoDanForChuyenXu(tblGiaoDan);
                RpGioiThieuChuyenXu rp = new RpGioiThieuChuyenXu();
                rp.TenLinhMuc = cbLinhMuc.Combo.SelectedItem != null ? cbLinhMuc.combo.SelectedItem.ToString() : "";
                rp.FileName = GxConstants.REPORT_GIOITHIEU_CHUYENXU_FILENAME;
                rp.Export(dataObj);
            }
        }
        public void SetGiaoDanForChuyenXu(DataTable tblGiaoDan)
        {
          
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTen);
            tblGiaoDan.Columns.Add(GiaoDanConst.TenThanh);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgaySinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiSinh);
            tblGiaoDan.Columns.Add(ThanhVienGiaDinhConst.VaiTro);
            tblGiaoDan.Columns.Add(ThanhVienGiaDinhConst.ChuHo);

            tblGiaoDan = Memory.GetData(SqlConstants.SELECT_CHITIET_GIADINH, RowInfor[GiaDinhConst.MaGiaDinh]);
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            if (dataObj.Tables.Contains(tblGiaoDan.TableName)) dataObj.Tables.Remove(GiaoDanConst.TableName);
           
            DataObj.Tables.Add(tblGiaoDan);
        }

        public void SetGiaoDanForRuaToi(DataTable tblGiaoDan)
        {
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTen);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgaySinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiSinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenCha);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenMe);
            var rowGiaoDan = tblGiaoDan.NewRow();

            rowGiaoDan[GiaoDanConst.HoTen] = RowInfor[GiaoDanConst.HoTen];
            rowGiaoDan[GiaoDanConst.NgaySinh] = RowInfor[GiaoDanConst.NgaySinh];
            rowGiaoDan[GiaoDanConst.NoiSinh] = RowInfor[GiaoDanConst.NoiSinh];
            rowGiaoDan[GiaoDanConst.HoTenCha] = RowInfor[GiaoDanConst.HoTenCha];
            rowGiaoDan[GiaoDanConst.HoTenMe] = RowInfor[GiaoDanConst.HoTenMe];
            tblGiaoDan.Rows.Add(rowGiaoDan);
            if (dataObj.Tables.Contains(tblGiaoDan.TableName)) dataObj.Tables.Remove(GiaoDanConst.TableName);
            DataObj.Tables.Add(tblGiaoDan);
        }
        public void SetGiaoDanForHonPhoi(DataTable tblGiaoDan)
        {
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTen);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgaySinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiSinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenCha);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenMe);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgayThemSuc);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiThemSuc);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiRuaToi);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgayRuaToi);
            var rowGiaoDan = tblGiaoDan.NewRow();

            rowGiaoDan[GiaoDanConst.HoTen] = string.Concat(RowInfor[GiaoDanConst.TenThanh]," ",RowInfor[GiaoDanConst.HoTen]);
            rowGiaoDan[GiaoDanConst.NgaySinh] = RowInfor[GiaoDanConst.NgaySinh];
            rowGiaoDan[GiaoDanConst.NoiSinh] = RowInfor[GiaoDanConst.NoiSinh];
            rowGiaoDan[GiaoDanConst.HoTenCha] = RowInfor[GiaoDanConst.HoTenCha];
            rowGiaoDan[GiaoDanConst.HoTenMe] = RowInfor[GiaoDanConst.HoTenMe];
            rowGiaoDan[GiaoDanConst.NoiThemSuc] = RowInfor[GiaoDanConst.NoiThemSuc];
            rowGiaoDan[GiaoDanConst.NgayThemSuc] = RowInfor[GiaoDanConst.NgayThemSuc];
            rowGiaoDan[GiaoDanConst.NoiRuaToi] = RowInfor[GiaoDanConst.NoiRuaToi];
            rowGiaoDan[GiaoDanConst.NgayRuaToi] = RowInfor[GiaoDanConst.NgayRuaToi];
            tblGiaoDan.Rows.Add(rowGiaoDan);
            if (dataObj.Tables.Contains(tblGiaoDan.TableName)) dataObj.Tables.Remove(GiaoDanConst.TableName);
            DataObj.Tables.Add(tblGiaoDan);
        }
        public void SetGiaoDanForThemSuc(DataTable tblGiaoDan)
        {
            tblGiaoDan.TableName = GiaoDanConst.TableName;
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTen);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgaySinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiSinh);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenCha);
            tblGiaoDan.Columns.Add(GiaoDanConst.HoTenMe);
            tblGiaoDan.Columns.Add(GiaoDanConst.NoiRuaToi);
            tblGiaoDan.Columns.Add(GiaoDanConst.NgayRuaToi);
            var rowGiaoDan = tblGiaoDan.NewRow();

            rowGiaoDan[GiaoDanConst.HoTen] = string.Concat(RowInfor[GiaoDanConst.TenThanh], " ", RowInfor[GiaoDanConst.HoTen]);
            rowGiaoDan[GiaoDanConst.NgaySinh] = RowInfor[GiaoDanConst.NgaySinh];
            rowGiaoDan[GiaoDanConst.NoiSinh] = RowInfor[GiaoDanConst.NoiSinh];
            rowGiaoDan[GiaoDanConst.HoTenCha] = RowInfor[GiaoDanConst.HoTenCha];
            rowGiaoDan[GiaoDanConst.HoTenMe] = RowInfor[GiaoDanConst.HoTenMe];
            rowGiaoDan[GiaoDanConst.NoiRuaToi] = RowInfor[GiaoDanConst.NoiRuaToi];
            rowGiaoDan[GiaoDanConst.NgayRuaToi] = RowInfor[GiaoDanConst.NgayRuaToi];
            tblGiaoDan.Rows.Add(rowGiaoDan);
            if (dataObj.Tables.Contains(tblGiaoDan.TableName)) dataObj.Tables.Remove(GiaoDanConst.TableName);
            DataObj.Tables.Add(tblGiaoDan);
        }
        public void SetForGiaoXuNhan(DataTable tblGiaoXuNhan)
        {
            tblGiaoXuNhan.TableName = GiaoXuNhanConst.TableName;
            tblGiaoXuNhan.Columns.Add(GiaoXuNhanConst.TenGiaoPhan2);
            tblGiaoXuNhan.Columns.Add(GiaoXuNhanConst.TenGiaoXu2);
            DataRow row = tblGiaoXuNhan.NewRow();
            row[GiaoXuNhanConst.TenGiaoPhan2] = txtGiaoPhan.Text;
            row[GiaoXuNhanConst.TenGiaoXu2] = txtGiaoXu.Text;
            tblGiaoXuNhan.Rows.Add(row);
            if (dataObj.Tables.Contains(tblGiaoXuNhan.TableName)) dataObj.Tables.Remove(GiaoXuNhanConst.TableName);
            DataObj.Tables.Add(tblGiaoXuNhan);
        }
        public void SetForGiaoXu(DataTable tblGiaoXu)
        {
            tblGiaoXu.TableName = GiaoXuConst.TableName;
            tblGiaoXu.Columns.Add(GiaoPhanConst.TenGiaoPhan);
            tblGiaoXu.Columns.Add(GiaoXuConst.TenGiaoXu);
            tblGiaoXu.Columns.Add(GiaoHatConst.TenGiaoHat);
            var rowGiaoXu = tblGiaoXu.NewRow();
            rowGiaoXu[GiaoPhanConst.TenGiaoPhan] = Memory.GiaoXuInfo[GiaoPhanConst.TenGiaoPhan];
            rowGiaoXu[GiaoHatConst.TenGiaoHat] = Memory.GiaoXuInfo[GiaoHatConst.TenGiaoHat];
            rowGiaoXu[GiaoXuConst.TenGiaoXu] = Memory.GiaoXuInfo[GiaoXuConst.TenGiaoXu];
            tblGiaoXu.Rows.Add(rowGiaoXu);
            if (dataObj.Tables.Contains(tblGiaoXu.TableName)) dataObj.Tables.Remove(GiaoXuConst.TableName);
            DataObj.Tables.Add(tblGiaoXu);
        }
        private void frmGioiThieuRuaToi_Load(object sender, EventArgs e)
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST);
            foreach(DataRow row in tbl.Rows)
            {
                cbLinhMuc.Combo.Items.Add(string.Concat(row[LinhMucConst.TenThanh], " ", row[LinhMucConst.HoTen]));
            }
        }
    }
    public enum TypeExport
    {
        GioiThieuRuaToi,
        GioiThieuGiaoLyPhonPhoi,
        GioiThieuThemSuc,
        GioiThieuChuyenXu,
    }
}
