# Guia de Sobrevivência: Ambiente Docker (EasyPizza)

Este documento descreve o passo a passo para inicializar, configurar e gerenciar o ambiente de desenvolvimento do EasyPizza utilizando Docker.

> **Backend e frontend são duas stacks Docker independentes** (repos separados, cada um com seu próprio `docker-compose.yml`). Não há mais dependência cruzada entre os dois — o compose do backend não builda mais o frontend. Em dev, os dois rodam lado a lado no mesmo host e se comunicam via `localhost` (o frontend fala com a API em `http://localhost:5000/api`, configurável via `VITE_API_URL` no `docker-compose.yml` do frontend).

## 1. Subindo o Ambiente (O "Botão de Ligar")

Sempre que você ligar o computador e for começar a programar, você precisa subir as **duas stacks**, uma em cada terminal (ou sequencialmente no mesmo terminal):

```bash
# Backend: Banco de Dados, pgAdmin e API (.NET)
cd /home/nathan/projects/easypizzab
docker compose up --build -d
```

```bash
# Frontend: React/Vite
cd /home/nathan/projects/easypizza
docker compose up --build -d
```

*(O `--build` garante que se você instalou pacotes novos ou mudou o Dockerfile, ele vai reconstruir. O `-d` libera o seu terminal após ligar os containers).*

Antes de subir o backend pela primeira vez, copie `easypizzab/.env.example` para `easypizzab/.env` e preencha os valores (chave JWT, senha do Postgres, senha do usuário Master) — esse arquivo nunca é versionado, é a única fonte de segredos do backend.

## 2. Acessando os Sistemas

Com os containers rodando, você acessa tudo pelo navegador:

*   **Frontend (React):** `http://admin.lvh.me:3333` (O `lvh.me` funciona como localhost para testes de subdomínio).
*   **API (Swagger/Endpoints):** `http://localhost:5000` (Redireciona internamente para a 8080 da API).
*   **Painel do Banco (pgAdmin):** `http://localhost:5050`
    *   **Login:** `admin@easypizza.com`
    *   **Senha:** `admin`

## 3. Configurando a Conexão no pgAdmin

Para visualizar o banco de dados visualmente pela primeira vez no pgAdmin, você deve registrar o servidor:

1. Acesse o pgAdmin (`http://localhost:5050`) e clique em **Add New Server**.
2. Na aba **General**, digite um nome (ex: `EasyPizza DB`).
3. Na aba **Connection**, preencha exatamente assim:
   *   **Host name/address:** `db` *(Muito importante! Não use localhost)*
   *   **Port:** `5432`
   *   **Username:** `postgres`
   *   **Password:** `1234`
4. Salve.

## 4. Atualizando o Banco de Dados (Rodando Migrations)

Se você recriou os containers do zero ou puxou código novo que altera o banco de dados, você precisará aplicar as *Migrations* do EF Core.
Como a nossa API de produção não tem as ferramentas do SDK instaladas, usamos um container temporário e descartável (que funciona como nossa esteira de CI/CD local) para fazer o serviço sujo.

> **Importante desde que os segredos saíram do `appsettings.json`** (ver seção sobre `.env`): esse container temporário também precisa do `--env-file .env`, senão a API recusa subir por falta da chave JWT (`Jwt:Key`) e as ferramentas do EF Core não conseguem nem descobrir os `DbContext`s.

Estando na pasta `/home/nathan/projects/easypizzab`, rode o comando abaixo:

```bash
docker run --rm -v $(pwd):/app -w /app/src/EasyPizza.Api --network easypizzab_default --env-file .env mcr.microsoft.com/dotnet/sdk:10.0 bash -c "dotnet restore && dotnet build && dotnet tool install -g dotnet-ef && export PATH=\"\$PATH:/root/.dotnet/tools\" && dotnet ef database update --context MasterDbContext --connection \"Host=db;Database=easypizza_master;Username=postgres;Password=1234\""
```
*Esse comando baixa o SDK, espelha seu código, compila, instala a ferramenta do EF, roda no banco Master e depois se auto-destrói.*

## 5. Visualizando Logs (Estilo Console do Visual Studio)

Se você quiser ver o que está acontecendo por trás dos panos (requisições chegando na API, erros, etc.), use o terminal para "seguir" os logs em tempo real:

*   **Ver apenas os logs da API:**
    ```bash
    docker logs -f easypizza_api
    ```
*   **Ver apenas os logs do Frontend:**
    ```bash
    docker logs -f easypizza_frontend
    ```
*   **Ver todos os logs misturados:**
    ```bash
    docker compose logs -f
    ```

Para sair da tela de logs, basta apertar `Ctrl + C`.

## 6. Desligando Tudo

Quando terminar de trabalhar e quiser liberar a memória RAM do computador, desligue as duas stacks:

```bash
cd /home/nathan/projects/easypizzab && docker compose down
cd /home/nathan/projects/easypizza && docker compose down
```
*(O `down` desliga e remove os containers, mas seus dados do banco continuam salvos no volume `postgres_data`).*
