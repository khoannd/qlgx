using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Threading;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace AutoUpdate
{
    public class AutoUpdate
    {
        public event EventHandler OnStart;
        public event EventHandler OnDownloading;
        public event EventHandler OnError;
        public event EventHandler OnFinished;

        private UpdateProcessInformation information = new UpdateProcessInformation();

        public UpdateProcessInformation Information
        {
            get { return information; }
            set { information = value; }
        }

        XmlNamespaceManager nsmgr = null;

        private XmlDocument docVersion = new XmlDocument();

        public AutoUpdate()
        {
            information.ApplicationFolder = System.Windows.Forms.Application.StartupPath + "\\";
        }

        private bool downloadFile(string url, string target)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadFile(url, target);
                return true;
            }
            catch(Exception ex)
            {
                if (ex.Message == "Unable to connect to the remote server")
                {
                    information.ErrorInfo = ex.Message;
                }
                else if (ex.Message == "The remote server returned an error: (404) Not Found.")
                {
                    information.ErrorInfo = "Can't download version information file from remote server";
                }
                else if (ex.Message.Contains("The remote name could not be resolved"))
                {
                    information.ErrorInfo = "The updating website can't found";
                }
                else
                {
                    information.ErrorInfo = "Can't download version information file from remote server";
                }
                
                return false;
            }
        }

        public bool GetVersionInfo()
        {
            try
            {
                XmlDocument docVersion = new XmlDocument();
                docVersion.Load(information.ApplicationFolder + Constants.CONFIG_FILE);

                // Create an XmlNamespaceManager to resolve the default namespace.
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(docVersion.NameTable);
                nsmgr.AddNamespace("vs", "urn:newversion-schema");

                XmlElement root = docVersion.DocumentElement;
                XmlNodeList nodeList = root.SelectNodes("/vs:application/vs:version-info", nsmgr);
                if (nodeList != null && nodeList.Count > 0)
                {
                    information.CurrentVersion = nodeList[0].Attributes["value"].Value;
                }

                nodeList = docVersion.DocumentElement.SelectNodes("/vs:application/vs:version-info/vs:downloadpath", nsmgr);
                if (nodeList != null && nodeList.Count > 0)
                {
                    information.ServerUrl = nodeList[0].InnerText;
                }
                return true;
            }
            catch(Exception ex) {
                information.ErrorInfo = ex.Message;
                return false;
            }
        }

        public int CheckVersionInfo()
        {
            if (!GetVersionInfo())
            {
                information.ErrorInfo = "Can't load version config file!";
                return -1;
            }
            string localTempPath = Path.GetTempFileName();
            if (downloadFile(information.ServerUrl + Constants.CONFIG_FILE, localTempPath))
            {
                try
                {
                    docVersion.Load(localTempPath);

                    // Create an XmlNamespaceManager to resolve the default namespace.
                    nsmgr = new XmlNamespaceManager(docVersion.NameTable);
                    nsmgr.AddNamespace("vs", "urn:newversion-schema");

                    XmlElement root = docVersion.DocumentElement;
                    XmlNodeList nodeList = root.SelectNodes("/vs:application/vs:version-info", nsmgr);
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        information.ServerVersion = nodeList[0].Attributes["value"].Value;
                        information.ApplicationName = nodeList[0].Attributes["exename"].Value;
                        if (compareVersion() > 0)
                        {
                            try
                            {
                                nodeList = docVersion.DocumentElement.SelectNodes("/vs:application/vs:version-info/vs:info", nsmgr);
                                if (nodeList != null && nodeList.Count > 0)
                                {
                                    information.UpdateInfo = nodeList[0].InnerText;
                                }
                            }
                            catch { }
                            return 1;//has new version
                        }
                        else return 0;//current versionis newest
                    }
                }
                catch
                {
                    information.ErrorInfo = "Can't read version configure file";
                    return -1;
                }
            }
            return -1;//error
        }

        private int compareVersion()
        {
            return string.Compare(information.ServerVersion, information.CurrentVersion);
        }

        public void Execute()
        {
            try
            {
                if (information.ServerVersion == "")
                {
                    int check = CheckVersionInfo();
                    if (check == -1)
                    {
                        if (OnError != null) OnError(information.ErrorInfo, EventArgs.Empty);
                        return;
                    }
                    else if (check == 0)
                    {
                        if (OnFinished != null) OnFinished("No update available. Your version is newest version", EventArgs.Empty);
                        return;
                    }
                }

                if (OnStart != null) OnStart(this, EventArgs.Empty);
                XmlNodeList nodeList = null;
                if (information.UpdateType == UpdateType.UpdateZipFile)
                {
                    nodeList = docVersion.DocumentElement.SelectNodes("/vs:application/vs:version-info/vs:downloadpath", nsmgr);
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        XmlNode node = nodeList[0];
                        string fileServerPath = node.InnerText + node.Attributes["value"].Value;
                        string localFile = Path.GetTempFileName();
                        if (downloadFile(fileServerPath, localFile))
                        {
                            //extract to application folder
                            if (OnDownloading != null) OnDownloading("...", EventArgs.Empty);
                            FastZip fzip = new FastZip();
                            fzip.ExtractZip(localFile, information.ApplicationFolder, FastZip.Overwrite.Always, null, "", "", true);
                            try
                            {
                                File.Delete(information.ApplicationFolder + Constants.CONFIG_FILE);
                                downloadFile(information.ServerUrl + Constants.CONFIG_FILE, information.ApplicationFolder + Constants.CONFIG_FILE);
                            }
                            catch { }

                            if (OnFinished != null) OnFinished("Update successfully", EventArgs.Empty);
                            Process.Start(information.ApplicationFolder + information.ApplicationName, "update " + information.ServerVersion);
                            try
                            {
                                File.Delete(localFile);
                            }
                            catch { }
                        }
                    }
                }
                else if (information.UpdateType == UpdateType.UpdateEveryLibrary)
                {
                    nodeList = docVersion.DocumentElement.SelectNodes("/vs:application/vs:version-info/vs:folder", nsmgr);
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        string localFolder = information.ApplicationFolder + "Update\\";
                        if (Directory.Exists(localFolder))
                        {
                            Directory.CreateDirectory(localFolder);
                        }
                        XmlNode node = nodeList[0];
                        string folderPath = node.Attributes["value"].Value;
                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            string filePath = folderPath + childNode.Attributes["value"].Value;
                            string localFile = localFolder + childNode.Attributes["value"].Value;
                            if (OnDownloading != null) OnDownloading(childNode.Attributes["value"].Value, EventArgs.Empty);
                            if (!downloadFile(filePath, localFile))
                            {
                                //abort update process
                                if (OnError != null) OnError("Download failed", EventArgs.Empty);
                            }
                        }
                        if (OnFinished != null) OnFinished("Update successfully", EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                information.ErrorInfo = ex.Message;
                if (OnError != null) OnError(ex.Message, EventArgs.Empty);
            }
        }
    }
}
