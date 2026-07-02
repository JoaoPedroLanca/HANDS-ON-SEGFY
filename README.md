# DesafioSegfy — API de Apólices de Seguro Auto

API em C# / .NET 10 para cadastro de apólices de seguro automóvel, com uma
interface web simples para operá-la pelo navegador.

## Requisitos

- **.NET 10 SDK** (único pré-requisito).
- Não precisa instalar banco de dados: usa **SQLite** (arquivo local), criado
  automaticamente na primeira execução.

## Como executar

Na raiz do repositório:

```bash
dotnet run --project DesafioSegfy
```

O banco (`seguros.db`) é criado/migrado sozinho no startup — sem script manual.
Com o app no ar (perfil padrão `http`, porta **5104**):

- **Interface web:** http://localhost:5104/
- **Swagger (API):** http://localhost:5104/swagger

> Para rodar em HTTPS: `dotnet run --project DesafioSegfy --launch-profile https`
> (porta 7031). Pode ser necessário `dotnet dev-certs https --trust` na 1ª vez.

## Testes

```bash
dotnet test
```

Cobre o domínio (número, CPF/CNPJ, placa, vigência, status/transições) e um teste
de integração da consulta dos 30 dias contra SQLite real.

## Estrutura

Solução com **2 projetos**, dentro do app, as camadas são separadas por **pasta**:

```
DesafioSegfy/            # aplicação
├── Domain/             # Entities, Enums e Service (regras de negócio)
├── Infra/              # DbContext, repositório, exceções de persistência
├── Api/                # Controllers, DTOs, middleware de erros
├── Pages/ + wwwroot/   # interface web (Razor Page + HTML/CSS/JS)
└── Migrations/
DesafioSegfy.Tests/      # xUnit 
```

## Endpoints

| Método | Rota | Ação |
|---|---|---|
| POST | `/apolices` | Cria (gera número e status Ativa) |
| GET | `/apolices` | Lista todas |
| GET | `/apolices/{id}` | Busca por id |
| PATCH | `/apolices/{id}` | Atualiza dados editáveis (parcial) |
| DELETE | `/apolices/{id}` | Remove |
| POST | `/apolices/{id}/cancelar` | Cancela (transição de status) |
| GET | `/apolices/vencendo` | Apólices que vencem em 30 dias |

## Decisões de arquitetura

Cada escolha priorizou **simplicidade proporcional ao tamanho do problema**,
evitando abstração desnecessária:

- **2 projetos, camadas por pasta** — separar em vários projetos seria
  overengineering para esta aplicação, as pastas já deixam as responsabilidades
  explícitas. Mas em um projeto real provavelmente seria projetos diferentes
- **Regras de negócio em serviços de domínio** (`Domain/Service`) — validação de
  CPF/CNPJ e placa, geração do número e transições de status ficam isoladas e
  fáceis de testar, as entidades carregam só os dados.
- **EF Core + SQLite, migração no startup** — o avaliador não instala SGBD nem
  roda script, ou docker, nem nada: basta `dotnet run` e o banco nasce pronto.
- **Consulta dos 30 dias em SQL** (`FromSqlRaw` parametrizado) — atende ao
  requisito de escrever SQL de verdade, sem concatenar string, mas o normal seria realizar com Linq. O restante usa Linq, que é o natural.
- **Número único sob concorrência** — índice único no banco + *retry* na criação:
  o banco é a fonte da verdade, a aplicação apenas expõe.
- **`Expirada` é derivada da data** — no banco só existe `Ativa`/`Cancelada`, o
  status "vencido" é calculado na leitura, não gravado.
- **Mapeamento manual entidade↔DTO** — sem AutoMapper, a entidade nunca é
  retornada direto na resposta HTTP.
- **Erros padronizados** — um único formato `{ "erro": "..." }` (400 para regra de
  negócio, 404 quando não encontra, 409 para conflito de unicidade).
- **Front consumindo a própria API** — a página é servida pelo mesmo app e fala
  com a API via `fetch` (mesma origem, sem CORS, um único deploy).
