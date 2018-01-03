using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GxGlobal
{
    public class Dispatcher
    {
        public static void ShowTab(Form frm)
        {
            Form main = Application.OpenForms["frmMain"];
            if (main != null)
            {
                main.GetType().InvokeMember("ShowForm", BindingFlags.InvokeMethod, null, main, new object[] { frm });
            }
        }
    }
}
