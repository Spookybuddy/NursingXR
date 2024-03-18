namespace GIGXR.Platform.CommonAssetTypes.Container
{
    /// <summary>
    /// Interface for assets which must be able to be initialized from configuration data
    /// by a container (currenly only container is InstanceContainerAssetTypeComponent)
    /// </summary>
    /// <typeparam name="TInitializationArgs"></typeparam>
    public interface IInstantiable<TInitializationArgs>
    {
        public void InitializeInstance(TInitializationArgs args);
    }
}
