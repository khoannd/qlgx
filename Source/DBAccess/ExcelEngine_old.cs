// @(h) DataResource.cs ver 1.0 (08.03.11 Hai)
//
// @(s) 
//      Using for export to excel
//

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

namespace GxGlobal
{
    public class ExcelEngine
    {
        private object oMissing = System.Reflection.Missing.Value;
        private Object ExcelAp = null;
        private Object ExcelWkbks = null;
        private Object Excelbk = null;
        private Object ActiveSheet = null;
        private CultureInfo m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
        private string _filedestination = "";
        public static string _filedestination_temp = "";
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

        private object range = null;

        public object Range
        {
            get { return range; }
            set { range = value; }
        }

        private ReportType loaiBaoCao = ReportType.KhongRo;

        public ReportType LoaiBaoCao
        {
            get { return loaiBaoCao; }
            set { loaiBaoCao = value; }
        }

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

                object ExcelSheets = null;
                Object[] parameters = null;
                Type objClassType;
                objClassType = Type.GetTypeFromProgID("Excel.Application");
                ExcelAp = Activator.CreateInstance(objClassType);
                ExcelWkbks = ExcelAp.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, ExcelAp, null);
                parameters = new Object[15];
                parameters[0] = fileNameTemp;
                parameters[1] = 3;
                parameters[2] = false;
                parameters[3] = 3;
                parameters[4] = Type.Missing;
                parameters[5] = Type.Missing;
                parameters[6] = Type.Missing;
                parameters[7] = Type.Missing;
                parameters[8] = ";";
                parameters[9] = false;
                parameters[10] = Type.Missing;
                parameters[11] = Type.Missing;
                parameters[12] = Type.Missing;
                parameters[13] = Type.Missing;
                parameters[14] = Type.Missing;

                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Excelbk = ExcelWkbks.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, ExcelWkbks, parameters);

                ExcelSheets = ExcelWkbks.GetType().InvokeMember("Sheets", BindingFlags.GetProperty, null, Excelbk, null);
                parameters = new Object[1];
                parameters[0] = 1;

                ActiveSheet = ExcelSheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, ExcelSheets, parameters);


                parameters = new Object[1];
                parameters[0] = true;
                ExcelAp.GetType().InvokeMember("ScreenUpdating", BindingFlags.SetProperty, null, ExcelAp, parameters);

                parameters[0] = true;
                Excelbk.GetType().InvokeMember("Saved", BindingFlags.SetProperty, null, Excelbk, parameters);
                parameters[0] = false;
                ExcelAp.GetType().InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, ExcelAp, parameters);

                //Get RowCount and ColumnCount
                object allCells = ActiveSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, null, ActiveSheet,
                                                                    new object[] { "A1", "Z255" });
                range = GetUsedRange(allCells);
                
                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void Print()
        {
            string fileName = "";
            Microsoft.Office.Interop.Excel.ApplicationClass ExcelAp = new Microsoft.Office.Interop.Excel.ApplicationClass();
            Microsoft.Office.Interop.Excel.Workbooks books = ExcelAp.Workbooks;
            books.Open(exportPath, 3, false, 3, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                ";", false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            ExcelAp.ActiveWorkbook.PrintOut(Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                            Type.Missing, Type.Missing, Type.Missing, Type.Missing);

            // Cleanup:
            GC.Collect();
            GC.WaitForPendingFinalizers();

            ExcelAp.ActiveWorkbook.Close(false, Type.Missing, Type.Missing);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ExcelAp.ActiveWorkbook);

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(books);

            ExcelAp.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ExcelAp);
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

                Object[] parameters = new Object[1];
                parameters[0] = true;
                Excelbk.GetType().InvokeMember("Saved", BindingFlags.SetProperty, null, Excelbk, parameters);
                parameters[0] = false;
                ExcelAp.GetType().InvokeMember("DisplayAlerts", BindingFlags.SetProperty, null, ExcelAp, parameters);


                parameters = new Object[12];
                parameters[0] = _filedestination;
                parameters[1] = Type.Missing;
                parameters[2] = Type.Missing;
                parameters[3] = Type.Missing;
                parameters[4] = Type.Missing;
                parameters[5] = Type.Missing;
                parameters[6] = 1;
                parameters[7] = Type.Missing;
                parameters[8] = Type.Missing;
                parameters[9] = Type.Missing;
                parameters[10] = Type.Missing;
                parameters[11] = Type.Missing;

                try
                {
                    Excelbk.GetType().InvokeMember("SaveAs", BindingFlags.InvokeMethod, null, Excelbk, parameters);
                }
                catch
                {
                    // File.Delete(_filedestination);
                }


                ExcelWkbks.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, ExcelWkbks, null);

                ExcelAp.GetType().InvokeMember("Quit", BindingFlags.InvokeMethod, null, ExcelAp, null);
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
            if (Memory.GetConfig(GxConstants.CF_LANGUAGE).ToString() != GxConstants.LANG_VN)
            {
                value = Memory.GetStringKhongDau(value.ToString());
            }
            m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Object objrange = get_Range(rowIndex, colIndex);
            Object[] parameters = new Object[1];
            parameters[0] = value;

            objrange.GetType().InvokeMember("Value2", BindingFlags.SetProperty, null, objrange, parameters);

            Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            releaseNativeReference(objrange);

        }

        //public void Write_to_excel(string key, object value)
        //{
        //    m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
        //    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        //    for (int i = 1; i <= rowCount; i++)
        //    {
        //        for (int j = 1; j <= columnCount; j++)
        //        {
        //            Object objrange = get_Range(i, j);
        //            if (objrange != null)
        //            {
        //                object value2 = objrange.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, objrange, null);
        //                if (value2 != null && (value2.ToString().ToLower() == string.Format("[{0}]", key.ToLower())))
        //                {
        //                    Object[] parameters = new Object[1];
        //                    parameters[0] = value;
        //                    objrange.GetType().InvokeMember("Value2", BindingFlags.SetProperty, null, objrange, parameters);
        //                }
        //                releaseNativeReference(objrange);
        //            }
        //        }
        //    }
        //    Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
        //}

        public void Write_to_excel(string key, object value)
        {
            key = string.Format("[{0}]", key);
            string lang = Memory.GetConfig(GxConstants.CF_LANGUAGE).ToString();
            if (lang != GxConstants.LANG_VN && key.ToLower().StartsWith("ngay"))
            {
                value = Memory.GetDateStringByLang(value);
            }
            if (key.ToLower().StartsWith("ngay") || key.ToLower().StartsWith("so"))
            {
                value = "'" + value;
            }
            else if (lang == GxConstants.LANG_EN && Memory.GetConfig(GxConstants.CF_US_FORMAT_NAME).ToString() == "1"
                        && Memory.IsHoTenKey(key.ToLower()))
            {
                value = Memory.GetHoTenByLang(value.ToString(), lang);
            }
            if (lang != GxConstants.LANG_VN)
            {
                value = Memory.GetStringKhongDau(value.ToString());
            }
            if (range == null) return;
            try
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                //object cells = range.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, range, new object[] { 1, 1 });
                //if (cells == null) return;
                object rgFound = FindRange(key);
                if (rgFound == null) return;
                int firstRow = FindRowIndex(rgFound);
                int firstCol = FindColIndex(rgFound);

                object currRange = rgFound;
                while (currRange != null)
                {
                    object value2 = currRange.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, currRange, null);
                    if (value2 != null && (value2.ToString().ToLower() == string.Format("[{0}]", key.ToLower())))
                    {
                        Object[] parameters = new Object[1];
                        parameters[0] = value;
                        currRange.GetType().InvokeMember("Value2", BindingFlags.SetProperty, null, currRange, parameters);
                    }
                    currRange = FindNext(range, currRange);
                    if (currRange != null)
                    {
                        int currRow = FindRowIndex(currRange);
                        int currCol = FindColIndex(currRange);

                        if (currRow>-1 && currCol>-1 && currRow == firstRow && currCol == firstCol)
                        {
                            releaseNativeReference(currRange);
                            currRange = null;
                        }
                    }
                }

                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
            }
            catch { }
        }

        public object FindRange(string key)
        {
            object cells = range.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, range, new object[] { 1, 1 });
            if (cells == null) return null;
            return Find(range, cells, key);
        }

        public int FindRowIndex(object cell)
        {
            if (cell == null) return -1;
            object row = cell.GetType().InvokeMember("Row", BindingFlags.GetProperty, null, cell, null);
            if (row == null) return -1;
            return (int)row;
        }

        public int FindColIndex(object cell)
        {
            if (cell == null) return -1;
            object row = cell.GetType().InvokeMember("Column", BindingFlags.GetProperty, null, cell, null);
            if (row == null) return -1;
            return (int)row;
        }

        private object FindNext(object range, object after)
        {
            object[] parameters = new object[] { after };
            return range.GetType().InvokeMember("FindNext", BindingFlags.InvokeMethod, null, range, parameters);
        }

        private object Find(object range, object after, object findWhat)
        {
            try
            {
                object[] parameters = new object[]{
                            findWhat,
                            after,
                            XlFindLookIn.xlValues,
                            XlLookAt.xlPart,
                            Type.Missing,
                            XlSearchDirection.xlNext,
                            false,
                            Type.Missing,
                            Type.Missing
                };
                return range.GetType().InvokeMember("Find", BindingFlags.InvokeMethod, null, range, parameters);
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
        private object GetUsedRange(object allCells)
        {
            object rangedCells = null;

            if (allCells != null)
            {
                object lastCell = null;

                try
                {
                    // Get the last cell to find out the number of
                    // columns and rows
                    //lastCell = allCells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
                    object[] parameters = new object[2];
                    parameters[0] = 11;
                    parameters[1] = Type.Missing;
                    lastCell = allCells.GetType().InvokeMember("SpecialCells", BindingFlags.InvokeMethod, null, allCells, parameters);

                    // Get the last row and column information
                    //rowCount = lastCell.Row;
                    rowCount = (int)lastCell.GetType().InvokeMember("Row", BindingFlags.GetProperty, null, lastCell, null);
                    //columnCount = lastCell.Column;
                    columnCount = (int)lastCell.GetType().InvokeMember("Column", BindingFlags.GetProperty, null, lastCell, null);

                    lastCell = null;

                    if ((rowCount > 0) && (columnCount > 0))
                    {
                        // find the used range
                        // User range string should be something like "A1:D4" etc.
                        // To find the last cell index, we do the following thing.
                        string lastColumn = GetLastColumnName(columnCount);

                        parameters[0] = "A1";
                        parameters[1] = lastColumn + rowCount.ToString(); ;
                        rangedCells = ActiveSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, null, ActiveSheet, parameters);
                    }
                }
                catch
                {
                    return null;
                }
            }
            return rangedCells;
        }

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

        private void copy(string fileSource, string fileDestination)
        {

            int i;
            FileStream fin;
            FileStream fout;

            try
            {
                // open input file 
                try
                {
                    fin = new FileStream(fileSource, FileMode.Open, FileAccess.Read);
                }
                catch
                {
                    return;
                }

                // open output file 
                try
                {
                    fout = new FileStream(fileDestination, FileMode.CreateNew, FileAccess.Write);
                }
                catch
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            // Copy File 
            try
            {
                do
                {
                    i = fin.ReadByte();
                    if (i != -1) fout.WriteByte((byte)i);
                } while (i != -1);
            }
            catch
            {
            }


            fin.Close();
            fout.Close();
            fin.Dispose();
            fout.Dispose();
            fin = null;
            fout = null;
        }

        public void SetWidth(int colIndex, int value)
        {
            //Worksheet sheet1 = (Worksheet)book.Worksheets.get_Item(1);
            //((Range)sheet1.Columns["C", Type.Missing]).ColumnWidth = 30;
            string colName = MapColIndexToColName(colIndex);
            object objRange = ActiveSheet.GetType().InvokeMember("Columns", BindingFlags.GetProperty, null, ActiveSheet, new object[] { colName, Type.Missing });
            objRange.GetType().InvokeMember("ColumnWidth", BindingFlags.SetProperty, null, objRange, new object[] { pixel2WidthUnits(value) });
            //objRange.GetType().InvokeMember("AutoFit", BindingFlags.SetProperty, null, objRange, new object[] { 1 });

        }

        public static int EXCEL_COLUMN_WIDTH_FACTOR = 256; 
        public static int UNIT_OFFSET_LENGTH = 7; 
        public static int[] UNIT_OFFSET_MAP = new int[] { 0, 36, 73, 109, 146, 182, 219 };

        private int pixel2WidthUnits(int pxs)
        {
            return pxs/10 + 5;
        } 

        public string MapColIndexToColName(int idx)
        {
            return char.ConvertFromUtf32(idx + 64);
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

        private object get_Range(int rowIndex, int colIndex)
        {
            try
            {
                Object[] parameters = new Object[2];
                parameters[0] = rowIndex;
                parameters[1] = colIndex;
                return ActiveSheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, ActiveSheet, parameters);
            }
            catch {
                return null;
            }
        }

        private object get_Range(string cell1, string cell2)
        {
            try
            {
                string sRange = cell1 + (string.IsNullOrEmpty(cell2) ? "" : ":" + cell2);
                Object[] parameters = new Object[1];
                parameters[0] = sRange;
                return ActiveSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, System.Type.DefaultBinder, ActiveSheet, parameters);
            }
            catch
            {
                return null;
            }
        }

        public void Border_Range(string cell1, string cell2)
        {
            object range = get_Range(cell1, cell2);
            if (range != null)
            {
                object border = range.GetType().InvokeMember("Borders", BindingFlags.GetProperty, null, range, null);

                border.GetType().InvokeMember("LineStyle", BindingFlags.SetProperty, null, border, new object[] { XlLineStyle.xlContinuous });

                border.GetType().InvokeMember("Weight", BindingFlags.SetProperty, null, border, new object[] { XlBorderWeight.xlThin });

                border.GetType().InvokeMember("ColorIndex", BindingFlags.SetProperty, null, border, new object[] { XlColorIndex.xlColorIndexAutomatic });
            }
        }

        public void SetFont(string cell1, string cell2, System.Drawing.Font font)
        {
            object range = get_Range(cell1, cell2);
            if (range != null)
            {
                object objFont = range.GetType().InvokeMember("Font", BindingFlags.GetProperty, null, range, null);
                objFont.GetType().InvokeMember("Strikethrough", BindingFlags.SetProperty, null, objFont, new object[] { font.Strikeout });
                objFont.GetType().InvokeMember("Size", BindingFlags.SetProperty, null, objFont, new object[] { font.Size });
                objFont.GetType().InvokeMember("Italic", BindingFlags.SetProperty, null, objFont, new object[] { font.Italic });
                objFont.GetType().InvokeMember("Bold", BindingFlags.SetProperty, null, objFont, new object[] { font.Bold });
                objFont.GetType().InvokeMember("Name", BindingFlags.SetProperty, null, objFont, new object[] { font.Name });
                objFont.GetType().InvokeMember("Underline", BindingFlags.SetProperty, null, objFont, new object[] { font.Underline });
            }
        }

        public void SetWrapText(string cell1, string cell2, bool wr)
        {
            object range = get_Range(cell1, cell2);
            if (range != null)
            {
                range.GetType().InvokeMember("WrapText", BindingFlags.SetProperty, null, range, new object[] { wr });
            }
        }

        public void Range_Insert(int rowIndex, int countRow)
        {
            {
                m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Object[] parameters = new Object[2];

                parameters = new Object[2];
                parameters[0] = rowIndex;
                parameters[1] = System.Reflection.Missing.Value;
                Object objRange = ActiveSheet.GetType().InvokeMember("Rows", BindingFlags.GetProperty, null, ActiveSheet, parameters);

                for (int i = 1; i < countRow; i++)
                {
                    parameters = new Object[2];
                    parameters[0] = System.Reflection.Missing.Value;
                    parameters[1] = 1;
                    objRange.GetType().InvokeMember("Insert", BindingFlags.InvokeMethod, null, objRange, parameters);
                }

                Thread.CurrentThread.CurrentCulture = m_CurrentCulture;
                releaseNativeReference(objRange);
            }
        }

        public void Write_to_excel_color(int rowIndex, int colIndex, System.Drawing.Color color)
        {
            m_CurrentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Object objRange = get_Range(rowIndex, colIndex);

            Object objInterior = objRange.GetType().InvokeMember("Interior", BindingFlags.GetProperty, null, objRange, null);

            Object[] parameters = new Object[1];

            parameters[0] = System.Drawing.ColorTranslator.ToOle(color);
            objInterior.GetType().InvokeMember("Color", BindingFlags.SetProperty, null, objInterior, parameters);
            Thread.CurrentThread.CurrentCulture = m_CurrentCulture;

            releaseNativeReference(objRange);
            releaseNativeReference(objInterior);
        }
    }

    public enum XlFindLookIn
    {
        xlValues = -4163,
        xlComments = -4144,
        xlFormulas = -4123,
    }

    public enum XlSearchDirection
    {
        xlNext = 1,
        xlPrevious = 2,
    }

    public enum XlLookAt
    {
        xlWhole = 1,
        xlPart = 2,
    }

    public enum XlBordersIndex
    {
        xlDiagonalDown = 5,
        xlDiagonalUp = 6,
        xlEdgeLeft = 7,
        xlEdgeTop = 8,
        xlEdgeBottom = 9,
        xlEdgeRight = 10,
        xlInsideVertical = 11,
        xlInsideHorizontal = 12,
    }

    public enum XlLineStyle
    {
        xlLineStyleNone = -4142,
        xlDouble = -4119,
        xlDot = -4118,
        xlDash = -4115,
        xlContinuous = 1,
        xlDashDot = 4,
        xlDashDotDot = 5,
        xlSlantDashDot = 13,
    }

    public enum XlBorderWeight
    {
        xlMedium = -4138,
        xlHairline = 1,
        xlThin = 2,
        xlThick = 4,
    }

    public enum XlColorIndex
    {
        xlColorIndexNone = -4142,
        xlColorIndexAutomatic = -4105,
    }
}
