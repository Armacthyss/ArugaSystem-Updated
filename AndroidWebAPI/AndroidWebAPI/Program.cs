using AndroidWebAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Ports ────────────────────────────────────────────────────
builder.WebHost.UseUrls(
    "http://localhost:57147",
    "http://localhost:57148"
);

// ── Services ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<VaccineRepository>();

builder.Services.AddScoped<ParentRepository>();          // ← only once
builder.Services.AddDbContext<AppDbContext>(options =>   // ← only once
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddHostedService<NotificationGeneratorService>();  // ← only once

// ── Build ─────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware (Optional but recommended) ─────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");
app.UseAuthorization();
app.MapControllers();

// ── Start the Application ─────────────────────────────────────
app.Run(); // <--- THIS IS THE MISSING PIECE