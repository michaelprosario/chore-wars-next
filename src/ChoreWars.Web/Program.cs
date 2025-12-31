using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Services;
using ChoreWars.Infrastructure.Data;
using ChoreWars.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=chorewars.db"));

// Repository Registration
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IRepository<QuestCompletion>, Repository<QuestCompletion>>();
builder.Services.AddScoped<IRepository<Party>, Repository<Party>>();

// Service Registration
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IProgressionService, ProgressionService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
