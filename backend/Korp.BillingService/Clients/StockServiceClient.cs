using System.Net;
using System.Net.Http.Json;
using Korp.BillingService.Exceptions;

namespace Korp.BillingService.Clients;

public sealed class StockServiceClient(HttpClient httpClient)
{
    public async Task WithdrawAsync(
        StockWithdrawalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/stock/withdraw",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return;

            if (response.StatusCode == HttpStatusCode.Conflict)
                throw new InvalidOperationException(
                    "Saldo insuficiente para concluir a nota.");

            throw new StockServiceUnavailableException(
                $"O StockService retornou {(int)response.StatusCode}.");
        }
        catch (HttpRequestException exception)
        {
            throw new StockServiceUnavailableException(
                "Não foi possível acessar o StockService.",
                exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new StockServiceUnavailableException(
                "O StockService excedeu o tempo limite.",
                exception);
        }
    }
}
