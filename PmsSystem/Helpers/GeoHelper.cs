 using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PmsSystem.Helpers
{
    public static class GeoHelper
    {
        private static Dictionary<string, string[]>? _ilceler;
        private static readonly object _lock = new object();
        private static Task<Dictionary<string, string[]>>? _fetchTask;

        public static Task<Dictionary<string, string[]>> PreloadGeoDataAsync()
        {
            lock (_lock)
            {
                if (_ilceler != null) 
                    return Task.FromResult(_ilceler);

                if (_fetchTask == null)
                {
                    _fetchTask = FetchGeoDataAsync();
                }
                return _fetchTask;
            }
        }
        
        private static async Task<Dictionary<string, string[]>> FetchGeoDataAsync()
        {
            try
            {
                using var client = new HttpClient();
                string url = "https://turkiyeapi.dev/api/v1/provinces";
                string json = await client.GetStringAsync(url);

                var jsonDoc = JsonDocument.Parse(json);
                var data = jsonDoc.RootElement.GetProperty("data");

                var tempDict = new Dictionary<string, string[]>();

                foreach (var province in data.EnumerateArray())
                {
                    string provinceName = province.GetProperty("name").GetString() ?? "";
                    
                    var districtsArray = province.GetProperty("districts").EnumerateArray()
                                         .Select(d => d.GetProperty("name").GetString() ?? "")
                                         .ToArray();

                    tempDict[provinceName] = districtsArray;
                }

                // Öncelikli olarak 3 büyük şehri (İstanbul, Ankara, İzmir) en başa koyalım
                var sortedDict = new Dictionary<string, string[]>();
                
                string[] prioritisedCities = { "İstanbul", "Ankara", "İzmir" };
                foreach (var city in prioritisedCities)
                {
                    if (tempDict.ContainsKey(city))
                    {
                        sortedDict[city] = tempDict[city];
                    }
                }

                foreach (var kvp in tempDict.OrderBy(x => x.Key))
                {
                    if (!sortedDict.ContainsKey(kvp.Key))
                    {
                        sortedDict[kvp.Key] = kvp.Value;
                    }
                }

                _ilceler = sortedDict;
            }
            catch (Exception)
            {
                // Fallback veriler (Eğer API'ye ulaşılamazsa veya internet yoksa yedek)
                _ilceler = new Dictionary<string, string[]> {
                    {"İstanbul", new[]{"Kadıköy","Beşiktaş","Fatih","Üsküdar","Bakırköy","Şişli","Beyoğlu","Ataşehir","Maltepe","Pendik","Kartal","Tuzla","Sarıyer","Beylikdüzü","Esenyurt","Bağcılar","Bahçelievler","Zeytinburnu","Avcılar","Sultangazi","Esenler","Gaziosmanpaşa","Başakşehir","Küçükçekmece","Büyükçekmece","Sancaktepe","Sultanbeyli","Çekmeköy","Ümraniye","Beykoz","Arnavutköy","Çatalca","Silivri","Şile","Adalar"}},
                    {"Ankara", new[]{"Çankaya","Keçiören","Mamak","Yenimahalle","Etimesgut","Sincan","Altındağ","Pursaklar","Polatlı","Gölbaşı"}},
                    {"İzmir", new[]{"Konak","Karşıyaka","Bornova","Buca","Bayraklı","Çiğli","Gaziemir","Narlıdere","Balçova","Karabağlar"}},
                    {"Antalya", new[]{"Muratpaşa","Konyaaltı","Kepez","Aksu","Döşemealtı","Alanya","Manavgat","Serik","Kemer","Kaş","Kumluca","Finike","Demre","Gazipaşa","Elmalı","Akseki","Gündoğmuş","İbradı","Korkuteli"}},
                    {"Bursa", new[]{"Osmangazi","Nilüfer","Yıldırım","Gemlik","Mudanya","İnegöl","Kestel","Gürsu","Orhangazi","İznik","Karacabey","Mustafakemalpaşa","Yenişehir"}},
                };
            }

            return _ilceler;
        }

        public static Dictionary<string, string[]> GetCachedIlceler()
        {
            if (_ilceler != null) return _ilceler;
            return PreloadGeoDataAsync().GetAwaiter().GetResult();
        }
    }
}
