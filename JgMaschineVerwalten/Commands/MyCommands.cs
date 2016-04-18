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

    private static RoutedUICommand _ReparaturBearbeiten;
    public static RoutedUICommand ReparaturBearbeiten
    {
      get { return _ReparaturBearbeiten; }
    }

    private static RoutedUICommand _ManuelleAnmeldungBediener;
    public static RoutedUICommand ManuelleAnmeldungBediener
    {
      get { return MyCommands._ManuelleAnmeldungBediener; }
    }

    private static RoutedUICommand _BedienerUmmelden;
    public static RoutedUICommand BedienerUmmelden
    {
      get { return MyCommands._BedienerUmmelden; }
    }

    private static RoutedUICommand _ManuelleAbmeldungBediener;
    public static RoutedUICommand ManuelleAbmeldungBediener
    {
      get { return MyCommands._ManuelleAbmeldungBediener; }
    }

    static MyCommands()
    {
      _ReparaturNeu = new RoutedUICommand("Reparatur Neu", "ReparaturNeu", typeof(MyCommands));
      _ReparaturBearbeiten = new RoutedUICommand("Reperatur bearbeiten", "ReparaturBearbeiten", typeof(MyCommands));
      _ManuelleAnmeldungBediener = new RoutedUICommand("Anmeldung Bediener", "AnmeldungBediener", typeof(MyCommands));
      _BedienerUmmelden = new RoutedUICommand("Bediener ummelden", "BedienerUmmelden", typeof(MyCommands));
      _ManuelleAbmeldungBediener = new RoutedUICommand("Bediener abmelden", "BedienerAbmelden", typeof(MyCommands));
    }
  }
}
