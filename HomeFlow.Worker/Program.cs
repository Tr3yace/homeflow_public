using HomeFlow.Data.Settings;
using HomeFlow.Service.Services;
using HomeFlow.Service.Services.Interface;
using HomeFlow.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WorkerSettings>(
    builder.Configuration.GetSection("WorkerSettings"));

builder.Services.Configure<LogSettings>(
    builder.Configuration.GetSection("LogSettings"));

builder.Services.Configure<DashboardSettings>(
    builder.Configuration.GetSection("DashboardSettings"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IGetBinCollectionFromApi,
    GetBinCollectionFromApi>();

builder.Services.AddScoped<ILogService,
    LogService>();

builder.Services.AddHostedService<BinRefreshWorker>();

var host = builder.Build();

host.Run();