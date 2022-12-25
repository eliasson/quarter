using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quarter.Core.Utils;

public static class ObjectValidator
{
    public static bool IsValid(object o, out IList<ValidationResult> result)
    {
        result = new List<ValidationResult>();
        var contextItems = new Dictionary<object, object?>();
        var validationContext = new ValidationContext(o!, null, contextItems);
        return Validator.TryValidateObject(o!, validationContext, result, true);
    }
}