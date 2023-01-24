#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["cloudsharpback/cloudsharpback.csproj", "cloudsharpback/"]
RUN dotnet restore "cloudsharpback/cloudsharpback.csproj"
COPY . .
WORKDIR "/src/cloudsharpback"
RUN dotnet build "cloudsharpback.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cloudsharpback.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update -y 
RUN apt-get -y install curl python3 transmission-cli transmission-common transmission-daemon
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp && chmod a+rx /usr/local/bin/yt-dlp
ENV CURL_CA_BUNDLE=/etc/ssl/certs/ca-certificates.crt
#dev
ENV ASPNETCORE_ENVIRONMENT=Development 
###
ENTRYPOINT ["dotnet", "cloudsharpback.dll"]