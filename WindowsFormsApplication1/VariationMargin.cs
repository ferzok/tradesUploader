using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using HtmlAgilityPack;

namespace WindowsFormsApplication1
{
    public class VariationMargin
    {
        private CommonFunctions fn = new CommonFunctions(_connstring);
        private static string _connstring;
        public delegate void MessageStreamDelegate(string str);
        public event MessageStreamDelegate MessageRecived = delegate { };
        public void PostLog(string message)
        {
            MessageRecived(message);
        }
        
        public VariationMargin(string s)
        {
            _connstring = s;
        }

        internal class FullTrade
        {
            public string Account { get; set; }
            public string Symbol { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
            public double Value { get; set; }
        }


        private double? GetVM(DateTime vmDate, string brocker)
        {
            var db = new EXANTE_Entities(_connstring);
            DateTime nextdate = vmDate.AddDays(1);
            double? sum =
                db.FT.Where(
                    o =>
                    (o.ReportDate >= vmDate.Date && o.ReportDate < nextdate.Date && o.valid == 1 && o.brocker == brocker))
                  .Sum(o => o.value);
            db.Dispose();
            return sum;
        }

        public void calcualteVM(DateTime VMDate, string Brocker)
        {
            DateTime TimeStart = DateTime.Now;
            PostLog("\r\n" + TimeStart + ": " + " Calculting VM for " + Brocker);
            List<FullTrade> listofaccountpositions = Getlistofaccountposition(VMDate, Brocker);
            listofaccountpositions = udpateVMforaccount(listofaccountpositions, VMDate, Brocker);
            DateTime TimeEndUpdating = DateTime.Now;
            PostLog("\r\n" + TimeEndUpdating + ": " + " End VM Calculation.VM = " +GetVM(VMDate, Brocker).ToString() + ". Time:" +
                                         (TimeStart - TimeEndUpdating).ToString());
        }

        private List<FullTrade> udpateVMforaccount(List<FullTrade> listofaccountpositions, DateTime VMDate,
                                                         string Brocker)
        {
            int i = 0;
            var db = new EXANTE_Entities(_connstring);
            DateTime nextdate = VMDate.AddDays(1);
            IQueryable<FT> listtodelete = from recon in db.FT
                                          where recon.ReportDate >= VMDate.Date && recon.ReportDate < nextdate.Date
                                                && recon.Type.Contains("VM") && recon.cp.Contains(Brocker)
                                          select recon;
            db.FT.RemoveRange(listtodelete);
            fn.SaveDBChanges(ref db);

            while (i < listofaccountpositions.Count)
            {
                FullTrade fullTrade = listofaccountpositions[i];
                double valueccy = 0;
                string counterccy = null;
                if (fullTrade.Value == 0)
                {
                    double currentAtomOfVM = getatomofVM(fullTrade.Symbol, VMDate,ref counterccy);
                    double priceFromDb = GetPrice(VMDate, fullTrade.Symbol);
                    double closeAtomOfVM = Math.Round(Math.Round(currentAtomOfVM*priceFromDb, 5), 2,
                                                      MidpointRounding.AwayFromZero);
                    fullTrade.Value =
                        Math.Round(
                            Math.Round(
                                fullTrade.Qty*
                                (closeAtomOfVM -
                                 Math.Round(Math.Round(currentAtomOfVM*fullTrade.Price, 5), 2,
                                            MidpointRounding.AwayFromZero)), 5), 2, MidpointRounding.AwayFromZero);
                    int j = i + 1;

                    while (j < listofaccountpositions.Count)
                    {
                        if ((listofaccountpositions[j].Value == 0) &&
                            (listofaccountpositions[j].Symbol == fullTrade.Symbol))
                        {
                            //double t0 = currentAtomOfVM*listofaccountpositions[j].Price;
                           // double t1 = Math.Round(currentAtomOfVM*listofaccountpositions[j].Price, 2,MidpointRounding.AwayFromZero);
                           // double t2 = closeAtomOfVM - t1;
                           // double t3 = listofaccountpositions[j].Qty*t2;
                           // double t4 = Math.Round(t3, 2);


                            listofaccountpositions[j].Value =
                                Math.Round(
                                    Math.Round(
                                        listofaccountpositions[j].Qty*
                                        Math.Round(
                                            Math.Round(
                                                (closeAtomOfVM -
                                                 Math.Round(
                                                     Math.Round(currentAtomOfVM*listofaccountpositions[j].Price, 5), 2,
                                                     MidpointRounding.AwayFromZero)), 5), 2,
                                            MidpointRounding.AwayFromZero), 5), 2, MidpointRounding.AwayFromZero);
                        }
                        j++;
                    }
                }
                i++;
                valueccy = GetValueccy(VMDate, fullTrade.Symbol);
                
                db.FT.Add(new FT
                    {
                        cp = Brocker,
                        brocker = Brocker,
                        ReportDate = VMDate,
                        account_id = fullTrade.Account,
                        timestamp = DateTime.Now,
                        symbol = fullTrade.Symbol,
                        ccy = "RUB",
                        value = fullTrade.Value,
                        valid = 1,
                        Type = "VM",
                        User = "parser",
                        Comment = " ",
                        Reference = null,
                        ValueDate = VMDate,
                        TradeDate = VMDate,
                        BOSymbol = fullTrade.Symbol,
                        GrossPositionIndicator = null,
                        JOURNALACCOUNTCODE = null,
                        ValueCCY = -Math.Round(fullTrade.Value*valueccy, 2, MidpointRounding.AwayFromZero),
                        counterccy = counterccy
                    });
            }
            fn.SaveDBChanges(ref db);
            db.Dispose();

            return listofaccountpositions;
        }

        private static double GetValueccy(DateTime VMDate, string symbol)
        {
            var db = new EXANTE_Entities(_connstring);

            int indexofOption = CustomIndexOf(symbol, '.', 3);
            string key = "";
            if (indexofOption > 0)
            {
                key = symbol.Substring(0, indexofOption) + ".";
            }
            else key = symbol;


            List<Mapping> map =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "OPEN" && ct.Type == "FORTS" && ct.BOSymbol == key
                 select ct).ToList();

            if ((map.Count > 0) && (map[0].Round == 1))
            {
                double? ccyrateFromDblinq = GetCcyrateFromDb(VMDate.Date, db, map[0].ccy1forFORTS);
                if (map[0].ccy2forFORTS!=null)
                {
                    ccyrateFromDblinq = ccyrateFromDblinq/GetCcyrateFromDb(VMDate.Date, db, map[0].ccy2forFORTS);
                }
                db.Dispose();
                return (double) (1/ccyrateFromDblinq);
            }
            else
            {
                db.Dispose();
                return 0;
            }
        }

        private double GetPrice(DateTime VMDate, string symbol)
        {
            var db = new EXANTE_Entities(_connstring);
            IQueryable<Price> pricelinq = from ct in db.Prices
                                          where
                                              ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker == symbol &&
                                              ct.Date == VMDate.Date
                                          select ct;
            if (pricelinq.Any())
            {
                var returnvalue = (double) pricelinq.ToList()[0].Price1;
                db.Dispose();
                return returnvalue;
            }
            else
            {
                db.Dispose();
                return UpdateFortsPrices(VMDate, symbol);
            }
        }

        private double getatomofVM(string symbol, DateTime VMDate,ref string counterccy)
        {
            var db = new EXANTE_Entities(_connstring);
            double atomvalue = 0;
            int indexofOption = CustomIndexOf(symbol, '.', 3);
            string key = symbol;
            if (indexofOption > 0)
            {
                key = symbol.Substring(0, indexofOption + 1);
            }
            List<Mapping> map =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "OPEN" && ct.Type == "FORTS" && ct.BOSymbol == key
                 select ct).ToList();
            if (map.Count == 1)
            {
                atomvalue = (double) (map[0].MtyPrice/map[0].MtyVolume);
                if (map[0].Round == 1)
                {
                    var ccyrateFromDb = GetCcyrateFromDb(VMDate, db, map[0].ccy1forFORTS);
                    atomvalue = Math.Round((atomvalue*ccyrateFromDb), 5, MidpointRounding.AwayFromZero);
                    if (map[0].ccy2forFORTS != null)
                    {
                        atomvalue = Math.Round((atomvalue/GetCcyrateFromDb(VMDate, db, map[0].ccy2forFORTS)), 5,
                                               MidpointRounding.AwayFromZero);
                        counterccy = map[0].ccy2forFORTS.Substring(3, 3);
                    }
                    else
                    {
                        counterccy = map[0].ccy1forFORTS.Substring(3, 3);
                    }
                }
            }
            db.Dispose();
            return atomvalue;
        }

        private static double GetCcyrateFromDb(DateTime VMDate, EXANTE_Entities db, string ccy)
        {
            IQueryable<Price> ccyrateFromDblinq =
                (from ct in db.Prices
                 where
                     ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker.Contains(ccy) &&
                     ct.Date == VMDate.Date
                 select ct);
            double ccyrateFromDb = 0;
            if (!ccyrateFromDblinq.Any())
            {
              //  updateFORTSccyrates(VMDate.ToString("dd.MM.yyyy"));
                ccyrateFromDb =
                    (double) (from ct in db.Prices
                              where
                                  ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker.Contains(ccy) &&
                                  ct.Date == VMDate.Date
                              select ct).ToList()[0].Price1;
            }
            else
            {
                ccyrateFromDb = (double) ccyrateFromDblinq.ToList()[0].Price1;
            }
            return ccyrateFromDb;
        }

        private List<FullTrade> Getlistofaccountposition(DateTime fortsDate, string Brocker)
        {
            var db = new EXANTE_Entities(_connstring);

            DateTime nextdate = fortsDate.AddDays(1);
            IQueryable<FullTrade> positionbefore =
                from ct in db.Ctrades
                where
                    ct.valid == 1 && ct.Date < fortsDate.Date && ct.symbol_id.Contains("FORTS") && ct.cp_id == Brocker
                group ct by new
                    {
                        ct.account_id,
                        ct.symbol_id
                    }
                into g
                where g.Sum(x => x.qty) != 0
                select
                    new FullTrade
                        {
                            Account = g.Key.account_id,
                            Symbol = g.Key.symbol_id,
                            Qty = (double) g.Sum(x => x.qty),
                            Price = 0,
                            Value = 0
                        };
            //      var listofTrades = positionbefore.ToList();
            var listofTrades = new List<FullTrade>(positionbefore.ToList());
            foreach (FullTrade listofTrade in listofTrades)
            {
                listofTrade.Price = GetFortsPrices(fortsDate, listofTrade.Symbol);
            }

            IQueryable<FullTrade> tradesToday =
                from ct in db.Ctrades
                where
                    ct.valid == 1 && ct.Date < nextdate.Date && ct.Date >= fortsDate.Date &&
                    ct.symbol_id.Contains("FORTS") && ct.cp_id == Brocker
                select
                    new FullTrade
                        {
                            Account = ct.account_id,
                            Symbol = ct.symbol_id,
                            Qty = (double) ct.qty,
                            Price = (double) ct.price,
                            Value = 0
                        };
            listofTrades.AddRange(tradesToday.ToList());
            db.Dispose();
            return listofTrades;
        }

        private double GetFortsPrices(DateTime fortsDate, string symbol)
        {
            var db = new EXANTE_Entities(_connstring);

            IQueryable<double?> lastprice =
                from ct in db.Prices
                where ct.Valid == 1 && ct.Date < fortsDate.Date && ct.Ticker == symbol
                orderby ct.Date descending
                select ct.Price1;
            if (!lastprice.Any())
            {
             PostLog("There is no prices for " + ": " + symbol + ". VM can be incorrect!");
                return 0;
            }
            else
            {
                return (double) lastprice.ToList()[0];
            }
        }

        public static int CustomIndexOf(string source, char toFind, int position)
        {
            int index = -1;
            for (int i = 0; i < position; i++)
            {
                index = source.IndexOf(toFind, index + 1);

                if (index == -1)
                    break;
            }

            return index;
        }
        private static List<List<string>> GetPage(string page, string rowsplitter, string cellsplitter,
                                                  List<string> unusefulltags)
        {
            string htmlCode;
            using (var client = new WebClient())
            {
                htmlCode = client.DownloadString(page);
            }

            string[] strArray = htmlCode.Split(new[] { rowsplitter }, StringSplitOptions.None);
            var result = new List<List<string>>();
            string row = null;
            for (int i = 0; i < strArray.Count(); i++)
            {
                row = strArray[i];
                int lastlength = 0;
                while (lastlength != row.Count())
                {
                    lastlength = row.Count();
                    for (int index = 0; index < unusefulltags.Count(); index++)
                    {
                        row = row.Replace(unusefulltags[index], "");
                    }
                }
                string[] rowlist = row.Split(new[] { cellsplitter }, StringSplitOptions.None);

                result.Add(new List<string>(rowlist));
            }
            return result;
        }

        private double UpdateFortsPrices(DateTime fortsDate, string currentInstrument)
        {
            const string initialstring = "http://moex.com/ru/derivatives/contractresults.aspx?code=";
            //  var listCurrentInstruments = getFORTSinstrument(fortsDate);
            var db = new EXANTE_Entities(_connstring);
            Dictionary<string, CommonFunctions.Map> map = fn.getSymbolMap("OPEN", "FORTS");
            var list = new List<string>();
            list.Add("><tr valign=top class=tr0><td>");
            list.Add("><td align='right' nowrap>");
            list.Add("><tr valign=top class=tr1><td>");
            list.Add("\xa0");
            list.Add("\r");
            list.Add("\n");
            list.Add("><tr class=tr1 align=right><td>");
            list.Add("><tr class=tr0 align=right><td>");
            double pricefw = 0;
            CommonFunctions.Map symbolvalue;
            int indexofOption = CustomIndexOf(currentInstrument, '.', 3);
            string key = "";
            if (indexofOption > 0)
            {
                key = currentInstrument.Substring(0, indexofOption + 1);
            }
            else key = currentInstrument;
            if (!map.TryGetValue(key, out symbolvalue))
            {
              PostLog("New Symbol: " + key);
            }
            else
            {
                string mappingsymbol = symbolvalue.BOSymbol;
                string fullmappingsymbol;
                var vd = (DateTime) symbolvalue.ValueDate;
                if (indexofOption > 0)
                {
                    fullmappingsymbol = mappingsymbol + currentInstrument[indexofOption + 1] + "A " +
                                        currentInstrument.Substring(indexofOption + 2);
                }
                else
                {
                    fullmappingsymbol = mappingsymbol;
                }
                List<List<string>> webpage = GetPage(initialstring + fullmappingsymbol, "/tr", "</td", list);
                pricefw = getpricefromhtml(webpage, fortsDate);

                if (pricefw == -1)
                {
                    fullmappingsymbol = mappingsymbol + currentInstrument[indexofOption + 1] + "A" + currentInstrument.Substring(indexofOption + 2);
                    webpage = GetPage(initialstring + fullmappingsymbol, "/tr", "</td", list);
                    pricefw = getpricefromhtml(webpage, fortsDate);
                }
                if (pricefw != -1)
                {
                    db.Prices.Add(new Price
                        {
                            Ticker = currentInstrument,
                            Tenor = vd,
                            Price1 = pricefw,
                            Date = fortsDate,
                            Type = "FORTS",
                            Timestamp = DateTime.Now,
                            Valid = 1,
                            Username = "parser"
                        });
                }
                fn.SaveDBChanges(ref db);
            }
            db.Dispose();
            return pricefw;
        }

        private double getpricefromhtml(List<List<string>> pagelist, DateTime fortsDate)
        {
            int index = 1;
            string Datestring = fortsDate.ToString("dd.MM.yyyy");
            while ((index < pagelist.Count) && (pagelist[index].Count < 4 || pagelist[index][3].IndexOf("CSV") == -1))
                index++;
            index++;

            while ((index < pagelist.Count()) && (pagelist[index][0].IndexOf(Datestring) == -1)) index++;
            string temp = "";
            if (index < pagelist.Count())
            {
                temp = pagelist[index][2].Replace(',', '.');
                temp = temp.Replace("<td>", "");
                temp = temp.Replace(">", "");
                temp = temp.Replace(" ", "");
                temp = temp.Replace("В", "");
            }
            if (temp != "")
            {
                if (temp == "<tdalign='right'-")
                {
                    return 0;
                }
                else
                {
                    return Convert.ToDouble(temp);
                }
            }
            else return -1;
        }

        private List<string> getFORTSinstrument(DateTime fortsDate)
        {
            var db = new EXANTE_Entities(_connstring);
            DateTime nextdate = fortsDate.AddDays(1);
            IQueryable<string> contractrow =
                from ct in db.Ctrades
                where
                    ct.valid == 1 && ct.Date >= fortsDate.Date && ct.Date < (nextdate.Date) &&
                    ct.symbol_id.Contains(".FORTS.")
                select ct.symbol_id;
            return contractrow.Distinct().ToList();
        }

        public void updateFORTSccyrates(DateTime DateCalculation)
        {
            DateTime TimeStart = DateTime.Now;
            PostLog(TimeStart + ": " + "Getting ccy prices from MOEX");
            string Date = DateCalculation.ToString("yyyy-MM-dd");

            // const string initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx?currency=";
            const string initialstring = "http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=";
            // http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=USD/RUB&moment_start=2014-07-24&moment_end=2014-07-24
            var listccy = new List<string>();
            listccy.Add("USD/RUB");
            listccy.Add("EUR/RUB");
            listccy.Add("USD/JPY");
            listccy.Add("JPY/RUB");
            var db = new EXANTE_Entities(_connstring);
            foreach (string ccy in listccy)
            {
                string ccystring = initialstring + ccy + "&moment_start=" + Date + "&Date&moment_end=" + Date;
                var doc = new XmlDocument();

                doc.Load(ccystring);
                XmlNode upnode = doc.SelectSingleNode("rtsdata");
                string temp = "";
                if (upnode != null)
                {
                    temp = upnode.SelectSingleNode("rates").FirstChild.Attributes[1].Value;
                }

                db.Prices.Add(new Price
                    {
                        Ticker = ccy.Replace("/", ""),
                        Tenor =
                            DateTime.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Price1 = Convert.ToDouble(temp),
                        Date =
                            DateTime.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Type = "FORTS",
                        Timestamp = DateTime.Now,
                        Valid = 1,
                        Username = "parser"
                    });
            }
            fn.SaveDBChanges(ref db);
            db.Dispose();

            DateTime TimeEndUpdating = DateTime.Now;
           PostLog(TimeEndUpdating + ": " + "CCY FORTS rates for " + Date + "uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());
        }
        private static void GetHtmlPage(string url)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
            HtmlNodeCollection tags = doc.DocumentNode.SelectNodes("//abc//tag");
        }
        private void updatePrices()
        {
            string initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx";
            GetHtmlPage(initialstring);
            //    var forwardstring = "http://moex.com/ru/derivatives/contractresults.aspx?code=";
            var list = new List<string>();
            list.Add("><tr valign=top class=tr0><td>");
            list.Add("><td align='right' nowrap>");
            list.Add("><tr valign=top class=tr1><td>");
            list.Add("\xa0");
            list.Add("\r");
            list.Add("\n");
            list.Add("><tr class=tr1 align=right><td>");
            list.Add("><tr class=tr0 align=right><td>");
            List<List<string>> currate = GetPage(initialstring, "/tr", "</td", list);
            int index = 15;
            //  while ((index < currate.Count()) && (currate[index][0].IndexOf("Курс основного") == -1)) index++;
            while ((index < currate.Count()) && (currate[index][0].IndexOf("18.08.2014") == -1)) index++;
            string temp = "";
            if (index != currate.Count() + 1)
            {
                temp = currate[index][2].Replace(',', '.');
                temp = temp.Replace("<td>", "");
                temp = temp.Replace(">", "");
                temp = temp.Replace(" ", "");
            }
        }
    }
}