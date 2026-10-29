namespace GestionOrdinateurs.API.Services;

public class StockService
{
    private readonly object _verrou = new();

    public int StockSSD { get; private set; } = 10;

    public async Task ConsommerSSDAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);

        lock (_verrou)
        {
            if (StockSSD > 0)
            {
                StockSSD--;
            }
        }
    }
}
