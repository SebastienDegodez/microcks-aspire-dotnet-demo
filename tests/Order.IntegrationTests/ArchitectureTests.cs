using System.Reflection;
using NetArchTest.Rules;
using Xunit;
using Xunit.Abstractions;

using Order.Application;
using Order.Domain;
using Order.Infrastructure;

namespace Order.IntegrationTests;

public class ArchitectureTests
{
    private static string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string ApiNamespace = "Api";
    private const string ApplicationNamespace = "Application";
    private const string DomainNamespace = "Domain";
    private const string InfrastructureNamespace = "Infrastructure";


    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationReference).Assembly;
    private static readonly Assembly DomainAssembly = typeof(DomainReference).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureReference).Assembly;

    public ITestOutputHelper TestOutputHelper { get; }

    public ArchitectureTests(ITestOutputHelper testOutputHelper)
    {
        this.TestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Api_ShouldHaveOnlyDependsOn_Application()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(ApiNamespace)
            .Should().HaveDependencyOn(ApplicationNamespace)
            .And()
            .NotHaveDependencyOn(DomainNamespace)
            .And()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"{ApiNamespace} ne doit avoir de dépendance qu'avec {ApplicationNamespace}");
    }

    [Fact]
    public void Application_ShouldHaveDependsOn_Infrastucture()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ResideInNamespace(ApplicationNamespace)
            .Should().HaveDependencyOn(InfrastructureNamespace)
            .And().HaveDependencyOn(DomainNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"{ApplicationNamespace} doit pas avoir de dépendance qu'avec {InfrastructureNamespace} et {DomainNamespace}");
    }

    [Fact]
    public void Infrastucture_ShouldHaveDependsOn_Domain()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That().ResideInNamespace(InfrastructureNamespace)
            .Should().HaveDependencyOn(DomainNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, $"{InfrastructureNamespace} doit avoir de dépendance autre avec {DomainNamespace}");
    }

    [Fact]
    public void Api_ShouldNotHaveDependencyOn_EntityFramework()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(ApiNamespace)
            .ShouldNot().HaveDependencyOn(EntityFrameworkCore)
            .GetResult();

        Assert.True(result.IsSuccessful, "API ne doit pas avoir de dépendance avec EF. La référence à EF doit-être dans Infrastructure");
    }


    [Fact]
    public void Domain_ShouldNotHaveDependencyOn_EntityFramework()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(EntityFrameworkCore)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain ne doit pas avoir de dépendance avec EF. La référence à EF doit-être dans Infrastructure");
    }

    [Fact]
    public void Domain_Aggregate_ShouldBeImmutable()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().BeImmutable()
            .GetResult();

        if (result.IsSuccessful == false)
        {
            foreach (var failingTypeName in result.FailingTypeNames)
            {
                this.TestOutputHelper.WriteLine($"Le type '{failingTypeName}' doit-être immutable.");
            }
        }
        Assert.True(result.IsSuccessful, "L'ensemble des types doivent_être immutable.");
    }

}

