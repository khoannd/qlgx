using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ChartTongHonPhoi
    {
        public static int Export(DataSet ds)
        {
            try
            {
                ExcelEngine excel = new ExcelEngine();
                string fileName = "BieuDoHonPhoi.xls";

                string outputPath = Memory.GetTempPath(fileName);

                excel.CreateNewObject(outputPath);
                excel.SetGridLines(true); 

                DataTable tblData = ds.Tables[0];
                int startRow = 25;
                int col = 1;
                int row = startRow;

                excel.Write_to_excel(row, col, "Năm");
                col++;
                for (int i = 0; i < tblData.Columns.Count; i++)
                {
                    excel.Write_to_excel(row, i + col, "'" + tblData.Columns[i].ColumnName);
                }
                row++;
                col = 1;
                excel.Write_to_excel(row, col, "Hôn phối");
                col++;
                for (int i = 0; i < tblData.Rows.Count; i++)
                {
                    for (int j = 0; j < tblData.Columns.Count; j++)
                    {
                        excel.Write_to_excel(row + i, j + col, tblData.Rows[i][j]);
                    }
                }
                string cell1 = "A" + startRow.ToString();
                string cell2 = excel.MapColIndexToColName(tblData.Columns.Count + 1) + (startRow + tblData.Rows.Count).ToString();

                int width = 400;
                int height = 300;
                if (tblData.Columns.Count > 5)
                {
                    width += 50 * (int)(tblData.Columns.Count - 5);
                }
                if (width > 900) width = 900;
                ChartData chartData = new ChartData();
                chartData.title = "Biểu đồ hôn phối";
                chartData.chartType = ChartType.xlColumnClustered;
                chartData.cellDataFrom = cell1;
                chartData.cellDataTo = cell2;
                chartData.width = width;
                chartData.height = height;
                chartData.showLegend = false;

                excel.createChart(chartData);
                excel.End_Write();
                System.Diagnostics.Process.Start(outputPath);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return -1;
            }
            return 0;
        }
    }
}
