using DesafioSegfy.Domain.Service;
using DesafioSegfy.Infra;
using DesafioSegfy.Infra.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SegfyDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")
                  ?? "Data Source=seguros.db"));


builder.Services.AddScoped<IApoliceRepository, ApoliceRepository>();
builder.Services.AddScoped<ApoliceService>();
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) 
{
    var context = scope.ServiceProvider.GetRequiredService<SegfyDbContext>();
    context.Database.Migrate();
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();