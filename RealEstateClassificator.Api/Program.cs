using Microsoft.EntityFrameworkCore;
using RealEstateClassificator.Core.Profiles;
using RealEstateClassificator.Core.Services;
using RealEstateClassificator.Core.Services.Interfaces;
using RealEstateClassificator.Dal;
using RealEstateClassificator.Dal.Interfaces;
using RealEstateClassificator.Dal.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(CardProfile));
builder.Services.AddScoped<IPageParserService, PageParserService>();
builder.Services.AddScoped<ICardParserService, CardParserService>();
builder.Services.AddDbContext<RealEstateClassificatorContext>(x => x.UseNpgsql("Host=localhost;Port=5432;Database=RealEstateClassificator;Username=postgres;Password=123"));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
