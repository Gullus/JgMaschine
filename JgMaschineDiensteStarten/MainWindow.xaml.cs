using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JgMaschineDiensteStarten
{
  public partial class MainWindow : Window
  {
    private List<CDienste> _ListeDienste;

    public MainWindow()
    {
      InitializeComponent();

      var dienste = Properties.Settings.Default.ListeDienste.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
      _ListeDienste = dienste.Select(s => new CDienste(s.Trim())).ToList();

      foreach (var dienst in _ListeDienste)
      {
        ServiceController sc = new ServiceController(dienst.Name);
        try
        {
          dienst.Status = sc.Status;
        }
        catch (Exception ex)
        {
          dienst.IsChecked = false;
          dienst.IstOk = false;
          dienst.FehlerText = ex.Message;
        }
      }

      (FindResource("CvsDienste") as CollectionViewSource).Source = _ListeDienste;
    }

    private void BtnDienstStarten_Click(object sender, RoutedEventArgs e)
    {
      var msg = "Ausgewählte Dienset starten ?";
      if (MessageBox.Show(msg, "Dienste starten !", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
      {
        foreach (var dienst in _ListeDienste.Where(w => (w.IsChecked) && (w.IstOk)).ToList())
        {
          ServiceController sc = new ServiceController(dienst.Name);
          if (sc.Status == ServiceControllerStatus.Stopped)
          {
            dienst.FehlerText = "";
            dienst.Status = null;

            try
            {
              sc.Start();
              sc.WaitForStatus(ServiceControllerStatus.Running);

              dienst.Status = sc.Status;
            }
            catch (Exception ex)
            {
              dienst.FehlerText = ex.Message;
            }
          }
        }

        (FindResource("CvsDienste") as CollectionViewSource).View.Refresh();
      }
    }

    private void BtnDienstBeenden_Click(object sender, RoutedEventArgs e)
    {
      var msg = "Ausgewählte Dienset beenden ?";
      if (MessageBox.Show(msg, "Dienste beenden !", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
      {
        foreach (var dienst in _ListeDienste.Where(w => (w.IsChecked) && (w.IstOk)).ToList())
        {
          ServiceController sc = new ServiceController(dienst.Name);
          if (sc.Status == ServiceControllerStatus.Running)
          {
            dienst.FehlerText = "";
            dienst.Status = null;

            try
            {
              sc.Stop();
              sc.WaitForStatus(ServiceControllerStatus.Stopped);

              dienst.Status = sc.Status;
            }
            catch (Exception ex)
            {
              dienst.FehlerText = ex.Message;
            }
          }
        }

        (FindResource("CvsDienste") as CollectionViewSource).View.Refresh();
      }
    }
  }

  public class CDienste
  {
    public bool IsChecked { get; set; } = true;
    public string Name { get; set; }
    public ServiceControllerStatus? Status { get; set; } = null;
    public bool IstOk { get; set; } = true;
    public string FehlerText { get; set; } = "";

    public CDienste(string NeuName)
    {
      Name = NeuName;
    }
  }
}
