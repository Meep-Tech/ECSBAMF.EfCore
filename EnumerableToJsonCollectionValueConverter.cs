using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert a collection of something to and from a json array
  /// </summary>
  public abstract class EnumerableToJsonCollectionValueConverter<T> : ValueConverter<IEnumerable<T>, string> {

    public EnumerableToJsonCollectionValueConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static Expression<Func<string, IEnumerable<T>>> convertFromProviderExpression = x => FromJsonString(x);
    private static Expression<Func<IEnumerable<T>, string>> convertToProviderExpression = x => ToJsonString(x);

    static IEnumerable<T> FromJsonString(string itemsJson)
      => JArray.Parse(itemsJson).Select(token => token.Value<T>());

    static string ToJsonString(IEnumerable<T> items)
      => JArray.FromObject(items).ToString();
  }
}
