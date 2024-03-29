using System.Collections;
using Bogus;
using cloudsharpback.Models.DTO.Member;
using Newtonsoft.Json;

namespace cloudsharpback.Test;

public static class Utils
{
    public static int PassCount = 0;
    public static int FailCount = 0;
    public static int ErrorCount = 0;
    
    
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

    public static T GetRandomItem<T>(List<T> rows)
        => rows.ElementAt(Random.Shared.Next(0, rows.Count - 1));

    public static MemberDto GetFakeMemberDto(Faker faker)
    {
        return new MemberDto()
        {
            Directory = Guid.NewGuid().ToString(),
            Email = faker.Internet.Email(),
            Id = (ulong)faker.UniqueIndex,
            Nickname = faker.Internet.UserName(),
            ProfileImage = null,
            Role = 2
        };
    }

    public static string MakeFakeFile(Faker faker, string memberDir, string? fileDir, string? ext = null, bool fullPath = false)
    {
        fileDir ??= string.Empty;
        var fileName = faker.System.CommonFileName(ext);
        var fileContent = faker.Lorem.Sentences();
        var filePath = Path.Combine(memberDir, fileDir, fileName);
        Directory.CreateDirectory(memberDir);
        File.WriteAllText(filePath, fileContent);
        var path = filePath.Remove(0, memberDir.Length);
        return fullPath ? filePath : path.TrimStart('/');
    }
    
    public static string MakeFakeFileAtDirectory(Faker faker, string dirPath, string? filename = null)
    {
        var fileName = filename ??faker.System.CommonFileName();
        var fileContent = faker.Lorem.Paragraphs();
        Directory.CreateDirectory(dirPath);
        var path = Path.Combine(dirPath, fileName);
        File.WriteAllText(path, fileContent);
        return path;
    }
}