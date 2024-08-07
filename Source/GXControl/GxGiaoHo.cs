using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.EditControls;
using GxGlobal;

namespace GxControl
{
    public partial class GxGiaoHo : GxBaseField
    {
        protected bool choNhapMaRieng = false;

        public GxGiaoHo()
        {
            InitializeComponent();
            label1.Text = "Giáo họ";
            uiComboBox1.ComboStyle = ComboStyle.DropDownList;
            InitControl();
            LoadData();
            this.uiComboBox1.SelectedIndexChanged += new EventHandler(uiComboBox1_SelectedIndexChanged);
            choNhapMaRieng = (Memory.GetConfig(GxConstants.CF_TUNHAP_MAGIADINH) == GxConstants.CF_TRUE);
         
        }
       
        private bool isLuuTru = false;

        public bool IsLuuTru
        {
            get { return isLuuTru; }
            set { isLuuTru = value; }
        }
        private bool isHasLuuTru = false;

        public bool IsHasLuuTru
        {
            get { return isHasLuuTru; }
            set { isHasLuuTru = value; }
        }

        private Janus.Windows.GridEX.TriState isAo = Janus.Windows.GridEX.TriState.Empty;

        public Janus.Windows.GridEX.TriState IsAo
        {
            get { return isAo; }
            set { 
                isAo = value;
                if (autoLoadGrid)
                {
                    if (isAo == Janus.Windows.GridEX.TriState.True)
                    {
                        bool auto = this.autoLoadGrid;
                        this.autoLoadGrid = false;
                        this.SelectedValue = -1;//chọn "tất cả"
                        this.autoLoadGrid = auto;
                    }
                    LoadGridData();
                }
            }
        }

        public UIComboBox Combo
        {
            get { return uiComboBox1; }
        }

        private GxGiaoDanList gridGiaoDan = null;

        public GxGiaoDanList GridGiaoDan
        {
            get { return gridGiaoDan; }
            set { gridGiaoDan = value; gridGiaDinh = null; }
        }

        private GxGiaDinhList gridGiaDinh = null;

        public GxGiaDinhList GridGiaDinh
        {
            get { return gridGiaDinh; }
            set { gridGiaDinh = value; gridGiaoDan = null; }
        }

        private string whereSQL = "";

        public string WhereSQL
        {
            get { return whereSQL; }
            set { whereSQL = value; }
        }

        private bool autoLoadGrid = true;

        public bool AutoLoadGrid
        {
            get { return autoLoadGrid; }
            set { autoLoadGrid = value; }
        }

        public int MaGiaoHo
        {
            get
            {
                try
                {
                    if(uiComboBox1.SelectedValue != null)
                        return (int)uiComboBox1.SelectedValue;
                    return - 1;
                }
                catch
                {
                    return -1;
                }
            }
            set
            {
                SelectedValue = value;
                if (value == -1)
                {
                    uiComboBox1.Text = "";
                }
            }
        }

        public new bool Enabled
        {
            get { return uiComboBox1.Enabled; }
            set
            {
                uiComboBox1.Enabled = value;
                uiComboBox1.BackColor = Color.WhiteSmoke;
            }
        }

        public object SelectedValue
        {
            get { return uiComboBox1.SelectedValue; }
            set
            {
                try
                {
                    bool exists = false;
                    for (int i = 0; i < uiComboBox1.Items.Count; i++)
                    {
                        if (uiComboBox1.Items[i].Value != null && value != null && uiComboBox1.Items[i].Value.ToString() == value.ToString())
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists) uiComboBox1.SelectedValue = value;
                    else
                    {
                        uiComboBox1.SelectedValue = -1;
                        uiComboBox1.Text = "";
                    }
                }
                catch { }
            }
        }

        private bool hasShowAll = false;

        public bool HasShowAll
        {
            get { return hasShowAll; }
            set
            {
                if (value && !this.uiComboBox1.Items.Contains(-1))
                {
                    UIComboBoxItem item1 = new UIComboBoxItem("Tất cả");
                    item1.Value = -1;
                    this.uiComboBox1.Items.Insert(0, item1);
                    //SelectedValue = -1;
                }
                hasShowAll = value;
            }
        }
        
        private bool showNgoaiXu = true;
 

        public bool ShowNgoaiXu
        {
            get { return showNgoaiXu; }
            set 
            { 
                showNgoaiXu = value;
                if (value == true && !this.uiComboBox1.Items.Contains(0))
                {
                    UIComboBoxItem item1 = new UIComboBoxItem("Ngoài xứ", (object)0);
                    if (hasShowAll)
                    {
                        this.uiComboBox1.Items.Insert(1, item1);
                    }
                    else
                    {
                        this.uiComboBox1.Items.Insert(0, item1);
                    }
                }
            }
        }
        //private bool isTextChanging = false;
        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(isTextChanging)
            //{
            //    return;
            //}
            if (gridGiaoDan != null || gridGiaDinh != null)
            {
                if (autoLoadGrid)
                {
                    LoadGridData();
                }
            }
            else
            {
                if (uiComboBox1.SelectedItem != null && uiComboBox1.SelectedItem.DataRow != null && uiComboBox1.SelectedItem.DataRow is DataRow)
                {
                    DataRow r = (DataRow)uiComboBox1.SelectedItem.DataRow;
                    if (r != null)
                    {
                        if (r.Table.Columns.Contains("HasChild") && (bool)r["HasChild"])
                        {
                            MessageBox.Show("Xin vui lòng chọn giáo khu thay cho giáo họ nếu giáo họ có chứa giáo khu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            if (Memory.CurrentGiaoHo != 0)
                                this.MaGiaoHo = Memory.CurrentGiaoHo;
                            else
                                this.MaGiaoHo = -1;
                            return;
                        }
                    }
                }
            }
            if (this.MaGiaoHo > 0) 
                Memory.CurrentGiaoHo = this.MaGiaoHo;
            //if (uiComboBox1.SelectedItem != null && uiComboBox1.SelectedItem.DataRow != null)
            //{
            //    DataRow r = (DataRow)uiComboBox1.SelectedItem.DataRow;
            //    if (!Memory.IsNullOrEmpty(r[GiaoHoConst.MaGiaoHoCha]) && Memory.GetInt(r[GiaoHoConst.MaGiaoHoCha]) != -1)
            //    {
            //        //select parent text
            //        DataRow[] rows = r.Table.Select(string.Format("MaGiaoHo={0}", r[GiaoHoConst.MaGiaoHoCha]));
            //        string tenGiaoHo = this.uiComboBox1.Text;
            //        if(rows.Length!=0)
            //        {
            //            tenGiaoHo = string.Format("{0} > {1}", rows[0][GiaoHoConst.TenGiaoHo], tenGiaoHo);
            //        }
            //        var value = this.uiComboBox1.SelectedValue;
            //        isTextChanging = true;
            //        this.uiComboBox1.Text = tenGiaoHo;
            //        this.uiComboBox1.SelectedValue = value;
            //        isTextChanging = false;
            //    }
            //}
        }

        public void LoadGridData()
        {
            if (gridGiaoDan != null)
            {
                gridGiaoDan.Focus();
                string where = "";
                if(!isHasLuuTru)
                    where = isLuuTru ? " AND (DaXoa=-1 OR DaChuyenXu=-1 OR QuaDoi=-1) " : " AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 ";
                if (this.MaGiaoHo > -1)
                {
                    where += string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", this.MaGiaoHo, this.MaGiaoHo);
                }
                if (this.MaGiaoHo != 0)
                {
                    if (isAo == Janus.Windows.GridEX.TriState.True)
                    {
                        where += " AND GiaoDanAo=-1 ";
                    }
                    else if (isAo == Janus.Windows.GridEX.TriState.False)
                    {
                        where += " AND GiaoDanAo=0 ";
                    }
                }
                gridGiaoDan.WhereSQL = where + whereSQL;
                gridGiaoDan.LoadData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO, null);
            }

            if (gridGiaDinh != null)
            {
                gridGiaDinh.Focus();
                string where = "";
                if (!isHasLuuTru)
                    where = isLuuTru ? " AND (DaXoa=-1 OR DaChuyenXu=-1 ) " : " AND DaXoa=0 AND DaChuyenXu=0";
                
                if (this.MaGiaoHo > -1)
                {
                    where += string.Format(" AND ({0}={1} OR {2}={3}) ", GiaoHoConst.MaGiaoHo, this.MaGiaoHo, GiaoHoConst.MaGiaoHoCha, this.MaGiaoHo);
                }
                if (isAo == Janus.Windows.GridEX.TriState.True)
                {
                    where += " AND GiaDinhAo=-1 ";
                }
                else if (isAo == Janus.Windows.GridEX.TriState.False)
                {
                    where += " AND GiaDinhAo=0 ";
                }
                string orderBy = " ORDER BY MaGiaDinh ASC ";
                if (choNhapMaRieng)
                {
                    orderBy = " ORDER BY MaGiaDinhRieng ASC ";
                }
                gridGiaDinh.WhereSQL = where + whereSQL + orderBy;
                gridGiaDinh.LoadData(SqlConstants.SELECT_GIADINH_LIST, null);
            }
        }

        protected override void InitControl()
        {
            editBase = uiComboBox1;
            base.InitControl();
        }
        private void LoadData()
        {
            if (Memory.IsDesignMode) return;
            DataTable tbl = Memory.GetData("SELECT * FROM GiaoHo ORDER BY TenGiaoHo ASC");
            LoadData(tbl);
        }
        public void LoadData(DataTable tbl)
        {
            try
            {
                if (Memory.IsDesignMode) return;
                uiComboBox1.Items.Clear();
                if (hasShowAll)
                {
                    UIComboBoxItem item1 = new UIComboBoxItem("Tất cả");
                    item1.Value = -1;
                    this.uiComboBox1.Items.Insert(0, item1);
                    //SelectedValue = -1;
                }
                if (showNgoaiXu == true)
                {
                    UIComboBoxItem item1 = new UIComboBoxItem("Ngoài xứ", (object)0);
                    if (hasShowAll)
                    {
                        this.uiComboBox1.Items.Insert(1, item1);
                    }
                    else
                    {
                        this.uiComboBox1.Items.Insert(0, item1);
                    }
                }
                if (tbl != null)
                {
                    Dictionary<int, DataRow> dicGiaoHo = new Dictionary<int, DataRow>();
                    tbl.Columns.Add("HasChild", typeof(bool));
                    foreach (DataRow row in tbl.Rows)
                    {
                        int maGiaoHo = (int)row[GiaoHoConst.MaGiaoHo];
                        if (row[GiaoHoConst.MaGiaoHoCha].ToString() == "")
                        {
                            dicGiaoHo.Add(maGiaoHo, row);
                            DataRow[] rows = tbl.Select(string.Format("MaGiaoHoCha={0}", maGiaoHo));
                            if (rows.Length > 0)
                            {
                                row["HasChild"] = true;
                                foreach (DataRow r in rows)
                                {
                                    r["HasChild"] = false;
                                    dicGiaoHo.Add((int)r[GiaoHoConst.MaGiaoHo], r);
                                }
                            }
                            else
                            {
                                row["HasChild"] = false;
                            }
                        }
                    }
                    foreach (KeyValuePair<int, DataRow> i in dicGiaoHo)
                    {
                        UIComboBoxItem item = new UIComboBoxItem(i.Key);
                        item.DataRow = i.Value;
                        item.Text = i.Value[GiaoHoConst.TenGiaoHo].ToString();
                        item.Value = i.Key;
                        if (i.Value[GiaoHoConst.MaGiaoHoCha].ToString() == "")
                        {
                            item.IndentLevel = 0;
                        }
                        else
                        {
                            item.IndentLevel = 1;
                            if(dicGiaoHo.ContainsKey(Memory.GetInt(i.Value[GiaoHoConst.MaGiaoHoCha])))
                            {
                                item.Text = string.Format("{0} > {1}", dicGiaoHo[Memory.GetInt(i.Value[GiaoHoConst.MaGiaoHoCha])][GiaoHoConst.TenGiaoHo], item.Text);
                            }
                        }
                        this.uiComboBox1.Items.Add(item);
                    }
                }
            }
            catch { }
        }

        private void GxGiaoHo_Load(object sender, EventArgs e)
        {
            //LoadData();
        }
    }
}
