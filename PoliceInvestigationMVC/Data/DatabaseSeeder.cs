using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PoliceInvestigationMVC.Models;

namespace PoliceInvestigationMVC.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Create roles if they do not already exist.
            string[] roles = { "Administrator", "PoliceOfficer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Create the Administrator login account.
            var adminEmail = "admin@police.local";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    adminUser, "Admin@12345");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }

            // 3. Create sample police stations.
            var stations = new List<PoliceStation>
            {
                new PoliceStation
                {
                    Name = "Mamelodi Police Station",
                    StationCode = "MAM001",
                    Location = "Mamelodi, Pretoria",
                    Province = "Gauteng",
                    ContactNumber = "0128018600",
                    StationCommander = "Captain Sample"
                },
                new PoliceStation
                {
                    Name = "Hatfield Police Station",
                    StationCode = "HAT001",
                    Location = "Hatfield, Pretoria",
                    Province = "Gauteng",
                    ContactNumber = "0123661700",
                    StationCommander = "Captain Example"
                },
                new PoliceStation
                {
                    Name = "Sandton Police Station",
                    StationCode = "SAN001",
                    Location = "Sandton, Johannesburg",
                    Province = "Gauteng",
                    ContactNumber = "0117224200",
                    StationCommander = "Captain Demo"
                }
            };

            foreach (var station in stations)
            {
                bool exists = await context.PoliceStations
                    .AnyAsync(s => s.StationCode == station.StationCode);

                if (!exists)
                {
                    context.PoliceStations.Add(station);
                }
            }

            await context.SaveChangesAsync();

            // 4. Create sample police officers and their login accounts.
            var officerData = new[]
            {
                new { ServiceNumber = "SAP001", Name = "Thabo Mokoena",
                    Rank = "Sergeant", StationCode = "MAM001",
                    Email = "sap001@police.local", Phone = "0711000001" },

                new { ServiceNumber = "SAP002", Name = "Lerato Nkosi",
                    Rank = "Constable", StationCode = "MAM001",
                    Email = "sap002@police.local", Phone = "0711000002" },

                new { ServiceNumber = "SAP003", Name = "Sipho Dlamini",
                    Rank = "Sergeant", StationCode = "HAT001",
                    Email = "sap003@police.local", Phone = "0711000003" },

                new { ServiceNumber = "SAP004", Name = "Naledi Khumalo",
                    Rank = "Constable", StationCode = "HAT001",
                    Email = "sap004@police.local", Phone = "0711000004" },

                new { ServiceNumber = "SAP005", Name = "Pieter Botha",
                    Rank = "Warrant Officer", StationCode = "SAN001",
                    Email = "sap005@police.local", Phone = "0711000005" }
            };

            foreach (var item in officerData)
            {
                var user = await userManager.FindByEmailAsync(item.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(
                        user, "Officer@12345");

                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join("; ",
                            result.Errors.Select(e => e.Description)));
                    }
                }

                if (!await userManager.IsInRoleAsync(user, "PoliceOfficer"))
                {
                    await userManager.AddToRoleAsync(user, "PoliceOfficer");
                }

                var officer = await context.PoliceOfficers
                    .FirstOrDefaultAsync(o =>
                        o.ServiceNumber == item.ServiceNumber);

                if (officer == null)
                {
                    var station = await context.PoliceStations
                        .FirstAsync(s => s.StationCode == item.StationCode);

                    officer = new PoliceOfficer
                    {
                        ServiceNumber = item.ServiceNumber,
                        Name = item.Name,
                        Rank = item.Rank,
                        StationId = station.Id,
                        ContactNumber = item.Phone,
                        Email = item.Email,
                        EmploymentStatus = "Active",
                        ApplicationUserId = user.Id
                    };

                    context.PoliceOfficers.Add(officer);
                }
                else if (string.IsNullOrEmpty(officer.ApplicationUserId))
                {
                    officer.ApplicationUserId = user.Id;
                }
            }

            await context.SaveChangesAsync();

            // 5. Create sample investigations without assigned officers.
            var investigations = new List<Investigation>
            {
                new Investigation
                {
                    CaseNumber = "CAS-2026-001",
                    Complainant = "John Example",
                    CrimeCategory = "House Breaking",
                    Title = "House Breaking - Mamelodi",
                    Description = "Reported house breaking incident.",
                    Status = "Open",
                    CaseType = "Criminal",
                    Priority = "High",
                    DateCreated = DateTime.Now,
                    InvestigatingOfficerId = null
                },
                new Investigation
                {
                    CaseNumber = "CAS-2026-002",
                    Complainant = "Mary Sample",
                    CrimeCategory = "Theft",
                    Title = "Theft of Motor Vehicle",
                    Description = "Reported theft of a motor vehicle.",
                    Status = "Open",
                    CaseType = "Criminal",
                    Priority = "Medium",
                    DateCreated = DateTime.Now,
                    InvestigatingOfficerId = null
                },
                new Investigation
                {
                    CaseNumber = "CAS-2026-003",
                    Complainant = "David Demo",
                    CrimeCategory = "Fraud",
                    Title = "Reported Fraud",
                    Description = "A fraud incident was reported.",
                    Status = "Open",
                    CaseType = "Criminal",
                    Priority = "High",
                    DateCreated = DateTime.Now,
                    InvestigatingOfficerId = null
                },
                new Investigation
                {
                    CaseNumber = "CAS-2026-004",
                    Complainant = "Sarah Test",
                    CrimeCategory = "Assault",
                    Title = "Reported Assault",
                    Description = "An assault incident was reported.",
                    Status = "Open",
                    CaseType = "Criminal",
                    Priority = "Low",
                    DateCreated = DateTime.Now,
                    InvestigatingOfficerId = null
                }
            };

            foreach (var investigation in investigations)
            {
                bool exists = await context.Investigations
                    .AnyAsync(i => i.CaseNumber == investigation.CaseNumber);

                if (!exists)
                {
                    context.Investigations.Add(investigation);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}