using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
using PmsSystem.Database;

namespace PmsSystem.Helpers
{
    public static class KbsHelper
    {
        public class KbsGuestInfo
        {
            public string IdentityNumber { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string FatherName { get; set; } = string.Empty;
            public string MotherName { get; set; } = string.Empty;
            public string BirthPlace { get; set; } = string.Empty;
            public DateTime BirthDate { get; set; }
            public string Gender { get; set; } = string.Empty;
            public string Nationality { get; set; } = "Türkiye";
            public string RoomNumber { get; set; } = string.Empty;
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
        }

        public static List<KbsGuestInfo> GetDailyCheckedInGuests(DateTime date)
        {
            var guests = new List<KbsGuestInfo>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Get guests currently checked-in or checked-in on the selected date
                string query = @"
                    SELECT 
                        c.IdentityNumber, c.FirstName, c.LastName, c.FatherName, c.MotherName, 
                        c.BirthPlace, c.BirthDate, c.Gender, c.Nationality, 
                        r.RoomNumber, res.CheckInDate, res.CheckOutDate
                    FROM RESERVATIONS res
                    JOIN CUSTOMERS c ON res.CustomerID = c.CustomerID
                    JOIN ROOMS r ON res.RoomID = r.RoomID
                    WHERE res.Status = 'CheckedIn' OR (res.CheckInDate = @date AND res.Status != 'Cancelled')";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@date", date.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guests.Add(new KbsGuestInfo
                            {
                                IdentityNumber = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                FirstName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                LastName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                FatherName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                MotherName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                BirthPlace = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                BirthDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                                Gender = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                Nationality = reader.IsDBNull(8) ? "Türkiye" : reader.GetString(8),
                                RoomNumber = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                CheckInDate = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10),
                                CheckOutDate = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11)
                            });
                        }
                    }
                }
            }
            return guests;
        }

        public static string GenerateKbsXml(List<KbsGuestInfo> guests, string tesisKodu, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string filename = $"KBS_{tesisKodu}_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            string fullPath = Path.Combine(outputDirectory, filename);

            XDocument xmlDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("TesisKimlikBildirim",
                    new XAttribute("TesisKodu", tesisKodu),
                    new XAttribute("GonderimTarihi", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("Misafirler",
                        GetGuestsXmlElements(guests)
                    )
                )
            );

            xmlDoc.Save(fullPath);
            return fullPath;
        }

        private static List<XElement> GetGuestsXmlElements(List<KbsGuestInfo> guests)
        {
            var elements = new List<XElement>();
            foreach (var guest in guests)
            {
                elements.Add(new XElement("Misafir",
                    new XElement("KimlikNo", guest.IdentityNumber),
                    new XElement("Adi", guest.FirstName),
                    new XElement("Soyadi", guest.LastName),
                    new XElement("BabaAdi", guest.FatherName),
                    new XElement("AnaAdi", guest.MotherName),
                    new XElement("DogumYeri", guest.BirthPlace),
                    new XElement("DogumTarihi", guest.BirthDate.ToString("yyyy-MM-dd")),
                    new XElement("Cinsiyet", guest.Gender),
                    new XElement("Uyruk", guest.Nationality),
                    new XElement("OdaNo", guest.RoomNumber),
                    new XElement("GirisTarihi", guest.CheckInDate.ToString("yyyy-MM-dd")),
                    new XElement("CikisTarihi", guest.CheckOutDate.ToString("yyyy-MM-dd"))
                ));
            }
            return elements;
        }
    }
}
