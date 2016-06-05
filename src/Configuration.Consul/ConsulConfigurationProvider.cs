using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Configuration.Consul
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class ConsulConfigurationProvider : IConfigurationProvider
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public bool TryGet(string key, out string value)
        {
            if (_values.ContainsKey(key))
            {
                value = _values[key];
                return true;
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
                var result = client.PutAsync("http://localhost:8500/v1/kv/" + key, new StringContent(value)).Result;
            }
        }

        public IChangeToken GetReloadToken()
        {
            return new CancellationChangeToken(new CancellationToken());
        }

        public void Load()
        {
            //Here we want to actually load up all the information available from consul
            //  and cache it in a dictionary for quicker use
            using (var client = new HttpClient())
            {
                try
                {
                    var result = client.GetAsync("http://localhost:8500/v1/kv/?recurse").Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var json = result.Content.ReadAsStringAsync().Result;
                        var items = GetValues(json);
                        if (items != null)
                        {
                            _values = items.ToDictionary(x => x.ConfigurationKey, x => x.StringValue);
                        }
                    }
                }
                catch
                {
                    return;
                }

            }
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
