using HomeFlow.Data.Models.DTO;

namespace HomeFlow.Service.Mappers
{
    public static class BinCollectionMapper
    {
        public static BinCollectionDto ToDto(BinCollection entity)
        {
            if (entity == null)
                return null;

            return new BinCollectionDto
            {
                ServiceName = entity.ServiceName,
                Round = entity.Round,
                RoundGroup = entity.RoundGroup,
                ScheduleDescription = entity.ScheduleDescription,
                LastCollection = entity.LastCollection,
                NextCollection = entity.NextCollection,
                MissedCollectionAllowed = entity.MissedCollectionAllowed,
                LastUpdated = entity.LastUpdated
            };
        }
    }
}
