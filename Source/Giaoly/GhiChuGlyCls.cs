using System;
using System.Collections.Generic;
using System.Text;

namespace Giaoly
{
    class GhiChuGlyCls
    {
        private int idGiaoDan = 0;

        public int MaGiaoDan
        {
            get { return idGiaoDan; }
            set
            {
                idGiaoDan = value;
            }
        }

        private bool hoanThanh = false;

        public bool HoanThanh
        {
            get { return hoanThanh; }
            set
            {
                hoanThanh = value;
            }
        }

        private string ghiChu = "";

        public string GhiChu
        {
            get { return ghiChu; }
            set
            {
                ghiChu = value;
            }
        }
    }

    
}
