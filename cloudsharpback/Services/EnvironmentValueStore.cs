using System.Text.RegularExpressions;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class EnvironmentValueStore : IEnvironmentValueStore
{
    private List<RequiredEnvironmentValue> _requiredEnvironmentValues;
    private Dictionary<string, string>? _environmentValues = null;

    public EnvironmentValueStore(IConfiguration configuration)
    {
        _requiredEnvironmentValues = configuration.GetSection("EnvironmentValues").Get<List<RequiredEnvironmentValue>>()!;
    }

    public string GeValueByKey(string key)
    {
        if (_environmentValues is null)
        {
            SetValues();
        }
        if (!_environmentValues!.ContainsKey(key))
        {
            throw new ArgumentException();
        }
        return _environmentValues[key];
    }
    public bool CheckValue(RequiredEnvironmentValue required, out string? errorMessage, out string value)
    {
        var val = Environment.GetEnvironmentVariable(required.Key);
        errorMessage = string.Empty;
        value = string.Empty;
        if (string.IsNullOrEmpty(val))
        {
            if (!string.IsNullOrEmpty(required.DefaultValue))
            {
                errorMessage = $"Environment Value [{required.Key}] is null. Using default value : {required.DefaultValue}";
                value = required.DefaultValue;
                return true;
            }
            errorMessage = $"Environment Value [{required.Key}] is required";
            return false;
        }
        if (!string.IsNullOrEmpty(required.Pattern) &&
            !Regex.IsMatch(val, required.Pattern))
        {
            errorMessage = $"Environment Value [{required.Key}] has wrong value";
            return false;
        }
        value = val;
        return true;
    }

    public bool CheckValues()
    {
        Console.WriteLine("check environment values..");
        _environmentValues = new Dictionary<string, string>();
        var success = true;
        foreach (var required in _requiredEnvironmentValues)
        {
            success = CheckValue(required, out var errorMessage, out var value);
            if (errorMessage is not null)
            {
                Console.Error.WriteLine(errorMessage);               
            }
            if (success)
            {
                _environmentValues.Add(required.Key, value);
            }
        }
        return success;
    }

    private void SetValues()
    {
        if (!CheckValues())
        {
            throw new ApplicationException("there are some problems about environment values");
        }
    }
    
}