FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

# Support for GRPC
RUN apk add --no-cache libc6-compat

WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet dev-certs https --trust
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/
COPY --from=builder /app/out .
CMD [ "dotnet", "Api.dll" ]