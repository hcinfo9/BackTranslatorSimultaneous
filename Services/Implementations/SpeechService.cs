using System;
using System.IO;
using System.Text;
using BackTranslatorSimultaneous.Services.Interfaces;
using NAudio.Wave;
using Vosk;

public class SpeechService : ISpeechService, IDisposable
{
    private readonly Model _model;
    private readonly WaveInEvent _waveIn;
    private readonly VoskRecognizer _recognizer;
    private StringBuilder _finalTranscript; // Acumulador para o texto final
    private string _lastPartialResult = string.Empty;
    private DateTime _lastSpokenTime;
    private readonly TimeSpan _silenceTimeout = TimeSpan.FromSeconds(1);

    public event Action<string> OnFinalResult; // Evento para resultados finais

    public SpeechService(string modelPath)
    {
        _model = new Model(modelPath);

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1) // 16kHz, mono
        };

        _recognizer = new VoskRecognizer(_model, 16000.0f);

        _waveIn.DataAvailable += WaveIn_DataAvailable;
        _finalTranscript = new StringBuilder(); // Inicializa o acumulador de texto
    }

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
        {
            var result = _recognizer.Result(); // Resultado completo
            var text = ExtractTextFromResult(result);
            if (!string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Texto reconhecido: " + text);
                _finalTranscript.Append(text + " "); // Acumula o texto completo
                OnFinalResult?.Invoke(text);
            }
        }
        else
        {
            var partialResult = ExtractTextFromResult(_recognizer.PartialResult());

            // Apenas exibe o parcial se for diferente do último e não vazio
            if (!string.IsNullOrWhiteSpace(partialResult) && partialResult != _lastPartialResult)
            {
                Console.WriteLine("Texto parcial: " + partialResult);
                _lastPartialResult = partialResult;
                _lastSpokenTime = DateTime.Now;
            }
        }

        // Reset no texto parcial após timeout de silêncio
        if (DateTime.Now - _lastSpokenTime > _silenceTimeout)
        {
            _lastPartialResult = string.Empty;
        }
    }

    private string ExtractTextFromResult(string resultJson)
    {
        // Verifica se a string JSON contém o campo "text" e extrai o valor de texto
        var startIndex = resultJson.IndexOf("\"text\" : \"") + 10;
        var endIndex = resultJson.LastIndexOf("\"");

        if (startIndex >= 10 && endIndex > startIndex)
        {
            return resultJson.Substring(startIndex, endIndex - startIndex);
        }

        return string.Empty;
    }

    public void StartRecognition()
    {
        _finalTranscript.Clear(); // Limpa o acumulador no início de cada gravação
        _waveIn.StartRecording();
        Console.WriteLine("Reconhecimento de fala iniciado.");
    }

    public void StopRecognition()
    {
        _waveIn.StopRecording();
        Console.WriteLine("Reconhecimento de fala finalizado.");
    }

    public string GetFinalTranscript()
    {
        return _finalTranscript.ToString().Trim(); // Retorna o texto consolidado
    }

    public void Dispose()
    {
        _recognizer.Dispose();
        _waveIn.Dispose();
        _model.Dispose();
    }
}
