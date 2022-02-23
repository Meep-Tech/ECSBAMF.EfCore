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
  public class UseCustomConverterOnCollection : UseCustomEfCoreConverterAttribute {

    public Type SingleItemConverterType {
      get;
    }

    public UseCustomConverterOnCollection(Type ItemConverterType, Type GenericItemTypeOverride = null) : base(
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
        return _customConverter = _createCollectionConverter(sinlgeItemConverter, CustomConverterType);
      }
    } ValueConverter _customConverter;

    internal static ValueConverter _createCollectionConverter(ValueConverter sinlgeItemConverter, Type customConverterType) {
      return  (ValueConverter)Activator.CreateInstance(
         customConverterType,
         new Func<object, object>(items => {
           List<string> itemStrings = new();
           foreach (var item in items as IEnumerable) {
             itemStrings.Add(JsonConvert.SerializeObject(sinlgeItemConverter.ConvertToProvider(item)));
           }
           return JsonConvert.SerializeObject(itemStrings);
         }),
         new Func<object, object>(itemsJson => {
           List<object> items = new();
           foreach (string itemString in JArray.Parse(itemsJson as string).Select(t => t.Value<string>())) {
             items.Add(sinlgeItemConverter.ConvertFromProvider(JsonConvert.DeserializeObject(itemString)));
           }
           return items;
         })
       );
    }

  }

  internal static class LocalConverterTypeExtensions {
    public static Type MakeCollectionConverterForGeneric(this Type type, Type itemType) {
      IEnumerable<Type> baseConverterTypes
        = type.GetInheritedGenericTypes(typeof(ValueConverter<,>));
      Type convertFromType = baseConverterTypes.First();
      Type convertToType = itemType ?? baseConverterTypes.Last();

      Type convertFromCollectionType = typeof(IEnumerable<>).MakeGenericType(convertFromType);
      Type convertToCollectionType = typeof(IEnumerable<>).MakeGenericType(convertToType);

      return typeof(ValueConverter<,>).MakeGenericType(
          convertFromCollectionType,
          convertToCollectionType
      );
    }
  }
}
