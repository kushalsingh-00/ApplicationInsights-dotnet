﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Endpoints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.ApplicationInsights.Common.Extensions;

    public class EndpointThing
    {
        private string connectionString;
        private Dictionary<string, string> connectionStringParsed;

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
            set
            {
                this.connectionString = value;
                this.connectionStringParsed = ParseConnectionString(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>key1=value1;key2=value2;key3=value3</remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if there are duplicate keys.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if the input string is in the wrong format.</exception>
        internal static Dictionary<string, string> ParseConnectionString(string value)
        {
            return value
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(split => split[0], split => split[1]);
        }

        public string InstrumentationKey { get; set; }

        public Uri GetEndpoint(EndpointName endpointName)
        {
            // todo
            // 1. check for explicit endpoint
            //    1a. then check for location
            // 2. check for endpoint suffix
            //    2a. then check for location
            // 3. use classic endpoint

            var endpointMeta = endpointName.GetAttribute<EndpointMetaAttribute>();

            if (this.connectionStringParsed.TryGetValue(endpointMeta.ExplicitName, out string endpoint))
            {
                var builder = new EndpointBuilder
                {
                    Location = GetLocation(),
                    Host = endpoint,
                };

                return builder.ToUri();
            }
            else if (this.connectionStringParsed.TryGetValue("EndpointSuffix", out string endpointSuffix))
            {
                var builder = new EndpointBuilder
                {
                    Location = GetLocation(),
                    Prefix = endpointMeta.EndpointPrefix,
                    Host = endpointSuffix,
                };

                return builder.ToUri();
            }
            else
            {
                return new Uri(endpointMeta.Default);
            }
        }

        private string GetLocation() => this.connectionStringParsed.TryGetValue("Location", out string location) ? location : null;
    }
}
