using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserService.Services.UserService>();
builder.Services.AddSingleton<KafkaConsumerService>();
builder.Services.AddHostedService<KafkaConsumerHostedService>();
builder.Services.AddSingleton<KafkaProducerService>();


// --- Добавляем CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // Разрешить любые домены
            .AllowAnyMethod()      // Разрешить любые HTTP методы (GET, POST, PUT, DELETE)
            .AllowAnyHeader();     // Разрешить любые заголовки
    });
});

// Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });
    c.SchemaFilter<IgnoreIdSchemaFilter>(); // Подключение кастомного фильтра
});


var app = builder.Build();


// --- Подключаем CORS до авторизации ---
app.UseCors("AllowAll");  // применяем политику

// Swagger для режима разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
