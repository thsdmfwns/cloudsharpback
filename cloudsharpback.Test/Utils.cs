using System.Collections;
using Newtonsoft.Json;

namespace cloudsharpback.Test;

public static class Utils
{
    public static string ToJson(object obj)
        => JsonConvert.SerializeObject(obj);

    public static ulong GetFailId(IList rows, int max = 100)
        => (ulong)Random.Shared.Next(rows.Count + 1, max);
}