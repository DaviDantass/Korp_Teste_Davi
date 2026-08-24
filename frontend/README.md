# Frontend Angular

Frontend do sistema de emissao de notas fiscais. A documentacao principal do projeto esta em [../README.md](../README.md).

## Executar

Com o backend e os bancos em Docker:

```powershell
npm install
npm start
```

Abra `http://localhost:4200`. O `proxy.conf.json` encaminha `/api` para o StockService e `/billing-api` para o BillingService.

## Validar

```powershell
npm.cmd test -- --watch=false
npm.cmd run build
```

Os testes usam Vitest e `HttpTestingController`. A aplicacao usa Angular standalone, Router, RxJS, FormsModule, signals, HTML e SCSS proprio.
