using TMPro;
using UnityEngine;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// An AssetTypeComponent that holds information locally for an Asset. This data is set when creating the Prefab that represents an Asset.
    /// </summary>
    public class MetaDataAssetTypeComponent : MonoBehaviour
    {
        #region Editor Set Values

        [SerializeField]
        private string prefabModelVersion;

        [SerializeField]
        private GameObject infoMenuPrefab;

        [SerializeField]
        private Vector3 positionOffset;

        #endregion

        private GameObject InfoMenuInstance
        {
            get 
            {
                if(_infoMenuInstance == null)
                    _infoMenuInstance = Instantiate(infoMenuPrefab, gameObject.transform);

                return _infoMenuInstance; 
            }
        }

        private GameObject _infoMenuInstance;

        private TextMeshProUGUI MetaDataDisplay
        {
            get
            {
                if (_metaDataDisplay == null)
                    _metaDataDisplay = InfoMenuInstance.GetComponentInChildren<TextMeshProUGUI>();

                return _metaDataDisplay;
            }
        }

        private TextMeshProUGUI _metaDataDisplay;

        #region PublicAPI

        public string PrefabModelVersion { get { return prefabModelVersion; } }

        public void ShowMenu()
        {
            InfoMenuInstance.SetActive(true);

            MetaDataDisplay.text = $"Model Version: {prefabModelVersion}";

            InfoMenuInstance.transform.localPosition = positionOffset;
        }

        public void HideMenu()
        {
            InfoMenuInstance.SetActive(false);
        }

        #endregion
    }
}