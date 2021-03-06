using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertDB.vnConvert
{
    public class ConvertBase
    {
        protected int iCode = 0;
        protected int toCode = 0;
        protected string[] Code;
        protected FontCase m_CharCase = FontCase.Normal;
        protected string SplitFirstChar = " ~`!@#$%^&*()-+=|\\{[]}:;'\"<,>.?/\t\n\r";

        public FontIndex FontType
        {
            get { return (FontIndex)iCode; }
        }

        public ConvertBase(FontIndex ToFontCode)
        {
            toCode = (FontIndex)ToFontCode;
            InitData();
        }

        protected virtual void InitData()
        { }

        public virtual void BeginMapCode(ref string[,] Code)
        { }

        public virtual bool IsVietnamese(String s)
        {
            return false;
        }

        private int GetnChar(FontIndex index)
        {
            switch (index)
            {
                case FontIndex.iUTF:
                    return 3;
                case FontIndex.iUNI:
                case FontIndex.iUTH:
                case FontIndex.iVWF:
                case FontIndex.iNOSIGN:
                case FontIndex.iVISCII:
                case FontIndex.iVPS:
                case FontIndex.iBKHCM1:
                    return 1;
                case FontIndex.iNCR:
                    return 7;
                case FontIndex.iNCRHex:
                    return 8;
                case FontIndex.iCString:
                    return 6;
                default:
                    return 2;
            }
        }

        public virtual bool Convert(ref String strConv)
        {
            int iSrc = iCode;
            int iDest = toCode;
            if (strConv.Trim() == "") return false;
            if (iSrc == iDest) return false;
            String s = "", s1 = "";
            //if (iSrc == (int)FontIndex.iNotKnown) //Nếu không tìm thấy tên mã đưa vào
            //{
            //    //Tự động tìm mã vào dựa theo các ký tự dấu tiếng việt
            //    int intcode = (int)getCode(strConv);
            //    if (intcode > -1) iSrc = intcode;
            //    else return false;
            //}
            ////Mac dinh ma xuat ra la Unicode
            //if (iDest == -1) iDest = 0;
            //Một số mã, 1 ký tự có dấu có thể > 1 ký tự ASNSI
            //mChar là minimun char number, nChar là Maximun char number trong 1 bảng mã
            //Ví dụ: Mã TCVN có thể có 1, hoặc 2 ký tự, khi đó mChar=1, nChar=2
            //Lấy số ký tự lớn nhất có thể tổ hợp cho 1 ký tự có dấu trong mã nguồn
            int nChar = GetnChar((FontIndex)int.Parse(Code[0, iSrc]));
            int mChar = nChar > 1 ? nChar - 1 : 1;
            //CHuỗi chứa kết quả sau khi chuyển đổi
            StringBuilder sResult = new StringBuilder();
            sResult.Append("");
            //Cho biết có sự chuyển đổi xảy ra trên chuỗi đưa vào không.
            bool hasconvert = false;
            //Mã VietwareF có ký tự "̣" = "ò" và "́" = "ì". Ta chuyển nó về chung 1 kiểu để tiện chuyển mã
            if (iCode == (int)FontIndex.iVWF)
            {
                StringBuilder strBd = new StringBuilder(strConv);
                strBd.Replace("̣", "ò").Replace("́", "ì");
                strConv = strBd.ToString();
            }
            int Len = strConv.Length;
            //Thêm vào 7 ký tự trắng vào chuỗi cần chuyển đổi
            //Vì mã NCR Decimal có thể tổ hợp 7 ký tự cho 1 ký tự có dấu
            strConv = strConv.PadRight(Len + 7, ' ');//THêm vào để tránh lỗi khi xử lý chuỗi
            Len = strConv.Length;
            //Khai báo biến chứa chuỗi làm việc 
            int i = 0;
            int j, k;
            string tmp = ""; bool IsFirstCharOfWord = false;
            //Bat dau chuyen ma
            while (i < Len - 7)
            {
                #region Kiểm tra xem ký tự đầu tiên có phải là ký tự đầu tiên của 1 từ không
                //Trường hợp này chỉ chuyển ký tự đầu tiên của chuỗi
                //hoặc những ký tự mà trước nó là khoảng trắng, hoặc tab (ký tự đầu tiên mỗi từ
                //thành chữ hoa
                if (m_CharCase == FontCase.UpperCaseFirstChar)
                {
                    if (i == 0)
                    {
                        IsFirstCharOfWord = true;
                    }
                    else if (i > 0)
                    {
                        tmp = strConv.Substring(i - 1, 1);
                        if (SplitFirstChar.Contains(tmp))
                        {
                            IsFirstCharOfWord = true;
                        }
                        else
                        {
                            IsFirstCharOfWord = false;
                        }
                    }
                }
                #endregion
                //De phong cac ma to hop (mot ky co dau=2->3 ky tu khong dau)
                //nChar, mChar thể hiện sự giao động số ký tự trong 1 bảng mã.                
                for (j = nChar; j >= mChar; j--)
                {
                    s1 = "";// Luu giu ma kt dich sau khi chuyen
                    if (strConv.Substring(i, 1).Equals(" ")) //Nếu hiện tại là khoảng trắng thì chắc chắn ko có mã nào rồi
                    {
                        s = " ";
                        break;
                    }
                    s = strConv.Substring(i, j);//Lay ky tu hien tai trong chuoi
                    //Vong lap tim ma cac kt tu co dau
                    for (k = 1; k < 135; k++)//duyet qua ma nguon, tim vi tri kt trong ma
                    {
                        if (s == Code[k, iSrc])//neu thay
                        {
                            //Giải quyết trường hợp chuyển sang hoa thường
                            if (m_CharCase == FontCase.UpperCase && k < 68)
                            {
                                //Nếu ký tự hiện tại là chữ thường mới xét
                                s1 = Code[k + 67, iDest];
                            }
                            else if (m_CharCase == FontCase.LowerCase && k >= 68)
                            {
                                s1 = Code[k - 67, iDest];
                            }
                            else if (m_CharCase == FontCase.UpperCaseFirstChar)
                            {
                                if (IsFirstCharOfWord && k < 68)
                                {
                                    s1 = Code[k + 67, iDest];
                                }
                                else if (!IsFirstCharOfWord && k >= 68)
                                {
                                    s1 = Code[k - 67, iDest];
                                }
                                else s1 = Code[k, iDest];
                            }
                            else s1 = Code[k, iDest];//Lưu ký tự tương ứng trong mã đích, trường hợp không xét hoa thường;
                            i += j;//Tang i tuy theo truong hop cua j
                            break;
                        }
                    }
                    //Vì j chỉ > 3 khi mã là NCR Decimal, mà mã NCR Decimal chỉ có thể là 6, hoặc 7 ký ự
                    //Tức là trường hợp j = 5 sẽ không xảy ra
                    //Khi đó, ta chắc chắn mã hiện tại là NCR Decimal và trường hợp j=5 không bao giờ xảy ra
                    //==> ta không cần cho j giảm nữa, để tối ưu về thời gian
                    if ((!s1.Equals("")) || (j == 5)) break;//neu tim thay thi thoi lap
                }
                //s1 chứa ký tự có dấu của mã đích tìm được
                //Nếu s1 khác "" nghĩa là có sự chuyển đổi
                if (!s1.Equals(""))
                {
                    sResult.Append(s1);//dua ky chuyen duoc vao chuoi ket qua
                    hasconvert = true;
                }
                else
                {
                    if (m_CharCase == FontCase.Normal)
                    {
                        sResult.Append(s.Substring(0, 1));
                    }
                    else
                    {
                        if (m_CharCase == FontCase.UpperCase)
                        {
                            sResult.Append(s.Substring(0, 1).ToUpper());//Đưa ký tự không dấu (không được chuyển đổi) vào chuỗi kết quả
                        }
                        else if (m_CharCase == FontCase.LowerCase)
                        {
                            sResult.Append(s.Substring(0, 1).ToLower());//Đưa ký tự không dấu (không được chuyển đổi) vào chuỗi kết quả
                        }
                        else if (m_CharCase == FontCase.UpperCaseFirstChar)
                        {
                            if (IsFirstCharOfWord)
                            {
                                sResult.Append(s.Substring(0, 1).ToUpper());
                            }
                            else
                            {
                                sResult.Append(s.Substring(0, 1).ToLower());
                            }
                        }
                    }
                    //Trường hợp có sự chuyển đổi, i đã được tăng ở trên
                    //Trường hợp không có sự chuyển đổi ở ký tự hiện tại thì tăng i để duyệt ký tự kế tiếp
                    i++;
                }
            }
            //Nếu không có sự chuyển đổi nào xảy ra
            if (!hasconvert)
            {
                //Loại bỏ 7 khoảng trắng đã thêm vào ban đầu
                strConv = strConv.Remove(Len - 7, 7);
                if (m_CharCase == FontCase.UpperCase)
                {
                    strConv = strConv.ToUpper();
                    return true;
                }
                else if (m_CharCase == FontCase.LowerCase)
                {
                    strConv = strConv.ToLower();
                    return true;
                }
                return false;
            }
            else
            {
                strConv = sResult.ToString();
                return true;
            }
        }
    }
}
