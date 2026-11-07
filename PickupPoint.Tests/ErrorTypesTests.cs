using PickupPoint.Core.Errors;
using Xunit;

namespace PickupPoint.Tests;

public sealed class ErrorTypesTests
{
    [Fact]
    public void Result_Ok_KeepsValue()
    {
        Result<int> result = Result<int>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Result_Fail_KeepsErrorAndHasNoValue()
    {
        Result<int> result = Result<int>.Fail("Кількість має бути додатною");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal("Кількість має бути додатною", result.Error);
    }

    [Fact]
    public void Result_Fail_WithEmptyError_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => Result<int>.Fail(" "));
    }

    [Fact]
    public void DomainRuleException_KeepsRuleAndMessage()
    {
        DomainRuleException exception = new(
            "order.exists",
            "Замовлення 99 не знайдено");

        Assert.Equal("order.exists", exception.Rule);
        Assert.Equal("Замовлення 99 не знайдено", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void DomainRuleException_WithInner_PreservesCause()
    {
        InvalidOperationException cause = new("Стан Issued забороняє зміну");
        DomainRuleException exception = new(
            "order.editable",
            "Редагувати можна лише нове замовлення",
            cause);

        Assert.Equal("order.editable", exception.Rule);
        Assert.Same(cause, exception.InnerException);
    }

    [Fact]
    public void DomainRuleException_WithEmptyRule_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new DomainRuleException(" ", "Текст"));
    }
}
