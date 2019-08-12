using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class GxSearchBox : GxTextField
    {
        public GxSearchBox()
        {
            InitializeComponent();
        }

        public List<string> GetKeywords()
        {
            string txt = base.Text;
            List<string> lst = new List<string>();
            if (txt != "")
            {
                string[] arr = null;
                if (txt.Contains("\""))
                {
                    int start = -1; 
                    for (int i = 0; i < txt.Length; i++)
                    {
                        if (txt.Substring(i, 1) == "\"")
                        {
                            if (start == -1)
                            {
                                start = i;
                            }
                            else
                            {
                                int len = i - start;
                                string tmp = txt.Substring(start + 1, len - 1);
                                lst.Add(tmp.Trim());
                                txt = txt.Remove(start, len + 1);
                                start = -1;
                                i = i - 2;
                            }
                        }
                    }
                    txt = txt.Replace("\"", "");

                    //arr = txt.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries);
                    ////Neu co tu 2 dau " tro len
                    //if (arr != null && arr.Length >= 2)
                    //{
                    //    for (int i = 1; i < arr.Length; i++)
                    //    {
                    //        lst.Add(arr[i]);
                    //        for (int j = i; i < arr.Length - 1; j++)
                    //        {
                    //            arr[j] = arr[j + 1];
                    //        }
                    //        Array.Resize<string>(ref arr, arr.Length - 1);
                    //        i--;
                    //    }
                    //    txt = string.Join(" ", arr).Trim();
                    //}
                }
                if (txt.Contains(" "))
                {
                    arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 0)
                    {
                        for (int i = 0; i < arr.Length; i++)
                        {
                            lst.Add(arr[i]);
                        }
                    }
                }
                else if(!string.IsNullOrEmpty(txt))
                {
                    lst.Add(txt);
                }
            }
            return lst;
        }
    }
}
