using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Janus.Windows.GridEX;

namespace GxControl
{
    public partial class GxFilter : UserControl
    {
        public GxFilter()
        {
            InitializeComponent();
        }
        private GxGrid grdData;

        public GxGrid GrdData
        {
            get { return grdData; }
            set
            {
                grdData = value;
                if (grdData != null)
                {
                    filterEditor1.SourceControl = value;
                    filterEditor1.ApplyFilter();
                }
            }
        }

        private Dictionary<object, object> filterParameter;

        public Dictionary<object, object> FilterParameter
        {
            get { return filterParameter; }
            set
            {
                filterParameter = value;
                if (value != null)
                {
                    SetDefaultFilter();
                }
            }
        }
        /// <summary>
        /// Lọc dữ liệu khi bắt đầu load data 
        /// </summary>
        private void SetDefaultFilter()
        {
            try
            {
                if (FilterParameter != null && FilterParameter.Count > 0 && grdData != null)
                {
                    GridEXFilterCondition conditionsub;
                    GridEXFilterCondition condition = new GridEXFilterCondition();
                    int dem = 0;
                    foreach (KeyValuePair<object, object> arrCondition in FilterParameter)
                    {
                        conditionsub = new GridEXFilterCondition();
                        conditionsub.Column = GrdData.RootTable.Columns[arrCondition.Key.ToString().Trim()];
                        conditionsub.Value1 = arrCondition.Value.ToString().Trim();

                        if (dem > 0)
                        {
                            condition.AddCondition(LogicalOperator.And, conditionsub);
                        }
                        else
                        {
                            condition = conditionsub;
                        }
                        dem = dem + 1;
                    }
                    condition.ConditionOperator = ConditionOperator.Contains;
                    filterEditor1.FilterCondition = condition;
                    if (filterEditor1.FilterCondition != null) filterEditor1.FilterCondition.ConditionOperator = Janus.Data.ConditionOperator.Contains;
                    filterEditor1.AutomaticHeightResize = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception (GxFilter, SetDefaultFilter)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void AutoApply(bool value)
        {
            filterEditor1.AutoApply = value;
            filterEditor1.ApplyFilter();
        }

        
        #region"FMSFilter_Load"
        private void FMSFilter_Load(object sender, EventArgs e)
        {
            //filterEditor1.FilterConditionChanged += new EventHandler(filterEditor1_FilterConditionChanged);
        }

        private void filterEditor1_FilterConditionChanged(object sender, EventArgs e)
        {
            if (filterEditor1.FilterCondition != null && filterEditor1.FilterCondition.Value1 == null || filterEditor1.FilterCondition.Value1.ToString() == "") return;//fix bug khong nhap gi cung thong bao
            if (grdData != null && grdData.RowCount == 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu tương ứng");
            }
            else
            {
                MessageBox.Show("Tìm thấy " + grdData.RowCount.ToString() + " dữ liệu tương ứng");
            }
        }
        #endregion 

    }
}
