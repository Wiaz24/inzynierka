using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PowerPredictor;
using PowerPredictor.Data;
using PowerPredictor.Models;
using PowerPredictor.Services;
using PowerPredictor.Services.Interfaces;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new NullReferenceException("No connection string found in config file");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ILoadService, LoadService>();
builder.Services.AddBlazorBootstrap();

var predictSettings = builder.Configuration.GetSection("ONNXSettings")
    ?? throw new NullReferenceException("No Predictor service settings found in config file");
builder.Services.Configure<PredictServiceConfiguration>(predictSettings);
builder.Services.AddScoped<IPredictService, PredictService>();

var emailSettings = builder.Configuration.GetSection("EmailSettings")
    ?? throw new NullReferenceException("No email settings found in config file");
builder.Services.Configure<EmailServiceConfiguration>(emailSettings);
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

//Entity framework core service
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1); // Ustaw czas wa¿noœci na 1 godzinê
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "StdUser", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    var adminSettings = builder.Configuration.GetSection("AdminSettings")
        ?? throw new NullReferenceException("No admin settings found in config file");

    var userManager = services.GetRequiredService<UserManager<User>>();
    var admin = await userManager.FindByEmailAsync(adminSettings["Email"]);
    if (admin == null)
    {
        admin = new User
        {
            UserName = adminSettings["Email"] ?? throw new NullReferenceException("Invalid admin email adress in config file"),
            Email = adminSettings["Email"] ?? throw new NullReferenceException("Invalid admin email adress in config file"),
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, adminSettings["Password"] ?? throw new NullReferenceException("Invalid admin password in config file"));
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    try
    {
        // Get an instance of your context class and run any pending migrations
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log any exceptions that occur during startup
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LoginMiddleware>();
app.UseMiddleware<LogoutMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
