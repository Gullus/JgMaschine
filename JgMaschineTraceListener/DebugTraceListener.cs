﻿using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace JgMaschineTraceListener
{
  [ConfigurationElementType(typeof(CustomTraceListenerData))]
  public class DebugTraceListener : CustomTraceListener
  {
    public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
    {
      if (data is LogEntry && this.Formatter != null)
      {
        this.WriteLine(this.Formatter.Format(data as LogEntry));
      }
      else
      {
        this.WriteLine(data.ToString());
      }
    }

    public override void Write(string message)
    {
      Console.Write(message);
    }

    public override void WriteLine(string message)
    {
      Console.WriteLine(message);
    }
  }
}
