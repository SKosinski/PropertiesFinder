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

            foreach (var page in pages)
            {
                Dictionary<string, string> info = new Dictionary<string, string>();
                doc = web.Load(page);
                InfoExtracter.ExtractInfoFromPropertyPage(info, doc);

                Entry entry = new Entry
                {

                    //OfferDetails = new OfferDetails
                    //{
                    //    Url = page,
                    //    CreationDateTime = DateTime.Now,
                    //    LastUpdateDateTime = null,
                    //    OfferKind = OfferKind.RENTAL,
                    //    SellerContact = new SellerContact
                    //    {
                    //        Email = null,
                    //        Name = info[0],
                    //        Telephone = info[1]
                    //    },

                    //    IsStillValid = true
                    //},
                    //PropertyFeatures = new PropertyFeatures
                    //{
                    //    GardenArea = null,
                    //    Balconies = null,
                    //    BasementArea = null,
                    //    OutdoorParkingPlaces = null,
                    //    IndoorParkingPlaces = null,
                    //},
                    //PropertyAddress = new PropertyAddress
                    //{
                    //    City = null,
                    //    District = null,
                    //    StreetName = null,
                    //    DetailedAddress = null,
                    //},
                    //PropertyPrice = new PropertyPrice
                    //{
                    //    TotalGrossPrice = null,
                    //    PricePerMeter = null,
                    //    ResidentalRent = null,
                    //},

                    //RawDescription = "Kup Teraz!"
                };
                dumpEntries.Add(entry);
            }
            dump.Entries = dumpEntries;

            return dump;

        }

        private static void GetAllPropertyPages(List<string> pages, HtmlDocument doc)
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

