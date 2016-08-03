using System.Windows.Input;

namespace JgMaschineAuswertung.Commands
{
  public static class MyCommands
  {
    private static RoutedUICommand _ReportAnzeigen;
    public static RoutedUICommand ReportAnzeigen
    {
      get { return _ReportAnzeigen; }
    }

    private static RoutedUICommand _ReportDrucken;
    public static RoutedUICommand ReportDrucken
    {
      get { return _ReportDrucken; }
    }

    private static RoutedUICommand _ReportNeu;
    public static RoutedUICommand ReportNeu
    {
      get { return _ReportNeu; }
    }

    private static RoutedUICommand _ReportAusDateiLaden;
    public static RoutedUICommand ReportAusDateiLaden
    {
      get { return _ReportAusDateiLaden; }
    }

    private static RoutedUICommand _ReportInDateiSpeichern;
    public static RoutedUICommand ReportInDateiSpeichern
    {
      get { return _ReportInDateiSpeichern; }
    }

    private static RoutedUICommand _ReportBearbeiten;
    public static RoutedUICommand ReportBearbeiten
    {
      get { return _ReportBearbeiten; }
    }

    private static RoutedUICommand _ReportOptionen;
    public static RoutedUICommand ReportOptionen
    {
      get { return _ReportOptionen; }
    }

    private static RoutedUICommand _SqlVerbindung;
    public static RoutedUICommand SqlVerbindung
    {
      get { return _SqlVerbindung; }
    }

    static MyCommands()
    {
      _ReportAnzeigen = new RoutedUICommand("Report anzeigen", "ReportAnzeigen", typeof(MyCommands));
      _ReportDrucken = new RoutedUICommand("Report drucken", "RportDrucken", typeof(MyCommands));
      _ReportNeu = new RoutedUICommand("Report Neu", "ReportNeu", typeof(MyCommands));
      _ReportAusDateiLaden = new RoutedUICommand("Report laden", "ReportLaden", typeof(MyCommands));
      _ReportInDateiSpeichern = new RoutedUICommand("Report Speichern", "ReportSpeichern", typeof(MyCommands));
      _ReportBearbeiten = new RoutedUICommand("Report bearbeiten", "ReportBearbeiten", typeof(MyCommands));
      _ReportOptionen = new RoutedUICommand("Report Optionen", "ReportOptionen", typeof(MyCommands));
      _SqlVerbindung = new RoutedUICommand("Sql Verbindung", "SqlVerbinung", typeof(MyCommands));
    }
  }
}
