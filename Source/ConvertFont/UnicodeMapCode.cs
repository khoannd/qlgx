using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertDB.vnConvert
{
    public class UnicodeMapCode: IMapCode
    {
        FontIndex fontType = FontIndex.iUNI;

        public FontIndex FontType
        {
            get { return fontType; }
            set { fontType = value; }
        }

        public void MapCode(ref string[,] Code)
        {
            Code[1, (int)fontType] = "á";
            Code[2, (int)fontType] = "à";
            Code[3, (int)fontType] = "ả";
            Code[4, (int)fontType] = "ã";
            Code[5, (int)fontType] = "ạ";
            Code[6, (int)fontType] = "ă";
            Code[7, (int)fontType] = "ắ";
            Code[8, (int)fontType] = "ằ";
            Code[9, (int)fontType] = "ẳ";
            Code[10, (int)fontType] = "ẵ";
            Code[11, (int)fontType] = "ặ";
            Code[12, (int)fontType] = "â";
            Code[13, (int)fontType] = "ấ";
            Code[14, (int)fontType] = "ầ";
            Code[15, (int)fontType] = "ẩ";
            Code[16, (int)fontType] = "ẫ";
            Code[17, (int)fontType] = "ậ";
            Code[18, (int)fontType] = "é";
            Code[19, (int)fontType] = "è";
            Code[20, (int)fontType] = "ẻ";
            Code[21, (int)fontType] = "ẽ";
            Code[22, (int)fontType] = "ẹ";
            Code[23, (int)fontType] = "ê";
            Code[24, (int)fontType] = "ế";
            Code[25, (int)fontType] = "ề";
            Code[26, (int)fontType] = "ể";
            Code[27, (int)fontType] = "ễ";
            Code[28, (int)fontType] = "ệ";
            Code[29, (int)fontType] = "í";
            Code[30, (int)fontType] = "ì";
            Code[31, (int)fontType] = "ỉ";
            Code[32, (int)fontType] = "ĩ";
            Code[33, (int)fontType] = "ị";
            Code[34, (int)fontType] = "ó";
            Code[35, (int)fontType] = "ò";
            Code[36, (int)fontType] = "ỏ";
            Code[37, (int)fontType] = "õ";
            Code[38, (int)fontType] = "ọ";
            Code[39, (int)fontType] = "ô";
            Code[40, (int)fontType] = "ố";
            Code[41, (int)fontType] = "ồ";
            Code[42, (int)fontType] = "ổ";
            Code[43, (int)fontType] = "ỗ";
            Code[44, (int)fontType] = "ộ";
            Code[45, (int)fontType] = "ơ";
            Code[46, (int)fontType] = "ớ";
            Code[47, (int)fontType] = "ờ";
            Code[48, (int)fontType] = "ở";
            Code[49, (int)fontType] = "ỡ";
            Code[50, (int)fontType] = "ợ";
            Code[51, (int)fontType] = "ú";
            Code[52, (int)fontType] = "ù";
            Code[53, (int)fontType] = "ủ";
            Code[54, (int)fontType] = "ũ";
            Code[55, (int)fontType] = "ụ";
            Code[56, (int)fontType] = "ư";
            Code[57, (int)fontType] = "ứ";
            Code[58, (int)fontType] = "ừ";
            Code[59, (int)fontType] = "ử";
            Code[60, (int)fontType] = "ữ";
            Code[61, (int)fontType] = "ự";
            Code[62, (int)fontType] = "ý";
            Code[63, (int)fontType] = "ỳ";
            Code[64, (int)fontType] = "ỷ";
            Code[65, (int)fontType] = "ỹ";
            Code[66, (int)fontType] = "ỵ";
            Code[67, (int)fontType] = "đ";
            Code[68, (int)fontType] = "Á";
            Code[69, (int)fontType] = "À";
            Code[70, (int)fontType] = "Ả";
            Code[71, (int)fontType] = "Ã";
            Code[72, (int)fontType] = "Ạ";
            Code[73, (int)fontType] = "Ă";
            Code[74, (int)fontType] = "Ắ";
            Code[75, (int)fontType] = "Ằ";
            Code[76, (int)fontType] = "Ẳ";
            Code[77, (int)fontType] = "Ẵ";
            Code[78, (int)fontType] = "Ặ";
            Code[79, (int)fontType] = "Â";
            Code[80, (int)fontType] = "Ấ";
            Code[81, (int)fontType] = "Ầ";
            Code[82, (int)fontType] = "Ẩ";
            Code[83, (int)fontType] = "Ẫ";
            Code[84, (int)fontType] = "Ậ";
            Code[85, (int)fontType] = "É";
            Code[86, (int)fontType] = "È";
            Code[87, (int)fontType] = "Ẻ";
            Code[88, (int)fontType] = "Ẽ";
            Code[89, (int)fontType] = "Ẹ";
            Code[90, (int)fontType] = "Ê";
            Code[91, (int)fontType] = "Ế";
            Code[92, (int)fontType] = "Ề";
            Code[93, (int)fontType] = "Ể";
            Code[94, (int)fontType] = "Ễ";
            Code[95, (int)fontType] = "Ệ";
            Code[96, (int)fontType] = "Í";
            Code[97, (int)fontType] = "Ì";
            Code[98, (int)fontType] = "Ỉ";
            Code[99, (int)fontType] = "Ĩ";
            Code[100, (int)fontType] = "Ị";
            Code[101, (int)fontType] = "Ó";
            Code[102, (int)fontType] = "Ò";
            Code[103, (int)fontType] = "Ỏ";
            Code[104, (int)fontType] = "Õ";
            Code[105, (int)fontType] = "Ọ";
            Code[106, (int)fontType] = "Ô";
            Code[107, (int)fontType] = "Ố";
            Code[108, (int)fontType] = "Ồ";
            Code[109, (int)fontType] = "Ổ";
            Code[110, (int)fontType] = "Ỗ";
            Code[111, (int)fontType] = "Ộ";
            Code[112, (int)fontType] = "Ơ";
            Code[113, (int)fontType] = "Ớ";
            Code[114, (int)fontType] = "Ờ";
            Code[115, (int)fontType] = "Ở";
            Code[116, (int)fontType] = "Ỡ";
            Code[117, (int)fontType] = "Ợ";
            Code[118, (int)fontType] = "Ú";
            Code[119, (int)fontType] = "Ù";
            Code[120, (int)fontType] = "Ủ";
            Code[121, (int)fontType] = "Ũ";
            Code[122, (int)fontType] = "Ụ";
            Code[123, (int)fontType] = "Ư";
            Code[124, (int)fontType] = "Ứ";
            Code[125, (int)fontType] = "Ừ";
            Code[126, (int)fontType] = "Ử";
            Code[127, (int)fontType] = "Ữ";
            Code[128, (int)fontType] = "Ự";
            Code[129, (int)fontType] = "Ý";
            Code[130, (int)fontType] = "Ỳ";
            Code[131, (int)fontType] = "Ỷ";
            Code[132, (int)fontType] = "Ỹ";
            Code[133, (int)fontType] = "Ỵ";
            Code[134, (int)fontType] = "Đ";
        }
    }
}
