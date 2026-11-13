FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .editorconfig ./
COPY src/Tally.Domain/Tally.Domain.csproj src/Tally.Domain/
COPY src/Tally.Application/Tally.Application.csproj src/Tally.Application/
COPY src/Tally.Infrastructure/Tally.Infrastructure.csproj src/Tally.Infrastructure/
COPY src/Tally.Api/Tally.Api.csproj src/Tally.Api/
RUN dotnet restore src/Tally.Api/Tally.Api.csproj

COPY src/ src/
RUN dotnet publish src/Tally.Api/Tally.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Tally.Api.dll"]