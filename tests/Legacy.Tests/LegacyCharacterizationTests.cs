using System.Globalization;
using PickupPoint.Legacy;
using Xunit;

namespace PickupPoint.Legacy.Tests;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static List<PickupLine> TwoLines() => new()
    {
        new PickupLine { Code = "A-1", Qty = 2, Price = 150m },
        new PickupLine { Code = "B-2", Qty = 1, Price = 400m },
    };

    private static List<PickupLine> LinesWithSubtotal(decimal subtotal) => new()
    {
        new PickupLine { Code = "A-1", Qty = 2, Price = 150m },
        new PickupLine { Code = "B-2", Qty = 1, Price = 400m },
        new PickupLine { Code = "C-3", Qty = 1, Price = subtotal - 700m },
    };

    [Theory]
    [InlineData(null, OrderState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", OrderState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", OrderState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", OrderState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfItems,
        OrderState state,
        string mail,
        string expected)
    {
        List<PickupLine>? items = kindOfItems switch
        {
            "two" => TwoLines(),
            "empty" => new List<PickupLine>(),
            _ => null,
        };
        var sut = new LegacyPickupProcessor();

        var actual = sut.Handle(
            1001,
            7,
            "Іван",
            mail,
            RecipientKind.Regular,
            0,
            items,
            state,
            "UAH",
            new DateTime(2026, 3, 1),
            false);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipOrder_ReturnsExactReport()
    {
        var sut = new LegacyPickupProcessor();

        var report = sut.Handle(
            1001,
            7,
            "Іван",
            "a@b.c",
            RecipientKind.Vip,
            3,
            TwoLines(),
            OrderState.New,
            "UAH",
            new DateTime(2026, 3, 1),
            false);

        var expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "A-1 x2 = 300.00 UAH\n" +
            "B-2 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
        Assert.Contains("ok 1001\n", sut.DumpLog());
    }

    [Theory]
    [InlineData(RecipientKind.Regular, 0, 760)]
    [InlineData(RecipientKind.Vip, 0, 655)]
    [InlineData(RecipientKind.Staff, 0, 550)]
    [InlineData(RecipientKind.Regular, 11, 725)]
    public void Preview_ByKind_ReturnsTotal(
        RecipientKind kind,
        int done,
        decimal expected)
    {
        var sut = new LegacyPickupProcessor();

        var total = sut.Preview(kind, done, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_AtFreeShippingBoundary_ReturnsTotal()
    {
        var sut = new LegacyPickupProcessor();

        var total = sut.Preview(RecipientKind.Regular, 0, LinesWithSubtotal(1000m));

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void Preview_JustBelowFreeShippingBoundary_AddsShipping()
    {
        var sut = new LegacyPickupProcessor();

        var total = sut.Preview(RecipientKind.Regular, 0, LinesWithSubtotal(999.99m));

        Assert.Equal(1059.99m, total);
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        var recipient = new Recipient
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = RecipientKind.Vip,
            DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };

        var years = DateTime.Now.Year - 2020;
        var expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] " +
            "<ivan@mail.com> стаж " + years;

        var actual = new LegacyPickupProcessor().DescribeClient(recipient);

        Assert.Equal(expected, actual);
    }
}
