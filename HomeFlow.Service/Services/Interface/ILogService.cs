namespace HomeFlow.Service.Services.Interface
{
    public interface ILogService
    {
        Task WriteAsync(string level, string message, Exception ex = null, string source = null);
    }
}
