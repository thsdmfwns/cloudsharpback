using cloudsharpback.Filter;
using cloudsharpback.Hubs;
using cloudsharpback.Middleware;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using tusdotnet;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IPathStore, PathStore>();
builder.Services.AddScoped<IDBConnService, DBConnService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberFileService, MemberFileService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<ITusService, TusService>();
builder.Services.AddScoped<IYoutubeDlService, YoutubeDlService>();
builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IFileStreamService, FileStreamService>();
//store
builder.Services.AddSingleton<ITicketStore, TicketStore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(x => x.OperationFilter<AddAuthHeaderOperationFilter>());
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
                .AllowAnyOrigin()
                .WithExposedHeaders(tusdotnet.Helpers.CorsHelper.GetExposedHeaders()));

app.UseTus(ctx => ctx.RequestServices.GetService<ITusService>()!.GetTusConfiguration());

app.UseMiddleware<HttpErrorMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHub<YoutubeDlHub>("/ytdl");

app.Run();
