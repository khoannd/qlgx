using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Word = Microsoft.Office.Interop.Word;
using System.Globalization;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Drawing;
using Microsoft.Office.Interop.Word;

namespace GxGlobal
{
    /// <summary>
    /// Work with Word Application
    /// </summary>
    public class WordEngine
    {
        //2010.05.15 Khoan add start
        #region EVENT
        public event EventHandler OnBeforeSave;
        public event EventHandler OnQuit;
        public event EventHandler OnOpen;
        #endregion
        //2010.05.15 Khoan add end
        private object oMissing = Missing.Value;
        private ApplicationClass WordApp = null;
        private Documents WordDocs = null;
        private Document WordDoc = null;
        //private Selection CurrentSelection;
        public Selection CurrentSelection
        {
            get { return WordApp.Application.Selection; }
        }

        private bool allowVisible = false;

        public bool AllowVisible
        {
            get { return allowVisible; }
            set { allowVisible = value; }
        }

        private string _fileName = "";

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        private Table table;

        object oEndOfDoc = "\\endofdoc";
        public static string _filedestination_temp = "";
        private CultureInfo cultureinfo = Thread.CurrentThread.CurrentCulture;
        private object fal = (object)false;
        private object tru = true;
        /// <summary>
        /// Create new word object from template file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileTemplate"></param>
        /// <returns></returns>
        public bool CreateObject(string fileName, string fileTemplate)
        {
            try
            {
                string file = Path.GetFileName(fileName);
                string path = Path.GetDirectoryName(fileName);
                string fileNameTemp = Path.Combine(path, file);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                if (File.Exists(fileNameTemp))
                {
                    File.Delete(fileNameTemp);
                }
                copy(fileTemplate, fileNameTemp);

                _fileName = fileName;
                _filedestination_temp = fileNameTemp;


                WordApp = new ApplicationClass();
                WordDocs = WordApp.Documents;
                cultureinfo = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                //2010.05.15 Khoan add start
                WordApp.ApplicationEvents2_Event_DocumentBeforeSave += new ApplicationEvents2_DocumentBeforeSaveEventHandler(WordApp_ApplicationEvents2_Event_DocumentBeforeSave);
                WordApp.ApplicationEvents2_Event_Quit += new ApplicationEvents2_QuitEventHandler(WordApp_ApplicationEvents2_Event_Quit);
                WordApp.ApplicationEvents2_Event_DocumentOpen += new ApplicationEvents2_DocumentOpenEventHandler(WordApp_ApplicationEvents2_Event_DocumentOpen);
                //WordApp.Visible = allowVisible;
                //2010.05.15 Khoan add end

                object _pathFile = (object)fileNameTemp;
                WordDoc = WordDocs.Open(ref _pathFile, ref fal, ref fal, ref fal, ref oMissing,
                                        ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                        ref fal, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
             
                WordDoc.Activate();
                //CurrentSelection = WordDoc.Application.Selection;

                Thread.CurrentThread.CurrentCulture = cultureinfo;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create new word object from template file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileTemplate"></param>
        /// <returns></returns>
        public bool CreateObject(string fileName)
        {
            try
            {
                _fileName = fileName;

                WordApp = new ApplicationClass();
                WordDocs = WordApp.Documents;
                cultureinfo = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                //2010.05.15 Khoan add start
                WordApp.ApplicationEvents2_Event_DocumentBeforeSave += new ApplicationEvents2_DocumentBeforeSaveEventHandler(WordApp_ApplicationEvents2_Event_DocumentBeforeSave);
                WordApp.ApplicationEvents2_Event_Quit += new ApplicationEvents2_QuitEventHandler(WordApp_ApplicationEvents2_Event_Quit);
                WordApp.ApplicationEvents2_Event_DocumentOpen += new ApplicationEvents2_DocumentOpenEventHandler(WordApp_ApplicationEvents2_Event_DocumentOpen);
                //WordApp.Visible = allowVisible;
                //2010.05.15 Khoan add end

                object _pathFile = (object)fileName;
                WordDoc = WordDocs.Open(ref _pathFile, ref fal, ref fal, ref fal, ref oMissing,
                                        ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                        ref fal, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                if (allowVisible)
                {
                    WordDoc.Activate();
                }
                //CurrentSelection = WordDoc.Application.Selection;

                Thread.CurrentThread.CurrentCulture = cultureinfo;
            }
            catch (Exception)
            {
                Quit(false);
                return false;
            }
            return true;
        }

        //2010.05.15 Khoan add start
        /// <summary>
        /// When word doc is opening
        /// </summary>
        /// <param name="Doc"></param>
        private void WordApp_ApplicationEvents2_Event_DocumentOpen(Document Doc)
        {
            if (OnOpen != null) OnOpen(Doc, EventArgs.Empty);
        }
        /// <summary>
        /// When word app is quiting
        /// </summary>
        private void WordApp_ApplicationEvents2_Event_Quit()
        {
            if (OnQuit != null) OnQuit(WordApp, EventArgs.Empty);
        }
        /// <summary>
        /// When word app is saving
        /// </summary>
        /// <param name="Doc"></param>
        /// <param name="SaveAsUI"></param>
        /// <param name="Cancel"></param>
        private void WordApp_ApplicationEvents2_Event_DocumentBeforeSave(Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            if (OnBeforeSave != null) OnBeforeSave(Doc, EventArgs.Empty);
        }
        //2010.05.15 Khoan add end

        /// <summary>
        /// Copy file from source to destination
        /// </summary>
        private void copy(string fileSource, string fileDestination)
        {

            try
            {
                File.Copy(fileSource, fileDestination, true);
                FileInfo file = new FileInfo(fileDestination);
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Release COM object
        /// </summary>
        /// <param name="o"></param>
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

        /// <summary>
        /// Open result excel file on excel application
        /// </summary>
        public void ViewFile()
        {
            //if (System.IO.File.Exists(_filedestination_temp))
            //{
            //    System.IO.File.Delete(_filedestination_temp);
            //}
            _filedestination_temp = "";
            System.Diagnostics.Process.Start(_fileName);
        }
        

        /// <summary>
        /// Delete temporary file
        /// </summary>
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

        /// <summary>
        /// If AllowVisible is false, finish write data to excel and release all COM objects. 
        /// If AllowVisible is true, show word document.
        /// </summary>
        public void End_Write()
        {
            try
            {
                if (!allowVisible)
                {
                    Quit(true);
                }
                else
                {
                    if (WordDoc != null)
                    {
                        foreach (Table t in WordDoc.Tables)
                        {
                            t.AllowPageBreaks = false;
                        }
                    }
                    WordApp.Visible = true;
                }
            }
            catch (Exception)
            {

            }
        }
        public void Visible()
        {
            try
            {
                WordApp.Visible = true;
            }
            catch (Exception)
            {
            }
        }
        public void Save()
        {
            WordDoc.Save();
        }

        public void Quit(bool saveChange)
        {
            try
            {
                cultureinfo = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                if (WordDoc != null)
                {
                    object oSaveChange = saveChange;
                    WordDoc.Close(ref oSaveChange, ref oMissing, ref oMissing);

                    WordApp.Quit(ref oSaveChange, ref oMissing, ref oMissing);
                }
                else
                {
                    if (OnQuit != null) OnQuit(null, EventArgs.Empty);
                }
                releaseNativeReference(WordDoc);
                releaseNativeReference(WordDocs);
                releaseNativeReference(WordApp);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.CurrentThread.CurrentCulture = cultureinfo;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Write Text In Word Document
        /// </summary>
        public void WriteText(string text)
        {
            CurrentSelection.TypeText(text);
        }
        /// <summary>
        /// Delete line
        /// </summary>
        /// <param name="count"></param>
        public void Delete(int count)
        {
            try
            {
                object c = count;
                object oUnit = Word.WdUnits.wdWord;
                CurrentSelection.Delete(ref oUnit, ref c);
            }
            catch
            {
            }
        }
        /// <summary>
        /// Remove background by add new line and delete current line
        /// </summary>
        public void RemoveBackgroundLastParagraph()
        {
            this.WriteNewLine();

            //Move up
            object oUnit = Word.WdUnits.wdLine;
            object oMove = Word.WdMovementType.wdMove;
            object oCount = 1;
            CurrentSelection.MoveUp(ref oUnit, ref oCount, ref oMove);

            this.Delete(1);
        }

        /// <summary>
        /// Write New line In Word Document
        /// </summary>
        public void WriteNewLine()
        {
            CurrentSelection.TypeParagraph();
        }

        /// <summary>
        /// Move next line
        /// </summary>
        public void MoveDown(int count)
        {
            try
            {
                object oUnit = Word.WdUnits.wdLine;
                object oMove = Word.WdMovementType.wdMove;
                object oCount = 1;
                WordDoc.Tables[WordDoc.Tables.Count].Rows[WordDoc.Tables[WordDoc.Tables.Count].Rows.Count].Select();

                CurrentSelection.MoveDown(ref oUnit, ref oCount, ref oMove);

                //object oUnit = Word.WdUnits.wdStory;
                //object oMove = Word.WdMovementType.wdMove;
                //CurrentSelection.EndKey(ref oUnit, ref oMove);
                //CurrentSelection.InsertParagraphAfter();
                //CurrentSelection.EndKey(ref oUnit, ref oMove);
            }
            catch (Exception)
            {

            }

        }
        public void MoveEnd()
        {
            try
            {
                //WordDoc.Characters.Last.Select();
                object lastPosition = WordDoc.Content.StoryLength - 1;
                var range = WordDoc.Range(ref lastPosition, ref lastPosition);
                range.Select();
            }
            catch (Exception)
            {

            }
        }
        public void InsertPage()
        {
            try
            {
                WordDoc.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            }
            catch (Exception)
            {

            }
        }
        public void SelectAll()
        {
            try
            {
                CurrentSelection.Document.Content.Select();
            }
            catch (Exception)
            {

            }
        }
        public void Copy()
        {
            try
            {
                CurrentSelection.Copy();
            }
            catch (Exception)
            {

            }
        }
        public void Paste()
        {
            try
            {
                CurrentSelection.Paste();
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// Move next line
        /// </summary>
        public void MoveOutTable(Table oTable)
        {
            try
            {
                object oUnit = Word.WdUnits.wdLine;
                object oMove = Word.WdMovementType.wdMove;
                object oCount = 1;
                oTable.Rows[oTable.Rows.Count].Select();
                CurrentSelection.MoveDown(ref oUnit, ref oCount, ref oMove);

                oTable.Rows[oTable.Rows.Count].Select();
                CurrentSelection.MoveDown(ref oUnit, ref oCount, ref oMove);

                //object oUnit = Word.WdUnits.wdStory;
                //object oMove = Word.WdMovementType.wdMove;
                //CurrentSelection.EndKey(ref oUnit, ref oMove);
                //CurrentSelection.InsertParagraphAfter();
                //CurrentSelection.EndKey(ref oUnit, ref oMove);
            }
            catch (Exception)
            {

            }

        }
        /// <summary>
        /// Move next line
        /// </summary>
        public void MoveUp(int count)
        {
            try
            {
                object oUnit = Word.WdUnits.wdLine;
                object oMove = Word.WdMovementType.wdMove;
                object oCount = count;
                CurrentSelection.MoveUp(ref oUnit, ref oCount, ref oMove);
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Write New line At Bookmark In Word Document
        /// </summary>
        public void WriteNewLineAtBookmark(string BookMarkName)
        {
            object missing = System.Reflection.Missing.Value;

            try
            {
                if (WordDoc.Bookmarks.Exists(BookMarkName))
                {
                    //object oBookMark = (object)BookMarkName;
                    //WordDoc.Bookmarks.get_Item(ref oBookMark).Select();
                    SelectBookmark(BookMarkName);
                    CurrentSelection.TypeParagraph();
                    //.Range.Text = BookMarkText;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }
        }

        public void InsertBookmarkToCell_Table(Table oTableTemp, int iRow, int iCol, string strBookmark)
        {
            oTableTemp.Cell(iRow, iCol).Select();
            WordDoc.Bookmarks.Add(strBookmark, ref oMissing);
        }

        public Table InsertTable_AtCellParentTable(Table oParentTable, int iRow, int iCol, int iRowChild, int iColChild)
        {
            //Create Table
            return WordDoc.Tables.Add(oParentTable.Cell(iRow, iCol).Range, iRowChild, iColChild, ref oMissing, ref oMissing);
        }

        /// <summary>
        /// Write Text In Word Document
        /// </summary>
        public void WriteText(string text, bool writeEnterBeforeText, bool writeEnterAfterText)
        {
            if (writeEnterBeforeText)
            {
                CurrentSelection.TypeParagraph();
            }
            CurrentSelection.TypeText(text);
            if (writeEnterAfterText)
            {
                CurrentSelection.TypeParagraph();
            }
        }

        /// <summary>
        /// Write Text In Word Document
        /// </summary>
        public void WriteTextWithShading(string text, WdColor bgColor)
        {
            CurrentSelection.TypeText(text);
            CurrentSelection.Shading.BackgroundPatternColor = bgColor;
        }

        /// <summary>
        /// Write Text In Word Document. Used for Proposal
        /// </summary>
        public void WriteTextWithShadingProposal(string text, WdColor bgColor)
        {
            CurrentSelection.TypeText(text);
            CurrentSelection.Shading.BackgroundPatternColor = bgColor;
            CurrentSelection.ParagraphFormat.RightIndent = -8;
        }

        /// <summary>
        /// Set font default for document
        /// </summary>
        public void SetDefaultFontFormat(int FontSize, bool isBold)
        {
            CurrentSelection.ClearFormatting();
            CurrentSelection.Font.Name = "Calibri";
            CurrentSelection.Font.Size = FontSize;
            if (isBold)
            {
                CurrentSelection.Font.Bold = 1;
            }
            else
            {
                CurrentSelection.Font.Bold = 0;
            }
            CurrentSelection.Font.Color = (WdColor)Microsoft.VisualBasic.Information.RGB(0, 0, 128);
            CurrentSelection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            CurrentSelection.ParagraphFormat.LineSpacing = 10;
            CurrentSelection.ParagraphFormat.SpaceAfter = 0;
            CurrentSelection.ParagraphFormat.SpaceBefore = 0;

        }
        //2012.03.21 Thanh add start
        /// <summary>
        /// Set font default for document
        /// </summary>
        public void SetDefaultFontFormat(int FontSize, bool isBold, WdColor color)
        {
            CurrentSelection.ClearFormatting();
            CurrentSelection.Font.Name = "Calibri";
            CurrentSelection.Font.Size = FontSize;
            if (isBold)
            {
                CurrentSelection.Font.Bold = 1;
            }
            else
            {
                CurrentSelection.Font.Bold = 0;
            }
            CurrentSelection.Font.Color = color;
            CurrentSelection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            CurrentSelection.ParagraphFormat.LineSpacing = 10;
            CurrentSelection.ParagraphFormat.SpaceAfter = 0;
            CurrentSelection.ParagraphFormat.SpaceBefore = 0;

        }
        /// <summary>
        /// get word color
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public WdColor GetWordColor(int red, int green, int blue)
        {
            return (WdColor)Microsoft.VisualBasic.Information.RGB(red, green, blue);
        }

        //2012.03.21 Thanh add end
        /// <summary>
        /// Write Table In Word Document
        /// </summary>
        public Table WriteTable(Range range, int numberRow, int numberColumn)
        {
            table = WordDoc.Tables.Add(range, numberRow, numberColumn, ref oMissing, ref oMissing);
            formatTable(table);
            return table;
        }

        /// <summary>
        /// Border range from cell1 to cell2
        /// </summary>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        public void Border_Range(Table tableTemp)
        {
            object oStyle = "Table Grid";
            tableTemp.set_Style(ref oStyle);
        }

        /// <summary>
        /// Shading range from one range
        /// </summary>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        public void Shading_row(Table range, int rowPosition, WdColor color)
        {

            range.Rows[rowPosition].Shading.BackgroundPatternColor = color;
        }

        /// <summary>
        /// Border range from one range
        /// </summary>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        public void Border_row(Table range, int rowPosition, WdColor color)
        {
            range.Rows[rowPosition].Borders.OutsideLineStyle = WdLineStyle.wdLineStyleSingle;
            range.Rows[rowPosition].Borders.OutsideLineWidth = WdLineWidth.wdLineWidth100pt;
            range.Rows[rowPosition].Borders.OutsideColor = color;
        }

        /// <summary>
        /// Border all rows of table with specific color
        /// </summary>
        /// <param name="oTable"></param>
        /// <param name="color"></param>
        public void Border_Table(Table oTable, WdColor color)
        {
            //for (int i = 1; i <= oTable.Rows.Count; i++)
            //{
            //    Border_row(oTable, i, color);
            //}
            //oTable.Rows.Borders.InsideColor = color;
            oTable.Rows.Borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows.Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows.Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows.Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows.Borders[WdBorderType.wdBorderHorizontal].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows.Borders[WdBorderType.wdBorderVertical].LineStyle = WdLineStyle.wdLineStyleSingle;

            oTable.Rows.Borders[WdBorderType.wdBorderLeft].Color = color;
            oTable.Rows.Borders[WdBorderType.wdBorderTop].Color = color;
            oTable.Rows.Borders[WdBorderType.wdBorderRight].Color = color;
            oTable.Rows.Borders[WdBorderType.wdBorderBottom].Color = color;
            oTable.Rows.Borders[WdBorderType.wdBorderHorizontal].Color = color;
            oTable.Rows.Borders[WdBorderType.wdBorderVertical].Color = color;
        }
        /// <summary>
        /// Border top line
        /// </summary>
        /// <param name="oTable"></param>
        /// <param name="row"></param>
        /// <param name="color"></param>
        public void Border_Row_OnTop(Table oTable, int row, WdColor color)
        {
            oTable.Rows[row].Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleSingle;
            oTable.Rows[row].Borders[WdBorderType.wdBorderTop].Color = color;
        }

        public void RemoveTopBorder(Table oTable, int row)
        {
            oTable.Rows[row].Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
        }

        /// <summary>
        /// Get Range End Word
        /// </summary>
        /// <returns></returns>
        public Range GetRangeEndWord()
        {
            Range range = WordDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            return range;
        }

        public void SetSelectionEndWord()
        {
            WordDoc.Bookmarks.get_Item(ref oEndOfDoc).Select();
        }

        /// <summary>
        /// write text in table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="text"></param>
        public void WriteTextInTable(Table table, int row, int col, string text)
        {
            table.Cell(row, col).Range.Text = text;
        }

        /// <summary>
        /// write text in table with single line paragraph
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="text"></param>
        public void WriteTextInTableSingleLine(Table table, int row, int col, string text)
        {
            table.Cell(row, col).Range.Text = text;
            CurrentSelection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            CurrentSelection.ParagraphFormat.LineSpacing = 10;
            CurrentSelection.ParagraphFormat.SpaceAfter = 0;
            CurrentSelection.ParagraphFormat.SpaceBefore = 0;
        }

        public void WriteTableAtSelection(int row, int col, int rowInsertText, int colInsertText, string text)
        {
            Table tbl = CurrentSelection.Tables.Add(GetRangeEndWord(), row, col, ref oMissing, ref oMissing);
            formatTable(tbl);
            tbl.Cell(rowInsertText, colInsertText).Range.Text = text;
        }
        //2011.04.22 Minh Add Start
        public void WriteTextWithDefaultBullet(string text)
        {
            try
            {
                WordApp.Visible = false;
                CurrentSelection.Range.ParagraphFormat.LeftIndent = -1;
                CurrentSelection.Range.ListFormat.ApplyBulletDefault(ref oMissing);
                CurrentSelection.Range.Text = text;
                CurrentSelection.Range.InsertParagraphBefore();
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }
        }
        //2011.04.22 Minh Add End
        private void formatTable(Table table)
        {
            for (int i = 1; i <= table.Rows.Count; i++)
            {
                table.Rows[i].AllowBreakAcrossPages = (int)Microsoft.Office.Core.MsoTriState.msoFalse;
            }
        }

        public void WriteOneRow(string text)
        {
            Table tbl = CurrentSelection.Tables.Add(GetRangeEndWord(), 1, 1, ref oMissing, ref oMissing);
            tbl.AllowAutoFit = true;
            object oStyle = "Table Grid";
            tbl.set_Style(ref oStyle);
            tbl.Cell(1, 1).Range.Text = text;
        }

        /// <summary>
        /// Searches the bookmark by name and places text at the text
        /// </summary>
        /// <param name="BookMarkName"></param>
        /// <param name="BookMarkText"></param>
        public void WriteToBookMark(string BookMarkName, string BookMarkText)
        {
            object missing = System.Reflection.Missing.Value;

            try
            {
                if (WordDoc.Bookmarks.Exists(BookMarkName))
                {
                    object oBookMark = (object)BookMarkName;
                    WordDoc.Bookmarks.get_Item(ref oBookMark).Range.Text = BookMarkText;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }
        }
        //2012.02.06 Thanh add start
        public bool checkBookMark(string BookMarkName)
        {
            bool result = false;
            try
            {
                if (WordDoc.Bookmarks.Exists(BookMarkName))
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return false;
            }
        }
        //2012.02.06 Thanh add end
        /// <summary>
        /// Searches the bookmark by name and places text at the text
        /// </summary>
        /// <param name="BookMarkName"></param>
        /// <param name="BookMarkText"></param>
        public void WriteToBookMark(string BookMarkName, string BookMarkText, WdColor bgColor)
        {
            object missing = System.Reflection.Missing.Value;

            try
            {
                if (WordDoc.Bookmarks.Exists(BookMarkName))
                {
                    object oBookMark = (object)BookMarkName;
                    Range oRange = WordDoc.Bookmarks.get_Item(ref oBookMark).Range;
                    oRange.Text = BookMarkText;
                    oRange.Shading.BackgroundPatternColor = bgColor;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }
        }

        /// <summary>
        /// Write Table In Word Document
        /// </summary>
        public Table WriteTableToBookMark(string BookMarkName, int numberRow, int numberColumn)
        {
            Range range;

            object missing = System.Reflection.Missing.Value;

            try
            {
                if (WordDoc.Bookmarks.Exists(BookMarkName))
                {
                    object oBookMark = (object)BookMarkName;
                    range = WordDoc.Bookmarks.get_Item(ref oBookMark).Range;
                }
                else
                {
                    range = this.GetRangeEndWord();
                }

                table = WordDoc.Tables.Add(range, numberRow, numberColumn, ref oMissing, ref oMissing);
                table.AllowPageBreaks = false;
                formatTable(table);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }

            return table;
        }

        public void SelectBookmark(string bookMarkName)
        {
            Range range = null;

            object missing = System.Reflection.Missing.Value;

            try
            {
                if (WordDoc.Bookmarks.Exists(bookMarkName))
                {
                    object oBookMark = (object)bookMarkName;
                    range = WordDoc.Bookmarks.get_Item(ref oBookMark).Range;
                }
                else
                {
                    //range = this.GetRangeEndWord();//if not exist then no need to select end of word
                }
                if (range != null)
                {
                    range.Select();
                }

            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                String err = ex.Message;
            }
        }

        public void MergeCellTable(Table table, int iRow)
        {
            try
            {
                table.Rows[iRow].Cells.Merge();
                //table.Rows[]
            }
            catch
            {
                //
            }
        }

        public void MergeCellTable(Table table, int iRow1, int iCol1, int iRow2, int iCol2)
        {
            try
            {
                //table.Rows[iRow].Cells.Merge();
                //table.Rows[]

                Cell cell1 = table.Cell(iRow1, iCol1);
                Cell cell2 = table.Cell(iRow2, iCol2);

                //oTable.Rows[1].Cells[1].Merge(cell2);
                cell1.Merge(cell2);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
        }

        public void DeleteRowTable(Table table, int iRow)
        {
            table.Rows[iRow].Delete();
        }

        public static int ConvertDocToPDF(object Source, object Target)
        {
            Microsoft.Office.Interop.Word.ApplicationClass MSdoc;
            object Unknown = Type.Missing;
            //Creating the instance of Word Application          
            MSdoc = new Microsoft.Office.Interop.Word.ApplicationClass();

            try
            {
                MSdoc.Visible = false;
                MSdoc.Documents.Open(ref Source, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown, ref Unknown, ref Unknown);
                MSdoc.Application.Visible = false;
                MSdoc.WindowState = Microsoft.Office.Interop.Word.WdWindowState.wdWindowStateMinimize;

                object format = WdSaveFormat.wdFormatPDF;

                MSdoc.ActiveDocument.SaveAs(ref Target, ref format,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                       ref Unknown, ref Unknown);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return -1;
            }
            finally
            {
                if (MSdoc != null)
                {
                    MSdoc.Documents.Close(ref Unknown, ref Unknown, ref Unknown);
                    //WordDoc.Application.Quit(ref Unknown, ref Unknown, ref Unknown);
                }
                // for closing the application
                object saveChanges = Type.Missing;
                object originalFormat = Type.Missing;
                object routerDocument = Type.Missing;
                MSdoc.Quit(ref saveChanges, ref originalFormat, ref routerDocument);
            }
            return 0;
        }

        public void Dispose()
        {
            Quit(false);
        }

        public void FormatRowHeader(Table oTable, int row, WdColor color)
        {
            oTable.Rows[row].Height = 20;
            oTable.Rows[row].HeightRule = WdRowHeightRule.wdRowHeightAtLeast;
            oTable.Rows[row].Shading.BackgroundPatternColor = color;
        }

        /// <summary>
        /// Auto fit columns
        /// </summary>
        /// <param name="oTable"></param>
        /// <param name="fromCol"></param>
        /// <param name="toCol"></param>
        public void AutoFitColumn(Table oTable, int fromCol, int toCol)
        {
            for (int i = fromCol; i <= toCol; i++)
            {
                oTable.Columns[i].AutoFit();
            }
            oTable.PreferredWidthType = WdPreferredWidthType.wdPreferredWidthPercent;
            oTable.PreferredWidth = 99;
        }

        /// <summary>
        /// Auto fit columns. Used for proposal
        /// </summary>
        /// <param name="oTable"></param>
        /// <param name="fromCol"></param>
        /// <param name="toCol"></param>
        public void AutoFitColumnProposal(Table oTable, int fromCol, int toCol)
        {
            for (int i = fromCol; i <= toCol; i++)
            {
                oTable.Columns[i].AutoFit();
            }
            oTable.PreferredWidthType = WdPreferredWidthType.wdPreferredWidthPercent;
            oTable.PreferredWidth = 101;
        }

        /// <summary>
        /// Set font default for document
        /// </summary>
        public void SetDefaultFontFormat(int FontSize, bool isBold, bool isItalic)
        {
            CurrentSelection.ClearFormatting();
            CurrentSelection.Font.Name = "Calibri";
            CurrentSelection.Font.Size = FontSize;
            if (isBold)
            {
                CurrentSelection.Font.Bold = 1;
            }
            else
            {
                CurrentSelection.Font.Bold = 0;
            }
            if (isItalic)
            {
                CurrentSelection.Font.Italic = 1;
            }
            else
            {
                CurrentSelection.Font.Italic = 0;
            }
            CurrentSelection.Font.Color = (WdColor)Microsoft.VisualBasic.Information.RGB(0, 0, 128);
            CurrentSelection.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            CurrentSelection.ParagraphFormat.LineSpacing = 10;
            CurrentSelection.ParagraphFormat.SpaceAfter = 0;
            CurrentSelection.ParagraphFormat.SpaceBefore = 0;

        }
        public void Replace(object findWhat, object replaceWith)
        {
            Replace(findWhat, replaceWith, "");
        }

        public void Replace(object findWhat, object replaceWith, object defaultValue)
        {
            //Range range = WordApp.ActiveDocument.Content;


            //object findtext = "z1";
            //object findreplacement = "KLOAKA OBECNA";
            //object findforward = true;
            //object findformat = false;
            //object findwrap = Word.WdFindWrap.wdFindContinue;
            //object findmatchcase = false;
            //object findmatchwholeword = false;
            //object findmatchwildcards = false;
            //object findmatchsoundslike = false;
            //object findmatchallwordforms = false;
            //object findreplace = Word.WdReplace.wdReplaceAll;


            //range.Find.Execute(ref findtext, ref findmatchcase, ref findmatchwholeword, ref findmatchwildcards,
            //ref findmatchsoundslike, ref findmatchallwordforms, ref findforward, ref findwrap,
            //ref findformat, ref findreplacement, ref replace, ref nevim, ref nevim, ref nevim, ref nevim);

            if(replaceWith == null || replaceWith.ToString().Trim() == "")
            {
                replaceWith = defaultValue;
            }
            object missing = System.Type.Missing;
            // Loop through the StoryRanges (sections of the Word doc)
            foreach (Word.Range tmpRange in WordDoc.StoryRanges)
            {
                // Set the text to find and replace
                tmpRange.Find.MatchCase = false;
                tmpRange.Find.Text = string.Concat("[", findWhat, "]");
                tmpRange.Find.Replacement.Text = replaceWith.ToString();
                
                // Set the Find.Wrap property to continue (so it doesn't
                // prompt the user or stop when it hits the end of
                // the section)
                tmpRange.Find.Wrap = Word.WdFindWrap.wdFindContinue;
                
                // Declare an object to pass as a parameter that sets
                // the Replace parameter to the "wdReplaceAll" enum
                object replaceAll = Word.WdReplace.wdReplaceAll;

                // Execute the Find and Replace -- notice that the
                // 11th parameter is the "replaceAll" enum object
                tmpRange.Find.Execute(ref missing, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref missing,
                    ref missing, ref missing, ref missing, ref replaceAll,
                    ref missing, ref missing, ref missing, ref missing);
            }
        }
    }

    public enum WdSaveFormat
    {
        wdFormatDocument97 = 0,
        wdFormatDocument = 0,
        wdFormatTemplate97 = 1,
        wdFormatTemplate = 1,
        wdFormatText = 2,
        wdFormatTextLineBreaks = 3,
        wdFormatDOSText = 4,
        wdFormatDOSTextLineBreaks = 5,
        wdFormatRTF = 6,
        wdFormatEncodedText = 7,
        wdFormatUnicodeText = 7,
        wdFormatHTML = 8,
        wdFormatWebArchive = 9,
        wdFormatFilteredHTML = 10,
        wdFormatXML = 11,
        wdFormatXMLDocument = 12,
        wdFormatXMLDocumentMacroEnabled = 13,
        wdFormatXMLTemplate = 14,
        wdFormatXMLTemplateMacroEnabled = 15,
        wdFormatDocumentDefault = 16,
        wdFormatPDF = 17,
        wdFormatXPS = 18,
        wdFormatFlatXML = 19,
        wdFormatFlatXMLMacroEnabled = 20,
        wdFormatFlatXMLTemplate = 21,
        wdFormatFlatXMLTemplateMacroEnabled = 22,
    }
}

