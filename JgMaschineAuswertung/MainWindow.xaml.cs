using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using JgMaschineAuswertung.Commands;
using JgMaschineLib;

namespace JgMaschineAuswertung
{
  public partial class MainWindow : RibbonWindow
  {
    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private string _FileSqlVerbindung = "JgMaschineSqlVerbindung.Xml";
    private JgList<JgMaschineData.tabAuswertung> _ListeAuswertungen;

    public MainWindow()
    {
      InitializeComponent();
    }

    private async void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _ListeAuswertungen = new JgList<JgMaschineData.tabAuswertung>((CollectionViewSource)FindResource("vsAuswertung"));
      _ListeAuswertungen.MyQuery = _ListeAuswertungen.Db.tabAuswertungSet.Where(w => w.FilterAuswertung == JgMaschineData.EnumFilterAuswertung.Allgemein).OrderBy(o => o.ReportName);
      _ListeAuswertungen.ListeTabellen = new System.Windows.Controls.DataGrid[] { dgAuswertung };
      await _ListeAuswertungen.DatenGenerierenAsync();

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          var ausw = (JgMaschineData.tabAuswertung)_ListeAuswertungen.AktDatensatz;

          repEvent.Report.Save(memStr);
          ausw.Report = memStr.ToArray();
          ausw.GeaendertDatum = DateTime.Now;
          ausw.GeaendertName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          _ListeAuswertungen.AktSichern(JgMaschineData.EnumStatusDatenabgleich.Geaendert);
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
        Fenster.FormAuswertungBearbeiten form = new Fenster.FormAuswertungBearbeiten(null);
        if (form.ShowDialog() ?? false)
        {
          string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          form.Auswertung.Id = Guid.NewGuid();
          form.Auswertung.FilterAuswertung = JgMaschineData.EnumFilterAuswertung.Allgemein;
          form.Auswertung.ErstelltDatum = DateTime.Now;
          form.Auswertung.ErstelltName = username;

          form.Auswertung.GeaendertName = form.Auswertung.ErstelltName;
          form.Auswertung.GeaendertDatum = form.Auswertung.ErstelltDatum;

          _ListeAuswertungen.Add(form.Auswertung);

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
          _ListeAuswertungen.AktDatensatz.Report = mem.ToArray();
          _ListeAuswertungen.AktSichern(JgMaschineData.EnumStatusDatenabgleich.Geaendert);
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
          mem = new MemoryStream(_ListeAuswertungen.AktDatensatz.Report);
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
      e.CanExecute = (_ListeAuswertungen.AktDatensatz != null) && (_ListeAuswertungen.AktDatensatz.Report != null);
    }

    private void CanExecReportVorhanden(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = _ListeAuswertungen.AktDatensatz != null;
    }

    private void ExceReportAnzeigenDruck(object sender, ExecutedRoutedEventArgs e)
    {
      _Report.Clear();
      var dsRep = _ListeAuswertungen.AktDatensatz;

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
  }
}
