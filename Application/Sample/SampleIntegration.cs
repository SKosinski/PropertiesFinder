using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace Application.Sample
{
    public class SampleIntegration : IWebSiteIntegration
    {
        /*
         * Zmiany:
         * 
         * na stronie bazos jedynymi informacjami gwarantowanymi dla ogłoszenią są:
         * - Imię właściciela
         * - Telefon Właściciela
         * - Miasto z numerem pocztowym
         * 
         * Reszta informacji może zawierać się jedynie w opisie ogłoszenia, ale nie musi
         * W związku z tym słownik w którym umieszczam wyciągnięte informacje ze strony posiada domyslne wartości (tworzone w CreateDictionary)
         * Gdyż takie nienullowalne wartości jak Area, NumberOfRooms czy StreetName niekoniecznie znajdują się w opisie.
         * 
         * Dodałem również miasto "NIEZNANY" w razie gdyby miasto podane przez właściciela nie znajdowało się na liście
         * 
         */
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public SampleIntegration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                Url = "https://nieruchomosci.bazos.pl/wynajem/mieszkania/",
                Name = "Bazos Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = false,
                    HomeRental = false,
                    HouseSale = false,
                    HouseRental = false
                }
            };
        }

        public Dump GenerateDump()
        {
            List<string> pages = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(WebPage.Url);
            GetAllPropertyPages(pages, doc);
            //Tutaj w normalnej sytuacji musimy ściągnąć dane z konkretnej strony, przeparsować je i dopiero wtedy zapisać do modelu Dump

            var dump = new Dump()
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = new List<Entry>()
            };

            List<Entry> dumpEntries = new List<Entry>();
            // Dla każdego ogłoszenia ze strony głównej tworzymy nowe Entry i dodajemy do Dumpa
            foreach (var page in pages)
            {
                Dictionary<string, string> info = CreateDictionary();

                doc = web.Load(page);
                InfoExtracter.ExtractInfoFromPropertyPage(info, doc);
                decimal ppm;
                PolishCity city;
                
                if(Enum.IsDefined(typeof(PolishCity), info["City"]))
                {
                    city = (PolishCity)System.Enum.Parse(typeof(PolishCity), info["City"].ToUpper());
                }
                else
                {
                    city = 0;
                }

                if (info["Area"] != "0")
                {
                    ppm = Convert.ToDecimal(info["TotalGrossPrice"]) / Convert.ToDecimal(info["Area"]);
                }
                else
                {
                    ppm = 0;
                }

                Entry entry = new Entry
                {

                    OfferDetails = new OfferDetails
                    {
                        Url = page,
                        CreationDateTime = DateTime.Now,
                        LastUpdateDateTime = null,
                        OfferKind = OfferKind.RENTAL,
                        SellerContact = new SellerContact
                        {
                            Email = null,
                            Name = info["Name"],
                            Telephone = info["Telephone"]
                        },

                        IsStillValid = true
                    },
                    PropertyDetails = new PropertyDetails
                    {
                        Area = Convert.ToDecimal(info["Area"]),
                        NumberOfRooms = Convert.ToInt32(info["NumberOfRooms"]),
                        FloorNumber = Convert.ToInt32(info["FloorNumber"]),
                        YearOfConstruction = Convert.ToInt32(info["YearOfConstruction"]),
                    },
                    PropertyFeatures = new PropertyFeatures
                    {
                        GardenArea = Convert.ToDecimal(info["GardenArea"]),
                        Balconies = Convert.ToInt32(info["Balconies"]),
                        BasementArea = Convert.ToDecimal(info["BasementArea"]),
                        OutdoorParkingPlaces = Convert.ToInt32(info["OutdoorParkingPlaces"]),
                        IndoorParkingPlaces = Convert.ToInt32(info["IndoorParkingPlaces"]),
                    },
                    PropertyAddress = new PropertyAddress
                    {
                        City = city,
                        District = null,
                        StreetName = info["StreetName"],
                        DetailedAddress = info["DetailedAddress"],
                    },
                    PropertyPrice = new PropertyPrice
                    {
                        TotalGrossPrice = Convert.ToDecimal(info["TotalGrossPrice"]),
                        PricePerMeter = ppm,
                        ResidentalRent = Convert.ToInt32(info["ResidentalRent"]),
                    },
                    RawDescription = info["RawDescription"]
                };

                dumpEntries.Add(entry);
            }
            dump.Entries = dumpEntries;

            return dump;
        }

        private static Dictionary<string, string> CreateDictionary()
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            info = new Dictionary<string, string>
            {
                {"City", "0"},
                {"LastUpdateDateTime", "0"},
                {"Email", "0"},
                {"Area", "0"},
                {"NumberOfRooms", "0"},
                {"FloorNumber", "0"},
                {"YearOfConstruction", "0"},
                {"GardenArea", "0"},
                {"Balconies", "0"},
                {"BasementArea", "0"},
                {"OutdoorParkingPlaces", "0"},
                {"IndoorParkingPlaces", "0"},
                {"District", "Nieznany"},
                {"StreetName", "Nieznany"},
                {"TotalGrossPrice", "0"},
                {"PricePerMeter", "0"},
                {"ResidentalRent", "0"},
            };
            return info;
        }

        private static void GetAllPropertyPages(List<string> pages, HtmlDocument doc) //Zbieramy linki wszystkich ogłoszeń ze strony głównej
        {
            var pagesNodes = doc.DocumentNode.SelectNodes("//span[@class=\"nadpis\"]");
            foreach (HtmlNode node in pagesNodes)
            {
                var address = node.FirstChild.GetAttributeValue("href", string.Empty);
                address = "https://nieruchomosci.bazos.pl" + address;
                pages.Add(address);
            }
        }
    }
}

//foreach (var item in info)
//{
//    if(item.Contains("Imię:") || item.Contains("Telefon:") || item.Contains("Lokalizacja:") || item.Contains("Widziało:") || item.Contains("Cena:"))
//    {
//        info.Remove(item);
//    }
//    else
//    {
//        Console.WriteLine(item);
//    }
//}
//var bnode = node.Decendants().Select
//var anode = bnode.SelectNodes("//b").First();
//Console.WriteLine(anode.InnerText);

//var name = node.SelectNodes("//b").First().InnerText;
//var nodes = doc.DocumentNode.ChildNodes;
//foreach(HtmlAgilityPack.HtmlNode node in nodes)
//{
//    var infoTable = node.SelectSingleNode("//td[@class=\"listadvlevo\"]");
//    var bnode = infoTable.SelectSingleNode("./b");
//    var anode = bnode.SelectSingleNode("./a");
//    Console.WriteLine(anode.InnerText);
//}

//{
//    new Entry
//    {
//        OfferDetails = new OfferDetails
//        {
//            Url = $"{WebPage.Url}/{randomValue}",
//            CreationDateTime = DateTime.Now,
//            OfferKind = OfferKind.SALE,
//            SellerContact = new SellerContact
//            {
//                Email = "okazje@mieszkania.pl"
//            },
//            IsStillValid = true
//        },
//        RawDescription = "Kup Teraz!",
//    },
//    new Entry
//    {
//        OfferDetails = new OfferDetails
//        {
//            Url = $"{WebPage.Url}/{(randomValue+1)%10}",
//            CreationDateTime = DateTime.Now,
//            OfferKind = OfferKind.RENTAL,
//            SellerContact = new SellerContact
//            {
//                Email = "przeceny@mieszkania.pl"
//            },
//            IsStillValid = true
//        },
//        RawDescription = "NAPRAWDĘ WARTO!!",
//    }
//}

