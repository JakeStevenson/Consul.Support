using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Configuration.Consul
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsulAgent(this IConfigurationBuilder builder, string IPAddress = "0.0.0.0", ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(new ConsulConfigurationSource(IPAddress, loggerFactory));
            return builder;
        }

    }
}
