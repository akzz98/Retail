using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Retail.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure TableStorageService with connection strings from appsettings.json
builder.Services.AddSingleton<TableStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("TableStorageConnection");
    var tableName = configuration.GetValue<string>("ConnectionStrings:ProductTableName");  // Correct key
    return new TableStorageService(connectionString, tableName);
});

// Configure CategoryStorageService with connection strings from appsettings.json
builder.Services.AddSingleton<CategoryStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("TableStorageConnection");
    var tableName = configuration.GetValue<string>("ConnectionStrings:CategoryTableName");  // Correct key
    return new CategoryStorageService(connectionString, tableName);
});

// Configure BlobStorageService with connection strings from appsettings.json
builder.Services.AddSingleton<BlobStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("BlobStorageConnection");
    var containerName = configuration.GetValue<string>("ConnectionStrings:BlobContainerName");
    return new BlobStorageService(connectionString, containerName);
});

// Add session services
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Prevent client-side scripts from accessing the session cookie
    options.Cookie.IsEssential = true; // Ensure the session cookie is used for essential functionality
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
