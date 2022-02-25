using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Data.EFCore
{
  public static class ConfigurationExtensions {

    static readonly ValueConverter _componentConverter 
      = new ComponentsToJsonCollectionValueConverter();

    #region Initialization

    /// <summary>
    /// Initialize the ef core settings for the universe object before it's loaded.
    /// </summary>
    public static ModelEfCoreSettings InitializeXbamEfCoreSettings(this Universe universe, ModelEfCoreSettings modelSerializerSettings = null) {
      universe.SetExtraContext(modelSerializerSettings ?? new ModelEfCoreSettings(universe));
      return universe.GetExtraContext<ModelEfCoreSettings>();
    }

    /// <summary>
    /// Used to set up Ecsbam settings needed for general models in your custom DbContext class.
    /// </summary>
    public static ModelBuilder SetUpEcsbamModels(this ModelBuilder modelBuilder, Universe universe) {
      modelBuilder.Ignore<Universe>();
      
      modelBuilder.AddModelTypes(universe);

      // enum type custom converters by default:
      foreach(System.Type enumType in universe.Enumerations.ByType.Keys) {
        Type singleEnumValueConverterType = typeof(EnumerationToKeyStringConverter<>).MakeGenericType(enumType);
        ValueConverter singleEnumValueConverter = (ValueConverter)Activator.CreateInstance(singleEnumValueConverterType);

        // single item to string
        modelBuilder.UseValueConverterForType(enumType, singleEnumValueConverter);
        // multiple items to json array
        modelBuilder.UseValueConverterForType(
          typeof(IEnumerable<>).MakeGenericType(enumType),
          UseCustomConverterOnEachItemToAJsonArrayAttribute._createConverter(
            UseCustomConverterOnEachCollectionItemAttribute._createCollectionConverter(
              singleEnumValueConverter,
              singleEnumValueConverterType.MakeCollectionConverterForGeneric()
            )
          )
        );
      }

      modelBuilder.UseCustomValueConverters();

      return modelBuilder;
    }

    /// <summary>
    /// Adds all model types in ModelSerializerSettings.TypesToMapToDbContext to the desired DbContext.
    /// </summary>
    public static ModelBuilder AddModelTypes(this ModelBuilder modelBuilder, Universe universe) {
      foreach ((System.Type modelType, Action<EntityTypeBuilder> config) in universe.GetExtraContext<ModelEfCoreSettings>().TypesToMapToDbContext) {
        var builder = modelBuilder.Entity(modelType);
        // Archetype based builder types use Table Per Hirearchy pattern, and use the Archetype field as a discriminator
        if (modelType.IsAssignableToGeneric(typeof(Model<,>))) {
          builder.HasDiscriminator(nameof(Archetype), typeof(Archetype));
        }

        // unique have their id field as key
        if (typeof(IUnique).IsAssignableFrom(modelType)) {
          modelBuilder.Entity(modelType.FullName).Property(typeof(string), "Id")
            .IsRequired().HasAnnotation("Key", 0);
        } // if a user wants to set a custom key, they need to apply this interface. 
        else if (/*!typeof(IUniqueWithCustomKeyColumn).IsAssignableFrom(modelType)
          // only apply HasNoKey to base model types
          && */modelType.GetModelBaseType().Equals(modelType)
        ) {
          builder.HasNoKey();
        }

        // check if it has a custom config
        config?.Invoke(builder);
      }

      return modelBuilder;
    }

    /// <summary>
    /// Can be used in DBContext to set up the custom value converters
    /// </summary>
    public static ModelBuilder UseCustomValueConverters(this ModelBuilder modelBuilder) {
      foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
        // check if it has components and add that field:
        if (typeof(IReadableComponentStorage).IsAssignableFrom(entityType.ClrType)) {
          // TODO: make it so the specified field can be changed with ModelComponentsProperty if Components isn't found.
          modelBuilder.Entity(entityType.ClrType.FullName)
            .Property(typeof(IReadOnlyDictionary<string, IComponent>), "Components")
            .IsRequired()
            .HasConversion(_componentConverter);
        }

        // note that entityType.GetProperties() will throw an exception, so we have to use reflection 
        foreach (var property in entityType.ClrType.GetProperties()) {
          ValueConverter customConverter = null;
          UseCustomEfCoreConverterAttribute useCustomConverterAttribute
            = property.GetCustomAttributes(typeof(UseCustomEfCoreConverterAttribute), true).FirstOrDefault()
              as UseCustomEfCoreConverterAttribute;

          // custom converter field:
          if (useCustomConverterAttribute is not null) {
            // if we have a cached converter of this type, use that.
            customConverter = useCustomConverterAttribute.CustomConverter;
          } // archetype field:
          else if (property.TryToGetAttribute<ArchetypePropertyAttribute>(out _)) {
            //Type modelBaseType = entityType.ClrType.GetModelBaseType();
            Type converterType = typeof(ArchetypeToKeyStringConverter<>)
              .MakeGenericType(property.PropertyType);
            customConverter = (ValueConverter)Activator.CreateInstance(converterType);
          } /* // components field:
          else if (property.TryToGetAttribute<ModelComponentsProperty>(out _) 
            || property.PropertyType == typeof(IReadOnlyDictionary<string, IComponent>)
          ) {
            customConverter = _componentConverter;
          }*/

          if (customConverter is not null) {
            modelBuilder.Entity(entityType.Name).Property(property.Name)
                .HasConversion(customConverter);
          }
        }
      }


      return modelBuilder;
    }

    /// <summary>
    /// Use a custom value converter for all properties of a type
    /// </summary>
    public static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter) {
      return modelBuilder.UseValueConverterForType(typeof(T), converter);
    }

    /// <summary>
    /// Use a custom value converter for all properties of a type
    /// </summary>
    public static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter) {
      foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
        // note that entityType.GetProperties() will throw an exception, so we have to use reflection 
        var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);
        foreach (var property in properties) {
          modelBuilder.Entity(entityType.Name).Property(property.Name)
              .HasConversion(converter);
        }
      }

      return modelBuilder;
    }

    #endregion

    #region Modification

    /// <summary>
    /// Update the EFCore entity builder for this model type
    /// </summary>
    public static void ModifyEfCoreBuilderFor<TModel>(this Universe.ModelsData modelsData, Action<EntityTypeBuilder> action)
      where TModel : IModel<TModel> {
      if(modelsData.Universe.GetExtraContext<ModelEfCoreSettings>().TypesToMapToDbContext.TryGetValue(typeof(TModel), out var found) && found is not null) {
        modelsData.Universe.GetExtraContext<ModelEfCoreSettings>().TypesToMapToDbContext[typeof(TModel)] = action + found;
      }
      else {
        modelsData.Universe.GetExtraContext<ModelEfCoreSettings>().TypesToMapToDbContext[typeof(TModel)] = action;
      }
    }

    #endregion

    /// <summary>
    /// Helper to try to get an attribute by type.
    /// </summary>
    public static bool TryToGetAttribute<TAttribute>(this PropertyInfo property, out TAttribute attrubute, bool inherit = true)
      where TAttribute : Attribute
        => (attrubute = property.GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault()
          as TAttribute) is not null;
  }

}
