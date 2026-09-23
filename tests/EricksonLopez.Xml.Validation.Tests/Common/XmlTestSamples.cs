// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Xml.Validation.Tests.Common;

/// <summary>
/// Centralized test fixtures, schemas, and XML payloads for the validation test suite.
/// Eliminates copy-pasted string constants across individual test classes.
/// </summary>
public static class XmlTestSamples
{
    public const string TargetNamespace = "http://ericksonlopez.dev/invoice";

    public const string SampleXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/invoice"
                   xmlns="http://ericksonlopez.dev/invoice"
                   elementFormDefault="qualified">
          <xs:element name="Invoice">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Id" type="xs:string" />
                <xs:element name="Total" type="xs:decimal" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    public const string ValidXml = """
        <Invoice xmlns="http://ericksonlopez.dev/invoice">
            <Id>INV-001</Id>
            <Total>199.99</Total>
        </Invoice>
        """;

    public const string InvalidXml = """
        <Invoice xmlns="http://ericksonlopez.dev/invoice">
            <Id>INV-001</Id>
        </Invoice>
        """;

    public const string AltNamespace = "http://ericksonlopez.dev/product";

    public const string AltXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/product"
                   xmlns="http://ericksonlopez.dev/product"
                   elementFormDefault="qualified">
          <xs:element name="Product">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Name" type="xs:string" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    public const string WarningNamespace = "http://ericksonlopez.dev/warning";

    public const string WarningXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/warning"
                   xmlns="http://ericksonlopez.dev/warning"
                   elementFormDefault="qualified">
          <xs:element name="WarningRoot">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Total" type="xs:decimal" />
                <xs:any processContents="lax" minOccurs="0" maxOccurs="unbounded" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    public const string WarningOnlyXml = """
        <WarningRoot xmlns="http://ericksonlopez.dev/warning">
          <Total>100.50</Total>
          <UndeclaredElement>hello</UndeclaredElement>
        </WarningRoot>
        """;

    public const string WarningAndErrorXml = """
        <WarningRoot xmlns="http://ericksonlopez.dev/warning">
          <Total>not_a_number</Total>
          <UndeclaredElement>hello</UndeclaredElement>
        </WarningRoot>
        """;

    public const string XxeXml = """
        <!DOCTYPE Invoice [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
        <Invoice xmlns="http://ericksonlopez.dev/invoice">
            <Id>&xxe;</Id>
            <Total>100</Total>
        </Invoice>
        """;

    public const string SchemaLocationXml = """
        <Invoice xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                 xsi:schemaLocation="http://ericksonlopez.dev/invoice http://ericksonlopez.dev/schemas/invoice.xsd"
                 xmlns="http://ericksonlopez.dev/invoice">
            <Id>INV-SCHEMA-LOC</Id>
            <Total>49.99</Total>
        </Invoice>
        """;

    public const string CommonTypesNamespace = "http://ericksonlopez.dev/common";

    public const string CommonTypesXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/common"
                   xmlns="http://ericksonlopez.dev/common"
                   elementFormDefault="qualified">
          <xs:complexType name="CustomerType">
            <xs:sequence>
              <xs:element name="Name" type="xs:string" />
            </xs:sequence>
          </xs:complexType>
        </xs:schema>
        """;

    public const string OrderNamespace = "http://ericksonlopez.dev/order";

    public const string OrderWithImportXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/order"
                   xmlns="http://ericksonlopez.dev/order"
                   xmlns:c="http://ericksonlopez.dev/common"
                   elementFormDefault="qualified">
          <xs:import namespace="http://ericksonlopez.dev/common" schemaLocation="common.xsd" />
          <xs:element name="Order">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Customer" type="c:CustomerType" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    public const string ExternalDirImportNamespace = "http://ericksonlopez.dev/ext-dir";

    public const string ExternalDirImportXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/ext-dir"
                   xmlns="http://ericksonlopez.dev/ext-dir"
                   elementFormDefault="qualified">
          <xs:import namespace="http://external.org/test" schemaLocation="http://127.0.0.1:9999/nonexistent.xsd" />
          <xs:element name="Root" type="xs:string" />
        </xs:schema>
        """;
}
