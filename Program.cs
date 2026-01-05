using MyMesSystem_B.ModelServices;
using MyMesSystem_B.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ProductModelService>();
builder.Services.AddScoped<ProductService>();

// 在 var builder = WebApplication.CreateBuilder(args); 之後加入
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();// 在這行之後，Services 就不能再修改了
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
//app.UseStatusCodePages();
app.UseHttpsRedirection();

// 在 app.UseHttpsRedirection(); 之後加入
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
