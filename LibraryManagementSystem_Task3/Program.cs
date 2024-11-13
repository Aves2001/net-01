var builder = WebApplication.CreateBuilder(args);

// Додаємо служби контролерів
builder.Services.AddControllers();

// Додаємо Swagger у сервіси
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Налаштування Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
        c.RoutePrefix = string.Empty; // Щоб Swagger відкривався на головній сторінці
    });
}

app.UseRouting();

//app.UseAuthorization();   

app.MapControllers();

app.Run();
