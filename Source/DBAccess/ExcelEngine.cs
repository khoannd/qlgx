using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using System.Data.OleDb;

namespace GxGlobal
{
    public class ExcelEngine
    {
        private object oMissing = System.Reflection.Missing.Value;
        private ApplicationClass ExcelAp = null;
        private Workbooks ExcelWkbks = null;
        private Workbook Excelbk = null;
        public Worksheet ActiveSheet = null;
        private CultureInfo m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
        private string _filedestination = "";
        public static string _filedestination_temp = "";
        
        #region PROPERTY
        private int rowCount = 0;
        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; }
        }

        private int columnCount = 0;

        public int ColumnCount
        {
            get { return columnCount; }
            set { columnCount = value; }
        }

        private Range usedRange = null;

        public Range UsedRange
        {
            get { return usedRange; }
            set { usedRange = value; }
        }

        private ReportType loaiBaoCao = ReportType.KhongRo;

        public ReportType LoaiBaoCao
        {
            get { return loaiBaoCao; }
            set { loaiBaoCao = value; }
        }
        #endregion

        #region PRIVATE
        private string GetLastColumnName(int lastColumnIndex)
        {
            string lastColumn = "";

            // check whether the column count is > 26
            if (lastColumnIndex > 26)
            {
                // If the column count is > 26, the the last column index will be something
                // like "AA", "DE", "BC" etc

                // Get the first letter
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                char first = Convert.ToChar(64 + ((lastColumnIndex - 1) / 26));

                // Get the second letter
                char second = Convert.ToChar(64 + (lastColumnIndex % 26 == 0 ? 26 : lastColumnIndex % 26));

                // Concat. them
                lastColumn = first.ToString() + second.ToString();
            }
            else
            {
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                lastColumn = Convert.ToChar(64 + lastColumnIndex).ToString();
            }
            return lastColumn;
        }

        private Range Find(Range range, object after, object findWhat)
        {
            try
            {
                return range.Find(findWhat,
                            after,
                            XlFindLookIn.xlValues,
                            XlLookAt.xlPart,
                            XlSearchOrder.xlByColumns,
                            XlSearchDirection.xlNext,
                            false,
                            false,
                            false
                            );

            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Get used rage of active work sheet
        /// </summary>
        /// <param name="allCells">The Range to get</param>
        /// <returns></returns>
        private Range GetUsedRange(Range allCells)
        {
            Range rangedCells = null;

            if (allCells != null)
            {
                Range lastCell = null;

                try
                {
                    // Get the last cell to find out the number of
                    // columns and rows
                    lastCell = allCells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);

                    // Get the last row and column information
                    //rowCount = lastCell.Row;
                    rowCount = lastCell.Row;
                    //columnCount = lastCell.Column;
                    columnCount = lastCell.Column;

                    if (columnCount <= 30) columnCount = 30;

                    lastCell = null;

                    if ((rowCount > 0) && (columnCount > 0))
                    {
                        // find the used range
                        // User range string should be something like "A1:D4" etc.
                        // To find the last cell index, we do the following thing.
                        string lastColumn = GetLastColumnName(columnCount);
                        rangedCells = ActiveSheet.get_Range("A1", lastColumn + rowCount.ToString());
                    }
                }
                catch
                {
                    return null;
                }
            }
            return rangedCells;
        }

        private void copy(string fileSource, string fileDestination)
        {

            try
            {
                System.IO.File.Copy(fileSource, fileDestination, true);
            }
            catch (Exception)
            {                
                throw;
            }
        }

        private int pixel2WidthUnits(int pxs)
        {
            return pxs / 10 + 5;
        }

        private void releaseNativeReference(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }
            catch { ;}
            finally
            {
                o = null;
            }
        }

        private Range get_Range(int rowIndex, int colIndex)
        {
            try
            {
                return (Range)ActiveSheet.Cells[rowIndex, colIndex];
            }
            catch
            {
                return null;
            }
        }

        private Range get_Range(string cell1, string cell2)
        {
            try
            {
                return ActiveSheet.get_Range(cell1, cell2);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region PUBLIC

        /// <summary>
        /// Init excel application
        /// </summary>
        /// <param name="fileName">filename to save</param>
        /// <param name="fileTemplate">filename template</param>
        /// <returns>True : init succesfull</returns>
        public bool CreateObject(string fileName, string fileTemplate)
        {

            try
            {
                string file = System.IO.Path.GetFileName(fileName);
                string path = System.IO.Path.GetDirectoryName(fileName);
                string fileNameTemp = System.IO.Path.Combine(path, file);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                if (System.IO.File.Exists(fileNameTemp))
                {
                    System.IO.File.Delete(fileNameTemp);
                }

                copy(fileTemplate, fileNameTemp);

                _filedestination = fileName;
                _filedestination_temp = fileNameTemp;

                ExcelAp = new ApplicationClass();
                ExcelWkbks = ExcelAp.Workbooks;

                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Excelbk = ExcelWkbks.Open(fileNameTemp, 3, false, 3, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                    ";", false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                ActiveSheet = (Worksheet)Excelbk.ActiveSheet;

                ExcelAp.ScreenUpdating = true;
                Excelbk.Saved = true;

                ExcelAp.DisplayAlerts = false;

                //Get RowCount and ColumnCount
                Range allCells = ActiveSheet.get_Range("A1", "Z500");
                usedRange = GetUsedRange(allCells);
                //range = ActiveSheet.UsedRange;

                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Create new excel file
        /// </summary>
        /// <param name="filePath"></param>
        public void CreateNewObject(string filePath)
        {
            _filedestination = filePath;
            object misValue = System.Reflection.Missing.Value;

            ExcelAp = new ApplicationClass();
            ExcelWkbks = ExcelAp.Workbooks;

            m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Excelbk = ExcelAp.Workbooks.Add(misValue);

            ActiveSheet = (Worksheet)Excelbk.Worksheets.get_Item(1);

            //Excelbk.SaveAs(_filedestination, XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

            ExcelAp.ScreenUpdating = true;
            Excelbk.Saved = true;

            ExcelAp.DisplayAlerts = false;

            //Get RowCount and ColumnCount
            Range allCells = ActiveSheet.get_Range("A1", "Z500");
            usedRange = GetUsedRange(allCells);

            Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
        }

        /// <summary>
        /// Finish write data to excel
        /// </summary>
        public void End_Write()
        {
            try
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Excelbk.Saved = true;
                ExcelAp.DisplayAlerts = false;

                try
                {
                    Excelbk.SaveAs(_filedestination, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                                                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch
                {
                    // File.Delete(_filedestination);
                }

                ExcelWkbks.Close();

                ExcelAp.Quit();

                releaseNativeReference(ActiveSheet);
                releaseNativeReference(Excelbk);
                releaseNativeReference(ExcelWkbks);
                releaseNativeReference(ExcelAp);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch
            {
            }
        }

        public void Write_to_excel(int rowIndex, int colIndex, object value)
        {
            if (Memory.GetConfig(GxConstants.CF_LANGUAGE) != GxConstants.LANG_VN)
            {
                value = Memory.GetStringKhongDau(value.ToString());
            }
            m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Range objrange = get_Range(rowIndex, colIndex);
            objrange.Value2 = value;
            Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            releaseNativeReference(objrange);

        }

        public void AutoFitRow(int startRow, int lastRow)
        {
            for (int i = startRow; i <= lastRow; i++)
            {
                try
                {
                    ((Range)ActiveSheet.Rows[i, Type.Missing]).AutoFit();
                }
                catch
                {
                }
            }
        }

        public void Write_to_excel(string key, object value)
        {
            string lang = Memory.GetConfig(GxConstants.CF_LANGUAGE);
            if (lang != GxConstants.LANG_VN && key.ToLower().StartsWith("ngay"))
            {
                value = Memory.GetDateStringByLang(value);
            }
            if (key.ToLower().StartsWith("ngay") || key.ToLower().StartsWith("so"))
            {
                value = "'" + value;
            }
            else if (lang == GxConstants.LANG_EN && Memory.GetConfig(GxConstants.CF_US_FORMAT_NAME) == "1"
                        && Memory.IsHoTenKey(key.ToLower()))
            {
                value = Memory.GetHoTenByLang(value.ToString(), lang);
            }
            if (lang != GxConstants.LANG_VN)
            {
                value = Memory.GetStringKhongDau(value.ToString());
            }
            if (usedRange == null) return;
            try
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                //Range rgFound = FindRange(key);
                //if (rgFound == null) return;
                //int firstRow = rgFound.Row;
                //int firstCol = rgFound.Column;

                //Range currRange = rgFound;
                //while (currRange != null)
                //{
                //    currRange.Value2 = value;
                //    currRange = usedRange.FindNext(currRange);
                //    if (currRange != null)
                //    {
                //        int currRow = currRange.Row;
                //        int currCol = currRange.Column;

                //        if (currRow>-1 && currCol>-1 && currRow == firstRow && currCol == firstCol)
                //        {
                //            releaseNativeReference(currRange);
                //            currRange = null;
                //        }
                //    }
                //}
                Replace(string.Format("[{0}]", key), value);
                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch { }
        }

        public void Replace(string key, object value)
        {
            if (usedRange == null) return;
            try
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                usedRange.Replace(key, value, XlLookAt.xlPart, XlSearchOrder.xlByColumns, true, false, false, false);
                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch { }
        }

        public Range FindRange(string key)
        {
            Range cells = (Range)usedRange.Cells[1, 1];
            if (cells == null) return null;
            return Find(usedRange, cells, key);
        }

        public int FindRowIndex(string key)
        {
            Range range = FindRange(key);
            if (range == null) return -1;
            return range.Row;
        }

        public int FindColIndex(string key)
        {
            Range range = FindRange(key);
            if (range == null) return -1;
            return range.Column;
        }

        public void SetWidth(int colIndex, int value)
        {
            string colName = MapColIndexToColName(colIndex);
            Range objRange = ActiveSheet.Columns;
            objRange.ColumnWidth = value;
        }

        public void ExecuteExport()
        {
            if (System.IO.File.Exists(_filedestination_temp))
            {
                System.IO.File.Delete(_filedestination_temp);
            }
            _filedestination_temp = "";
            System.Diagnostics.Process.Start(_filedestination);
        }

        public static void DeleteFileTmp()
        {
            try
            {
                if (System.IO.File.Exists(_filedestination_temp))
                {
                    System.IO.File.Delete(_filedestination_temp);
                }
            }
            catch
            {
            }
        }

        public string MapColIndexToColName(int idx)
        {
            char[] chars = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            //return char.ConvertFromUtf32(idx + 64);
            idx -= 1; //adjust so it matches 0-indexed array rather than 1-indexed column

            int quotient = idx / 26;
            if (quotient > 0)
                return MapColIndexToColName(quotient) + chars[idx % 26].ToString();
            else
                return chars[idx % 26].ToString();
        }

        public void Border_Range(string cell1, string cell2)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null)
            {
                Microsoft.Office.Interop.Excel.Borders boders = range.Borders;//[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical];
                boders.LineStyle = XlLineStyle.xlContinuous;
                boders.Weight = XlBorderWeight.xlThin;
                boders.ColorIndex = XlColorIndex.xlColorIndexAutomatic;
            }
        }

        public void Merge(string cell1, string cell2)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null)
            {
                range.Merge(Type.Missing);
            }
        }

        public void SetFont(string cell1, string cell2, System.Drawing.Font font)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null && UsedRange.Font != null)
            {
                range.Font.Strikethrough = font.Strikeout;
                range.Font.Size = font.Size;
                range.Font.Italic = font.Italic;
                range.Font.Bold = font.Bold;
                range.Font.Name = font.Name;
                range.Font.Underline = font.Underline;
            }
        }

        public void SetStrikeout(string cell1, string cell2, System.Drawing.Font font)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null && UsedRange.Font != null)
            {
                range.Font.Strikethrough = font.Strikeout;
                range.Font.Underline = font.Underline;
            }
        }

        public void SetUnderline(string cell1, string cell2, System.Drawing.Font font)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null && UsedRange.Font != null)
            {
                range.Font.Underline = font.Underline;
            }
        }

        public void SetItalic(string cell1, string cell2, System.Drawing.Font font)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null && UsedRange.Font != null)
            {
                range.Font.Italic = font.Italic;
            }
        }

        public void SetWrapText(string cell1, string cell2, bool wr)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null)
            {
                range.WrapText = wr;
            }
        }

        public void SetAlignment(string cell1, string cell2, XlHAlign align)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null)
            {
                range.HorizontalAlignment = align;
            }
        }

        public void SetVerticalAlignment(string cell1, string cell2, XlVAlign align)
        {
            Range range = get_Range(cell1, cell2);
            if (range != null)
            {
                range.VerticalAlignment = align;
            }
        }

        public void Range_Insert(int rowIndex, int countRow)
        {
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Range objRange = (Range)ActiveSheet.Rows[rowIndex, Type.Missing];
                objRange.Insert(Type.Missing, countRow);

                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
                releaseNativeReference(objRange);
            }
        }

        public void Write_to_excel_color(int rowIndex, int colIndex, System.Drawing.Color color)
        {
            m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Range objRange = get_Range(rowIndex, colIndex);
            objRange.Interior.Color = color;

            releaseNativeReference(objRange);
        }

        public void createChart(ChartData chartData)
        {
            Range chartRange;

            ChartObjects xlCharts = (ChartObjects)ActiveSheet.ChartObjects(Type.Missing);
            ChartObject myChart = (ChartObject)xlCharts.Add(chartData.left, chartData.top, chartData.width, chartData.height);
            Chart chartPage = myChart.Chart;
            chartPage.HasTitle = true;
            chartPage.ChartTitle.Text = chartData.title;

            chartRange = ActiveSheet.get_Range(chartData.cellDataFrom, chartData.cellDataTo);
            chartRange.Font.Name = "Arial";
            chartRange.Font.Size = 8;

            chartPage.SetSourceData(chartRange, Type.Missing);
            chartPage.ChartType = (XlChartType)(chartData.chartType);
            chartPage.Legend.Font.Name = "Arial";
            chartPage.Legend.Font.Size = 9;
            if (chartData.showLabel)
            {
                if (chartData.showPercent)
                {
                    chartPage.ApplyDataLabels(
                    XlDataLabelsType.xlDataLabelsShowValue,
                    false, false, false, false, false, false, true,
                    false, false);
                }
                else
                {
                    chartPage.ApplyDataLabels(
                    XlDataLabelsType.xlDataLabelsShowValue,
                    false, false, false, false, false, true, false,
                    false, false);
                }
            }

            if (!chartData.showLegend)
            {
                chartPage.HasLegend = false;
            }

            Axis Xaxis = (Axis)chartPage.Axes(XlAxisType.xlValue,
                          XlAxisGroup.xlPrimary);
            try
            {
                Xaxis.MinimumScale = 0;
            }
            catch { }
        }

        public void SetGridLines(bool hidden)
        {
            ActiveSheet.PageSetup.Application.ActiveWindow.DisplayGridlines = !hidden;
        }
        #endregion

        #region OleDb
        /// <summary>
        /// Get DataSet contains DataTable in excel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(string path, string tableName)
        {
            try
            {
                OleDbConnection connect = new OleDbConnection();
                OleDbDataAdapter adapter = new OleDbDataAdapter();

                connect = new OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + "';Extended Properties='Excel 8.0;HDR=YES;IMEX=1'");

                adapter = new OleDbDataAdapter(string.Format("select * from [{0}$]", tableName), connect);
                adapter.TableMappings.Add("Table", tableName);
                DataSet ds = new DataSet();
                adapter.Fill(ds, tableName);
                adapter.Dispose();

                connect.Close();
                return ds;
            }
            catch (System.Exception ex)
            {
                //Write log
                //Temporary solution
                System.Windows.Forms.MessageBox.Show("Error when loading data: " + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Execute all rows in DataTable into Excel File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tableName"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static Int32 Execute(string path, string tableName, System.Data.DataTable tbl)
        {
            int rs = 0;
            if (tbl == null)
            {
                Memory.Instance.Error = new Exception("DataTable can't null");
                return -1;
            }
            try
            {
                OleDbDataAdapter da = null;
                OleDbConnection connect = new OleDbConnection("provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + path + "';Extended Properties='Excel 8.0;HDR=YES;IMEX=1'");
                try
                {
                    connect.Open();
                    da = new OleDbDataAdapter(string.Format("select * from [{0}$]", tableName), connect);

                    OleDbCommandBuilder cmdBd = new OleDbCommandBuilder(da);
                    cmdBd.ConflictOption = ConflictOption.OverwriteChanges;
                    cmdBd.QuotePrefix = "[";
                    cmdBd.QuoteSuffix = "]";
                    da.DeleteCommand = cmdBd.GetDeleteCommand();
                    da.InsertCommand = cmdBd.GetInsertCommand();
                    da.UpdateCommand = cmdBd.GetUpdateCommand();
                    rs = da.Update(tbl);
                }
                catch (Exception ex)
                {
                    Memory.Instance.Error = ex;
                    rs = -1;
                }
                finally
                {
                    connect.Close();
                }
            }
            catch (Exception e)
            {
                //Map server exception to meaningful client error message
                Memory.Instance.Error = e;
                rs = -2;
            }
            return rs;
        }
        #endregion
    }

    public enum XlHAlign
    {
        xlHAlignRight = -4152,
        xlHAlignLeft = -4131,
        xlHAlignJustify = -4130,
        xlHAlignDistributed = -4117,
        xlHAlignCenter = -4108,
        xlHAlignGeneral = 1,
        xlHAlignFill = 5,
        xlHAlignCenterAcrossSelection = 7,
    }

    public class ChartData
    {
        public string title = ""; 
        public ChartType chartType = ChartType.xlBarClustered; 
        public string cellDataFrom = "";
        public string cellDataTo = "";
        public int left = 0;
        public int top = 10; 
        public int width = 0; 
        public int height = 0; 
        public bool showLabel = false;
        public bool showLegend = true;
        public bool showPercent = false;
    }
}
