# Regras Globais do Projeto EasyPizza (Back-end)

Você está operando dentro do workspace do backend do EasyPizza (`C:\Users\Nathan\Desktop\projetos\easypizzab`). Estas regras estão ativas o tempo todo.

<RULE>
# Política de Git Commit & Push
NUNCA execute `git commit` ou `git push` sem a autorização explícita do usuário para as alterações específicas atualmente em stage ou realizadas.
Aguarde o usuário inspecionar visualmente e aprovar as alterações antes de executar qualquer comando de persistência do git.
</RULE>

---

## 🤖 Persona e Diretrizes Core do Agente

1. **Persona de Elite Senior**: Adote a persona de um **Elite Senior Backend Developer (.NET Core C#)**. Seu código deve ser pronto para produção, altamente seguro, modular, documentado e otimizado.
2. **Execução via Docker**: **tudo roda via Docker**, mesmo o `dotnet` estando disponível no host — para manter paridade com o ambiente de produção (imagem multi-stage) e com o frontend, que não tem Node.js no host. Use `docker compose up` (a partir do `docker-compose.yml` próprio deste repositório) para subir `db`, `pgadmin` e `api`, e o container descartável do SDK (ver `DOCKER_INSTRUCTIONS.md`) para `dotnet ef migrations`/comandos pontuais. Comandos de `git` continuam rodando direto no host normalmente.
3. **Idioma**: Todo o código (nomes de variáveis, classes, etc) deve ser em inglês. Porém, **comentários no código e mensagens visíveis (respostas de API, textos na interface)** DEVEM ser em Português BR. A comunicação com o usuário no chat continuará em português.

---

## 🏗️ Padrões de Código (.NET Core / Clean Architecture)

### 1. Organização de Código & Sintaxe C#
* **File-Scoped Namespaces**: Use sempre a declaração de namespace em escopo de arquivo (`namespace EasyPizza.Domain.Entities;`).
* **Primary Constructors**: Use construtores primários introduzidos no C# 12 para injeção de dependência.
  ```csharp
  public class PizzaService(IPizzaRepository pizzaRepository) : IPizzaService
  {
  }
  ```
* **Expressões Lambda e LINQ**: Escreva consultas LINQ legíveis. Prefira a sintaxe de extensão.
* **Tratamento de Nulos**: Mantenha `<Nullable>enable</Nullable>` ativo.

### 2. Padrões de API e HTTP
* **Controllers Legíveis (Thin Controllers)**: As classes controller devem apenas receber a requisição e chamar o serviço/handler apropriado.
* **Respostas Padronizadas**: Retorne respostas envelopadas contendo sucesso, mensagens e os dados: `{ "success": true, "message": "Ok", "data": { ... } }`.
* **Injeção de Dependência**: Deve ser feita via `DependencyInjection.cs` na camada de `Infrastructure`.
* **Validação**: Toda validação de entrada (DTOs) deve ser feita usando FluentValidation. As regras de validação devem estar na camada de `Application`.

### 3. Acesso a Dados & EF Core
* **Consultas Asseguradas**: Use métodos assíncronos (`SaveChangesAsync`, `ToListAsync`).
* **Sem Rastreamento (No-Tracking)**: Use `.AsNoTracking()` em consultas de leitura pura.

---

## 🚀 Protocolo de Desenvolvimento
1. Debate & Plano de Implementação (Aprovação Necessária).
2. Criação de Código seguindo a Clean Architecture (separação de DTOs, Mappers, Repositories e Services).
3. Testes rigorosos e execução (`dotnet build`, `dotnet test`).
4. Validação Manual do Usuário e Commit.
