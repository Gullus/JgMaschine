using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JgMaschineVerwalten
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
      EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText));
      EventManager.RegisterClassHandler(typeof(TextBox), Control.MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText));

      base.OnStartup(e);
    }

    void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
    {
      DependencyObject parent = e.OriginalSource as UIElement;
      while (parent != null && !(parent is TextBox))
      {
        parent = VisualTreeHelper.GetParent(parent);
      }

      if (parent != null)
      {
        var textBox = (TextBox)parent;
        if (!textBox.IsKeyboardFocusWithin)
        {
          textBox.Focus();
          e.Handled = true;
        }
      }
    }

    void SelectAllText(object sender, RoutedEventArgs e)
    {
      var textBox = e.OriginalSource as TextBox;
      if (textBox != null)
      {
        textBox.SelectAll();
      }
    }
  }
}
