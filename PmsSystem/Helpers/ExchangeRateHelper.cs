using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace PmsSystem.Helpers
{
    public class ExchangeRateHelper
    {
        private static Dictionary<string, decimal> _rates = new Dictionary<string, decimal>();
        private static DateTime _lastFetched = DateTime.MinValue;

        public static async Task<Dictionary<string, decimal>> GetRatesAsync()
        {
            if (DateTime.Now.Subtract(_lastFetched).TotalHours < 1 && _rates.Count > 0)
                return _rates;

            try
            {
                using (var client = new HttpClient())
                {
                    // TCMB XML Kur Listesi
                    var response = await client.GetStringAsync("https://www.tcmb.gov.tr/kurlar/today.xml");
                    var xml = XDocument.Parse(response);

                    var newRates = new Dictionary<string, decimal>();
                    newRates["TRY"] = 1.0m;

                    foreach (var currency in xml.Descendants("Currency"))
                    {
                        string code = currency.Attribute("CurrencyCode")?.Value;
                        if (code == "USD" || code == "EUR")
                        {
                            string buyingStr = currency.Element("ForexBuying")?.Value;
                            if (!string.IsNullOrEmpty(buyingStr) && 
                                decimal.TryParse(buyingStr.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                            {
                                newRates[code] = rate;
                            }
                        }
                    }

                    if (newRates.ContainsKey("USD") && newRates.ContainsKey("EUR"))
                    {
                        _rates = newRates;
                        _lastFetched = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda boş dönme, en azından TRY kalsın
                if (_rates.Count == 0) _rates["TRY"] = 1.0m;
                Console.WriteLine("Döviz kuru çekme hatası: " + ex.Message);
            }

            return _rates;
        }

        public static string GetCurrencySymbol(string code)
        {
            return code switch
            {
                "USD" => "$",
                "EUR" => "€",
                _ => "₺"
            };
        }
    }
}
