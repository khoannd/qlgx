using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertDB.vnConvert
{
    public class VNIMapCode: IMapCode
    {
        FontIndex fontType = FontIndex.iVNI;

        public FontIndex FontType
        {
            get { return fontType; }
            set { fontType = value; }
        }

        public void MapCode(ref string[,] Code)
        {
            Code[1, (int)fontType] = "aù";
            Code[2, (int)fontType] = "aø";
            Code[3, (int)fontType] = "aû";
            Code[4, (int)fontType] = "aõ";
            Code[5, (int)fontType] = "aï";
            Code[6, (int)fontType] = "aê";
            Code[7, (int)fontType] = "aé";
            Code[8, (int)fontType] = "aè";
            Code[9, (int)fontType] = "aú";
            Code[10, (int)fontType] = "aü";
            Code[11, (int)fontType] = "aë";
            Code[12, (int)fontType] = "aâ";
            Code[13, (int)fontType] = "aá";
            Code[14, (int)fontType] = "aà";
            Code[15, (int)fontType] = "aå";
            Code[16, (int)fontType] = "aã";
            Code[17, (int)fontType] = "aä";
            Code[18, (int)fontType] = "eù";
            Code[19, (int)fontType] = "eø";
            Code[20, (int)fontType] = "eû";
            Code[21, (int)fontType] = "eõ";
            Code[22, (int)fontType] = "eï";
            Code[23, (int)fontType] = "eâ";
            Code[24, (int)fontType] = "eá";
            Code[25, (int)fontType] = "eà";
            Code[26, (int)fontType] = "eå";
            Code[27, (int)fontType] = "eã";
            Code[28, (int)fontType] = "eä";
            Code[29, (int)fontType] = "í";
            Code[30, (int)fontType] = "ì";
            Code[31, (int)fontType] = "æ";
            Code[32, (int)fontType] = "ó";
            Code[33, (int)fontType] = "ò";
            Code[34, (int)fontType] = "où";
            Code[35, (int)fontType] = "oø";
            Code[36, (int)fontType] = "oû";
            Code[37, (int)fontType] = "oõ";
            Code[38, (int)fontType] = "oï";
            Code[39, (int)fontType] = "oâ";
            Code[40, (int)fontType] = "oá";
            Code[41, (int)fontType] = "oà";
            Code[42, (int)fontType] = "oå";
            Code[43, (int)fontType] = "oã";
            Code[44, (int)fontType] = "oä";
            Code[45, (int)fontType] = "ô";
            Code[46, (int)fontType] = "ôù";
            Code[47, (int)fontType] = "ôø";
            Code[48, (int)fontType] = "ôû";
            Code[49, (int)fontType] = "ôõ";
            Code[50, (int)fontType] = "ôï";
            Code[51, (int)fontType] = "uù";
            Code[52, (int)fontType] = "uø";
            Code[53, (int)fontType] = "uû";
            Code[54, (int)fontType] = "uõ";
            Code[55, (int)fontType] = "uï";
            Code[56, (int)fontType] = "ö";
            Code[57, (int)fontType] = "öù";
            Code[58, (int)fontType] = "öø";
            Code[59, (int)fontType] = "öû";
            Code[60, (int)fontType] = "öõ";
            Code[61, (int)fontType] = "öï";
            Code[62, (int)fontType] = "yù";
            Code[63, (int)fontType] = "yø";
            Code[64, (int)fontType] = "yû";
            Code[65, (int)fontType] = "yõ";
            Code[66, (int)fontType] = "î";
            Code[67, (int)fontType] = "ñ";
            Code[68, (int)fontType] = "AÙ";
            Code[69, (int)fontType] = "AØ";
            Code[70, (int)fontType] = "AÛ";
            Code[71, (int)fontType] = "AÕ";
            Code[72, (int)fontType] = "AÏ";
            Code[73, (int)fontType] = "AÊ";
            Code[74, (int)fontType] = "AÉ";
            Code[75, (int)fontType] = "AÈ";
            Code[76, (int)fontType] = "AÚ";
            Code[77, (int)fontType] = "AÜ";
            Code[78, (int)fontType] = "AË";
            Code[79, (int)fontType] = "AÂ";
            Code[80, (int)fontType] = "AÁ";
            Code[81, (int)fontType] = "AÀ";
            Code[82, (int)fontType] = "AÅ";
            Code[83, (int)fontType] = "AÃ";
            Code[84, (int)fontType] = "AÄ";
            Code[85, (int)fontType] = "EÙ";
            Code[86, (int)fontType] = "EØ";
            Code[87, (int)fontType] = "EÛ";
            Code[88, (int)fontType] = "EÕ";
            Code[89, (int)fontType] = "EÏ";
            Code[90, (int)fontType] = "EÂ";
            Code[91, (int)fontType] = "EÁ";
            Code[92, (int)fontType] = "EÀ";
            Code[93, (int)fontType] = "EÅ";
            Code[94, (int)fontType] = "EÃ";
            Code[95, (int)fontType] = "EÄ";
            Code[96, (int)fontType] = "Í";
            Code[97, (int)fontType] = "Ì";
            Code[98, (int)fontType] = "Æ";
            Code[99, (int)fontType] = "Ó";
            Code[100, (int)fontType] = "Ò";
            Code[101, (int)fontType] = "OÙ";
            Code[102, (int)fontType] = "OØ";
            Code[103, (int)fontType] = "OÛ";
            Code[104, (int)fontType] = "OÕ";
            Code[105, (int)fontType] = "OÏ";
            Code[106, (int)fontType] = "OÂ";
            Code[107, (int)fontType] = "OÁ";
            Code[108, (int)fontType] = "OÀ";
            Code[109, (int)fontType] = "OÅ";
            Code[110, (int)fontType] = "OÃ";
            Code[111, (int)fontType] = "OÄ";
            Code[112, (int)fontType] = "Ô";
            Code[113, (int)fontType] = "ÔÙ";
            Code[114, (int)fontType] = "ÔØ";
            Code[115, (int)fontType] = "ÔÛ";
            Code[116, (int)fontType] = "ÔÕ";
            Code[117, (int)fontType] = "ÔÏ";
            Code[118, (int)fontType] = "UÙ";
            Code[119, (int)fontType] = "UØ";
            Code[120, (int)fontType] = "UÛ";
            Code[121, (int)fontType] = "UÕ";
            Code[122, (int)fontType] = "UÏ";
            Code[123, (int)fontType] = "Ö";
            Code[124, (int)fontType] = "ÖÙ";
            Code[125, (int)fontType] = "ÖØ";
            Code[126, (int)fontType] = "ÖÛ";
            Code[127, (int)fontType] = "ÖÕ";
            Code[128, (int)fontType] = "ÖÏ";
            Code[129, (int)fontType] = "YÙ";
            Code[130, (int)fontType] = "YØ";
            Code[131, (int)fontType] = "YÛ";
            Code[132, (int)fontType] = "YÕ";
            Code[133, (int)fontType] = "Î";
            Code[134, (int)fontType] = "Ñ";
        }
    }
}
