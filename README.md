# Korp — Sistema de emissão de notas fiscais

Aplicação web para cadastro de produtos, controle de estoque e emissão de notas fiscais. O sistema foi construído com Angular no frontend e dois serviços independentes em ASP.NET Core, com persistência em PostgreSQL.

## Visão geral

O sistema é dividido por responsabilidade:

- **StockService** mantém produtos, códigos, descrições e saldos;
- **BillingService** mantém notas fiscais, itens, numeração e status;
- o frontend Angular apresenta os fluxos e consome as APIs;
- cada serviço possui seu próprio banco PostgreSQL.

```text
Angular ──> StockService ──> stock-db
       └─> BillingService ─> billing-db

Fechamento de nota:
Angular ──> BillingService ──> StockService
                              ├─ baixa confirmada: nota Closed
                              └─ falha: nota continua Open
```

A separação dos bancos evita que um serviço dependa diretamente das tabelas do outro. A comunicação entre os serviços acontece por HTTP.

## Funcionalidades

### Produtos

- listagem paginada;
- busca por código ou descrição;
- cadastro com saldo inicial;
- edição da descrição;
- entrada de estoque;
- baixa manual de estoque.

### Notas fiscais

- criação de uma nota com vários produtos;
- controle de quantidade por item;
- numeração sequencial;
- status `Open` (Aberta) e `Closed` (Fechada);
- listagem paginada;
- consulta dos detalhes.

### Fechamento

Uma nota é criada como Aberta. Ao solicitar o fechamento, o BillingService verifica o status, envia os itens ao StockService e aguarda a confirmação da baixa. A nota só é salva como Fechada após essa confirmação.

Se o StockService estiver indisponível, o BillingService retorna `503 Service Unavailable` e a nota permanece Aberta. O frontend mostra o estado de processamento e uma mensagem compreensível para o usuário.

## Tecnologias

### Frontend

- Angular 22 com standalone components;
- TypeScript;
- Angular Router;
- RxJS e `HttpClient`;
- `FormsModule` com `ngModel`;
- signals para estado reativo;
- HTML semântico e SCSS próprio.

Os componentes visuais não dependem de Angular Material. A interface usa HTML e SCSS para manter o layout leve e consistente.

### Backend

- C#;
- ASP.NET Core .NET 10;
- Entity Framework Core;
- Npgsql;
- PostgreSQL;
- LINQ;
- xUnit.

### Infraestrutura

- Docker Compose;
- migrations do EF Core executadas na inicialização;
- banco independente para cada serviço.

## Decisões técnicas

### Responsabilidade no fechamento

O BillingService é o dono da nota e conhece seus itens. Por isso, o frontend chama apenas o endpoint de fechamento do BillingService. O BillingService solicita a baixa ao StockService e só então altera o status da nota.

### Numeração

A numeração da nota é gerada por uma sequence do PostgreSQL. O frontend não calcula números, evitando colisões quando mais de uma nota é criada ao mesmo tempo.

### Baixa de estoque

A baixa verifica se o saldo é suficiente antes de subtrair a quantidade. A atualização é condicional e o banco possui uma restrição que impede saldo negativo.

Quando a nota possui vários produtos, a baixa é executada em uma transação. Se qualquer item falhar, o lote inteiro é revertido.

Também existe controle de concorrência para o caso de duas baixas disputarem o último saldo e idempotência com `IdempotencyKey` e `RequestHash` para evitar efeitos duplicados em requisições repetidas.

### Paginação

A paginação ocorre no backend. Os endpoints recebem `page` e `pageSize`; o repositório usa LINQ/EF Core com `Count`, `Skip` e `Take`. A resposta inclui os itens e os metadados da página. O frontend apenas solicita a página e exibe os controles de navegação.

### Erros

Cada API possui um middleware global que transforma erros de validação, domínio e infraestrutura em `ProblemDetails`. Isso mantém o formato das respostas consistente. O frontend interpreta o erro e preserva o estado correto da tela.

## API

### StockService — `http://localhost:5189`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/products?page=1&pageSize=10&search=term` | Lista e filtra produtos |
| `POST` | `/api/products` | Cadastra um produto |
| `GET` | `/api/products/{id}` | Consulta um produto |
| `PUT` | `/api/products/{id}` | Atualiza a descrição |
| `POST` | `/api/stock/{id}/stock-in` | Registra entrada |
| `POST` | `/api/stock/{id}/stock-out` | Registra baixa manual |
| `POST` | `/api/stock/withdraw` | Executa baixa em lote |
| `GET` | `/health` | Verifica a saúde do serviço |

### BillingService — `http://localhost:5073`

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/invoices?page=1&pageSize=10` | Lista notas |
| `POST` | `/api/invoices` | Cria uma nota Aberta |
| `GET` | `/api/invoices/{id}` | Consulta uma nota e seus itens |
| `POST` | `/api/invoices/{id}/close` | Solicita baixa e fechamento |
| `GET` | `/health` | Verifica a saúde do serviço |

As listagens retornam uma estrutura com `items`, `page`, `pageSize`, `totalItems` e `totalPages`.

## Estrutura do projeto

```text
backend/
  Korp.StockService/       # produtos e estoque
  Korp.BillingService/     # notas fiscais
frontend/                  # aplicação Angular
tests/
  Korp.StockService.Tests/
  Korp.BillingService.Tests/
compose.yaml               # APIs e bancos
.env.example               # configuração local
```

No frontend, as telas ficam em `src/app/pages`, os contratos em `src/app/models` e as integrações HTTP em `src/app/services`.

## Executando localmente

### Pré-requisitos

- Docker Desktop com Docker Compose;
- .NET SDK 10;
- Node.js e npm.

### Backend e bancos

Na raiz do projeto:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
docker compose ps
```

Portas padrão:

| Recurso | Porta |
|---|---:|
| Stock API | `5189` |
| Billing API | `5073` |
| Stock PostgreSQL | `5433` |
| Billing PostgreSQL | `5434` |

### Frontend

```powershell
cd frontend
npm install
npm start
```

Acesse `http://localhost:4200`. O proxy do Angular encaminha `/api` para o StockService e `/billing-api` para o BillingService.

## Testes e build

Backend, na raiz:

```powershell
dotnet build Korp.sln --no-restore
dotnet test Korp.sln --no-build
```

Frontend, dentro de `frontend`:

```powershell
npm.cmd test -- --watch=false
npm.cmd run build
```

Os testes frontend usam Vitest e `HttpTestingController` para verificar chamadas HTTP, payloads, estados de loading, sucesso e falha. Os testes backend cobrem domínio, APIs, integração, concorrência e comunicação com o StockService.

## Parando e recuperando um serviço

Para simular a indisponibilidade do estoque:

```powershell
docker compose stop stock-api
```

Depois de testar o comportamento de erro, recupere o serviço:

```powershell
docker compose start stock-api
```

Os dados persistem nos volumes PostgreSQL enquanto os volumes não forem removidos.
