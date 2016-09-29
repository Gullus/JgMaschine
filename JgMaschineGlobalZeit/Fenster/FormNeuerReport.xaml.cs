using System.Windows;

namespace JgGlobalZeit.Fenster
{
  public partial class FormNeuerReport : Window
  {
    public string ReportName
    {
      get { return tbReportName.Text;  }
    }

    public FormNeuerReport()
    {
      InitializeComponent();
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
