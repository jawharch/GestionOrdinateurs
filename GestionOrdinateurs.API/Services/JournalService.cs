namespace GestionOrdinateurs.API.Services;

public sealed class JournalService : IAsyncDisposable
{
    private readonly string _reparationsPath;
    private readonly string _ordinateursPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private StreamWriter? _reparationsWriter;
    private StreamWriter? _ordinateursWriter;
    private bool _disposed;

    public JournalService(string contentRootPath)
    {
        _reparationsPath = Path.Combine(contentRootPath, "Logs", "Reparations", "reparations.log");
        _ordinateursPath = Path.Combine(contentRootPath, "Logs", "Ordinateurs", "ordinateurs.log");
    }

    public Task EcrireReparationAsync(string message, CancellationToken cancellationToken = default) =>
        EcrireAsync(message, reparation: true, cancellationToken);

    public Task EcrireOrdinateurAsync(string message, CancellationToken cancellationToken = default) =>
        EcrireAsync(message, reparation: false, cancellationToken);

    private async Task EcrireAsync(
        string message,
        bool reparation,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var path = reparation ? _reparationsPath : _ordinateursPath;
            var writer = reparation ? _reparationsWriter : _ordinateursWriter;
            if (writer == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                writer = new StreamWriter(path, append: true);
                if (reparation)
                {
                    _reparationsWriter = writer;
                }
                else
                {
                    _ordinateursWriter = writer;
                }
            }

            await writer.WriteLineAsync($"{DateTime.Now:dd/MM/yyyy} - {message}".AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_reparationsWriter != null)
            {
                await _reparationsWriter.DisposeAsync();
            }

            if (_ordinateursWriter != null)
            {
                await _ordinateursWriter.DisposeAsync();
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}
