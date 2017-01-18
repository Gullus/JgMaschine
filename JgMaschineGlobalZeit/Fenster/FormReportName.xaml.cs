using System.Windows;

namespace JgMaschineGlobalZeit.Fenster
{
  public partial class FormReportName : Window
  {
    public string ReportName
    {
      get { return tbReportName.Text;  }
    }

    public FormReportName(string NeuReportName = "")
    {
      InitializeComponent();
      tbReportName.Text = NeuReportName;
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }
  }
}
