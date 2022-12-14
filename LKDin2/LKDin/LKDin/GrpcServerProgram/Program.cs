using GrpcServerProgram.Services;
using GrpcServerProgram.Servidor;
using Microsoft.AspNetCore.Hosting.Server;
using System;



namespace GrpcServerProgram
{
    class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8001, configure => configure.UseHttps());
            });

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