FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copia os arquivos de projeto para restaurar as dependências
COPY src/EasyPizza.Api/*.csproj src/EasyPizza.Api/
COPY src/EasyPizza.Application/*.csproj src/EasyPizza.Application/
COPY src/EasyPizza.Domain/*.csproj src/EasyPizza.Domain/
COPY src/EasyPizza.Infrastructure/*.csproj src/EasyPizza.Infrastructure/
RUN dotnet restore src/EasyPizza.Api/EasyPizza.Api.csproj

# Copia o resto do código e faz o build da aplicação
COPY src/ src/
RUN dotnet publish src/EasyPizza.Api/EasyPizza.Api.csproj -c Release -o /app/out

# Imagem de runtime mais leve (apenas o necessário para rodar)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expõe a porta 8080 (padrão do .NET ASP.NET)
EXPOSE 8080

ENTRYPOINT ["dotnet", "EasyPizza.Api.dll"]
