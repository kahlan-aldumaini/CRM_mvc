using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Repositories;
using CRM_mvc.Models.Repositories.Interfaces;
using CRM_mvc.Services;
using CRM_mvc.Utilities.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Session
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    // if (connectionString != null) options.UseSqlServer(connectionString);
    if (connectionString != null) options.UseMySQL(connectionString);
});
// builder.Services.AddDbContext<ApplicationDbContext>(options => 
//    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequiredLength = 5;

    // sign in options
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

// model scoped
builder.Services.AddScoped<ICallRepository, CallRepository>();

// add upload file service
builder.Services.AddScoped<UploadFile, LocalFileUploadService>();

// fix JsonException: A possible object cycle was detected. This can either be due to a cycle or if the object depth is larger than the maximum allowed depth of 32.
// first: install package: Microsoft.AspNetCore.Mvc.NewtonsoftJson
// second: add this code
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add signalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();


app.MapDefaultControllerRoute();

app.MapHub<CallHub>("/hasCall");
app.MapHub<ChartReportsHub>("/chart-report");
app.MapHub<ChatHub>("/chat");

app.Run();