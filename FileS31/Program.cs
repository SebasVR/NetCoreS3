using Amazon.S3;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DynamoDb.Libs.DynamoDb;
using Amazon.DynamoDBv2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSService<IAmazonS3>();

var Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

builder.Services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", Configuration["AWS:AccessKey"]);
Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", Configuration["AWS:SecretKey"]);
Environment.SetEnvironmentVariable("AWS_REGION", Configuration["AWS:Region"]);

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<ICreateTable, CreateTable>();
builder.Services.AddSingleton<IPutItem, PutItem>();
builder.Services.AddSingleton<IGetItem, GetItem>();
builder.Services.AddSingleton<IUpdateItem, UpdateItem>();
builder.Services.AddSingleton<IDeleteTable, DeleteTable>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
