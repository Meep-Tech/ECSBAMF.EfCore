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
  public class ComponentsToJsonCollectionValueConverter : ValueConverter<IReadOnlyDictionary<string, IComponent>, string> {

    public ComponentsToJsonCollectionValueConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static Expression<Func<string, IReadOnlyDictionary<string, IComponent>>> convertFromProviderExpression = x => FromJsonString(x);
    private static Expression<Func<IReadOnlyDictionary<string, IComponent>, string>> convertToProviderExpression = x => ToJsonString(x);

    static IReadOnlyDictionary<string, IComponent> FromJsonString(string componentsJson)
      => JArray.Parse(componentsJson).Select(token =>
        IComponent.FromJson(token as JObject)
      ).ToDictionary(
        component => component.Key,
        component => component
      );

    static string ToJsonString(IReadOnlyDictionary<string, IComponent> components)
      => JArray.FromObject(components.Select(componentData => componentData.Value.ToJson())).ToString();
  }
}
