using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GxGlobal;

namespace GxControl
{
    public partial class GxPictureField : UserControl
    {
        ContextMenuStrip context;
        public GxPictureField()
        {
            InitializeComponent();
            context = new ContextMenuStrip();
            GetContext(context);
            picAvatar.ContextMenuStrip = context;
            openFileDialog1.Filter = "Image(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
          
            picAvatar.MouseClick += PicAvatar_MouseClick;
        }

        private void PicAvatar_MouseClick(object sender, MouseEventArgs e)
        {
            context.Show(picAvatar,e.Location);
            
        }

    

        private void GetContext(ContextMenuStrip context)
        {
            context.BackColor = Color.FromArgb(240, 240, 240);
            context.Items.Add("Xem hình").Click += SeePictureField_Click; ;
            context.Items.Add("Thay đổi hình đại diện").Click += ChangeGxPictureField_Click;
            context.Items.Add("Xóa hình").Click += ResetPictureField_Click;
        }

        private void ResetPictureField_Click(object sender, EventArgs e)
        {
            fileName = "";
            if(manHinh=="Giáo dân")
            {
                picAvatar.Image = GxControl.Properties.Resources.avatar;
            } 
            else
            {
                picAvatar.Image = GxControl.Properties.Resources.family_hope_cntr_icon;
            }    
        }

        private void ChangeGxPictureField_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                FileInfo img = new FileInfo(openFileDialog1.FileName);
                string name = string.Concat(img.Name.Substring(0, img.Name.IndexOf('.')),DateTime.Now.Ticks,img.Extension);
                this.fileName = string.Concat(@"Images\",name);
                
               
                string dest = string.Concat(Memory.AppPath,fileName);
                try//????
                {
                    File.Copy(img.FullName, dest, true);//ghi đè
                }
                catch(Exception ex)
                {
                   
                }
                picAvatar.Image = Image.FromFile(dest);        
                 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeePictureField_Click(object sender, EventArgs e)
        {
            frmAvatar avatar = new frmAvatar();
            avatar.Source = picAvatar.Image;
            avatar.Show();           
        }

        public Image ImagePicture
        {
            get
            {
                return picAvatar.Image;
            }

            set
            {
                this.picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                picAvatar.Image = value;
            }
        }
        string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
        }
        private string manHinh;
        public string TenManHinh
        {
            set
            {
                manHinh = value;
            }
            get
            {
                return manHinh;
            }
        }
    }
}
