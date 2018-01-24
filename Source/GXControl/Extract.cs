using GxGlobal;
using Janus.Windows.ButtonBar;
using System;
using System.Collections.Generic;
using System.Text;

namespace GxControl
{
    /// <summary>
    /// 2018-01-08 SonVc 
    /// Class thực hiện trích xuất dữ liệu dựa theo yêu cầu của user,bao gồm :
    ///     SelectHeadByAge() : Select chủ hộ theo tuổi
    ///     SelectByVaiTro(bool sex): Select danh sách giáo dân theo vai trò gia đình  : chồng hoặc vợ
    /// </summary>
    public class Extract
    {
        #region Properties
        private int fromYear;
        private int toYear;
        private GxGiaoHo typeGiaoHo;
        private GxGiaoDanList listGrid;
        private bool isLuuTru;
        private string condition;
        private string where;
        private bool allowNull;
        public int FromYear
        {
            get
            {
                return fromYear;
            }

            set
            {
                fromYear = DateTime.Now.Year - value;
            }
        }

        public int ToYear
        {
            get
            {
                return toYear;
            }

            set
            {
                toYear = DateTime.Now.Year - value;
            }
        }
        public GxGiaoHo TypeGiaoHo
        {
            get
            {
                return typeGiaoHo;
            }

            set
            {
                typeGiaoHo = value;
            }
        }

        public GxGiaoDanList ListGrid
        {
            get
            {
                return listGrid;
            }

            set
            {
                listGrid = value;
            }
        }

        public bool IsLuuTru
        {
            get
            {
                return isLuuTru;
            }

            set
            {
                isLuuTru = value;
            }
        }

        public bool AllowNull
        {
            get
            {
                return allowNull;
            }

            set
            {
                allowNull = value;
            }
        }
        #endregion

        #region Init
        public Extract( GxGiaoHo typeGiaoHo, GxGiaoDanList list)
        {
            this.typeGiaoHo = typeGiaoHo;
            this.listGrid = list;
        }
        public Extract() { }
        #endregion
        #region Method
        public void SetWhere()
        {
            where = "";
            if (!isLuuTru) where = " AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0 ";
            if (typeGiaoHo.MaGiaoHo > -1)
            {
                where = string.Concat(where, string.Format(" AND (MaGiaoHo={0} OR MaGiaoHoCha={0}) ", typeGiaoHo.MaGiaoHo, typeGiaoHo.MaGiaoHo));
            }
            condition = " AND ((NamSinh BETWEEN '{0}' AND '{1}')";
            condition = string.Format(condition, fromYear, toYear);
            //TRƯỜNG HỢP CHO NULL
            condition = allowNull ? string.Concat(condition, " OR ( NamSinh IS NULL OR NamSinh = '' ))") : string.Concat(condition, ")");
        }
        public void SelectHeadByAge()
        {
            SetWhere();
            where = string.Concat(where, " AND ThanhVienGiaDinh.ChuHo = -1");
            listGrid.LoadData(string.Concat(SqlConstants.SELECT_LISTGIAODAN_BYGIADINH, where, condition), null);
        }
        public void SelectByVaiTro(bool sex)
        {
            SetWhere();
            if (sex == true)
            {
                where = string.Concat(where, " AND ThanhVienGiaDinh.VaiTro = 1 ");
            }
            else
            {
                where = string.Concat(where, "AND ThanhVienGiaDinh.VaiTro = 0 ");
            }
            listGrid.LoadData(string.Concat(SqlConstants.SELECT_LISTGIAODAN_BYGIADINH, where,condition), null);
        }
        public void SelectByTuoi(TypeByAge type)
        {
            SetWhere();
            int yearNow = DateTime.Now.Year;
            if(type == TypeByAge.THIEU_NHI)
            {
                condition = string.Format(" AND (NamSinh BETWEEN '{0}' AND '{1}')",fromYear,toYear);
            }
            else if(type == TypeByAge.GIOI_TRE)
            {
                condition = string.Format(" AND (NamSinh BETWEEN '{0}' AND '{1}') AND DaCoGiaDinh = 0", fromYear,toYear);
            }
            else
            {
                condition = string.Format(" AND (NamSinh BETWEEN '{0}' AND '{1}')",1,fromYear);
            }
            listGrid.LoadData(string.Concat(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO, where, condition), null);
        }    
        #endregion
    }
}
