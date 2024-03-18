namespace GIGXR.Platform.Interfaces
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Dictation;

    public interface IDictationManager
    {
        UniTask<DictationResult> DictateAsync(bool removePunctuation);

        void CancelDictation();
    }
}