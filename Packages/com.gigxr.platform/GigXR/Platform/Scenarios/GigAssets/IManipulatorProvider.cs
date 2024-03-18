using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Interface for class which provides a reference to an <c>ObjectManipulator</c>
    /// </summary>
    public interface IManipulatorProvider
    {
        public ObjectManipulator ObjectManipulator { get; }
        public BoundsControl BoundsControl { get; }
    }
}
