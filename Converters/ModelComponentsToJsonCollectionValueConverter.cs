using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert a collection of model components to and from a json array
  /// </summary>
  public class ModelComponentsToJsonCollectionValueConverter : ValueConverter<IReadableComponentStorage.ReadOnlyModelComponentCollection, string> {

    static ComponentsToJsonCollectionValueConverter<IModel.IComponent> _internalConverter
      = new();

    public ModelComponentsToJsonCollectionValueConverter() : base(
      c => (string)_internalConverter.ConvertToProvider(
        c as IReadOnlyDictionary<string, IModel.IComponent>
      ), 
      s => new(
        null,
        (IReadOnlyDictionary<string, IModel.IComponent>)
          _internalConverter.ConvertFromProvider(s))
    ) {}
  }
}
