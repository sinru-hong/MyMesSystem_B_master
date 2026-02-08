using MyMesSystem_B.ModelServices;
using MyMesSystem_B.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<ProductModelService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UsersModelService>();
builder.Services.AddScoped<UsersService>();
//builder.Services.AddScoped<ProjectsModelService>();
builder.Services.AddScoped<ProjectsService>();
builder.Services.AddScoped<UploadPathModelService>();
builder.Services.AddScoped<UploadPathService>();
// 在 var builder = WebApplication.CreateBuilder(args); 之後加入
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => {
        // 💡 關鍵 1: 當需要使用 Session/Cookie 時，不能用 AllowAnyOrigin
        //policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        // 必須明確指定你的前端網址 (例如 localhost:XXXX)
        policy.WithOrigins("https://localhost:44344") // 這裡填入你前端的 URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // 💡 關鍵 2: 必須允許憑證 (Cookie)
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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
