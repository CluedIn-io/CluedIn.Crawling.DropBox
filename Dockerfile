# Pull base CluedIn Server image
FROM cluedin/cluedin-server:release-v2.3_2.17.2-beta.1.846 AS baseServerImage

ARG DNS_NAME=CluedIn

# Merge files
COPY .artifacts/CluedIn/ServerComponent /app/ServerComponent

# Entrypoint for CluedIn Server
ENTRYPOINT ["powershell", "-c", "C:\\entrypoint.ps1"]