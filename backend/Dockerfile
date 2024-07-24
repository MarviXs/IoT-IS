FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app
COPY src/Api/*.csproj ./
RUN dotnet restore
COPY src/Api/ ./
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
CMD [ "dotnet", "Api.dll" ]