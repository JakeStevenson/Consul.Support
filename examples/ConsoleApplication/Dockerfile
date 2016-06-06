FROM microsoft/dotnet:latest

COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]

ENTRYPOINT ["dotnet", "run"]

