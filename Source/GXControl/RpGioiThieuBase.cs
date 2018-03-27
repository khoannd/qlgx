using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GxControl
{
    public abstract class RpGioiThieuBase
    {
        private string fileName;
        public WordEngine word = null;
        public string TenLinhMuc { get; set; }
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
            }
        }

        public int Export(DataSet ds)
        {
            DataTable tblGiaoXu = ds.Tables[GiaoXuConst.TableName];
            DataTable tblGiaoXuNhan = ds.Tables[GiaoXuNhanConst.TableName];
            DataTable tblGiaoDan = ds.Tables[GiaoDanConst.TableName];

            if (tblGiaoDan.Rows.Count == 0 || tblGiaoXu.Rows.Count == 0) return -1;
            string templatePath = Memory.GetReportTemplatePath(FileName);
            string outputPath = Memory.GetTempPath(FileName);

            DataRow rowGiaoXu = tblGiaoXu.Rows[0];
            DataRow rowGiaoDan = tblGiaoDan.Rows[0];
            DataRow rowGiaoXuNhan = tblGiaoXuNhan.Rows[0];

            string reportFormat = Memory.GetReportFormat();
            templatePath = string.Concat(templatePath, reportFormat);
            outputPath = string.Concat(outputPath, reportFormat);
            if (reportFormat == GxConstants.DOC_FORMAT)
            {
                word = new WordEngine();
                word.CreateObject(outputPath, templatePath);
                SetReplace(tblGiaoXuNhan, tblGiaoXu, tblGiaoDan);
                word.End_Write();
                System.Diagnostics.Process.Start(outputPath);
            }


            return 1;
        }
        public abstract int SetReplace(DataTable tblGiaoXuNhan, DataTable tblGiaoXu, DataTable tblGiaoDan);
        
    }
}
