using HomeFlow.Service.Helpers.Enums.BinTypes;

namespace HomeFlow.Service.Helpers.Extensions
{
    public static class BinTypeExtensions
    {
        public static string GetDisplayName(this BinType binType)
        {
            return binType switch
            {
                BinType.AHP => "Nappy Waste",
                BinType.Refuse => "Black Bin",
                BinType.Food => "Food Waste",
                BinType.Recycling => "Recycling",
                _ => "Unknown"
            };
        }

        public static string GetCssClass(this BinType binType)
        {
            return binType switch
            {
                BinType.Recycling => "recycling",
                BinType.Refuse => "refuse",
                BinType.AHP => "ahp",
                BinType.Food => "food",
                _ => string.Empty
            };
        }
    }
}
