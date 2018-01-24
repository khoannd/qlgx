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
            context.Items.Add("Xem ảnh").Click += SeePictureField_Click; ;
            context.Items.Add("Thay đổi ảnh đại diện").Click += ChangeGxPictureField_Click;
        }

        private void ChangeGxPictureField_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                FileInfo img = new FileInfo(openFileDialog1.FileName);
                this.fileName = string.Concat(@"Images\", Path.GetFileName(openFileDialog1.FileName));

               
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
        /// ahihih
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeePictureField_Click(object sender, EventArgs e)
        {
            frmAvatar avatar = new frmAvatar();
            avatar.Source = picAvatar.Image;
            avatar.ShowDialog();
            
        }

        public Image ImagePicture
        {
            get
            {
                return picAvatar.Image;
            }

            set
            {
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
    }
}
