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
            //Zbieramy info z poszczególnych elementów strony
            ExtractTopBar(info, doc);
            ExtractLeftColumn(info, doc);
            ExtractDescription(info, doc);
        }

        private static void ExtractDescription(Dictionary<string, string> info, HtmlDocument doc)
        {
            //Zbieram wszystkie wyrazy z opisu i konwertuje je na listę
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

            //Zapobiegam wychodzeniu poza range na liście z wyrazami z opisu
            descList.Insert(0, "Start");
            descList.Add("End");
            ExtractInfoFromDescription(info, descList);
        }

        private static void ExtractInfoFromDescription(Dictionary<string, string> info, List<string> descList)
        {
            int rent = 0;
            bool roomInfo = false, areaInfo = false;

            //Dla każego wyrazu sprawdzamy czy nie wpisuje się w daną informację
            for (int i = 0; i < descList.Count; i++)
            {
                var currentString = ChangePolishCharacters(descList[i]);
                //GardenArea
                if (currentString.Contains("OGROD"))
                {
                    info["GardenArea"] = "1";
                    string dictName = "GardenArea";
                    int tempInt = i;
                    CheckArea(info, descList, dictName, tempInt);
                }
                //Balconies
                else if (currentString.Contains("BALKON"))
                {
                    info["Balconies"] = "1";
                    if (descList[i - 1].All(char.IsDigit))
                        info["Balconies"] = descList[i - 1];
                }
                //BasementArea
                else if (currentString.Contains("PIWNICA"))
                {
                    info["BasementArea"] = "1";
                    string dictName = "BasementArea";
                    int tempInt = i;
                    CheckArea(info, descList, dictName, tempInt);
                }
                //OutdoorParkingPlaces
                else if (currentString.Contains("PARKING") || currentString.Contains("POSTOJ"))
                {
                    string parkingPlace = "a";
                    int tempInt = i;
                    if (ChangePolishCharacters(descList[i - 1]).Contains("PODZIEMNY"))
                    {
                        info["IndoorParkingPlaces"] = "1";
                        tempInt = i - 1;
                        parkingPlace = "IndoorParkingPlaces";
                        NumberOfParkingPlaces(info, descList, tempInt, parkingPlace); //Sprawdzam kilka wyrazów wstecz o ilość miejsc parkingowych
                    }
                    else if (ChangePolishCharacters(descList[i + 1]).Contains("PODZIEMNY"))
                    {
                        info["IndoorParkingPlaces"] = "1";
                        parkingPlace = "IndoorParkingPlaces";
                        NumberOfParkingPlaces(info, descList, tempInt, parkingPlace);
                    }
                    else
                    {
                        info["OutdoorParkingPlaces"] = "1";
                        parkingPlace = "OutdoorParkingPlaces";
                        NumberOfParkingPlaces(info, descList, tempInt, parkingPlace);
                    }
                }
                //IndoorParkingPlaces
                else if (currentString.Contains("GARAZ"))
                {
                    info["IndoorParkingPlaces"] = "1";
                    string parkingPlace = "IndoorParkingPlaces";
                    NumberOfParkingPlaces(info, descList, i, parkingPlace);
                }
                //StreetName
                else if (currentString == "UL")
                {
                    info["StreetName"] = descList[i + 1];
                }
                //Area
                else if (currentString == "M" && !areaInfo)
                {
                    if (descList[i - 1].All(char.IsDigit))
                    {
                        info["Area"] = descList[i - 1];
                        areaInfo = true;
                    }
                }
                else if (currentString.EndsWith("M") && !areaInfo)
                {
                    var substring = descList[i].Substring(0, descList[i].Length - 1);
                    if (substring.All(char.IsDigit))
                    {
                        info["Area"] = substring;
                        areaInfo = true;
                    }
                }
                //NumberOfRooms
                else if (currentString.Contains("POKOI") || currentString.Contains("POKOJ"))
                {
                    if (descList[i - 1].All(char.IsDigit))
                    {
                        info["NumberOfRooms"] = descList[i - 1];
                        roomInfo = true;
                    }
                    else if (!roomInfo)
                        info["NumberOfRooms"] = "1";
                }
                else if (currentString.Contains("KAWALERK"))
                {
                    info["NumberOfRooms"] = "1";
                }
                //FloorNumber
                else if (currentString.Contains("PIETR"))
                {
                    if (descList[i - 1].All(char.IsDigit))
                        info["FloorNumber"] = descList[i - 1];
                    else if (descList[i + 1].All(char.IsDigit))
                        info["FloorNumber"] = descList[i + 1];
                }
                else if (currentString.Contains("PARTER"))
                {
                    info["FloorNumber"] = "0";

                }
                //ResidentalRent
                else if (currentString == "ZL")
                {
                    if (descList[i - 1].All(char.IsDigit))
                        rent += Convert.ToInt32(descList[i - 1]);
                }
                else if (currentString.EndsWith("ZL"))
                {
                    var substring = descList[i].Substring(0, descList[i].Length - 2);
                    if (substring.All(char.IsDigit))
                        rent += Convert.ToInt32(substring);
                }
                //YearOfConstruction
                else if (currentString == "R")
                {
                    if (descList[i - 1].All(char.IsDigit))
                        info["YearOfConstruction"] = descList[i - 1];
                }
                else if (currentString.EndsWith("R"))
                {
                    var substring = descList[i].Substring(0, descList[i].Length - 1);
                    if (substring.All(char.IsDigit))
                        info["YearOfConstruction"] = substring;
                }
            }
            //Rent
            if (rent != 0)
                info["ResidentalRent"] = rent.ToString();
        }


        private static void ExtractTopBar(Dictionary<string, string> info, HtmlDocument doc)
        {
            //Creation Date Time
            var dataInfo = doc.DocumentNode.SelectNodes("//span[@class=\"velikost10\"]");

            var listInfo = new List<string>();
            if (dataInfo != null)
            {
                ExtractInnerText(dataInfo, listInfo);
                string creationDate = "";
                if (listInfo.Count > 2)
                {
                    for(int i=0; i<listInfo.Count; i++)
                    {
                        if (listInfo[i] == "TOP")
                        {
                            creationDate = listInfo[i+1];
                            break;
                        }
                        if (i == listInfo.Count-1)
                        {
                            creationDate = listInfo[0];
                        }
                    }
                }

                else
                    creationDate = listInfo[0];
                char[] separators = { '[', '.', ' ', ']' };
                var creationStrings = creationDate.Split(separators);
                creationDate = creationStrings[3] + "-" + creationStrings[4] + "-" + creationStrings[6];
                info["CreationDateTime"] = creationDate;
            }

            //Rental or Sale
            dataInfo = doc.DocumentNode.SelectNodes("//div[@class=\"drobky\"]");

            listInfo = new List<string>();
            if (dataInfo != null)
            {
                ExtractInnerText(dataInfo, listInfo);
                if (listInfo.Contains("Na wynajem"))
                    info["Rental"] = "WYNAJEM";
                else
                    info["Rental"] = "SPRZEDAZ";
            }

            //Raw description info
            var rawDescriptionInfo = doc.DocumentNode.SelectNodes("//h1[@class=\"nadpis\"]");

            listInfo = new List<string>();
            if (rawDescriptionInfo != null)
            {
                ExtractInnerText(rawDescriptionInfo, listInfo);
                info["RawDescription"] = listInfo[0];
            }
        }

        private static void ExtractLeftColumn(Dictionary<string, string> info, HtmlDocument doc)
        {
            //Name, telephone, Detailed Address, City, TotalGrossPrice
            var contactInfo = doc.DocumentNode.SelectNodes("//td[@class=\"listadvlevo\"]");
            var listInfo = new List<string>();
            if (contactInfo != null)
            {
                ExtractInnerText(contactInfo, listInfo);
                //wyrzucam niepotrzebne wyrazy
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

                info["Name"] = listInfo[0];
                info["Telephone"] = listInfo[1];
                var split = listInfo[2].Split(" ");
                info["DetailedAddress"] = split[0];
                info["City"] = ChangePolishCharacters(split[1]);

                char[] separators = { ' ', 'z', 'l', 'ł' };
                string totalGrossPrice = listInfo[4].Trim(separators);

                totalGrossPrice = totalGrossPrice.Replace(" ", string.Empty);

                if (totalGrossPrice.All(char.IsDigit))
                    info["TotalGrossPrice"] = totalGrossPrice;
            }
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

        public static void ExtractInnerText(HtmlNodeCollection nodeCol, List<string> info)
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

        private static void NumberOfParkingPlaces(Dictionary<string, string> info, List<string> descList, int tempInt, string parkingPlace)
        {
            for (int j = 0; j < 2; j++)
            {
                tempInt = tempInt - 1;
                if (ChangePolishCharacters(descList[tempInt]).Contains("MIEJSC"))
                {
                    if (descList[tempInt - 1].All(char.IsDigit))
                    {
                        info[parkingPlace] = descList[tempInt - 1];
                    }
                }
            }
        }

        private static void CheckArea(Dictionary<string, string> info, List<string> descList, string dictName, int tempInt) //Sprawdzam czy znajde wielkosc pomieszczenia wokol slowa klucza
        {
            tempInt = tempInt + 1;
            for (int j = 0; j <= 3; j++)
            {
                string tempString = ChangePolishCharacters(descList[tempInt]);
                if (tempString == "M")
                {
                    if (descList[tempInt - 1].All(char.IsDigit))
                        info[dictName] = descList[tempInt - 1];
                }
                else if (tempString.EndsWith("M"))
                {
                    var substring = tempString.Substring(0, tempString.Length - 1);
                    if (substring.All(char.IsDigit))
                        info[dictName] = substring;
                }
                tempInt--;
            }
        }
    }
}

