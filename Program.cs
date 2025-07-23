// TaskTrackerApi/Program.cs

using TaskTrackerApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaskTrackerApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerApi.Services.Implementations;
using TaskTrackerApi.Services.Interfaces;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
        builder.WithOrigins(
            "http://localhost:3000",
            "https://a51398-tasktracker-client.netlify.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddDbContext<TaskDbContext>(options =>
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    options.UseInMemoryDatabase("TaskTrackerDb"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<TaskDbContext>()
  .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddTransient<TaskPdfExporter>();
builder.Services.AddTransient<UserListPdfExporter>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"];
if (string.IsNullOrEmpty(keyString))
    throw new Exception("JWT Key is not configured.");

var key = Encoding.UTF8.GetBytes(keyString);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminSection = builder.Configuration.GetSection("AdminCredentials");
    var adminEmail = adminSection["Email"];
    var adminUserName = adminSection["UserName"];
    var adminPassword = adminSection["Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) ||
    string.IsNullOrWhiteSpace(adminUserName) ||
    string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new Exception("Admin credentials are not properly configured.");
    }

    var existingAdmin = await userManager.FindByNameAsync(adminUserName);
    if (existingAdmin == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    if (await userManager.FindByNameAsync("demo_user1") == null &&
    await userManager.FindByNameAsync("demo_user2") == null)
    {
        var demoUser1 = new ApplicationUser
        {
            UserName = "demo_user1",
            Email = "demo1@example.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(demoUser1, "YourPassword123!");
        await userManager.AddToRoleAsync(demoUser1, "User");

        var demoUser2 = new ApplicationUser
        {
            UserName = "demo_user2",
            Email = "demo2@example.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(demoUser2, "YourPassword123!");
        await userManager.AddToRoleAsync(demoUser2, "User");

        var now = DateTime.UtcNow;

        var tasks = new List<TaskItem>
    {
        new TaskItem
        {
            Title = "Welcome Task",
            Description = "This is your first task!",
            DueDate = now.AddDays(3),
            IsCompleted = false,
            UserId = demoUser1.Id
        },
        new TaskItem
        {
            Title = "Try completing a task",
            Description = "Mark this one as done to test!",
            DueDate = now.AddDays(5),
            IsCompleted = false,
            UserId = demoUser1.Id
        },
        new TaskItem
        {
            Title = "Second user's task",
            Description = "Task for another demo user",
            DueDate = now.AddDays(1),
            IsCompleted = true,
            UserId = demoUser2.Id
        }
    };

        context.TaskItems.AddRange(tasks);
        await context.SaveChangesAsync();
    }
}

app.Run();
