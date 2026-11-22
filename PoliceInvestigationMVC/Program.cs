using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using Microsoft.AspNetCore.Identity;

//Creates the application builder and loads configuration and services. 
var builder = WebApplication.CreateBuilder(args);

// Registers the database context and configures the application to use SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Registers MVC controllers, views and Razor Pages services.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configures Identity password requirements and application roles.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password must contain at least 8 characters.
    options.Password.RequiredLength = 8;

    // Require a number, uppercase, lowercase and special character.
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configures the login and access-denied pages for the application.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

//Builds the application using the registered services and configuration.
var app = builder.Build();

// Creates sample roles, users, stations, officers and investigations.
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

// Configures error handling and security settings for the application.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configures HTTPS, static files, routing, authentication and authorization.
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Defines the default route and enables Razor Pages routing..
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Start the application and listen for incoming HTTP requests.
app.Run();
