using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace WindowsFormsApplication1
{
    public class ABN
    {
        private static string _currentConnection;
        public delegate void MessageStreamDelegate(string str);
        public event MessageStreamDelegate MessageRecived = delegate { };
        private CommonFunctions fn = new CommonFunctions(_currentConnection);
        public void PostLog(string message)
        {
            MessageRecived(message);
        }
        public ABN(string s)
        {
            _currentConnection = s;
        }


        private static double GetValueFromCliff(string row)
        {
            double volumelong = double.Parse(row.Substring(0, 10) + "." + row.Substring(10, 2),
                                             CultureInfo.InvariantCulture);
            string volumeshort = row.Substring(13, 10);
            double resvolume = volumelong -
                               double.Parse(row.Substring(13, 10) + "." + row.Substring(23, 2),
                                            CultureInfo.InvariantCulture);
            return resvolume;
        }

        private void updateBalance(List<string> rowlist, DateTime reportdate)
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<string> cpidfromDb = from cp in db.DailyChecks
                                            where cp.Table == "Daily" && cp.date == reportdate
                                            select cp.status;
            var listforDb = new List<ABN_cashposition>();
            foreach (string row in rowlist)
            {
                string value = row.Substring(90, 18);
                value = value.Substring(0, value.Count() - 2) + "." + value.Substring(value.Count() - 2, 2);
                db.ABN_cashposition.Add(new ABN_cashposition
                {
                    ReportDate = reportdate,
                    Currency = row.Substring(54, 3),
                    Value = row[108] != 'C'
                                ? -1 * double.Parse(value, CultureInfo.InvariantCulture)
                                : double.Parse(value, CultureInfo.InvariantCulture),
                    valid = 1,
                    User = "parser",
                    TimeStamp = DateTime.Now,
                    Description = row.Substring(109, 40).Trim()
                });
            }
            fn.SaveDBChanges(ref db);
            db.Dispose();
        }


        private List<CpTrade> ExtractTradesFromCliff(List<string> rowlist, Dictionary<string, CommonFunctions.Map> symbolmap)
        {
            var allfromfile = new List<CpTrade>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime)fn.getDatefromString(rowlist[0].Substring(6, 8));
            foreach (string row in rowlist)
            {
                string typeoftrade = row.Substring(60, 2);
                DateTime? tradedate = fn.getDatefromString(row.Substring(582), true) ??
                                      fn.getDatefromString(row.Substring(295), true);
                string symbol = row.Substring(66, 6).Trim();
                string type = row.Substring(60, 2);
                CommonFunctions.Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;
                int round = 10;
                string symbol_id = symbol + type;
                DateTime? valuedate = fn.getDatefromString(row.Substring(303)) ?? fn.getDatefromString(row.Substring(72));

                if (typeoftrade == "FU")
                {
                    symbol_id = symbol_id + Convert.ToDateTime(valuedate).ToShortDateString();
                }

                if (symbolmap.TryGetValue(symbol_id, out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    BoSymbol = symbolvalue.BOSymbol;
                    round = (int)symbolvalue.Round;
                }

                double exchfee = double.Parse(row.Substring(137, 10) + '.' + row.Substring(147, 2),
                                              CultureInfo.InvariantCulture);
                if (row.Substring(149, 1) == "D") exchfee = -exchfee;
                string exchfeeccy = row.Substring(150, 3);

                double fee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2),
                                          CultureInfo.InvariantCulture);
                if (row.Substring(165, 1) == "D") fee = -fee;
                string clearingfeeccy = row.Substring(166, 3);
                double value;
                double transacPrice;
                if (typeoftrade != "FU")
                {
                    value = double.Parse(row.Substring(276, 16) + '.' + row.Substring(292, 2),
                                         CultureInfo.InvariantCulture);
                    if (row.Substring(294, 1) == "D") value = -value;
                    transacPrice =
                        Math.Round(
                            double.Parse(row.Substring(360, 8) + "." + row.Substring(368, 7),
                                         CultureInfo.InvariantCulture) * (double)MtyPrice, round);
                }
                else
                {
                    transacPrice =
                        Math.Round(
                            double.Parse(row.Substring(230, 8) + "." + row.Substring(238, 7),
                                         CultureInfo.InvariantCulture) * (double)MtyPrice, round);
                    value = -Math.Round(GetValueFromCliff(row.Substring(112)) * (double)MtyVolume * transacPrice, 10);
                }
                string Id = null;
                /*   if (type == "FW")
        {
            BoSymbol = BoSymbol + valuedate.Value.ToShortDateString();
            Id = BoSymbol + (GetValueFromCliff(row.Substring(112)) * MtyVolume).ToString() + transacPrice.ToString();
        }*/

                allfromfile.Add(new CpTrade
                {
                    ReportDate = reportdate,
                    TradeDate = typeoftrade == "FU"
                                    ? fn.getDatefromString(row.Substring(496), true)
                                    : fn.getDatefromString(row.Substring(582), true) ??
                                      fn.getDatefromString(row.Substring(295), true),
                    BrokerId = "ABN",
                    Symbol = symbol,
                    Type = (row.Substring(405, 4) == "FW-E")
                               ? "FW-E"
                               : type,
                    Qty = GetValueFromCliff(row.Substring(112)) * MtyVolume,
                    Price = transacPrice,
                    ValueDate = valuedate,
                    cp_id = getCPid(row.Substring(54, 6).Trim(), cpdic),
                    ExchangeFees = exchfee,
                    Fee = fee,
                    Id = Id,
                    BOSymbol = BoSymbol,
                    BOTradeNumber = null,
                    value = value,
                    Timestamp = DateTime.UtcNow,
                    valid = 1,
                    username = "cliffparser",
                    //  FullId = null,
                    BOcp = null,
                    exchangeOrderId = null,
                    TypeOfTrade = row.Substring(108, 2),
                    Comment = null,
                    ExchFeeCcy = exchfeeccy,
                    ClearingFeeCcy = clearingfeeccy,
                    ccy = row.Substring(105, 3)
                });
            }
            return allfromfile;
        }

        private DateTime ExtractPositionFromCliff(List<string> rowlist)
        {
            Dictionary<string, CommonFunctions.Map> symbolmap = fn.getMap("ABN");
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = fn.getDatefromString(rowlist[0].Substring(6, 8));
            foreach (string row in rowlist)
            {
                string type = row.Substring(60, 2);
                string symbol = row.Substring(66, 6).Trim();
                string symbol_id = symbol + type;
                DateTime? valuedate;
                double transacPrice = 0;
                int round = 10;
                DateTime tradedate;
                double qty;
                string ccy;
                string optiontype = null;
                double? strike = null;
                if (type == "FU")
                {
                    valuedate = fn.getDatefromString(row.Substring(72, 8));
                    symbol_id = symbol_id + Convert.ToDateTime(valuedate).ToShortDateString();
                    tradedate = (DateTime)fn.getDatefromString(row.Substring(183, 8));
                    transacPrice =
                        Math.Round(
                            double.Parse(row.Substring(108, 8) + "." + row.Substring(116, 7),
                                         CultureInfo.InvariantCulture), round);
                    qty = GetValueFromCliff(row.Substring(124));
                    ccy = row.Substring(105, 3);
                }
                else
                {
                    if (type == "OP")
                    {
                        valuedate = fn.getDatefromString(row.Substring(73, 8));
                        tradedate = (DateTime)fn.getDatefromString(row.Substring(184, 8));
                        strike =
                            Math.Round(
                                double.Parse(row.Substring(81, 8) + "." + row.Substring(89, 7),
                                             CultureInfo.InvariantCulture), round);
                        transacPrice =
                            Math.Round(
                                double.Parse(row.Substring(169, 8) + "." + row.Substring(177, 7),
                                             CultureInfo.InvariantCulture), round);
                        ccy = row.Substring(121, 3);
                        qty = GetValueFromCliff(row.Substring(145));
                        optiontype = row.Substring(72, 1);
                    }
                    else
                    {
                        valuedate = fn.getDatefromString(row.Substring(72, 8));
                        tradedate = (DateTime)fn.getDatefromString(row.Substring(209, 8));
                        transacPrice =
                            Math.Round(
                                double.Parse(row.Substring(182, 8) + "." + row.Substring(190, 7),
                                             CultureInfo.InvariantCulture), round);
                        ccy = row.Substring(117, 3);
                        qty = GetValueFromCliff(row.Substring(120));
                    }
                }
                CommonFunctions.Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;

                if (symbolmap.TryGetValue(symbol_id, out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    BoSymbol = symbolvalue.BOSymbol;
                    round = (int)symbolvalue.Round;
                }
                else
                {
                    PostLog("\r\n" + "There is no BO Symbol for this id:" + symbol_id);
                }
                transacPrice = Math.Round(transacPrice * (double)MtyPrice, round);
                qty = (double)(qty * MtyVolume);
                double value = -Math.Round(qty * transacPrice, round);

                db.CpPositions.Add(new CpPosition
                {
                    ReportDate = reportdate,
                    Brocker = "ABN",
                    TradeDate = tradedate,
                    Symbol = symbol,
                    Qty = qty,
                    Price = transacPrice,
                    BOSymbol = BoSymbol,
                    ValueDate = valuedate,
                    cp_id = getCPid(row.Substring(54, 6).Trim(), cpdic),
                    Type = type,
                    OptionType = optiontype,
                    Strike = strike,
                    Value = value,
                    ccy = ccy,
                    Timestamp = DateTime.UtcNow,
                    valid = 1,
                    username = "cliffparser"
                });
            }
            fn.SaveDBChanges(ref db);
            return (DateTime) reportdate;
        }

        private List<CpTrade> ExtractOptionTradesFromCliff(List<string> rowlist, Dictionary<string, CommonFunctions.Map> symbolmap)
        {
            var allfromfile = new List<CpTrade>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime)fn.getDatefromString(rowlist[0].Substring(6, 8));
            foreach (string row in rowlist)
            {
                string code = row.Substring(124, 2);
                string typeoftrade = row.Substring(60, 2);
                DateTime? tradedate =fn.getDatefromString(row.Substring(554), true) ??
                                     fn.getDatefromString(row.Substring(562), true);
                string symbol = row.Substring(66, 6).Trim();
                string Counterparty = row.Substring(54, 6).Trim();
                DateTime? valuedate = fn.getDatefromString(row.Substring(73, 8).Trim());
                string type = row.Substring(72, 1);
                double strike = double.Parse(row.Substring(81, 8) + '.' + row.Substring(89, 7),
                                             CultureInfo.InvariantCulture);
                double volumelong = double.Parse(row.Substring(128, 10) + '.' + row.Substring(138, 2),
                                                 CultureInfo.InvariantCulture);
                double volume = volumelong -
                                double.Parse(row.Substring(141, 10) + '.' + row.Substring(151, 2),
                                             CultureInfo.InvariantCulture);
                double price = double.Parse(row.Substring(247, 8) + '.' + row.Substring(255, 7),
                                            CultureInfo.InvariantCulture);

                CommonFunctions.Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;


                if (symbolmap.TryGetValue(symbol + "OP", out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    //     BoSymbol = symbolvalue.BOSymbol + "." + getLetterOfMonth(valuedate.Value.Month) + valuedate.Value.Year + "." + type + strike * MtyPrice;
                }
                string symbol_id = symbol + "." + type + strike;

                double exchfee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2),
                                              CultureInfo.InvariantCulture);
                if (row.Substring(165, 1) == "D") exchfee = -exchfee;
                string exchfeeccy = row.Substring(166, 3);

                double fee = double.Parse(row.Substring(169, 10) + '.' + row.Substring(179, 2),
                                          CultureInfo.InvariantCulture);
                if (row.Substring(181, 1) == "D") fee = -fee;
                string clearingfeeccy = row.Substring(182, 3);

                allfromfile.Add(new CpTrade
                {
                    ReportDate = reportdate,
                    TradeDate = tradedate,
                    BrokerId = "ABN",
                    Symbol = symbol_id,
                    Type = typeoftrade,
                    Qty = volume * MtyVolume,
                    Price = price, // * MtyPrice,
                    ValueDate = valuedate,
                    cp_id = getCPid(Counterparty, cpdic),
                    ExchangeFees = exchfee,
                    Fee = fee,
                    Id = null,
                    BOSymbol = null,
                    BOTradeNumber = null,
                    value = null,
                    Timestamp = DateTime.UtcNow,
                    valid = 1,
                    username = "cliffparser",
                    //  FullId = null,
                    BOcp = null,
                    exchangeOrderId = null,
                    TypeOfTrade = code,
                    Comment = null,
                    ExchFeeCcy = exchfeeccy,
                    ClearingFeeCcy = clearingfeeccy,
                    ccy = row.Substring(121, 3)
                });
            }
            return allfromfile;
        }
        public void GetABNPos(Dictionary<string, List<string>> cliffdict, DateTime reportdate)
        {
            List<string> rowlist;
            DateTime TimeFuturepositionStart = DateTime.Now;
            PostLog("\r\n" + TimeFuturepositionStart + ": " + "start ABN future position parsing");

            if (cliffdict.TryGetValue("320", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeFutureParsing = DateTime.Now;
            PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN future position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeFutureParsing - TimeFuturepositionStart).ToString() + "s");

            DateTime TimeStockPositionStart = DateTime.Now;
            PostLog("\r\n" + TimeStockPositionStart + ": " + "start ABN stock position parsing");
            if (cliffdict.TryGetValue("420", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeStockParsing = DateTime.Now;
            PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN stock position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeStockParsing - TimeStockPositionStart).ToString() + "s");

            DateTime TimeOptionPositionStart = DateTime.Now;
            PostLog("\r\n" + TimeOptionPositionStart + ": " + "start ABN option position parsing");
            if (cliffdict.TryGetValue("220", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeOptionParsing = DateTime.Now;
            PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN stock position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeOptionParsing - TimeOptionPositionStart).ToString() + "s");
        }


        public void ABNParser(DateTime reportdate,bool CliffCheckBox,string filename)
        {
            List<CpTrade> allfromfile = null;
            var db = new EXANTE_Entities(_currentConnection);
            Dictionary<string, CommonFunctions.Map> symbolmap = fn.getMap("ABN");
            if (CliffCheckBox)
            {
                Dictionary<string, List<string>> cliffdict = LoadCliff(filename, reportdate);
                List<string> rowlist;

                DateTime TimeUpdateBalanceStart = DateTime.Now;
                if (cliffdict.TryGetValue("610", out rowlist)) updateBalance(rowlist, reportdate);
                DateTime TimeFutureParsing = DateTime.Now;
                PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +"Update Balance Completed. Time:" +
                                             (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");

                if (cliffdict.TryGetValue("310", out rowlist))
                    allfromfile = ExtractTradesFromCliff(rowlist, symbolmap);
                DateTime TimeStockParsing = DateTime.Now;
                PostLog("\r\n" + TimeStockParsing.ToLongTimeString() + ": " +
                                             "Future parsing Completed. Time:" +
                                             (TimeStockParsing - TimeFutureParsing).ToString() + "s");

                if (cliffdict.TryGetValue("410", out rowlist))
                    allfromfile.AddRange(ExtractTradesFromCliff(rowlist, symbolmap));
                DateTime TimeOptionParsing = DateTime.Now;
                PostLog("\r\n" + TimeOptionParsing.ToLongTimeString() + ": " +
                                             "Stock parsing Completed. Time:" +
                                             (TimeOptionParsing - TimeStockParsing).ToString() + "s");

                if (cliffdict.TryGetValue("210", out rowlist))
                    allfromfile.AddRange(ExtractOptionTradesFromCliff(rowlist, symbolmap));
                DateTime TimeEndOptionParsing = DateTime.Now;
                PostLog("\r\n" + TimeEndOptionParsing.ToLongTimeString() + ": " +
                                             "Option parsing Completed. Time:" +
                                             (TimeEndOptionParsing - TimeOptionParsing).ToString() + "s");

                GetABNPos(cliffdict, reportdate);
                DateTime TimePositionParsing = DateTime.Now;
                PostLog("\r\n" + TimeOptionParsing.ToLongTimeString() + ": " +
                                             "Position parsing Completed. Time:" +
                                             (TimePositionParsing - TimeEndOptionParsing).ToString() + "s");
                if (cliffdict.TryGetValue("600", out rowlist))
                {
                    reportdate = getcashmovements(rowlist);
                }
                DateTime TimeFTParsing = DateTime.Now;
                PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                             "FT parsing completed for " + reportdate.ToShortDateString() + ". Time:" +
                                             (TimeFTParsing - TimePositionParsing).ToString() + "s");
            }
            else
            {
                allfromfile = ExtractTradesFromXml(symbolmap,filename);
            }
            foreach (CpTrade tradeIndex in allfromfile)
            {
                db.CpTrades.Add(tradeIndex);
            }
            fn.SaveDBChanges(ref db);
        }

        private DateTime GetValueDate(XmlNode itemNode)
        {
            if (itemNode.SelectSingleNode("SettlementDate") == null)
            {
                if (itemNode.SelectSingleNode("Product/Expiry") == null)
                {
                    return DateTime.ParseExact(itemNode.SelectSingleNode("ValueDate").InnerText, "yyyyMMdd",
                                               CultureInfo.CurrentCulture);
                }
                else
                {
                    return DateTime.ParseExact(itemNode.SelectSingleNode("Product/Expiry").InnerText, "yyyyMMdd",
                                               CultureInfo.CurrentCulture);
                }
            }
            else
            {
                return DateTime.ParseExact(itemNode.SelectSingleNode("SettlementDate").InnerText, "yyyyMMdd",
                                           CultureInfo.CurrentCulture);
            }
        }

        private List<CpTrade> ExtractTradesFromXml(Dictionary<string, CommonFunctions.Map> symbolmap,string filename)
        {
            //todo: unzip file
            var doc = new XmlDocument();
            //doc.Load(@"C:\20140214.xml");
            doc.Load(filename);
            var db = new EXANTE_Entities(_currentConnection);
            var allfromfile = new List<CpTrade>();
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
              int row = -1;
            {
                foreach (XmlNode mainnode in doc.DocumentElement.ChildNodes)
                {
                   foreach (XmlNode itemNode in mainnode.SelectNodes("UnsettledMovement"))
                    {
                        XmlNodeList list = itemNode.ChildNodes;
                        string MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //    if (new [] {"01", "23", "24"}.Contains(MovementCode)){
                        row++;
                        int Pricemty = 1;
                        /*  todo Решить задачу с комиссиями  
                                  var ExchangeFees = selectSingleNode != null && (selectSingleNode.InnerText == "D")
                                                              ? -1*Convert.ToDouble(itemNode.SelectSingleNode("ExchangeFee/Value").InnerText)
                                                              : Convert.ToDouble(itemNode.SelectSingleNode("ExchangeFee/Value").InnerText);
                                       var Fee = singleNode != null && (singleNode.InnerText == "D")
                                                     ? -1*Convert.ToDouble(itemNode.SelectSingleNode("ClearingFee/Value").InnerText)
                                                     : Convert.ToDouble(itemNode.SelectSingleNode("ClearingFee/Value").InnerText)*/
                        string typeOftrade = GetTypeOfTradeFromXml(itemNode);
                        if (typeOftrade == "FW" || typeOftrade == "FX")
                        {
                            if (itemNode.SelectSingleNode("TransactionPriceCurrency/CurrencyPricingUnit") !=
                                null)
                            {
                                Pricemty = 10000 / Convert.ToInt32(itemNode.SelectSingleNode(
                                    "TransactionPriceCurrency/CurrencyPricingUnit").InnerText);
                            }
                        }

                        string symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + typeOftrade;
                        CommonFunctions.Map symbolvalue;
                        string bosymbol = "";
                        if (symbolmap.TryGetValue(symbolid, out symbolvalue))
                        {
                            bosymbol = symbolvalue.BOSymbol;
                        }
                        else
                        {
                            bosymbol = "";
                        }


                        allfromfile.Add(new CpTrade
                        {
                            ReportDate =
                                DateTime.ParseExact(itemNode.SelectSingleNode("ProcessingDate").InnerText,
                                                    "yyyyMMdd", CultureInfo.CurrentCulture),
                            TradeDate = (itemNode.SelectSingleNode("TimeStamp") != null)
                                            ? Convert.ToDateTime(
                                                itemNode.SelectSingleNode("TimeStamp").InnerText)
                                            : DateTime.ParseExact(
                                                itemNode.SelectSingleNode("TransactionDate").InnerText,
                                                "yyyyMMdd", CultureInfo.CurrentCulture),
                            BrokerId = "test",
                            Symbol = itemNode.SelectSingleNode("Product/Symbol").InnerText,
                            Type = typeOftrade,
                            Qty = (itemNode.SelectSingleNode("QuantityShort") == null)
                                      ? Convert.ToInt64(itemNode.SelectSingleNode("QuantityLong").InnerText)
                                      : -1 * Convert.ToInt64(itemNode.SelectSingleNode("QuantityShort").InnerText),
                            Price = (itemNode.SelectSingleNode("TransactionPrice") != null)
                                        ? (double)
                                          decimal.Round(
                                              Convert.ToDecimal(
                                                  itemNode.SelectSingleNode("TransactionPrice").InnerText) /
                                              Pricemty, 8)
                                        : 0,
                            ValueDate = GetValueDate(itemNode),
                            cp_id =
                                getCPid(
                                    itemNode.SelectSingleNode("OppositeParty/OppositePartyCode").InnerText,
                                    cpdic),
                            ExchangeFees = 0,
                            Fee = 0,
                            Id = null,
                            BOSymbol = (bosymbol == "") ? null : bosymbol,
                            BOTradeNumber = null,
                            value = (itemNode.SelectSingleNode("EffectiveValue/ValueDC") != null)
                                        ? (itemNode.SelectSingleNode("EffectiveValue/ValueDC").InnerText ==
                                           "D")
                                              ? -1 *
                                                Convert.ToDouble(
                                                    itemNode.SelectSingleNode("EffectiveValue/Value")
                                                            .InnerText)
                                              : Convert.ToDouble(
                                                  itemNode.SelectSingleNode("EffectiveValue/Value")
                                                          .InnerText)
                                        : 0,
                            Timestamp = DateTime.UtcNow,
                            valid = 1,
                            username = "xmlparser",
                            //  FullId = null,
                            BOcp = null,
                            exchangeOrderId = null,
                            TypeOfTrade = MovementCode,
                            Comment = (itemNode.SelectSingleNode("TransactionOrigin") != null)
                                          ? itemNode.SelectSingleNode("TransactionOrigin").InnerText
                                          : ""
                        });
                    }

                    foreach (XmlNode itemNode in mainnode.SelectNodes("FutureMovement"))
                    {
                        XmlNodeList list = itemNode.ChildNodes;
                        string MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //  if (new[] { "01", "23", "24" }.Contains(MovementCode)){
                        int Pricemty = 1;
                        double price = Convert.ToDouble(itemNode.SelectSingleNode("TransactionPrice").InnerText) /
                                       Pricemty;
                        long qty = (itemNode.SelectSingleNode("QuantityShort") == null)
                                       ? Convert.ToInt64(itemNode.SelectSingleNode("QuantityLong").InnerText)
                                       : -1 * Convert.ToInt64(itemNode.SelectSingleNode("QuantityShort").InnerText);
                        string symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + "FU" +
                                          Convert.ToDateTime(GetValueDate(itemNode)).ToShortDateString();
                        CommonFunctions.Map symbolvalue;
                        string bosymbol = "";
                        if (symbolmap.TryGetValue(symbolid, out symbolvalue))
                        {
                            bosymbol = symbolvalue.BOSymbol;
                        }
                        else
                        {
                            bosymbol = "";
                        }

                        allfromfile.Add(new CpTrade
                        {
                            ReportDate =
                                DateTime.ParseExact(itemNode.SelectSingleNode("ProcessingDate").InnerText,
                                                    "yyyyMMdd", CultureInfo.CurrentCulture),
                            TradeDate = Convert.ToDateTime(itemNode.SelectSingleNode("TimeStamp").InnerText),
                            BrokerId = "test",
                            Symbol = itemNode.SelectSingleNode("Product/Symbol").InnerText,
                            Type = GetTypeOfTradeFromXml(itemNode),
                            Qty = qty,
                            Price = price,
                            ValueDate = GetValueDate(itemNode),
                            cp_id =
                                getCPid(
                                    itemNode.SelectSingleNode("OppositeParty/OppositePartyCode").InnerText,
                                    cpdic),
                            ExchangeFees = 0,
                            Fee = 0,
                            Id = null,
                            BOSymbol = bosymbol,
                            BOTradeNumber = null,
                            value =
                                -Convert.ToInt64(itemNode.SelectSingleNode("Tradingunit").InnerText == "D") *
                                price * qty,
                            Timestamp = DateTime.UtcNow,
                            valid = 1,
                            username = "xmlparser",
                            //  FullId = null,
                            BOcp = null,
                            exchangeOrderId = null,
                            TypeOfTrade = MovementCode,
                            Comment = (itemNode.SelectSingleNode("TransactionOrigin") != null)
                                          ? itemNode.SelectSingleNode("TransactionOrigin").InnerText
                                          : ""
                        });
                        //if 01   }
                    }
                }
            }
            return allfromfile;
        }
        private DateTime getcashmovements(List<string> rowlist)
        {
            var db = new EXANTE_Entities(_currentConnection);
            DateTime reportdate = DateTime.ParseExact(rowlist[0].Substring(6, 8), "yyyyMMdd",
                                                      CultureInfo.InvariantCulture);
            Dictionary<string, CommonFunctions.Map> bomap = fn.getMap("ABN");
            CommonFunctions.Map symbolvalue;
            foreach (string row in rowlist)
            {
                string symbol = row.Substring(62, 6).Trim();
                string symbol2 = row.Substring(106, 4).Trim();
                /*      if (Convert.ToInt64(row.Substring(135, 9).Trim()) == 587856)
                      {
                          var t = 1;
                      }*/
                string type = row.Substring(60, 2).Trim();
                string bosymbol = "";
                if (bomap.TryGetValue(symbol + type, out symbolvalue))
                {
                    bosymbol = symbolvalue.BOSymbol;
                }
                db.FT.Add(new FT
                {
                    cp = row.Substring(54, 6).TrimEnd(),
                    brocker = "ABN",
                    ReportDate =
                        DateTime.ParseExact(row.Substring(6, 8), "yyyyMMdd",
                                            CultureInfo.InvariantCulture),
                    account_id = null,
                    timestamp = DateTime.Now,
                    symbol = symbol,
                    ccy = row.Substring(68, 3).Trim(),
                    value = row[105] != 'C'
                                ? -1 * Convert.ToDouble(row.Substring(87, 18)) / 100
                                : Convert.ToDouble(row.Substring(87, 18)) / 100,
                    valid = 1,
                    Type = type,
                    User = "parser",
                    Comment = row.Substring(111, 24).TrimEnd(' '),
                    Reference = Convert.ToInt64(row.Substring(135, 9).Trim()),
                    ValueDate =
                        DateTime.ParseExact(row.Substring(79, 8), "yyyyMMdd",
                                            CultureInfo.InvariantCulture),
                    TradeDate =
                        DateTime.ParseExact(row.Substring(71, 8), "yyyyMMdd",
                                            CultureInfo.InvariantCulture),
                    BOSymbol = bosymbol,
                    GrossPositionIndicator = row.Substring(110, 1),
                    JOURNALACCOUNTCODE = row.Substring(106, 4),
                    ValueCCY = null,
                    counterccy = null
                });
            }
            fn.SaveDBChanges(ref db);
            return reportdate;
        }

        private string GetTypeOfTradeFromXml(XmlNode itemNode)
        {
            switch (itemNode.SelectSingleNode("Product/ProductGroupName").InnerText)
            {
                case "Equities":
                    return "ST";
                case "Futures":
                    return "FU";
                case "Others":
                    if (itemNode.SelectSingleNode("TransactionOrigin") != null)
                    {
                        if (itemNode.SelectSingleNode("TransactionOrigin").InnerText == "FW-E")
                        {
                            return "FW-E";
                        }
                        return itemNode.SelectSingleNode("TransactionOrigin").InnerText;
                    }
                    else
                    {
                        switch (itemNode.SelectSingleNode("TransactionType").InnerText)
                        {
                            case "FORWARD CONF":
                                return "FW";
                            case "FX CONF":
                                return "FX";
                            case "TRADE":
                                if (itemNode.SelectSingleNode("Depot/DepotId") != null)
                                {
                                    if (itemNode.SelectSingleNode("Depot/DepotId").InnerText == "METALS")
                                    {
                                        return "METALS";
                                    }
                                    return "Others";
                                }
                                else
                                {
                                    return "Others";
                                }
                            default:
                                return "Others";
                        }
                    }
                default:
                    return itemNode.SelectSingleNode("Product/ProductGroupName").InnerText;
            }
        }

        
        public void ABNFTParsing(string oFilename, DateTime reportdate)
        {
            var abn = new ABN(_currentConnection);
            Dictionary<string, List<string>> cliffdict = abn.LoadCliff(oFilename, reportdate);
            List<string> rowlist;
            DateTime TimeUpdateBalanceStart = DateTime.Now;
            PostLog("\r\n" + TimeUpdateBalanceStart + ": " + "start FT parsing reconciliation");
            if (cliffdict.TryGetValue("600", out rowlist))
            {
                reportdate = getcashmovements(rowlist);
            }
            DateTime TimeFutureParsing = DateTime.Now;
            PostLog("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "FT parsing completed for " + reportdate.ToShortDateString() + ". Time:" +
                                  (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");
        }

        private Dictionary<string, Contract> getContractDetails()
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<Contract> cpfromDb = from cp in db.Contracts
                                            select cp;
            return cpfromDb.ToDictionary(k => k.id, k => k);
        }


        public void UpdateABNSheet(DateTime reportdate)
        {
            DateTime prevreportdate = reportdate.AddDays(-3);
            var ts = new TimeSpan(20, 00, 0);
            prevreportdate = prevreportdate.Date + ts;
            var db = new EXANTE_Entities(_currentConnection);
            var cplist = new List<string> { "LEK", "CQG", "FASTMATCH", "CURRENEX", "EXANTE", "" };
            DateTime TimeStart = DateTime.Now;
            PostLog(TimeStart + ": " + "Preparing ABN View");
            DateTime nextdate = reportdate.AddDays(1);
            IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                where
                                                    cptrade.valid == 1 && cptrade.BrokerId == "ABN" &&
                                                    cptrade.ReportDate >= reportdate.Date &&
                                                    cptrade.ReportDate < (nextdate.Date)
                                                select cptrade;
            List<CpTrade> cptradelist = cptradefromDb.ToList();
            Dictionary<int, string> cpmappings = GetCPmapping();
            Dictionary<string, Contract> contractdetailstable = getContractDetails();
            var updatelist = new List<ABNReconResult>();
            IQueryable<Ctrade> queryable = from ct in db.Ctrades
                                           where
                                               ct.valid == 1 && ct.BOtradeTimestamp >= prevreportdate &&
                                               ct.BOtradeTimestamp < (nextdate.Date)
                                           select ct;
            Dictionary<long?, Ctrade> boTradeslist = queryable.ToDictionary(k => k.tradeNumber, k => k);


            foreach (CpTrade cpTrade in cptradelist)
            {
                string cpname;
                if (!cpmappings.TryGetValue((int)cpTrade.cp_id, out cpname))
                {
                    PostLog("\r\n" + "There is no counterparty for this id");
                }
                Contract contractDetails = null;
                double leverage = 1;
                if ((cpTrade.BOSymbol == null) ||
                    (!contractdetailstable.TryGetValue(cpTrade.BOSymbol, out contractDetails)))
                {
                    PostLog("\r\n" + "There is no id in contracts for " + cpTrade.Symbol + " " +
                                          cpTrade.Type + " " + cpTrade.TypeOfTrade + " " + cpTrade.FullId);
                }
                else leverage = (double)contractDetails.Leverage;
                string account = null;
                double bosum = 0;
                string ccy = null;
                if (cpTrade.BOTradeNumber != null)
                {
                    string[] BOTrNrs = cpTrade.BOTradeNumber.Split(';');
                    Ctrade ctradevalue;
                    foreach (string boTrNr in BOTrNrs)
                    {
                        long currenttradenumber = Convert.ToInt64(boTrNr);
                        if (!boTradeslist.TryGetValue(currenttradenumber, out ctradevalue))
                        {
                            PostLog("\r\n" + "Didn't find Ctrade with tradenumber = " +
                                                  currenttradenumber.ToString());
                        }

                        IQueryable<Ctrade> ctradefromDb = from ctrade in db.Ctrades
                                                          where
                                                              ctrade.valid == 1 &&
                                                              ctrade.tradeNumber == currenttradenumber
                                                          // && ctrade.Date >= reportdate.Date && cptrade.ReportDate < (nextdate.Date)
                                                          select ctrade;
                        ctradevalue = ctradefromDb.FirstOrDefault();
                        if (account == null)
                        {
                            account = ctradevalue.account_id;
                        }
                        else
                        {
                            if (account != ctradevalue.account_id)
                            {
                                PostLog("\r\n" + "Accounts are different for cptrade.fullid=" +
                                                      cpTrade.FullId);
                            }
                        }
                        bosum = (double)(bosum + ctradevalue.fees);
                        ccy = ctradevalue.currency;
                    }
                }
                updatelist.Add(new ABNReconResult
                {
                    ReportDate = (DateTime)cpTrade.ReportDate,
                    TradeDate = cpTrade.TradeDate,
                    Symbol = cpTrade.Symbol,
                    TYPE = cpTrade.Type,
                    Qty = cpTrade.Qty,
                    Price = cpTrade.Price,
                    ValueDate = cpTrade.ValueDate,
                    ABN_cp = cpname,
                    BOSymbol = cpTrade.BOSymbol,
                    BOTradeNumber = cpTrade.BOTradeNumber,
                    BOcp = cpTrade.BOcp,
                    Mty = leverage,
                    Value = -leverage * cpTrade.Price * cpTrade.Qty,
                    BODate = cpTrade.TradeDate,
                    TypeOfTrade = cpTrade.TypeOfTrade,
                    COMMENT = cpTrade.Comment,
                    ExchFee = cpTrade.ExchangeFees,
                    ExchFeeCcy = cpTrade.ExchFeeCcy,
                    ClearFee = cpTrade.Fee,
                    ClearingFeeCcy = cpTrade.ClearingFeeCcy,
                    fullid = cpTrade.FullId,
                    BOfee = bosum, // todo calculate from join
                    BOCurrency = ccy, // todo calculate from join
                    BOAccount = account // todo calculate from join
                });
            }
            if (updatelist != null)
            {
                IQueryable<ABNReconResult> listtodelete = from recon in db.ABNReconResults
                                                          where
                                                              recon.ReportDate >= reportdate.Date &&
                                                              recon.ReportDate < nextdate.Date
                                                          select recon;
                db.ABNReconResults.RemoveRange(listtodelete);
                fn.SaveDBChanges(ref db);
                foreach (ABNReconResult reconResult in updatelist)
                {
                    db.ABNReconResults.Add(reconResult);
                }

                fn.SaveDBChanges(ref db);
            }
            DateTime TimeEndUpdating = DateTime.Now;
            PostLog("\r\n" + TimeEndUpdating.ToLongTimeString() + ": " +
                                  "Updating completed. Time:" + (TimeEndUpdating - TimeStart).ToString());
        }
        private Dictionary<int, string> GetCPmapping()
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            return cpfromDb.ToDictionary(k => k.cp_id, k => k.Name);
        }

        private int? getCPid(string cpname, Dictionary<string, int> cpdic)
        {
            if (cpname != null)
            {
                int cp_id;
                if (cpdic.TryGetValue(cpname, out cp_id))
                {
                    return cp_id;
                }
                else
                {
                    var dbentity = new EXANTE_Entities(_currentConnection);
                    dbentity.counterparties.Add(new counterparty { Name = cpname });
                    dbentity.SaveChanges();
                    IQueryable<int> cpidfromDb = from cp in dbentity.counterparties
                                                 where cp.Name == cpname
                                                 select cp.cp_id;
                    cpdic.Add(cpname, cpidfromDb.First());
                    return cpidfromDb.First();
                }
            }
            else
            {
                PostLog("Нет идентификатора counterparty");
                return 0;
            }
        }

        public Dictionary<string, List<string>> LoadCliff(string fileName, DateTime reportdate)
        {
            var reader = new StreamReader(fileName);
            //     var reader = new StreamReader(@"C:\20140428----1978-------C");
            string lineFromFile = reader.ReadLine();
            if (lineFromFile != null)
            {
                reportdate = (DateTime)fn.getDatefromString(lineFromFile.Substring(6, 8));
            }
            var cliffdict = new Dictionary<string, List<string>>();
            while (!reader.EndOfStream)
            {
                if (lineFromFile != null)
                {
                    string code = lineFromFile.Substring(0, 3);
                    if (cliffdict.ContainsKey(code))
                    {
                        cliffdict[code].Add(lineFromFile);
                    }
                    else cliffdict.Add(code, new List<string> { lineFromFile });
                }
                lineFromFile = reader.ReadLine();
            }
            return cliffdict;
        }
    }
}