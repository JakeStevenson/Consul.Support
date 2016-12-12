using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Configuration.Consul
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class ConsulConfigurationProvider : IConfigurationProvider
    {
        private readonly string _consulUrl;
        private Dictionary<string, string> _values = new Dictionary<string, string>();
        private readonly ILogger _logger;
	
	//Ugly hack because often the configurationprovider loads BEFORE logging.  We save off connection issues
	//So we can report them again later
        private bool _ableToConnect;
	private Exception _connectionError;

        public ConsulConfigurationProvider(string ip = "", ILoggerFactory loggerFactory = null)
        {
            //If we weren't given a logger by DI, then create a dumb one.
            if (loggerFactory == null)
            {
                loggerFactory = new LoggerFactory();
            }
            _logger = loggerFactory.CreateLogger<ConsulConfigurationProvider>();

            if (string.IsNullOrEmpty(ip))
            {
                ip = "localhost";
            }
            _consulUrl = string.Format("http://{0}:8500", ip);
        }

        public void Load()
        {
            //Here we want to actually load up all the information available from consul
            //  and cache it in a dictionary for quicker use
            _logger.LogInformation("Loading settings from Consul cluster at " + _consulUrl);
            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var result = client.GetAsync(_consulUrl + "/v1/kv/?recurse").Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var items = GetValues(json);
                        if (items != null)
                        {
                            _values = items.ToDictionary(x => x.ConfigurationKey, x => x.StringValue);
                        }
                        _ableToConnect = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Unable to reach consul cluster specified at " + _consulUrl);
		    _connectionError = ex;
                    return;
                }
            }
        }

        public bool TryGet(string key, out string value)
        {
            if (_values.ContainsKey(key))
            {
                value = _values[key];
                return true;
            }
            _logger.LogDebug("Requested key " + key + " not found.");
            if (!_ableToConnect)
            {
                _logger.LogDebug("Never connected to consul agent at " + _consulUrl);
		if(_connectionError!=null){
			_logger.LogDebug(_connectionError.Message);
		}
            }
            value = string.Empty;
            return false;
        }

        public void Set(string key, string value)
        {
            key = key.Replace(':', '/'); 

            _values[key] = value;
            using (var client = new HttpClient())
            {
                var result = client.PutAsync(_consulUrl + "/v1/kv/" + key, new StringContent(value)).Result;
            }
        }

        public IChangeToken GetReloadToken()
        {
            return new CancellationChangeToken(new CancellationToken());
        }


        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {

            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            var ret = _values
                .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(kv => Segment(kv.Key, prefix.Length))
                .Concat(earlierKeys);
            return ret;
        }
        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            var ret =  indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
            return ret;
        }



        private IEnumerable<ConsulKV> GetValues(string json)
        {
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(IEnumerable<ConsulKV>));
                var info = ((IEnumerable<ConsulKV>)serializer.ReadObject(ms));
                return info;
            }
        }
    }
}
