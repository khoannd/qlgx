using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DongBoDuLieu
{
    class ReadFileCSV
    {
        private List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
        
        public ReadFileCSV(string fileName)
        {
            if (File.Exists(fileName))
            {
                string line = "";
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string[] header = sr.ReadLine().Split(';');
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] data =line.Split(';');
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        for (int i = 0; i < header.Length-1; i++)
                        {
                            dic.Add(header[i], data[i]);
                        }
                        this.Data.Add(dic);
                    }
                }
            }
        }

        public List<Dictionary<string, string>> Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
    }
}
