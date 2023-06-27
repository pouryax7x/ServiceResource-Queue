
using ServiceResource;
using ServiceResource.Business.Queue;
using ServiceResource.Business.SR;
using ServiceResource.Enums;
using ServiceResource.Interfaces;


//ValidateEnumClasses<Service_MethodName>();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunQueues();

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();
