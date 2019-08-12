using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.Data;

namespace ExcelReport
{
    public class ChartBiTich
    {
        public static int Export(DataSet ds)
        {
            try
            {
                ExcelEngine excel = new ExcelEngine();
                string fileName = "BieuDoBiTich.xls";

                string outputPath = Memory.GetTempPath(fileName);

                excel.CreateNewObject(outputPath);
                excel.SetGridLines(true); 

                DataTable tblData = ds.Tables[0];
                int startRow = 25;
                int col = 1;
                int row = startRow;

                excel.Write_to_excel(row, col, "");
                col++;
                for (int i = 1; i < tblData.Columns.Count; i++)
                {
                    excel.Write_to_excel(row, i + col - 1, "'" + tblData.Columns[i].ColumnName);
                }
                row++;
                //write detail data
                col = 1;
                for (int i = 0; i < tblData.Rows.Count; i++)
                {
                    for (int j = 0; j < tblData.Columns.Count; j++)
                    {
                        excel.Write_to_excel(row + i, j + col, tblData.Rows[i][j]);
                    }
                }

                string cell1 = "A" + startRow.ToString();
                string cell2 = excel.MapColIndexToColName(tblData.Columns.Count) + (startRow + tblData.Rows.Count).ToString();

                int width = 500;
                int height = 300;
                if (tblData.Columns.Count > 5)
                {
                    width += 50 * (int)(tblData.Columns.Count - 5);
                }
                if (width > 900) width = 900;

                ChartData chartData = new ChartData();
                chartData.title = "Biểu đồ theo dõi bí tích";
                chartData.chartType = ChartType.xlColumnClustered;
                chartData.cellDataFrom = cell1;
                chartData.cellDataTo = cell2;
                chartData.width = width;
                chartData.height = height;

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
