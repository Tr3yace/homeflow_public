using HomeFlow.Service.Helpers.Enums.BinTypes;

namespace HomeFlow.Service.Helpers
{
    public static class BinTypeParser
    {
        public static BinType? Parse(string? serviceName)
        {
            return Enum.TryParse<BinType>(
                serviceName,
                true,
                out var result)
                ? result
                : null;
        }
    }
}