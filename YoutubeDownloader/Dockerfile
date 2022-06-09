
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install curl ffmpeg python3.9-minimal ca-certificates -y && cp /usr/bin/python3.9 /usr/bin/python3 && chmod a+rx /usr/bin/python3
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp && chmod a+rx /usr/local/bin/yt-dlp
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["YoutubeDownloader/YoutubeDownloader.csproj", "YoutubeDownloader/"]
RUN dotnet restore "YoutubeDownloader/YoutubeDownloader.csproj"
COPY . .
WORKDIR "/src/YoutubeDownloader"
RUN dotnet build "YoutubeDownloader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YoutubeDownloader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YoutubeDownloader.dll"]