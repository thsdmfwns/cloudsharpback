using System.Collections;
using Newtonsoft.Json;

namespace cloudsharpback.Test;

public static class Utils
{
    public static string ToJson(object obj)
    {
        var dic = new SortedDictionary<string, object?>();
        obj.GetType()
            .GetProperties()
            .ToList()
            .ForEach(x => dic.Add(x.Name, x.GetValue(obj)));
        return JsonConvert.SerializeObject(dic, Formatting.Indented);
    }
        

    public static ulong GetFailId(IList rows, int max = 100)
        => (ulong)Random.Shared.Next(rows.Count + 1, max);
}