namespace HomeFlow.Service.Services.Interface
{
    public interface IGetBinCollectionFromApi
    {
        Task<bool> FetchAndStoreBinCollectionAsync(string uprn);
    }
}