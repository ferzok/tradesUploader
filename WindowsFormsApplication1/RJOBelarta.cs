using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace WindowsFormsApplication1
{
    public class RJOBelarta
    {

        public RJOBelarta(string s)
        {
            _connstring = s;
        }
        private CommonFunctions fn = new CommonFunctions(_connstring);
        private static string _connstring;
       
        public delegate void MessageStreamDelegate(string str);
        public event MessageStreamDelegate MessageRecived = delegate { };
        public void PostLog(string message)
        {
            MessageRecived(message);
        }

        private DateTime getDateFromPdfRJO(string txt)
        {
            return DateTime.ParseExact(txt.Trim(), "dd-MMM-yy", CultureInfo.InvariantCulture);
        }
        private List<InitialTrade> ParsingToTrades(string txt, ref EXANTE_Entities db, DateTime reportdate, string account)
        {
            string[] rows;
            rows = txt.Split('\n');
            int j = 4;
            var InitTradeslist = new List<InitialTrade>();
            while(!rows[j].Contains("P U R C H A S E   &   S A L E")&&!rows[j].Contains("O P E N   P O S I T I O N S")){
              InitTradeslist.AddRange(GetTradesTillTotal(ref txt, ref db, reportdate, account,ref j, rows));
                while ((rows[j][0] == ' ') && !rows[j].Contains("P U R C H A S E   &   S A L E") &&
                       !rows[j].Contains("O P E N   P O S I T I O N S")) j++;
            }
            fn.SendToDb(ref db,InitTradeslist,500);
            return InitTradeslist;
        }

        private List<InitialTrade> GetTradesTillTotal(ref string txt, ref EXANTE_Entities db, DateTime reportdate, string account,ref int j,
                                        string[] rows)
        {
            var InitTradeslist = new List<InitialTrade>();
            string[] tabs;
            double sumqty = 0;
            while (j <= rows.Count() && !rows[j].Contains("TOTAL"))
            {
                tabs = rows[j].Split(' ');
                int index = 3;
                double? qty = getQtyFromRjoPdf(tabs, ref index);
                sumqty = sumqty + Math.Abs((double) qty);
                InitTradeslist.Add(new InitialTrade
                    {
                        Account = account,
                        BrokerId = "RJOBelarta",
                        ReportDate = reportdate.Date,
                        TradeDate = getDateFromPdfRJO(tabs[0]),
                        Exchange = tabs[1],
                        Qty = qty,
                        Symbol = getStringFromRjoPdf(tabs, ref index),
                        Price = getPriceFromRjoPdf(tabs, ref index),
                        ccy = getStringFromRjoPdf(tabs, ref index),
                        Timestamp = DateTime.UtcNow,
                        ClearingFeeCcy = null,
                        Comment = null,
                        cp_id = null,
                        ExchangeFees = null,
                        exchangeOrderId = null,
                        ExchFeeCcy = null,
                        Type = "FU"
                    });
                txt = txt + tabs[1];
                j++;
            }
            tabs = rows[j].Split(' ');
            double Fee = Convert.ToDouble(tabs[tabs.Count() - 1])/sumqty;
            string feeccy = getCcyFeeFromRjoPdf(tabs, tabs.Count() - 2);
            foreach (InitialTrade initialTrade in InitTradeslist)
            {
                initialTrade.ExchFeeCcy = feeccy;
                initialTrade.ExchangeFees = Fee;
            }
            fn.SendToDb(ref db, InitTradeslist, 500);
            j++;
            return InitTradeslist;
        }

        private string getCcyFeeFromRjoPdf(string[] tabs,int index)
        {
            while (tabs[index] == "")
            {
                index--;
            }
            return tabs[index];
        }

        private double? getPriceFromRjoPdf(string[] tabs, ref int index)
        {
            while (tabs[index] == "")
            {
                index++;
            }
            return Convert.ToDouble(tabs[index++]);
        }

        private string getStringFromRjoPdf(string[] tabs, ref int index)
        {
            string result = "";
            while (tabs[index] != "")
            {
                result = result + tabs[index] + " ";
                index++;
            }
            return result.Substring(0, result.Count() - 1);
        }

        private double? getQtyFromRjoPdf(string[] tabs, ref int index)
        {
            int i = 3;
            while (tabs[i]=="")
            {
                i++;
            }
            double result = Convert.ToDouble(tabs[i]);
            index = i + 1;
            while (tabs[index] == "")
            {
                index++;
            }

            if (index - i < 3) result = -result;
            return result;
        }

        public List<InitialTrade> RJOBelartaPdfParsing(string oFilename, ref EXANTE_Entities db)
        {
            DateTime TimeStart = DateTime.Now;
            Dictionary<string, long> checkId = (from ct in db.CpTrades
                                                where
                                                    ct.TradeDate.ToString().Contains("2016-") &&
                                                    ct.BrokerId == "RJOBelarta"
                                                select ct).ToDictionary(k => k.exchangeOrderId.ToString(),
                                                                        k => k.FullId);
            var reader = new PdfReader(oFilename);
            int count = reader.NumberOfPages;
            string txt = "";
            var i = 1;
            txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
            var reportdate = getDateFromPdfRJO(txt.Substring(txt.IndexOf("STATEMENT DATE:") + 15, 10));
            var account = txt.Substring(txt.IndexOf("ACCOUNT:") + 8, 10).Trim();
            int indexStart = txt.IndexOf("T R A D E S   C O N F I R M A T I O N S");
            var listinittrades = ParsingToTrades(txt.Substring(indexStart), ref db, reportdate, account);
            DateTime TimeEnd = DateTime.Now;
            PostLog(TimeEnd.ToLongTimeString() + ": " + listinittrades.Count.ToString() + " trades Belarta RJO uploading completed." +
                                         (TimeEnd - TimeStart).ToString());
            PostLog(oFilename);
            return listinittrades;
        }
    }
}