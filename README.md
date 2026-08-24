# Korp — Sistema de emissão de notas fiscais

Sistema web desenvolvido para o desafio técnico da Korp. A aplicação permite cadastrar produtos, controlar saldos de estoque e emitir notas fiscais com múltiplos itens.

## Visão geral

O projeto usa uma arquitetura de microsserviços:

- **StockService**: responsável por produtos, saldos, entradas e baixas;
- **BillingService**: responsável por notas fiscais, itens, numeração e fechamento;
- **Angular**: interface para cadastro, consulta e demonstração do fluxo;
- **PostgreSQL**: persistência real, com um banco por microsserviço;
- **Docker Compose**: execução local dos serviços e bancos.

No fechamento, o fluxo é:

```text
Angular -> BillingService -> StockService -> PostgreSQL de estoque
                                      |
                                      v
                         baixa confirmada ou erro 503
                                      |
                                      v
                           Billing fecha ou mantém Open
```

A nota nasce Aberta. Ela só passa a Fechada depois que todos os itens são baixados com sucesso no StockService.

## Funcionalidades

- Home com navegação para os fluxos principais;
- Produtos: listagem paginada, busca por código/descrição, cadastro, edição, entrada e baixa;
- Notas fiscais: listagem paginada e consulta de detalhes;
- Nova nota: seleção de vários produtos, quantidades e inclusão/remoção de itens;
- Fechamento/impressão: loading, baixa de estoque e mudança para Fechada;
- Tratamento de falha: retorno 503, mensagem amigável e nota preservada como Aberta;
- Baixa atômica, baixa em lote transacional, concorrência sem saldo negativo e idempotência.

## Tecnologias

### Frontend

- Angular 22 standalone;
- TypeScript;
- Angular Router;
- RxJS e `HttpClient`;
- FormsModule com `ngModel`;
- HTML semântico e SCSS próprio.

O projeto não utiliza Angular Material. Os componentes visuais foram feitos com HTML e SCSS para manter a interface simples e alinhada ao escopo.

### Backend

- C#;
- ASP.NET Core .NET 10;
- Entity Framework Core;
- Npgsql;
- PostgreSQL;
- LINQ para filtros, ordenação e paginação;
- xUnit para testes.

### Infraestrutura

- Docker Compose;
- Migrations automáticas na inicialização;
- PostgreSQL separado para StockService e BillingService.

## Como executar

### 1. Pré-requisitos

- .NET SDK 10;
- Node.js e npm;
- Docker Desktop com Docker Compose.

### 2. Configurar variáveis

Na raiz do projeto:

```powershell
Copy-Item .env.example .env
```

Os valores padrão usam PostgreSQL nas portas `5433` e `5434`.

### 3. Subir backend e bancos

```powershell
docker compose up -d --build
docker compose ps
```

As APIs ficam disponíveis em:

| Serviço | URL |
|---|---|
| StockService | `http://localhost:5189` |
| BillingService | `http://localhost:5073` |
| Stock PostgreSQL | `localhost:5433` |
| Billing PostgreSQL | `localhost:5434` |

Health checks:

- `http://localhost:5189/health`;
- `http://localhost:5073/health`.

### 4. Executar frontend

```powershell
cd frontend
npm install
npm start
```

Abra `http://localhost:4200`. O proxy Angular encaminha `/api` para o StockService e `/billing-api` para o BillingService.

## Comandos de validação

Na raiz:

```powershell
dotnet build Korp.sln --no-restore
dotnet test Korp.sln --no-build
```

No frontend:

```powershell
cd frontend
npm.cmd test -- --watch=false
npm.cmd run build
```

Os testes frontend usam Vitest e `HttpTestingController`. O modo `--browsers=ChromeHeadless` não está configurado neste projeto Angular 22 e exige um provider de browser adicional.

## Cenário de falha para demonstração

Com os containers em execução:

```powershell
docker compose stop stock-api
```

Tente fechar uma nota Aberta pela interface. O BillingService retorna `503 Service Unavailable`, a interface informa a indisponibilidade e a nota continua Aberta.

Depois recupere o serviço:

```powershell
docker compose start stock-api
```

Ao tentar novamente, a baixa é confirmada e a nota pode ser fechada.

## Endpoints principais

### StockService

| Método | Rota | Finalidade |
|---|---|---|
| GET | `/api/products?page=1&pageSize=10&search=termo` | Produtos paginados e filtrados |
| POST | `/api/products` | Cadastro de produto |
| GET | `/api/products/{id}` | Consulta de produto |
| PUT | `/api/products/{id}` | Edição de produto |
| POST | `/api/stock/{id}/stock-in` | Entrada de estoque |
| POST | `/api/stock/{id}/stock-out` | Baixa manual |
| POST | `/api/stock/withdraw` | Baixa em lote usada no fechamento |

### BillingService

| Método | Rota | Finalidade |
|---|---|---|
| GET | `/api/invoices?page=1&pageSize=10` | Notas paginadas |
| POST | `/api/invoices` | Criação de nota Aberta |
| GET | `/api/invoices/{id}` | Detalhes da nota |
| POST | `/api/invoices/{id}/close` | Baixa e fechamento condicionado ao sucesso |

## Documentação adicional

- [Documentação técnica completa](docs/TECHNICAL_DOCUMENTATION.md): detalhamento exigido pelo desafio, arquitetura, endpoints, Angular, RxJS, C#, LINQ, erros, banco e evidências;
- `LEARN.md`: roteiro pessoal de estudo e apresentação. É ignorado pelo Git e não faz parte da entrega pública.

## Entrega do desafio

A entrega deve incluir:

1. link do repositório público `Korp_Teste_SeuNome`;
2. link do vídeo de apresentação;
3. detalhamento técnico.

Antes do envio, substitua os links abaixo pelos links finais:

- Repositório: **a preencher**;
- Vídeo: **a preencher**.

O envio deve ser feito para `rh@korp.com.br` dentro do prazo informado no desafio.
