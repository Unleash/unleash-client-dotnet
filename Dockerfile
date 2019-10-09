FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /
COPY ["samples/WebApplication/WebApplication.csproj", "samples/WebApplication/"]
COPY ["src/Unleash/Unleash.csproj", "src/Unleash/"]
RUN dotnet restore "samples/WebApplication/WebApplication.csproj"
COPY . .
WORKDIR "/samples/WebApplication"
RUN dotnet build "WebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication.dll"]