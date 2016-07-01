using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using dn = System.ComponentModel.DataAnnotations;
using wc = System.Windows.Controls;
using System.ComponentModel;

namespace JgMaschineSetup.ValidationRules
{
  public class KontrFeldNichtLeer : wc.ValidationRule
  {
    List<dn.ValidationResult> _Fehler = new List<dn.ValidationResult>();

    public override wc.ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
    {
      var mBind = (BindingExpression)owner;
      object ds = mBind.DataItem;
      var propName = mBind.ParentBinding.Path.Path;

      TypeDescriptor.AddProviderTransparent(new dn.AssociatedMetadataTypeTypeDescriptionProvider(ds.GetType()), ds.GetType());

      _Fehler = new List<dn.ValidationResult>();
      var validationContext = new dn.ValidationContext(ds) { MemberName = propName };
      var bo = dn.Validator.TryValidateProperty(value, validationContext, _Fehler);

      return base.Validate(value, cultureInfo, owner);
    }

    public override wc.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    {
      if (_Fehler?.Count > 0)
        return new wc.ValidationResult(false, _Fehler[0].ErrorMessage);
      return wc.ValidationResult.ValidResult;
    }
  }
}
