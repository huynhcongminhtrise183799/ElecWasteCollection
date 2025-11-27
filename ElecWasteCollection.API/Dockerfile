FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build  # Use jammy (Ubuntu 22.04)
WORKDIR /src
COPY ElecWasteCollection.API/*.csproj ./ElecWasteCollection.API/
COPY ElecWasteCollection.Application/*.csproj ./ElecWasteCollection.Application/
COPY ElecWasteCollection.Domain/*.csproj ./ElecWasteCollection.Domain/
COPY ElecWasteCollection.Infrastructure/*.csproj ./ElecWasteCollection.Infrastructure/
RUN dotnet restore ElecWasteCollection.API/ElecWasteCollection.API.csproj
COPY . .
WORKDIR /src/ElecWasteCollection.API
RUN dotnet publish -c Release -o /app/publish --runtime ubuntu.22.04-x64 /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime  # Use jammy (Ubuntu 22.04)
WORKDIR /app

# Install dependencies for Emgu.CV (run script from repo)
RUN apt-get update && apt-get install -y wget && \
    wget https://raw.githubusercontent.com/emgucv/emgucv/master/platforms/ubuntu/22.04/apt_install_dependency && \
    bash apt_install_dependency && \
    rm apt_install_dependency && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ElecWasteCollection.API.dll"]