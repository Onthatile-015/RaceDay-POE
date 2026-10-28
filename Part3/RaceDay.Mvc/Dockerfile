# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first so the NuGet restore layer is cached
COPY RaceDay.Mvc.csproj ./
RUN dotnet restore RaceDay.Mvc.csproj

# Then copy the rest of the source and publish
COPY . .
RUN dotnet publish RaceDay.Mvc.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ApiSettings__BaseUrl=http://host.docker.internal:5080/

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]