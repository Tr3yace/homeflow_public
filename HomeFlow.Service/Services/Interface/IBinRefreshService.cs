namespace HomeFlow.Service.Services.Interface
{
    public interface IBinRefreshService
    {
        // Refreshes the stored bin data for a registered UPRN if it is stale.
        // Never throws: on failure the caller simply reads the existing stored data.
        Task EnsureFreshAsync(string uprn, CancellationToken cancellationToken = default);
    }
}
