using Microsoft.EntityFrameworkCore;
using RealEstateClassificator.Application.Components;
using RealEstateClassificator.Core.Profiles;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Core.Services;
using RealEstateClassificator.Dal.Interfaces;
using RealEstateClassificator.Dal.Repository;
using RealEstateClassificator.Dal.Services;
using RealEstateClassificator.Dal;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAutoMapper(typeof(CardProfile));
builder.Services.AddScoped<IPageParserService, PageParserService>();
builder.Services.AddScoped<ICardParserService, CardParserService>();
builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddDbContext<RealEstateClassificatorContext>(x => x.UseNpgsql("Host=localhost;Port=5432;Database=RealEstateClassificator;Username=postgres;Password=123"));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); 

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5127") });
builder.Services.AddRadzenComponents();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
