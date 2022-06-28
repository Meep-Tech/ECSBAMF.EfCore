using Meep.Tech.Collections.Generic;
using Meep.Tech.Data.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Serialize the objects as a json array using an intermediate valueconverter.
  /// </summary>
  public class UseCustomConverterOnEachItemToAJsonArrayAttribute : UseCustomConverterOnEachCollectionItemAttribute {

    public UseCustomConverterOnEachItemToAJsonArrayAttribute(Type ItemConverterType, Type GenericItemTypeOverride = null) 
      : base(ItemConverterType, GenericItemTypeOverride) {}

    public override ValueConverter CustomConverter {
      get {
        if (_customConverter is not null) {
          return _customConverter;
        }

        var baseCollectionConverter = base.CustomConverter;
        return _cachedCustomConverters[CustomConverterType] = _customConverter = _createConverter(baseCollectionConverter);
      }
    }

    internal static ValueConverter _createConverter(ValueConverter baseCollectionConverter) 
      => new SimpleValueConverter<IEnumerable, string>(
        collection => JsonConvert.SerializeObject(baseCollectionConverter.ConvertToProvider(collection)),
        json => (IEnumerable)baseCollectionConverter.ConvertFromProvider(JsonConvert.DeserializeObject(json, baseCollectionConverter.GetType().GetGenericArguments().First()))
      );

    ValueConverter _customConverter;
  }

  /// <summary>
  /// Serialize the objects as an array using an intermediate valueconverter.
  /// </summary>
  public class UseCustomConverterOnEachCollectionItemAttribute : UseCustomEfCoreConverterAttribute {

    public Type SingleItemConverterType {
      get;
    }

    public UseCustomConverterOnEachCollectionItemAttribute(Type ItemConverterType, Type GenericItemTypeOverride = null) : base(
      ItemConverterType.MakeCollectionConverterForGeneric(GenericItemTypeOverride)
    ) {
      SingleItemConverterType = GenericItemTypeOverride is null 
        ? ItemConverterType 
        : ItemConverterType.MakeGenericType(GenericItemTypeOverride);
    }

    public override ValueConverter CustomConverter {
      get {
        if (_customConverter is not null) {
          return _customConverter;
        }

        var sinlgeItemConverter = _cachedCustomConverters.TryGetValue(SingleItemConverterType, out var existingSingleConverter)
          ? existingSingleConverter
          : (_cachedCustomConverters[SingleItemConverterType] = (ValueConverter)Activator.CreateInstance(SingleItemConverterType));
        return _cachedCustomConverters[CustomConverterType] = _customConverter = _createCollectionConverter(sinlgeItemConverter, CustomConverterType);
      }
    } ValueConverter _customConverter;

    internal static ValueConverter _createCollectionConverter(ValueConverter sinlgeItemConverter, Type customConverterType) {
      System.Reflection.ConstructorInfo[] customConverterCtors = customConverterType.GetConstructors();
      System.Reflection.ConstructorInfo customConverterCtor = customConverterCtors.First();
      var to = new Func<object, object>((i) => (i as IEnumerable).Cast<object>().Select(ii => sinlgeItemConverter.ConvertToProvider(ii)));
      var from = new Func<object, object>((i) => (i as IEnumerable).Cast<object>().Select(ii => sinlgeItemConverter.ConvertFromProvider(ii)));
      return (ValueConverter)customConverterCtor.Invoke(new object[] {
        to,
        from
      });
    }

    static IEnumerable<O> _useSingleToConverterOnCollectionItems<I,O>(ValueConverter sinlgeItemConverter, IEnumerable<I> items) {
      foreach (var item in items) {
        yield return (O)sinlgeItemConverter.ConvertToProvider(item);
      }
    }

    static IEnumerable<O> _useSingleFromConverterOnCollectionItems<I, O>(ValueConverter sinlgeItemConverter, IEnumerable<I> items) {
      foreach (var item in items) {
        yield return (O)sinlgeItemConverter.ConvertFromProvider(item);
      }
    }
  }

  internal static class LocalConverterTypeExtensions {

    public static Type MakeCollectionConverterForGeneric(this Type type, Type itemType = null) {
      IEnumerable<Type> baseConverterTypes
        = type.GetFirstInheritedGenericTypeParameters(typeof(ValueConverter<,>));

      Type convertFromType = baseConverterTypes.First();
      Type convertToType = itemType ?? baseConverterTypes.Last();

      Type convertFromCollectionType = typeof(IEnumerable<>).MakeGenericType(convertFromType);
      Type convertToCollectionType = typeof(IEnumerable<>).MakeGenericType(convertToType);

      return typeof(EnumerableToEnumerableValueConverter<,>).MakeGenericType(
          convertFromCollectionType,
          convertToCollectionType
      );
    }
  }
}
