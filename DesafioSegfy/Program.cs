using DesafioSegfy.Api.Middleware;
using DesafioSegfy.Domain.Service;
using DesafioSegfy.Infra;
using DesafioSegfy.Infra.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SegfyDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
                  ?? "Data Source=seguros.db"));


builder.Services.AddScoped<IApoliceRepository, ApoliceRepository>();
builder.Services.AddScoped<ApoliceService>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensagem = string.Join(" ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? "Requisição inválida."
                : e.ErrorMessage));

        return new BadRequestObjectResult(new { erro = mensagem });
    };
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SegfyDbContext>();
    context.Database.Migrate();
};

app.UseStaticFiles();

// Swagger continua disponível em /swagger; a raiz "/" serve o front (Razor Page).
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrosMiddleware>();
app.UseHttpsRedirection();

app.MapControllers();
app.MapRazorPages();

app.Run();