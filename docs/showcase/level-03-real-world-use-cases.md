# Level 03: Real-World Enterprise Use Cases

## Overview

Enterprise systems frequently exchange critical documents based on global standard schemas:
- **UBL 2.1 Electronic Invoicing** (`urn:oasis:names:specification:ubl:schema:xsd:Invoice-2`)
- **ISO 20022 pain.001 Customer Credit Transfer** (`urn:iso:std:iso:20022:tech:xsd:pain.001.001.03`)

This level demonstrates how `EricksonLopez.Xml.Validation` loads these complex schemas from physical files and validates documents with precise constraint violation diagnostics.

> [!NOTE]
> The schemas included in `Schemas/` for this showcase are simplified demonstration models containing representative subsets of elements for each standard.

---

## 1. Registering Schemas from Physical Files

Using the `RegisterSchemaFile` extension method from `XmlSchemaCacheExtensions`, schemas stored as physical files are compiled and indexed by their target namespace:

```csharp
var cache = new XmlSchemaCache();
var validator = new XmlSchemaValidator(cache);

var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
var invoiceXsdPath = Path.Combine(schemasDir, "invoice.xsd");
var paymentXsdPath = Path.Combine(schemasDir, "payment.xsd");

// Register UBL 2.1 and ISO 20022 pain.001 schemas
cache.RegisterSchemaFile("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", invoiceXsdPath);
cache.RegisterSchemaFile("urn:iso:std:iso:20022:tech:xsd:pain.001.001.03", paymentXsdPath);
```

---

## 2. Validating UBL 2.1 Electronic Invoices

### Scenario A: Conforming Electronic Invoice
```xml
<Invoice xmlns="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2">
  <ID>INV-2026-9812</ID>
  <IssueDate>2026-09-02</IssueDate>
  <InvoiceTypeCode>380</InvoiceTypeCode>
  <DocumentCurrencyCode>USD</DocumentCurrencyCode>
  <AccountingSupplierParty>
    <PartyName>Erickson Corporation</PartyName>
    <TaxIdentifier>US-EIN-987654321</TaxIdentifier>
  </AccountingSupplierParty>
  <AccountingCustomerParty>
    <PartyName>Global Enterprise Ltd</PartyName>
    <TaxIdentifier>GB-VAT-123456789</TaxIdentifier>
  </AccountingCustomerParty>
  <LegalMonetaryTotal>
    <LineExtensionAmount>1000.00</LineExtensionAmount>
    <TaxExclusiveAmount>1000.00</TaxExclusiveAmount>
    <TaxInclusiveAmount>1180.00</TaxInclusiveAmount>
    <PayableAmount>1180.00</PayableAmount>
  </LegalMonetaryTotal>
</Invoice>
```

```csharp
var result = validator.Validate(validUblXml, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
// result.IsSuccess == true
```

### Scenario B: Structural Schema Violation
When an invoice omits mandatory sequence elements (such as `InvoiceTypeCode` or `DocumentCurrencyCode`):

```csharp
var result = validator.Validate(invalidUblXml, "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

// result.IsFailure == true
// result.Error.Code == "XmlValidation.SchemaViolation"
// result.Error.Description includes line, position, and expected elements:
// "Line 6, Pos 4: The element 'Invoice' ... has invalid child element ... List of possible elements expected: 'InvoiceTypeCode' ..."
```

---

## 3. Validating ISO 20022 pain.001 Payment Initiation

Validating financial message transfers with currency codes, BIC, and control totals:

```csharp
var isoResult = validator.Validate(validIsoPaymentXml, "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03");
// isoResult.IsSuccess == true
```

---

## Code Reference
- [`Level03RealWorldUseCases.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level03RealWorldUseCases.cs)
