using StatefulDependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDataProcessingService, DataProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/sockets/{socketId:int}/subscribe",
        (int socketId, IDataProcessingService service) => service.StartProcessing(socketId))
    .WithName("SubscribeToSocketWithId")
    .WithOpenApi();

app.MapPost("/sockets/{socketId:int}/unsubscribe",
        (int socketId, IDataProcessingService service) => service.StopProcessing(socketId))
    .WithName("UnsubscribeFromSocketWithId")
    .WithOpenApi();

app.Run();