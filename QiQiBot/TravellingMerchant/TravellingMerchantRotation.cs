using System;

namespace QiQiBot.TravellingMerchant;

public static class TravellingMerchantRotation
{
    // Fixed first slot item (always present in the shop)
    public const string FixedSlotItem = "Uncharted island map (Deep Sea Fishing)";

    // RuneDate epoch:27 February200200:00:00 UTC
    private static readonly DateTime RuneDateEpochUtc = new(2002,2,27,0,0,0, DateTimeKind.Utc);

    // Port of Module:Rotations/Merchant/data slot_map.A/B/C
    // NOTE: Lua arrays are0-based here; C# arrays are0-based as usual, so we copy in order.
    private static readonly string[] SlotAItems =
    [
        "Gift for the Reaper",
        "Broken fishing rod",
        "Barrel of bait",
        "Anima crystal",
        "Small goebie burial charm",
        "Goebie burial charm",
        "Menaphite gift offering (small)",
        "Menaphite gift offering (medium)",
        "Shattered anima",
        "Distraction & Diversion reset token (daily)",
        "Sacred clay (Deep Sea Fishing)",
        "Livid plant (Deep Sea Fishing)",
        "Slayer VIP Coupon",
        "Silverhawk down",
        "Unstable air rune",
        "Advanced pulse core",
        "Tangled fishbowl",
        "Unfocused damage enhancer",
        "Horn of honour",
    ];

    // In Lua: p.slot_map["B"] = p.slot_map["A"]
    private static readonly string[] SlotBItems = SlotAItems;

    private static readonly string[] SlotCItems =
    [
        "Taijitu",
        "Large goebie burial charm",
        "Menaphite gift offering (large)",
        "Distraction & Diversion reset token (weekly)",
        "Distraction & Diversion reset token (monthly)",
        "Dungeoneering Wildcard",
        "Message in a bottle (Deep Sea Fishing)",
        "Crystal triskelion",
        "Starved ancient effigy",
        "Deathtouched dart",
        "Dragonkin lamp",
        "Harmonic dust",
        "Unfocused reward enhancer",
    ];

    /// <summary>
    /// Represents the three variable items sold by the Travelling Merchant on a given runedate.
    /// The fixed first slot ("Uncharted island map (Deep Sea Fishing)") is not included here.
    /// </summary>
    public readonly record struct DailyStock(string SlotA, string SlotB, string SlotC);

    /// <summary>
    /// Convert a <see cref="DateTime"/> to a RuneDate (days since27 February2002 UTC).
    /// Time of day is ignored; only the calendar date in UTC is used.
    /// </summary>
    public static long ToRuneDate(DateTime dateTime)
    {
        var utc = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };

        var dateOnlyUtc = utc.Date;
        var diff = dateOnlyUtc - RuneDateEpochUtc.Date;
        return (long)diff.TotalDays;
    }

    /// <summary>
    /// Compute the three sold items for a given runedate (the same value used by rsrandom.runeDate_*).
    /// </summary>
    public static DailyStock GetDailyStock(long runeDate)
    {
        var aIndex = NextInt(runeDate, 3, SlotAItems.Length);
        var bIndex = NextInt(runeDate, 8, SlotBItems.Length);
        var cIndex = NextInt(runeDate, 5, SlotCItems.Length);

        return new DailyStock(
        SlotAItems[aIndex],
        SlotBItems[bIndex],
        SlotCItems[cIndex]
        );
    }

    /// <summary>
    /// Compute the three sold items for a specific date based on RuneDate conversion.
    /// </summary>
    public static DailyStock GetDailyStock(DateTime dateTime)
    => GetDailyStock(ToRuneDate(dateTime));

    /// <summary>
    /// Compute the three sold items for a day offset relative to <paramref name="todayRuneDate"/>.
    /// Offset0 = today,1 = tomorrow, -1 = yesterday, etc.
    /// </summary>
    public static DailyStock GetDailyStockFromOffset(long todayRuneDate, int offset)
    => GetDailyStock(todayRuneDate + offset);

    // Port of p.get in the Lua module:
    // local seed = runedate * (2 ^32) + (runedate % k)
    // return rsrandom.nextInt(seed, n)
    private static int NextInt(long runeDate, int k, int n)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

        var seed = unchecked((ulong)runeDate * 0x1_0000_0000UL + (ulong)(runeDate % k));
        var rng = new RsRandom(seed);
        return rng.NextInt(n);
    }

    /// <summary>
    /// Minimal C# port of the rsrandom.nextInt generator used by the wiki.
    /// This matches the Lua implementation in Module:Rotations/RsRandom (OOP API).
    /// </summary>
    private sealed class RsRandom
    {
        private const ulong Multi = 0x5DEECE66DUL; // MULTI
        private const ulong Mask = 1UL << 48; // MASK =2^48

        private ulong _seed;
        private bool _init;

        public RsRandom(ulong seed)
        {
            _seed = seed;
            _init = false;
        }

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

            // Lua: if not self.init then seed = bitxor(seed, MULTI) % MASK; self.init = true end
            if (!_init)
            {
                _seed = (BitXor(_seed, Multi)) % Mask;
                _init = true;
            }

            // seed = multiply_avoidlimit(seed)
            _seed = MultiplyAvoidLimit(_seed);

            // store full seed state before it gets shifted and converted to a slot number
            var value = _seed;

            // seed = rshift(seed,17)
            value >>= 17; // logical right shift, matches rshift

            // Lua instance method always uses modulo (power-of-two optimization only in p.nextInt)
            var result = (int)(value % (ulong)n);
            return result;
        }

        private static ulong BitXor(ulong a, ulong b) => a ^ b;

        // Port of multiply_avoidlimit from Lua; operates in53-bit safe way using16-bit chunks.
        private static ulong MultiplyAvoidLimit(ulong seed)
        {
            const ulong c0 = 0xe66d; // lower16 bits
            const ulong c1 = 0xdeec; // middle16 bits
            const ulong c2 = 0x0005; // upper16 bits
            const ulong chunk = 1UL << 16; //2^16

            var s0 = seed % chunk;
            var s1 = (seed / chunk) % chunk;
            var s2 = seed / (chunk * chunk);

            ulong carry = 11; // Lua literal11

            var r0 = s0 * c0 + carry;
            carry = r0 / chunk;
            r0 %= chunk;

            var r1 = s1 * c0 + s0 * c1 + carry;
            carry = r1 / chunk;
            r1 %= chunk;

            var r2 = s2 * c0 + s1 * c1 + s0 * c2 + carry;
            r2 %= chunk;

            return r2 * chunk * chunk + r1 * chunk + r0;
        }
    }
}
