using JgMaschineAuswertung.Commands;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;

namespace JgMaschineAuswertung
{
  public partial class MainWindow : RibbonWindow
  {
    private JgMaschineData.JgModelContainer _Db;
    private CollectionViewSource _VsAuswertung { get { return (System.Windows.Data.CollectionViewSource)FindResource("vsAuswertung"); } }

    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private string _FileSqlVerbindung = "JgMaschineSqlVerbindung.Xml";

    public MainWindow()
    {
      InitializeComponent();

      _Db = new JgMaschineData.JgModelContainer();

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          var ausw = (JgMaschineData.tabAuswertung)_VsAuswertung.View.CurrentItem;

          repEvent.Report.Save(memStr);
          ausw.Report = memStr.ToArray();
          ausw.GeaendertDatum = DateTime.Now;
          ausw.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

          _Db.SaveChanges();
        }
        catch (Exception f)
        {
          System.Windows.MessageBox.Show("Fehler beim speichern des Reports !\r\nGrund: " + f.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
          memStr.Dispose();
        }
      };

      InitCommands();
    }

    private void InitCommands()
    {
      CommandBindings.Add(new CommandBinding(MyCommands.ReportNeu, (sen, erg) =>
      {
        JgMaschineAuswertung.Fenster.FormAuswertungBearbeiten form = new Fenster.FormAuswertungBearbeiten(null);
        if (form.ShowDialog() ?? false)
        {
          string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          form.Auswertung.ErstelltDatum = DateTime.Now;
          form.Auswertung.ErstelltName = username;
          form.Auswertung.Filter = "JgMaschineSqlVerbindung";

          form.Auswertung.GeaendertName = form.Auswertung.ErstelltName;
          form.Auswertung.GeaendertDatum = form.Auswertung.ErstelltDatum;

          _Db.tabAuswertungSet.Add(form.Auswertung);
          _Db.SaveChanges();
          _VsAuswertung.Source = _Db.tabAuswertungSet.OrderBy(o => o.ReportName).ToList();
          _VsAuswertung.View.MoveCurrentTo(form.Auswertung);

          _Report.Clear();

          JgMaschineLib.SqlVerbindung.SqlVerbindung verb = new JgMaschineLib.SqlVerbindung.SqlVerbindung(_FileSqlVerbindung);
          _Report.SetParameterValue("SqlVerbindung", verb);
          _Report.Design();
        }
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportAnzeigen, ExceReportAnzeigenDruck, CanExecReportVorhandenAndNull));
      CommandBindings.Add(new CommandBinding(MyCommands.ReportDrucken, ExceReportAnzeigenDruck, CanExecReportVorhandenAndNull));
      CommandBindings.Add(new CommandBinding(MyCommands.ReportBearbeiten, ExceReportAnzeigenDruck, CanExecReportVorhanden));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportLaden, (sen, erg) =>
      {
        Microsoft.Win32.OpenFileDialog dia = new Microsoft.Win32.OpenFileDialog();
        dia.Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*";
        dia.FilterIndex = 1;
        if (dia.ShowDialog() ?? false)
        {
          MemoryStream mem = new MemoryStream();
          using (Stream f = File.OpenRead(dia.FileName))
          {
            f.CopyTo(mem);
          }
          (_VsAuswertung.View.CurrentItem as JgMaschineData.tabAuswertung).Report = mem.ToArray();
          _Db.SaveChanges();
        }

      }, CanExecReportVorhanden));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportSpeichern, (sen, erg) =>
      {
        Microsoft.Win32.SaveFileDialog dia = new Microsoft.Win32.SaveFileDialog();
        dia.Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*";
        dia.FilterIndex = 1;
        if (dia.ShowDialog() ?? false)
        {
          MemoryStream mem;
          mem = new MemoryStream((_VsAuswertung.View.CurrentItem as JgMaschineData.tabAuswertung).Report);
          using (Stream f = File.Create(dia.FileName))
          {
            mem.CopyTo(f);
          }
        }
      }, CanExecReportVorhandenAndNull));

      CommandBindings.Add(new CommandBinding(MyCommands.SqlVerbindung, (sen, erg) =>
      {
        JgMaschineLib.SqlVerbindung.SqlVerbindung verb = new JgMaschineLib.SqlVerbindung.SqlVerbindung(_FileSqlVerbindung);
        verb.VerbindungBearbeiten(JgMaschineLib.SqlVerbindung.SqlVerbindung.EnumVerbindungen.JgMaschine);
      }));

    }

    private void CanExecReportVorhandenAndNull(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = (_VsAuswertung.View.CurrentItem != null) && ((_VsAuswertung.View.CurrentItem as JgMaschineData.tabAuswertung).Report != null);
    }

    private void CanExecReportVorhanden(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = _VsAuswertung.View.CurrentItem != null;
    }

    private void ExceReportAnzeigenDruck(object sender, ExecutedRoutedEventArgs e)
    {
      _Report.Clear();
      var dsRep = (JgMaschineData.tabAuswertung)_VsAuswertung.View.CurrentItem;

      if (dsRep.Report != null)
      {
        MemoryStream mem;
        mem = new MemoryStream(dsRep.Report);
        _Report.Load(mem);
      }

      JgMaschineLib.SqlVerbindung.SqlVerbindung verb = new JgMaschineLib.SqlVerbindung.SqlVerbindung(_FileSqlVerbindung);
      _Report.SetParameterValue("SqlVerbindung", verb.Get(JgMaschineLib.SqlVerbindung.SqlVerbindung.EnumVerbindungen.JgMaschine));

      if (e.Command == MyCommands.ReportAnzeigen)
        _Report.Show();
      else if (e.Command == Commands.MyCommands.ReportDrucken)
        _Report.Print();
      else if (e.Command == Commands.MyCommands.ReportBearbeiten)
        _Report.Design();      
    }

    private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _VsAuswertung.Source = _Db.tabAuswertungSet.OrderBy(o => o.ReportName).ToList();
    }
  }
}
