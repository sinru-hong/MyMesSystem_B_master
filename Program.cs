using MyMesSystem_B.ModelServices;
using MyMesSystem_B.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ProductModelService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UsersModelService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<UploadPathModelService>();
builder.Services.AddScoped<UploadPathService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => {
        policy.WithOrigins("https://localhost:44344")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
