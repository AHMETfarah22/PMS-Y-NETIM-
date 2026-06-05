using System;
using System.IO;
using System.Text;
using System.Xml;
using MySql.Data.MySqlClient;
using PmsSystem.Database;

namespace PmsSystem.Helpers
{
    public static class EInvoiceHelper
    {
        public class InvoiceDetails
        {
            public int ReservationID { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string IdentityOrTaxNumber { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public decimal RoomAmount { get; set; }
            public decimal ExtrasAmount { get; set; }
            public decimal TaxRate { get; set; } = 10; // KDV rate for konaklama in Turkey (usually 10% for accommodation)
            public decimal TotalAmount => RoomAmount + ExtrasAmount;
            public string InvoiceType { get; set; } = "E-Arşiv Fatura"; // E-Fatura or E-Arşiv
        }

        public static InvoiceDetails GetReservationBillingDetails(int reservationId)
        {
            var details = new InvoiceDetails { ReservationID = reservationId };
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        c.FirstName, c.LastName, c.IdentityNumber, c.Address, c.Email, c.Phone,
                        res.TotalAmount, res.ExtraAmount
                    FROM RESERVATIONS res
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    WHERE res.ReservationID = @rid";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@rid", reservationId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            details.CustomerName = $"{reader.GetString(0)} {reader.GetString(1)}";
                            details.IdentityOrTaxNumber = reader.IsDBNull(2) ? "11111111111" : reader.GetString(2);
                            details.Address = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            details.Email = reader.IsDBNull(4) ? "" : reader.GetString(4);
                            details.Phone = reader.IsDBNull(5) ? "" : reader.GetString(5);
                            details.RoomAmount = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            details.ExtrasAmount = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                            
                            // Simple logic to determine if e-Fatura or e-Arşiv based on tax identifier (T.C. or Tax ID length)
                            details.InvoiceType = details.IdentityOrTaxNumber.Length == 10 ? "E-Fatura" : "E-Arşiv Fatura";
                        }
                    }
                }
            }
            return details;
        }

        public static string GenerateUblInvoiceXml(InvoiceDetails details, string companyTitle, string companyTaxOffice, string companyTaxNumber, string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string invoiceUUID = Guid.NewGuid().ToString();
            string invoiceID = $"PMS{DateTime.Now:yyyy}{new Random().Next(100000, 999999)}";
            string filename = $"{invoiceID}.xml";
            string fullPath = Path.Combine(outputDir, filename);

            decimal baseAmount = details.TotalAmount / (1 + (details.TaxRate / 100));
            decimal taxAmount = details.TotalAmount - baseAmount;

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (XmlWriter writer = XmlWriter.Create(fullPath, settings))
            {
                writer.WriteStartDocument();
                // Standard UBL 2.1 e-Invoice Schema headers for Turkey
                writer.WriteStartElement("Invoice", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                writer.WriteAttributeString("xmlns", "cac", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                writer.WriteAttributeString("xmlns", "cbc", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

                writer.WriteElementString("cbc", "UUID", null, invoiceUUID);
                writer.WriteElementString("cbc", "ID", null, invoiceID);
                writer.WriteElementString("cbc", "CopyIndicator", null, "false");
                writer.WriteElementString("cbc", "IssueDate", null, DateTime.Now.ToString("yyyy-MM-dd"));
                writer.WriteElementString("cbc", "IssueTime", null, DateTime.Now.ToString("HH:mm:ss"));
                writer.WriteElementString("cbc", "InvoiceTypeCode", null, "SATIS");

                // Supplier (Tesis) Details
                writer.WriteStartElement("cac", "AccountingSupplierParty");
                writer.WriteStartElement("cac", "Party");
                writer.WriteStartElement("cac", "PartyName");
                writer.WriteElementString("cbc", "Name", null, companyTitle);
                writer.WriteEndElement(); // PartyName
                writer.WriteStartElement("cac", "PartyTaxScheme");
                writer.WriteStartElement("cac", "TaxScheme");
                writer.WriteElementString("cbc", "Name", null, companyTaxOffice);
                writer.WriteEndElement(); // TaxScheme
                writer.WriteEndElement(); // PartyTaxScheme
                writer.WriteEndElement(); // Party
                writer.WriteEndElement(); // AccountingSupplierParty

                // Customer Details
                writer.WriteStartElement("cac", "AccountingCustomerParty");
                writer.WriteStartElement("cac", "Party");
                writer.WriteStartElement("cac", "PartyIdentification");
                writer.WriteElementString("cbc", "ID", null, details.IdentityOrTaxNumber);
                writer.WriteEndElement(); // PartyIdentification
                writer.WriteStartElement("cac", "PartyName");
                writer.WriteElementString("cbc", "Name", null, details.CustomerName);
                writer.WriteEndElement(); // PartyName
                writer.WriteStartElement("cac", "Contact");
                writer.WriteElementString("cbc", "Telephone", null, details.Phone);
                writer.WriteElementString("cbc", "ElectronicMail", null, details.Email);
                writer.WriteEndElement(); // Contact
                writer.WriteEndElement(); // Party
                writer.WriteEndElement(); // AccountingCustomerParty

                // Tax Total
                writer.WriteStartElement("cac", "TaxTotal");
                writer.WriteStartElement("cac", "TaxSubtotal");
                writer.WriteElementString("cbc", "TaxableAmount", null, baseAmount.ToString("F2"));
                writer.WriteElementString("cbc", "TaxAmount", null, taxAmount.ToString("F2"));
                writer.WriteStartElement("cac", "TaxCategory");
                writer.WriteElementString("cbc", "Percent", null, details.TaxRate.ToString("F0"));
                writer.WriteStartElement("cac", "TaxScheme");
                writer.WriteElementString("cbc", "Name", null, "KDV");
                writer.WriteEndElement(); // TaxScheme
                writer.WriteEndElement(); // TaxCategory
                writer.WriteEndElement(); // TaxSubtotal
                writer.WriteEndElement(); // TaxTotal

                // Legal Monetary Total
                writer.WriteStartElement("cac", "LegalMonetaryTotal");
                writer.WriteElementString("cbc", "LineExtensionAmount", null, baseAmount.ToString("F2"));
                writer.WriteElementString("cbc", "TaxExclusiveAmount", null, baseAmount.ToString("F2"));
                writer.WriteElementString("cbc", "TaxInclusiveAmount", null, details.TotalAmount.ToString("F2"));
                writer.WriteElementString("cbc", "PayableAmount", null, details.TotalAmount.ToString("F2"));
                writer.WriteEndElement(); // LegalMonetaryTotal

                writer.WriteEndElement(); // Invoice
                writer.WriteEndDocument();
            }

            return fullPath;
        }

        public static bool SendToIntegrator(string xmlFilePath, string username, string password, out string errMessage)
        {
            // Bu metod Uyumsoft, Logo, QNB eFinans gibi bir entegratörün Web Servisine UBL XML göndermeyi simüle eder.
            try
            {
                // Entegratör SOAP/REST Client oluşturma simülasyonu
                // client.SendInvoice(username, password, xmlContent);
                
                errMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
    }
}
