using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddSingleton<ChessSchoolAPI.Services.UserService>();
builder.Services.Configure<ChessSchoolAPI.Services.ChessStudentService>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<ChessSchoolAPI.Services.ChessStudentService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});



// Добавьте авторизацию
builder.Services.AddAuthorization();

// Other configurations...
// Log Redis-related information
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.Configuration = redisConnection;

    // Добавление логирования Redis
    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnection);
    options.ConfigurationOptions.AbortOnConnectFail = false; // Отключаем автоотключение
    options.ConfigurationOptions.AllowAdmin = true; // Разрешить выполнение административных команд
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chess API", Version = "v1" });
    c.SchemaFilter<IgnoreIdSchemaFilter>(); // Подключение кастомного фильтра
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введите 'Bearer' [пробел] и ваш токен в поле ниже.\n\nПример: 'Bearer eyJhbGciOiJIUzI1NiIsIn...'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Logging.AddConsole();
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Приложение запускается...");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Настройка логирования Redis
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // Логирование в консоль
});



//var logger = loggerFactory.CreateLogger<Program>();

// Подключение к Redis и логирование
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);

// Подписка на события Redis
redisConnection.ConnectionFailed += (sender, args) =>
{
    logger.LogError($"Redis connection failed: {args.Exception?.Message}");
};

redisConnection.ConnectionRestored += (sender, args) =>
{
    logger.LogInformation($"Redis connection restored: {args.EndPoint}");
};

redisConnection.ErrorMessage += (sender, args) =>
{
    logger.LogError($"Redis error message: {args.Message}");
};

redisConnection.ConfigurationChanged += (sender, args) =>
{
    logger.LogInformation($"Redis configuration changed: {args.EndPoint}");
};

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chess API v1");
    });
}

// Добавьте использование CORS перед другими Middlewares
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseDeveloperExceptionPage();
app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
