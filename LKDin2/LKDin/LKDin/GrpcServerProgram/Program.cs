using GrpcServerProgram.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Servidor;
using System;



namespace GrpcServerProgram
{
    class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();

            ProgramaServidor server = new ProgramaServidor();
            StartServer(server);

            var app = builder.Build();
            app.MapGrpcService<AdminService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            // Configure the HTTP request pipeline.

            app.Run();
        }

        public static async Task StartServer(ProgramaServidor server)
        {
            Console.WriteLine("Server will start accepting connections from the clients");
            await Task.Run(() => server.ComenzarRecibirConexiones());
        }
    }
}