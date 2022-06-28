using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert a collection of components to and from a json array
  /// </summary>
  public class ComponentsToJsonCollectionValueConverter<TComponent> : ValueConverter<IReadOnlyDictionary<string, TComponent>, string> where TComponent : Data.IComponent {

    public ComponentsToJsonCollectionValueConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static readonly Expression<Func<string, IReadOnlyDictionary<string, TComponent>>> convertFromProviderExpression = x => FromJsonString(x);
    private static readonly Expression<Func<IReadOnlyDictionary<string, TComponent>, string>> convertToProviderExpression = x => ToJsonString(x);

    static IReadOnlyDictionary<string, TComponent> FromJsonString(string componentsJson)
      => JArray.Parse(componentsJson).Select(token =>
        (TComponent)IComponent.FromJson(token as JObject)
      ).ToDictionary(
        component => component.Key,
        component => component
      );

    static string ToJsonString(IReadOnlyDictionary<string, TComponent> components)
      => JArray.FromObject(components.Select(componentData => componentData.Value.ToJson())).ToString();
  }

  /// <summary>
  /// Used to convert a collection of generic components to and from a json array
  /// </summary>
  public class ComponentsToJsonCollectionValueConverter : ComponentsToJsonCollectionValueConverter<IComponent> {
    public ComponentsToJsonCollectionValueConverter() :
      base() {
    }
  }
}
