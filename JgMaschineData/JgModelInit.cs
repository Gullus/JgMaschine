using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace JgMaschineData
{
  public partial class JgModelContainer
  {
    public static string _Bediener = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

    private static string DbFehlerText(DbEntityValidationException ex)
    {
      // Retrieve the error messages as a list of strings.
      var errorMessages = ex.EntityValidationErrors
              .SelectMany(x => x.ValidationErrors)
              .Select(x => x.ErrorMessage);

      // Join the list to a single string.
      var fullErrorMessage = string.Join("; ", errorMessages);

      System.Windows.MessageBox.Show(fullErrorMessage);

      // Combine the original exception message with the new one.
      return string.Concat(ex.Message, " Fehler: ", fullErrorMessage);
    }

    public override int SaveChanges()
    {
      try
      {
        ChangeTracker.DetectChanges();

        var modified = ChangeTracker
          .Entries()
          .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        DatenAbgleich dag = null;
        var datum = DateTime.Now;

        foreach (var item in modified)
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
        // Throw a new DbEntityValidationException with the improved exception message.
        throw new DbEntityValidationException(DbFehlerText(ex), ex.EntityValidationErrors);
      }
    }

    public override Task<int> SaveChangesAsync()
    {
      try
      {
        return base.SaveChangesAsync();
      }
      catch (DbEntityValidationException ex)
      {
        // Throw a new DbEntityValidationException with the improved exception message.
        throw new DbEntityValidationException(DbFehlerText(ex), ex.EntityValidationErrors);
      }
    }
  }
}
