using Meep.Tech.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert an Archetype to a general string for storage
  /// </summary>
  public class EnumerationToKeyStringConverter<TEnumeration> : ValueConverter<TEnumeration, string> where TEnumeration : Enumeration<TEnumeration> {
    public EnumerationToKeyStringConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static Expression<Func<string, TEnumeration>>
        convertFromProviderExpression = x => ToArchetype(x);
    private static Expression<Func<TEnumeration, string>>
        convertToProviderExpression = x => ToString(x);

    static TEnumeration ToArchetype(string externalKey) {
      return externalKey.Split("@") is string[] parts
        ? parts.Length == 1
          ? Archetypes.DefaultUniverse.Enumerations.Get<TEnumeration>(parts[0])
          : parts.Length == 2
            ? (Universe.s.TryToGet(parts[1]) ?? Archetypes.DefaultUniverse).Enumerations.Get<TEnumeration>(parts[0])
            : throw new ArgumentException("EnumerationKey")
        : throw new ArgumentNullException("EnumerationKey");
    }

    static string ToString(TEnumeration enumeration)
      => enumeration.ExternalId.ToString() + (!string.IsNullOrEmpty(enumeration.Universe.Key)
        ? "@" + enumeration.Universe.Key
        : "");
  }
}
