using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace opgg.Helpers
{
    class OPGGV2
    {
        public static bool disabletext;
        public static List<Info> Ranks { get; set; }
        public static CookieContainer container = new CookieContainer();
        public class Info
        {
            public String Name { get; set; }
            public String Ranking { get; set; }
            public String lpamount { get; set; }
            public String winratio { get; set; }
            public String kdaratio { get; set; }
            public Color winratiocolor { get; set; }
            public Color kdaratiocolor { get; set; }
            public String herohandle { get; set; }
        }


        public static string rank = "";


        public OPGGV2()
        {
            try
            {
                Ranks = new List<Info>();
                var region = "eune";
                var player = "Sonmester";
                var source = GetSource(region, player);
                if (region != "Not Supported" && source.Length > 5)
                {
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(source);
                    var Summoners = doc.DocumentNode.SelectNodes("//tbody/tr[contains( @class, 'Champion ')]");
                    foreach (var Summoner in Summoners)
                    {
                        Info info = new Info();
                        info.Name = Summoner.SelectSingleNode("//a[@class='summonerName']").InnerText;
                        Console.Write(Summoner.SelectSingleNode("//a[@class='summonerName']").InnerText);
                        //TierRank: Platinum 1 (3 LP) or Level 30
                        var TierRank = Summoner.SelectNodes("//div[@class='TierRank']")[1].InnerText;
                        if (TierRank.Contains("Level"))
                        {
                            info.Ranking = "Unranked L" + TierRank.Substring(7, TierRank.Length - 7).TrimEnd().TrimStart();
                            info.lpamount = "-";
                        }
                        else
                        {
                            info.Ranking = new Regex("(.*?)\\s").Match(TierRank).ToString();
                            info.lpamount = new Regex("[3](.*?)LP").Match(TierRank).ToString();
                        }
                        //WinRatio: 50%
                        info.winratio = Summoner.SelectSingleNode("//td[@class='WinRatio']/div").InnerText;
                        info.winratiocolor = colorwinratio(double.Parse(info.winratio.Remove(info.winratio.Length - 1)));
                        info.kdaratio = Summoner.SelectSingleNode("//td[@class='ChampionInfo']/div[@class='KDA']/span").InnerText;
                        info.kdaratiocolor = Color.White;
                        info.herohandle = "asd";
                        Ranks.Add(info);
                        
                    }
                }
                else
                {
                   
                }
            }
            catch (Exception exception)
            {
                System.IO.File.WriteAllText(@"C:\Users\error.txt", exception.ToString());
            }

        }


        public static string GetSource(string region, string player)
        {
            try
            {
                string referer = "http://" + region + ".op.gg/summoner/userName=" + player;
                string useragent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36";
                Uri specuri = new Uri("http://" + region + ".op.gg/summoner/ajax/spectator/userName=" + player + "&force=true");
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


        public static Color colorwinratio(double winratiocolor)
        {
            //  int winratiocolor = 0;

            if (winratiocolor >= 90 && winratiocolor <= 100)
            {
                return Color.Orange; //Challenger
            }
            if (winratiocolor >= 80 && winratiocolor < 90)
            {
                return Color.LimeGreen; //Master
            }
            if (winratiocolor >= 70 && winratiocolor < 80)
            {
                return Color.Cyan; //Diamond
            }
            if (winratiocolor >= 60 && winratiocolor < 70)
            {
                return Color.DeepSkyBlue; //Platinum
            }
            if (winratiocolor >= 50 && winratiocolor < 60)
            {
                return Color.Gold; //Gold
            }
            if (winratiocolor >= 40 && winratiocolor < 50)
            {
                return Color.Silver; //Silver
            }
            if (winratiocolor >= 30 && winratiocolor < 40)
            {
                return Color.SandyBrown; //Bronze
            }
            if (winratiocolor < 30)
            {
                return Color.Red; //Red
            }
            return Color.White;
        }

        public static Color rankcolor(string rank)
        {
            if (rank.ToLower().Contains("error"))
            {
                return Color.SandyBrown;
            }
            if (rank.ToLower().Equals("unranked"))
            {
                return Color.SandyBrown;
            }
            if (rank.Contains("Bronze"))
            {
                return Color.Brown;
            }
            if (rank.Contains("Silver"))
            {
                return Color.Silver;
            }
            if (rank.Contains("Gold"))
            {
                return Color.Gold;
            }
            if (rank.Contains("Platinum"))
            {
                // other codes to try: #06828E, #06828E, #33D146, #33D146, #55AC82
                return Color.LawnGreen;
            }
            if (rank.Contains("Diamond"))
            {
                //other codes: #38B0D5, #38B0D5, #2389B1, #3A7FBA
                return Color.DeepSkyBlue;
            }
            if (rank.Contains("Master"))
            {
                // other codes: #B6F3EC, #B6F3EC, #A8E0D5, #73847E, #E5BF50
                return Color.LimeGreen;
            }
            if (rank.Contains("Challenger"))
            {
                //other codes: #DB910D, #DF9B42, #12607E
                return Color.Orange;
            }
            return Color.White;
        }

        public static Color rankincolor(string rank)
        {
            if (rank.ToLower().Contains("error"))
            {
                return Color.Red;
            }
            if (rank.ToLower().Contains("unranked"))
            {
                return Color.SandyBrown;
            }
            if (rank.ToLower().Contains("bronze"))
            {
                return Color.Brown;
            }
            if (rank.ToLower().Contains("silver"))
            {
                return Color.Silver;
            }
            if (rank.ToLower().Contains("gold"))
            {
                return Color.Gold;
            }
            if (rank.ToLower().Contains("platinum"))
            {
                // other codes to try: #06828E, #06828E, #33D146, #33D146, #55AC82
                return Color.DeepSkyBlue;
            }
            if (rank.ToLower().Contains("diamond"))
            {
                //other codes: #38B0D5, #38B0D5, #2389B1, #3A7FBA
                return Color.Cyan;
            }
            if (rank.ToLower().Contains("master"))
            {
                // other codes: #B6F3EC, #B6F3EC, #A8E0D5, #73847E, #E5BF50
                return Color.LimeGreen;
            }
            if (rank.ToLower().Contains("challenger"))
            {
                //other codes: #DB910D, #DF9B42, #12607E
                return Color.Orange;
            }
            return Color.Red;
        }
    }
}
