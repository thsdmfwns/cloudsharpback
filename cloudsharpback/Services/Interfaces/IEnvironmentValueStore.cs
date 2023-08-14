using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IEnvironmentValueStore
{
    string GeValueByKey(string key);
    bool CheckValue(RequiredEnvironmentValue required, out string? errorMessage, out string value);
    bool CheckValues();
}