using cloudsharpback.Filter;
using cloudsharpback.Hubs;
using cloudsharpback.Middleware;
using cloudsharpback.Repository;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
//scope
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberFileService, MemberFileService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<IYoutubeDlService, YoutubeDlService>();
builder.Services.AddScoped<IFileStreamService, FileStreamService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IShareRepository, ShareRepository>();
builder.Services.AddScoped<IPasswordStoreService, PasswordStoreService>();
builder.Services.AddScoped<IPasswordStoreDirectoryRepository, PasswordStoreDirectoryRepository>();
builder.Services.AddScoped<IPasswordStoreValueRepository, PasswordStoreValueRepository>();
builder.Services.AddScoped<IPasswordStoreKeyRepository, PasswordStoreKeyRepository>();
builder.Services.AddScoped<ITicketStore, TicketStore>();
//singleton
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddSingleton<IDBConnectionFactory, DBConnectionFactory>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IPathStore, PathStore>();
builder.Services.AddSingleton<IEnvironmentValueStore, EnvironmentValueStore>(); ;
//grpc
builder.Services.AddGrpc();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cloud# Back End",
        Version = "v1"
    });
    x.OperationFilter<AddAuthHeaderOperationFilter>();
    x.EnableAnnotations();
});
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

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin());

app.UseMiddleware<HttpErrorMiddleware>();

app.MapGrpcService<TusdHookService>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<YoutubeDlHub>("/ytdl");

app.Run();
