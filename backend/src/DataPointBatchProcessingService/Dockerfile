# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
# Support for GRPC
RUN apk add --no-cache libc6-compat
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out --no-restore

# Stage 3: Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS production
WORKDIR /app
COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "DataPointBatchProcessingService.dll"]