using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarter.Core.Utils;

public static class CollectionUtils
{
    public static T FirstOrThrow<T>(this IEnumerable<T> enumerable, Exception ex)
    {
        var first = enumerable.FirstOrDefault();
        return first ?? throw ex;
    }
}
