using HomeFlow.Data.Models.DTO;

namespace HomeFlow.Service.Services.Interface
{
    public interface IBinCollectionService
    {
        Task<BinCollectionDto?> GetNextCollectionAsync(string uprn);
        Task<IEnumerable<BinCollectionDto>> GetAllCollectionsAsync(string uprn);
        Task<bool> IsRegisteredAsync(string uprn);
    }
}
