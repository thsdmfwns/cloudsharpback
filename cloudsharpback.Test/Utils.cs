using System.Collections;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace cloudsharpback.Test;

public static class Utils
{
    public static int PassCount = 0;
    public static int FailCount = 0;
    
    
    public static string ClassToJson(object obj)
    {
        var dic = new SortedDictionary<string, object?>();
        obj.GetType()
            .GetProperties()
            .ToList()
            .ForEach(x => dic.Add(x.Name, x.GetValue(obj)));
        return JsonConvert.SerializeObject(dic, Formatting.Indented);
    }

    public static string ToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
        

    public static ulong GetFailId(IList rows, int max = 100)
        => (ulong)Random.Shared.Next(rows.Count + 1, max);

    public static IConfiguration GetConfiguration()
    => new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
}