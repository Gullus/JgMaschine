using System.Windows.Input;

namespace JgGlobalZeit.Commands
{
  public static class MyCommands
    {
      private static RoutedUICommand _ArbeitszeitLoeschen;
      public static RoutedUICommand ArbeitszeitLoeschen
      {
        get { return _ArbeitszeitLoeschen; }
      }

      static MyCommands()
      {
        _ArbeitszeitLoeschen = new RoutedUICommand("Arbeitszeit Löschen", "ArbeitszeitLoeschen", typeof(MyCommands));
      }
    }
  }

