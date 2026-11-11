using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Data;
using Microsoft.AspNetCore.Identity;

//Creates the application builder and loads configuration and services. 
var builder = WebApplication.CreateBuilder(args);

//Registers ApplicationDbContext and configures it to use SQL Server
//with the DefaultConnection string.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
// Registers Razor Pages services for pages such as Identity login and registration.
builder.Services.AddRazorPages();

//Configure ASP.NET Core Identity to manage application users and roles.
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configures where users are redirected when login is required.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

//Builds the application using the registered services and configuration.
var app = builder.Build();

//Create a service scope to manage and initialize application roles.
using (var scope = app.Services.CreateScope())
{
    //Retrieves the role manager used to create and manage user roles
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    //Defines the roles to be created in the application.
    string[] roles = { "Administrator", "PoliceOfficer" };

    //Checks if each role exists and creates it if it does not.
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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
// Configures the default route for controller actions,
// specifying the Home controller and Index action as defaults.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Allow Razor pages routing,
app.MapRazorPages();
// Start the application and listen for incoming HTTP requests.
app.Run();
