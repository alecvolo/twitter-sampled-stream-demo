#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

COPY "src/Services/StreamingDemo.Api/StreamingDemo.Api.csproj" "src/Services/StreamingDemo.Api/StreamingDemo.Api.csproj"
COPY "src/Services/StreamingDemo.Api.Tests/StreamingDemo.Api.Tests.csproj" "src/Services/StreamingDemo.Api.Tests/StreamingDemo.Api.Tests.csproj"
COPY "src/Web/StreamingDemo.Web/StreamingDemo.Web.csproj" "src/Web/StreamingDemo.Web/StreamingDemo.Web.csproj"

RUN dotnet restore "src/Services/StreamingDemo.Api/StreamingDemo.Api.csproj"

COPY . .
WORKDIR /src/Services/StreamingDemo.Api

ARG BUILD_VERSION
ENV BUILD_VERSION ${BUILD_VERSION:-0.0.0}
RUN echo $BUILD_VERSION

RUN dotnet publish --no-restore -c Release -o /app /p:Version=$BUILD_VERSION
FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StreamingDemo.Api.dll"]
