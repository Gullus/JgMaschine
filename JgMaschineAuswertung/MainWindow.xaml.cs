using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using JgMaschineAuswertung.Commands;
using JgMaschineData;
using JgMaschineLib;

namespace JgMaschineAuswertung
{
  public partial class MainWindow : RibbonWindow
  {
    private FastReport.Report _Report;
    private FastReport.EnvironmentSettings _ReportSettings = new FastReport.EnvironmentSettings();

    private string _FileSqlVerbindung = "JgMaschineSqlVerbindung.Xml";
    private JgEntityList<tabAuswertung> _Auswertungen;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _Auswertungen = new JgEntityList<tabAuswertung>()
      {
        ViewSource = (CollectionViewSource)FindResource("vsAuswertung"),
        Tabellen = new DataGrid[] { dgAuswertung },
        OnDatenLaden = (d, p) =>
        {
          return d.tabAuswertungSet
            .Where(w => w.FilterAuswertung == EnumFilterAuswertung.Allgemein)
            .OrderBy(o => o.ReportName).ToList();
        }
      };
      _Auswertungen.DatenLaden();

      _Report = new FastReport.Report();
      _Report.FileName = "Datenbank";
      _ReportSettings.CustomSaveReport += (obj, repEvent) =>
      {
        MemoryStream memStr = new MemoryStream();
        try
        {
          var ausw = _Auswertungen.Current;

          repEvent.Report.Save(memStr);
          ausw.Report = memStr.ToArray();
          ausw.GeaendertDatum = DateTime.Now;
          ausw.GeaendertName = Helper.Benutzer;
          _Auswertungen.DsSave(ausw);
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
          string username = Helper.Benutzer;
          form.Auswertung.FilterAuswertung = JgMaschineData.EnumFilterAuswertung.Allgemein;
          form.Auswertung.ErstelltDatum = DateTime.Now;
          form.Auswertung.ErstelltName = username;

          form.Auswertung.GeaendertName = form.Auswertung.ErstelltName;
          form.Auswertung.GeaendertDatum = form.Auswertung.ErstelltDatum;

          _Auswertungen.Add(form.Auswertung);

          _Report.Clear();

          JgMaschineLib.SqlVerbindung.SqlVerbindung verb = new JgMaschineLib.SqlVerbindung.SqlVerbindung(_FileSqlVerbindung);
          _Report.SetParameterValue("SqlVerbindung", verb);
          _Report.Design();
        }
      }));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportAnzeigen, ExceReportAnzeigenDruck, CanExecReportVorhandenAndNull));
      CommandBindings.Add(new CommandBinding(MyCommands.ReportDrucken, ExceReportAnzeigenDruck, CanExecReportVorhandenAndNull));
      CommandBindings.Add(new CommandBinding(MyCommands.ReportBearbeiten, ExceReportAnzeigenDruck, CanExecReportVorhanden));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportAusDateiLaden, (sen, erg) =>
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
          _Auswertungen.Current.Report = mem.ToArray();
          _Auswertungen.DsSave();
          _Auswertungen.Refresh();
        }

      }, CanExecReportVorhanden));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportOptionen, (sen, erg) =>
      {
        Fenster.FormAuswertungBearbeiten form = new Fenster.FormAuswertungBearbeiten(_Auswertungen.Current);
        if (form.ShowDialog() ?? false)
        {
          string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
          form.Auswertung.GeaendertName = username;
          form.Auswertung.GeaendertDatum = form.Auswertung.ErstelltDatum;
          _Auswertungen.DsSave();
        }
        else
          _Auswertungen.Reload();
      }, CanExecReportVorhanden));

      CommandBindings.Add(new CommandBinding(MyCommands.ReportInDateiSpeichern, (sen, erg) =>
      {
        Microsoft.Win32.SaveFileDialog dia = new Microsoft.Win32.SaveFileDialog();
        dia.Filter = "Fastreport (*.frx)|*.frx|Alle Dateien (*.*)|*.*";
        dia.FilterIndex = 1;
        if (dia.ShowDialog() ?? false)
        {
          MemoryStream mem;
          mem = new MemoryStream(_Auswertungen.Current.Report);
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
      e.CanExecute = (_Auswertungen.Current != null) && (_Auswertungen.Current.Report != null);
    }

    private void CanExecReportVorhanden(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = _Auswertungen.Current != null;
    }

    private void ExceReportAnzeigenDruck(object sender, ExecutedRoutedEventArgs e)
    {
      _Report.Clear();
      var dsRep = _Auswertungen.Current;

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

    private void Click_TabelleAktuallisieren(object sender, RoutedEventArgs e)
    {
      _Auswertungen.DatenNeuLaden();
    }
  }
}
