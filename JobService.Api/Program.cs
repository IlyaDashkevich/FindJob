var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<DataSeeder>();
builder.Services.AddAutoMapper(typeof(JobProfile),typeof(EmployerProfile),typeof(ApplicantProfile), typeof(ResumeProfile));
builder.Services.AddScoped <IEmployerService, EmployerService>(); 
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IJobService, JobService.Infrastructure.Services.JobService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"], 
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:63343")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployerOnly", policy => policy.RequireRole("Employer"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Job Service API", Version = "v1" });

    // Определение схемы безопасности
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите токен, начиная с 'Bearer '",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // Требование безопасности
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
            new string[] {}
        }
    });
});
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString
        ("DefaultConnection"));
});

var app = builder.Build();

const string filePathUploads = "C:\\bguir/JobService\\JobService.Core\\file";
if (!Directory.Exists(filePathUploads))
    Directory.CreateDirectory(filePathUploads);

var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

using (var scope = scopedFactory.CreateScope())
{
    var service = scope.ServiceProvider.GetService<DataSeeder>();
    service.SeedDataContext();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
