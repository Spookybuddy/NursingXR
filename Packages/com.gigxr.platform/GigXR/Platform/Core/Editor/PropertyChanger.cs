namespace GIGXR.Platform.Core.Editor
{
    // using Castle.Core.Internal;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    
    /// <summary>
    /// This was something I whipped together for a quick change. Not suitable for any more updates, needs to be
    /// turned into a generic resource. 
    /// </summary>
    public class PropertyChanger
    {
        // [MenuItem("Parrot/Update Property Values In Scene")]
        public static void RunPropertyChanger()
        {
            var goRoots = SceneManager.GetActiveScene().GetRootGameObjects();
            // Debug.Log();

            var allGO = goRoots[3].GetComponentsInChildren<Transform>(true);
            
            // If the GO has an Image object, and the image's color is set to a certain color
            // And check its name as Quad
            foreach (var obj in allGO)
            {
                // Debug.Log("Checking.." + obj.name);
                
                if ((obj.name == "Quad") && (obj.GetComponent<Image>()))
                {
                    Image image = obj.GetComponent<Image>();

                    Color oldCyanColor = new Color(0.003921569f, 0.9058824f, 0.9294118f, 1f);
                    Color newTealColor = new Color(0f, 0.5294118f, 0.5490196f, 1f);

                    Color oldLightGrayColor = new Color(0.7490196f, 0.7490196f, 0.7490196f, 1f);
                    Color newGrayColor = new Color(0.3921569f, 0.3921569f, 0.3921569f, 1f);

                    if (image.color == oldLightGrayColor)
                    {
                        Debug.Log("Found candidate for replace");
                        image.color = newGrayColor;
                    }
                }
            }
        }
    }
}
