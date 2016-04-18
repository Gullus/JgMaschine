using System.Windows.Input;

namespace JgMaschineSetup.Commands
{
  public static class MyCommands
  {
    private static RoutedUICommand _MaschineBearbeiten;
    public static RoutedUICommand MaschineBearbeiten
    {
      get { return _MaschineBearbeiten; }
    }

    private static RoutedUICommand _BedienerBeabeiten;
    public static RoutedUICommand BedienerBeabeiten
    {
      get { return _BedienerBeabeiten; }
    }

    private static RoutedUICommand _StandortBearbeiten;
    public static RoutedUICommand StandortBearbeiten
    {
      get { return _StandortBearbeiten; }
    }

    private static RoutedUICommand _ProtokollBearbeiten;
    public static RoutedUICommand ProtokollBearbeiten
    {
      get { return _ProtokollBearbeiten; }
    }

    private static RoutedUICommand _ReportBenutzerBearbeiten;
    public static RoutedUICommand ReportBenutzerBearbeiten
    {
      get { return _ReportBenutzerBearbeiten; }
    }

    private static RoutedUICommand _ReportBenutzerAnzeigen;
    public static RoutedUICommand ReportBenutzerAnzeigen
    {
      get { return _ReportBenutzerAnzeigen; }
    }

    static MyCommands()
    {
      _MaschineBearbeiten = new RoutedUICommand("Maschine bearbeiten", "MaschineBearbeiten", typeof(MyCommands));
      _BedienerBeabeiten = new RoutedUICommand("Bediener bearbeiten", "BedienerBearbeiten", typeof(MyCommands));
      _StandortBearbeiten = new RoutedUICommand("Standort bearbeiten", "StandortBearbeiten", typeof(MyCommands));
      _ProtokollBearbeiten = new RoutedUICommand("Protokoll bearbeiten", "ProtokollBearbeiten", typeof(MyCommands));

      _ReportBenutzerBearbeiten = new RoutedUICommand("Benutzer bearbeiten", "BenutzerBearbeiten", typeof(MyCommands));
      _ReportBenutzerAnzeigen = new RoutedUICommand("Benutzer Anzeigen", "BenutzerAnzeigen", typeof(MyCommands));
    }
  }
}
