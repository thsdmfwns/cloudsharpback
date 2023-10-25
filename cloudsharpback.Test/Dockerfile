FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["cloudsharpback.Test/cloudsharpback.Test.csproj", "cloudsharpback.Test/"]
COPY ["cloudsharpback/cloudsharpback.csproj", "cloudsharpback/"]
RUN dotnet restore "cloudsharpback.Test/cloudsharpback.Test.csproj"
COPY . .
COPY ["cloudsharpback.Test/EntryPoint.sh", "cloudsharpback.Test/"]
COPY ["tools", "tools"]
ENV PATH="$PATH:/src/tools"
ENV TOOLPATH="/src/tools"
WORKDIR "/src/cloudsharpback.Test"
RUN chmod +x "/src/cloudsharpback.Test/EntryPoint.sh"
RUN chmod +x "/src/tools"
#ENTRYPOINT ["tail", "-f", "/dev/null"]
ENTRYPOINT ["/src/cloudsharpback.Test/EntryPoint.sh"]