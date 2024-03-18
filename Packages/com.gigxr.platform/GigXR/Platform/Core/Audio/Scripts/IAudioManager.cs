using System;
using UnityEngine;

namespace GIGXR.Platform.Core.Audio
{
    public interface IAudioManager
    {
        public AudioSource PlayAudio(string audioName, float volume = 1f, bool loopmode = false, float pitch = 1f, Transform spatializeTransform = null, AudioSpatializationParameters spatializationParamters = null, AudioSource audioSourceReference = null);

        public AudioSource StopAudio();

        public AudioDatabase AudioDatabase { get; }

    public void AddMixerEffect(AudioMixerType mixerName, AudioManager.MixerParameters parameter, float amount);
    public void RemoveAndDestroyLoopingAudio(string audioName);

        public event EventHandler<EventArgs> AudioPlayed;
        public event EventHandler<EventArgs> AudioPaused;
        public event EventHandler<EventArgs> AudioFinished;
    }
}
