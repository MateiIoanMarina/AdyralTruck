using AdyralTruck.Data.Context;
using AdyralTruck.Service.Mapping;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// data context
builder.Services.AddDbContext<DataContext>(cfg =>
{
    //Server=(local);Initial Catalog=AdyralTruck;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True
    //Server=WIN-1BE6D6M2J9M\\SQLEXPRESS;Initial Catalog=AdyralTruck;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True
    cfg.UseSqlServer("Server=WIN-1BE6D6M2J9M\\SQLEXPRESS;Initial Catalog=AdyralTruck;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// singleton services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// auto mapper
builder.Services.AddAutoMapper(typeof(Program),
    typeof(FurnizorMappingProfile));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

RotativaConfiguration.Setup(app.Environment.WebRootPath);

app.Run();
