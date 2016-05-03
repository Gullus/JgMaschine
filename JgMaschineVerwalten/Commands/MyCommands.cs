using System.Windows.Input;

namespace JgMaschineVerwalten.Commands
{
  public static class MyCommands
  {
    private static RoutedUICommand _ReparaturNeu;
    public static RoutedUICommand ReparaturNeu
    {
      get { return _ReparaturNeu; }
    }

    private static RoutedUICommand _AnmeldungBediener;
    public static RoutedUICommand AnmeldungBediener
    {
      get { return MyCommands._AnmeldungBediener; }
    }

    static MyCommands()
    {
      _ReparaturNeu = new RoutedUICommand("Reparatur Neu", "ReparaturNeu", typeof(MyCommands));
      _AnmeldungBediener = new RoutedUICommand("Anmeldung Bediener", "AnmeldungBediener", typeof(MyCommands));
    }
  }
}
