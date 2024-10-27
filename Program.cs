using BackTranslatorSimultaneous.Services.Interfaces;
using BackTranslatorSimultaneous.Services;
using BackTranslatorSimultaneous.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using BackTranslatorSimultaneous.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configuração do caminho do modelo Vosk
string modelPath = @"C:\Users\Henrique Donato\Desktop\Projetos\Project_Translator Simultaneous\BackTranslatorSimultaneous\BackTranslatorSimultaneous\Models\vosk-model-small-en-us-0.15";

// Adiciona o SpeechService ao DI container como singleton
builder.Services.AddSingleton<ISpeechService>(new SpeechService(modelPath));

// Configura as definições de tradução a partir do arquivo appsettings.json
builder.Services.Configure<TranslationSettingsDTO>(builder.Configuration.GetSection("AzureTranslator"));

// Adiciona o serviço de tradução ao contêiner de injeção de dependência
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

// Configuração do SignalR para WebSocket
builder.Services.AddSignalR();

// Configura o serviço Ollama com a interface
builder.Services.AddHttpClient<IOllamaService, OllamaService>();

// Adiciona controladores e Swagger para API Documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure o pipeline de requisição HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configura o endpoint para o SignalR Hub
app.MapHub<TranslationHub>("/translationHub");

app.Run();
