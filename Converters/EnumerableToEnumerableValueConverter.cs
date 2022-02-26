using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert a collection of something to a collection of something else
  /// </summary>
  public class EnumerableToEnumerableValueConverter<I, O> : SimpleValueConverter<IEnumerable<I>, IEnumerable<O>> {

    public EnumerableToEnumerableValueConverter(
      Func<object, object> convertToProviderExpression,
      Func<object, object> convertFromProviderExpression
    ) : base((i) => (IEnumerable<O>)convertToProviderExpression(i), (o) => (IEnumerable<I>)convertFromProviderExpression(o)) {}
  }
}
