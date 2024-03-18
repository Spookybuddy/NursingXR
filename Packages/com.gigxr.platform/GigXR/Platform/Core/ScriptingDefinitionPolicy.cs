using System.Collections;
using UnityEngine;

namespace GIGXR.Platform
{
    [ExecuteInEditMode]
    public class ScriptingDefinitionPolicy : MonoBehaviour
    {
        // --- Serialised Variables:

        [SerializeField] private string definition;

        // --- Public Methods:

        /// <summary>
        /// Set the gameobject active if the definition matches any of the input definitions.
        /// </summary>
        /// <param name="defines"></param>
        public void EnableForDefines(string[] defines)
        {
            bool contained = false;

            foreach (string s in defines)
            {
                if (s == definition) contained = true;
            }

            gameObject.SetActive(contained);
        }
    }
}