using Meep.Tech.Data.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Can be used to set a custom converter for a field on an Ecsbam Model.
  /// You can use the functions in ModelBuilderExtensions to set this up on your DbContext
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class UseCustomEfCoreConverterAttribute : Attribute {

    internal static Dictionary<Type, ValueConverter> _cachedCustomConverters
      = new();

    /// <summary>
    /// The custom type converter
    /// </summary>
    public Type CustomConverterType {
      get;
    }

    /// <summary>
    /// The custom converter to use.
    /// </summary>
    public virtual ValueConverter CustomConverter
      => _cachedCustomConverters[CustomConverterType];

    public UseCustomEfCoreConverterAttribute(Type CustomConverterType) {
      if (!CustomConverterType.IsAssignableToGeneric(typeof(ValueConverter<,>))) {
        throw new ArgumentException($"Type of {CustomConverterType} does not extend Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<,>.");
      }

      this.CustomConverterType = CustomConverterType;

      // Make sure there's a parameterless Ctor for the ValueConverter
      _generateDefaultConverter(CustomConverterType);
    }

    void _generateDefaultConverter(Type CustomConverterType) {
      try {
        if (!_cachedCustomConverters.ContainsKey(CustomConverterType)) {
          _cachedCustomConverters[this.CustomConverterType]
            = ((Func<ValueConverter>)(() => Activator.CreateInstance(this.CustomConverterType) as ValueConverter))();
        }
      }
      catch (Exception ex) {
        throw new ArgumentException($"Could not invoke Activator.CreateInstance for parameterless ctor for ValueConverter<,> of type {this.CustomConverterType}", ex);
      }
    }
  }
}
