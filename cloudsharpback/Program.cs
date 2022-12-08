using cloudsharpback.Hubs;
using cloudsharpback.Middleware;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using tusdotnet;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IDBConnService, DBConnService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IShareService, ShareService>();
builder.Services.AddSingleton<ITusService, TusService>();
builder.Services.AddSingleton<IYoutubeDlService, YoutubeDlService>();
builder.Services.AddSingleton<ITorrentDlService, TorrentDlService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue; 
    x.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .WithExposedHeaders(tusdotnet.Helpers.CorsHelper.GetExposedHeaders()));

app.UseTus(ctx => ctx.RequestServices.GetService<ITusService>()!.GetTusConfiguration());

app.UseMiddleware<HttpErrorMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<YoutubeDlHub>("/ytdl");

app.Run();
