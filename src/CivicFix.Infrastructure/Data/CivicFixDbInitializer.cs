using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CivicFix.Infrastructure.Data;

public class CivicFixDbInitializer
{
    public static async Task SeedAsync(CivicFixDbContext context, ILogger logger)
    {
        try
        {
            // Apply pending migrations or ensure database is created
            await context.Database.EnsureCreatedAsync();

            if (await context.Departments.AnyAsync())
            {
                logger.LogInformation("Database already seeded with departments.");
                return;
            }

            logger.LogInformation("Starting database seeding with Sonipat municipal departments, categories, and initial users...");

            // 1. Seed Departments
            var waterDept = new Department
            {
                Name = "Water Supply & Sewerage Department",
                Code = "WATER",
                Description = "Handles municipal pipeline leakages, drinking water supply, contamination, and low pressure.",
                ContactEmail = "water@sonipat.gov.in",
                ContactPhone = "+91-130-2220101",
                HeadOfficerName = "Er. Rajesh Malik"
            };

            var roadsDept = new Department
            {
                Name = "Public Works & Road Infrastructure Department",
                Code = "ROADS",
                Description = "Maintains city roads, pothole repairs, street dividers, footpaths, and signage.",
                ContactEmail = "pwd@sonipat.gov.in",
                ContactPhone = "+91-130-2220102",
                HeadOfficerName = "Er. Sunil Hooda"
            };

            var drainageDept = new Department
            {
                Name = "Drainage & Stormwater Department",
                Code = "DRAINAGE",
                Description = "Manages open drains, waterlogging prevention, manhole covers, and stormwater discharge.",
                ContactEmail = "drainage@sonipat.gov.in",
                ContactPhone = "+91-130-2220103",
                HeadOfficerName = "Er. Amit Chahal"
            };

            var sanitationDept = new Department
            {
                Name = "Solid Waste Management & Sanitation Department",
                Code = "GARBAGE",
                Description = "Manages community garbage collection, illegal dumping, public bin clearance, and sweeping.",
                ContactEmail = "sanitation@sonipat.gov.in",
                ContactPhone = "+91-130-2220104",
                HeadOfficerName = "Dr. Manju Sharma"
            };

            var streetlightDept = new Department
            {
                Name = "Streetlight & Illumination Department",
                Code = "STREETLIGHT",
                Description = "Responsible for street pole lighting, LED bulb replacements, and dark-spot resolution.",
                ContactEmail = "streetlights@sonipat.gov.in",
                ContactPhone = "+91-130-2220105",
                HeadOfficerName = "Er. Naveen Dahiya"
            };

            var electricalDept = new Department
            {
                Name = "Electricity Safety & Power Department",
                Code = "ELECTRICITY",
                Description = "Addresses exposed live wires, transformer sparking, hanging electrical cables, and power safety.",
                ContactEmail = "electricity@sonipat.gov.in",
                ContactPhone = "+91-130-2220106",
                HeadOfficerName = "Er. Rakesh Vats"
            };

            var horticultureDept = new Department
            {
                Name = "Horticulture & Urban Forestry Department",
                Code = "TREES",
                Description = "Manages fallen trees, dangerous overhangs, public park tree maintenance, and pruning.",
                ContactEmail = "horticulture@sonipat.gov.in",
                ContactPhone = "+91-130-2220107",
                HeadOfficerName = "Sh. Arvind Rohilla"
            };

            var publicPropertyDept = new Department
            {
                Name = "Municipal Assets & Public Property Department",
                Code = "PUBLIC_PROPERTY",
                Description = "Maintains public parks, community halls, bus stands, municipal fencing, and monuments.",
                ContactEmail = "assets@sonipat.gov.in",
                ContactPhone = "+91-130-2220108",
                HeadOfficerName = "Sh. Dinesh Batra"
            };

            var animalDept = new Department
            {
                Name = "Animal Control & Veterinary Welfare Department",
                Code = "ANIMAL",
                Description = "Handles stray cattle rescue, dog control, carcass removal, and rabies vaccinations.",
                ContactEmail = "animalcare@sonipat.gov.in",
                ContactPhone = "+91-130-2220109",
                HeadOfficerName = "Dr. Sandeep Nain"
            };

            var constructionDept = new Department
            {
                Name = "Building & Construction Safety Department",
                Code = "CONSTRUCTION",
                Description = "Supervises unauthorized construction debris, unsafe excavation pits, and building safety.",
                ContactEmail = "construction@sonipat.gov.in",
                ContactPhone = "+91-130-2220110",
                HeadOfficerName = "Er. Kamal Saini"
            };

            var otherDept = new Department
            {
                Name = "General Municipal & Citizen Grievance Cell",
                Code = "OTHER",
                Description = "General grievance redressal for civic issues not explicitly covered by dedicated departments.",
                ContactEmail = "grievances@sonipat.gov.in",
                ContactPhone = "+91-130-2220100",
                HeadOfficerName = "Sh. Anand Kumar"
            };

            var departments = new[]
            {
                waterDept, roadsDept, drainageDept, sanitationDept, streetlightDept,
                electricalDept, horticultureDept, publicPropertyDept, animalDept, constructionDept, otherDept
            };

            await context.Departments.AddRangeAsync(departments);

            // 2. Seed Categories
            var categories = new List<Category>
            {
                // Water
                new() { Name = "Main Pipeline Leakage", Code = "WATER_PIPE_LEAK", PrimaryCategoryGroup = "WATER", Department = waterDept, DefaultSlaHours = 24 },
                new() { Name = "Contaminated Water Supply", Code = "WATER_CONTAMINATION", PrimaryCategoryGroup = "WATER", Department = waterDept, DefaultSlaHours = 12 },
                new() { Name = "Low Water Pressure / No Supply", Code = "WATER_NO_SUPPLY", PrimaryCategoryGroup = "WATER", Department = waterDept, DefaultSlaHours = 24 },

                // Roads
                new() { Name = "Potholes & Broken Road Surface", Code = "ROAD_POTHOLE", PrimaryCategoryGroup = "ROADS", Department = roadsDept, DefaultSlaHours = 48 },
                new() { Name = "Damaged Road Divider / Kerb", Code = "ROAD_DIVIDER_DAMAGED", PrimaryCategoryGroup = "ROADS", Department = roadsDept, DefaultSlaHours = 72 },
                new() { Name = "Missing or Damaged Signage", Code = "ROAD_SIGNAGE_DAMAGED", PrimaryCategoryGroup = "ROADS", Department = roadsDept, DefaultSlaHours = 96 },

                // Drainage
                new() { Name = "Open / Missing Manhole Cover", Code = "DRAIN_OPEN_MANHOLE", PrimaryCategoryGroup = "DRAINAGE", Department = drainageDept, DefaultSlaHours = 6 },
                new() { Name = "Blocked Drain Overflowing", Code = "DRAIN_BLOCKED_OVERFLOW", PrimaryCategoryGroup = "DRAINAGE", Department = drainageDept, DefaultSlaHours = 24 },
                new() { Name = "Severe Monsoon Waterlogging", Code = "DRAIN_WATERLOGGING", PrimaryCategoryGroup = "DRAINAGE", Department = drainageDept, DefaultSlaHours = 12 },

                // Garbage
                new() { Name = "Overflowing Public Dustbin", Code = "GARBAGE_OVERFLOW_BIN", PrimaryCategoryGroup = "GARBAGE", Department = sanitationDept, DefaultSlaHours = 12 },
                new() { Name = "Illegal Garbage Dump on Vacant Plot", Code = "GARBAGE_ILLEGAL_DUMP", PrimaryCategoryGroup = "GARBAGE", Department = sanitationDept, DefaultSlaHours = 36 },
                new() { Name = "Dead Animal Carcass Removal", Code = "GARBAGE_DEAD_ANIMAL", PrimaryCategoryGroup = "GARBAGE", Department = sanitationDept, DefaultSlaHours = 6 },

                // Streetlights
                new() { Name = "Streetlight Not Working", Code = "STREETLIGHT_OUTAGE", PrimaryCategoryGroup = "STREETLIGHT", Department = streetlightDept, DefaultSlaHours = 24 },
                new() { Name = "Continuous Day Burning Light", Code = "STREETLIGHT_DAY_BURNING", PrimaryCategoryGroup = "STREETLIGHT", Department = streetlightDept, DefaultSlaHours = 48 },

                // Electricity
                new() { Name = "Hanging / Broken Live Wire Hazard", Code = "ELECTRICITY_LIVE_WIRE", PrimaryCategoryGroup = "ELECTRICITY", Department = electricalDept, DefaultSlaHours = 4 },
                new() { Name = "Sparking Transformer", Code = "ELECTRICITY_TRANSFORMER_SPARK", PrimaryCategoryGroup = "ELECTRICITY", Department = electricalDept, DefaultSlaHours = 4 },

                // Trees
                new() { Name = "Fallen Tree Blocking Road", Code = "TREE_FALLEN_BLOCKING", PrimaryCategoryGroup = "TREES", Department = horticultureDept, DefaultSlaHours = 6 },
                new() { Name = "Dangerous Overhanging Branches", Code = "TREE_DANGEROUS_BRANCH", PrimaryCategoryGroup = "TREES", Department = horticultureDept, DefaultSlaHours = 48 },

                // Animals
                new() { Name = "Aggressive Stray Dogs / Pack", Code = "ANIMAL_STRAY_DOGS", PrimaryCategoryGroup = "ANIMAL", Department = animalDept, DefaultSlaHours = 24 },
                new() { Name = "Stray Cattle Traffic Obstruction", Code = "ANIMAL_STRAY_CATTLE", PrimaryCategoryGroup = "ANIMAL", Department = animalDept, DefaultSlaHours = 12 },

                // Public Property & Construction
                new() { Name = "Damaged Public Park Infrastructure", Code = "PROPERTY_PARK_DAMAGE", PrimaryCategoryGroup = "PUBLIC_PROPERTY", Department = publicPropertyDept, DefaultSlaHours = 72 },
                new() { Name = "Unsafe Open Construction Pit / Debris", Code = "CONSTRUCTION_UNSAFE_PIT", PrimaryCategoryGroup = "CONSTRUCTION", Department = constructionDept, DefaultSlaHours = 24 },
                new() { Name = "Other Civic Grievance", Code = "OTHER_GENERAL", PrimaryCategoryGroup = "OTHER", Department = otherDept, DefaultSlaHours = 72 }
            };

            await context.Categories.AddRangeAsync(categories);

            // 3. Seed Standard Seed Users (Default password: Password123!)
            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            var adminUser = new User
            {
                FullName = "Sonipat Municipal Admin",
                Email = "admin@sonipat.civicfix.gov.in",
                PhoneNumber = "+91-9876500001",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.Admin,
                IsActive = true
            };

            var waterOfficer = new User
            {
                FullName = "Er. Rajesh Malik (Officer)",
                Email = "water.officer@sonipat.civicfix.gov.in",
                PhoneNumber = "+91-9876500002",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.DepartmentOfficer,
                Department = waterDept,
                IsActive = true
            };

            var roadsOfficer = new User
            {
                FullName = "Er. Sunil Hooda (Officer)",
                Email = "roads.officer@sonipat.civicfix.gov.in",
                PhoneNumber = "+91-9876500003",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.DepartmentOfficer,
                Department = roadsDept,
                IsActive = true
            };

            var sanitationOfficer = new User
            {
                FullName = "Dr. Manju Sharma (Officer)",
                Email = "sanitation.officer@sonipat.civicfix.gov.in",
                PhoneNumber = "+91-9876500004",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.DepartmentOfficer,
                Department = sanitationDept,
                IsActive = true
            };

            var waterWorker = new User
            {
                FullName = "Ramesh Kumar (Plumber)",
                Email = "ramesh.kumar@worker.civicfix.gov.in",
                PhoneNumber = "+91-9876500010",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.FieldWorker,
                Department = waterDept,
                IsActive = true,
                WorkerProfile = new WorkerProfile
                {
                    Specialization = "Pipeline & Valve Repair",
                    AssignedWardOrZone = "Sector 14 & Model Town",
                    ActiveJobsCount = 0,
                    MaxCapacity = 5,
                    IsAvailable = true,
                    Rating = 4.9,
                    TotalCompletedJobs = 42
                }
            };

            var roadWorker = new User
            {
                FullName = "Suresh Sharma (Road Crew)",
                Email = "suresh.sharma@worker.civicfix.gov.in",
                PhoneNumber = "+91-9876500011",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.FieldWorker,
                Department = roadsDept,
                IsActive = true,
                WorkerProfile = new WorkerProfile
                {
                    Specialization = "Asphalt & Pothole Patching",
                    AssignedWardOrZone = "Murthal Road & Sector 15",
                    ActiveJobsCount = 0,
                    MaxCapacity = 5,
                    IsAvailable = true,
                    Rating = 4.8,
                    TotalCompletedJobs = 38
                }
            };

            var citizenVikram = new User
            {
                FullName = "Vikram Singh",
                Email = "vikram.singh@gmail.com",
                PhoneNumber = "+91-9812345678",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.Citizen,
                IsActive = true
            };

            var citizenPriya = new User
            {
                FullName = "Priya Verma",
                Email = "priya.verma@gmail.com",
                PhoneNumber = "+91-9823456789",
                PasswordHash = defaultPasswordHash,
                Role = UserRole.Citizen,
                IsActive = true
            };

            await context.Users.AddRangeAsync(adminUser, waterOfficer, roadsOfficer, sanitationOfficer, waterWorker, roadWorker, citizenVikram, citizenPriya);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully with initial Sonipat data!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
