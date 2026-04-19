FROM mcr.microsoft.com/dotnet/sdk:10.0 as build

WORKDIR /src

# Copy project files
COPY DonationPlatform.Core/DonationPlatform.Core.csproj DonationPlatform.Core/
COPY DonationPlatform.Data/DonationPlatform.Data.csproj DonationPlatform.Data/
COPY DonationPlatform.API/DonationPlatform.API.csproj DonationPlatform.API/

# Restore dependencies
RUN dotnet restore DonationPlatform.API/DonationPlatform.API.csproj

# Copy source code
COPY DonationPlatform.Core/ DonationPlatform.Core/
COPY DonationPlatform.Data/ DonationPlatform.Data/
COPY DonationPlatform.API/ DonationPlatform.API/

# Build
WORKDIR /src/DonationPlatform.API
RUN dotnet build -c Release -o /app/build

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/build .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5000

ENTRYPOINT ["dotnet", "DonationPlatform.API.dll"]
