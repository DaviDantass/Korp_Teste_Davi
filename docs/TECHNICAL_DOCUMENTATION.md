# Detalhamento técnico

## 1. Objetivo

Aplicação web para cadastrar produtos, controlar estoque e emitir notas fiscais. A nota é criada como Aberta e só fica Fechada depois que o estoque confirma a baixa dos itens.

## 2. Tecnologias

- Frontend: Angular 22, TypeScript, Router, RxJS, FormsModule, HTML e SCSS.
- Backend: C# com ASP.NET Core .NET 10.
- Persistência: Entity Framework Core, Npgsql e PostgreSQL.
- Testes: xUnit no backend e Vitest/HttpTestingController no frontend.
- Execução: Docker Compose.

Não foi utilizado Golang nem Angular Material. Os componentes visuais foram feitos com HTML semântico e SCSS próprio.

## 3. Arquitetura simples

O projeto possui dois serviços:

**StockService** controla produtos, saldos, entradas e baixas.

**BillingService** controla notas, itens, numeração e status.

Cada serviço possui seu próprio banco PostgreSQL. O frontend não acessa banco diretamente.

```text
Angular -> StockService -> stock-db
Angular -> BillingService -> billing-db

Fechamento:
Angular -> BillingService -> StockService -> baixa confirmada
                                      |
                                      +-> sucesso: nota Closed
                                      +-> falha: nota continua Open
```

## 4. Funcionalidades

- Home com navegação;
- Produtos: listagem, busca, paginação, cadastro, edição, entrada e baixa;
- Notas: listagem, paginação e detalhes;
- Nova nota: vários produtos, quantidades e remoção de itens;
- Fechamento: botão visível para notas Abertas, loading e atualização do status;
- Erro de estoque: feedback 503 e nota preservada Aberta.

## 5. Fluxo de fechamento

1. O BillingService cria a nota com status `Open`.
2. O usuário abre os detalhes e solicita a impressão/fechamento.
3. O BillingService verifica se a nota ainda está aberta.
4. O BillingService envia os itens para `POST /api/stock/withdraw`.
5. O StockService verifica os saldos e realiza a baixa.
6. Com sucesso, o BillingService chama `Invoice.Close()` e salva a nota.
7. Se o StockService falhar, o BillingService retorna 503 e não fecha a nota.

A numeração é gerada pelo BillingService usando uma sequence do PostgreSQL. O frontend não calcula o número.

## 6. Endpoints principais

### StockService — `http://localhost:5189`

| Método | Rota | Uso |
|---|---|---|
| GET | `/api/products?page=1&pageSize=10&search=termo` | Lista, busca e pagina produtos |
| POST | `/api/products` | Cadastra produto |
| GET | `/api/products/{id}` | Consulta produto |
| PUT | `/api/products/{id}` | Edita descrição |
| POST | `/api/stock/{id}/stock-in` | Entrada |
| POST | `/api/stock/{id}/stock-out` | Baixa manual |
| POST | `/api/stock/withdraw` | Baixa usada no fechamento |

### BillingService — `http://localhost:5073`

| Método | Rota | Uso |
|---|---|---|
| GET | `/api/invoices?page=1&pageSize=10` | Lista notas paginadas |
| POST | `/api/invoices` | Cria nota Aberta |
| GET | `/api/invoices/{id}` | Consulta detalhes |
| POST | `/api/invoices/{id}/close` | Solicita baixa e fechamento |

## 7. Paginação e estoque

A paginação é feita no backend. O repositório usa LINQ/EF Core para contar, ordenar, aplicar `Skip` e `Take`. O frontend envia a página desejada e exibe os metadados recebidos.

Uma baixa só acontece quando o saldo é suficiente. A atualização é condicional para evitar saldo negativo. A baixa de vários itens é feita em uma transação: se um item falhar, nenhum item do lote é confirmado.

O projeto também possui controle de concorrência e idempotência como melhorias opcionais do desafio. Na prática, isso evita que duas baixas consumam o mesmo último saldo e que uma requisição repetida cause uma segunda baixa.

## 8. Angular e RxJS

As telas usam standalone components e Angular Router. `OnInit` carrega dados quando uma tela é aberta. Os services `StockService` e `BillingService` concentram as chamadas do `HttpClient`.

O `HttpClient` retorna `Observable`. Os componentes usam `subscribe` para tratar sucesso e erro, atualizam signals e controlam loading. Os templates usam `@if`, `@for`, bindings e `ngModel`.

## 9. C#, LINQ e erros

O backend usa ASP.NET Core .NET 10 e Entity Framework Core. LINQ é usado nos filtros de produtos por código/descrição e na paginação com `Count`, `Skip` e `Take`.

Um middleware global transforma erros em `ProblemDetails`. Assim, entradas inválidas, recursos inexistentes, conflitos e indisponibilidade do StockService recebem respostas HTTP padronizadas. O Angular converte essas respostas em mensagens compreensíveis.

## 10. Como executar

```powershell
Copy-Item .env.example .env
docker compose up -d --build
cd frontend
npm install
npm start
```

Frontend: `http://localhost:4200`.

Testes:

```powershell
dotnet test Korp.sln --no-build
cd frontend
npm.cmd test -- --watch=false
npm.cmd run build
```

## 11. Evidências para o vídeo

1. Cadastrar produto e mostrar o saldo.
2. Criar nota com dois itens e mostrar status Aberta.
3. Fechar a nota, mostrar loading, status Fechada e saldo reduzido.
4. Parar `stock-api`, tentar fechar outra nota e mostrar 503 com status Aberta.
5. Iniciar o serviço novamente e concluir o fluxo.

## 12. Entrega

O desafio pede um repositório público, o link do vídeo e este detalhamento técnico. O `LEARN.md` é material pessoal de estudo e está ignorado pelo Git.
