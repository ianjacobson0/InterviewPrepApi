using InterviewPrepApi.Data;
using Microsoft.EntityFrameworkCore;
using InterviewPrepApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InterviewPrepApi.Auth;
using InterviewPrepApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
string key = "EDDCD88B243A8B6A9D7E9577FC5DB";

builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	x.RequireHttpsMetadata = false;
	x.SaveToken = true;
	x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});

builder.Services.AddSingleton<JwtAuthenticationManager>(provider =>
{
	var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
	return new JwtAuthenticationManager(key, scopeFactory);
});

// docker
builder.Services.AddSingleton<CodeRunner>();

// cors
builder.Services.AddCors(o => o.AddPolicy("allow_all", build =>
{
	build.AllowAnyHeader()
	.AllowAnyOrigin()
	.AllowAnyMethod();
}));

// Build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("allow_all");

app.MapControllers();

app.Run();
