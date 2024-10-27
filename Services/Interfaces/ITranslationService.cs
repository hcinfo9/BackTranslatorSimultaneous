using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackTranslatorSimultaneous.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
    }
}