var builder = WebApplication.CreateBuilder(args);

// ������ ������ ����������
builder.Services.AddControllers();

// ������ Swagger � ������
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ������������ Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
        c.RoutePrefix = string.Empty; // ��� Swagger ���������� �� ������� �������
    });
}

app.UseRouting();

//app.UseAuthorization();   

app.MapControllers();

app.Run();
