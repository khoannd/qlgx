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
        List<GiaoXu> giaoXus = new List<GiaoXu>();
        public frmCreateGiaoXu()
        {
            InitializeComponent();
            listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
            listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
        }
        public void Set()
        {
            Thread thread = new Thread(() =>
            {
                //SetListView();
                SetGiaoPhan();
                //cbGiaoPhan.SelectedIndex = 0;
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void SetGiaoPhan()
        {
            var giaoPhans = GetGiaoPhans();
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
        public void SetListView()
        {
            listView1.Items.Clear();
            if (giaoXus == null) return;
            foreach (var giaoXu in giaoXus)
            {

                var gs = new GiaoXu() { TenGiaoXu = giaoXu.TenGiaoXu, TenGiaoHat = giaoXu.TenGiaoHat, TenGiaoPhan = giaoXu.TenGiaoPhan, DiaChi = giaoXu.DiaChi, DienThoai = giaoXu.DienThoai, Website = giaoXu.Website,MaGiaoXuRieng = giaoXu.ID,MaGiaoHatRieng = giaoXu.Ma_GiaoHat,MaGiaoPhanRieng = giaoXu.MaGiaoPhan,Hinh = giaoXu.Hinh,Email = giaoXu.Email};
                ListViewItem item = new ListViewItem(new string[] { string.Concat("Giáo xứ:",giaoXu.TenGiaoXu), string.Concat("Thuộc giáo hạt :",giaoXu.TenGiaoHat), string.Concat("Thuộc giáo phận:",giaoXu.TenGiaoPhan),string.Concat("Địa chỉ:",giaoXu.DiaChi),string.Concat("Số điện thoại:", giaoXu.DienThoai) }, 0);
                item.Tag = gs;
                listView1.Items.Add(item);
                var img = GetImage(gs.MaGiaoXuRieng);
                if(img != null)
                {
                    imageList1.Images.Add(img);
                    item.ImageIndex = imageList1.Images.Count - 1;
                    giaoXu.ImageIndex = item.ImageIndex;
                } else
                {
                    item.ImageIndex = 0;
                    giaoXu.ImageIndex = 0;
                }
            }
        }
        public Image GetImage(string idGiaoXu)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_DOWNLOAD_GIAO_XU_IMAGE,idGiaoXu));
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
        public void SetListViewSearch(string name)
        {
            listView1.Items.Clear();
            if (giaoXus == null) return;
            foreach (var giaoXu in giaoXus)
            {
                if (!giaoXu.TenGiaoXu.ToLower().Contains(name)) continue;
                var gs = new GiaoXu() { TenGiaoXu = giaoXu.TenGiaoXu, TenGiaoHat = giaoXu.TenGiaoHat, TenGiaoPhan = giaoXu.TenGiaoPhan, DiaChi = giaoXu.DiaChi, DienThoai = giaoXu.DienThoai, Website = giaoXu.Website, MaGiaoXuRieng = giaoXu.ID, MaGiaoHatRieng = giaoXu.Ma_GiaoHat, MaGiaoPhanRieng = giaoXu.MaGiaoPhan, Hinh = giaoXu.Hinh, Email = giaoXu.Email };
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
            Thread thread = new Thread(() =>
            {
                var image = GetImage(giaoXuInfo.MaGiaoXuRieng);
                image.Save(giaoXuInfo.Hinh);
            });
            thread.IsBackground = true;
            thread.Start();
            var rsGiaoPhan = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_PHAN, giaoXuInfo.TenGiaoPhan,giaoXuInfo.MaGiaoPhanRieng);
            var rsGiaoHat = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_HAT, giaoXuInfo.TenGiaoHat, giaoXuInfo.MaGiaoHatRieng);
            var rsGiaoXu = Memory.ExecuteSqlCommand(SqlConstants.INSERT_GIAO_XU, giaoXuInfo.TenGiaoXu, giaoXuInfo.DiaChi, giaoXuInfo.DienThoai, giaoXuInfo.Website, giaoXuInfo.Hinh,giaoXuInfo.MaGiaoXuRieng,giaoXuInfo.Email);
            if (!Memory.HasError())
            {
                this.Close();
            }
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        public List<GiaoXu> GetGiaoXus(string giaoHatId)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_LIST_GIAO_XU_THEO_GIAO_HAT,giaoHatId));
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
            }catch(Exception ex)
            {
                return null;
            }

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //SetListViewSearch(txtSearch.Text);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            SetListViewSearch(txtSearch.Text.ToLower());
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbGiaoPhan_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbGiaoPhan.SelectedValue;
            if(item is GiaoPhan)
            {
                item = (cbGiaoPhan.SelectedValue as GiaoPhan).MaGiaoPhan;
            }
            var giaoHats = GetGiaoHatsInGiaoPhan(item.ToString());
            cbGiaoHat.DataSource = giaoHats;
            cbGiaoHat.ValueMember = "MaGiaoHat";
            cbGiaoHat.DisplayMember = "TenGiaoHat";
        }
        public List<GiaoHat> GetGiaoHatsInGiaoPhan(string giaoPhanId)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Concat(GxConstants.URL_GET_LIST_GIAO_HAT_THEO_GIAO_PHAN,giaoPhanId));
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
            var item = cbGiaoHat.SelectedValue;
            if (item is GiaoHat)
            {
                item = (cbGiaoHat.SelectedValue as GiaoHat).MaGiaoHat;
            }
            Thread thread = new Thread(() =>
            {
                giaoXus = GetGiaoXus(item.ToString());
                SetListView();
            });
            thread.IsBackground = true;
            thread.Start();
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
    }
    public class GiaoPhan
    {
        private string maGiaoPhan;
        private string tenGiaoPhan;

        public string MaGiaoPhan { get => maGiaoPhan; set => maGiaoPhan = value; }
        public string TenGiaoPhan { get => tenGiaoPhan; set => tenGiaoPhan = value; }
    }
    public class GiaoHat
    {
        private string maGiaoHat;
        private string tenGiaoHat;

        public string MaGiaoHat { get => maGiaoHat; set => maGiaoHat = value; }
        public string TenGiaoHat { get => tenGiaoHat; set => tenGiaoHat = value; }
    }
}
