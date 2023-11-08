FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["cloudsharpback.E2ETest/cloudsharpback.E2ETest.csproj", "cloudsharpback.E2ETest/"]
COPY ["cloudsharpback/cloudsharpback.csproj", "cloudsharpback/"]
RUN dotnet restore "cloudsharpback.E2ETest/cloudsharpback.E2ETest.csproj"
COPY . .
COPY ["cloudsharpback.E2ETest/EntryPoint.sh", "cloudsharpback.E2ETest/"]
COPY ["tools", "tools"]
ENV PATH="$PATH:/src/tools"
ENV TOOLPATH="/src/tools"
WORKDIR "/src/cloudsharpback.E2ETest"
RUN chmod +x "/src/cloudsharpback.E2ETest/EntryPoint.sh"
RUN chmod +x "/src/tools"
ENTRYPOINT ["/src/cloudsharpback.E2ETest/EntryPoint.sh"]