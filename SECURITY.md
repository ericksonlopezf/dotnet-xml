# Security Policy — EricksonLopez.Xml.Validation

Security is a primary design goal of `EricksonLopez.Xml.Validation`. The library is architected to protect .NET applications from XML-based attack vectors, particularly XML External Entity (XXE) injection and resource exhaustion attacks.

---

## 1. Supported Versions

Security fixes and patches are applied to the following active versions:

| Version | Status | Supported .NET Runtimes | Notes |
|---|---|---|---|
| **1.0.x** | **Supported** | `.NET 8.0`, `.NET 9.0`, `.NET 10.0` | Active release line (`VersionPrefix=1.0.0`) |
| < 1.0.0 | Unsupported | N/A | Pre-release or legacy versions |

---

## 2. Reporting a Vulnerability

If you discover a security vulnerability or potential threat within this codebase, please follow responsible disclosure practices:

1. **Do NOT open a public GitHub issue.**
2. Report the vulnerability privately by emailing [ericksonlopezf@gmail.com](mailto:ericksonlopezf@gmail.com).
3. Include detailed information:
   - Affected package version.
   - Operating system and .NET runtime version.
   - Proof-of-concept (PoC) code or sample XML payload.
   - Description of the expected impact and attack vector.

### Response Timelines
- **Initial Acknowledgment:** Within 48 business hours.
- **Triage and Impact Assessment:** Within 5 business days.
- **Fix Delivery & Advisory Publication:** Coordinated with the reporter before public disclosure.

---

## 3. Known Security Boundaries and Invariants

`EricksonLopez.Xml.Validation` enforces architectural security boundaries that cannot be circumvented via the public API:

### 3.1 Unconditional Anti-XXE Protection
- Every `XmlReaderSettings` instantiated internally applies:
  ```csharp
  DtdProcessing = DtdProcessing.Prohibit;
  XmlResolver = null;
  ```
- **DTD Processing:** Any document containing a `<!DOCTYPE ...>` declaration is immediately rejected and returned as `Error.Validation("XmlValidation.XmlMalformed", ...)`.
- **Entity Resolution:** The internal `XmlResolver` is set to `null` across all schema compilation and XML validation routines. Remote or local file entity resolution is prohibited.

### 3.2 Schema Compilation Isolation
- When registering schemas via `IXmlSchemaCache`, `XmlSchemaSet.XmlResolver` is explicitly set to `null`.
- Schemas cannot dynamically fetch external schemas over HTTP/HTTPS during compilation. All imported or included schemas must be supplied locally.

### 3.3 Resource Exhaustion Mitigations
- Documents containing recursive entity expansions ("Billion Laughs" attack) are intercepted by the underlying parser before expansion occurs.
- Asynchronous validation (`ValidateAsync`) supports `CancellationToken` to terminate processing of excessively large streams.

---

## 4. Supply Chain Security

- **Sigstore Provenance Attestation:** Production package artifacts (`.nupkg`) are cryptographically attested using GitHub's build provenance attestation (`actions/attest-build-provenance` v2.2.3) powered by Sigstore, establishing verifiable origin from the official repository workflow.
- **NuGet Trusted Publishing (OIDC):** Package publishing to NuGet.org uses OpenID Connect (OIDC) token exchange via `NuGet/login@v1`. No permanent static NuGet API keys are stored as repository secrets.
- **Strong Naming:** Assemblies are cryptographically signed with the project Strong Name Key (`EricksonLopez.snk`, `PublicKeyToken=f3a287785b2818a1`), enabling consumers with strict signing policies to verify binary identity.
- **Central Package Management (CPM):** All dependencies are centrally declared and pinned in [`Directory.Packages.props`](Directory.Packages.props) to prevent transitive dependency hijacking or version drift.
- **Deterministic Builds & SourceLink:** Builds are reproducible with `<Deterministic>true</Deterministic>` and provide verified source auditing through `Microsoft.SourceLink.GitHub`.
- **Trim & Native AOT Compatibility:** Analyzer flags `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` ensure that IL code undergoes strict static analysis without dynamic reflection vulnerabilities.
