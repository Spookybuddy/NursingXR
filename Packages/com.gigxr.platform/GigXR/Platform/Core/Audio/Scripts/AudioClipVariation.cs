using System;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    [Serializable]
    public enum VariationCondition
    {
        Random, RespiratoryRate, HeartRate
    }

    [Serializable]
    public class AudioClipVariation
    {

        public AudioClip clipFromVariation;
        public VariationCondition variationCondition;
        public float variationConditionValue;

        public AudioClipVariation()
        {

            clipFromVariation = null;
            variationCondition = VariationCondition.Random;
            variationConditionValue = 1000000;

        }
    }
}