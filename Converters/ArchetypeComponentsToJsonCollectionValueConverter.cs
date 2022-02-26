namespace Meep.Tech.Data.EFCore {
  /// <summary>
  /// Used to convert a collection of archetype components to and from a json array
  /// </summary>
  public class ArchetypeComponentsToJsonCollectionValueConverter : ComponentsToJsonCollectionValueConverter<Data.IModel.IComponent> {
    public ArchetypeComponentsToJsonCollectionValueConverter() :
      base() {
    }
  }
}
