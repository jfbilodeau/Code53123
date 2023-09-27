FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime imagedocker
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["./ControlCenter"]

# docker build . -t controlcenter:vX
# docker tag controlcenter:v1 MYACR.azurecr.io/demo/controlcenter:vX
# docker push MYACR.azurecr.io/demo/controlcenter:vX