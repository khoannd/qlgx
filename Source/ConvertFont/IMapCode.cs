using System;
using System.Collections.Generic;
using System.Text;

namespace ConvertDB
{
    interface IMapCode
    {
        FontIndex FontType { get; set;}
        void MapCode(ref string[,] Code);        
    }
}
