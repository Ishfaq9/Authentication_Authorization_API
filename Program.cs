using apiTest.Data.ApplicationModel;
using apiTest.Data.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.


//db context
IServiceCollection serviceCollection = builder.Services.AddDbContext<UserDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//identity 
// For Identity
builder.Services.AddIdentity<ApplicationUser ,IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidAudience = configuration["JWT:ValidAudience"]! /*"http://localhost:4200"*/,
        ValidIssuer = configuration["JWT:ValidIssuer"]! /*"http://localhost:44324"*/,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))//"JWTAuthenticationHIGHsecuredPasswordIsh1234271"))
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
        });
});

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

if (app.Environment.IsDevelopment())
{
    // Enable Swagger in development mode
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        c.RoutePrefix = string.Empty; // Serve the Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.SetIsOriginAllowed(origin => true);
    //options.WithOrigins("http://localhost:4200");
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowCredentials();
});

app.UseAuthorization();
app.UseAuthorization();




app.MapControllers();

app.Run();
