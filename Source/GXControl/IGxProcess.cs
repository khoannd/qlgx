using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GxControl
{
    public interface IGxProcess
    { 
        event EventHandler OnStart;
        event CancelEventHandler OnError;
        event EventHandler OnFinished;
        event EventHandler OnExecuting;
        void Execute();
        GxGlobal.ProcessOptions ProcessOptions { get;set;}
        object ProcessData { get;set;}
    }
}
