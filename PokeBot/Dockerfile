FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PokeBot/PokeBot.csproj", "PokeBot/"]
RUN dotnet restore "PokeBot/PokeBot.csproj"
COPY . .
WORKDIR "/src/PokeBot"
RUN dotnet build "PokeBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PokeBot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PokeBot.dll"]