#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["leave-management/leave-management.csproj", "leave-management/"]
COPY ["ResourceAutoCompleter/ResourceAutoCompleter.csproj", "ResourceAutoCompleter/"]
RUN dotnet restore "leave-management/leave-management.csproj"
COPY . .
WORKDIR "/src/leave-management"
RUN dotnet build "leave-management.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "leave-management.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "leave-management.dll"]