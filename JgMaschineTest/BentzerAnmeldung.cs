using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

// This sample demonstrates the use of the WindowsIdentity class to impersonate a user.
// IMPORTANT NOTES:
// This sample requests the user to enter a password on the console screen.
// Because the console window does not support methods allowing the password to be masked,
// it will be visible to anyone viewing the screen.
// On Windows Vista and later this sample must be run as an administrator. 

namespace JgMaschineTest
{
  public static class BenutzerAnmeldung
  {
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public extern static bool CloseHandle(IntPtr handle);

    // If you incorporate this code into a DLL, be sure to demand FullTrust.
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public static void Anmeldung(string BenutzerName, string BenutzerKennwort, string DomaenenName, string PfadDatei, string BvbsCode)
    {
      Console.WriteLine($"Benutzername: {BenutzerName}");
      Console.WriteLine($"BenutzerKennwort: {BenutzerKennwort}");
      Console.WriteLine($"Domäne: {DomaenenName}");
      Console.WriteLine($"Bvbs Code: {BvbsCode}");

      SafeTokenHandle safeTokenHandle;
      try
      {
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token.
        const int LOGON32_LOGON_INTERACTIVE = 2;

        // Call LogonUser to obtain a handle to an access token.
        bool returnValue = LogonUser(BenutzerName, DomaenenName, BenutzerKennwort,
            LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

        Console.WriteLine("LogonUser called.");

        if (false == returnValue)
        {
          int ret = Marshal.GetLastWin32Error();
          Console.WriteLine("LogonUser failed with error code : {0}", ret);
          throw new System.ComponentModel.Win32Exception(ret);
        }
        using (safeTokenHandle)
        {
          Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
          Console.WriteLine("Value of Windows NT token: " + safeTokenHandle);

          // Check the identity.
          Console.WriteLine("Before impersonation: " + WindowsIdentity.GetCurrent().Name);
          // Use the token handle returned by LogonUser.
          using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
          {
            using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
            {
              var maAdresse = string.Format(@"\\{0}\", DomaenenName);

              string dat = maAdresse + PfadDatei + @"\Auftrag.txt";
              try
              {
                Console.WriteLine($"Bauteil in Datei: {dat} schreiben.");
                File.WriteAllText(dat, BvbsCode, Encoding.UTF8);
                Console.WriteLine("Datei geschreiben !");
              }
              catch (Exception f)
              {
                string s = $"Fehler beim schreiben der Progress Produktionsliste.\nDatei: {dat}.\nGrund: {f.Message}";
                Console.WriteLine(s);
              }
            }
          }
          Console.WriteLine("Context geschlossen: " + WindowsIdentity.GetCurrent().Name);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception occurred. " + ex.Message);
      }

    }
  }
  public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
  {
    private SafeTokenHandle()
        : base(true)
    { }

    [DllImport("kernel32.dll")]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    protected override bool ReleaseHandle()
    {
      return CloseHandle(handle);
    }
  }
}