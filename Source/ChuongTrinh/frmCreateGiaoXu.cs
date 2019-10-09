using GxControl;
using GxGlobal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmCreateGiaoXu : Form
    {
        //hiepdv begin add
        List<GiaoXu> giaoXuAll = new List<GiaoXu>();
        //hiepdv end add
        List<GiaoXu> giaoXus = new List<GiaoXu>();
        public object ListGiaoPhan { get; set; }
        public object ListGiaoHat { get; set; }
        public frmCreateGiaoXu()
        {
            InitializeComponent();
            // listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
            listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        public void Set()
        {
            SetGiaoPhan();
            Thread thread = new Thread(() =>
            {
                //SetListView();
                //SetGiaoPhan();
                //cbGiaoPhan.SelectedIndex = 0;
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void SetGiaoPhan()
        {
            var giaoPhans = GetGiaoPhans();
            ListGiaoPhan = giaoPhans;
            cbGiaoPhan.DataSource = giaoPhans;
            cbGiaoPhan.DisplayMember = "TenGiaoPhan";
            cbGiaoPhan.ValueMember = "MaGiaoPhan";
        }

        private List<GiaoPhan> GetGiaoPhans()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GxConstants.URL_GET_LIST_GIAO_PHAN);
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers["PassWord"] = "admin";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {

                }
                var rs = responds.GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<GiaoPhan>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        public void SetListView(List<GiaoXu> listGX)
        {
            listView1.Items.Clear();
            if (listGX == null) return;
            foreach (var giaoXu in listGX)
            {
                var gs = new GiaoXu() { TenGiaoXu = giaoXu.TenGiaoXu, TenGiaoHat = giaoXu.TenGiaoHat, TenGiaoPhan = giaoXu.TenGiaoPhan, DiaChi = giaoXu.DiaChi, DienThoai = giaoXu.DienThoai, Website = giaoXu.Website, MaGiaoXuRieng = giaoXu.MaGiaoXuRieng, MaGiaoHatRieng = giaoXu.Ma_GiaoHat, MaGiaoPhanRieng = giaoXu.MaGiaoPhan, Hinh = giaoXu.Hinh, Email = giaoXu.Email };
                ListViewItem item = new ListViewItem(new string[] { string.Concat("Giáo xứ:", giaoXu.TenGiaoXu), string.Concat("Thuộc giáo hạt :", giaoXu.TenGiaoHat), string.Concat("Thuộc giáo phận:", giaoXu.TenGiaoPhan), string.Concat("Địa chỉ:", giaoXu.DiaChi), string.Concat("Số điện thoại:", giaoXu.DienThoai) }, 0);
                item.Tag = gs;
                listView1.Items.Add(item);
                var img = GetImage(gs.MaGiaoXuRieng);
                if (img != null)
                {
                    imageList1.Images.Add(img);
                    item.ImageIndex = imageList1.Images.Count - 1;
                    giaoXu.ImageIndex = item.ImageIndex;
                }
                else
                {
                    item.ImageIndex = 0;
                    giaoXu.ImageIndex = 0;
                }
                Thread.Sleep(500);
            }
        }
        public Image GetImage(string MaGiaoXuRieng)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_DOWNLOAD_GIAO_XU_IMAGE, MaGiaoXuRieng));
                request.ContentType = "image/jpg";
                request.Method = "POST";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                var rs = responds.GetResponseStream();
                return Image.FromStream(rs);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void SetListViewSearch(string name, List<GiaoXu> listGX)
        {
            listView1.Items.Clear();
            if (listGX == null) return;
            foreach (var giaoXu in listGX)
            {
                if (!giaoXu.TenGiaoXu.ToLower().Contains(name)) continue;
                var gs = new GiaoXu() { TenGiaoXu = giaoXu.TenGiaoXu, TenGiaoHat = giaoXu.TenGiaoHat, TenGiaoPhan = giaoXu.TenGiaoPhan, DiaChi = giaoXu.DiaChi, DienThoai = giaoXu.DienThoai, Website = giaoXu.Website, MaGiaoXuRieng = giaoXu.MaGiaoXuRieng, MaGiaoHatRieng = giaoXu.Ma_GiaoHat, MaGiaoPhanRieng = giaoXu.MaGiaoPhan, Hinh = giaoXu.Hinh, Email = giaoXu.Email };
                ListViewItem item = new ListViewItem(new string[] { string.Concat("Giáo xứ:", giaoXu.TenGiaoXu), string.Concat("Thuộc giáo hạt :", giaoXu.TenGiaoHat), string.Concat("Thuộc giáo phận:", giaoXu.TenGiaoPhan), string.Concat("Địa chỉ:", giaoXu.DiaChi), string.Concat("Số điện thoại:", giaoXu.DienThoai) }, giaoXu.ImageIndex);
                item.Tag = gs;
                listView1.Items.Add(item);
            }
        }

        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Insert
            var selectd = listView1.SelectedItems[0];
            //var sql = SqlConstants.GiaoXu
            var giaoXuInfo = selectd.Tag as GiaoXu;
            //confirm Giáo xứ đã chọn
            DialogResult tl = MessageBox.Show(String.Format("Bạn đã chọn giáo xứ [{0}] thuộc giáo hạt [{1}] của giáo phận [{2}]. \r\nChọn [Yes] để chấp nhận.\r\nChọn [No] để chọn lại.", giaoXuInfo.TenGiaoXu, giaoXuInfo.TenGiaoHat, giaoXuInfo.TenGiaoPhan),
                "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (tl == DialogResult.Yes)
            {
                Thread thread = new Thread(() =>
                {
                    var image = GetImage(giaoXuInfo.MaGiaoXuRieng);
                    //2018-08-28 Gia modifi start
                    if (image != null)
                    {
                        image.Save(giaoXuInfo.Hinh);
                    }
                    //2018-08-28 Gia modifi end
                });
                thread.IsBackground = true;
                thread.Start();
                if(!insertInforGiaoXu(giaoXuInfo))
                {
                    MessageBox.Show("Bạn đã chọn giáo xứ thất bại!!!\r\n Vui lòng tắt chương trình và khởi động lại.\r\nHoặc liên hệ qlgx.net", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }

                /*
                var rsGiaoPhan = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_PHAN, giaoXuInfo.TenGiaoPhan, giaoXuInfo.MaGiaoPhanRieng);
                var rsGiaoHat = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_HAT, giaoXuInfo.TenGiaoHat, giaoXuInfo.MaGiaoHatRieng);
                var rsGiaoXu = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_XU, giaoXuInfo.TenGiaoXu, giaoXuInfo.DiaChi, giaoXuInfo.DienThoai, giaoXuInfo.Website, giaoXuInfo.Hinh, giaoXuInfo.MaGiaoXuRieng, giaoXuInfo.Email);
                */
                //2018-08-29 Gia add start
                //Memory.SetMaGiaoXuRiengAllTable(int.Parse(giaoXuInfo.MaGiaoXuRieng));
                //2018-08-29 Gia add end
                Memory.ShowError();
                if (!Memory.HasError())
                {
                    //this.Hide();
                }
                //hiepdv begin add
                MessageBox.Show("Bạn đã chọn xong giáo xứ!!!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                //hiepdv end add
                return;
            }
            return;
        }

        //Insert giáo phận giáo hạt và giáo xứ
        public bool insertInforGiaoXu(GiaoXu giaoXuInfo)
        {
            string MaDinhDanh = GenerateUniqueCode.GetUniqueCode();
            string TenMay = GenerateUniqueCode.GetComputerName();
            if (Memory.SendMaDinhDanhTenMayLenServer(MaDinhDanh, TenMay, giaoXuInfo.MaGiaoXuRieng))
            {
                DataTable tbl;
                //Giáo phận
                tbl = Memory.GetData(SqlConstants.SELECT_ALLGiaoPhan);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    Memory.ExecuteSqlCommand("Delete from GiaoPhan");
                }
                Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_PHAN, giaoXuInfo.TenGiaoPhan, giaoXuInfo.MaGiaoPhanRieng);
                //Giáo hạt
                tbl = Memory.GetData(SqlConstants.SELECT_ALLGiaoHat);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    Memory.ExecuteSqlCommand("Delete from GiaoHat");
                }
                Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_HAT, giaoXuInfo.TenGiaoHat, giaoXuInfo.MaGiaoHatRieng);

                //Giáo xứ
                tbl = Memory.GetData(SqlConstants.SELECT_ALLGiaoXu);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    Memory.ExecuteSqlCommand("Delete from GiaoXu");
                }
                Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_XU, giaoXuInfo.TenGiaoXu, giaoXuInfo.DiaChi, giaoXuInfo.DienThoai, giaoXuInfo.Website, giaoXuInfo.Hinh, giaoXuInfo.MaGiaoXuRieng, giaoXuInfo.Email,MaDinhDanh,TenMay);

                return true;
            }
            return false;
        }
        public List<GiaoXu> GetGiaoXus(string giaoHatId)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_LIST_GIAO_XU_THEO_GIAO_HAT, giaoHatId));
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers["PassWord"] = "admin";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                var rs = responds.GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<GiaoXu>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //hiepdv begin add
        public List<GiaoXu> GetAllGiaoXu()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_ALL_LIST_GIAO_XU));
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers["PassWord"] = "admin";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                var rs = responds.GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<GiaoXu>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //hiepdv end add

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //SetListViewSearch(txtSearch.Text);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (chkSearchAll.Checked)
            {
                SetListViewSearch(txtSearch.Text.ToLower(), giaoXuAll);
            }
            else
            {
                SetListViewSearch(txtSearch.Text.ToLower(), giaoXus);
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void cbGiaoPhan_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbGiaoPhan.SelectedValue;
            if (item is GiaoPhan)
            {
                item = (cbGiaoPhan.SelectedValue as GiaoPhan).MaGiaoPhan;
            }
            var giaoHats = GetGiaoHatsInGiaoPhan(item.ToString());
            ListGiaoHat = giaoHats;
            cbGiaoHat.DataSource = giaoHats;
            cbGiaoHat.ValueMember = "MaGiaoHat";
            cbGiaoHat.DisplayMember = "TenGiaoHat";
        }
        public List<GiaoHat> GetGiaoHatsInGiaoPhan(string giaoPhanId)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_LIST_GIAO_HAT_THEO_GIAO_PHAN, giaoPhanId));
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Headers["PassWord"] = "admin";
                var responds = (HttpWebResponse)request.GetResponse();
                if (responds == null || responds.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                var rs = responds.GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<GiaoHat>>(data);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void cbGiaoHat_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeData();
        }
        //
        public void changeData()
        {
            var item = cbGiaoHat.SelectedValue;
            if (item is GiaoHat)
            {
                item = (cbGiaoHat.SelectedValue as GiaoHat).MaGiaoHat;
            }
            Thread thread = new Thread(() =>
            {
                giaoXus = GetGiaoXus(item.ToString());
                SetListView(giaoXus);
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void chkSearchAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSearchAll.Checked)
            {
                Thread thread = new Thread(() =>
                {
                    giaoXuAll = GetAllGiaoXu();
                    SetListView(giaoXuAll);
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                changeData();
            }

        }
    }
    public class GiaoXu
    {
        public string ID { get; set; } = "";
        public string TenGiaoXu { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string DienThoai { get; set; } = "";
        public string TenGiaoHat { get; set; } = "";
        public string TenGiaoPhan { get; set; } = "";
        public string Website { get; set; } = "";
        public string MaGiaoXuRieng { get; set; } = "";
        public string MaGiaoHatRieng { get; set; } = "";
        public string MaGiaoPhanRieng { get; set; } = "";
        public string MaGiaoPhan { get; set; } = "";
        public string Ma_GiaoHat { get; set; } = "";
        public string Hinh { get; set; } = "";
        public string Email { get; set; } = "";
        public int ImageIndex { get; set; } = 0;
        public string MaGiaoXuDoi { get; set; }
        public string GhiChu { get; set; }
    }
    public class GiaoPhan
    {
        public string MaGiaoPhan { get; set; }
        public string TenGiaoPhan { get; set; }
    }
    public class GiaoHat
    {
        public string MaGiaoHat { get; set; }
        public string TenGiaoHat { get; set; }
    }
}
