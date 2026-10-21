using HomeFlow.Data.Models.DTO;
using HomeFlow.Data.Settings;
using HomeFlow.Service.Mappers;
using Microsoft.EntityFrameworkCore;
using HomeFlow.Service.Services.Interface;

namespace HomeFlow.Service.Services
{
    public class BinCollectionService : IBinCollectionService
    {
        private readonly ApplicationDbContext _context;

        public BinCollectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BinCollectionDto?> GetNextCollectionAsync(string uprn)
        {
            var entity = await _context.BinCollections
                .Where(x => x.UPRN == uprn && x.NextCollection != null)
                .OrderBy(x => x.NextCollection)
                .FirstOrDefaultAsync();

            return entity is null ? null : BinCollectionMapper.ToDto(entity);
        }

        public async Task<IEnumerable<BinCollectionDto>> GetAllCollectionsAsync(string uprn)
        {
            var entities = await _context.BinCollections
                .Where(x => x.UPRN == uprn)
                .ToListAsync();

            return entities.Select(BinCollectionMapper.ToDto);
        }

        public async Task<bool> IsRegisteredAsync(string uprn)
        {
            return await _context.RegisteredUprns
                .AnyAsync(x => x.Uprn == uprn);
        }
    }
}
