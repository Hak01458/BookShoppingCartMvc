using BookShoppingCartMvcUI;
using BookShoppingCartMvcUI.Shared;
using Microsoft.AspNetCore.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BookShoppingCartMvcUI.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
// MediatR
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddMediatR(typeof(Program).Assembly);
builder.Services.AddTransient<IHomeRepository, HomeRepository>();
builder.Services.AddTransient<ICartRepository, CartRepository>();
builder.Services.AddTransient<IUserOrderRepository, UserOrderRepository>();
builder.Services.AddTransient<IStockRepository, StockRepository>();
builder.Services.AddTransient<IGenreRepository, GenreRepository>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IReportRepository, ReportRepository>();
// register facade (resolve mediator and cache for CartFacade constructor)
builder.Services.AddTransient<BookShoppingCartMvcUI.Facades.ICartFacade>(sp =>
{
    var cartRepo = sp.GetRequiredService<ICartRepository>();
    var stockRepo = sp.GetRequiredService<IStockRepository>();
    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BookShoppingCartMvcUI.Facades.CartFacade>>();
    var mediator = sp.GetRequiredService<MediatR.IMediator>();
    var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    return new BookShoppingCartMvcUI.Facades.CartFacade(cartRepo, stockRepo, logger, mediator, cache);
});

// register profile service and proxy
builder.Services.AddScoped<BookShoppingCartMvcUI.Services.ProfileService>();
builder.Services.AddScoped<BookShoppingCartMvcUI.Services.IProfileService, BookShoppingCartMvcUI.Services.ProfileProxy>(sp =>
{
    var real = sp.GetRequiredService<BookShoppingCartMvcUI.Services.ProfileService>();
    var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    return new BookShoppingCartMvcUI.Services.ProfileProxy(real, cache);
});

// keep previously added singleton cache if present
builder.Services.AddSingleton<IAppCache, AppCache>();

// Factory Method: register the concrete creator as the abstract creator type
builder.Services.AddTransient<CartItemCreator, BookLeafCreator>();

// Template method registration (if present earlier)
builder.Services.AddTransient<IReportGenerator, TopNSellingReportGenerator>();

var app = builder.Build();
// Uncomment it when you run the project first time, It will registered an admin
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedDefaultData(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
