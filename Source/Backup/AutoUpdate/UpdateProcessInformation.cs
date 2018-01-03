using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate
{
    public class UpdateProcessInformation
    {
        private int applicationId = 0;

        public int ApplicationId
        {
            get { return applicationId; }
            set { applicationId = value; }
        }

        private string applicationName = "";

        public string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }
        private string currentVersion = "";

        public string CurrentVersion
        {
            get { return currentVersion; }
            set { currentVersion = value; }
        }
        private string serverUrl = "http://quanlygiaoxu.net/";

        public string ServerUrl
        {
            get { return serverUrl; }
            set { serverUrl = value; }
        }
        private string serverVersion = "";

        public string ServerVersion
        {
            get { return serverVersion; }
            set { serverVersion = value; }
        }
        private string updateInfo = "";

        public string UpdateInfo
        {
            get { return updateInfo; }
            set { updateInfo = value; }
        }
        private string applicationFolder = "";

        public string ApplicationFolder
        {
            get { return applicationFolder; }
            set { applicationFolder = value; }
        }

        private UpdateType updateType = UpdateType.UpdateZipFile;

        public UpdateType UpdateType
        {
            get { return updateType; }
            set { updateType = value; }
        }

        private string errorInfo = "";

        public string ErrorInfo
        {
            get { return errorInfo; }
            set { errorInfo = value; }
        }
    }
}
