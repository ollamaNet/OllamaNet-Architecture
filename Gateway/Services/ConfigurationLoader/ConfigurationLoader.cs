using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gateway.Services.ConfigurationLoader
{
    public static class ConfigurationLoader
    {
        private static readonly Regex VariablePattern = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);

        public static IServiceCollection AddOcelotWithSplitConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var configBuilder = new ConfigurationBuilder();
            var combinedConfig = LoadAndCombineConfigurations();

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, JsonConvert.SerializeObject(combinedConfig));
            configBuilder.AddJsonFile(tempFile, false, false);

            var ocelotConfiguration = configBuilder.Build();

            services.AddOcelot(ocelotConfiguration); // Do not cast the result
            return services; // Return the original IServiceCollection
        }


        public static JObject LoadAndCombineConfigurations()
        {
            var configDir = Path.Combine(Directory.GetCurrentDirectory(), "Configurations");
            var serviceUrlsPath = Path.Combine(configDir, "ServiceUrls.json");
            var globalConfigPath = Path.Combine(configDir, "Global.json");
            
            // Load service URLs
            var serviceUrls = JObject.Parse(File.ReadAllText(serviceUrlsPath));
            
            // Load global configuration
            var globalConfig = JObject.Parse(File.ReadAllText(globalConfigPath));
            
            // Create the combined configuration
            var combinedConfig = new JObject();
            
            // Add global configuration
            combinedConfig.Merge(globalConfig);
            
            // Load and merge all route configurations
            var routeConfigs = Directory.GetFiles(configDir, "*.json")
                .Where(f => !f.EndsWith("ServiceUrls.json") && !f.EndsWith("Global.json"))
                .Select(f => JObject.Parse(File.ReadAllText(f)))
                .ToList();
            
            // Combine all routes
            var allRoutes = new JArray();
            foreach (var config in routeConfigs)
            {
                if (config["Routes"] is JArray routes)
                {
                    foreach (var route in routes)
                    {
                        // Replace variables in each route
                        ReplaceVariables(route, serviceUrls);
                        allRoutes.Add(route);
                    }
                }
            }
            
            // Add all routes to the combined configuration
            combinedConfig["Routes"] = allRoutes;
            
            // Save the combined configuration to a file for inspection
            var combinedConfigString = JsonConvert.SerializeObject(combinedConfig, Formatting.Indented);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "combined_ocelot_config.json"), combinedConfigString);
            
            return combinedConfig;
        }
        
        private static void ReplaceVariables(JToken token, JObject serviceUrls)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToList())
                {
                    if (property.Value is JValue value && value.Type == JTokenType.String)
                    {
                        var strValue = value.Value<string>();
                        if (strValue != null && strValue.Contains("${"))
                        {
                            property.Value = ReplaceVariableInString(strValue, serviceUrls);
                        }
                    }
                    else
                    {
                        ReplaceVariables(property.Value, serviceUrls);
                    }
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array)
                {
                    ReplaceVariables(item, serviceUrls);
                }
            }
        }
        
        private static JToken ReplaceVariableInString(string value, JObject serviceUrls)
        {
            return VariablePattern.Replace(value, match =>
            {
                var path = match.Groups[1].Value;
                var parts = path.Split('.');
                
                JToken current = serviceUrls;
                foreach (var part in parts)
                {
                    if (current is JObject currentObj)
                    {
                        current = currentObj[part];
                        if (current == null)
                            return match.Value; // Keep the original if not found
                    }
                    else
                    {
                        return match.Value; // Keep the original if path is invalid
                    }
                }
                
                return current.ToString();
            });
        }
    }
} 