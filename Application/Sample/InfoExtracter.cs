using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                ExtractInnerText(dataInfo, listInfo);
            }

            char[] separators = { ' ', '.', ',', '!', '?', '-' };
            var description = listInfo[0].Split(separators);
            List<string> descList = description.ToList<string>();

            for (int i = 0; i < descList.Count;)
            {
                if (descList[i] == "")
                {
                    descList.Remove(descList[i]);
                }
                else
                {
                    i++;
                }
            }

            //Zapobiegami wychodzeniu poza range na liście z wyrazami z opisu
            descList.Insert(0, "Start");
            descList.Add("End");
            ExtractInfoFromDescription(info, descList);
        }

        private static void ExtractInfoFromDescription(Dictionary<string, string> info, List<string> descList)
        {
            int rent = 0;
            bool roomInfo = false;

            //Dla każego wyrazu sprawdzamy czy nie wpisuje się w daną informację
            for (int i = 0; i < descList.Count; i++)
            {
                //GardenArea
                if (descList[i].ToLower().Contains("ogród") || descList[i].ToLower().Contains("ogródek"))
                {
                    info["GardenArea"] = "1";
                }
                //Balconies
                else if (descList[i].ToLower().Contains("balkon"))
                {
                    info["Balconies"] = "1";
                }
                //BasementArea
                else if (descList[i].ToLower().Contains("piwnica"))
                {
                    info["BasementArea"] = "1";
                }
                //OutdoorParkingPlaces
                else if (descList[i].ToLower().Contains("parking") || descList[i].ToLower().Contains("postoj"))
                {
                    if (descList[i - 1].ToLower().Contains("podziemny") || descList[i + 1].ToLower().Contains("podziemny"))
                        info["IndoorParkingPlaces"] = "1";
                    else
                        info["OutdoorParkingPlaces"] = "1";
                }
                //IndoorParkingPlaces
                else if (descList[i].ToLower().Contains("garaż"))
                {
                    info["IndoorParkingPlaces"] = "1";
                }
                //StreetName
                else if (descList[i].ToLower() == "ul")
                {
                    info["StreetName"] = descList[i + 1];
                }
                //Area
                else if (descList[i].ToLower() == "m")
                {
                    if (descList[i - 1].All(char.IsDigit))
                        info["Area"] = descList[i - 1];
                }
                else if (descList[i].ToLower().EndsWith("m"))
                {
                    var substring = descList[i].Substring(0, descList[i].Length - 1);
                    if (substring.All(char.IsDigit))
                        info["Area"] = substring;
                }
                //NumberOfRooms
                else if (descList[i].ToLower().Contains("pokoi") || descList[i].ToLower().Contains("pokoj") || descList[i].ToLower().Contains("pokój"))
                {
                    if (descList[i - 1].All(char.IsDigit))
                    {
                        info["NumberOfRooms"] = descList[i - 1];
                        roomInfo = true;
                    }
                    else if (!roomInfo)
                        info["NumberOfRooms"] = "1";
                }
                else if (descList[i].ToLower().Contains("kawalerk"))
                {
                    info["NumberOfRooms"] = "1";
                }
                //FloorNumber
                else if (descList[i].ToLower().Contains("piętr"))
                {
                    if (descList[i - 1].All(char.IsDigit))
                        info["FloorNumber"] = descList[i - 1];
                    else if (descList[i + 1].All(char.IsDigit))
                        info["FloorNumber"] = descList[i + 1];
                }
                else if (descList[i].ToLower().Contains("parter"))
                {
                    info["FloorNumber"] = "0";

                }
                //ResidentalRent
                else if (descList[i].ToLower() == "zl" || descList[i].ToLower() == "zł")
                {
                    if (descList[i - 1].All(char.IsDigit))
                        rent += Convert.ToInt32(descList[i - 1]);
                }
                else if (descList[i].ToLower().EndsWith("zl") || descList[i].ToLower().EndsWith("zł"))
                {
                    var substring = descList[i].Substring(0, descList[i].Length - 2);
                    if (substring.All(char.IsDigit))
                        rent += Convert.ToInt32(substring);
                }
                //YearOfConstruction
                else if (descList[i].ToLower() == "r")
                {
                    if (descList[i - 1].All(char.IsDigit))
                        info["YearOfConstruction"] = descList[i - 1];
                }
            }
            if (rent != 0)
                info["ResidentalRent"] = rent.ToString();
        }

        private static void ExtractTopBar(Dictionary<string, string> info, HtmlDocument doc)
        {
            var dataInfo = doc.DocumentNode.SelectNodes("//span[@class=\"velikost10\"]");

            var listInfo = new List<string>();
            if (dataInfo != null)
            {
                ExtractInnerText(dataInfo, listInfo);
            }
            string creationDate = "";
            if(listInfo.Count>2)
                creationDate = listInfo[2];
            else
                creationDate = listInfo[0];

            char[] separators = { '[', '.', ' ', ']' };
            var creationStrings = creationDate.Split(separators);
            creationDate = creationStrings[3] + "-" + creationStrings[4] + "-" + creationStrings[6];
            info.Add("CreationDateTime", creationDate);

            var rawDescriptionInfo = doc.DocumentNode.SelectNodes("//h1[@class=\"nadpis\"]");
            listInfo = new List<string>();
            if (rawDescriptionInfo != null)
            {
                ExtractInnerText(rawDescriptionInfo, listInfo);
            }
            info.Add("RawDescription", listInfo[0]);
        }

        private static void ExtractLeftColumn(Dictionary<string, string> info, HtmlDocument doc)
        {
            var contactInfo = doc.DocumentNode.SelectNodes("//td[@class=\"listadvlevo\"]");
            var listInfo = new List<string>();
            if (contactInfo != null)
            {
                ExtractInnerText(contactInfo, listInfo);
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
            var split = listInfo[2].Split(" ");
            info.Add("DetailedAddress", split[0]);
            info["City"] = ChangePolishCharacters(split[1]);
            char[] separators = { ' ', 'z', 'l', 'ł' };
            string tempGross = listInfo[4].Trim(separators);
            string[] totalGrossPriceSplit = tempGross.Split(" ");
            string totalGrossPrice = "";
            for (int i = 0; i<totalGrossPriceSplit.Length; i++)
            {
                totalGrossPrice += totalGrossPriceSplit[i];
            }
            if(totalGrossPrice.All(char.IsDigit))
                info["TotalGrossPrice"] = totalGrossPrice;
        }

        private static string ChangePolishCharacters(string s) //Usuwanie polskich znaków z nazwy miasta
        {
            s = s.ToUpper();
            string[,] exchangeableChar = 
            {
                { "Ą", "A" }, { "Ć", "C" }, { "Ę", "E" }, { "Ł", "L" }, { "Ń", "N" }, { "Ó", "O" }, { "Ś", "S" }, { "Ź", "Z" }, { "Ż", "Z" }
            };
            for (int i = 0; i < exchangeableChar.GetLength(0); i++)
            {
                s = s.Replace(exchangeableChar[i, 0], exchangeableChar[i, 1]);
            }

            return s;
        }

        private static void ExtractInnerText(HtmlNodeCollection nodeCol, List<string> info)
        {
            foreach (HtmlNode node in nodeCol)
            {
                if (node.ChildNodes.Count != 0)
                {
                    var newNodeCol = node.ChildNodes;
                    ExtractInnerText(newNodeCol, info);
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

