using System.Windows.Input;

namespace JgMaschineGlobalZeit.Commands
{
  public static class MyCommands
  {
    private static RoutedUICommand _ArbeitszeitLoeschen;
    public static RoutedUICommand ArbeitszeitLoeschen
    {
      get { return _ArbeitszeitLoeschen; }
    }

    private static RoutedUICommand _SollStundenAendern;
    public static RoutedUICommand SollStundenAendern
    {
      get { return _SollStundenAendern; }
    }

    private static RoutedUICommand _UeberstundenBezahltAendern;
    public static RoutedUICommand UberstundenBezahltAendern
    {
      get { return _UeberstundenBezahltAendern; }
    }

    static MyCommands()
    {
      _ArbeitszeitLoeschen = new RoutedUICommand("Arbeitszeit Löschen", "ArbeitszeitLoeschen", typeof(MyCommands));
      _SollStundenAendern = new RoutedUICommand("Solllstunden ändern", "SollstundenAendern", typeof(MyCommands));
      _UeberstundenBezahltAendern = new RoutedUICommand("Überstunden bezahlt ändern", "UeberstundenBezahltAendern", typeof(MyCommands));
    }
  }
}

