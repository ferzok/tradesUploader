using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace WindowsFormsApplication1
{
    public class Bloomberg
    {
        private static string _currentConnection;
        public delegate void MessageStreamDelegate(string str);
        public event MessageStreamDelegate MessageRecived = delegate { };
        private CommonFunctions fn = new CommonFunctions(_currentConnection);
        public void PostLog(string message)
        {
            MessageRecived(message);
        }
        public Bloomberg(string s)
        {
            _currentConnection = s;
        }
        public void ParsingBloomberg(DateTime reportDate,string filename)
        {
            var ObjExcel = new Application();
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(filename,
                                                           0, false, 5, "", "", false,
                                                           XlPlatform
                                                               .xlWindows, "",
                                                           true, false, 0, true,
                                                           false, false);
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Calendar"];
            Range xlRange = ObjWorkSheet.UsedRange;
            IFormatProvider theCultureInfo = new CultureInfo("en-GB", true);
            var db = new EXANTE_Entities(_currentConnection);
            int delta = 0;
            int startrow = 17,
                idIsin = 7 + delta,
                idDeclDate = 8 + delta,
                idExDate = 9 + delta,
                idReDate = 10 + delta,
                idPayDate = 11 + delta,
                idDVDAmount = 12 + delta,
                idDVDFR = 13 + delta,
                idDvdType = 14 + delta;
            int i = startrow;
            Dictionary<string, List<Contract>> contracts = (from cp in db.Contracts
                                                            where cp.isin != null
                                                            select cp).ToList()
                                                                      .GroupBy(x => x.isin)
                                                                      .ToDictionary(x => x.Key, x => x.ToList());
            dynamic isinExcel = xlRange.Cells[i, idIsin].value2;
            var reportdatestring = reportDate.ToString("yyyy-MM-dd");
            Dictionary<string, long> CA = (from cp in db.CorporateActions
                                           where cp.ReportDate.ToString().Contains(reportdatestring)
                                           select cp).ToDictionary(
                                               k =>
                                               (k.symbolId + k.isin + k.DeclaredDate.Value.ToShortDateString() +
                                                k.ExDate.Value.ToShortDateString() + k.RecordDate.Value.ToShortDateString() +
                                                k.PayableDate.Value.ToShortDateString() + k.DividendAmount.ToString() +
                                                k.DividendFrqncy + k.DividendType), k => k.id);
         
            while (isinExcel != null)
            {
                List<Contract> contractDetails;
                dynamic DeclaredDate = DateTime.FromOADate(xlRange.Cells[i, idDeclDate].value2);
                dynamic ExDate = DateTime.FromOADate(xlRange.Cells[i, idExDate].value2);
                dynamic RecordDate = DateTime.FromOADate(xlRange.Cells[i, idReDate].value2);
                dynamic PayableDate = DateTime.FromOADate(xlRange.Cells[i, idPayDate].value2);
                dynamic DividendAmount = xlRange.Cells[i, idDVDAmount].value2;
                dynamic DividendType = xlRange.Cells[i, idDvdType].value2;
                dynamic DividendFrqncy = xlRange.Cells[i, idDVDFR].value2;
                dynamic isin = xlRange.Cells[i, idIsin].value2;

                if (!contracts.TryGetValue(isinExcel, out contractDetails))
                {
                    PostLog("\r\n" + "There isin in contracts: " + xlRange.Cells[i, idIsin].value2);

                    var testkey = "NULL" + isin + DeclaredDate.ToString("M/d/yyyy") + ExDate.ToString("M/d/yyyy") +
                                  RecordDate.ToString("M/d/yyyy") + PayableDate.ToString("M/d/yyyy") + DividendAmount +
                                  DividendFrqncy + DividendType;
                    long id;
                    if (!CA.TryGetValue(testkey, out id))
                    {
                        db.CorporateActions.Add(new CorporateActions
                            {
                                isin = xlRange.Cells[i, idIsin].value2,
                                DeclaredDate = DeclaredDate,
                                ExDate = ExDate,
                                RecordDate = RecordDate,
                                PayableDate = PayableDate,
                                DividendAmount = DividendAmount,
                                DividendType = DividendType,
                                DividendFrqncy = DividendFrqncy,
                                symbolId = null,
                                Timestamp = DateTime.UtcNow
                            });
                    }
                }
                else
                {
                    foreach (Contract contractDetail in contractDetails)
                    {
                        var testkey = contractDetail.Contract1 + isin + DeclaredDate.ToString("M/d/yyyy") +
                                      ExDate.ToString("M/d/yyyy") +
                                      RecordDate.ToString("M/d/yyyy") + PayableDate.ToString("M/d/yyyy") + DividendAmount +
                                      DividendFrqncy + DividendType;
                        long id;
                        if (!CA.TryGetValue(testkey, out id))
                        {
                            DateTime? lastdate = new DateTime();
                            dynamic qty = getQtyFromCtrade(db, contractDetail.Contract1, ExDate, ref lastdate, isin,
                                                                  reportDate.Date);
                            string comment = null;
                            if ((RecordDate.Year > 1900) && (PayableDate.Year > 1900))
                            {
                                if (DividendAmount == null) DividendAmount = 0;
                                //  comment = getLastCommentFromCorporateAction(db, reportDate, isin, DeclaredDate,ExDate, RecordDate,PayableDate, DividendAmount,DividendType,DividendFrqncy);
                            }

                            db.CorporateActions.Add(new CorporateActions
                                {
                                    isin = xlRange.Cells[i, idIsin].value2,
                                    DeclaredDate = DeclaredDate,
                                    ExDate = ExDate,
                                    RecordDate = RecordDate,
                                    PayableDate = PayableDate,
                                    DividendAmount = DividendAmount,
                                    DividendType = DividendType,
                                    DividendFrqncy = DividendFrqncy,
                                    symbolId = contractDetail.Contract1,
                                    Timestamp = DateTime.UtcNow,
                                    BOQty = qty,
                                    LastTradeDate = lastdate,
                                    ReportDate = reportDate.Date,
                                    Comment = comment
                                });
                        }
                    }
                }
                fn.SaveDBChanges(ref db);
                i++;
                isinExcel = xlRange.Cells[i, idIsin].value2;
            }
            fn.SaveDBChanges(ref db);
            db.Dispose();
            ObjWorkBook.Close();
            ObjExcel.Quit();
            Marshal.FinalReleaseComObject(ObjWorkBook);
            Marshal.FinalReleaseComObject(ObjExcel);
        }
        private string getLastCommentFromCorporateAction(EXANTE_Entities db, DateTime reportDate, string isin,
                                                         DateTime declaredDate, DateTime exDate, DateTime recordDate,
                                                         DateTime payableDate, double dividendAmount,
                                                         string dividendType, string dividendFrqncy)
        {
            CorporateActions t =
                (from o in db.CorporateActions
                 where
                     o.DividendType.Contains(dividendType) && o.DividendFrqncy.Contains(dividendFrqncy) &&
                     o.DividendAmount == dividendAmount && o.isin.Contains(isin)
                     && o.DeclaredDate == declaredDate.Date && o.ExDate == exDate.Date &&
                     o.RecordDate == recordDate.Date &&
                     o.PayableDate == payableDate.Date && o.ReportDate < reportDate.Date
                 select o).OrderByDescending(o => o.ReportDate).FirstOrDefault();
            if (t == null)
            {
                return "";
            }
            else
            {
                return t.Comment;
            }
        }
        private Double? getQtyFromCtrade(EXANTE_Entities db, string bosymbol, DateTime fromOaDate,
                                         ref DateTime? lastdate, string isin, DateTime reportdate)
        {
            /*var old = db.QtyByAccounts.Where(o => o.ExDate < reportdate.Date && o.Symbol.Contains(bosymbol)&& o.ExDate==fromOaDate.Date);
            
            if ((old.Count()) > 0)
            {
                foreach (var item in old)
                {
                    db.QtyByAccounts.Add(new QtyByAccounts
                        {
                            Symbol = bosymbol,
                            isin = isin,
                            Account_id = item.Account_id,
                            Qty = item.Qty,
                            ExDate = fromOaDate.Date,
                            timestamp = DateTime.Now,
                            LastTradeDate = item.LastTradeDate,
                            ReportDate = reportdate.Date,
                            Cp=item.Cp
                        });
                }
                var overall = old.Sum(o => o.Qty);
                lastdate = old.Max(o => o.LastTradeDate);
                return overall;
            }
            else*/
            {
                DateTime starttime = DateTime.Now;

                var sum =
                    db.Ctrades.Where(
                        o =>
                        o.BOtradeTimestamp < (fromOaDate.Date) && o.valid == 1 &&
                        (o.cp_id.Contains("LEK") || o.cp_id.Contains("INSTANT_US") || o.cp_id.Contains("INSTANT")) &&
                        o.symbol_id == bosymbol).GroupBy(o => new
                        {
                            o.account_id,
                            o.cp_id,
                            o.symbol_id
                        }).Select(g => new
                        {
                            g.Key.account_id,
                            g.Key.cp_id,
                            qty = g.Sum(o => o.qty),
                            LastDate = g.Max(o => o.BOtradeTimestamp)
                        });
                DateTime endtime = DateTime.Now;
                TimeSpan delta = endtime - starttime;


                foreach (var item in sum)
                {
                    db.QtyByAccounts.Add(new QtyByAccounts
                    {
                        Symbol = bosymbol,
                        isin = isin,
                        Account_id = item.account_id,
                        Qty = item.qty,
                        ExDate = fromOaDate.Date,
                        timestamp = DateTime.Now,
                        LastTradeDate = item.LastDate,
                        ReportDate = reportdate.Date,
                        Cp = item.cp_id
                    });
                }
                double? overall = sum.Sum(o => o.qty);


                //  var firstOrDefault = sum.FirstOrDefault();
                if (overall != null)
                {
                    lastdate = sum.Max(o => o.LastDate);
                    return overall;
                    //  lastdate = (DateTime) firstOrDefault.LastDate;
                    //  return firstOrDefault.qty;
                }
                else
                {
                    lastdate = null;
                    return null;
                }
            }
        }
    }
}