using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Quarter.Core.Utils;

/// <summary>
/// Validates an object with DataAnnotations recursively (the default ObjectValidator is not recursive)
/// </summary>
public static class RecursiveObjectValidator
{
    public static bool IsValid<T>(T prospect, out IList<ValidationResult> errors)
    {
        var processedObjects = new HashSet<object>();
        errors = new List<ValidationResult>();
        var contextItems = new Dictionary<object, object?>();
        Validate(prospect, errors, contextItems, processedObjects);
        return !errors.Any();
    }

    private static void Validate<T>(T prospect, ICollection<ValidationResult> totalResults,
        IDictionary<object, object?> contextItems, ISet<object> processedObjects)
    {
        if (prospect is null) return;
        if (processedObjects.Contains(prospect)) return;

        processedObjects.Add(prospect);

        var validationContext = new ValidationContext(prospect!, null, contextItems);
        _ = Validator.TryValidateObject(prospect!, validationContext, totalResults, true);

        foreach (var propertyInfo in ReadableProperties())
        {
            if (!(propertyInfo.PropertyType == typeof(string)) && !propertyInfo.PropertyType.IsValueType)
            {
                var partialResult = new List<ValidationResult>();
                var propertyValue = propertyInfo.GetValue(prospect);

                // TODO: Include the parent name when nested to get more precise errors. E.g. "Root.Child.Foo field is required."
                switch (propertyValue)
                {
                    // Support enumerable properties
                    case IEnumerable enumerable:
                        foreach (var item in enumerable)
                        {
                            if (item == null) continue;
                            Validate(item, partialResult, contextItems, processedObjects);
                            CopyResults(partialResult, totalResults, propertyInfo);
                        }
                        break;

                    // Support nested objects
                    case { }:
                        Validate(propertyValue, partialResult, contextItems, processedObjects);
                        CopyResults(partialResult, totalResults, propertyInfo);
                        break;
                }
            }
        }

        IEnumerable<PropertyInfo> ReadableProperties()
            => prospect.GetType()
                .GetProperties()
                .Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0);
    }

    private static void CopyResults(IEnumerable<ValidationResult> source, ICollection<ValidationResult> destination, MemberInfo propertyInfo)
    {
        foreach (var validationResult in source)
        {
            var newMemberNames = validationResult.MemberNames.Select(x => propertyInfo.Name + "." + x);
            var newValidationResult = new ValidationResult(validationResult.ErrorMessage, newMemberNames);
            destination.Add(newValidationResult);
        }
    }
}
