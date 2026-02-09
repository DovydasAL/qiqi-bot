using System;
using QiQiBot.TravellingMerchant;

namespace QiQiBot.Tests;

public class TravellingMerchantRotationTests
{
    [Theory]
    [InlineData(8743L, "Slayer VIP Coupon", "Silverhawk down", "Taijitu")]
    [InlineData(8744L, "Goebie burial charm", "Menaphite gift offering (medium)", "Crystal triskelion")]
    [InlineData(8745L, "Menaphite gift offering (small)", "Menaphite gift offering (small)", "Starved ancient effigy")]
    [InlineData(8746L, "Distraction & Diversion reset token (daily)", "Menaphite gift offering (medium)", "Dragonkin lamp")]
    [InlineData(8747L, "Menaphite gift offering (small)", "Menaphite gift offering (medium)", "Dragonkin lamp")]
    [InlineData(8749L, "Barrel of bait", "Small goebie burial charm", "Menaphite gift offering (large)")]
    [InlineData(8750L, "Anima crystal", "Menaphite gift offering (small)", "Crystal triskelion")]
    [InlineData(8751L, "Small goebie burial charm", "Goebie burial charm", "Dungeoneering Wildcard")]
    [InlineData(8752L, "Horn of honour", "Horn of honour", "Menaphite gift offering (large)")]
    [InlineData(8753L, "Advanced pulse core", "Unfocused damage enhancer", "Large goebie burial charm")]
    [InlineData(8754L, "Broken fishing rod", "Horn of honour", "Menaphite gift offering (large)")]
    public void GetDailyStock_MatchesKnownWikiRotation(long runeDate, string expectedA, string expectedB, string expectedC)
    {
        var stock = TravellingMerchantRotation.GetDailyStock(runeDate);

        Assert.Equal(expectedA, stock.SlotA);
        Assert.Equal(expectedB, stock.SlotB);
        Assert.Equal(expectedC, stock.SlotC);
    }

    [Fact]
    public void GetDailyStockFromOffset_MatchesGetDailyStock_ForOffset()
    {
        const long todayRuneDate = 8750;
        const int offset = 2;

        var viaOffset = TravellingMerchantRotation.GetDailyStockFromOffset(todayRuneDate, offset);
        var direct = TravellingMerchantRotation.GetDailyStock(todayRuneDate + offset);

        Assert.Equal(direct, viaOffset);
    }

    [Fact]
    public void FixedSlotItem_IsExpectedValue()
    {
        Assert.Equal("Uncharted island map (Deep Sea Fishing)", TravellingMerchantRotation.FixedSlotItem);
    }

    [Fact]
    public void ToRuneDate_Zero_ForEpochDate()
    {
        var d = new DateTime(2002, 2, 27, 12, 34, 56, DateTimeKind.Utc);
        var rd = TravellingMerchantRotation.ToRuneDate(d);
        Assert.Equal(0, rd);
    }

    [Fact]
    public void ToRuneDate_MatchesKnownRuneDates_ForUtcDates()
    {
        // From the wiki table:10–15 Feb2026 map to rune dates8749–8754
        Assert.Equal(8749, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(8750, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(8751, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(8752, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(8753, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal(8754, TravellingMerchantRotation.ToRuneDate(new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void GetDailyStock_DateTime_Overload_UsesRuneDateConversion()
    {
        var date = new DateTime(2026, 2, 10, 5, 0, 0, DateTimeKind.Utc);

        var viaDate = TravellingMerchantRotation.GetDailyStock(date);
        var viaRuneDate = TravellingMerchantRotation.GetDailyStock(8749);

        Assert.Equal(viaRuneDate, viaDate);
    }
}
