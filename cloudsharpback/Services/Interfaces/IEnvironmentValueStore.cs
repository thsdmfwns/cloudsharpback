using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IEnvironmentValueStore
{
    string this[RequiredEnvironmentValueKey key] { get; }
}