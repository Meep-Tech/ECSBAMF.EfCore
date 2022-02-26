namespace Meep.Tech.Data.EFCore {
  /// <summary>
  /// Used to convert a collection of model components to and from a json array
  /// </summary>
  public class ModelComponentsToJsonCollectionValueConverter : ComponentsToJsonCollectionValueConverter<Data.IModel.IComponent> {
    public ModelComponentsToJsonCollectionValueConverter() :
      base() {
    }
  }
}
