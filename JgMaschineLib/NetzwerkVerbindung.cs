using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;

namespace JgMaschineLib
{

  //using System;
  //using System.IO;
  //using System.Net;

  //class Program
  //{
  //  static void Main(string[] args)
  //  {
  //    var networkPath = @"//server/share";
  //    var credentials = new NetworkCredential("username", "password");

  //    using (new NetzwerkVerbindung(networkPath, credentials))
  //    {
  //      var fileList = Directory.GetFiles(networkPath);
  //    }

  //    foreach (var file in fileList)
  //    {
  //      Console.WriteLine("{0}", Path.GetFileName(file));
  //    }
  //  }
  //}

  public class NetzwerkVerbindung : IDisposable
  {
    readonly string _networkName;

    public NetzwerkVerbindung(string networkName, NetworkCredential credentials)
    {
      _networkName = networkName;

      var netResource = new NetResource
      {
        Scope = ResourceScope.GlobalNetwork,
        ResourceType = ResourceType.Disk,
        DisplayType = ResourceDisplaytype.Share,
        RemoteName = networkName
      };

      var userName = string.IsNullOrEmpty(credentials.Domain)
          ? credentials.UserName
          : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

      var result = WNetAddConnection2(
          netResource,
          credentials.Password,
          userName,
          0);

      if (result != 0)
      {
        throw new Exception("Error connecting to remote share. Fehler: " + result.ToString());
      }
    }

    ~NetzwerkVerbindung()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      WNetCancelConnection2(_networkName, 0, true);
    }

    [DllImport("mpr.dll")]
    private static extern int WNetAddConnection2(NetResource netResource,
        string password, string username, int flags);

    [DllImport("mpr.dll")]
    private static extern int WNetCancelConnection2(string name, int flags,
        bool force);

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
      public ResourceScope Scope;
      public ResourceType ResourceType;
      public ResourceDisplaytype DisplayType;
      public int Usage;
      public string LocalName;
      public string RemoteName;
      public string Comment;
      public string Provider;
    }

    public enum ResourceScope : int
    {
      Connected = 1,
      GlobalNetwork,
      Remembered,
      Recent,
      Context
    };

    public enum ResourceType : int
    {
      Any = 0,
      Disk = 1,
      Print = 2,
      Reserved = 8,
    }

    public enum ResourceDisplaytype : int
    {
      Generic = 0x0,
      Domain = 0x01,
      Server = 0x02,
      Share = 0x03,
      File = 0x04,
      Group = 0x05,
      Network = 0x06,
      Root = 0x07,
      Shareadmin = 0x08,
      Directory = 0x09,
      Tree = 0x0a,
      Ndscontainer = 0x0b
    }
  }
}