using System;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Configuration.Consul
{
    [DataContract]
    internal class ConsulKV
    {
        [DataMember]
        public int CreateIndex { get; set; }
        [DataMember]
        public int ModifyIndex { get; set; }
        [DataMember]
        public int LockIndex { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public int Flags { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Session { get; set; }


        public string ConfigurationKey => Key.Replace("/", ConfigurationPath.KeyDelimiter);

        public string StringValue
        {
            get
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Value));
            }
        }
    }
}
