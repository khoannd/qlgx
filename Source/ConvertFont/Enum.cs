using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
//[assembly: InternalsVisibleTo("ConvertDB")]	
namespace ConvertDB.vnConvert
{
    public class Constants
    {
        public const string MSG_Text = "Văn bản ";
        public const string MSG_RTF = "Văn bản RTF ";
        public const string MSG_HTM = "Văn bản HTML ";
    }

    public enum FontIndex
    {
        /// <summary>
        /// Mã không xác định
        /// </summary>
        iNotKnown = -1,
        /// <summary>
        /// Mã NCR Hex (Maximun 8 char)
        /// </summary>
        iNCRHex,
        /// <summary>
        /// Mã NCR Decimal (7 char)
        /// </summary>
        iNCR,
        /// <summary>
        /// Mã C String (Maximum 6 char)
        /// </summary>
        iCString,
        /// <summary>
        /// Mã Unicode UTF-8 (3 char)
        /// </summary>
        iUTF,
        /// <summary>
        /// Mã TCVN3(ABC) (1 char)
        /// </summary>
        iTCV,
        /// <summary>
        /// Mã VietwareF (Maximun 1 char)
        /// </summary>
        iVWF,
        /// <summary>
        /// Mã VISCII (Maximun 1 char)
        /// </summary>
        iVISCII,
        /// <summary>
        /// Mã BK HCM 1 (Maximun 1 char)
        /// </summary>
        iBKHCM1,
        /// <summary>
        /// MÃ VPS (Maximun 1 char)
        /// </summary>
        iVPS,
        /// <summary>
        /// Mã VNI-Windows (2 char)
        /// </summary>
        iVNI,
        /// <summary>
        /// Mã VietwareX
        /// </summary>
        iVWX,
        /// <summary>
        /// Mã BK HCM2 (Maximun 2 char)
        /// </summary>
        iBKHCM2,
        /// <summary>
        /// Mã Unicode do Microsoft định nghĩa cho Việt Nam (2 char)
        /// </summary>
        iCP1258,
        /// <summary>
        /// Mã Unicode Tổ hợp (2 char)
        /// </summary>
        iUTH,
        /// <summary>
        /// Mã Unicode dựng sẵn (1 char)
        /// </summary>
        iUNI,
        /// <summary>
        /// Mã tiếng việt không dấu (1 char)
        /// </summary>
        iNOSIGN,
        /// <summary>
        /// Mã VIQR
        /// </summary>
        iVIQ
    }

    public enum FontCase
    {
        UpperCase, LowerCase, UpperCaseFirstChar, Normal
    }

    public enum ClipboardType
    {
        Text, Rtf, Html, None
    }
}
