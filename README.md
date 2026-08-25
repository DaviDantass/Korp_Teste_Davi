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

## Detalhamento técnico

### Ciclos de vida do Angular

O ciclo de vida utilizado explicitamente é o `OnInit`, por meio de `ngOnInit`. Ele aparece em `ProductsComponent`, `InvoicesComponent`, `NewInvoiceComponent` e `InvoiceDetailComponent` para iniciar as consultas necessárias assim que cada tela é criada: carregar produtos, listar notas ou buscar os dados da nota selecionada. Não há uso de `OnDestroy`, `AfterViewInit` ou de outros hooks, pois os `Observable`s consumidos são requisições HTTP do `HttpClient`, que emitem uma resposta e são concluídas, sem manter assinaturas contínuas.

### RxJS e fluxo assíncrono

O frontend usa RxJS nos serviços `StockService` e `BillingService`. Os métodos expõem `Observable<T>` retornados pelo `HttpClient` para listagem, consulta, cadastro, movimentação de estoque, criação e fechamento de notas. Os componentes chamam `subscribe` com observadores que tratam:

- `next`: grava os dados recebidos nos signals, encerra indicadores de carregamento/processamento, atualiza a tela e, quando necessário, navega ou inicia `window.print()`;
- `error`: encerra o estado de processamento e apresenta mensagens específicas, como saldo insuficiente (`409`) ou StockService indisponível (`503`).

Neste projeto, `next` e `error` são callbacks passados ao `subscribe`; não há uso direto de um `Subject` nem chamada manual ao método `next()`.

### Bibliotecas e recursos do frontend

- Angular 22 com componentes standalone: estrutura da aplicação e das telas;
- Angular Router: navegação entre produtos, notas, criação e detalhe da nota;
- `HttpClient`: comunicação HTTP com StockService e BillingService;
- RxJS: tipagem e consumo assíncrono das respostas como `Observable`;
- `FormsModule` e `ngModel`: formulários e ligação dos campos da interface;
- Angular signals: estado local reativo, incluindo dados, paginação, mensagens e indicadores de carregamento;
- `DatePipe`: formatação de datas na visualização das notas;
- Vitest e `HttpTestingController`: testes unitários do frontend e das integrações HTTP.

Não foi utilizado Angular Material nem outra biblioteca de componentes visuais. A interface foi construída com HTML semântico e SCSS próprio.

### Backend, dependências e persistência

Os dois microsserviços foram implementados em C# com ASP.NET Core sobre .NET 10. O acesso a dados usa Entity Framework Core 10 com o provider Npgsql para PostgreSQL; cada serviço possui seu próprio banco e executa suas migrations na inicialização. O projeto também usa OpenAPI no ambiente de desenvolvimento e xUnit nos testes do backend.

Golang não foi utilizado. Portanto, gerenciamento de dependências Go, módulos Go e frameworks Golang não se aplicam a esta solução. As dependências C# são declaradas nos arquivos `.csproj`, e as do frontend no `package.json`, gerenciadas pelo npm.

### Erros e exceções no backend

Cada API registra um `ExceptionHandlingMiddleware` global. O middleware captura exceções não tratadas, registra o erro e converte exceções de domínio, validação e infraestrutura em respostas HTTP coerentes. Exemplos: recurso inexistente retorna `404`, dados inválidos retornam `400`, conflitos como produto duplicado, saldo insuficiente ou operação inválida retornam `409`, e indisponibilidade do StockService retorna `503`; falhas inesperadas retornam `500` sem expor detalhes internos.

As respostas usam `ProblemDetails` e o content type `application/problem+json`, preenchendo `status`, `title`, `detail` e `instance`. Na comunicação entre serviços, o `StockServiceClient` também converte falhas HTTP e timeout em exceções específicas para que o BillingService forneça feedback apropriado ao frontend.

### LINQ e paginação

LINQ é utilizado tanto para consultas traduzidas pelo EF Core quanto para transformação e validação em memória. Exemplos reais incluem:

- `Where` para filtrar produtos por código/descrição e para fazer a baixa somente quando `Stock >= quantity`;
- `OrderBy` por código na listagem de produtos e `OrderByDescending` por número na listagem de notas;
- `CountAsync` para calcular a quantidade total de registros;
- `Skip` e `Take` para buscar somente a página solicitada;
- `Select` para converter entidades e itens em DTOs;
- `GroupBy`, `Any` e `Count` para rejeitar produtos repetidos em uma mesma baixa em lote.

Os parâmetros `page` e `pageSize` são normalizados nos serviços, e os DTOs paginados retornam `items`, `page`, `pageSize`, `totalItems` e `totalPages`.

### Concorrência, transação e idempotência

A baixa usa uma atualização condicional executada diretamente no banco por `ExecuteUpdateAsync`: a instrução só subtrai a quantidade quando o produto ainda possui saldo suficiente. Como o teste de saldo e a subtração ocorrem na mesma operação SQL, duas requisições concorrentes disputando a última unidade não conseguem ambas consumi-la. O banco também possui a restrição `stock >= 0` como proteção adicional.

Na baixa de vários itens, o StockService abre uma transação de banco. Todas as atualizações de saldo e o registro da operação são confirmados juntos; se qualquer item falhar, a transação é descartada e nenhuma baixa parcial do lote é mantida. Assim, a baixa em lote é transacional/atômica dentro do StockService.

Para idempotência, o BillingService gera uma chave estável no formato `invoice-close:{invoiceId}`. O StockService usa um advisory lock transacional do PostgreSQL por chave, consulta o registro da operação e mantém um índice único sobre `IdempotencyKey`. A primeira execução armazena também o `RequestHash` e o resultado; uma repetição com a mesma chave e o mesmo conteúdo devolve o resultado salvo sem baixar o estoque novamente, enquanto a reutilização da chave com conteúdo diferente retorna conflito.

Essa idempotência protege contra efeitos duplicados em tentativas repetidas, mas não torna atômicos o fechamento completo entre BillingService e StockService. Os serviços usam bancos separados e não existe transação distribuída: a baixa em lote é atômica apenas no StockService; depois de sua confirmação, o BillingService persiste o status `Closed` em uma operação própria. Se essa gravação posterior falhar, uma nova tentativa pode reutilizar com segurança a baixa já registrada e então concluir o fechamento da nota.

## Decisões técnicas

### Responsabilidade no fechamento

O BillingService é o dono da nota e conhece seus itens. Por isso, o frontend chama apenas o endpoint de fechamento do BillingService. O BillingService solicita a baixa ao StockService e só então altera o status da nota.

### Numeração

A numeração da nota é gerada por uma sequence do PostgreSQL. O frontend não calcula números, evitando colisões quando mais de uma nota é criada ao mesmo tempo.

### Baixa de estoque

A baixa verifica se o saldo é suficiente antes de subtrair a quantidade. A atualização é condicional e o banco possui uma restrição que impede saldo negativo.

Quando a nota possui vários produtos, a baixa é executada em uma transação local do StockService. Se qualquer item falhar, o lote inteiro é revertido. Essa atomicidade não se estende ao BillingService, que persiste o fechamento em seu próprio banco depois que o StockService confirma a baixa.

Também existe controle de concorrência para o caso de duas baixas disputarem o último saldo e idempotência com `IdempotencyKey` e `RequestHash` para evitar efeitos duplicados em requisições repetidas. Idempotência não equivale a uma transação distribuída entre os microsserviços.

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
