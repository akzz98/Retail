using Retail.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add TableStorageService to the services container
builder.Services.AddSingleton<TableStorageService>(sp =>
{
    var connectionString = "DefaultEndpointsProtocol=https;AccountName=a1storageservicetest;" +
                            "AccountKey=an4GuQzkA6bUselMYA1PtW5PsDimE8Q8bLB9VZaQ4Xt/8EsWp6Sn3LZtvCksdGQEObPT4twq1RTc+AStiWt5kQ==;" +
                            "EndpointSuffix=core.windows.net";
    var tableName = "ProductsTable";
    return new TableStorageService(connectionString, tableName);
});

//Add BlobStorageService to the services container
builder.Services.AddSingleton<BlobStorageService>(sp =>
{
    var connectionString = "DefaultEndpointsProtocol=https;AccountName=a1storageservicetest;" +
                            "AccountKey=an4GuQzkA6bUselMYA1PtW5PsDimE8Q8bLB9VZaQ4Xt/8EsWp6Sn3LZtvCksdGQEObPT4twq1RTc+AStiWt5kQ==;" +
                            "EndpointSuffix=core.windows.net";
    var containerName = "product-images";
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
