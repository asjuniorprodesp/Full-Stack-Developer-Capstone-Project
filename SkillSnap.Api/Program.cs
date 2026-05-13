using Microsoft.EntityFrameworkCore;
using SkillSnap.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5217",
                "https://localhost:7212")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=skillsnap.db";

builder.Services.AddDbContext<SkillSnapContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");

app.MapControllers();

app.Run();
