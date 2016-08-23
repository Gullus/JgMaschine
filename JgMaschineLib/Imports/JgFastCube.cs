using System.Runtime.InteropServices;

namespace JgMaschineLib.Imports
{
  public static class JgFastCube
  {
    [DllImport("JgFastCube.dll")]
    public static extern void JgFastCubeStart([MarshalAs(UnmanagedType.BStr)]string ConnectionString, [MarshalAs(UnmanagedType.BStr)] string SqlText, [MarshalAs(UnmanagedType.BStr)] string PfadOptione);

    [DllImport("JgFastCube.dll")]
    public static extern void MessageTest1();

    [DllImport("JgFastCube.dll")]
    public static extern void MessageTest2([MarshalAs(UnmanagedType.BStr)]string MeinText);
  }
}
