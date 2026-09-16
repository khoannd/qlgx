using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoUpdate
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // GiaoXu.exe truyen ma tien trinh (PID) cua chinh no lam tham so dau tien, de
            // frmProcess biet chinh xac can doi tien trinh nao thoat het truoc khi ghi de
            // file - xem frmProcess.cs de biet ly do. Neu khong co tham so (vi du ai do tu
            // chay AutoUpdate.exe truc tiep), PidTienTrinhGoi giu nguyen null va frmProcess
            // tu dong lui ve cach do tim cu.
            frmProcess frm = new frmProcess();
            if (args.Length > 0)
            {
                int pid;
                if (int.TryParse(args[0], out pid)) frm.PidTienTrinhGoi = pid;
            }
            Application.Run(frm);
        }
    }
}