using System;
using Configuration.Consul;
using Microsoft.Extensions.Configuration;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json").Build();
            builder.AddConsulAgent("localhost");

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
