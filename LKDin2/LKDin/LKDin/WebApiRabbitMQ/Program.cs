using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApiRabbitMQ.Service;

namespace WebApiRabbitMQ
{
    public class Program
    {
        private static  MQService mq ;
        public static void Main(string[] args)
        {
            // creamos conexion con RabbitMQ
            mq = new MQService();
            CreateHostBuilder(args).Build().Run(); // bloqueante
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
