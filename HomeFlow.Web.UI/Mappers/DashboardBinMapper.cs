using HomeFlow.Data.Models.DTO;
using HomeFlow.Service.Helpers;
using HomeFlow.Service.Helpers.Extensions;
using HomeFlow.Web.UI.Pages.Dashboard.ViewModels;

namespace HomeFlow.Web.UI.Mappers
{
    public static class DashboardBinMapper
    {
        public static DashboardBinViewModel? Map(BinCollectionDto dto)
        {
            var binType = BinTypeParser.Parse(dto.ServiceName);

            if (!binType.HasValue)
                return null;

            var daysUntil = dto.NextCollection.HasValue
                ? (dto.NextCollection.Value.Date - DateTime.Today).Days
                : 0;

            return new DashboardBinViewModel
            {
                ServiceName = binType.Value.GetDisplayName(),

                CssClass = binType.Value.GetCssClass(),

                NextCollection = dto.NextCollection,

                LastUpdated = dto.LastUpdated,

                CollectionStatus = daysUntil switch
                {
                    0 => "Today",
                    1 => "Tomorrow",
                    > 1 => $"In {daysUntil} days",
                    _ => "Past Due"
                }
            };
        }
    }
}