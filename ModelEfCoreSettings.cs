using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Serializer settings for ECSBAM Models, and settings for EFCore DBContext related to them.
  /// </summary>
  public partial class ModelEfCoreSettings : Universe.ExtraContext {
    internal Universe _universe;

    /// <summary>
    /// Whether or not ECSBAM should set up the models with a db context.
    /// This can be used to disable EFCore for testing.
    /// </summary>
    public bool TryToSetUpDbContext {
      get;
      set;
    } = true;

    /// <summary>
    /// If this is true, models must have the [Table] attribute to be set up by ecsbam using efcore by default.
    /// </summary>
    public bool ModelsMustOptInToEfCoreUsingAttribute {
      get;
      set;
    } = false;

    /// <summary>
    /// The types to map to the db context.
    /// You can provide a config function if you want, but don't have to (null is default).
    /// </summary>
    public Dictionary<System.Type, Action<EntityTypeBuilder>> TypesToMapToDbContext {
      get;
    } = new Dictionary<System.Type, Action<EntityTypeBuilder>>();

    /// <summary>
    /// The default entity framework db serializer context
    /// </summary>
    public Func<
      DbContextOptions<DefaultModelDbContext>, // general options obj
      Universe,
      DefaultModelDbContext // the returned options
    > GetDefaultDbContextForModelSerialization {
      get;
      set;
    } = (options, universe)
      => new DefaultModelDbContext(
        options,
        universe
      );

    /// <summary>
    /// The db context used by the serializer
    /// </summary>
    public DbContext DbContext {
      get;
      private set;
    }

    public ModelEfCoreSettings(Universe universe) {
      _universe = universe;
    }

    /// <summary>
    /// Add all model types to the dbcontext.
    /// </summary>
    protected override void OnModelTypeRegistered(Type modelType, IModel defaultModel) {
      if (TryToSetUpDbContext && !TypesToMapToDbContext.ContainsKey(modelType)) {
        if (ModelsMustOptInToEfCoreUsingAttribute) {
          System.ComponentModel.DataAnnotations.Schema.TableAttribute tableAttribute
            = modelType.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
          // if we need a table attribute, and it's null, just skip this last set.
          if (tableAttribute is null) {
            return;
          }
        }

        // attach as default (no config function)
        TypesToMapToDbContext[modelType] = null;
      }
    }

    protected override void OnLoaderFinalize() {
      if (TryToSetUpDbContext) {
        DbContext
         ??= GetDefaultDbContextForModelSerialization(
           new DbContextOptions<DefaultModelDbContext>(),
           _universe
         );
      }
    }
  }
}