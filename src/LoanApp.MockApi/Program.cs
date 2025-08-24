using Microsoft.AspNetCore.Mvc;
using LoanApp.MockApi.Services;
using LoanApp.MockApi.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// simple in-memory stores
builder.Services.AddSingleton<InMemoryStore>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();