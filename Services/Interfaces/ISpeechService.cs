using NAudio.Wave;

namespace BackTranslatorSimultaneous.Services.Interfaces
{
    public interface ISpeechService : IDisposable
    {

        string GetFinalTranscript();
        void StartRecognition();
        void StopRecognition();
    }
}
