// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates enterprise schema validation patterns using namespaces and document structures
/// inspired by UBL 2.1 e-Invoice and ISO 20022 pain.001 standards.
/// </summary>
/// <remarks>
/// <strong>Important:</strong> The XSD schemas used in this level are <em>simplified demonstration schemas</em>
/// that share the official namespace URIs but contain only a subset of elements.
/// They are <em>not</em> the complete, normative UBL 2.1 or ISO 20022 schema definitions.
/// For production use against real-world documents, obtain the official schemas from
/// OASIS (UBL 2.1) and ISO 20022.org (pain.001) respectively.
/// </remarks>
public static class Level03RealWorldUseCases
{
    // NOTE: The following namespace URIs are the official standard URIs.
    // The XSD schemas in Schemas/ are simplified demo-only versions of these standards.
    private const string UblNamespace = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private const string IsoPaymentNamespace = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

    private const string ValidUblInvoiceXml = """
        <?xml version="1.0" encoding="utf-8"?>
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
        """;

    private const string InvalidUblInvoiceXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Invoice xmlns="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2">
          <ID>INV-2026-9812</ID>
          <IssueDate>2026-09-02</IssueDate>
          <!-- Missing mandatory InvoiceTypeCode and DocumentCurrencyCode per schema -->
          <AccountingSupplierParty>
            <PartyName>Erickson Corporation</PartyName>
            <TaxIdentifier>US-EIN-987654321</TaxIdentifier>
          </AccountingSupplierParty>
          <LegalMonetaryTotal>
            <LineExtensionAmount>1000.00</LineExtensionAmount>
            <TaxExclusiveAmount>1000.00</TaxExclusiveAmount>
            <TaxInclusiveAmount>1180.00</TaxInclusiveAmount>
            <PayableAmount>1180.00</PayableAmount>
          </LegalMonetaryTotal>
        </Invoice>
        """;

    private const string ValidIsoPaymentXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:pain.001.001.03">
          <CstmrCdtTrfInitn>
            <GrpHdr>
              <MsgId>MSG-20260923-001</MsgId>
              <CreDtTm>2026-09-23T10:00:00Z</CreDtTm>
              <NbOfTxs>1</NbOfTxs>
              <CtrlSum>50000.00</CtrlSum>
            </GrpHdr>
            <PmtInf>
              <PmtInfId>PMT-001</PmtInfId>
              <PmtMtd>TRF</PmtMtd>
              <ReqdExctnDt>2026-09-23</ReqdExctnDt>
              <Dbtr>
                <Nm>Treasury Corp</Nm>
              </Dbtr>
              <CdTrfTxInf>
                <PmtId>
                  <EndToEndId>E2E-999-XYZ</EndToEndId>
                </PmtId>
                <Amt>
                  <InstdAmt>50000.00</InstdAmt>
                </Amt>
                <Cdtr>
                  <Nm>Supplier International</Nm>
                </Cdtr>
              </CdTrfTxInf>
            </PmtInf>
          </CstmrCdtTrfInitn>
        </Document>
        """;

    /// <summary>
    /// Executes the real-world use cases showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 03: REAL-WORLD ENTERPRISE USE CASES");
        Console.WriteLine("================================================================================\n");

        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        // Registering enterprise schemas from XSD files on disk
        var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
        var invoiceXsdPath = Path.Combine(schemasDir, "invoice.xsd");
        var paymentXsdPath = Path.Combine(schemasDir, "payment.xsd");

        Console.WriteLine("[1] Registering UBL 2.1 schema (invoice.xsd)...");
        cache.RegisterSchemaFile(UblNamespace, invoiceXsdPath);

        Console.WriteLine("[2] Registering ISO 20022 pain.001 schema (payment.xsd)...");
        cache.RegisterSchemaFile(IsoPaymentNamespace, paymentXsdPath);

        // Scenario A: Valid UBL 2.1 electronic invoice
        Console.WriteLine("\n[Scenario A] Conforming UBL 2.1 Electronic Invoice:");
        var ublSuccessResult = validator.Validate(ValidUblInvoiceXml, UblNamespace);
        Console.WriteLine($"    Result: IsSuccess={ublSuccessResult.IsSuccess}");

        // Scenario B: UBL 2.1 invoice with missing mandatory elements (structural violation)
        Console.WriteLine("\n[Scenario B] Non-conforming UBL 2.1 Electronic Invoice with structural violation:");
        var ublFailResult = validator.Validate(InvalidUblInvoiceXml, UblNamespace);
        if (ublFailResult.IsFailure)
        {
            Console.WriteLine($"    Code: {ublFailResult.Error.Code}");
            Console.WriteLine($"    Detailed message with location: {ublFailResult.Error.Description}");
        }

        // Scenario C: Conforming ISO 20022 pain.001 payment message
        Console.WriteLine("\n[Scenario C] Conforming ISO 20022 pain.001 Payment Initiation:");
        var isoResult = validator.Validate(ValidIsoPaymentXml, IsoPaymentNamespace);
        Console.WriteLine($"    Result: IsSuccess={isoResult.IsSuccess}");

        Console.WriteLine("\n✔ Level 03 completed successfully.\n");
        return Task.CompletedTask;
    }
}
