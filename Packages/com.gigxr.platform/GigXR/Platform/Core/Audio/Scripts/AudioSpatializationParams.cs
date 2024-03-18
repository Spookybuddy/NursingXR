using System;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    [Serializable]
    public class AudioSpatializationParameters
    {
        [SerializeField]
        private float _minDistance;

        [SerializeField]
        private float _maxDistance;

        [SerializeField]
        private AudioRolloffMode _rolloffMode;

        [SerializeField]
        private float _dopplerLevel;

        [SerializeField]
        private float _spread;

        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public AudioRolloffMode RolloffMode => _rolloffMode;
        public float DopplerLevel => _dopplerLevel;
        public float Spread => _spread;

        public AudioSpatializationParameters(float minDistance = 1f, float maxDistance = 500f, AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic, float dopplerLevel = 1f, float spread = 0f)
        {
            this._minDistance = minDistance;
            this._maxDistance = maxDistance;
            this._rolloffMode = rolloffMode;
            this._dopplerLevel = dopplerLevel;
            this._spread = spread;
        }

        private static AudioSpatializationParameters _default;
        public static AudioSpatializationParameters Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new AudioSpatializationParameters();
                }

                return _default;
            }
        }
    }
}
