using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JgMaschineData
{
  public partial class JgModelContainer
  {
    public static string _Bediener = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

    public override int SaveChanges()
    {
      IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> listeSpeichern = null; 

      try
      {
        ChangeTracker.DetectChanges();

        listeSpeichern = ChangeTracker
          .Entries()
          .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        DatenAbgleich dag = null;
        var datum = DateTime.Now;

        foreach (var item in listeSpeichern)
        {
          dag = (DatenAbgleich)item.ComplexProperty("DatenAbgleich").CurrentValue;
          if (dag != null)
          {
            dag.Bearbeiter = _Bediener;
            dag.Datum = datum;

            if (item.State == EntityState.Added)
              dag.Status = EnumStatusDatenabgleich.Neu;
            else
              dag.Status = EnumStatusDatenabgleich.Geaendert;
          }
        }

        return base.SaveChanges();
      }
      catch (DbEntityValidationException ex)
      {
        var sb = new StringBuilder();
        foreach (var f in ex.EntityValidationErrors)
        {
          sb.AppendLine($"  {f.Entry.GetType()}");
          foreach (var fi in f.ValidationErrors)
            sb.AppendLine($"    {fi.PropertyName} - {fi.ErrorMessage}");
        }

        throw new Exception($"Valitationsfehler beim speichern der Daten.\nGrund: {FehlerText(ex)}\nDaten:\n{sb.ToString()}");
      }
      catch (Exception ex)
      {
        var sb = new StringBuilder();
        if (listeSpeichern != null)
        {
          foreach (System.Data.Entity.Infrastructure.DbEntityEntry ds in listeSpeichern)
            sb.AppendLine($"  {ds.Entity.GetType()}");
        }

        throw new Exception($"Fehler beim speichern der Daten.\nGrund: {FehlerText(ex)}\nDaten:\n{sb.ToString()}");
      }
    }

    public override Task<int> SaveChangesAsync()
    {
      IEnumerable<System.Data.Entity.Infrastructure.DbEntityEntry> listeSpeichern = null;

      try
      {
        ChangeTracker.DetectChanges();

        listeSpeichern = ChangeTracker
          .Entries()
          .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        DatenAbgleich dag = null;
        var datum = DateTime.Now;

        foreach (var item in listeSpeichern)
        {
          dag = (DatenAbgleich)item.ComplexProperty("DatenAbgleich").CurrentValue;
          if (dag != null)
          {
            dag.Bearbeiter = _Bediener;
            dag.Datum = datum;

            if (item.State == EntityState.Added)
              dag.Status = EnumStatusDatenabgleich.Neu;
            else
              dag.Status = EnumStatusDatenabgleich.Geaendert;
          }
        }

        return base.SaveChangesAsync();
      }
      catch (DbEntityValidationException ex)
      {
        var sb = new StringBuilder();
        foreach (var f in ex.EntityValidationErrors)
        {
          sb.AppendLine($"  {f.Entry.GetType()}");
          foreach (var fi in f.ValidationErrors)
            sb.AppendLine($"    {fi.PropertyName} - {fi.ErrorMessage}");
        }

        throw new Exception($"Valitationsfehler beim speichern der Daten.\nGrund: {FehlerText(ex)}\nDaten:\n{sb.ToString()}");
      }
      catch (Exception ex)
      {
        var sb = new StringBuilder();
        if (listeSpeichern != null)
        {
          foreach (System.Data.Entity.Infrastructure.DbEntityEntry ds in listeSpeichern)
            sb.AppendLine($"  {ds.Entity.GetType()}");
        }

        throw new Exception($"Fehler beim speichern der Daten.\nGrund: {FehlerText(ex)}\nDaten:\n{sb.ToString()}");
      }
    }

    public static string FehlerText(Exception Fehler)
    {
      var ex = Fehler;
      var sb = new StringBuilder();
      var einzug = "  ";
      while (true)
      {
        sb.AppendLine(einzug + Fehler.Message);
        if (ex.InnerException == null)
          return sb.ToString();

        ex = ex.InnerException;
        einzug += "  ";
      }
    }
  }
}
