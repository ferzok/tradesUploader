using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace WindowsFormsApplication1
{
    class AxiPdfParser
    {
        private static string _connstring;
        public AxiPdfParser(string s)
        {
            _connstring = s;
        }
        private readonly CommonFunctions fn = new CommonFunctions(_connstring);

        private static double AxiPdfGetNegativeValue(string traderow)
        {
            double value = 0;
            if (traderow.Contains("("))
            {
                value = -Convert.ToDouble(traderow.Replace("(", "").Replace(")", ""));
            }
            else
            {
                value = Convert.ToDouble(traderow);
            }
            return value;
        }

        private static int AxiPdfGetStarRow(string[] rows, string Abstractname)
        {
            int i_row = 0;
            while ((i_row < rows.Length) && (!rows[i_row].Contains(Abstractname)))
            {
                i_row++;
            }
            if (i_row < rows.Length)
            {
                while ((i_row < rows.Length) && (!rows[i_row].Contains("LONP100 ML INVEST")))
                {
                    i_row++;
                }
            }
            if (i_row == rows.Length)
            {
                return -1;
            }
            else
            {
                if (rows[i_row].Contains("LONP100 ML INVEST"))
                {
                    return i_row + 1;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

      private List<Axi_SettlingTrades> GetSettlingTrades(PdfReader reader, DateTime reportdate,ref int pagenumber)
        {
            var page = PdfTextExtractor.GetTextFromPage(reader, pagenumber, new LocationTextExtractionStrategy());

            if (page.Contains("SETTLING TRADE DETAILS"))
            {
                var listAxiSettlingTrades = new List<Axi_SettlingTrades>();
                bool flagStop = false;
                string[] rows;
                string account;
                rows = page.Split('\n');
                var i_row = AxiPdfGetStarRow(rows, "SETTLING TRADE DETAILS");
                if (i_row != -1)
                {
                    account = rows[i_row - 1];
                    while ((pagenumber < reader.NumberOfPages) && (!flagStop))
                    {
                        page = PdfTextExtractor.GetTextFromPage(reader, pagenumber, new LocationTextExtractionStrategy());
                        rows = page.Split('\n');
                        if (i_row != -1)
                        {
                            while ((i_row < rows.Length) && (!rows[i_row].Contains("Page")) &&
                                   (!rows[i_row].Contains("CASH MOVEMENTS")) &&
                                   (!rows[i_row].Contains("NEW TRADING ACTIVITY"))
                                   && (!flagStop))
                            {
                                var axirow = new Axi_SettlingTrades();
                                axirow.Reportdate = reportdate;
                                axirow.Account = account;
                                string[] traderow =
                                    rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                if (traderow.Count() == 4)
                                {
                                    if (traderow[0].Split('-').Count() > 1)
                                    {
                                        throw new NotImplementedException();
                                    }
                                    axirow.TradeCode = traderow[0];
                                    axirow.Product = traderow[1];
                                    axirow.TradeRate = Convert.ToDouble(traderow[2]);
                                    axirow.ConsolidationRate = Convert.ToDouble(traderow[3]);
                                    i_row++;
                                    traderow = rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');

                                    axirow.TradeNumber = Convert.ToInt64(traderow[0]);
                                    axirow.TradeDate = GetDateFromPdfItem(traderow[1]);
                                    axirow.ValueDate = GetDateFromPdfItem(traderow[2]);
                                    axirow.ccy2Amount = AxiPdfGetNegativeValue(traderow[7]);
                                    axirow.cc1Amount = AxiPdfGetNegativeValue(traderow[6]);
                                    axirow.ccy1 = traderow[5];
                                    axirow.ccyPair = traderow[3];
                                    axirow.direction = traderow[4];
                                }
                                else
                                {
                                    axirow.TradeCode = traderow[1];
                                    axirow.Product = traderow[2];
                                    axirow.TradeRate = Convert.ToDouble(traderow[9]);
                                    axirow.ConsolidationRate = Convert.ToDouble(traderow[11]);
                                    axirow.TradeNumber = Convert.ToInt64(traderow[0]);
                                    axirow.TradeDate = GetDateFromPdfItem(traderow[3]);
                                    axirow.ValueDate = GetDateFromPdfItem(traderow[4]);
                                    axirow.direction = traderow[6];
                                    axirow.ccy2Amount = AxiPdfGetNegativeValue(traderow[10]);
                                    axirow.cc1Amount = AxiPdfGetNegativeValue(traderow[8]);
                                    axirow.ccy1 = traderow[7];
                                    axirow.ccyPair = traderow[5];
                                }
                                listAxiSettlingTrades.Add(new Axi_SettlingTrades
                                    { 
                                        Reportdate = axirow.Reportdate,
                                        Account = axirow.Account,
                                        TradeCode = axirow.TradeCode,
                                        Product = axirow.Product,
                                        TradeRate = axirow.TradeRate,
                                        ConsolidationRate = axirow.ConsolidationRate,
                                        TradeNumber = axirow.TradeNumber,
                                        TradeDate = axirow.TradeDate,
                                        ValueDate = axirow.ValueDate,
                                        direction = axirow.direction,
                                        ccy2Amount = axirow.ccy2Amount,
                                        cc1Amount = axirow.cc1Amount,
                                        ccy1 = axirow.ccy1,
                                        ccyPair = axirow.ccyPair
                                    });
                                i_row = i_row + 1;
                            }
                            if ((i_row < rows.Length) &&
                                ((rows[i_row].Contains("CASH MOVEMENTS")) ||
                                 (page.Contains("NEW TRADING ACTIVITY"))))
                            {
                                flagStop = true;
                            }
                        }
                        i_row = 0;
                        if (!flagStop) pagenumber++;
                    }
                }
                return listAxiSettlingTrades;
            }
            else return null;
        }
    
        public void AxiTradesParser(string oFilename)
        {
            var db = new EXANTE_Entities(_connstring);
            var reader = new PdfReader(oFilename);
            int count = reader.NumberOfPages;
            string txt = "";
            int pageNumber = 1;
            txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            DateTime reportdate = GetReportData(txt);
            DateTime begindate = reportdate.AddHours(-120);
                Dictionary<string, long> checkId = (from ct in db.CpTrades
                                                where
                                                    ct.TradeDate > begindate &&
                                                    ct.BrokerId == "Axi"
                                                select ct).ToDictionary(k => k.exchangeOrderId.ToString(), k => k.FullId);


            while (pageNumber <= count && !txt.Contains("ROLLOVER TRADE DETAILS") &&!txt.Contains("NEW TRADING ACTIVITY")
                && !txt.Contains("SETTLING TRADE DETAILS"))
            {
                pageNumber++;
                txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            }
            var dicCpCtrades = new Dictionary<string, List<CpTrade>>();
           
            List<Axi_RolloverTrades> axirolls = GetAxiRolloverTrades(reader, reportdate, ref pageNumber);
            if (axirolls.Count > 0) fn.SendToDb(ref db, axirolls);
            CleanlDuplicates(ref axirolls, checkId, "NearLeg");
            CleanlDuplicates(ref axirolls, checkId, "FarLeg");
            var listcptrades = GetCpTradesFromAxiRolls(axirolls);

            txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            while (pageNumber <= count && !txt.Contains("SETTLING TRADE DETAILS") &&!txt.Contains("NEW TRADING ACTIVITY"))
            {
                pageNumber++;
                txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            }
            var settlingTrades = GetSettlingTrades(reader,  reportdate, ref pageNumber);
            if (settlingTrades.Count > 0) fn.SendToDb(ref db, settlingTrades);
            CleanlDuplicates(ref settlingTrades, checkId, "ROLL_SET");
            CleanlDuplicates(ref settlingTrades, checkId, "SETTLED");
            listcptrades.AddRange(GetCpTradesFromSettling(settlingTrades));

            txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            while (pageNumber < count && !txt.Contains("NEW TRADING ACTIVITY"))
            {
                pageNumber++;
                txt = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            }
            var axitrades =  GetAxiTrades(reader,ref pageNumber, reportdate);
            if (axitrades.Count > 0) fn.SendToDb(ref db, axitrades);
            dicCpCtrades =GetCpTradesFromAxiTrades(axitrades);

            foreach (var trade in listcptrades)
            {
                db.CpTrades.Add(trade);
            }
            foreach (var valuePair in dicCpCtrades)
            {
                if (valuePair.Value.Count == 1)
                {
                    db.CpTrades.Add(valuePair.Value[0]);
                }
                else
                {
                    if (valuePair.Value.Count == 2)
                    {
                        if (((valuePair.Value[0].Symbol != "JPY/USD") &&
                             (valuePair.Value[0].Symbol != "CHF/USD") &&
                             (valuePair.Value[0].Symbol != "ZAR/USD") &&
                             (valuePair.Value[0].Symbol != "JPY/ZAR") &&
                             (valuePair.Value[0].Symbol != "AUD/GBP") &&
                             (valuePair.Value[0].Symbol != "CAD/USD") &&
                             (valuePair.Value[0].Symbol != "CAD/GBP") &&
                             (valuePair.Value[0].Symbol != "CAD/EUR") &&
                             (valuePair.Value[0].Symbol != "CAD/NZD") &&
                             (valuePair.Value[0].Symbol != "JPY/NZD") &&
                             (valuePair.Value[0].Symbol != "AUD/EUR") &&
                             (valuePair.Value[0].Symbol != "CHF/GBP") &&
                             (valuePair.Value[0].Symbol != "CHF/EUR") &&
                             (!valuePair.Value[0].Symbol.Contains("THB/")) &&
                             (!valuePair.Value[0].Symbol.Contains("TRY/"))
                             && (valuePair.Value[0].Symbol != "MXN/USD") &&
                             (valuePair.Value[0].Symbol != "NOK/USD") &&
                             (valuePair.Value[0].Symbol != "RUB/USD")&&
                             (valuePair.Value[0].Symbol != "USD/XAG")&&
                             (valuePair.Value[0].Symbol != "USD/XAU")))
                        //(valuePair.Value[0].Symbol.Contains("/USD"))) ||
                        //(!valuePair.Value[0].Symbol.Contains("USD/")))
                        {
                            db.CpTrades.Add(valuePair.Value[0]);
                        }
                        else
                        {
                            db.CpTrades.Add(valuePair.Value[1]);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            fn.convertCptradesToInitTrades(db.CpTrades.Local.ToList(), ref db);
            fn.SaveDBChanges(ref db);
            db.Dispose();
        }

        private Dictionary<string,List <CpTrade>> GetCpTradesFromAxiTrades(List<Axi_Trades> axitrades)
        {
            var dicCpCtrades = new Dictionary<string, List<CpTrade>>();
            foreach (var axiroll in axitrades)
            {
                if (axiroll.TradeCode == "NEW")
                {
                    if (!dicCpCtrades.ContainsKey(axiroll.TradeNumber.ToString()))
                    {
                        dicCpCtrades.Add(axiroll.TradeNumber.ToString(), new List<CpTrade>());
                    }
                    dicCpCtrades[axiroll.TradeNumber.ToString()].Add(new CpTrade
                        {
                            ReportDate = axiroll.ReportDate,
                            account = axiroll.Account,
                            BrokerId = "Axi",
                            TradeDate = axiroll.TradeDate,
                            Symbol = axiroll.CcyPair,
                            Type = axiroll.Product,
                            Qty = axiroll.Ccy1Amount,
                            Price = axiroll.TradeRate,
                            ValueDate = axiroll.ValueDate,
                            value = axiroll.Ccy2Amount,
                            Timestamp = DateTime.UtcNow,
                            valid = 1,
                            exchangeOrderId = axiroll.TradeNumber.ToString(),
                            TypeOfTrade = "Trade"
                        });
                }
            }
            return dicCpCtrades;
        }

        private List<CpTrade> GetCpTradesFromSettling(List<Axi_SettlingTrades> settlingTrades)
        {
            var result = new List<CpTrade>();
            foreach (var axiroll in settlingTrades)
            {
                result.Add(new CpTrade
                    {
                        ReportDate = axiroll.Reportdate,
                        account = axiroll.Account,
                        BrokerId = "Axi",
                        TradeDate = axiroll.TradeDate,
                        Symbol = axiroll.ccyPair,
                        Type = axiroll.Product,
                        Qty = axiroll.cc1Amount,
                        Price = axiroll.TradeRate,
                        ValueDate = axiroll.ValueDate,
                        value = axiroll.ccy2Amount,
                        Timestamp = DateTime.UtcNow,
                        valid = 1,
                        exchangeOrderId = axiroll.TradeNumber.ToString() + axiroll.TradeCode,
                        TypeOfTrade = axiroll.TradeCode
                    });
                
            }
            return result;
        }

        private List<CpTrade> GetCpTradesFromAxiRolls(List<Axi_RolloverTrades> axirolls)
        {
            var result = new List<CpTrade>();                         
            foreach (var axiroll in axirolls)
            {
                result.Add(new CpTrade
                   {
                       ReportDate = axiroll.Reportdate,
                       account = axiroll.Account,
                       BrokerId = "Axi",
                       TradeDate = axiroll.TradeDate,
                       Symbol = axiroll.CcyPair,
                       Type = "FX",
                       Qty = axiroll.Ccy1Amount,
                       Price = axiroll.TradeRate,
                       ValueDate = axiroll.NearDate,
                       value = axiroll.Ccy2Amount,
                       Timestamp = DateTime.UtcNow,
                       valid = 1,
                       exchangeOrderId = axiroll.TradeNumber.ToString() + "NearLeg",
                       TypeOfTrade = "Swap"
                   });   
                result.Add(new CpTrade
                   {
                       ReportDate = axiroll.Reportdate,
                       account = axiroll.Account,
                       BrokerId = "Axi",
                       TradeDate = axiroll.TradeDate,
                       Symbol = axiroll.CcyPair,
                       Type = "FX",
                       Qty = -axiroll.Ccy1Amount,
                       Price = axiroll.FarTradeRate,
                       ValueDate = axiroll.FarDate,
                       value = axiroll.FarTradeRate*axiroll.Ccy1Amount,
                       Timestamp = DateTime.UtcNow,
                       valid = 1,
                       exchangeOrderId = axiroll.TradeNumber.ToString() + "FarLeg",
                       TypeOfTrade = "Swap"
                   });  
            }
            return result;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        private void CleanlDuplicates<T>(ref List<T> listtrades, Dictionary<string, long> checkId,string identifier)
        {
            for(int i = listtrades.Count - 1; i >= 0; i--) {
                if (checkId.ContainsKey(GetPropertyValue(listtrades[i], "TradeNumber") + identifier)) listtrades.RemoveAt(i);
            }
        }

        private static List<Axi_Trades> GetAxiTrades(PdfReader reader, ref int pageNumber, DateTime reportdate)
        {
            var page = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
            if (page.Contains("NEW TRADING ACTIVITY"))
            {
                var listaxitrades = new List<Axi_Trades>();
                bool flagStop = false;
                var rows = page.Split('\n');
                var i_row = AxiPdfGetStarRow(rows, "NEW TRADING ACTIVITY");
                if (i_row > -1)
                {
                    var account = rows[i_row - 1];
                    while ((pageNumber < reader.NumberOfPages) && (!flagStop))
                    {
                        page = PdfTextExtractor.GetTextFromPage(reader, pageNumber, new LocationTextExtractionStrategy());
                        rows = page.Split('\n');
                        if (i_row != -1)
                        {
                            while ((i_row < rows.Length) && (!rows[i_row].Contains("Page")) &&
                                   (!rows[i_row].Contains("CASH MOVEMENTS")))
                            {
                                if (rows[i_row].Contains("Amount"))
                                {
                                    i_row = i_row + 2;
                                }
                                else
                                {
                                    var axitrade = new Axi_Trades();
                                    if ((rows[i_row].Contains("NEW")) || (rows[i_row].Contains("SETTLED")))
                                    {
                                        string[] traderow =
                                            rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                        int add = 0;
                                        string tradenumber = "";
                                        if ((traderow[0].TrimStart().TrimEnd() == "NEW") ||
                                            (traderow[0].TrimEnd().TrimStart() == "SETTLED"))
                                        {
                                            add = 1;
                                            tradenumber = rows[i_row + 1].TrimStart();
                                        }
                                        else
                                        {
                                            tradenumber = traderow[0];
                                        }
                                        axitrade.Account = account;
                                        axitrade.ReportDate = reportdate;
                                        axitrade.TradeNumber = Convert.ToInt64(tradenumber);
                                        axitrade.TradeCode = traderow[1 - add];
                                        axitrade.TradeDate = GetDateFromPdfItem(traderow[3 - add]);
                                        axitrade.ValueDate = GetDateFromPdfItem(traderow[4 - add]);
                                        axitrade.Ccy2Amount = AxiPdfGetNegativeValue(traderow[10 - add]);
                                        axitrade.TradeRate = Convert.ToDouble(traderow[9 - add]);
                                        axitrade.Ccy1Amount = AxiPdfGetNegativeValue(traderow[8 - add]);
                                        axitrade.CcyPair = traderow[5 - add];
                                        axitrade.Ccy1 = traderow[7 - add];
                                        axitrade.Product = traderow[2 - add];
                                        axitrade.Direction = traderow[6 - add];
                                        listaxitrades.Add(new Axi_Trades
                                            {
                                               Account = account,
                                               ReportDate = reportdate,
                                               TradeNumber = Convert.ToInt64(tradenumber),
                                               TradeCode = traderow[1 - add],
                                               TradeDate = GetDateFromPdfItem(traderow[3 - add]),
                                               ValueDate = GetDateFromPdfItem(traderow[4 - add]),
                                               Ccy2Amount = AxiPdfGetNegativeValue(traderow[10 - add]),
                                               TradeRate = Convert.ToDouble(traderow[9 - add]),
                                               Ccy1Amount = AxiPdfGetNegativeValue(traderow[8 - add]),
                                               CcyPair = traderow[5 - add],
                                               Ccy1 = traderow[7 - add],
                                               Product = traderow[2 - add],
                                               Direction = traderow[6 - add]
                                            });
                                    }
                                }
                                i_row++;
                            }
                            if ((i_row < rows.Length) && (rows[i_row].Contains("CASH MOVEMENTS")))
                            {
                                flagStop = true;
                            }
                        }
                        i_row = 0;
                        pageNumber++;
                    }
                }
                return listaxitrades;
            }
            else return null;
        }

        private static DateTime GetDateFromPdfItem(string tempdate)
        {
           if (tempdate.Length < 11) tempdate = "0" + tempdate;
           return DateTime.ParseExact(tempdate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
        }

        public  List<Axi_RolloverTrades> GetAxiRolloverTrades(PdfReader reader,  DateTime reportdate, ref int pagenumber)
        {
            var page = PdfTextExtractor.GetTextFromPage(reader, pagenumber, new LocationTextExtractionStrategy());
            if (page.Contains("ROLLOVER TRADE DETAILS"))
            {
                bool flagStop = false;
                var listAxiRolloverTrades = new List<Axi_RolloverTrades>();
                string[] rows;
                int i_row;
                rows = page.Split('\n');
                i_row = AxiPdfGetStarRow(rows, "ROLLOVER TRADE DETAILS");
                if (i_row != -1)
                {
                    string account = rows[i_row - 1];
                    while ((pagenumber < reader.NumberOfPages) && (!flagStop))
                    {
                        page = PdfTextExtractor.GetTextFromPage(reader, pagenumber, new LocationTextExtractionStrategy());
                        rows = page.Split('\n');
                        if (i_row != -1)
                        {
                            while ((i_row < rows.Length) && (!rows[i_row].Contains("Page")) &&
                                   (!rows[i_row].Contains("CASH MOVEMENTS")) &&
                                   (!rows[i_row].Contains("SETTLING TRADE DETAILS")) &&
                                   (!rows[i_row].Contains("NEW TRADING ACTIVITY"))
                                   && (!flagStop))
                            {
                                var axirollover = new Axi_RolloverTrades();
                                string[] traderow =
                                    rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                if (traderow.Count() == 1)
                                {
                                    string tempvalue = traderow[0];
                                    i_row++;
                                    traderow = rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                    Array.Resize(ref traderow, traderow.Length + 1);
                                    traderow[traderow.Length - 1] = tempvalue;
                                    Console.WriteLine("\r\n" + "Pay attention to value: " + tempvalue);
                                }
                                if ((traderow[0] == "SETTLED") || (traderow[1] == "SETTLED"))
                                {
                                    flagStop = true;
                                }
                                else
                                {
                                    int add = 0;
                                    string tradenumber;
                                    if (traderow[0].Split('-').Count() > 1)
                                    {
                                        add = 1;
                                        tradenumber = rows[i_row + 1].TrimStart();
                                    }
                                    else
                                    {
                                        tradenumber = traderow[0];
                                    }
                                    axirollover.TradeNumber = Convert.ToInt64(tradenumber);
                                    axirollover.TradeDate = GetDateFromPdfItem(traderow[1 - add]).Date;
                                    axirollover.NearDate = GetDateFromPdfItem(traderow[2 - add]).Date;
                                    axirollover.FarDate = GetDateFromPdfItem(traderow[3 - add]).Date;
                                    axirollover.Account = account;
                                    axirollover.CcyPair = traderow[4 - add];
                                    axirollover.Direction = traderow[5 - add] + traderow[6 - add] + traderow[7 - add];
                                    axirollover.Ccy1 = traderow[8 - add];
                                    axirollover.Ccy1Amount = AxiPdfGetNegativeValue(traderow[9 - add]);
                                    axirollover.TradeRate = Convert.ToDouble(traderow[10 - add]);
                                    axirollover.Ccy2Amount = AxiPdfGetNegativeValue(traderow[12 - add]);
                                    axirollover.Reportdate = reportdate.Date;
                                    axirollover.FarTradeRate = AxiPdfGetNegativeValue(traderow[11 - add]);
                                    var fartraderate = AxiPdfGetNegativeValue(traderow[11 - add]);
                                    if (traderow[11 - add].Contains("("))
                                    {
                                        axirollover.FarTradeRate = AxiPdfGetNegativeValue(traderow[12 - add]);
                                        fartraderate = AxiPdfGetNegativeValue(traderow[12 - add]); 
                                        Console.WriteLine("\r\n" + "Pay attention to value: " + axirollover.FarTradeRate.ToString());
                                    }
                                    if ((Math.Abs((double) (Convert.ToDouble(traderow[10 - add]) / axirollover.FarTradeRate)) > 1.2) ||
                                        (Math.Abs((double) (Convert.ToDouble(traderow[10 - add]) / axirollover.FarTradeRate)) < 0.8))
                                    {
                                        axirollover.FarTradeRate = AxiPdfGetNegativeValue(traderow[12 - add]);
                                        fartraderate = AxiPdfGetNegativeValue(traderow[12 - add]);
                                        Console.WriteLine("\r\n" + "Pay attention to value: " + axirollover.FarTradeRate.ToString());
                                    }
                                    listAxiRolloverTrades.Add(new Axi_RolloverTrades
                                        {
                                            Account =account,
                                            TradeNumber = Convert.ToInt64(tradenumber),
                                            TradeDate = GetDateFromPdfItem(traderow[1 - add]),
                                            NearDate = GetDateFromPdfItem(traderow[2 - add]),
                                            FarDate = GetDateFromPdfItem(traderow[3 - add]),
                                            CcyPair = traderow[4 - add],
                                            Direction = traderow[5 - add] + traderow[6 - add] + traderow[7 - add],
                                            Ccy1 = traderow[8 - add],
                                            Ccy1Amount = AxiPdfGetNegativeValue(traderow[9 - add]),
                                            TradeRate = Convert.ToDouble(traderow[10 - add]),
                                            Ccy2Amount = AxiPdfGetNegativeValue(traderow[12 - add]),
                                            Reportdate = reportdate,
                                            FarTradeRate = fartraderate,
                                            id=1
                                        });
                                    i_row = i_row + add + 1;
                                }
                            }
                            if ((i_row < rows.Length) &&((rows[i_row].Contains("CASH MOVEMENTS")) ||(page.Contains("NEW TRADING ACTIVITY")) ||
                                 (page.Contains("SETTLING TRADE DETAILS"))))
                            {
                                flagStop = true;
                            }
                        }
                        i_row = 0;
                        if (!flagStop) pagenumber++;
                    }
                }
                return listAxiRolloverTrades;
            }
            else return null;
        }

        private static DateTime GetReportData(string txt)
        {
            DateTime reportdate;
            int indexDate = txt.IndexOf("Date: ") + 6;
            int indexDateEnd = txt.IndexOf(" ", indexDate);
            string tempdate = txt.Substring(indexDate, indexDateEnd - indexDate);
            if (tempdate.Length < 11) tempdate = "0" + tempdate;
            try
            {
                reportdate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new FormatException("Axipdfparser Reportdate is wrong");
            }
            return reportdate;
        }
    }

}
