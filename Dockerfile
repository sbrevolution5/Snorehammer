FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Snorehammer.Web/Snorehammer.Web.csproj", "./"]
RUN dotnet restore

# Copy the rest of the code
COPY . .
RUN dotnet build "Snorehammer.Web/Snorehammer.Web.csproj" -c Release -o /app/build
RUN dotnet publish "Snorehammer.Web/Snorehammer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:3fcf6f1e809c0553f9feb222369f58749af314af6f063f389cbd2f913b4ad556
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Snorehammer.Web.dll"]
