using FluentAssertions;
using IntegrationServices.Domain.Common;
using IntegrationServices.Domain.ValueObjects;

namespace IntegrationServices.Tests.Domain;

public sealed class VisitCounterTests
{
    [Fact]
    public void Zero_Should_Create_Counter_With_Value_Zero()
    {
        var counter = VisitCounter.Zero();

        counter.Value.Should().Be(0);
    }

    [Fact]
    public void Increment_Should_Increase_Counter_By_One()
    {
        var counter = VisitCounter.Zero();

        var result = counter.Increment();

        result.Value.Should().Be(1);
    }

    [Fact]
    public void HasReachedReturnLimit_Should_Return_True_When_Value_Is_Three()
    {
        var counter = VisitCounter.From(3);

        counter.HasReachedReturnLimit().Should().BeTrue();
    }

    [Fact]
    public void From_Should_Throw_When_Value_Is_Negative()
    {
        var act = () => VisitCounter.From(-1);

        act.Should().Throw<DomainException>().WithMessage("Visit counter cannot be negative.");
    }
}