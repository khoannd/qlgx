/*-----------------------------------------------
 * Đây là thư viện vnConvert với class ConvertFont thực hiện các chức năng chuyển mã tiếng việt
 *  Dùng cho phần mềm 'Viet-font Convert for Database'
 * Tác giả: Nguyễn Đức Khoan
 * Email: nguyenduckhoan@gmail.com
 * Website: http://www.luudiachiweb.com/convertdb/
*/
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Generic;
//[assembly: InternalsVisibleTo("VietFontConverter")]	
namespace ConvertDB.vnConvert
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    //Class ConvertFont convert n vietnamese code
    /// <summary>
    /// Nhận dạng, chuyển mã tiếng việt
    /// </summary>
    public partial class ConvertFont
    {
        #region Declare private member
        private string[,] Code;
        public const int COUNT = 16;//Số bảng mã cần làm việc, ngoại trừ mã VIQR
        //private const int iRTF = (int)FontIndex.iRTF;
        private const int iNCR = (int)FontIndex.iNCR;
        private const int iUTF = (int)FontIndex.iUTF;
        private const int iTCV = (int)FontIndex.iTCV;
        private const int iVWF = (int)FontIndex.iVWF;
        private const int iVNI = (int)FontIndex.iVNI;
        private const int iVWX = (int)FontIndex.iVWX;
        private const int iUNI = (int)FontIndex.iUNI;
        private const int iUTH = (int)FontIndex.iUTH;
        private const int iCP1258 = (int)FontIndex.iCP1258;
        private const int iVIQ = (int)FontIndex.iVIQ;
        //Mã thêm sau start
        private const int iVISCII = (int)FontIndex.iVISCII;
        private const int iVPS = (int)FontIndex.iVPS;
        private const int iBKHCM2 = (int)FontIndex.iBKHCM2;
        private const int iBKHCM1 = (int)FontIndex.iBKHCM1;
        private const int iNCRHex = (int)FontIndex.iNCRHex;
        private const int iCString = (int)FontIndex.iCString;
        //Mã thêm sau end
        private const int iNOSIGN = (int)FontIndex.iNOSIGN;
        private FontCase m_CharCase = FontCase.Normal;
        private const int DISTANCE_CASE = 67;
        private const int countVnChar = 135;
        private const int maximunLenChar = 7;
        private int[] arrCorresponseCode = new int[] { iUNI, iUTH, iTCV, iVISCII, iVWX };
        List<string[]> lstNguyenAm = new List<string[]>();

        //
        string SplitFirstChar = " ~`!@#$%^&*()-+=|\\{[]}:;'\"<,>.?/\t\n\r";

        #endregion

        #region "Declare Public Property"
        public FontCase CharCase
        {
            get { return m_CharCase; }
            set { m_CharCase = value; }
        }
        #endregion

        #region Constructor
        public ConvertFont()
        {
            InitData();
        }
        #endregion

        #region private method
        /// <summary>
        /// Khởi tạo các giá trị mặc định cho mảng Code
        /// </summary>
        private void InitData()
        {
            Code = new string[countVnChar, COUNT];//Khởi tạo mảng
            //Gán các ký tự nhận dạng mã
            Code[0, iNCR] = ((int)FontIndex.iNCR).ToString();//NCR Decimal
            Code[0, iUTF] = ((int)FontIndex.iUTF).ToString();//Unicode UTF-8
            Code[0, iTCV] = ((int)FontIndex.iTCV).ToString();//TCVN3 (ABC)
            Code[0, iVNI] = ((int)FontIndex.iVNI).ToString();//VNI-Windows
            Code[0, iUTH] = ((int)FontIndex.iUTH).ToString();//Unicode tổ hợp
            Code[0, iUNI] = ((int)FontIndex.iUNI).ToString();//Unicode dựng sắn
            Code[0, iCP1258] = ((int)FontIndex.iCP1258).ToString();//Unicode dựng sắn
            Code[0, iNOSIGN] = ((int)(FontIndex.iNOSIGN)).ToString();//Mã không dấu
            Code[0, iVWF] = ((int)(FontIndex.iVWF)).ToString();
            Code[0, iVWX] = ((int)(FontIndex.iVWX)).ToString();
            //Các bảng mã mới thêm sau
            Code[0, iVISCII] = ((int)(FontIndex.iVISCII)).ToString();
            Code[0, iVPS] = ((int)(FontIndex.iVPS)).ToString();
            Code[0, iBKHCM2] = ((int)(FontIndex.iBKHCM2)).ToString();
            Code[0, iBKHCM1] = ((int)(FontIndex.iBKHCM1)).ToString();
            Code[0, iNCRHex] = ((int)(FontIndex.iNCRHex)).ToString();
            Code[0, iCString] = ((int)(FontIndex.iCString)).ToString();

            //Code[0,5] = "VIQ";//VIQR
            //Khởi tạo mảng các ký tự có dấu để ánh xạ trong các bàng mã
            //MapRTF();
            MapUnicode();
            MapVNI();
            MapVietwareF();
            MapTCV();
            MapVietwareX();
            MapUTH();
            MapUTF8();
            MapNCR();
            MapNoSign();
            MapCP1258();

            //Các bảng mã mới thêm sau
            MapBKHCM1();
            MapBKHCM2();
            MapCString();
            MapNCRHex();
            MapVISCII();
            MapVPS();
            //MapVIQR();
            MapNguyenAm();//Map cac nguyen am dung cho viec convert kieu bo dau
        }

        private int GetnChar(FontIndex index)
        {
            switch (index)
            {
                case FontIndex.iUTF:
                    return 3;
                case FontIndex.iUNI:
                //case FontIndex.iUTH:
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
            //if (index == FontIndex.iUTF)
            //{
            //    return 3;
            //}
            //else if (index == FontIndex.iUNI || index == FontIndex.iUTH ||
            //    index == FontIndex.iVWF || index == FontIndex.iNOSIGN )
            //{
            //    return 1;
            //}
            //else if (index == FontIndex.iNCR)
            //{
            //    return 7;
            //}
            //else
            //{
            //    return 2;
            //}
        }

        private int getIntCode(String code)
        {
            for (int i = 0; i < COUNT; i++)
                if (Code[0, i] == code) return i;
            return -1;
        }

        private bool isSpecialChar(string s)
        {
            return isSpecialChar(s, false);
        }
        //Tìm các ký tự đặc biệt là các ký tự có thể trùng nhau qua các mã
        private bool isSpecialChar(string s, bool isUNI)
        {
            //Các ký tự trùng nhau chỉ có length==2 hay length==1
            if (s.Length > 2) return false;
            string[] specialChar = { "í", "ì", "ó", "ò", "ô", "ñ", "î", "Ê", "È", "É", "á", "à", "â", "è", "é", "ê", "ù", "ý", "ú", "ö", "Í", "Ì", "Ó", "Ò", "Ô", "Ñ", "Î", "Õ", "Ý", "Ã", "oà", "oá", "oã", "uû", "OÁ", "OÀ", "OÃ" };
            string[] specialUNIChar ={ "ă", "â", "ê", "ô", "ơ", "ư", "đ", "í", "ì", "ó", "ò", "ô", "ñ", "î", "Ê", "È", "É", "á", "à", "â", "è", "é", "ê", "ù", "ý", "ú", "ö", "Í", "Ì", "Ó", "Ò", "Ô", "Ñ", "Î", "Õ", "Ý", "Ã", "oà", "oá", "oã", "uû", "OÁ", "OÀ", "OÃ" };
            string[] special;
            //Mot so ky tu UNICODE chi co o UTH hay UNI
            if (!isUNI) special = specialChar; else special = specialUNIChar;
            for (int i = 0; i < special.Length; i++)
                if (string.Compare(s, special[i], true) == 0) return true;
            return false;
        }

        /// <summary>
        /// Hàm nhận dạng mã tiếng việt.
        /// return chuỗi rỗng nếu không tìm được mã tiếng việt trong chuỗi đưa vào
        /// </summary>
        /// <param name="str"></param>
        /// <returns>trả về mã nhận dạng được ở dạng ký hiệu kiểu chuỗi</returns>
        private FontIndex getCode(String str)
        {
            if (str.Trim() == "") return FontIndex.iNotKnown;
            bool special;
            int ma = -1;
            String s = "";
            int Len = str.Length;
            // StringBuilder strBd = new StringBuilder(str.PadRight(Len + 7, ' '));//Thêm ký tự trắng cuối chuỗi 
            str = str.PadRight(Len + 7, ' ');
            Len = str.Length;
            //strBd.Append("       ");//Thêm ký tự trắng cuối chuỗi 
            //đề phòng mã tổ hợp phải lấy > 1 ký tự để so sánh 
            //hoặc mã VQIR và UTF-8: 
            //1 ký tự có dấu có thể >= 3 ký tự không dấu ANSI
            int j, k, c;
            int i = 0;
            while (i < Len - 7)
            {
                //Nếu hiện tại là khoảng trắng thì chắc chắn ko có mã nào rồi
                if (str.Substring(i, 1).Equals(" "))
                {
                    i++;
                    continue;
                }

                //Đề phòng mã tổ hợp phải lấy nhiều hơn 1 ký tự so sánh  
                //(một ký tự có dấu của các mã mà 1 kt có dấu có thể = 2->3 ký tự ANSI không dấu)
                for (j = 7; j > 0; j--)
                {
                    s = str.Substring(i, j);//Lấy j ký tự hiện tại trong chuỗi.
                    //Vòng lặp tìm vị trí ánh xạ các ký tự có dấu
                    //Thu tu cac bang ma duoc xep giam dan theo cac kt co dau
                    for (c = 0; c < COUNT - 1; c++)//nCode - 1 vì không xét mã không dấu
                    {
                        //Loai ra mot so truong hop hy huu~
                        //ko co ma nao j==4 hay j==5
                        if (j == 4 || j == 5) break;
                        //Chi NCR co thi j==6 hay j==7
                        if ((j >= 6) && c != iNCR) break;
                        // chi UTF hay VIQ thi moi co j==3
                        if (j == 3 && (c != iUTF)) continue;
                        //Chi VNI,TCV,UTH,CP1258 co j==2
                        if ((c == iVNI || c == iTCV || c == iUTH || c == iCP1258) && j > 2) continue;
                        //Bat dau nhan dang ma
                        for (k = 1; k < countVnChar; k++)
                            if (s == Code[k, c])//Nếu tìm thấy
                            {
                                //trả về tên mã tương ứng
                                special = isSpecialChar(s, (c == iUTH) || (c == iUNI));
                                if (!special) return (FontIndex)int.Parse(Code[0, c]);
                                ma = c; //i += j;
                                break;
                            }
                    }
                }
                i++;
            }
            //Nếu không tìm thấy mã nào
            return ma >= 0 ? (FontIndex)int.Parse(Code[0, ma]) : FontIndex.iNotKnown;
        }

       /*

        /// <summary>
        /// Với những mã mà 1 ký tự bên mã gốc chỉ tương ứng với 1 ký tự bên mã đích thì dùng cách replace là nhanh nhất
        /// </summary>
        private bool ConvertByReplace(ref string strConv, int iSrc, int iDest)
        {
            if (string.IsNullOrEmpty(strConv))
            {
                return false;
            }
            StringBuilder strBulder = new StringBuilder(strConv);
            for (int i = 1; i < countVnChar; i++)
            {
                strBulder.Replace(Code[i, iSrc], Code[i, iDest]);
            }
            strConv = strBulder.ToString();
            return true;
        }

        private bool CanReplace(int iCode)
        {
            for (int i = 0; i < arrCorresponseCode.Length; i++)
            {
                if (iCode == arrCorresponseCode[i])
                {
                    return true;
                }
            }
            return false;
        }

        private void ConvertTCVNToUNI(ref string strConv)
        {
            return ConvertByReplace(ref strConv, iTCV, iUNI);
        }

        private void ConvertUNIToTCVN(ref string strConv)
        {
            return ConvertByReplace(ref strConv, iUNI, iTCV);
        }

        private void ConvertUNIToUTH(ref string strConv)
        {
            return ConvertByReplace(ref strConv, iUNI, iUTH);
        }

        private void ConvertUTHToUNI(ref string strConv)
        {
            return ConvertByReplace(ref strConv, iUTH, iUNI);
        }

        private void ConvertUTHToUNI(ref string strConv)
        {
            return ConvertByReplace(ref strConv, iUTH, iUNI);
        }
        */
        #endregion

        #region Public method
        /// <summary>
        /// Chuyển mã một chuỗi văn bản tiếng việt với một số mã thông dụng
        /// </summary>
        /// <param name="strConv">
        /// Chuỗi văn bản tiếng việt cần chuyển đổi
        /// </param>
        /// <param name="iSrc">
        /// Mã gốc (int) của chuỗi tiếng việt đưa vào. Các mã thuộc struct FontIndex
        /// </param>
        /// <param name="iDest">
        /// Mã đích (int) của chuỗi tiếng việt đưa vào. Các mã thuộc struct FontIndex
        /// </param>
        /// <returns>Nếu có thực hiện chuyển đổi thì return true, ngược lại return false</returns>
        public bool Convert(ref String strConv, FontIndex iSource, FontIndex iDestination)
        {
            int iSrc = (int)iSource;
            int iDest = (int)iDestination;
            if (strConv.Trim() == "") return false;
            if (iSrc == iDest) return false;
            String s = "", s1 = "";
            if (iSrc == (int)FontIndex.iNotKnown) //Nếu không tìm thấy tên mã đưa vào
            {
                //Tự động tìm mã vào dựa theo các ký tự dấu tiếng việt
                int intcode = (int)getCode(strConv);
                if (intcode > -1) iSrc = intcode;
                else return false;
            }
            //Mac dinh ma xuat ra la Unicode
            if (iDest == -1) iDest = 0;
            ////Xét xem có thể dùng cách replace cho các mã này không?
            //if (CanReplace(iSrc) && CanReplace(iDest))
            //{
            //    return ConvertByReplace(ref strConv, iSrc, iDest);
            //}
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
            if (iSource == FontIndex.iVWF)
            {
                StringBuilder strBd = new StringBuilder(strConv);
                strBd.Replace("̣", "ò").Replace("́", "ì");
                strConv = strBd.ToString();
            }
            int Len = strConv.Length;
            //Thêm vào 7 ký tự trắng vào chuỗi cần chuyển đổi
            //Vì mã NCR Decimal có thể tổ hợp 7 ký tự cho 1 ký tự có dấu
            strConv = strConv.PadRight(Len + countVnChar, ' ');//THêm vào để tránh lỗi khi xử lý chuỗi
            Len = strConv.Length;
            //Khai báo biến chứa chuỗi làm việc 
            int i = 0;
            int j, k;
            string tmp = ""; bool IsFirstCharOfWord = false;
            //Bat dau chuyen ma
            while (i < Len - countVnChar)
            {
                #region Kiểm tra xem ký tự đầu tiên có phải là ký tự đầu tiên của 1 từ không
                //Trường hợp này chỉ chuyển ký tự đầu tiên của chuỗi
                //hoặc những ký tự mà trước nó là khoảng trắng, hoặc tab (ký tự đầu tiên mỗi từ
                //thành chữ hoa
                if (m_CharCase == FontCase.UpperCaseFirstChar)
                {
                    if (i == 0)
                        IsFirstCharOfWord = true;
                    else if (i > 0)
                    {
                        tmp = strConv.Substring(i - 1, 1);
                        if (SplitFirstChar.Contains(tmp))
                            IsFirstCharOfWord = true;
                        else IsFirstCharOfWord = false;
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
                    for (k = 1; k < countVnChar; k++)//duyet qua ma nguon, tim vi tri kt trong ma
                    {
                        if (s == Code[k, iSrc])//neu thay
                        {
                            //Giải quyết trường hợp chuyển sang hoa thường
                            if (m_CharCase == FontCase.UpperCase && k < DISTANCE_CASE + 1)
                            {
                                //Nếu ký tự hiện tại là chữ thường mới xét
                                s1 = Code[k + DISTANCE_CASE, iDest];
                            }
                            else if (m_CharCase == FontCase.LowerCase && k >= DISTANCE_CASE + 1)
                            {
                                s1 = Code[k - DISTANCE_CASE, iDest];
                            }
                            else if (m_CharCase == FontCase.UpperCaseFirstChar)
                            {
                                if (IsFirstCharOfWord && k < DISTANCE_CASE + 1)
                                {
                                    s1 = Code[k + DISTANCE_CASE, iDest];
                                }
                                else if (!IsFirstCharOfWord && k >= DISTANCE_CASE + 1)
                                {
                                    s1 = Code[k - DISTANCE_CASE, iDest];
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
                    sResult.Append(s1);//dua ky tu chuyen duoc vao chuoi ket qua
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
                strConv = strConv.Remove(Len - countVnChar, countVnChar);
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
                else if (m_CharCase == FontCase.UpperCaseFirstChar)
                {
                    if (strConv.Length > 1)
                    {
                        strConv = strConv.Substring(0, 1).ToUpper() + strConv.Substring(1);
                    }
                    else if (strConv.Length == 1)
                    {
                        strConv = strConv.Substring(0, 1).ToUpper();
                    }
                }
                return false;
            }
            else
            {
                strConv = sResult.ToString();
                return true;
            }
        }

        /// <summary>
        /// Chuyển mã từ TCVN3 sang Unicode và trả về mã không dấu, chuỗi trong biến tham số s sẽ có mã là Unicode sau khi chuyển mã
        /// </summary>
        public string Convert(ref string s)
        {
            string str = s;
            Convert(ref s, FontIndex.iTCV, FontIndex.iUNI);
            Convert(ref str, FontIndex.iTCV, FontIndex.iNOSIGN);
            return str;
        }

        /// <summary>
        /// Lấy ký hiệu nhận dạng mã theo kiểu số
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>


        /// <summary>
        /// Kiểm tra một chuỗi có phải tiếng việt không. 
        /// Dùng nếu cần quan tâm mã của chuỗi là gì
        /// </summary>
        /// <param name="s">set string to work</param>
        /// <param name="code">get vietnamese int code if string is vietnamese</param>
        /// <returns></returns>
        public bool isVietnamese(String s, ref FontIndex code)
        {
            code = getCode(s);
            if (code > FontIndex.iNotKnown) return true;
            return false;
        }

        /// <summary>
        /// Hàm nhận dạng tiếng việt và không quan tâm mã của chuỗi đưa vào là gì
        /// </summary>
        /// <param name="s">set string to work</param>
        /// <returns></returns>
        public bool isVietnamese(String s)
        {
            if (getCode(s) != FontIndex.iNotKnown) return true;
            return false;
        }

        //public string Rtf2Unicode(string s)
        //{
        //    for (int i = 1; i < countVnChar; i++)
        //    {
        //        s = s.Replace(Code[i, iRTF], Code[i, iUNI]);
        //    }
        //    return s;
        //}

        ///// <summary>
        ///// Convert for oà and uỳ only
        ///// </summary>
        ///// <param name="sConvert"></param>
        ///// <returns></returns>
        //public string ConvertVietnameseSignTwoCase(string sConvert)
        //{
        //    List<string[]> lst1 = new List<string[]>();
        //    lst1.Add(lstNguyenAm[0]);
        //    lst1.Add(lstNguyenAm[11]);
        //    lst1.Add(lstNguyenAm[12]);
        //    lst1.Add(lstNguyenAm[23]);
        //    List<string> lst = new List<string>();
        //    lst.Add("o");
        //    lst.Add("O");
        //    lst.Add("u");
        //    lst.Add("U");
        //    foreach (string s1 in lst)
        //    {
        //        for (int i = 1; i < 6; i++)
        //        {
        //            foreach (string[] s2 in lst1)
        //            {
        //                //sConvert = sConvert.Replace(
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Convert for all case, slower
        /// </summary>
        /// <param name="sConvert"></param>
        /// <returns></returns>
        public string ConvertVietnameseSign(string sConvert)
        {
            //chỉ xét trường hợp các vần có  2 ký tự mà y và a đứng sau cuối
            //a và y có index 0,3,4,7



            //list chua danh sach cac nguyen am khong dau
            List<string> lst = new List<string>();
            for (int i = 0; i < lstNguyenAm.Count; i++)
            {
                lst.Add(lstNguyenAm[i][0]);
            }
            string[] arr1 = null, arr2 = null;
            string kq = "";
            //chuyển kiểu gõ oà sang òa
            for (int i = 0; i < sConvert.Length - 1; i++)
            {
                string s = sConvert.Substring(i, 2);
                if (s == "uy")
                {
                    i += 3;
                    continue;
                }
                string s1 = sConvert[i].ToString();
                string s2 = sConvert[i + 1].ToString();
                //Neu ky tu hien tai la nguyen am khong dau
                if (lst.Contains(s1))
                {
                    //Xet ky tu tiep theo co phai la nguyen am co dau hay khong?
                    int vitriNguyenAmCoDau = viTriNguyenAmCoDau(s2);
                    if (vitriNguyenAmCoDau > 0)
                    {
                        //tìm mảng của ký tự đầu tiên không có dấu
                        arr1 = timMangNguyenAm(s1);
                        arr2 = timMangNguyenAm(s2);
                        //chỉ hoán đổi khi s2 là y hoặc a và s2 là ký tự cuối cùng của từ
                        //a và y có index 0,3,4,7
                        if (arr2.Equals(a) || arr2.Equals(A) || arr2.Equals(e) || arr2.Equals(E) 
                            || arr2.Equals(y) || arr2.Equals(Y))
                        {
                            //trừ trường hợp quà, già
                            if (i > 0 && sConvert.Substring(i - 1, 2) != "qu")
                            {
                                int len = sConvert.Length;
                                // nếu s2 là ký tự cuối cùng của từ
                                if (i + 2 >= len || SplitFirstChar.Contains(sConvert[i + 2].ToString()))
                                {
                                    //loai tru cac truong hop uy mà y có dấu, sau y còn ký tự khác ví dụ huỳnh, yến, uẩn
                                    //Hoan doi vi tri dau cua 2 nguyen am nay
                                    kq = string.Concat(arr1[vitriNguyenAmCoDau], arr2[0]);
                                    if (i + 2 < len)
                                    {
                                        sConvert = string.Concat(sConvert.Substring(0, i), kq, sConvert.Substring(i + 2));
                                    }
                                    else
                                    {
                                        sConvert = string.Concat(sConvert.Substring(0, i), kq);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sConvert;
        }

        private string[] timMangNguyenAm(string s)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int i = 0; i < lstNguyenAm.Count; i++)
                {
                    if (lstNguyenAm[i][j] == s)
                    {
                        return lstNguyenAm[i];
                    }
                }
            }
            return null;
        }

        private int viTriNguyenAmCoDau(string s)
        {
            foreach (string[] arr in lstNguyenAm)
            {
                for (int i = 1; i < arr.Length; i++)
                {
                    if (arr[i] == s)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        #endregion
    }
}
