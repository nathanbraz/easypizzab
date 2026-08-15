# EasyPizza — Backend

API .NET (ASP.NET Core / Entity Framework Core) do EasyPizza: uma plataforma SaaS multi-tenant de pedidos via WhatsApp. O nome é "pizza", mas serve qualquer tipo de comércio — a proposta é vender pra qualquer comerciante de comida. O cliente final manda mensagem no WhatsApp da loja, recebe um link mágico, e faz o pedido online sem cadastro tradicional (nome/telefone são capturados automaticamente pelo link; o único formulário manual é o de endereço).

Repositório irmão: [`easypizza`](../easypizza) (frontend React/Vite) — as duas stacks Docker são independentes, mas rodam lado a lado no mesmo host em desenvolvimento.

## Stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** + **Npgsql** (Postgres 16)
- **ASP.NET Identity** (JWT) para staff (lojista/master) + sessão própria via magic link (não-JWT) para o cliente final
- **FluentValidation** para validação de entrada
- Integração com WhatsApp via **Evolution API** (compatível também com payload no formato UltraMsg)
- **Docker Compose** para banco, pgAdmin e API

## Arquitetura

Clean Architecture em 4 projetos (`EasyPizza.slnx`):

```
src/
  EasyPizza.Domain/          Entidades e regras de negócio puras, sem dependências externas
  EasyPizza.Application/     DTOs, interfaces, serviços de aplicação, validação
  EasyPizza.Infrastructure/  EF Core, repositórios, migrations, integrações externas (WhatsApp)
  EasyPizza.Api/             Controllers, autenticação/autorização, composição de DI (Program.cs)
```

**Multi-tenancy**: cada loja tem seu próprio banco Postgres isolado (`easypizza_{slug}`), com a connection string guardada no banco Master (`easypizza_master`, tabela `Tenants`). O tenant da requisição é resolvido dinamicamente por `HttpTenantProvider` (header `X-Tenant-Slug` → subdomínio do host → rota → query string). Ver `docs/implementation_plan.md` para o racional completo dessa decisão.

**Duas autenticações completamente separadas**:
- **Staff** (lojista/master): JWT clássico via ASP.NET Identity, políticas `RequireTenant`/`RequireMaster` + permissões granulares (PBAC).
- **Cliente final**: token opaco (`OrderSession`, gerado pelo magic link do WhatsApp), validado a cada request contra o banco do tenant, enviado no header `X-Customer-Session`. Política `RequireCustomerSession`. Não é JWT — decisão deliberada, ver `docs/implementation_plan.md`.

## Rodando localmente

Tudo roda via Docker (o host de desenvolvimento não precisa ter o SDK do .NET instalado, embora ele também funcione localmente se preferir).

```bash
cp .env.example .env   # preencha os valores reais (chave JWT, senha do Postgres, senha do master)
docker compose up --build -d
```

Guia completo (subir o ambiente, aplicar migrations, ver logs, resetar) em [`DOCKER_INSTRUCTIONS.md`](./DOCKER_INSTRUCTIONS.md).

Serviços expostos:
- API: `http://localhost:5000` (Swagger/OpenAPI em `/openapi` no ambiente de Development)
- Postgres: `localhost:5432`
- pgAdmin: `http://localhost:5050`

## Documentação

- [`docs/implementation_plan.md`](./docs/implementation_plan.md) — decisões de arquitetura, fluxo completo do cliente, catálogo de bugs/dívidas técnicas corrigidos e o roadmap. Espelhado no repo do frontend.
- [`DOCKER_INSTRUCTIONS.md`](./DOCKER_INSTRUCTIONS.md) — operação do ambiente Docker no dia a dia.
- `.env.example` — variáveis de ambiente esperadas (segredos nunca ficam no `appsettings.json`, só em `.env`, que não é versionado).
