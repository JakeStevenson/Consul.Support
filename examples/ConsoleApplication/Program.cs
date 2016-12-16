using System;
using Configuration.Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var consulIP = Environment.GetEnvironmentVariable("CONSUL_IP");
            var logging = new LoggerFactory()
                .AddConsole();


            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json").Build();
            builder.AddConsulAgent(consulIP, logging);
            var config = builder.Build();

            var section = config.GetSection("SampleConfiguration");
            var test1 = section["Test1"];
            var test2 = section["Test2"];

            Console.WriteLine(test1);
            Console.WriteLine(test2);
            Console.ReadLine();
        }
    }
}
