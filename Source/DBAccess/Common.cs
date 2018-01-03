using System;
using System.Collections.Generic;
using System.Text;

namespace GxGlobal
{
    public class Utils
    {
        public static int MeasureString(string s, System.Windows.Forms.Control ctl)
        {
            System.Drawing.Graphics g = ctl.CreateGraphics();
            int w = 0;
            for (int i = 0; i < s.Length; i++)
            {
                w += (int)g.MeasureString(s.Substring(i, 1), ctl.Font).Width;
            }
            return w;
        }
    }
}
