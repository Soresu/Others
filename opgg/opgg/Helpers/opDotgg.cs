using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using LeagueSharp;



namespace opgg.Helpers
{
    class opDotgg
    {
        public static CookieContainer container = new CookieContainer();
        public static List<player> players = new List<player>();
        public class player
        {
            public string name { get; set; }
            public string thisSeason { get; set; }
            public string lastSeason { get; set; }
            public string WinRatio { get; set; }
            public player(string name, string thisSeason, string lastSeason, string WinRatio)
            {
                this.name = name;
                this.thisSeason = thisSeason;
                this.lastSeason = lastSeason;
                this.WinRatio = WinRatio;
            }
        }


        public static string Init(string region, string user)
        {
            try
            {
                string referer = "http://" + region + ".op.gg/summoner/userName=" + user;
                string useragent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                Uri specuri = new Uri("http://" + GetRegion() + ".op.gg/summoner/ajax/spectator/userName=" + user + "&force=true");
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(specuri);
                webRequest.UserAgent = useragent;
                webRequest.KeepAlive = true;
                webRequest.CookieContainer = container;
                webRequest.Method = "GET";
                webRequest.Referer = referer;
                webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                container.Add(webResponse.Cookies);
                string response = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                return response;
            }
            catch (Exception e)
            {
                Console.Write("Exception: " + e);
                return "";
            }
            
        }

        public opDotgg(string region, string user)
        {
                var doc = new HtmlAgilityPack.HtmlDocument();
                var source = opDotgg.Init(region,user);
                doc.LoadHtml(source);
                var Summoners = doc.DocumentNode.SelectNodes("//tbody/tr");
                foreach (var Summoner in Summoners)
                {
                    var summonerName = Summoner.SelectSingleNode("//a[@class='summonerName']").InnerText;
                    var TierRank = Summoner.SelectSingleNode("//div[@class='TierRank']").InnerText;
                    var KDA = Summoner.SelectSingleNode("//td[@class='WinRatio']/div").InnerText;
                    var PreviousTierRank = Summoner.SelectSingleNode("//td[@class='PreviousTierRank']//img").InnerHtml;
                    players.Add(new player(summonerName, TierRank, PreviousTierRank, KDA));
                }
        }

        public static string GetRegion()
        {
            return "eune";
            if (Game.Region.ToLower().Contains("na"))
            {
                return "na";
            }
            if (Game.Region.ToLower().Contains("euw"))
            {
                return "euw";
            }
            if (Game.Region.ToLower().Contains("eun"))
            {
                return "eune";
            }
            if (Game.Region.ToLower().Contains("la1"))
            {
                return "lan";
            }
            if (Game.Region.ToLower().Contains("la2"))
            {
                return "las";
            }
            if (Game.Region.ToLower().Contains("tr"))
            {
                return "tr";
            }
            if (Game.Region.ToLower().Contains("br"))
            {
                return "br";
            }
            if (Game.Region.ToLower().Contains("ru"))
            {
                return "ru";
            }
            if (Game.Region.ToLower().Contains("kr"))
            {
                return "kr";
            }
            if (Game.Region.ToLower().Contains("oc1"))
            {
                return "oce";
            }
            return "";
        }

    }
}
