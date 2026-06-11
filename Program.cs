using GestionPistasWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<TramoService>();
builder.Services.AddSingleton<PersistenciaService>();
builder.Services.AddSingleton<ReporteService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var persistencia = app.Services.GetRequiredService<PersistenciaService>();
var tramoService = app.Services.GetRequiredService<TramoService>();
persistencia.CargarTramos(tramoService);

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicación iniciada correctamente.");

app.Run();
