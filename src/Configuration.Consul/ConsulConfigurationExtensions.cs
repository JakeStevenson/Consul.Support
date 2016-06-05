using System;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Configuration.Consul
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsulAgent(this IConfigurationBuilder builder, string IPAddress = "0.0.0.0")
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(new ConsulConfigurationSource(IPAddress));
            return builder;
        }

    }
}
