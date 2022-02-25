using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert values. A non-abstract simple version.
  /// </summary>
  public class SimpleValueConverter<I, O> : ValueConverter<I, O> {

    public SimpleValueConverter(
      Expression<Func<I, O>> convertToProviderExpression,
      Expression<Func<O, I>> convertFromProviderExpression
    ) : base(convertToProviderExpression, convertFromProviderExpression) {}
  }
}
