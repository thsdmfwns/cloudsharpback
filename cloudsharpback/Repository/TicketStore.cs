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

    public async ValueTask<bool> TryAdd(string key, Guid token, object obj, TimeSpan? timeSpan = null)
    {
        var res = await _dbConnectionFactory.Redis.JSON().SetAsync($"{key}:{token}", "$", obj);
        if (timeSpan is not null)
        {
            _dbConnectionFactory.Redis.KeyExpire($"{key}:{token}", timeSpan);
        }

        return res;
    }

    public async ValueTask<bool> TryAddDownloadTicketAsync(DownloadTicket downloadTicket)
        => await TryAdd(DownloadTicket.RedisKey, downloadTicket.Token, downloadTicket, TimeSpan.FromSeconds(30));


    public async ValueTask<bool> TryAddUpLoadTicketAsync(UploadTicket uploadTicket)
        => await TryAdd(UploadTicket.RedisKey, uploadTicket.Token, uploadTicket, TimeSpan.FromSeconds(30));

    

    public async ValueTask<string?> GetAsync(string key, Guid ticketToken)
    {
        var result = await _dbConnectionFactory.Redis.JSON().GetAsync($"{key}:{ticketToken}");
        return result.ToString();
    }

    public async ValueTask<DownloadTicket?> GetDownloadTicket(Guid guidToken)
        => DownloadTicket.FromJson(await GetAsync(DownloadTicket.RedisKey, guidToken));

    
    public async ValueTask<UploadTicket?> GetUploadTicket(Guid guidToken)
        => UploadTicket.FromJson(await GetAsync(UploadTicket.RedisKey, guidToken));
    
    public async ValueTask<bool> Remove(string key, Guid ticketToken)
    {
        return await _dbConnectionFactory.Redis.JSON().DelAsync($"{key}:{ticketToken}") > 0;
    }
    
    public async ValueTask<bool> RemoveDownloadTicket(DownloadTicket downloadTicket)
        => await Remove(DownloadTicket.RedisKey, downloadTicket.Token);
    
    public async ValueTask<bool> RemoveUploadTicket(UploadTicket uploadTicket)
        => await Remove(UploadTicket.RedisKey, uploadTicket.Token);
    
    
}