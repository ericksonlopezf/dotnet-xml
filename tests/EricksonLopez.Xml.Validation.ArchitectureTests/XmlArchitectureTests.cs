// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Xml.Validation.ArchitectureTests;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Xml.Validation;
using NetArchTest.Rules;
using Xunit;

[Trait("Category", "Architecture")]
public sealed class XmlArchitectureTests
{
    private static readonly Assembly ValidationAssembly = typeof(XmlSchemaValidator).Assembly;

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    public void ValidationAssembly_ShouldNotDependOn_ForbiddenThirdPartyLibraries()
    {
        var result = Types.InAssembly(ValidationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Newtonsoft.Json",
                "RabbitMQ.Client",
                "Confluent.Kafka",
                "Microsoft.AspNetCore",
                "System.Reflection.Emit")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("EricksonLopez.Xml.Validation must remain lightweight and free of forbidden infrastructure dependencies.");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    public void Interfaces_ShouldStartWith_I()
    {
        var result = Types.InAssembly(ValidationAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("All interfaces in EricksonLopez.Xml.Validation must follow the 'I' prefix naming convention.");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    public void ConcreteServices_ShouldBeSealed()
    {
        var result = Types.InAssembly(ValidationAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .AreNotStatic()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("All concrete classes in EricksonLopez.Xml.Validation should be sealed to ensure predictable performance and prevent unintended inheritance.");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    public void PublicTypes_ShouldResideIn_XmlValidationNamespace()
    {
        var result = Types.InAssembly(ValidationAssembly)
            .That()
            .ArePublic()
            .Should()
            .ResideInNamespaceStartingWith("EricksonLopez.Xml.Validation")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("All public types must reside within the EricksonLopez.Xml.Validation namespace hierarchy.");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection used for architectural verification in test harness.")]
    public void ProductionAssembly_ShouldHaveZero_ObsoleteMembers()
    {
        var obsoleteEntities = ValidationAssembly.GetExportedTypes()
            .SelectMany(t =>
            {
                var members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                return members.Cast<MemberInfo>().Append(t);
            })
            .Where(m => m.GetCustomAttribute<ObsoleteAttribute>() is not null)
            .Select(m => $"{m.DeclaringType?.Name ?? "TopLevel"}.{m.Name}")
            .ToList();

        obsoleteEntities.Should().BeEmpty("EricksonLopez.Xml.Validation must enforce zero [Obsolete] across all types, methods, constructors, and properties.");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Reflection used for architectural verification in test harness.")]
    public void IXmlSchemaCache_Should_DeclareIsRootElementDeclaredAsAbstractMethod()
    {
        var interfaceType = typeof(IXmlSchemaCache);
        var method = interfaceType.GetMethod("IsRootElementDeclared");

        method.Should().NotBeNull("IXmlSchemaCache must define IsRootElementDeclared");
        method!.IsVirtual.Should().BeTrue("IsRootElementDeclared is part of an interface");
        method.IsAbstract.Should().BeTrue("IsRootElementDeclared must NOT have a default implementation because it cannot access the internal schema cache directly after the security refactor (XML-API-001)");

        var concreteType = typeof(XmlSchemaCache);
        var concreteMethod = concreteType.GetMethod("IsRootElementDeclared");

        concreteMethod.Should().NotBeNull();
        concreteMethod!.DeclaringType.Should().Be(concreteType, "XmlSchemaCache must override IsRootElementDeclared with an optimized dictionary lookup");
    }
}
