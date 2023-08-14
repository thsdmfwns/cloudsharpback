FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["cloudsharpback.Test/cloudsharpback.Test.csproj", "cloudsharpback.Test/"]
COPY ["cloudsharpback/cloudsharpback.csproj", "cloudsharpback/"]
RUN dotnet restore "cloudsharpback.Test/cloudsharpback.Test.csproj"
COPY . .
WORKDIR "/src/cloudsharpback.Test"
RUN mkdir /var/cloud-sharp/
ENTRYPOINT ["dotnet", "test"]
