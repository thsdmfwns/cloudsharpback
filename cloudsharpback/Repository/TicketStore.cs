using cloudsharpback.Models.Ticket;
using cloudsharpback.Services.Interfaces;
using NRedisStack.RedisStackCommands;
using cloudsharpback.Repository.Interface;

namespace cloudsharpback.Repository;

public class TicketStore : ITicketStore
{
    
    //todo add Redis
    private readonly IDBConnectionFactory _dbConnectionFactory;

    public TicketStore(IDBConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    private async ValueTask<bool> Add(string? key, Guid token, object obj, TimeSpan? timeSpan = null)
    {
        var res = await _dbConnectionFactory.Redis.JSON().SetAsync($"{key}:{token}", "$", obj);
        if (timeSpan is not null)
        {
            _dbConnectionFactory.Redis.KeyExpire($"{key}:{token}", timeSpan);
        }

        return res;
    }

    private async ValueTask<string?> GetAsync(string? key, Guid ticketToken)
    {
        var result = await _dbConnectionFactory.Redis.JSON().GetAsync($"{key}:{ticketToken}");
        return result.ToString();
    }
    
    private async ValueTask<bool> Remove(string? key, Guid ticketToken)
    {
        return await _dbConnectionFactory.Redis.JSON().DelAsync($"{key}:{ticketToken}") > 0;
    }

    private async ValueTask<bool> Exist(string key, Guid token)
    {
        return await _dbConnectionFactory.Redis.KeyExistsAsync($"{key}:{token}");
    }
    
    private async ValueTask<bool> SetExpire(string key, Guid token, TimeSpan timeSpan)
    {
        return await _dbConnectionFactory.Redis.KeyExpireAsync($"{key}:{token}", timeSpan);
    }



    public async ValueTask<bool> AddTicket<T>(ITicket<T> ticket) where T : ITicket<T>
        => await Add(T.RedisKey, ticket.Token, ticket, ticket.ExpireTime);


    public async ValueTask<T?> GetTicket<T>(Guid ticketToken) where T : ITicket<T>
        => T.FromJson(await GetAsync(T.RedisKey, ticketToken));


    public async ValueTask<bool> RemoveTicket<T>(ITicket<T> ticket) where T : ITicket<T>
        => await Remove(T.RedisKey, ticket.Token);
    
    
    public async ValueTask<bool> ExistTicket<T>(Guid ticketToken) where T : ITicket<T>
        => await Exist(T.RedisKey, ticketToken);
    public async ValueTask<bool> SetTicketExpire<T>(Guid ticketToken, TimeSpan timeSpan) where T : ITicket<T>
        => await SetExpire(T.RedisKey, ticketToken, timeSpan);
}