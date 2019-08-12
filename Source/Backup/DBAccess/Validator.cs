using System;
using System.Collections.Generic;
using System.Text;

namespace GxGlobal
{
    public class Validator
    {
        public static bool IsNumber(string s)
        {
            try
            {
                int i = int.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsYear(string year)
        {
            string date = "1/1/" + year;
            try
            {
                DateTime d = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsMonth(string month)
        {
            try
            {
                if (int.Parse(month) > 12) return false;
                string date = month + "/1" + "/2009";
                try
                {
                    DateTime d = DateTime.Parse(date, Memory.EnCultureInfo.DateTimeFormat);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool IsDate(string date)
        {
            try
            {
                DateTime d = DateTime.Parse(date, Memory.EnCultureInfo.DateTimeFormat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDate(string year, string month, string day)
        {
            try
            {
                DateTime d = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
