FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Spotsom.Api/Spotsom.Api.csproj", "Spotsom.Api/"]
RUN dotnet restore "Spotsom.Api/Spotsom.Api.csproj"

COPY Spotsom.Api/ Spotsom.Api/
RUN dotnet publish "Spotsom.Api/Spotsom.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/data /app/wwwroot/audio

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    ConnectionStrings__Spotsom="Data Source=/app/data/Spotsom.db"

EXPOSE 5000

ENTRYPOINT ["dotnet", "Spotsom.Api.dll"]
