using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sample
{
    class InfoExtracter
    {
        public static void ExtractInfoFromPropertyPage(Dictionary<string, string> info, HtmlDocument doc)
        {
            ExtractTopBar(info, doc);
            ExtractLeftColumn(info, doc);
            ExtractDescription(info, doc);
        }

        private static void ExtractDescription(Dictionary<string, string> info, HtmlDocument doc)
        {
            var dataInfo = doc.DocumentNode.SelectNodes("//div[@class=\"popis\"]");

            var listInfo = new List<string>();
            if (dataInfo != null)
            {
                ExtractInfo(dataInfo, listInfo);
            }

        }

        private static void ExtractTopBar(Dictionary<string, string> info, HtmlDocument doc)
        {
            var dataInfo = doc.DocumentNode.SelectNodes("//span[@class=\"velikost10\"]");

            var listInfo = new List<string>();
            if (dataInfo != null)
            {
                ExtractInfo(dataInfo, listInfo);
            }


            string creationDate = listInfo[2];
            char[] separators = { '[', '.', ' ', ']' };
            var creationStrings = creationDate.Split(separators);
            creationDate = creationStrings[3] + "-" + creationStrings[4] + "-" + creationStrings[6];
            info.Add("CreationDateTime", creationDate);

            var rawDescriptionInfo = doc.DocumentNode.SelectNodes("//h1[@class=\"nadpis\"]");
            listInfo = new List<string>();
            if (rawDescriptionInfo != null)
            {
                ExtractInfo(rawDescriptionInfo, listInfo);
            }
            info.Add("RawDescription", listInfo[0]);
        }

        private static void ExtractLeftColumn(Dictionary<string, string> info, HtmlDocument doc)
        {
            var contactInfo = doc.DocumentNode.SelectNodes("//td[@class=\"listadvlevo\"]");
            var listInfo = new List<string>();
            if (contactInfo != null)
            {
                ExtractInfo(contactInfo, listInfo);
            }

            for (int i = 0; i < listInfo.Count;)
            {
                if (listInfo[i].Contains("Imię:") || listInfo[i].Contains("Telefon:") || listInfo[i].Contains("Lokalizacja:") || listInfo[i].Contains("Widziało:") || listInfo[i].Contains("Cena:"))
                {
                    listInfo.Remove(listInfo[i]);
                }
                else
                {
                    i++;
                }
            }
            info.Add("Name", listInfo[0]);
            info.Add("Telephone", listInfo[1]);
            var split = listInfo[3].Split(" ");
            info.Add("DetailedAddress", split[0]);
            info.Add("City", split[1]);
            info.Add("TotalGrossPrice", listInfo[4]);
        }

        private static void ExtractInfo(HtmlNodeCollection nodeCol, List<string> info)
        {
            foreach (HtmlNode node in nodeCol)
            {
                if (node.ChildNodes.Count != 0)
                {
                    var newNodeCol = node.ChildNodes;
                    ExtractInfo(newNodeCol, info);
                }
                else
                {
                    if (node.InnerText != "\r\n" && node.InnerText != "")
                        info.Add(node.InnerText);
                }
            }
        }
    }
}

