using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate
{
    public enum UpdateType
    {
        /// <summary>
        /// Download one archive file and unzip all application files
        /// </summary>
        UpdateZipFile,
        /// <summary>
        /// Download newest files only
        /// </summary>
        UpdateEveryLibrary
    }
    public class Constants
    {
        public const string CONFIG_FILE = "VersionConfig.xml";
    }
}
