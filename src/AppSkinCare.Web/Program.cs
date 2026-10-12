using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using AppSkinCare.Core;
using AppSkinCare.Core.Models;
using AppSkinCare.Web;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
{
    o.AddServerHeader = false;
    o.Limits.MaxRequestBodySize = 64 * 1024; // una lista de ingredientes cabe de sobra
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

// Todo el conocimiento vive en la base de ingredientes (JSON incrustado en Core).
builder.Services.AddSingleton(CatalogoIngredientes.Cargar());
builder.Services.AddSingleton<Evaluador>();
builder.Services.AddSingleton<GeneradorRutina>();

// Límite sencillo por IP, solo para la API. En Azure App Service la IP real llega con
// ASPNETCORE_FORWARDEDHEADERS_ENABLED=true, que App Service activa por defecto en Linux.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("api", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

// Un error inesperado nunca enseña detalles técnicos ni el texto que escribió la persona.
app.UseExceptionHandler(manejador => manejador.Run(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = 500,
        Title = "Algo ha fallado por nuestra parte",
        Detail = "No he podido revisar esto ahora mismo. Inténtalo de nuevo en un rato."
    }, options: null, contentType: "application/problem+json");
}));

app.Use(async (contexto, siguiente) =>
{
    var cabeceras = contexto.Response.Headers;
    cabeceras["X-Content-Type-Options"] = "nosniff";
    cabeceras["Referrer-Policy"] = "no-referrer";
    cabeceras["X-Frame-Options"] = "DENY";
    cabeceras["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), interest-cohort=()";
    cabeceras["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; font-src 'self'; " +
        "connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    await siguiente();
});

app.UseRateLimiter();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = archivo =>
    {
        var esFuente = archivo.File.Name.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase);
        archivo.Context.Response.Headers.CacheControl = esFuente
            ? "public, max-age=31536000, immutable"
            : "no-cache"; // revalidar siempre: así los cambios de contenido se ven al momento
    }
});

// La portada se sirve a mano: con el enrutado implícito, el fallback 404 se adelantaría a UseDefaultFiles.
app.MapGet("/", async contexto =>
{
    contexto.Response.ContentType = "text/html; charset=utf-8";
    contexto.Response.Headers.CacheControl = "no-cache";
    await contexto.Response.SendFileAsync(app.Environment.WebRootFileProvider.GetFileInfo("index.html"));
});

var api = app.MapGroup("/api").RequireRateLimiting("api");

api.MapGet("/salud", (CatalogoIngredientes catalogo) =>
    Results.Ok(new { estado = "ok", ingredientes = catalogo.Todos.Count }));

api.MapPost("/evaluar", (PeticionEvaluacion? peticion, Evaluador evaluador) =>
{
    if (peticion is null) return Pedido("Falta lo que quieres revisar.");

    // Se valida aquí para devolver un mensaje limpio, sin el nombre interno del parámetro.
    return peticion.Validar() is { } problema
        ? Pedido(problema)
        : Results.Ok(evaluador.Evaluar(peticion));
});

api.MapPost("/rutina", (PerfilRutina? perfil, GeneradorRutina generador) =>
{
    if (perfil is null) return Pedido("Faltan tus datos.");

    return perfil.Validar() is { } problema
        ? Pedido(problema)
        : Results.Ok(generador.Generar(perfil));
});

api.MapGet("/ingredientes", (CatalogoIngredientes catalogo, string? q, string? categoria, int? max) =>
    Results.Ok(catalogo.Buscar(q, string.IsNullOrWhiteSpace(categoria) ? null : categoria, Math.Clamp(max ?? 60, 1, 200)).Select(IngredienteDto.De)));

api.MapGet("/ingredientes/{id}", (string id, CatalogoIngredientes catalogo) =>
    catalogo.PorId(id) is { } ing
        ? Results.Ok(IngredienteDto.De(ing))
        : Results.Problem(statusCode: 404, title: "No lo conozco", detail: "Ese ingrediente no está en la base."));

api.MapGet("/categorias", (CatalogoIngredientes catalogo) => Results.Ok(catalogo.Categorias()));

app.MapFallback(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status404NotFound;

    if (contexto.Request.Path.StartsWithSegments("/api"))
    {
        await contexto.Response.WriteAsJsonAsync(new ProblemDetails { Status = 404, Title = "No existe" },
            options: null, contentType: "application/problem+json");
        return;
    }

    contexto.Response.ContentType = "text/html; charset=utf-8";
    await contexto.Response.SendFileAsync(app.Environment.WebRootFileProvider.GetFileInfo("404.html"));
});

app.Run();

static IResult Pedido(string detalle) =>
    Results.Problem(statusCode: 400, title: "No puedo revisar esto", detail: detalle);
