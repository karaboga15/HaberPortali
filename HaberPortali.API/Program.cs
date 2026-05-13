using HaberPortali.API.Models;
using HaberPortali.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Program.cs içindeki AddIdentity satýrlarýný þununla deðiþtir:
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Þifre kurallarýný tamamen esnetiyoruz ki kayýt olurken hata vermesin!
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// KESÝN ÇÖZÜM 1: Identity'nin seni saçma sapan 404 sayfalarýna yönlendirmesini engelliyoruz.
// Bunun yerine dürüstçe 401 ve 403 HTTP kodlarýný döndürecek.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// KESÝN ÇÖZÜM 2: JWT ayarlarýna Rol Okuma özelliðini ekliyoruz.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        // TOKEN ÝÇÝNDEKÝ ROLÜ OKUYABÝLMESÝ ÝÇÝN ÞART OLAN SATIR:
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddCors(o => o.AddPolicy("AllowMVC",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT auth support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Haber Portali API", Version = "v1" });

    // Define the Bearer auth scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT tokeninizi girin.\n\nOrnek: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    // Make every endpoint require the Bearer token by default
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    foreach (var role in new[] { "Admin", "User" })
        if (!await rm.RoleExistsAsync(role))
            await rm.CreateAsync(new IdentityRole(role));
    if (await um.FindByNameAsync("admin") == null)
    {
        var admin = new AppUser { UserName = "admin", FirstName = "Admin", LastName = "Admin", Email = "admin@haberportali.com" };
        await um.CreateAsync(admin, "Admin123!");
        await um.AddToRoleAsync(admin, "Admin");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowMVC");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();