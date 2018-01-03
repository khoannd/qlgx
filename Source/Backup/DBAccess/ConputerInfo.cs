using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.IO;

namespace GxGlobal
{
    public class ComputerInfo
    {
        public static string GetUniqueID()
        {
            return System.Guid.NewGuid().ToString().Replace("-", "").Substring(0, 19).ToUpper();
            //string drive = Environment.SystemDirectory;
            //System.IO.DirectoryInfo dirInfo = new DirectoryInfo(drive);
            //drive = dirInfo.Root.Name;

            //if (drive.EndsWith(":\\"))
            //{
            //    //C:\ -> C
            //    drive = drive.Substring(0, drive.Length - 2);
            //}

            //string volumeSerial = getVolumeSerial(drive);
            //string cpuID = getCPUID();

            ////Mix them up and remove some useless 0's
            //return cpuID.Substring(13) + cpuID.Substring(1, 4) + volumeSerial + cpuID.Substring(4, 4);
        }

        private static string getVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        private static string getCPUID()
        {
            string cpuInfo = "";
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();

            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    //Get only the first CPU's ID
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            return cpuInfo;
        }
    }
}
