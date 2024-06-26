FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet build -c Release -o /app/build

FROM base AS final
WORKDIR /app

COPY --from=build /app/build .

COPY wwwroot /src/wwwroot
COPY obj /src/obj

ENTRYPOINT [ "dotnet","TRPO_chats.dll" ]
