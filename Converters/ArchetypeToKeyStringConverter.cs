using Meep.Tech.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Used to convert an Archetype to a general string for storage
  /// </summary>
  public class ArchetypeToKeyStringConverter<TFactory> : ValueConverter<TFactory, string> where TFactory : IFactory {
    public ArchetypeToKeyStringConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static Expression<Func<string, TFactory>>
        convertFromProviderExpression = x => ToArchetype(x);
    private static Expression<Func<TFactory, string>>
        convertToProviderExpression = x => ToString(x);

    static TFactory ToArchetype(string key) {
      return (TFactory)(IFactory)(key.Split("@") is string[] parts
        ? parts.Length == 1
          ? Archetypes.Id[key].Archetype
          : parts.Length == 2
            ? (Universe.s.TryToGet(parts[1]) ?? Archetypes.DefaultUniverse).Archetypes.Id[parts[0]].Archetype
            : throw new ArgumentException("ArchetypeKey")
        : throw new ArgumentNullException("ArchetypeKey"));
    }

    static string ToString(TFactory archetype)
      => archetype.Id.Key + (!string.IsNullOrEmpty(archetype.Id.Universe.Key)
        ? "@" + archetype.Id.Universe.Key
        : "");
  }

  /// <summary>
  /// Used to convert an Archetype to a general string for storage
  /// </summary>
  public class ArchetypeToKeyStringConverter : ValueConverter<IFactory, string> {
    public ArchetypeToKeyStringConverter() :
      base(convertToProviderExpression, convertFromProviderExpression) {
    }

    private static Expression<Func<string, IFactory>>
        convertFromProviderExpression = x => ToArchetype(x);
    private static Expression<Func<IFactory, string>>
        convertToProviderExpression = x => ToString(x);

    static IFactory ToArchetype(string key) {
      return key.Split("@") is string[] parts
        ? parts.Length == 1
          ? Archetypes.Id[key].Archetype
          : parts.Length == 2
            ? (Universe.s.TryToGet(parts[1]) ?? Archetypes.DefaultUniverse).Archetypes.Id[parts[2]].Archetype
            : throw new ArgumentException("ArchetypeKey")
        : throw new ArgumentNullException("ArchetypeKey");
    }

    static string ToString(IFactory archetype)
      => archetype.Id.Key + (!string.IsNullOrEmpty(archetype.Id.Universe.Key)
        ? "@" + archetype.Id.Universe.Key
        : "");
  }
}
