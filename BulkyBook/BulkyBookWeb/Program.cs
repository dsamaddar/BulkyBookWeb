using BulkyBook.Business.Services.IServices;
using BulkyBook.Business.Services;
using BulkyBook.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BulkyBookWeb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"));
});

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;           // must have at least one number (0-9)
    options.Password.RequireLowercase = true;       // must have at least one lowercase letter (a-z)
    options.Password.RequireUppercase = true;       // must have at least one uppercase letter (A-Z)
    options.Password.RequireNonAlphanumeric = true; // must have at least one special character (!@#$%^&*()
    options.Password.RequiredLength = 6;            // minimum 6 characters
    options.Password.RequiredUniqueChars = 1;       // minimum unique characters
})
        .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<ICategoryService,CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Customer" })
    .WithStaticAssets();


app.Run();
