using AdyralTruck.Data.Context;
using AdyralTruck.Service.Mapping;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// data context
builder.Services.AddDbContext<DataContext>(cfg =>
{
    cfg.UseSqlServer("Server=(local);Initial Catalog=AdyralTruck;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
});

// singleton services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// auto mapper
builder.Services.AddAutoMapper(typeof(Program),
    typeof(FurnizorMappingProfile));

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
