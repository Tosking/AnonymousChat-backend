﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TRPO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
                      builder => builder
                      .WithOrigins("http://localhost:3000", "http://172.19.0.2:3000", "172.19.0.2:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Удалены строки использования HTTPS перенаправления и статических файлов
// app.UseHttpsRedirection();
// app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.Run();
