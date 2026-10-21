using HomeFlow.Data.Settings;
using HomeFlow.Service.Services;
using HomeFlow.Service.Services.Interface;
using HomeFlow.Service.Settings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bind appsettings
builder.Services.Configure<ConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<DashboardSettings>(builder.Configuration.GetSection("DashboardSettings"));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<BinRefreshSettings>(builder.Configuration.GetSection("BinRefreshSettings"));
builder.Services.Configure<LogSettings>(builder.Configuration.GetSection("LogSettings"));

// register DbContext using the configured connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IBinCollectionService, BinCollectionService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IBinRefreshService, BinRefreshService>();


// Register ingestion client so controller can be activated.
// Short timeout because on-demand refresh runs inside the dashboard request.
builder.Services.AddHttpClient<IGetBinCollectionFromApi, GetBinCollectionFromApi>(client =>
    client.Timeout = TimeSpan.FromSeconds(10));

// Add controllers so API controllers are discovered and Swagger shows operations
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Map Razor Pages endpoints
app.MapRazorPages();

app.Run();