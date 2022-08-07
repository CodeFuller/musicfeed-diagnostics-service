docker pull codefuller/musicfeed-api-service:latest || goto :error
docker pull codefuller/musicfeed-updates-service:latest || goto :error
docker compose up --no-build || goto :error
docker compose down || goto :error

echo [92mFinished successfully![0m
goto :EOF

:error

echo [91mFailed :([0m
