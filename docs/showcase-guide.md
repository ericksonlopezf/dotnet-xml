# Showcase Execution Guide — EricksonLopez.Xml.Validation

Instructions for building, executing, and exploring the official **Showcase** reference implementation (`samples/EricksonLopez.Xml.Showcase`).

---

## 1. What is the Showcase Project?

The Showcase is the **official executable documentation** for `EricksonLopez.Xml.Validation`. Structured as an 11-level progressive curriculum, it guides developers from core conceptual foundations and threat modeling through to enterprise defensive perimeter pipelines.

Every level runs real code against real XSD schemas, demonstrating **only actual public APIs** of the library.

### Core Documentation Modules:
- [Level 00: Conceptual Foundations & Anti-XXE Threat Modeling](showcase/level-00-introduction.md)
- [Level 01: Standalone Quick Start (No DI)](showcase/level-01-basic-validation.md)
- [Level 02: Full Configuration & Dependency Injection](showcase/level-02-full-configuration.md)
- [Level 03: Real-World Enterprise Use Cases](showcase/level-03-real-world-use-cases.md)
- [Level 04: Advanced Integration & Input Modalities](showcase/level-04-advanced-integration.md)
- [Level 05: Concurrent Processing & Cooperative Cancellation](showcase/level-05-processing-and-concurrency.md)
- [Level 06: Error Taxonomy & Anti-XXE Defenses](showcase/level-06-error-handling-and-classification.md)
- [Level 07: Scalability, Throughput & Memory Efficiency](showcase/level-07-scalability-and-throughput.md)
- [Level 08: Customization & Extensibility via Decorators](showcase/level-08-customization-and-extensibility.md)
- [Level 09: Architectural Boundaries & Consumer Integration](showcase/level-09-architectural-boundaries-and-consumers.md)
- [Level 10: Enterprise Architecture & Defensive Perimeter Pipeline](showcase/level-10-enterprise-architecture.md)

---

## 2. Running All Showcase Levels

From the root of the repository:

```bash
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj
```

This sequentially executes Levels 00 through 10, printing diagnostic telemetry, throughput benchmarks, and validation summaries to the console.

---

## 3. Running Individual Levels

You can execute any individual level by passing its index as a command-line argument:

```bash
# Level 00: Conceptual Foundations & Anti-XXE Threat Modeling
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 0

# Level 01: Standalone Quick Start (No DI)
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 1

# Level 02: Microsoft Dependency Injection & Structured Logging
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 2

# Level 03: Real-World Enterprise Schemas (UBL 2.1 e-Invoice & ISO 20022)
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 3

# Level 04: Advanced Input Modalities (string, Stream, Span, async)
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 4

# Level 05: High Concurrency & Cooperative Cancellation
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 5

# Level 06: Error Taxonomy & Anti-XXE Attack Neutralization
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 6

# Level 07: Scalability Micro-Benchmarks & Throughput Analysis
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 7

# Level 08: Extensibility via Decorator Pattern
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 8

# Level 09: Architectural Boundaries & Queue Consumers
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 9

# Level 10: Enterprise Perimeter Defensive Pipeline
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 10
```

---

## 4. Showcase Project Structure

```text
samples/EricksonLopez.Xml.Showcase/
├── EricksonLopez.Xml.Showcase.csproj
├── Program.cs                                  # Interactive console entry point and level dispatcher
├── Schemas/                                    # XSD schema catalog (simplified for demonstration)
│   ├── orders.xsd
│   ├── invoice.xsd                             # Simplified schema inspired by UBL 2.1 Invoice (demo only — not the full standard)
│   ├── payment.xsd                             # Simplified schema inspired by ISO 20022 pain.001 (demo only — not the full standard)
│   ├── config.xsd
│   └── warnings.xsd
└── Levels/
    ├── Level00Conceptual.cs                    # Level 0: Philosophy, trade-offs, and Anti-XXE
    ├── Level01QuickStart.cs                    # Level 1: Basic standalone usage
    ├── Level02FullConfiguration.cs             # Level 2: DI, options, and logging
    ├── Level03RealWorldUseCases.cs             # Level 3: UBL invoice & ISO 20022 payments
    ├── Level04AdvancedIntegration.cs           # Level 4: Bulk registration & 4 input modes
    ├── Level05ProcessingAndConcurrency.cs      # Level 5: 200 concurrent tasks & cancellation
    ├── Level06ErrorHandlingAndClassification.cs # Level 6: Error taxonomy & XXE attacks
    ├── Level07ScalabilityAndThroughput.cs      # Level 7: Zero-allocation throughput benchmarks
    ├── Level08CustomizationAndExtensibility.cs # Level 8: IXmlSchemaValidator decorators
    ├── Level09ArchitecturalBoundariesAndConsumers.cs # Level 9: Queue consumers and boundaries
    └── Level10EnterpriseArchitecture.cs        # Level 10: Perimeter defensive pipeline
```
