using System;

namespace Meep.Tech.Data.EFCore {

  /// <summary>
  /// Can be used to set a custom converter for a collection based field on an Ecsbam Model.
  /// You can use the functions in ModelBuilderExtensions to set this up on your DbContext
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class UseCustomEfCoreCollectionConverterAttribute : UseCustomEfCoreConverterAttribute {

    /// <summary>
    /// The custom type converter
    /// </summary>
    public Type CollectionItemType {
      get;
    }

    public UseCustomEfCoreCollectionConverterAttribute(Type CustomCollectionConverterType, Type CollectionItemType) : base(CustomCollectionConverterType.MakeGenericType(CollectionItemType)) {
      this.CollectionItemType = CollectionItemType;
    }
  }
}
