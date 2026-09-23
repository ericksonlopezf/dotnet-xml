# Architecture & Flow Diagrams — EricksonLopez.Xml.Validation

Architectural diagrams and technical execution flows for **`EricksonLopez.Xml.Validation`**, derived directly from the verified source code.

---

## 1. High-Level System Architecture

The following diagram illustrates layer separation, isolation boundaries, and relationships between the consumer, the dependency injection container, and core validation components:

```mermaid
graph TB
    subgraph ConsumerLayer ["Consumer Layer (API / Worker / Pipeline)"]
        ClientApp["Client Application / Endpoints"]
    end

    subgraph DependencyInjection ["Dependency Injection Container"]
        DI["IServiceCollection.AddXmlValidation()"]
        Opts["XmlValidationOptions"]
    end

    subgraph CoreEngine ["Validation Engine (EricksonLopez.Xml.Validation)"]
        Validator["IXmlSchemaValidator\n(XmlSchemaValidator - Singleton)"]
        Cache["IXmlSchemaCache\n(XmlSchemaCache - Singleton)"]
        Dict[("ConcurrentDictionary\n<targetNamespace, XmlSchemaSet>")]
        SafeSettings["XmlReaderSettings\n(DtdProcessing.Prohibit\nXmlResolver = null)"]
    end

    subgraph Observability ["Observability & Diagnostics"]
        Logger["ILogger<XmlSchemaValidator>\n[LoggerMessage] EventIds 1001-1005"]
    end

    subgraph OutputContract ["Output Contract"]
        Result["Result<bool>\nSuccess(true) | Failure(Error)"]
    end

    ClientApp -->|"Injects"| Validator
    DI -->|"Registers as Singleton"| Validator
    DI -->|"Registers as Singleton"| Cache
    DI -.->|"Configures"| Opts
    Validator -->|"Resolves Precompiled Schema"| Cache
    Cache -->|"O(1) Namespace Index"| Dict
    Validator -->|"Enforces Anti-XXE"| SafeSettings
    Validator -.->|"Emits zero-alloc logs"| Logger
    Validator -->|"Returns"| Result
```

---

## 2. End-to-End Validation Flow

The complete decision logic and data transformation pipeline executed on every call to `Validate` or `ValidateAsync`:

```mermaid
flowchart TD
    Start(["Start: Validate / ValidateAsync Invocation"]) --> CheckCache{"Does targetNamespace exist\nin IXmlSchemaCache?"}

    CheckCache -- "No" --> RetNotFound["Return Error.NotFound\n'XmlValidation.SchemaNotRegistered'\n(EventId 1001)"]
    RetNotFound --> Done(["End"])

    CheckCache -- "Yes" --> PrepSettings["Configure XmlReaderSettings:\n• DtdProcessing = Prohibit (Anti-XXE)\n• XmlResolver = null\n• ValidationType = Schema\n• Schemas = XmlSchemaSet\n• MaxCharactersInDocument\n• Attach ValidationEventHandler"]

    PrepSettings --> CreateReader["Create XmlReader over Input\n(String / Stream / ReadOnlySpan<byte>)"]
    CreateReader --> ReadLoop{"XmlReader.Read()\n/ ReadAsync()"}

    ReadLoop -- "XmlException\n(Syntax error, DTD, or DoS limit)" --> RetMalformed["Return Error.Validation\n'XmlValidation.XmlMalformed'\n(EventId 1005)"]
    RetMalformed --> Done

    ReadLoop -- "First Element Node" --> CheckRoot{"IsRootElementDeclared\nin schemaCache?"}
    CheckRoot -- "No" --> AddRootError["Append Error:\nRoot element not declared"]
    CheckRoot -- "Yes" --> ContinueRead["Continue Streaming"]
    AddRootError --> ReadLoop
    ContinueRead --> ReadLoop

    ReadLoop -- "ValidationEventHandler Error" --> AddError["Append Error with Line/Position\n(up to MaxErrors)"]
    AddError --> ReadLoop

    ReadLoop -- "ValidationEventHandler Warning" --> CheckWarnConfig{"options.IncludeWarnings?"}
    CheckWarnConfig -- "Yes" --> AddWarn["Append Warning with '[Warning]' prefix"]
    CheckWarnConfig -- "No" --> IgnoreWarn["Silently ignore warning"]
    AddWarn --> ReadLoop
    IgnoreWarn --> ReadLoop

    ReadLoop -- "End of Document (EOF)" --> EvaluateErrors{"Errors > 0 OR\n(TreatWarningsAsErrors && Warnings > 0)?"}

    EvaluateErrors -- "Yes" --> RetViolation["Return Error.Validation\n'XmlValidation.SchemaViolation'\n(EventId 1002)"]
    EvaluateErrors -- "No" --> RetSuccess["Return Result<bool>.Success(true)\n(EventId 1003)"]

    RetViolation --> Done
    RetSuccess --> Done
```

---

## 3. Validation Sequence Diagram

Collaborator interactions across synchronous and asynchronous execution paths:

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client / Pipeline
    participant Validator as XmlSchemaValidator
    participant Cache as XmlSchemaCache
    participant Store as ConcurrentDictionary
    participant Reader as XmlReader
    participant Logger as ILogger

    Client->>Validator: Validate(xml, targetNamespace)
    activate Validator

    Validator->>Cache: TryGetSchemaSet (via internal IXmlSchemaSetProvider)
    activate Cache
    Cache->>Store: TryGetValue(targetNamespace, out schemaSet)
    Store-->>Cache: schemaSet
    Cache-->>Validator: bool (found or not)
    deactivate Cache

    alt Schema Not Found
        Validator->>Logger: LogSchemaNotRegistered (EventId 1001)
        Validator-->>Client: Result<bool>.Failure(Error.NotFound)
    else Schema Found
        Validator->>Reader: Create(Input, safeSettings)
        activate Reader
        loop Iterate over XML nodes
            Validator->>Reader: Read()
            Reader-->>Validator: Node / Validation Event
        end
        deactivate Reader

        alt Schema Violation Detected
            Validator->>Logger: LogValidationFailed (EventId 1002)
            Validator-->>Client: Result<bool>.Failure(Error.Validation)
        else Document Compliant
            Validator->>Logger: LogValidationSucceeded (EventId 1003)
            Validator-->>Client: Result<bool>.Success(true)
        end
    end
    deactivate Validator
```

---

## 4. Validation Lifecycle State Machine

Formal state transitions representing an in-flight validation request:

```mermaid
stateDiagram-v2
    [*] --> Received: Validate(input, ns)
    Received --> CheckingCache: Query IXmlSchemaCache

    CheckingCache --> SchemaNotRegistered: Schema not found in cache
    SchemaNotRegistered --> [*]: Returns Error.NotFound

    CheckingCache --> InitializingReader: Precompiled SchemaSet resolved
    InitializingReader --> ParsingNodes: Anti-XXE XmlReaderSettings applied

    state ParsingNodes {
        [*] --> ReadingNode
        ReadingNode --> AccumulatingErrors: XSD validation error event
        AccumulatingErrors --> ReadingNode: Continue stream
        ReadingNode --> EvaluatingWarning: XSD warning event
        EvaluatingWarning --> AccumulatingWarnings: IncludeWarnings = true
        EvaluatingWarning --> ReadingNode: IncludeWarnings = false
        AccumulatingWarnings --> ReadingNode
        ReadingNode --> Malformed: DTD or syntax violation
        ReadingNode --> Cancelled: CancellationToken triggered
        ReadingNode --> Completed: EOF reached
    }

    Malformed --> [*]: Returns Error.Validation (XmlMalformed)
    Cancelled --> [*]: Throws OperationCanceledException
    Completed --> EvaluatingResult

    EvaluatingResult --> ValidationFailed: Errors > 0 or (TreatWarningsAsErrors && Warnings > 0)
    EvaluatingResult --> ValidationSucceeded: No blocking errors or warnings

    ValidationFailed --> [*]: Returns Error.Validation (SchemaViolation)
    ValidationSucceeded --> [*]: Returns Result.Success(true)
```

---

## 5. Component Dependency Graph

Detailed package boundaries, interfaces, and framework dependencies:

```mermaid
graph LR
    subgraph BCL [".NET BCL"]
        XmlReader["System.Xml.XmlReader"]
        XmlSchemaSet["System.Xml.Schema.XmlSchemaSet"]
        ConcurrentDict["ConcurrentDictionary&lt;string, SchemaCacheEntry&gt;"]
    end

    subgraph FrameworkAbstractions ["Microsoft Extensions"]
        MS_DI["Microsoft.Extensions.DependencyInjection"]
        MS_Logging["Microsoft.Extensions.Logging.Abstractions"]
    end

    subgraph LibraryPackage ["EricksonLopez.Xml.Validation"]
        IXmlCache["IXmlSchemaCache"]
        XmlCache["XmlSchemaCache"]
        CacheExts["XmlSchemaCacheExtensions"]
        IXmlVal["IXmlSchemaValidator"]
        XmlVal["XmlSchemaValidator"]
        Options["XmlValidationOptions"]
        Exts["XmlValidationServiceCollectionExtensions"]
    end

    subgraph ExternalEcosystem ["EricksonLopez Ecosystem"]
        ResultPkg["EricksonLopez.Result\n(Result<T>, Error)"]
    end

    XmlCache -.->|"Implements"| IXmlCache
    CacheExts -.->|"Extends"| IXmlCache
    XmlCache --> ConcurrentDict
    XmlCache --> XmlSchemaSet
    XmlCache --> XmlReader

    XmlVal -.->|"Implements"| IXmlVal
    XmlVal --> IXmlCache
    XmlVal --> Options
    XmlVal --> MS_Logging
    XmlVal --> ResultPkg
    XmlVal --> XmlReader

    Exts --> MS_DI
    Exts --> IXmlCache
    Exts --> IXmlVal
    Exts --> Options
```

---

## 6. Defensive Anti-XXE Perimeter Ingestion Pipeline

How `IXmlSchemaValidator` acts as a frontline firewall (DMZ) in enterprise ingestion architectures:

```mermaid
flowchart LR
    UntrustedPayload["Untrusted XML Payload\n(WebHook / HTTP Body / Queue Message)"] --> Firewall["Perimeter Defensive Gateway\n(IXmlSchemaValidator)"]

    Firewall -- "XXE Attack / DTD Prohibited" --> RejectXXE["400 Bad Request\nImmediately Neutralized\nZero Entity Expansion"]
    Firewall -- "XSD Schema Violation" --> RejectSchema["422 Unprocessable Entity\nRejected with Line/Position"]
    Firewall -- "Compliant & Safe" --> SafeParser["Secure Typed Deserializer\n(Domain Entity / Command DTO)"]

    SafeParser --> DomainCore["Domain Core / CQRS Handlers\n(Guaranteed Valid and XXE-Free XML)"]
```
