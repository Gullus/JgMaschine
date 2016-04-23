using System.Windows.Controls;

namespace JgMaschineSetup.ValidationRules
{
  public class KontrFeldNichtLeer : ValidationRule
  {
    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
      if ((value == null) || (value.ToString().Length < 1))
        return new ValidationResult(false, "Feld darf nicht leer sein.");
      return ValidationResult.ValidResult;
    }
  }
}
