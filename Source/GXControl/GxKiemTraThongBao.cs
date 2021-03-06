using System;
using System.Collections.Generic;
using System.Text;
using GxGlobal;
using System.ComponentModel;

namespace GxControl
{
    public class GxKiemTraThongBao : IGxProcess
    {
        public event EventHandler OnStart = null;
        public event CancelEventHandler OnError = null;
        public event EventHandler OnFinished = null;
        public event EventHandler OnExecuting = null;
        private ProcessOptions processOptions = ProcessOptions.MergeData;
        public ProcessOptions ProcessOptions
        {
            get { return processOptions; }
            set { processOptions = value; }
        }

        private object processData = null;
        public object ProcessData
        {
            get { return processData; }
            set { processData = value; }
        }

        public void Execute()
        {
            try
            {
                processData = "";
                if (OnStart != null) OnStart(this, EventArgs.Empty);
                if (Memory.IsConnectionAvailable())
                {
                    System.Net.WebClient web = new System.Net.WebClient();
                    //check new version
                    if (!Memory.ServerUrl.EndsWith("/")) Memory.ServerUrl += "/";
                    processData = web.DownloadString(Memory.ServerUrl + "message.txt").Replace("ï»¿", "");
                }
            }
            catch (Exception ex)
            {
                processData = "";
                if (OnError != null) OnError(ex.Message, new CancelEventArgs());
            }
            finally
            {
                if (OnFinished != null)
                {
                    OnFinished(this, EventArgs.Empty);
                }
            }
        }
    }
}
