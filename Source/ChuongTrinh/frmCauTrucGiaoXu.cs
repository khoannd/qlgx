using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GXGlobal;
using System.Threading;

namespace GiaoXu
{
    public partial class frmCauTrucGiaoXu : Form
    {
        private TreeObject tree = new TreeObject();
        public frmCauTrucGiaoXu()
        {
            InitializeComponent();
            tree.OnLoadGiaDinhFinished += new EventHandler(tree_OnLoadGiaDinhFinished);
            tree.OnLoadGiaoDanFinished += new EventHandler(tree_OnLoadGiaoDanFinished);
            tree.OnLoadGiaoHoFinished += new EventHandler(tree_OnLoadGiaoHoFinished);
            treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeView1_NodeMouseClick);
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            switch (e.Node.Level)
            { 
                case 0://giao xu
                    break;
                case 1://giao ho
                    //ThreadStart thstart = new ThreadStart(tree.LoadGiaDinh);
                    //Thread thread = new Thread(thstart);
                    //thread.Start(int.Parse(e.Node.Name));                    
                    break;
                case 2://gia dinh
                    break;
                default:
                    break;
            }
        }

        private void tree_OnLoadGiaoHoFinished(object sender, EventArgs e)
        {
        }

        private void tree_OnLoadGiaoDanFinished(object sender, EventArgs e)
        {
        }

        private void tree_OnLoadGiaDinhFinished(object sender, EventArgs e)
        {
            if (treeView1.InvokeRequired)
            { 

            }
        }

        private void frmCauTrucGiaoXu_Load(object sender, EventArgs e)
        {

        }
    }
}