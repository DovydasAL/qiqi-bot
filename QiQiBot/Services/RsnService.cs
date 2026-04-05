using System;
using Microsoft.EntityFrameworkCore;
using QiQiBot.Models;

namespace QiQiBot.Services;

public class RsnService : IRsnService
{
    private readonly ClanContext _context;

    public RsnService(ClanContext context)
    {
        _context = context;
    }

    public async Task<string?> GetRsnAsync(ulong guildId, ulong userId)
    {
        var record = await _context.GuildUserRsns
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);

        return record?.RuneScapeName;
    }

    public async Task<Dictionary<ulong, string>> GetRsnsAsync(ulong guildId)
    {
        return await _context.GuildUserRsns
            .AsNoTracking()
            .Where(x => x.GuildId == guildId)
            .ToDictionaryAsync(x => x.UserId, x => x.RuneScapeName);
    }

    public async Task SetRsnAsync(ulong guildId, ulong userId, string runescapeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runescapeName);

        var sanitizedName = runescapeName.Trim();

        var record = await _context.GuildUserRsns
            .SingleOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);

        if (record is null)
        {
            record = new GuildUserRsn
            {
                GuildId = guildId,
                UserId = userId,
                RuneScapeName = sanitizedName
            };
            _context.GuildUserRsns.Add(record);
        }
        else
        {
            record.RuneScapeName = sanitizedName;
        }

        await _context.SaveChangesAsync();
    }
}
