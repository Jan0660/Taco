FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build/
COPY . .
RUN dotnet publish DiscordBridge -p:PublishSingleFile=true --no-self-contained -r linux-x64 -c release

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS prod
WORKDIR /app/
COPY --from=build /build/DiscordBridge/bin/release/net6.0/linux-x64 ./bin
CMD ["./bin/DiscordBridge"]
