using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Application = Microsoft.Office.Interop.Excel.Application;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
//using System.Data.Entity.Core.Common.;
//using System.Data.Objects; 
//using System.Data.Entity.Core.EntityClient;
//Objects.SqlClient;
//using System.DaSqlClient;

// using System.Web.Script.Serialization;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private const char Delimiter = ';';
        private const char CSVDelimeter = ',';
        private static string _currentConnection;
        private static string _currentAcc;

        public Form1()
        {
            InitializeComponent();
            ConnectionStringSettingsCollection connection = ConfigurationManager.ConnectionStrings;
            for (int i = 0; i < connection.Count; i++)
            {
                if (connection[i].ProviderName != "")
                {
                    comboBoxEnviroment.Items.Add(connection[i].Name);
                    if (connection[i].Name == "EXANTE_Entities")
                    {
                        comboBoxEnviroment.Text = "EXANTE_Entities";
                    }
                }
            }
            _currentConnection = comboBoxEnviroment.Text;
            var db = new EXANTE_Entities(_currentConnection);
            List<DBBORecon_mapping> brockerlist = (from rec in db.DBBORecon_mapping
                                                   where rec.valid == 1
                                                   select rec).ToList();
            foreach (DBBORecon_mapping t in brockerlist)
            {
                BrockerComboBox.Items.Add(t.NameProcess);
                if (t.NameProcess == "ADSS-ADSS")
                {
                    BrockerComboBox.Text = "ADSS-ADSS";
                    _currentAcc = "ADSS-ADSS";
                }
            }
            db.Dispose();
        }

        private void TradesParser_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var reportdate = new DateTime(2011, 01, 01);
                var db = new EXANTE_Entities(_currentConnection);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromFile = new List<Ctrade>();

                const int GMToffset = 4; //gmt offset from BO
                const int nextdaystarthour = 20; //start new day for FORTS
                const string template = "FORTS";
                DateTime nextdayvalueform = Fortsnextday.Value;
                string lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText(TimeStart.ToLongTimeString() + ": " + "start BO trades uploading");
                int index = 1;
                bool checkMalta = checkBoxMalta.Checked;
                if (lineFromFile != null)
                {
                    string[] rowstring = lineFromFile.Split(Delimiter);
                    int idDate = -1,
                        idSymbol = -1,
                        idAccount = -1,
                        idqty = -1,
                        idprice = -1,
                        idside = -1,
                        idfees = -1,
                        iduser = -1,
                        idcurrency = -1,
                        idorderid = -1,
                        idbrokerTimeDelta = -1,
                        idexchangeOrderId = -1,
                        idcontractMultiplier = -1,
                        idtradeNumber = -1,
                        idcounterparty = -1,
                        idgateway = -1,
                        idtradeType = -1,
                        idSettlementCp = -1,
                        idtradedVolume = -1,
                        idcptime = -1,
                        idorderPos = -1,
                        idvalueDate = -1;
                    for (int i = 0; i < rowstring.Length; i++)
                    {
                        switch (rowstring[i])
                        {
                            case "gwTime":
                                idDate = i;
                                break;
                            case "counterpartyTime":
                                idcptime = i;
                                break;
                            case "symbolId":
                                idSymbol = i;
                                break;
                            case "accountId":
                                idAccount = i;
                                break;
                            case "quantity":
                                idqty = i;
                                break;
                            case "price":
                                idprice = i;
                                break;
                            case "side":
                                idside = i;
                                break;
                            case "commission":
                                idfees = i;
                                break;
                            case "userId":
                                iduser = i;
                                break;
                            case "currency":
                                idcurrency = i;
                                break;
                            case "tradeType":
                                idtradeType = i;
                                break;
                            case "orderId":
                                idorderid = i;
                                break;
                            case "brokerTimeDelta":
                                idbrokerTimeDelta = i;
                                break;
                            case "orderPos":
                                idorderPos = i;
                                break;


                            case "exchangeOrderId":
                                idexchangeOrderId = i;
                                break;
                            case "contractMultiplier":
                                idcontractMultiplier = i;
                                break;
                            case "executionCounterparty":
                                idcounterparty = i;
                                break;
                            case "gatewayId":
                                idgateway = i;
                                break;
                            case "valueDate":
                                idvalueDate = i;
                                break;
                            case "settlementCounterparty":
                                idSettlementCp = i;
                                break;
                            case "tradedVolume":
                                idtradedVolume = i;
                                break;
                            default:
                                Console.WriteLine("Additional fields in the tr.file!");
                                break;
                        }
                    }

                    string stringindex = Convert.ToString(reportdate.Year);
                    if (reportdate.Month < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Month));
                    if (reportdate.Day < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Day));
                    long initialindex = Convert.ToInt64(stringindex);
                    IQueryable<Contract> contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    Dictionary<string, DateTime?> contractdetails = contractrow.ToDictionary(k => k.id, k => k.ValueDate);
                    string currntmonth = reportdate.Year + "-" + reportdate.Month;
                    Dictionary<string, long> checkId =
                        (from ct in db.Ctrades
                         where ct.BOtradeTimestamp.ToString().Contains("2010-12")
                         select ct).ToDictionary(k => (k.order_id.ToString() + k.orderPos.ToString()), k => k.fullid);
                   

                    while (!reader.EndOfStream)
                    {
                        lineFromFile = reader.ReadLine();
                        if (lineFromFile == null) continue;
                        rowstring = lineFromFile.Split(Delimiter);
                        string id = string.Concat(rowstring[idorderid], rowstring[idorderPos]);
                        if (!checkId.ContainsKey(id))
                        {
                            DateTime? valuedate;
                            if (!contractdetails.TryGetValue(rowstring[idSymbol], out valuedate))
                            {
                                valuedate = new DateTime(2011, 01, 01);
                                //todo fill correct value date from file
                                var test = new Contract
                                    {
                                        id = rowstring[idSymbol],
                                        Contract1 = rowstring[idSymbol],
                                        Exchange = "Needtoupdate",
                                        Type = "Needtoupdate",
                                        Leverage =
                                            (idcontractMultiplier > (rowstring.Length - 1)) ||
                                            (rowstring[idcontractMultiplier] == "")
                                                ? 1
                                                : double.Parse(rowstring[idcontractMultiplier],
                                                               CultureInfo.InvariantCulture),
                                        ValueDate = valuedate, //Convert.ToDateTime(rowstring[idvalueDate]),
                                        Currency =
                                            idcontractMultiplier > (rowstring.Length - 1)
                                                ? "USD"
                                                : rowstring[idcurrency],
                                        Margin = 0,
                                        FlatMargin = 0,
                                        Canbesettled = true,
                                        UpdateDate = DateTime.UtcNow,
                                        commission =
                                            double.Parse(rowstring[idfees], CultureInfo.InvariantCulture)/
                                            double.Parse(rowstring[idqty], CultureInfo.InvariantCulture),
                                        Timestamp = DateTime.UtcNow,
                                        valid = 1,
                                        username = "TradeParser"
                                    };
                                db.Contracts.Add(test);
                                SaveDBChanges(ref db);
                                contractrow =
                                    from ct in db.Contracts
                                    where ct.valid == 1
                                    select ct;
                                contractdetails = contractrow.ToDictionary(k => k.id, k => k.ValueDate);
                            }
                            int side = 1;
                            if (rowstring[idside] == "sell") side = -1;
                            DateTime vBOtradeTimestamp = Convert.ToDateTime(rowstring[idDate]);
                            if (rowstring[idSymbol].IndexOf(template) > 0)
                            {
                                DateTime fortscurrentDate = Convert.ToDateTime(rowstring[idDate]);
                                string initialdate = fortscurrentDate.ToShortDateString();
                                fortscurrentDate = fortscurrentDate.AddHours(24 - nextdaystarthour + GMToffset);
                                if (initialdate != fortscurrentDate.ToShortDateString())
                                    fortscurrentDate = nextdayvalueform;
                                rowstring[idDate] = fortscurrentDate.ToShortDateString();
                            }
                            index++;
                            if (index > 0)
                            {
                                /*  var ExchangeOrderId = rowstring[idexchangeOrderId];
                                var account_id = rowstring[idAccount];
                                var Date = Convert.ToDateTime(rowstring[idDate]);
                                var symbol_id = rowstring[idSymbol];
                                var qty = rowstring[idqty].IndexOf(".") == -1
                                              ? Convert.ToInt64(rowstring[idqty])*side
                                              : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)*side;
                                var price = double.Parse(rowstring[idprice], CultureInfo.InvariantCulture);
                                var cp_id = rowstring[idcounterparty];
                                var fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture);
                                var value_date = valuedate; //Convert.ToDateTime(rowstring[idvalueDate]),
                                var currency = idcontractMultiplier > (rowstring.Length - 1)
                                                   ? "USD"
                                                   : rowstring[idcurrency];
                                var Timestamp = DateTime.UtcNow;
                                var username = rowstring[iduser];
                                var order_id = rowstring[idorderid];
                                //  var gatewayId = rowstring[idgateway];
                                var BOtradeTimestamp = vBOtradeTimestamp;
                                var mty = double.Parse(rowstring[idcontractMultiplier], CultureInfo.InvariantCulture);
                                var SettlementCp = rowstring[idSettlementCp];
                                var Value = double.Parse(rowstring[idtradedVolume], CultureInfo.InvariantCulture);
                                /*    var cptimestamp = rowstring[idcptime]==""
                                                        ? null
                                                        : Convert.ToDateTime(rowstring[idcptime]);*/
                                db.Ctrades.Add(new Ctrade
                                    {
                                        ExchangeOrderId = rowstring[idexchangeOrderId],
                                        account_id = rowstring[idAccount],
                                        Date = Convert.ToDateTime(rowstring[idDate]),
                                        symbol_id = rowstring[idSymbol],
                                        qty = rowstring[idqty].IndexOf(".") == -1
                                                  ? Convert.ToInt64(rowstring[idqty])*side
                                                  : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)*side,
                                        price = double.Parse(rowstring[idprice], CultureInfo.InvariantCulture),
                                        cp_id = rowstring[idcounterparty],
                                        fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture),
                                        value_date = valuedate,
                                        currency = idcontractMultiplier > (rowstring.Length - 1)
                                                       ? "USD"
                                                       : rowstring[idcurrency],
                                        orderPos = Convert.ToInt32(rowstring[idorderPos]),
                                        Timestamp = DateTime.UtcNow,
                                        valid = 1,
                                        username = rowstring[iduser],
                                        order_id = rowstring[idorderid],
                                        // gatewayId = rowstring[idgateway],
                                        BOtradeTimestamp = vBOtradeTimestamp,
                                        tradeType = rowstring[idtradeType],
                                        SettlementCp = rowstring[idSettlementCp],
                                        Value =
                                            -side*
                                            Math.Abs(double.Parse(rowstring[idtradedVolume],
                                                                  CultureInfo.InvariantCulture)),
                                        mty =
                                            (Int64)
                                            double.Parse(rowstring[idcontractMultiplier], CultureInfo.InvariantCulture),
                                        deliveryDate = rowstring[idvalueDate] == ""
                                                           ? Convert.ToDateTime(rowstring[idDate])
                                                           : Convert.ToDateTime(rowstring[idvalueDate]),
                                        EntityLegalMalta = checkMalta
                                    });
                                if (index%100 == 0) SaveDBChanges(ref db);
                            }
                        }
                        else
                        {
                            LogTextBox.AppendText("\r\n" + "Same Id exists in BO: " + id);
                        }
                    }
                }
                TradesParserStatus.Text = "DB updating";

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                                   validationError.ErrorMessage);
                        }
                    }
                }
                db.Database.ExecuteSqlCommand("CALL updateTradeNumbers()");

                db.Dispose();
                TradesParserStatus.Text = "Done";
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "BO trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
                LogTextBox.AppendText("\r\n" + index.ToString() + " trades have been added.");
            }

            Console.WriteLine(result); // <-- For debugging use. 
        }

        //todo get trades from DB BO   
        private List<Ctrade> getTradesFromDB(DateTime reportdate, List<string> cplist, bool removeReconciled,
                                             List<string> settCp, bool maltaentity)
        {
            var db = new EXANTE_Entities(_currentConnection);
            DateTime prevreportdate = reportdate.AddDays(-(double) (numericUpDown1.Value));
            var ts = new TimeSpan(16, 00, 0);

            prevreportdate = prevreportdate.Date + ts;

            DateTime nextdate = reportdate.AddDays(4);
            var boTradeNumberlist = new List<long?>();
            if (removeReconciled)
            {
                IQueryable<string> boTradeNumbers = db.CpTrades.Where(
                    cptrade => cptrade.valid == 1 && cptrade.ReportDate >= reportdate.Date &&
                               cptrade.ReportDate < (nextdate.Date) && cptrade.BOTradeNumber != null)
                                                      .Select(cptrade => cptrade.BOTradeNumber);
                foreach (string boTradeNumber in boTradeNumbers)
                {
                    string[] templist = boTradeNumber.Split(';');
                    try
                    {
                        boTradeNumberlist.AddRange(
                            templist.Select(s => !string.IsNullOrEmpty(s) ? (long?) Convert.ToInt64(s) : null));
                    }
                    catch (Exception e2)
                    {
                        throw;
                    }
                }
                //   boTradeNumberlist.AddRange(boTradeNumbers.ToList().Select(s => (long?) Convert.ToInt64(s)));
            }
            /* var queryable = from ct in db.Ctrades
                        where ct.valid == 1 && ct.Date >= reportdate.Date && ct.Date < (nextdate.Date) &&
                              cplist.Contains(ct.cp_id) && !boTradeNumberlist.Contains(ct.tradeNumber)
                        select ct;*/
            IQueryable<Ctrade> queryable = from ct in db.Ctrades
                                           where
                                               ct.valid == 1 && ct.RecStatus == false &&
                                               ct.BOtradeTimestamp >= prevreportdate &&
                                               ct.Date < (nextdate.Date)
                                              && cplist.Contains(ct.cp_id)   
                                              && settCp.Contains(ct.SettlementCp)
                                              && ct.EntityLegalMalta == maltaentity
                                            //   && settCp.Contains(ct.cp_id)
                                           select ct;

            return queryable.ToList();
        }

        private Array getBOtoABNMapping()
        {
            var db = new EXANTE_Entities(_currentConnection);
            var queryable =
                from ct in db.Mappings
                where ct.valid == 1 && ct.Type == "Cp"
                select new {ct.BrockerSymbol, ct.BOSymbol};
            return queryable.ToArray();
        }

        private Dictionary<int, string> GetCPmapping()
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            return cpfromDb.ToDictionary(k => k.cp_id, k => k.Name);
        }


        private string FXFWDupdate(string str)
        {
            int indexE2 = str.IndexOf('.') + 1;
            if (indexE2 == 0)
            {
                indexE2 = str.IndexOf("A3");
                if (indexE2 == 0)
                {
                    indexE2 = str.IndexOf("E4");
                }
            }
            string currency = str.Substring(0, indexE2 - 1);
            //  currency=currency.Replace('/');
            if ((str.IndexOf("SPOT") == -1) && (str.IndexOf("EXANTE") == -1) && (str.IndexOf("E6") == -1) &&
                (str.IndexOf("E5") == -1))
            {
                string Date = str.Substring(indexE2 + 3, str.Length - indexE2 - 3);
                //  var month  =Date.match(/\w+/);
                //  var validRegExp = /['A'-'z']+/;
                /*  
        //    var month = validRegExp.Exec(Date);
            if (month!=null){
            month=month[0];
            var monthDigit;
            switch(month)
            {
              case "F":
                    monthDigit = "01";
                break;   
              case "G":
                    monthDigit = "02";
                break;    
              case "H":
                    monthDigit = "03";
                break;   
              case "J":
                    monthDigit = "04";
                break; 
              case "K":
                    monthDigit = "05";
                break;   
              case "M":
                    monthDigit = "06";
                break; 
              case "N":
                    monthDigit = "07";
                break; 
              case "O":
                    monthDigit = "08";
                break;    
              case "U":
                    monthDigit = "09";
                break;
              case "V":
                    monthDigit = "10";
                break;    
              case "X":
                    monthDigit = "11";
                break;
              case "Z":
                    monthDigit = "12";
                break;
              default:
                    MonthDigit = "";
            }
    var indexMonth=Date.IndexOf(month);
    var dayDigit = Date.Substring(0,indexMonth);
    if (Convert.ToInt16(dayDigit)<10)dayDigit="0"+dayDigit;
    var YearDigit = Date.Substring(indexMonth+1,Date.Length-indexMonth-1);
    currency=currency.Concat(YearDigit,monthDigit,dayDigit);
  } */
            }
            else
            {
                currency = currency + "FX";
            }
            return currency;
        }

        private void AbnRecon(DateTime reportdate, List<CpTrade> trades, string ccp,bool maltaentity)
        {
            var cplist = new List<string>
                {
                    "LEK",
                    "CQG",
                    "FASTMATCH",
                    "CURRENEX",
                    "EXANTE",
                    "AMRO",
                    "PATS",
                    "ADSS",
                    "OPEN",
                    "MOEX",
                    "CFH",
                    "MOEX-SPECTRA",
                    "MOEX-ASTS",
                    "IS-PRIME",
                    "IB",
                    "INSTANT",
                    "LMAX",
                    ""
                };
            bool mltytrades = MultyTradesCheckBox.Checked;
            var batchsize = 300;
            bool skipspr = SkipspreadcheckBox.Checked;
            var db = new EXANTE_Entities(_currentConnection);
            List<string> SettCp = (from ct in db.cpmapping
                                   where
                                       ct.cp.Contains(ccp)
                                   select ct.bosettcp).ToList();


            Dictionary<string, List<Ctrade>> boTradeslist =
                CreateIdForBoTrades(getTradesFromDB(reportdate, cplist, true, SettCp, maltaentity));
            int numberBoTrades = boTradeslist.Count;
            Array cpmapping = getBOtoABNMapping();
            Dictionary<string, Map> bomap = getMap(ccp);
            List<CpTrade> abnTradeslist = CreateIdForCpTrades(getOnlyTrades(trades), ccp);
            var recon = new List<Reconcilation>();
            var ii = 1;
            foreach (CpTrade cpTrade in abnTradeslist)
            {
                List<Ctrade> ctrade;
                if (boTradeslist.TryGetValue(cpTrade.Id, out ctrade))
                {
                    UpdateRecTrades(cpTrade, ctrade, db, recon);
                    ctrade.RemoveAt(0);
                    if (ctrade.Count == 0)
                    {
                        boTradeslist.Remove(cpTrade.Id);
                    }
                }
                else
                {
                    if (mltytrades)
                    {
                        List<Ctrade> reclist = CheckMultitrades(cpTrade, boTradeslist.Values.SelectMany(x => x).ToList());
                        if (reclist != null)
                        {
                            int n = reclist.Count;
                            for (int i = 0; i < n; i++)
                            {
                                string keysWithMatchingValues =
                                    boTradeslist.Where(p => p.Value[0].fullid == reclist[0].fullid)
                                                .Select(p => p.Key)
                                                .FirstOrDefault();
                                UpdateRecTrades(cpTrade, reclist, db, recon);
                                reclist.RemoveAt(0);
                                if (boTradeslist[keysWithMatchingValues].Count == 1)
                                {
                                    boTradeslist.Remove(keysWithMatchingValues);
                                }
                                else
                                {
                                    boTradeslist[keysWithMatchingValues].RemoveAt(0);
                                }
                            }
                        }
                    }
                }
                ii++;
                if (ii % batchsize == 0) SaveDBChanges(ref db);
             }
            SaveDBChanges(ref db);
            ii = 1;
            for (int j = boTradeslist.Count - 1; j >= 0; j--)
            {
                string currentkey = boTradeslist.Keys.ElementAt(j);
                List<Ctrade> valuePair = boTradeslist[currentkey];
                for (int listindex = 0; listindex < valuePair.Count; listindex++)
                {
                    Ctrade ctrade = valuePair[listindex];
                    var reclist = new List<CpTrade>();

                    if (!SkipspreadcheckBox.Checked)
                    {
                        if ((ctrade.symbol_id.Contains(".CS/")) || (ctrade.symbol_id.Contains(".RS/")))
                        {
                            List<long> reclistids = workeithCS(ctrade, abnTradeslist, false);
                            reclist.AddRange(
                                reclistids.Select(
                                    t => abnTradeslist.Where(item => (item.FullId == t)).FirstOrDefault()));
                            double leftsum = 0;
                            double rightsum = 0;
                            foreach (CpTrade cpTrade in reclist)
                            {
                                double? cqty = cpTrade.Qty;
                                if (cpTrade.Qty > 0)
                                {
                                    leftsum = (double) (leftsum + cqty);
                                }
                                else
                                {
                                    rightsum = (double) (rightsum + cqty);
                                }
                            }
                        }
                    }
                    if (reclist.Count == 0)
                    {
                        reclist = CheckMultitradesBack(ctrade,
                                                       abnTradeslist.Where(x => (x.BOTradeNumber == null)).ToList());
                    }

                    if (reclist != null)
                    {
                        int n = reclist.Count;
                        for (int i = 0; i < n; i++)
                        {
                            var templist = new List<Ctrade> {ctrade};
                            UpdateRecTrades(reclist[i], templist, db, recon);
                        }
                     //   SaveDBChanges(ref db);
                        boTradeslist[currentkey].RemoveAt(listindex);
                        listindex--;
                    }
                }
                if (valuePair.Count == 0)
                {
                    boTradeslist.Remove(currentkey);
                }
                ii++;
                if (ii % batchsize == 0) SaveDBChanges(ref db);
            }
            SaveDBChanges(ref db);
            ii = 1;
            if (mltytrades)
            {
                for (int j = boTradeslist.Count - 1; j >= 0; j--)
                {
                    string currentkey = boTradeslist.Keys.ElementAt(j);
                    List<Ctrade> valuePair = boTradeslist[currentkey];
                    for (int listindex = 0; listindex < valuePair.Count; listindex++)
                    {
                        Ctrade ctrade = valuePair[listindex];
                        var reclist = new List<CpTrade>();

                        if (!SkipspreadcheckBox.Checked)
                        {
                            if ((ctrade.symbol_id.Contains(".CS/")) || ctrade.symbol_id.Contains(".RS/"))
                            {
                                List<long> reclistids = workeithCS(ctrade, abnTradeslist, true);
                                reclist.AddRange(
                                    reclistids.Select(
                                        t => abnTradeslist.Where(item => (item.FullId == t)).FirstOrDefault()));
                                double leftsum = 0;
                                double rightsum = 0;
                                foreach (CpTrade cpTrade in reclist)
                                {
                                    double? cqty = cpTrade.Qty;
                                    if (cpTrade.Qty > 0)
                                    {
                                        leftsum = (double) (leftsum + cqty);
                                    }
                                    else
                                    {
                                        rightsum = (double) (rightsum + cqty);
                                    }
                                }
                            }
                        }
                        if (reclist.Count == 0)
                        {
                            reclist = CheckMultitradesBack(ctrade,
                                                           abnTradeslist.Where(x => (x.BOTradeNumber == null)).ToList());
                        }

                        if (reclist != null)
                        {
                            int n = reclist.Count;
                            for (int i = 0; i < n; i++)
                            {
                                var templist = new List<Ctrade> {ctrade};
                                UpdateRecTrades(reclist[i], templist, db, recon);
                            }
                          //  SaveDBChanges(ref db);
                            boTradeslist[currentkey].RemoveAt(listindex);
                            listindex--;
                        }
                    }
                    if (valuePair.Count == 0)
                    {
                        boTradeslist.Remove(currentkey);
                    }
                    ii++;
                    if (ii % batchsize == 0) SaveDBChanges(ref db);
                }
                SaveDBChanges(ref db);
            }
            ii = 1;
            
            DateTime TimeStart = DateTime.Now;
            foreach (Reconcilation reconcilation in recon)
            {
                db.Reconcilations.Add(reconcilation);
                if (ii%batchsize == 0)
                        {
                            
                            SaveDBChanges(ref db);
                            DateTime TimeEnd = DateTime.Now;
                            LogTextBox.AppendText("\r\n" + "Rec uploading time: " +(TimeEnd - TimeStart).ToString());
                            TimeStart = DateTime.Now;
                        }
            }
            SaveDBChanges(ref db);
        }

        private void MacRecon(DateTime reportdate, List<CpTrade> trades)
        {
            var cplist = new List<string> {"CQG", "PATS"};
            Dictionary<string, List<Ctrade>> boTradeslist =
                CreateIdForBoTrades(getTradesFromDB(reportdate, cplist, true, null,true));
            Array cpmapping = getBOtoABNMapping();
            Dictionary<string, Map> bomap = getMap("Mac");
            List<CpTrade> TradeList = CreateIdForCpTrades(getOnlyTrades(trades), "Mac");
            var recon = new List<Reconcilation>();
            var db = new EXANTE_Entities(_currentConnection);
            foreach (CpTrade cpTrade in TradeList)
            {
                List<Ctrade> ctrade;
                if (boTradeslist.TryGetValue(cpTrade.Id, out ctrade))
                {
                    UpdateRecTrades(cpTrade, ctrade, db, recon);
                    ctrade.RemoveAt(0);
                    if (ctrade.Count == 0)
                    {
                        boTradeslist.Remove(cpTrade.Id);
                    }
                }
                else
                {
                }
            }
            db.SaveChanges();
            foreach (Reconcilation reconcilation in recon)
            {
                db.Reconcilations.Add(reconcilation);
            }
            db.SaveChanges();
        }


        private List<long> workeithCS(Ctrade ctrade, List<CpTrade> abnTradeslist, Boolean mtytrades)
        {
            int inndexcs = ctrade.symbol_id.IndexOf(".CS/");
            int mty = 1;
            if (inndexcs == -1)
            {
                inndexcs = ctrade.symbol_id.IndexOf(".RS/");
                mty = -1;
            }
            int indexseparate = ctrade.symbol_id.IndexOf("-");
            string leftside = ctrade.symbol_id.Substring(0, inndexcs + 1) +
                              ctrade.symbol_id.Substring(inndexcs + 4, indexseparate - inndexcs - 4);
            string vd = getValueDate(leftside);
            double Cqty = (double) ctrade.qty*mty;
            double? spreadprice = ctrade.price*mty;
            string rightside = ctrade.symbol_id.Substring(0, inndexcs + 1) +
                               ctrade.symbol_id.Substring(indexseparate + 1, ctrade.symbol_id.Length - indexseparate - 1);
            var leftalltrades =
                abnTradeslist.Where(item => ((item.BOSymbol == leftside) && (item.BOTradeNumber == null)))
                             .Select(item => new {qty = item.Qty, price = item.Price, id = item.FullId});
            if (!mtytrades)
                leftalltrades = leftalltrades.Where(item => (Math.Abs((double) item.qty) == Math.Abs(Cqty)));
            var righttalltrades =
                abnTradeslist.Where(item => ((item.BOSymbol == rightside) && (item.BOTradeNumber == null)))
                             .Select(item => new {qty = item.Qty, price = item.Price, id = item.FullId});
            if (!mtytrades)
                righttalltrades = righttalltrades.Where(item => (Math.Abs((double) item.qty) == Math.Abs(Cqty)));
            List<double?> pricelist = leftalltrades.Select(item => item.price).Distinct().ToList();
            int indexprice = 0;
            bool pairfound = false;
            var reclist = new List<long>();
            while (indexprice < pricelist.Count && !pairfound)
            {
                double? currentprice = pricelist[indexprice];
                List<Trade> leftossibleletrades =
                    leftalltrades.Where(item => (item.price == currentprice))
                                 .Select(item => new Trade {id = item.id, qty = (double) item.qty})
                                 .ToList();
                leftossibleletrades = Samesidetrades(Cqty, leftossibleletrades);
                double sum = 0;
                foreach (Trade sumtrade in leftossibleletrades)
                {
                    sum = sum + sumtrade.qty;
                }

                if (Math.Abs(sum) >= Math.Abs(Cqty))
                {
                    List<int> leftreclist = CheckMultitradesNew(Cqty, leftossibleletrades);
                    if (leftreclist != null)
                    {
                        reclist.Clear();
                        for (int i = 0; i < leftreclist.Count; i++)
                        {
                            reclist.Add(leftossibleletrades[leftreclist[i]].id);
                        }
                        double? rightpathprice = (currentprice - spreadprice);
                        rightpathprice = Math.Round((double) rightpathprice, 8);
                        List<Trade> rightpossibleletrades =
                            righttalltrades.Where(item => (item.price == rightpathprice))
                                           .Select(item => new Trade {id = item.id, qty = (double) item.qty})
                                           .ToList();
                        rightpossibleletrades = Samesidetrades(-Cqty, rightpossibleletrades);
                        double rightsum = 0;
                        foreach (Trade sumtrade in rightpossibleletrades)
                        {
                            rightsum = rightsum + sumtrade.qty;
                        }

                        if (Math.Abs(rightsum) >= Math.Abs(Cqty))
                        {
                            double rightcty = -Cqty;
                            List<int> rightreclist = CheckMultitradesNew(rightcty, rightpossibleletrades);
                            if (rightreclist != null)
                            {
                                for (int i = 0; i < rightreclist.Count; i++)
                                {
                                    reclist.Add(rightpossibleletrades[rightreclist[i]].id);
                                }
                                pairfound = true;
                            }
                            else
                            {
                                reclist.Clear();
                            }
                            // var indexReclist = 0;
                            // pairfound = true;

                            /*     while ((indexReclist < leftreclist.Count) && (pairfound))
                            {
                                var testid = (int) leftreclist[indexReclist];
                                var CrtRecListQty = -leftossibleletrades.ElementAt((int) leftreclist[indexReclist]).qty;
                              //  var rightid = rightpossibleletrades.Where(item => (item.qty == CrtRecListQty)).Select(item => item.id).FirstOrDefault();
                                var rightidtrade = rightpossibleletrades.Where(item => (item.qty == CrtRecListQty)).Select(item => item).FirstOrDefault();
                               // var rightqty = rightidtrade.qty;
                                var rightid = rightidtrade.id;
                             //   List<Trade> righttrade = rightpossibleletrades.Where(item => (item.id == rightid)).Select(item => item).ToList(); //new Trade { id = item.id, qty = (double)item.qty }).ToList();
                                double rightqty = 0;
                                rightqty=rightidtrade.qty;
                                if (rightid != 0)
                                {
                                    reclist.Add(rightid);
                                    rightpossibleletrades =
                                        rightpossibleletrades.Where(item => (item.id != rightid)).Select(item => item).ToList();//new Trade { id = item.id, qty = (double)item.qty }).ToList();
                                 //   .RemoveAt(0);
                                    indexReclist++;
                                }
                                else
                                {
                                    pairfound = false;
                                }
                            }
                            if (!pairfound)
                            {
                                var rightreclist = CheckMultitradesNew(-Cqty, rightpossibleletrades);
                                reclist.Clear();
                                if (rightreclist != null)
                                {
                                    for (var i = 0; i < rightreclist.Count; i++)
                                    {
                                        reclist.Add(rightpossibleletrades[i].id);
                                    }
                                }
                            }*/
                        }
                        else reclist.Clear();
                    }
                }
                indexprice++;
            }

            /* if (pairfound)
            {
             /*   var templist = new List<Ctrade> {ctrade};
         //       var cpTrade= abnTradeslist.Where(item => (item.FullId == leftalltrades. )
         //       UpdateRecTrades(reclist[i], templist, db, boTradeslist, recon);

                var n = reclist.Count;
                   for (var i = 0; i < n; i++)
                            {
                                var keysWithMatchingValues =
                                    abnTradeslist.Where(p => p.Value[0].fullid == reclist[0].fullid)
                                                .Select(p => p.Key)
                                                .FirstOrDefault();
                                UpdateRecTrades(cpTrade, reclist, db, boTradeslist, recon);
                                reclist.RemoveAt(0);
                                if (abnTradeslist[keysWithMatchingValues].Count == 1)
                                {
                                    abnTradeslist.Remove(keysWithMatchingValues);
                                }
                                else
                                {
                                    abnTradeslist[keysWithMatchingValues].RemoveAt(0);
                                }
                            }
                
                return reclist;
            }*/
            return reclist;
        }

        private static List<Trade> Samesidetrades(double qty, List<Trade> trades)
        {
            List<Trade> possibleletrades;
            if (qty > 0)
            {
                IEnumerable<Trade> allpossibleletrades =
                    trades.Where(item => (item.qty > 0 && Math.Abs(item.qty) <= Math.Abs(qty)));
                possibleletrades = allpossibleletrades.OrderByDescending(o => o.qty).ToList();
            }
            else
            {
                IEnumerable<Trade> allpossibleletrade =
                    trades.Where(item => (item.qty < 0 && Math.Abs(item.qty) <= Math.Abs(qty)));
                possibleletrades = allpossibleletrade.OrderBy(o => o.qty).ToList();
            }
            return possibleletrades;
        }

        private string getValueDate(string leftside)
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<DateTime?> mapfromDb = from c in db.Contracts
                                              where c.id == leftside
                                              select c.ValueDate;
            if (mapfromDb.FirstOrDefault() != null) return mapfromDb.FirstOrDefault().Value.ToShortDateString();
            else return null;
        }

        private List<CpTrade> CheckMultitradesBack(Ctrade ctrade, List<CpTrade> ABNtrades)
        {
            List<long> sequence = null;
            List<CpTrade> listBoTrades = null;
            if (ctrade != null)
            {
                //var sameqty = ABNtrades.Where(item => (item.BOSymbol == ctrade.symbol_id && item.Price == ctrade.price&&));

                IEnumerable<CpTrade> possibletrades =
                    ABNtrades.Where(item => (item.BOSymbol == ctrade.symbol_id && item.Price == ctrade.price));
                //var accounts = possibletrades.GroupBy(x => x.).Select(g => g.First().account_id).ToList();

                if (ctrade.qty > 0)
                {
                    possibletrades = possibletrades.Where(item => item.Qty > 0);
                    possibletrades = possibletrades.OrderByDescending(o => o.Qty);
                }
                else
                {
                    possibletrades = possibletrades.Where(item => item.Qty < 0);
                    possibletrades = possibletrades.OrderBy(o => o.Qty);
                }
                sequence = new List<long>();
                if (possibletrades.Count() > 0)
                {
                    if (ctrade.qty == possibletrades.ElementAt(0).Qty)
                    {
                        if (possibletrades.ElementAt(0).BOTradeNumber != null)
                        {
                            sequence.Add(possibletrades.ElementAt(0).FullId);
                            listBoTrades.Add(possibletrades.ElementAt(0));
                        }
                    }
                    else
                    {
                        int i = 0;
                        double qty = 0;
                        while ((i < possibletrades.Count()) && (qty != ctrade.qty))
                        {
                            if (Math.Abs((double) possibletrades.ElementAt(i).Qty) < Math.Abs((double) ctrade.qty))
                            {
                                qty = (double) possibletrades.ElementAt(i).Qty;
                                if (sequence.Count == 0) sequence.Add(i);
                                else sequence[0] = i;
                                qty = calculateQtyBack(ctrade.qty, qty, i + 1, possibletrades.ToList(), sequence, 1);
                                if (qty != ctrade.qty) i++;
                            }
                            else i++;
                        }
                        if ((qty == ctrade.qty) && (sequence.Count > 0))
                        {
                            listBoTrades = new List<CpTrade> {possibletrades.ElementAt((int) sequence[0])};
                            for (i = 1; i < sequence.Count; i++)
                            {
                                listBoTrades.Add(possibletrades.ElementAt((int) sequence[i]));
                            }
                        }
                    }
                }
            }
            return listBoTrades;
        }

        private double calculateQtyBack(double? InitialQty, double qty, int i, List<CpTrade> possibletrades,
                                        List<long> sequence, int level)
        {
            double nextValue = 0;
            if (i < possibletrades.Count)
            {
                nextValue = (double) possibletrades[i].Qty;
            }
            while ((i < possibletrades.Count) && ((qty) != InitialQty))
            {
                if (Math.Abs(nextValue + qty) <= Math.Abs((double) InitialQty))
                {
                    qty = nextValue + qty;
                    if (sequence.Count == level) sequence.Add(i);
                    else sequence[level] = i;
                    if (qty != InitialQty)
                        qty = calculateQtyBack(InitialQty, qty, i + 1, possibletrades, sequence, level + 1);
                }
                else
                {
                    i++;
                    if (i < possibletrades.Count) nextValue = (double) possibletrades[i].Qty;
                }
            }
            return qty;
        }

        private static void UpdateRecTrades(CpTrade cpTrade, List<Ctrade> ctrade, EXANTE_Entities db,
                                            List<Reconcilation> recon)
        {
            long? botradenumber = ctrade[0].tradeNumber;
            if (cpTrade.BOTradeNumber == null)
            {
                cpTrade.BOTradeNumber = botradenumber.ToString();
            }
            else
            {
                cpTrade.BOTradeNumber = cpTrade.BOTradeNumber + ";" + botradenumber.ToString();
                if (cpTrade.BOTradeNumber.Length > 600)
                {
                   cpTrade.BOTradeNumber = "1";
                }

            }
            cpTrade.BOcp = ctrade[0].cp_id;
            cpTrade.BOSymbol = ctrade[0].symbol_id;
            cpTrade.Comment = ctrade[0].BOtradeTimestamp.Value.ToShortDateString();
            ctrade[0].RecStatus = true;
            db.CpTrades.Attach(cpTrade);
            db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
            db.Ctrades.Attach(ctrade[0]);
            db.Entry(ctrade[0]).State = (EntityState)System.Data.Entity.EntityState.Modified;


            recon.Add(new Reconcilation
                {
                    CpFull_id = cpTrade.FullId,
                    BOTradenumber = botradenumber,
                    Timestamp = DateTime.UtcNow,
                    username = "TradeParser",
                    valid = 1
                });

          //  SaveDBChanges(ref db);
        }

        private List<int> CheckMultitradesNew(double initialQty, List<Trade> possibletrades)
        {
            List<int> sequence = null;
            if (initialQty != 0)
            {
                if (possibletrades.Count() > 0)
                {
                    sequence = new List<int>();
                    if (initialQty == possibletrades.ElementAt(0).qty)
                    {
                        if (possibletrades.ElementAt(0) != null)
                        {
                            sequence.Add(0);
                        }
                    }
                    else
                    {
                        int i = 0;
                        double qty = 0;
                        while ((i < possibletrades.Count()) && (qty != initialQty))
                        {
                            qty = possibletrades.ElementAt(i).qty;
                            sequence.Clear();
                            sequence.Add(i);
                            qty = calculateQtyNew(initialQty, qty, i + 1, possibletrades.ToList(), sequence, 1);
                            if (qty != initialQty) i++;
                        }
                        if ((qty != initialQty)) //||(sequence.Count == 1))
                        {
                            sequence = null;
                        }
                    }
                }
            }
            return sequence;
        }

        private double calculateQtyNew(double? InitialQty, double qty, int i, List<Trade> possibletrades,
                                       List<int> sequence, int level)
        {
            double nextValue = 0;
            if (i < possibletrades.Count)
            {
                nextValue = possibletrades[i].qty;
            }
            while ((i < possibletrades.Count) && ((qty) != InitialQty))
            {
                if (Math.Abs(nextValue) + Math.Abs(qty) <= Math.Abs((double) InitialQty))
                {
                    qty = nextValue + qty;
                    if (sequence.Count == level) sequence.Add(i);
                    else
                    {
                        sequence[level] = i;
                        if (sequence.Count > level + 1) sequence.RemoveAt(level + 1);
                    }
                    if (qty != InitialQty)
                    {
                        double nextlevelqty = calculateQtyNew(InitialQty, qty, i + 1, possibletrades, sequence,
                                                              level + 1);
                        if (nextlevelqty != InitialQty)
                        {
                            i++;
                            if (sequence.Count > level + 1) sequence.RemoveAt(level + 1);
                            qty = qty - nextValue;
                        }
                        else
                        {
                            qty = nextlevelqty;
                        }
                    }
                }
                else
                {
                    i++;
                    if (i < possibletrades.Count) nextValue = possibletrades[i].qty;
                }
            }
            return qty;
        }


        private List<Ctrade> CheckMultitrades(CpTrade trade, List<Ctrade> boTrades)
        {
            List<long> sequence = null;
            List<Ctrade> listBoTrades = null;
            if (trade != null)
            {
                string symbol = trade.BOSymbol;
                double? price = trade.Price;
                //   bool positiveqtyflag = !(trade.Qty < 0);
                double? initialQty = trade.Qty;
                //      if ((boTrades[i].symbol_id == symbol && boTrades[i].price == price) && (boTrades[i].qty > 0 && positiveqtyflag && (Math.Abs((double)boTrades[i].qty) < qtyflag))) possibletrades.Add(boTrades[i]);
                //      var accounts = boTrades.GroupBy(x => x.account_id).Select(g => g.First().account_id).ToList();
                IEnumerable<Ctrade> possibletrades =
                    boTrades.Where(item => (item.symbol_id == symbol && item.price == price));
                List<string> accounts =
                    possibletrades.GroupBy(x => x.account_id).Select(g => g.First().account_id).ToList();

                /****/
                if (trade.Qty > 0)
                {
                    possibletrades =
                        possibletrades.Where(
                            item => (item.qty > 0 && Math.Abs((double) item.qty) < Math.Abs((double) initialQty)));
                    possibletrades = possibletrades.OrderByDescending(o => o.qty);
                }
                else
                {
                    possibletrades =
                        possibletrades.Where(
                            item => (item.qty < 0 && Math.Abs((double) item.qty) < Math.Abs((double) initialQty)));
                    possibletrades = possibletrades.OrderBy(o => o.qty);
                }

                sequence = new List<long>();
                if (possibletrades.Count() > 0)
                {
                    if (trade.Qty == possibletrades.ElementAt(0).qty)
                    {
                        if (possibletrades.ElementAt(0).tradeNumber != null)
                        {
                            sequence.Add(possibletrades.ElementAt(0).fullid);
                            listBoTrades.Add(possibletrades.ElementAt(0));
                        }
                    }
                    else
                    {
                        int i = 0;
                        double qty = 0;
                        while ((i < possibletrades.Count()) && (qty != initialQty))
                        {
                            qty = (double) possibletrades.ElementAt(i).qty;
                            if (sequence.Count == 0) sequence.Add(i);
                            else sequence[0] = i;
                            qty = calculateQty(trade.Qty, qty, i + 1, possibletrades.ToList(), sequence, 1);
                            if (qty != trade.Qty) i++;
                        }
                        if (((qty == trade.Qty)) && (sequence.Count > 0))
                        {
                            listBoTrades = new List<Ctrade> {possibletrades.ElementAt((int) sequence[0])};
                            for (i = 1; i < sequence.Count; i++)
                            {
                                listBoTrades.Add(possibletrades.ElementAt((int) sequence[i]));
                            }
                        }
                    }
                }


                /****/
            }
            return listBoTrades;
        }

        private double calculateQty(double? InitialQty, double qty, int i, List<Ctrade> possibletrades,
                                    List<long> sequence, int level)
        {
            //    private double calculateQty(double InitialQty,qty,i,possibletrades,Sequence,level){

            double nextValue = 0;
            if (i < possibletrades.Count)
            {
                nextValue = (double) possibletrades[i].qty;
            }
            while ((i < possibletrades.Count) && ((qty) != InitialQty))
            {
                if (Math.Abs(nextValue + qty) <= Math.Abs((double) InitialQty))
                {
                    qty = nextValue + qty;
                    if (sequence.Count == level) sequence.Add(i);
                    else sequence[level] = i;
                    if (qty != InitialQty)
                        qty = calculateQty(InitialQty, qty, i + 1, possibletrades, sequence, level + 1);
                    if (qty != InitialQty)
                    {
                        i++;
                        qty = qty - nextValue;
                        if (i < possibletrades.Count) nextValue = (double) possibletrades[i].qty;
                        for (int j = sequence.Count - 1; j > level; j--) sequence.RemoveAt(j);
                    }
                }
                else
                {
                    i++;
                    if (i < possibletrades.Count) nextValue = (double) possibletrades[i].qty;
                }
            }
            return qty;
        }

        private List<CpTrade> getOnlyTrades(List<CpTrade> trades)
        {
            for (int i = 0; i < trades.Count; i++)
            {
                if ((trades[i].TypeOfTrade != "01") && (trades[i].TypeOfTrade == "05"))
                {
                    trades.RemoveAt(i);
                    i--;
                }
            }
            return trades;
        }


        private static List<CpTrade> CreateIdForCpTrades(List<CpTrade> trades, string Brocker)
        {
            Dictionary<string, Map> ABNMap = getMap(Brocker);
            //   var optionDelimeter = ".";
            foreach (CpTrade cpTrade in trades)
            {
                if (cpTrade.BOSymbol == null)
                {
                    cpTrade.Id = "";
                }
                else
                {
                    if (cpTrade.BOTradeNumber == null)
                    {
                        string key = "";
                        /*  if ((cpTrade.Type == "OP"))
                          {
                              Map symbolvalue;
                              if (cpTrade.Symbol.IndexOf(optionDelimeter) > -1)
                              {
                                  key = cpTrade.Symbol.Substring(0, cpTrade.Symbol.IndexOf(optionDelimeter)) + cpTrade.Type;
                              }
                              else
                              {
                                  if (cpTrade.Symbol.IndexOf(" ") > -1)
                                  {
                                      optionDelimeter = " ";
                                      key = cpTrade.Symbol.Substring(0, cpTrade.Symbol.IndexOf(optionDelimeter)) + cpTrade.Type;
                                  }
                              }
                              if(ABNMap.TryGetValue(key,out symbolvalue))
                              {
                                  var daystring = "";
                                  if (symbolvalue.Round == 1) daystring = cpTrade.ValueDate.Value.Day.ToString();
                                  var indexdelimeter = cpTrade.Symbol.IndexOf(optionDelimeter);
                                  var type = cpTrade.Symbol.Substring(indexdelimeter + 1, 1);
                                  indexdelimeter = cpTrade.Symbol.IndexOf(optionDelimeter, indexdelimeter + 1);
                                  var stringstike = "";
                                  if (Brocker == "Lek")
                                  {                                   
                                      indexdelimeter = cpTrade.Symbol.IndexOf(optionDelimeter, indexdelimeter+1)-1;
                                      stringstike = cpTrade.Symbol.Substring(indexdelimeter + 2, cpTrade.Symbol.IndexOf(optionDelimeter, indexdelimeter + 1) - indexdelimeter+2);
                                  }
                                  else {stringstike = cpTrade.Symbol.Substring(indexdelimeter + 2);}
                                  var strike = Convert.ToDouble(stringstike)*symbolvalue.MtyPrice;
                                  stringstike = strike.ToString();
                                  stringstike = stringstike.Replace(optionDelimeter, "_");
                                  key = symbolvalue.BOSymbol + "." + daystring + getLetterOfMonth(cpTrade.ValueDate.Value.Month) + cpTrade.ValueDate.Value.Year +"." + type + stringstike;
                                  cpTrade.BOSymbol = key;
                              }
                          }
                          else
                          {*/
                        key = cpTrade.BOSymbol;
                        //  }
                        //todo убрать эти условия

                        switch (cpTrade.Type)
                        {
                            case "OP":
                                {
                                    break;
                                }
                            case "O":
                                {
                                    break;
                                }
                            case "ST":
                            case "FX":
                            case "FW-E":
                            case "PM":
                                {
                                    key = key + cpTrade.Type;
                                    break;
                                }
                                /*   case "F":
   {
       key = key + "ST";
       break;
   }*/
                            default:
                                string vd = cpTrade.ValueDate.GetValueOrDefault().ToShortDateString();
                                key = key + vd;
                                break;
                        }
                        key = key + cpTrade.Qty.ToString() + cpTrade.Price.ToString();

                        /*
                                        if (cpTrade.Type == "OP") {
                                            key = key + "ST" + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                                        }else{     if ((cpTrade.Type == "ST") || (cpTrade.Type == "FX") || (cpTrade.Type == "FW-E") || (cpTrade.Type == "PM"))
                                        {
                                            key = key + cpTrade.Type + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                                        }
                                        else
                                        {
                                            var vd = cpTrade.ValueDate.GetValueOrDefault().ToShortDateString();
                                            key = key + vd + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                                        }}*/
                        cpTrade.Id = key;
                    }
                }
            }
            return trades;
        }

        private static Dictionary<string, List<Ctrade>> CreateIdForBoTrades(List<Ctrade> boTradeslist)
        {
            var result = new Dictionary<string, List<Ctrade>>();
            var defaultvalue = new DateTime(2011, 1, 1);
            string defaltvd = defaultvalue.ToShortDateString();
            Dictionary<string, Map> bomap = getMap("BO");
            Map symbolvalue;

            foreach (Ctrade botrade in boTradeslist)
            {
                string vd = botrade.value_date.GetValueOrDefault().ToShortDateString();
                string key = botrade.symbol_id;
                if (vd == defaltvd)
                {
                    if (bomap.TryGetValue(key, out symbolvalue))
                    {
                        key = symbolvalue.BOSymbol + symbolvalue.Type;
                    }
                    else
                    {
                        // ((dateindex > -1)&& (Regex.Match(key.Substring(dateindex+3, 1), "[0-9]").Value != ""))
                        int dateindex = botrade.symbol_id.IndexOf("E2");
                        if (!IsOption(botrade.symbol_id))
                        {
                            if (IsFw(botrade.symbol_id) > -1)
                            {
                                dateindex = dateindex + 3;
                                string date = key.Substring(dateindex);
                                string Monthletter = Regex.Match(date, "[A-Z]").Value;
                                int Day = Convert.ToInt32(date.Substring(0, date.IndexOf(Monthletter)));
                                int Year = Convert.ToInt32(date.Substring(date.IndexOf(Monthletter) + 1));
                                int Month = GetMonthFromLetter(Monthletter);
                                var valuedate = new DateTime(Year, Month, Day);
                                string testtt = key.Substring(0, 7).Replace("/", "");
                                key = testtt + valuedate.ToShortDateString();
                            }
                            else
                            {
                                key = key + "ST";
                            }
                        }
                    }
                    key = key + botrade.qty.ToString() + botrade.price.ToString();
                }
                else
                {
                    key = key + vd + botrade.qty.ToString() + botrade.price.ToString();
                }
                if (result.ContainsKey(key))
                {
                    result[key].Add(botrade);
                }
                else result.Add(key, new List<Ctrade> {botrade}); //tempBotrade});
            }

            return result;
        }

        private static int IsFw(string symbolId)
        {
            int dateindex = symbolId.IndexOf("E2");
            if ((dateindex > -1) && (Regex.Match(symbolId.Substring(dateindex + 3, 1), "[0-9]").Value != ""))
            {
                return dateindex;
            }
            else
            {
                return -1;
            }
        }

        private static bool IsOption(string symbolId)
        {
            int amount = Regex.Matches(symbolId, "['.']").Count;
            if (amount == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int GetMonthFromLetter(string month)
        {
            switch (month)
            {
                case "F":
                    return 1;
                case "G":
                    return 2;
                case "H":
                    return 3;
                case "J":
                    return 4;
                case "K":
                    return 5;
                case "M":
                    return 6;
                case "N":
                    return 7;
                case "Q":
                    return 8;
                case "U":
                    return 9;
                case "V":
                    return 10;
                case "X":
                    return 11;
                case "Z":
                    return 12;
                default:
                    return 0;
            }
        }

        private Dictionary<string, Map> getSymbolMap(string brockertype, string types)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var mapfromDb = from m in db.Mappings
                            join c in db.Contracts on m.BOSymbol equals c.id
                            where m.Brocker == brockertype && types.Contains(m.Type)
                            select new
                                {
                                    m.BrockerSymbol,
                                    m.BOSymbol,
                                    m.MtyPrice,
                                    m.MtyVolume,
                                    m.Type,
                                    m.Round,
                                    c.ValueDate
                                };
            var results = new Dictionary<string, Map>();
            var mapfromDblist = mapfromDb.ToList();
            foreach (var item in mapfromDblist)
            {
                string key = item.BOSymbol;
                results.Add(key, new Map
                    {
                        BOSymbol = item.BrockerSymbol,
                        MtyPrice = item.MtyPrice,
                        MtyVolume = item.MtyVolume,
                        Round = item.Round,
                        Type = item.Type,
                        ValueDate = item.ValueDate,
                    });
            }
            return results;
        }

        // todo make uniqueid
        // todo recon

        private List<string> ABNgetRowsFromCliff(List<string> strArray, int startposition, int number, string value)
        {
            var result = new List<string>();
            for (int index = 0; index < strArray.Count; index++)
            {
                string tempstr = strArray[index];
                if (tempstr.Substring(startposition, number) == value)
                {
                    result.Add(tempstr);
                    strArray.RemoveAt(index);
                    index--;
                }
            }
            return result;
        }

        private Dictionary<string, Map> getMapping(string filter)
        {
            var db = new EXANTE_Entities(_currentConnection);
            // var mapfromDb = from map in db.Mappings
            //             where map.valid == 1 && map.Brocker == filter && (!map.Type.Contains("FORTS"))
            //           select map;

            var mapfromDb = from map in db.Mappings
                            join c in db.Contracts on map.BOSymbol equals c.id
                            where map.valid == 1 && map.Brocker == filter && (!map.Type.Contains("FORTS"))
                            select new
                                {
                                    map.BrockerSymbol,
                                    map.BOSymbol,
                                    map.MtyPrice,
                                    map.MtyVolume,
                                    map.Type,
                                    map.Round,
                                    c.ValueDate,
                                    c.Leverage,
                                    map.MtyStrike,
                                    map.UseDayInTicker,
                                    map.calendar,
                                };

            var results = new Dictionary<string, Map>();
            var mapfromDblist = mapfromDb.ToList();
            foreach (var item in mapfromDblist)
            {
                string key = item.BrockerSymbol;
                key = key + item.Type;
                //    if (item.Type == "OP") key = key + "OP";
                results.Add(key, new Map
                    {
                        BOSymbol = item.BOSymbol,
                        MtyPrice = item.MtyPrice,
                        MtyVolume = item.MtyVolume,
                        Round = item.Round,
                        Type = item.Type,
                        ValueDate = item.ValueDate,
                        MtyStrike = item.MtyStrike,
                        UseDayInTicker = item.UseDayInTicker,
                        calendar = item.calendar,
                        Leverage = item.Leverage
                    });
            }
            return results;
        }

        /*  
          private List<Array> ABNTradesParser(List<string> BodyStrArray)
          {
            var RawTradesArray = ABNgetRowsFromCliff(BodyStrArray,0,3,"410");
            var result = new List<Array>();
            if((RawTradesArray!=null)&&(RawTradesArray.Count>0)){ 
            var mappingST = getMapping("STOCK&FX");    
        /*    var mappingFW = getMapping("FW");
            var mapping;
            var result= new Array();
            for (var index =0;index <RawTradesArray.Count;index++){
              var tradecode = RawTradesArray[index].Substring(108,2);
              var code92= RawTradesArray[index].Substring(405,4);
              var typeofTrade = RawTradesArray[index].Substring(60,2);
                if ((code92 == "    "))
                {
                    var tempraw = new Array();
                    var tradedate = RawTradesArray[index].Substring(295, 8);
                    tempraw[0] = getDate(tradedate);
                    tempraw[1] = RawTradesArray[index].Substring(54, 6);

                    var symbol = RemoveChar(RawTradesArray[index].Substring(66, 6), " ");
                    tempraw[2] = symbol;

                    if (typeofTrade == "FW")
                    {
                        mapping = mappingFW;
                    }
                    else mapping = mappingST;

                    var j = Fn.getElementId(mapping, 0, symbol);

                    if (j > -1)
                    {
                        tempraw[10] = mapping[j][1];
                    }
                  else
                   {
                     tempraw[10] = "";
                     mappingST = getMapping("STOCK&FX");
                       mappingFW = getMapping("FW");
                   }
                  tempraw[2] = symbol;
     
                 var valuedate = RawTradesArray[index].Substring(303,8);
                 if (valuedate =="00000000")valuedate = RawTradesArray[index].Substring(72,8);
                 tempraw[3]=typeofTrade;
                 tempraw[4]=getDate(valuedate);
      
                 var volume = RawTradesArray[index].Substring(112,10);
                 var volumelong = parseInt(volume,10)+parseInt(RawTradesArray[index].Substring(122,2),10)/100;
                 volume = RawTradesArray[index].Substring(125,10);
                 volume = volumelong-parseInt(volume,10)-parseInt(RawTradesArray[index].Substring(135,2),10)/100;
                 if(j>-1) volume = volume*mapping[j][3];
                 tempraw[5] = volume;

                 var value =  RawTradesArray[index].Substring(276,16);
                 var value = parseInt(value,10)+parseInt(RawTradesArray[index].Substring(292,2),10)/100;
         
                 if(RawTradesArray[index].Substring(294,1)=="D")value=-value;

                    if (j > -1)
                    {
                        tempraw[6] = Fn.Round(-value/volume, mapping[j][5]);
                    }
                    else tempraw[6] = Fn.Round(-value/volume, 10);

                    var exchfee =  RawTradesArray[index].Substring(137,10);
                 var exchfee = parseInt(exchfee,10)+parseInt(RawTradesArray[index].Substring(147,2),10)/100;
                 if(RawTradesArray[index].Substring(149,1)=="D")exchfee=-exchfee; 
                 tempraw[7]=exchfee;
        
                  var clfee =  RawTradesArray[index].substr(153,10);
                 var clfee = parseInt(clfee,10)+parseInt(RawTradesArray[index].substr(163,2),10)/100;
                 if(RawTradesArray[index].substr(165,1)=="D")clfee=-clfee; 
                 tempraw[8]=clfee; 
       
                 tempraw[9]=tradecode;        
      
                 tempraw[11]="";

                    if (typeofTrade == "ST")
                    {
                        tempraw[12] = tempraw[6];
                    }
                    else tempraw[12] = value;
                    tempraw[13]= "";
        
                   result.add(tempraw);
              }
             }      
          }      
     return result;
  }
  */

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

        /*
                private string UpdateSymbol(trades,cmap){
           var mappingST = Fn.FilterMatrixEqual(cmap, 4, "STOCK&FX");    
           var mappingFW = Fn.FilterMatrixEqual(cmap, 4,  "FW");
           var mappingFU = Fn.FilterMatrixEqual(cmap, 4,  "FU"); 
           for (var ii =0;ii<mappingFU.length;ii++){ mappingFU[ii][0]=mappingFU[ii][0].concat(Fn.StringFromDate(mappingFU[ii][7])) }
        
           var mapping;
 
          for (var i=0;i<trades.length;i++){
            if(trades[i][10]==""){
              if (trades[i][3]=="FW"){
                mapping = mappingFW;
                var j = Fn.getElementId(mapping, 0, trades[i][2]); 
              }else {
                if(trades[i][3]=="FU"){
                  mapping = mappingFU;
                  var symbol = trades[i][2];
                  var j = Fn.getElementId(mapping, 0, symbol.concat(Fn.StringFromDate(trades[i][4],'-')));
                 }
                else {
                  mapping = mappingST;
                  var j = Fn.getElementId(mapping, 0, trades[i][2]); 
                }
              }
        
               if(j>-1)trades[i][10]=mapping[j][1]
               else {
            //     addnewsymboltoMapping(trades[i][2],trades[i][3]);
                 mappingST = Fn.FilterMatrixEqual(cmap, 4,"STOCK&FX");    
                 mappingFW = Fn.FilterMatrixEqual(cmap, 4, "FW");
               }
            }
          }
          return trades;
        }
        */

        private static Dictionary<string, Map> getMap(string brocker)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var mapfromDb = from m in db.Mappings
                            join c in db.Contracts on m.BOSymbol equals c.id
                            where m.Brocker == brocker && !m.Type.Contains("FORTS")
                            select new
                                {
                                    m.BrockerSymbol,
                                    m.BOSymbol,
                                    m.MtyPrice,
                                    m.MtyVolume,
                                    m.Type,
                                    m.Round,
                                    c.ValueDate,
                                    c.Leverage
                                };
            var results = new Dictionary<string, Map>();
            var mapfromDblist = mapfromDb.ToList();
            foreach (var item in mapfromDblist)
            {
                string key = item.BrockerSymbol;

                if (brocker != "BO")
                {
                    key = item.BrockerSymbol + item.Type;
                }

                if (item.Type == "FU") key = key + item.ValueDate.Value.ToShortDateString();
                results.Add(key, new Map
                    {
                        BOSymbol = item.BOSymbol,
                        MtyPrice = item.MtyPrice,
                        MtyVolume = item.MtyVolume,
                        Round = item.Round,
                        Type = item.Type,
                        ValueDate = item.ValueDate,
                    });
            }
            return results;
        }

        private void ABNReconButtonClick(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            Dictionary<string, Map> symbolmap = getMap("ABN");
            TradesParserStatus.Text = "Processing";
            if (noparsingCheckbox.Checked)
            {
                RecProcess(reportdate, "ABN",true);
            }
            else
            {
                var allfromfile = new List<CpTrade>();
                var futtrades = new List<CpTrade>();
                DialogResult result = openFileDialog2.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (CliffCheckBox.Checked)
                    {
                        Dictionary<string, List<string>> cliffdict = LoadCliff(openFileDialog2.FileName, reportdate);
                        List<string> rowlist;


                        DateTime TimeUpdateBalanceStart = DateTime.Now;
                        if (cliffdict.TryGetValue("610", out rowlist)) updateBalance(rowlist, reportdate);
                        DateTime TimeFutureParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                              "Update Balance Completed. Time:" +
                                              (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");

                        if (cliffdict.TryGetValue("310", out rowlist))
                            allfromfile = ExtractTradesFromCliff(rowlist, symbolmap);
                        DateTime TimeStockParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeStockParsing.ToLongTimeString() + ": " +
                                              "Future parsing Completed. Time:" +
                                              (TimeStockParsing - TimeFutureParsing).ToString() + "s");

                        if (cliffdict.TryGetValue("410", out rowlist))
                            allfromfile.AddRange(ExtractTradesFromCliff(rowlist, symbolmap));
                        DateTime TimeOptionParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeOptionParsing.ToLongTimeString() + ": " +
                                              "Stock parsing Completed. Time:" +
                                              (TimeOptionParsing - TimeStockParsing).ToString() + "s");

                        if (cliffdict.TryGetValue("210", out rowlist))
                            allfromfile.AddRange(ExtractOptionTradesFromCliff(rowlist, symbolmap));
                        DateTime TimeEndOptionParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeEndOptionParsing.ToLongTimeString() + ": " +
                                              "Option parsing Completed. Time:" +
                                              (TimeEndOptionParsing - TimeOptionParsing).ToString() + "s");

                        GetABNPos(cliffdict, reportdate);
                        DateTime TimePositionParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeOptionParsing.ToLongTimeString() + ": " +
                                              "Position parsing Completed. Time:" +
                                              (TimePositionParsing - TimeEndOptionParsing).ToString() + "s");
                        if (cliffdict.TryGetValue("600", out rowlist))
                        {
                            reportdate = getcashmovements(rowlist);
                        }
                        DateTime TimeFTParsing = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                              "FT parsing completed for " + reportdate.ToShortDateString() + ". Time:" +
                                              (TimeFTParsing - TimePositionParsing).ToString() + "s");
                    }
                    else
                    {
                        allfromfile = ExtractTradesFromXml(symbolmap);
                    }
                    foreach (CpTrade tradeIndex in allfromfile)
                    {
                        db.CpTrades.Add(tradeIndex);
                    }
                    db.SaveChanges();
                    allfromfile = allfromfile.Where(s => s.TypeOfTrade == "01").ToList();
                    DateTime TimeStartReconciliation = DateTime.Now;
                    AbnRecon(reportdate, allfromfile, "ABN",true);
                    DateTime TimeEndReconciliation = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeEndReconciliation.ToLongTimeString() + ": " +
                                          "Reconciliation completed. Time:" +
                                          (TimeEndReconciliation - TimeStartReconciliation).ToString() + "s");
                }
            }
            TradesParserStatus.Text = "Done";
        }

        private void RecProcess(DateTime reportdate, string ccp,bool maltaentity)
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart + ": " + "start " + ccp + " reconciliation");
            var db = new EXANTE_Entities(_currentConnection);
            Dictionary<string, Map> symbolmap = getMap(ccp);
            DateTime nextdate = reportdate.AddDays(1);
            IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                where
                                                    cptrade.valid == 1 && cptrade.BrokerId == ccp &&
                                                    cptrade.ReportDate >= reportdate.Date &&
                                                    cptrade.ReportDate < (nextdate.Date) &&
                                                    cptrade.BOTradeNumber == null
                                                select cptrade;
            if (ccp == "ABN")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "01"); //.Contains(o.StatusCode))
            if (ccp == "Mac")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "A");
            if (ccp == "CFH")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "OnlineTrade");
            List<CpTrade> cptradelist = cptradefromDb.ToList();
            DateTime TimeStartInternal= DateTime.Now;
            int batchsize = 700;
            int i =1;
            foreach (CpTrade cpTrade in cptradelist)
            {
                if (cpTrade.BOSymbol == null)
                {
                    Map symbolvalue;
                    string key = cpTrade.Symbol + cpTrade.Type;
                    if (cpTrade.Type == "FU")
                    {
                        if (cpTrade.ValueDate != null) key = key + cpTrade.ValueDate.Value.ToShortDateString();
                    }
                    if (symbolmap.TryGetValue(key, out symbolvalue))
                    {
                        cpTrade.BOSymbol = symbolvalue.BOSymbol;
                        cpTrade.Qty = cpTrade.Qty*symbolvalue.MtyVolume;
                        cpTrade.Price = cpTrade.Price*symbolvalue.MtyPrice;
                    }
                    db.CpTrades.Attach(cpTrade);
                    db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    i++;
                }
                if (i % batchsize == 0)
                {

                    SaveDBChanges(ref db);
                    DateTime TimeEndInternal = DateTime.Now;
                    LogTextBox.AppendText("\r\n" +ccp+ " trades rec time for "+batchsize.ToString()+" :" + (TimeEndInternal - TimeStartInternal).ToString());
                    TimeStartInternal = DateTime.Now;
                }
            }

            SaveDBChanges(ref db);
            db.Dispose();

            DateTime TimeStartReconciliation = DateTime.Now;
            AbnRecon(reportdate, cptradelist, ccp, maltaentity);
            DateTime TimeEndReconciliation = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndReconciliation.ToLongTimeString() + ": " +
                                  "Reconciliation completed. Time:" +
                                  (TimeStartReconciliation - TimeEndReconciliation).ToString() + "s");
        }

        private List<CpTrade> ExtractTradesFromXml(Dictionary<string, Map> symbolmap)
        {
            //todo: unzip file
            var doc = new XmlDocument();
            //doc.Load(@"C:\20140214.xml");
            doc.Load(openFileDialog2.FileName);
            var db = new EXANTE_Entities(_currentConnection);
            var allfromfile = new List<CpTrade>();
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);

            //var results = products.ToDictionary(product => product.Id);
            //   var authors = Linkdoc.Root.Elements().Select(x => x.Element("UnsettledMovement"));
            int row = -1;
            {
                //XmlNodeList nodes = doc.SelectNodes("/Transactions/AccountTransactions");
                foreach (XmlNode mainnode in doc.DocumentElement.ChildNodes)
                {
                    //  var test = Mainnode.SelectNodes("UnsettledMovement/MovementCode[@Value = '01']");
                    foreach (XmlNode itemNode in mainnode.SelectNodes("UnsettledMovement"))
                    {
                        XmlNodeList list = itemNode.ChildNodes;
                        string MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //    if (new [] {"01", "23", "24"}.Contains(MovementCode)){
                        row++;
                        int Pricemty = 1;
                        /* var selectSingleNode = itemNode.SelectSingleNode("ExchangeFee/Value");
                                 var singleNode = itemNode.SelectSingleNode("ClearingFee/Value"); 
                                 if (itemNode.SelectSingleNode("TransactionPriceCurrency/CurrencyPricingUnit") != null)
                                 {
                                     Pricemty = Convert.ToInt32(itemNode.SelectSingleNode("TransactionPriceCurrency/CurrencyPricingUnit").InnerText);
                                 }*/
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
                                Pricemty = 10000/Convert.ToInt32(itemNode.SelectSingleNode(
                                    "TransactionPriceCurrency/CurrencyPricingUnit").InnerText);
                            }
                        }

                        string symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + typeOftrade;
                        Map symbolvalue;
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
                                          : -1*Convert.ToInt64(itemNode.SelectSingleNode("QuantityShort").InnerText),
                                Price = (itemNode.SelectSingleNode("TransactionPrice") != null)
                                            ? (double)
                                              decimal.Round(
                                                  Convert.ToDecimal(
                                                      itemNode.SelectSingleNode("TransactionPrice").InnerText)/
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
                                                  ? -1*
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
                        // var cp_id = itemNode.SelectSingleNode("OppositePartyCode").InnerText;
                        //                       var value = itemNode.SelectSingleNode("").InnerText;
                        //if 01   }
                    }

                    foreach (XmlNode itemNode in mainnode.SelectNodes("FutureMovement"))
                    {
                        XmlNodeList list = itemNode.ChildNodes;
                        string MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //  if (new[] { "01", "23", "24" }.Contains(MovementCode)){
                        int Pricemty = 1;
                        double price = Convert.ToDouble(itemNode.SelectSingleNode("TransactionPrice").InnerText)/
                                       Pricemty;
                        long qty = (itemNode.SelectSingleNode("QuantityShort") == null)
                                       ? Convert.ToInt64(itemNode.SelectSingleNode("QuantityLong").InnerText)
                                       : -1*Convert.ToInt64(itemNode.SelectSingleNode("QuantityShort").InnerText);
                        string symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + "FU" +
                                          Convert.ToDateTime(GetValueDate(itemNode)).ToShortDateString();
                        Map symbolvalue;
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
                                    -Convert.ToInt64(itemNode.SelectSingleNode("Tradingunit").InnerText == "D")*
                                    price*qty,
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

        private Dictionary<string, List<string>> LoadCliff(string fileName, DateTime reportdate)
        {
            var reader = new StreamReader(fileName);
            //     var reader = new StreamReader(@"C:\20140428----1978-------C");
            string lineFromFile = reader.ReadLine();
            if (lineFromFile != null)
            {
                reportdate = (DateTime) getDatefromString(lineFromFile.Substring(6, 8));
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
                    else cliffdict.Add(code, new List<string> {lineFromFile});
                }
                lineFromFile = reader.ReadLine();
            }
            return cliffdict;
        }

        private static DateTime? getDatefromString(string lineFromFile, bool time = false)
        {
            if ((lineFromFile[0] != ' ') && (lineFromFile[0] != '0'))
            {
                return time
                           ? DateTime.Parse(lineFromFile.Substring(0, 4) + "-" + lineFromFile.Substring(4, 2) + "-" +
                                            lineFromFile.Substring(6, 2) + " " + lineFromFile.Substring(8, 2) + ":" +
                                            lineFromFile.Substring(10, 2) + ":" + lineFromFile.Substring(12, 2))
                           : DateTime.Parse(lineFromFile.Substring(0, 4) + "-" + lineFromFile.Substring(4, 2) + "-" +
                                            lineFromFile.Substring(6, 2));
            }
            else return null;
        }

        private List<CpTrade> ExtractOptionTradesFromCliff(List<string> rowlist, Dictionary<string, Map> symbolmap)
        {
            var allfromfile = new List<CpTrade>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
            foreach (string row in rowlist)
            {
                string code = row.Substring(124, 2);
                string typeoftrade = row.Substring(60, 2);
                DateTime? tradedate = getDatefromString(row.Substring(554), true) ??
                                      getDatefromString(row.Substring(562), true);
                string symbol = row.Substring(66, 6).Trim();
                string Counterparty = row.Substring(54, 6).Trim();
                DateTime? valuedate = getDatefromString(row.Substring(73, 8).Trim());
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

                Map symbolvalue;
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
                        Qty = volume*MtyVolume,
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

        private static string getLetterOfMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "F";
                case 2:
                    return "G";
                case 3:
                    return "H";
                case 4:
                    return "J";
                case 5:
                    return "K";
                case 6:
                    return "M";
                case 7:
                    return "N";
                case 8:
                    return "Q";
                case 9:
                    return "U";
                case 10:
                    return "V";
                case 11:
                    return "X";
                case 12:
                    return "Z";
                default:
                    return "";
            }
        }


        private List<CpTrade> ExtractTradesFromCliff(List<string> rowlist, Dictionary<string, Map> symbolmap)
        {
            var allfromfile = new List<CpTrade>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
            foreach (string row in rowlist)
            {
                string typeoftrade = row.Substring(60, 2);
                DateTime? tradedate = getDatefromString(row.Substring(582), true) ??
                                      getDatefromString(row.Substring(295), true);
                string symbol = row.Substring(66, 6).Trim();
                string type = row.Substring(60, 2);
                Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;
                int round = 10;
                string symbol_id = symbol + type;
                DateTime? valuedate = getDatefromString(row.Substring(303)) ?? getDatefromString(row.Substring(72));

                if (typeoftrade == "FU")
                {
                    symbol_id = symbol_id + Convert.ToDateTime(valuedate).ToShortDateString();
                }

                if (symbolmap.TryGetValue(symbol_id, out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    BoSymbol = symbolvalue.BOSymbol;
                    round = (int) symbolvalue.Round;
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
                                         CultureInfo.InvariantCulture)*(double) MtyPrice, round);
                }
                else
                {
                    transacPrice =
                        Math.Round(
                            double.Parse(row.Substring(230, 8) + "." + row.Substring(238, 7),
                                         CultureInfo.InvariantCulture)*(double) MtyPrice, round);
                    value = -Math.Round(GetValueFromCliff(row.Substring(112))*(double) MtyVolume*transacPrice, 10);
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
                                        ? getDatefromString(row.Substring(496), true)
                                        : getDatefromString(row.Substring(582), true) ??
                                          getDatefromString(row.Substring(295), true),
                        BrokerId = "ABN",
                        Symbol = symbol,
                        Type = (row.Substring(405, 4) == "FW-E")
                                   ? "FW-E"
                                   : type,
                        Qty = GetValueFromCliff(row.Substring(112))*MtyVolume,
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
            Dictionary<string, Map> symbolmap = getMap("ABN");
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
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
                    valuedate = getDatefromString(row.Substring(72, 8));
                    symbol_id = symbol_id + Convert.ToDateTime(valuedate).ToShortDateString();
                    tradedate = (DateTime) getDatefromString(row.Substring(183, 8));
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
                        valuedate = getDatefromString(row.Substring(73, 8));
                        tradedate = (DateTime) getDatefromString(row.Substring(184, 8));
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
                        valuedate = getDatefromString(row.Substring(72, 8));
                        tradedate = (DateTime) getDatefromString(row.Substring(209, 8));
                        transacPrice =
                            Math.Round(
                                double.Parse(row.Substring(182, 8) + "." + row.Substring(190, 7),
                                             CultureInfo.InvariantCulture), round);
                        ccy = row.Substring(117, 3);
                        qty = GetValueFromCliff(row.Substring(120));
                    }
                }
                Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;

                if (symbolmap.TryGetValue(symbol_id, out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    BoSymbol = symbolvalue.BOSymbol;
                    round = (int) symbolvalue.Round;
                }
                else
                {
                    LogTextBox.AppendText("\r\n" + "There is no BO Symbol for this id:" + symbol_id);
                }
                transacPrice = Math.Round(transacPrice*(double) MtyPrice, round);
                qty = (double) (qty*MtyVolume);
                double value = -Math.Round(qty*transacPrice, round);

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
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e2)
            {
                foreach (DbEntityValidationResult eve in e2.EntityValidationErrors)
                {
                    Console.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                          ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            return reportdate;
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
            var dbentity = new EXANTE_Entities(_currentConnection);
            IQueryable<string> cpidfromDb = from cp in dbentity.DailyChecks
                                            where cp.Table == "Daily" && cp.date == reportdate
                                            select cp.status;
            var listforDb = new List<ABN_cashposition>();
            foreach (string row in rowlist)
            {
                string value = row.Substring(90, 18);
                value = value.Substring(0, value.Count() - 2) + "." + value.Substring(value.Count() - 2, 2);
                dbentity.ABN_cashposition.Add(new ABN_cashposition
                    {
                        ReportDate = reportdate,
                        Currency = row.Substring(54, 3),
                        Value = row[108] != 'C'
                                    ? -1*double.Parse(value, CultureInfo.InvariantCulture)
                                    : double.Parse(value, CultureInfo.InvariantCulture),
                        valid = 1,
                        User = "parser",
                        TimeStamp = DateTime.Now,
                        Description = row.Substring(109, 40).Trim()
                    });
            }
            dbentity.SaveChanges();
            /*     dbentity.DailyChecks.Add(new DailyCheck
                  {
                    cp_id = null,
                    date = reportdate,
                    status = "ok",
                    user = "parser",
                    valid = true,
                    timestamp =DateTime.Now,
                    Table = "ABN_cashposition"
                   });
           dbentity.SaveChanges();*/
        }

        public static void Log(string message)
        {
            DateTime timestamp = DateTime.Now;
            File.AppendAllText("log_" + timestamp.ToShortDateString() + ".txt",
                               timestamp.ToShortDateString() + " " + message);
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
                    dbentity.counterparties.Add(new counterparty {Name = cpname});
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
                Log("Нет идентификатора counterparty");
                return 0;
            }
        }

        private List<Reconcilation> Reconciliation(List<CpTrade> cpTrades, Dictionary<string, List<BOtrade>> botrades,
                                                   string cpColumn, string BOCp)
        {
            PropertyInfo prop_cpTrades = typeof (CpTrade).GetProperty(cpColumn);
            //var prop_boTrades = typeof (Ctrade).GetProperty(boColumn);
            var recon = new List<Reconcilation>();
            for (int i = 0; i < cpTrades.Count; i++)
            {
                var value = (string) prop_cpTrades.GetValue(cpTrades[i], null);
                List<BOtrade> boitemlist;
                if (botrades.TryGetValue(value, out boitemlist))
                {
                    int iBoitemlist = 0;
                    bool found = false;
                    while ((iBoitemlist < boitemlist.Count) && (!found))
                    {
                        if ((boitemlist[iBoitemlist].Price.Equals(cpTrades[i].Price)) &&
                            (boitemlist[iBoitemlist].Qty.Equals(cpTrades[i].Qty)) &&
                            (!boitemlist[iBoitemlist].RecStatus))
                        {
                            found = true;
                        }
                        else iBoitemlist++;
                    }
                    if (found)
                    {
                        cpTrades[i].BOTradeNumber = boitemlist[iBoitemlist].TradeNumber.ToString();
                        cpTrades[i].BOSymbol = boitemlist[iBoitemlist].symbol;
                        cpTrades[i].BOcp = BOCp;
                        cpTrades[i].Id = boitemlist[iBoitemlist].ctradeid.ToString();
                        recon.Add(new Reconcilation
                            {
                                CpFull_id = i,
                                BOTradenumber = boitemlist[iBoitemlist].TradeNumber,
                                Timestamp = DateTime.UtcNow,
                                username = "TradeParser",
                                valid = 1
                            });
                        boitemlist[iBoitemlist].RecStatus = true;
                    }
                }
            }
            return recon;
            //    boTrades.Find(x => (string) prop_boTrades.GetPage(x, null) == value);
        }

        //        public int Method(Bar bar, string propertyName)
        // var prop = typeof(Bar).GetProperty(propertyName);
        //   int value = (int)prop.GetPage(bar,null);

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start ADSS trades uploading");

                reportdate = Adssparsing();

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "ADSS trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            RecProcess(reportdate, "ADSS",true);
            var db = new EXANTE_Entities(_currentConnection);
            //  db.Database.ExecuteSqlCommand("UPDATE CpTrades Set value = -Qty*Price WHERE BrokerId LIKE '%adss%'");
            db.Dispose();
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
            /*    var nextdate = reportDate;
                var startdate = new DateTime(minDate.Year, minDate.Month, minDate.Day, 0, 0, 0);
                var queryable =
                    from ct in db.Ctrades
                    where ct.Date >= startdate && ct.cp_id == "ADSS"
                    select new { ct.ExchangeOrderId, ct.tradeNumber, ct.qty, ct.price, ct.symbol_id, ct.fullid, ct.RecStatus };
                var botrades = new Dictionary<string, List<BOtrade>>();
                var n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    var Ctrade_id = ctrade.symbol_id.Replace(".EXANTE", "") + ctrade.qty.ToString() +
                                    ctrade.price.ToString();
                    Ctrade_id = Ctrade_id.Replace("/", "");

                    var tempBotrade = new BOtrade
                    {
                        TradeNumber = (long)ctrade.tradeNumber,
                        Qty = (double)ctrade.qty,
                        Price = (double)ctrade.price,
                        symbol = ctrade.symbol_id,
                        ctradeid = ctrade.fullid,
                        RecStatus = ctrade.RecStatus
                    };

                    if (botrades.ContainsKey(Ctrade_id))
                    {
                        botrades[Ctrade_id].Add(tempBotrade);
                    }
                    else botrades.Add(Ctrade_id, new List<BOtrade> { tempBotrade }); //tempBotrade});
                }
                var recon = Reconciliation(allfromfile, botrades, "exchangeOrderId", "2");

                foreach (var botrade in botrades)
                {
                    foreach (var botradeItemlist in botrade.Value)
                    {
                        if (botradeItemlist.RecStatus)
                        {
                            using (var data = new EXANTE_Entities(_currentConnection))
                            {
                                data.Database.ExecuteSqlCommand(
                                    "UPDATE Ctrades Set RecStatus ={0}  WHERE fullid = {1}", true,
                                    botradeItemlist.ctradeid);
                            }
                        }
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                }
                db.SaveChanges();

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpTrade_id = allfromfile[(int)reconitem.CpTrade_id].FullId;
                    db.Reconcilations.Add(reconitem);
                }
                db.SaveChanges();*/
        }

        private void BloombergParsing()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            DateTime reportDate = InputDate.Value.Date;
            if (result == DialogResult.OK) // Test result.
            {
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                               0, false, 5, "", "", false,
                                                               XlPlatform
                                                                   .xlWindows, "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
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
                                               select cp).ToDictionary(k => (k.symbolId + k.isin + k.DeclaredDate.Value.ToShortDateString() + k.ExDate.Value.ToShortDateString() + k.RecordDate.Value.ToShortDateString() + k.PayableDate.Value.ToShortDateString() + k.DividendAmount.ToString() + k.DividendFrqncy + k.DividendType), k => k.id);
            /*
                 Dictionary<string, long> checkId =
                        (from ct in db.Ctrades
                         where ct.BOtradeTimestamp.ToString().Contains("2010-12")
                         select ct).ToDictionary(k => (k.order_id.ToString() + k.orderPos.ToString()), k => k.fullid);*/

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
                        LogTextBox.AppendText("\r\n" + "There isin in contracts: " + xlRange.Cells[i, idIsin].value2);

                        var testkey = "NULL" + isin + DeclaredDate.ToString("M/d/yyyy") + ExDate.ToString("M/d/yyyy") +
                                          RecordDate.ToString("M/d/yyyy") + PayableDate.ToString("M/d/yyyy") + DividendAmount + DividendFrqncy + DividendType;
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
                            var testkey = contractDetail.Contract1 + isin + DeclaredDate.ToString("M/d/yyyy") + ExDate.ToString("M/d/yyyy") +
                                          RecordDate.ToString("M/d/yyyy") + PayableDate.ToString("M/d/yyyy") + DividendAmount + DividendFrqncy + DividendType;
                            long id;
                            if (!CA.TryGetValue(testkey, out id))
                            {
                                DateTime? lastdate = new DateTime();
                                dynamic qty = getQtyFromCtrade(db, contractDetail.Contract1, ExDate, ref lastdate, isin,reportDate.Date);
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
                    SaveDBChanges(ref db);
                    i++;
                    isinExcel = xlRange.Cells[i, idIsin].value2;
                }
                //  SaveDBChanges(ref db);
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
            }
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


        private DateTime Adssparsing()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                               0, false, 5, "", "", false,
                                                               XlPlatform
                                                                   .xlWindows, "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
                Worksheet ObjWorkSheet;
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Activity Log"];
                Range xlRange = ObjWorkSheet.UsedRange;
                IFormatProvider theCultureInfo = new CultureInfo("en-GB", true);
                //     int rowCount = xlRange.Rows.Count + 1;
                var db = new EXANTE_Entities(_currentConnection);
                //   var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();
                //Ticket Ref	Party	Type	Symbol	B/S	Amount	Currency	Rate	Counter Amount	Currency	Tenor	Value Date	Ticket Creation	Order Ref	GRID
                //EOD SWAP 201311190000/1127 FAR LEG	60002000000		NZDUSD	Sell	15 857.00	NZD	0.83218	13 195.88	USD	SPOT	21/11/2013	19/11/2013 06:18:55		
                //    var lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";

                DateTime reportDate;
                //  reportDate = DateTime.ParseExact(xlRange.Cells[9, 2].Value2, "dd/MM/yyyy HH:mm:ss", theCultureInfo);
                //  reportDate = DateTime.ParseExact("11/03/2015 00:00:00", "dd/MM/yyyy HH:mm:ss", theCultureInfo);
                reportDate = InputDate.Value;
                dynamic account = xlRange.Cells[6, 2].Value2;
                DateTime prevDate = reportDate.AddDays(-7);
                //openFileDialog2.FileName.Substring(openFileDialog2.FileName.IndexOf("_") + 1,openFileDialog2.FileName.LastIndexOf("-") -openFileDialog2.FileName.IndexOf("_") - 1);
                int idTradeDate = 2,
                    idSymbol = 3,
                    idQty = 5,
                    idSide = 4,
                    idPrice = 7,
                    idValueDate = 11,
                    idValue = 9,
                    idFee = 12,
                    idfeeccy = 13,
                    exchangeid = 1,
                    idccy = 10,
                    batchsize = 100;
                int i = 19;
                Dictionary<string, CpTrade> checkId = (from ct in db.CpTrades
                                                       where
                                                           ct.BrokerId.Contains("ADSS") &&
                                                           ct.ReportDate <= (reportDate.Date) &&
                                                           ct.ReportDate >= prevDate.Date
                                                       select ct).ToDictionary(
                                                           k => (k.Qty.ToString() + k.exchangeOrderId), k => k);
                Dictionary<string, long> checkIdFT = (from ct in db.FT
                                                      where
                                                          ct.brocker.Contains("ADSS") && ct.Type.Contains("PL") &&
                                                          ct.ReportDate >= prevDate.Date
                                                      select ct).ToDictionary(k => (k.Comment), k => k.fullid);
                // && ctrade.Date >= reportdate.Date && cptrade.ReportDate < (nextdate.Date)
                DateTime TimeStart = DateTime.Now;
               // i = 1380;
                while (xlRange.Cells[i, 1].value2 != null)
                {
                    
                    string exchorderid = xlRange.Cells[i, exchangeid].value2.ToString();
                    dynamic qty = xlRange.Cells[i, idSide].value2.IndexOf("Buy") == -1
                                      ? Convert.ToDouble(xlRange.Cells[i, idQty].value2)*(-1)
                                      : Convert.ToDouble(xlRange.Cells[i, idQty].value2);
                    if (!checkId.ContainsKey(qty.ToString() + exchorderid))
                    {
                        dynamic tradedate = DateTime.ParseExact(xlRange.Cells[i, idTradeDate].value2.ToString(),
                                                                "dd/MM/yyyy HH:mm:ss", theCultureInfo);

                        dynamic ValueDate = DateTime.ParseExact(xlRange.Cells[i, idValueDate].value2.ToString(),
                                                                "dd/MM/yyyy", theCultureInfo);
                        string typeoftrade = "Trade";
                        if (exchorderid.Contains("EOD SWAP")) typeoftrade = "EODSWAP";
                        db.CpTrades.Add(new CpTrade
                            {
                                ReportDate = reportDate.Date,
                                TypeOfTrade = typeoftrade,
                                TradeDate = tradedate,
                                BrokerId = "ADSS",
                                Symbol = xlRange.Cells[i, idSymbol].value2.ToString(),
                                Type = "FX",
                                Qty = qty,
                                Price = Convert.ToDouble(xlRange.Cells[i, idPrice].value2),
                                ValueDate = ValueDate,
                                Comment = account,
                                cp_id = 19,
                                ExchangeFees = Convert.ToDouble(xlRange.Cells[i, idFee].value2),
                                ExchFeeCcy = xlRange.Cells[i, idfeeccy].value2.ToString(),
                                Fee = null,
                                Id = null,
                                BOSymbol = null,
                                BOTradeNumber = null,
                                value = -Convert.ToDouble(xlRange.Cells[i, idValue].value2),
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "tradesparser",
                                //  FullId = null,
                                BOcp = null,
                                ccy = xlRange.Cells[i, idccy].value2.ToString(),
                                exchangeOrderId = xlRange.Cells[i, exchangeid].value2.ToString()
                            });
                        if (i%batchsize == 0)
                        {
                            
                            SaveDBChanges(ref db);
                            DateTime TimeEnd = DateTime.Now;
                            LogTextBox.AppendText("\r\n" + "ADSS trades uploading time: " +(TimeEnd - TimeStart).ToString());
                            TimeStart = DateTime.Now;
                        }
                        
                    }
                    i++;
                }
                SaveDBChanges(ref db);
                i = i + 5;

                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                return reportDate;
            }
            else return new DateTime(2011, 01, 01);
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }


        private string getHTML(string urlAddress)
        {
            urlAddress = "http://google.com";
            var request = (HttpWebRequest) WebRequest.Create(urlAddress);
            var response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                if (response.CharacterSet == null) readStream = new StreamReader(receiveStream);
                else readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                string data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
                return data;
            }
            else return "";
        }

        private void UpdatungViewCpTrades(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value;
            DateTime prevreportdate = reportdate.AddDays(-3);
            var ts = new TimeSpan(20, 00, 0);
            prevreportdate = prevreportdate.Date + ts;
            var db = new EXANTE_Entities(_currentConnection);
            var cplist = new List<string> {"LEK", "CQG", "FASTMATCH", "CURRENEX", "EXANTE", ""};
            TradesParserStatus.Text = "Processing";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText(TimeStart + ": " + "Preparing ABN View");
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
                if (!cpmappings.TryGetValue((int) cpTrade.cp_id, out cpname))
                {
                    LogTextBox.AppendText("\r\n" + "There is no counterparty for this id");
                }
                Contract contractDetails = null;
                double leverage = 1;
                if ((cpTrade.BOSymbol == null) ||
                    (!contractdetailstable.TryGetValue(cpTrade.BOSymbol, out contractDetails)))
                {
                    LogTextBox.AppendText("\r\n" + "There is no id in contracts for " + cpTrade.Symbol + " " +
                                          cpTrade.Type + " " + cpTrade.TypeOfTrade + " " + cpTrade.FullId);
                }
                else leverage = (double) contractDetails.Leverage;
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
                            LogTextBox.AppendText("\r\n" + "Didn't find Ctrade with tradenumber = " +
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
                                LogTextBox.AppendText("\r\n" + "Accounts are different for cptrade.fullid=" +
                                                      cpTrade.FullId);
                            }
                        }
                        bosum = (double) (bosum + ctradevalue.fees);
                        ccy = ctradevalue.currency;
                    }
                }
                updatelist.Add(new ABNReconResult
                    {
                        ReportDate = (DateTime) cpTrade.ReportDate,
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
                        Value = -leverage*cpTrade.Price*cpTrade.Qty,
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
                SaveDBChanges(ref db);
                foreach (ABNReconResult reconResult in updatelist)
                {
                    db.ABNReconResults.Add(reconResult);
                }

                SaveDBChanges(ref db);
            }
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating.ToLongTimeString() + ": " +
                                  "Updating completed. Time:" + (TimeEndUpdating - TimeStart).ToString());
        }

        private Dictionary<string, Contract> getContractDetails()
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<Contract> cpfromDb = from cp in db.Contracts
                                            select cp;
            return cpfromDb.ToDictionary(k => k.id, k => k);
        }

        private DateTime getcashmovements(List<string> rowlist)
        {
            var dbentity = new EXANTE_Entities(_currentConnection);
            var listforDb = new List<FT>();
            DateTime reportdate = DateTime.ParseExact(rowlist[0].Substring(6, 8), "yyyyMMdd",
                                                      CultureInfo.InvariantCulture);
            Dictionary<string, Map> bomap = getMap("ABN");
            Map symbolvalue;
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
                dbentity.FT.Add(new FT
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
                                    ? -1*Convert.ToDouble(row.Substring(87, 18))/100
                                    : Convert.ToDouble(row.Substring(87, 18))/100,
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
            dbentity.SaveChanges();
            return reportdate;
        }

        private void CheckConnection()
        {
            LogTextBox.AppendText("\r\n" + "Checking connection");

            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<Contract> cptradefromDb = from Contr in db.Contracts
                                                 where Contr.valid == 1
                                                 select Contr;
            List<Contract> test = cptradefromDb.ToList();
            LogTextBox.AppendText("\r\n" + "Good connection with " + _currentConnection);
        }

        private void comboBoxEnviroment_TextChanged(object sender, EventArgs e)
        {
            _currentConnection = comboBoxEnviroment.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            updateFORTSccyrates();
            calcualteVM(InputDate.Value, "M&L");
            calcualteVM(InputDate.Value, "MOEX");
            calcualteVM(InputDate.Value, "EXANTE");
            calcualteVM(InputDate.Value, "MOEX-SPECTRA");
            calcualteVM(InputDate.Value, "OPEN");
            calcualteVM(InputDate.Value, "INSTANT");


            var db = new EXANTE_Entities(_currentConnection);
            db.Database.ExecuteSqlCommand(
                "UPDATE FT Set Account_id = {0}  WHERE Account_id LIKE {1} AND ReportDate = {2}", "UJL5180.INV",
                "UJL5180.0%", InputDate.Value.Date);
            db.Database.ExecuteSqlCommand(
               "UPDATE FT Set Account_id = {0}  WHERE Account_id LIKE {1} AND ReportDate = {2}", "AEY5299.INV",
               "AEY5299.0%", InputDate.Value.Date);

            db.Dispose();
        }

        private double? GetVM(DateTime vmDate, string brocker)
        {
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = vmDate.AddDays(1);
            double? sum =
                db.FT.Where(
                    o =>
                    (o.ReportDate >= vmDate.Date && o.ReportDate < nextdate.Date && o.valid == 1 && o.brocker == brocker))
                  .Sum(o => o.value);
            db.Dispose();
            return sum;
        }

        private void calcualteVM(DateTime VMDate, string Brocker)
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart + ": " + " Calculting VM for " + Brocker);

            List<FullTrade> listofaccountpositions = Getlistofaccountposition(VMDate, Brocker);
            listofaccountpositions = udpateVMforaccount(listofaccountpositions, VMDate, Brocker);

            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + " End VM Calculation.VM = " +
                                  GetVM(InputDate.Value, Brocker).ToString() + ". Time:" +
                                  (TimeStart - TimeEndUpdating).ToString());
        }

        private List<FullTrade> udpateVMforaccount(List<FullTrade> listofaccountpositions, DateTime VMDate,
                                                   string Brocker)
        {
            int i = 0;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = VMDate.AddDays(1);
            IQueryable<FT> listtodelete = from recon in db.FT
                                          where recon.ReportDate >= VMDate.Date && recon.ReportDate < nextdate.Date
                                                && recon.Type.Contains("VM") && recon.cp.Contains(Brocker)
                                          select recon;
            db.FT.RemoveRange(listtodelete);
            SaveDBChanges(ref db);

            while (i < listofaccountpositions.Count)
            {
                FullTrade fullTrade = listofaccountpositions[i];
                double valueccy = 0;
                if (fullTrade.Value == 0)
                {
                    double currentAtomOfVM = getatomofVM(fullTrade.Symbol, VMDate);
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
                            double t0 = currentAtomOfVM*listofaccountpositions[j].Price;
                            double t1 = Math.Round(currentAtomOfVM*listofaccountpositions[j].Price, 2,
                                                   MidpointRounding.AwayFromZero);
                            double t2 = closeAtomOfVM - t1;
                            double t3 = listofaccountpositions[j].Qty*t2;
                            double t4 = Math.Round(t3, 2);


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
                        ValueCCY = -Math.Round(fullTrade.Value*valueccy, 2, MidpointRounding.AwayFromZero)
                    });
            }
            //  db.SaveChanges();
            SaveDBChanges(ref db);
            db.Dispose();

            return listofaccountpositions;
        }

        private static double GetValueccy(DateTime VMDate, string symbol)
        {
            var db = new EXANTE_Entities(_currentConnection);

            int indexofOption = CustomIndexOf(symbol, '.', 3);
            string key = "";
            if (indexofOption > 0)
            {
                key = symbol.Substring(0, indexofOption) + ".";
            }
            else key = symbol;


            List<int?> map =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "OPEN" && ct.Type == "FORTS" && ct.BOSymbol == key
                 select ct.Round).ToList();

            if ((map.Count > 0) && (map[0] == 1))
            {
                double? ccyrateFromDblinq =
                    (from ct in db.Prices
                     where
                         ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker.Contains("USDRUB") &&
                         ct.Date == VMDate.Date
                     select ct.Price1).ToList()[0];
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
            var db = new EXANTE_Entities(_currentConnection);
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

        private double getatomofVM(string symbol, DateTime VMDate)
        {
            var db = new EXANTE_Entities(_currentConnection);
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
                    IQueryable<Price> ccyrateFromDblinq =
                        (from ct in db.Prices
                         where
                             ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker.Contains("USDRUB") &&
                             ct.Date == VMDate.Date
                         select ct);
                    double ccyrateFromDb = 0;
                    if (!ccyrateFromDblinq.Any())
                    {
                        //  updateFORTSccyrates(VMDate.ToString("dd.MM.yyyy"));
                        ccyrateFromDb =
                            (double) (from ct in db.Prices
                                      where
                                          ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker.Contains("USDRUB") &&
                                          ct.Date == VMDate.Date
                                      select ct).ToList()[0].Price1;
                    }
                    else
                    {
                        ccyrateFromDb = (double) ccyrateFromDblinq.ToList()[0].Price1;
                    }
                    atomvalue = Math.Round((atomvalue*ccyrateFromDb), 5, MidpointRounding.AwayFromZero);
                }
            }
            db.Dispose();
            return atomvalue;
        }

        private List<FullTrade> Getlistofaccountposition(DateTime fortsDate, string Brocker)
        {
            var db = new EXANTE_Entities(_currentConnection);

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
            var db = new EXANTE_Entities(_currentConnection);

            IQueryable<double?> lastprice =
                from ct in db.Prices
                where ct.Valid == 1 && ct.Date < fortsDate.Date && ct.Ticker == symbol
                orderby ct.Date descending
                select ct.Price1;
            if (!lastprice.Any())
            {
                LogTextBox.AppendText("\r\n" + "There is no prices for " + ": " + symbol + ". VM can be incorrect!");
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

        private double UpdateFortsPrices(DateTime fortsDate, string currentInstrument)
        {
            const string initialstring = "http://moex.com/ru/derivatives/contractresults.aspx?code=";
            //  var listCurrentInstruments = getFORTSinstrument(fortsDate);
            var db = new EXANTE_Entities(_currentConnection);
            Dictionary<string, Map> map = getSymbolMap("OPEN", "FORTS");
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
            Map symbolvalue;
            //  var indexofOption = currentInstrument.IndexOf("FORTS")+11;
            int indexofOption = CustomIndexOf(currentInstrument, '.', 3);
            string key = "";
            if (indexofOption > 0)
            {
                key = currentInstrument.Substring(0, indexofOption + 1);
            }
            else key = currentInstrument;
            if (!map.TryGetValue(key, out symbolvalue))
            {
                LogTextBox.AppendText("\r\n" + "New Symbol: " + key);
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
                    // mappingsymbol = mappingsymbol + currentInstrument[indexofOption + 1] + "A" + currentInstrument.Substring(indexofOption + 2);
                    //+ "M" + vd.ToString("ddMMyy") + currentInstrument[indexofOption + 1] +"A " + currentInstrument.Substring(indexofOption + 2);
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
                SaveDBChanges(ref db);
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
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = fortsDate.AddDays(1);
            IQueryable<string> contractrow =
                from ct in db.Ctrades
                where
                    ct.valid == 1 && ct.Date >= fortsDate.Date && ct.Date < (nextdate.Date) &&
                    ct.symbol_id.Contains(".FORTS.")
                select ct.symbol_id;
            return contractrow.Distinct().ToList();
        }

        private void updateFORTSccyrates()
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart + ": " + "Getting ccy prices from MOEX");
            string Date = InputDate.Value.ToString("yyyy-MM-dd");

            // const string initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx?currency=";
            const string initialstring = "http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=";
            // http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=USD/RUB&moment_start=2014-07-24&moment_end=2014-07-24
            var listccy = new List<string>();
            listccy.Add("USD/RUB");
            listccy.Add("EUR/RUB");
            var db = new EXANTE_Entities(_currentConnection);
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
            SaveDBChanges(ref db);
            db.Dispose();

            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + "CCY FORTS rates for " + Date +
                                  " uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());
        }

        private void updatePrices()
        {
            //        using System.Net;
            string initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx";
            GetHtmlPage(initialstring);
            //    var forwardstring = "http://moex.com/ru/derivatives/contractresults.aspx?code=";
            // s+mapping[j][1],'/tr','</td',list);
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

        private static void GetHtmlPage(string url)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
            HtmlNodeCollection tags = doc.DocumentNode.SelectNodes("//abc//tag");


            //  var html = new HtmlDocument();
            //  html.
            // LoadHtml(wClient.DownloadString(url));
            /*foreach(HtmlNode link in doc.DocumentElement.SelectNodes("//a[@href")
            {
               HtmlAttribute att = link["href"];
               att.Value = FixLink(att);
            }*/
        }

        /*
    static void GetJobLinks(HtmlDocument html)
{
    var trNodes = html.GetElementbyId(«job-items»).ChildNodes.Where(x => x.Name == «tr»);
 
    foreach (var item in trNodes)
    {
        var tdNodes = item.ChildNodes.Where(x => x.Name == «td»).ToArray();
        if (tdNodes.Count() != 0)
        {
            var location = tdNodes[2].ChildNodes.Where(x => x.Name == «a»).ToArray();
 
            jobList.Add(new HabraJob()
            {
                Url = tdNodes[0].ChildNodes.First().Attributes[«href»].Value,
                Title = tdNodes[0].FirstChild.InnerText,
                Price = tdNodes[1].FirstChild.InnerText,
                Country = location[0].InnerText,
                Region = location[2].InnerText,
                City = location[2].InnerText
            });
        }
 
    }
 
}
*/


        private static List<List<string>> GetPage(string page, string rowsplitter, string cellsplitter,
                                                  List<string> unusefulltags)
        {
            string htmlCode;
            using (var client = new WebClient())
            {
                htmlCode = client.DownloadString(page);
            }

            // var strArray = htmlCode.Split(rowsplitter);
            string[] strArray = htmlCode.Split(new[] {rowsplitter}, StringSplitOptions.None);
            //  return str.Split(new[] { splitter }, StringSplitOptions.None);
            var result = new List<List<string>>();
            //    string[,] result = new string[,] {};
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
                // var rowlist = row.Split(cellsplitter);
                string[] rowlist = row.Split(new[] {cellsplitter}, StringSplitOptions.None);

                result.Add(new List<string>(rowlist));
            }
            return result;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac trades uploading");

                List<InitialTrade> LInitTrades = TradeParsing("Mac", "CSV", "FU", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "Mac");


                //  reportdate = MacTradeUploading();
                var db = new EXANTE_Entities(_currentConnection);
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);


                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Mac trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                // UpdateMacSymbol(reportdate);
            }
            //MacRecon(reportdate, cptradelist);
            //  Splittrades(reportdate, "Mac");


            UpdateMacSymbol(reportdate, "Mac");

            RecProcess(reportdate, "Mac",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void Splittrades(DateTime reportdate, string mac)
        {
            throw new NotImplementedException();
        }

        private void UpdateMacSymbol(DateTime reportdate, string cp)
        {
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                where
                                                    cptrade.valid == 1 && cptrade.BrokerId == "Mac" &&
                                                    cptrade.ReportDate >= reportdate.Date &&
                                                    cptrade.ReportDate <= (reportdate.Date) &&
                                                    cptrade.BOSymbol == null
                                                select cptrade;
            List<CpTrade> cptradelist = cptradefromDb.ToList();
            /*   var symbolmap = getMapping("Mac");
               var contractrow =
                       from ct in db.Contracts
                       where ct.valid == 1
                       select ct;
               var contractdetails = contractrow.ToDictionary(k => k.id, k => k);*/
            Dictionary<string, Map> symbolmap = GetMapSymbol(cp, db);

            foreach (CpTrade cpTrade in cptradelist)
            {
                Map symbolvalue;
                if (symbolmap.TryGetValue(cpTrade.Symbol + cpTrade.Type, out symbolvalue))
                {
                    string key = symbolvalue.BOSymbol + "." + getLetterOfMonth(cpTrade.ValueDate.Value.Month) +
                                 cpTrade.ValueDate.Value.Year;
                    Contract mapContract;
                    cpTrade.Price = cpTrade.Price*symbolvalue.MtyPrice;
                    cpTrade.value = -cpTrade.Price*cpTrade.Qty*symbolvalue.Leverage;
                    cpTrade.Qty = cpTrade.Qty*symbolvalue.MtyVolume;
                    cpTrade.BOSymbol = key;
                    /*      if (contractdetails.TryGetValue(key, out mapContract))
                          {
                              cpTrade.ValueDate = mapContract.ValueDate;
                              cpTrade.BOSymbol = key;
                          }
                          else
                          {
                              LogTextBox.AppendText("\r\n" + "Mac: No Map in Contracts for " + key);
                          }*/
                }
                db.CpTrades.Attach(cpTrade);
                db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                SaveDBChanges(ref db);
            }
        }

        private Dictionary<string, Map> GetMapSymbol(string cp, EXANTE_Entities db)
        {
            List<Mapping> mapfromDb =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == cp
                 select ct).ToList();

            var results = new Dictionary<string, Map>();
            List<Mapping> mapfromDblist = mapfromDb.ToList();
            foreach (Mapping item in mapfromDblist)
            {
                string key = item.BrockerSymbol;
                key = key + item.Type;
                results.Add(key, new Map
                    {
                        BOSymbol = item.BOSymbol,
                        MtyPrice = item.MtyPrice,
                        MtyVolume = item.MtyVolume,
                        Round = item.Round,
                        Type = item.Type,
                        MtyStrike = item.MtyStrike,
                        UseDayInTicker = item.UseDayInTicker,
                        calendar = item.calendar
                    });
            }
            return results;
        }


        private DateTime MacTradeUploading()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            int idSymbol = 6;
            int idMacside = 11;
            int idReportDate = 0;
            int idAccount = 1;
            int idcurrency = 4;
            int idTradeDate = 10;
            int idqty = 12;
            int idcp = 19;
            int idSellprice = 15;
            int idBuyPrice = 13;
            int idTypeofTrade = 8;
            int iddeliverydate = 7;
            int idcat = 5;
            int idexchfees = 24;
            int idfees = 23;
            int idoftrade = 32;
            Dictionary<string, Map> symbolmap = getMapping("Mac");
            int idTypeofOption = 9;
            int idstrike = 20;
            int idvalue = 39;
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                    select cp;
                Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();

                string lineFromFile = reader.ReadLine();
                Map symbolvalue;
                var reportdate = new DateTime();
                if (lineFromFile != null)
                {
                    string[] rowstring = lineFromFile.Replace("\"", "").Split(Delimiter);
                    IQueryable<Contract> contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);
                    while (!reader.EndOfStream)
                    {
                        lineFromFile = reader.ReadLine();
                        double? MtyVolume = 1;
                        double? MtyPrice = 1;
                        string BoSymbol = null;
                        double? Leverage = 1;
                        int round = 10;
                        double? mtystrike = 1;

                        if (lineFromFile == null) continue;
                        rowstring = lineFromFile.Replace("\"", "").Split(CSVDelimeter);
                        DateTime? valuedate = null;
                        Contract mapContract;

                        int side = 1;
                        double price = 0;
                        string symbol_id = rowstring[idSymbol].TrimEnd();
                        string key = symbol_id;
                        string typeoftrade = rowstring[idTypeofTrade].TrimEnd();
                        if (typeoftrade == "O")
                        {
                            key = key + "OP";
                        }
                        DateTime deliveryDate = DateTime.ParseExact(rowstring[iddeliverydate], "yyMM",
                                                                    CultureInfo.CurrentCulture);
                        if (symbolmap.TryGetValue(key, out symbolvalue))
                        {
                            MtyVolume = symbolvalue.MtyVolume;
                            MtyPrice = symbolvalue.MtyPrice;
                            BoSymbol = symbolvalue.BOSymbol;
                            round = (int) symbolvalue.Round;
                            mtystrike = symbolvalue.MtyStrike;
                            key = BoSymbol + ".";
                            BoSymbol = key + getLetterOfMonth(deliveryDate.Month) + deliveryDate.Year;
                            if (typeoftrade == "O")
                            {
                                key = BoSymbol + "." + rowstring[idTypeofOption].Trim() +
                                      (double.Parse(rowstring[idstrike], CultureInfo.InvariantCulture)*mtystrike)
                                          .ToString().Replace(".", "_");
                                BoSymbol = key;
                            }
                            if (contractdetails.TryGetValue(BoSymbol, out mapContract))
                            {
                                valuedate = mapContract.ValueDate;
                                Leverage = mapContract.Leverage;
                            }
                            else
                            {
                                valuedate = deliveryDate;
                                LogTextBox.AppendText("\r\n" + "Mac: No Map in Contracts for " + key);
                                if (typeoftrade == "O")
                                {
                                    BoSymbol = key;
                                }
                            }
                        }
                        else
                        {
                            LogTextBox.AppendText("\r\n" + "Mac: No Map in Mapping table for " + symbol_id);
                            valuedate = deliveryDate;
                        }

                        if (rowstring[idMacside] == "S")
                        {
                            side = -1;
                            price =
                                (double) (double.Parse(rowstring[idSellprice], CultureInfo.InvariantCulture)*MtyPrice);
                        }
                        else
                        {
                            price =
                                (double) (double.Parse(rowstring[idBuyPrice], CultureInfo.InvariantCulture)*MtyPrice);
                        }
                        reportdate = Convert.ToDateTime(rowstring[idReportDate]);
                        string account_id = rowstring[idAccount].TrimEnd();

                        string ccy = rowstring[idcurrency].TrimEnd();
                        DateTime TradeDate = Convert.ToDateTime(rowstring[idTradeDate]);
                        double? qty = rowstring[idqty].IndexOf(".") == -1
                                          ? Convert.ToInt64(rowstring[idqty])*side*MtyVolume
                                          : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)*side*MtyVolume;
                        int? cp_id = getCPid(rowstring[idcp].Trim(), cpdic);

                        string category = rowstring[idcat];
                        double value = double.Parse(rowstring[idvalue], CultureInfo.InvariantCulture);
                        double exchFees = double.Parse(rowstring[idexchfees], CultureInfo.InvariantCulture);
                        double Fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture);
                        string exchangeOrderId = rowstring[idoftrade].TrimEnd();

                        allfromfile.Add(new CpTrade
                            {
                                ReportDate = reportdate,
                                TradeDate = TradeDate,
                                BrokerId = "Mac",
                                Symbol = symbol_id,
                                Type = typeoftrade,
                                Qty = qty,
                                Price = price,
                                ValueDate = valuedate,
                                cp_id = cp_id,
                                ExchangeFees = exchFees,
                                Fee = Fees,
                                Id = null,
                                BOSymbol = BoSymbol,
                                BOTradeNumber = null,
                                value = value,
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "parser",
                                //  FullId = null,
                                BOcp = null,
                                exchangeOrderId = exchangeOrderId,
                                TypeOfTrade = category,
                                Comment = account_id,
                                ExchFeeCcy = ccy,
                                ClearingFeeCcy = ccy,
                                ccy = ccy
                            });
                    }
                }

                TradesParserStatus.Text = "DB updating";

                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                }

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                                   validationError.ErrorMessage);
                        }
                    }
                }
                return reportdate;
            }
            else return new DateTime(2011, 01, 01);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";

            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Lek trades uploading");
                reportdate = LekTradeUploading();
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Lek trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                var db = new EXANTE_Entities(_currentConnection);
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("LEK");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "LEK" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        cpTrade.value = cpTrade.value*Leverage;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "LEK",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private DateTime LekTradeUploading()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            //  var idSymbol = 7;
            int idMacside = 5;
            int idAccount = 1;
            //    var idcurrency = 10;
            //     var idTradeDate = 2;
            //   var idqty = 6;
            int idcp = 8;
            //    var idprice = 9;
            int idTypeofTrade = 8;
            int iddeliverydate = 4;
            int idvalue = 11;
            //   var idexchfees = 12;
            //     var idfees = 13;
            int idoftrade = 0;
            Dictionary<string, Map> symbolmap = getMapping("Lek");
            // var idTypeofOption = 9;
            //  var idstrike = 20;
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                ColumnMapping cMapping = (from ct in db.ColumnMappings
                                          where ct.Brocker == "LEK" && ct.FileType == "CSV"
                                          select ct).FirstOrDefault();

                IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                    select cp;
                Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();

                string lineFromFile = reader.ReadLine();
                // Map symbolvalue;
                var reportdate = new DateTime();
                if (lineFromFile != null)
                {
                    string[] rowstring = lineFromFile.Replace("\"", "").Split(Delimiter);
                    IQueryable<Contract> contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);
                    while (!reader.EndOfStream)
                    {
                        lineFromFile = reader.ReadLine();
                        double? MtyVolume = 1;
                        double? MtyPrice = 1;
                        string BoSymbol = null;
                        double? Leverage = 1;
                        //     int round = 10;

                        if (lineFromFile == null) continue;
                        rowstring = lineFromFile.Replace("\"", "").Split(CSVDelimeter);
                        DateTime valuedate;
                        int side = -1;
                        double price = 0;
                        string symbol_id = rowstring[(int) cMapping.cSymbol].TrimEnd();
                        string typeoftrade = rowstring[idTypeofTrade].TrimEnd();
                        string typeofInstrument = "ST";
                        valuedate = DateTime.ParseExact(rowstring[iddeliverydate], cMapping.DateFormat,
                                                        CultureInfo.CurrentCulture);
                        BoSymbol = GetSymbolLek(symbolmap, symbol_id, ref MtyVolume, contractdetails, ref MtyPrice,
                                                ref valuedate, ref Leverage);
                        price =
                            (double)
                            (double.Parse(rowstring[(int) cMapping.cPrice], CultureInfo.InvariantCulture)*MtyPrice);
                        if ((rowstring[idMacside] == "B") || (rowstring[idMacside] == "BOT"))
                        {
                            side = 1;
                        }
                        reportdate = DateTime.ParseExact(rowstring[(int) cMapping.cReportDate], cMapping.DateFormat,
                                                         CultureInfo.CurrentCulture);
                        string account_id = rowstring[idAccount].TrimEnd();

                        string ccy = rowstring[(int) cMapping.cCcy].TrimEnd();
                        DateTime TradeDate = DateTime.ParseExact(rowstring[(int) cMapping.cTradeDate],
                                                                 cMapping.DateFormat,
                                                                 CultureInfo.CurrentCulture);
                        double? qty = rowstring[(int) cMapping.cQty].IndexOf(".") == -1
                                          ? Convert.ToInt64(rowstring[(int) cMapping.cQty])*side*MtyVolume
                                          : double.Parse(rowstring[(int) cMapping.cQty], CultureInfo.InvariantCulture)*
                                            side*
                                            MtyVolume;
                        int? cp_id = getCPid(rowstring[idcp].Trim(), cpdic);
                        double exchFees = double.Parse(rowstring[(int) cMapping.cExchangeFees],
                                                       CultureInfo.InvariantCulture);
                        double value =
                            Math.Round(
                                -side*double.Parse(rowstring[(int) cMapping.cValue], CultureInfo.InvariantCulture), 2,
                                MidpointRounding.AwayFromZero);
                        double Fees = double.Parse(rowstring[(int) cMapping.cFee], CultureInfo.InvariantCulture);
                        string exchangeOrderId = rowstring[idoftrade].TrimEnd();
                        if (symbol_id.Contains("PUT") || symbol_id.Contains("CALL"))
                        {
                            typeofInstrument = "OP";
                        }
                        allfromfile.Add(new CpTrade
                            {
                                ReportDate = reportdate,
                                TradeDate = TradeDate,
                                BrokerId = "LEK",
                                Symbol = symbol_id,
                                Qty = qty,
                                Price = price,
                                ValueDate = valuedate,
                                cp_id = cp_id,
                                ExchangeFees = exchFees,
                                Fee = Fees,
                                Id = "",
                                TypeOfTrade = typeoftrade,
                                Type = typeofInstrument,
                                BOSymbol = BoSymbol,
                                BOTradeNumber = null,
                                value = value,
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "parser",
                                //  FullId = null,
                                BOcp = null,
                                exchangeOrderId = exchangeOrderId,
                                Comment = account_id,
                                ExchFeeCcy = ccy,
                                ClearingFeeCcy = ccy,
                                ccy = ccy
                            });
                    }
                }

                TradesParserStatus.Text = "DB updating";
                int i = 0;
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                    i++;
                }

               SaveDBChanges(ref db);
                LogTextBox.AppendText("\r\n" + "Lek: " + i + " trades have been added");
                return reportdate;
            }
            else return new DateTime(2011, 01, 01);
        }

        private string GetSymbolLek(Dictionary<string, Map> symbolmap, string symbol_id, ref double? MtyVolume,
                                    Dictionary<string, Contract> contractdetails, ref double? MtyPrice,
                                    ref DateTime valuedate, ref double? Leverage)
        {
            Map symbolvalue;
            int round;
            string BoSymbol = null;
            string key = symbol_id;

            if (symbol_id.Contains("(C)"))
            {
                key = symbol_id.Substring(0, symbol_id.IndexOf(" ")) + "OP";
            }
            if (symbolmap.TryGetValue(key, out symbolvalue))
            {
                MtyVolume = symbolvalue.MtyVolume;
                MtyPrice = symbolvalue.MtyPrice;
                BoSymbol = symbolvalue.BOSymbol;
                round = (int) symbolvalue.Round;
                key = BoSymbol + ".";
                if (symbol_id.Contains("(C)"))
                {
                    key = key + GetLekDayofOption(symbol_id);
                    key = key + "." + symbol_id.Substring(symbol_id.IndexOf(" ") + 1, 1) +
                          (GetLekStrike(symbol_id) /* * round */).ToString().Replace(".", "_");
                }
                else
                {
                    key = key + getLetterOfMonth(valuedate.Month) + valuedate.Year;
                }

                Contract mapContract;
                if (contractdetails.TryGetValue(key, out mapContract))
                {
                    valuedate = (DateTime) mapContract.ValueDate;
                    Leverage = mapContract.Leverage;
                    BoSymbol = key;
                }
                else
                {
                    LogTextBox.AppendText("\r\n" + "Lek: No Map in Contracts for " + key);
                }
            }
            else
            {
                LogTextBox.AppendText("\r\n" + "Lek: No Map in Mapping table for " + symbol_id);
            }
            return BoSymbol;
        }


        private string GetSymbolRJO(Dictionary<string, Map> symbolmap, string symbol_id, ref double? MtyVolume,
                                    Dictionary<string, Contract> contractdetails, ref double? MtyPrice,
                                    ref DateTime valuedate, ref double? Leverage, ref string typeoftrade)
        {
            Map symbolvalue;
            int round;
            string BoSymbol = null;
            string key = symbol_id;
            string type = "";
            string strike = "";
            typeoftrade = "FU";
            if (symbol_id.Contains("CALL") || symbol_id.Contains("PUT"))
            {
                type = symbol_id.Substring(0, symbol_id.IndexOf(" ")).Substring(0, 1);
                key = key.Substring(symbol_id.IndexOf(" ") + 1);
                typeoftrade = "OP";
            }
            int nextspace = key.IndexOf(" ");
            string month = key.Substring(0, nextspace);
            key = key.Substring(nextspace + 1);
            nextspace = key.IndexOf(" ");
            string year = "20" + key.Substring(0, nextspace);
            key = key.Substring(nextspace + 1);
            if (type != "")
            {
                strike = key.Substring(key.LastIndexOf(" ") + 1);
                key = key.Substring(0, key.LastIndexOf(" ")).TrimEnd();
            }
            key = key + typeoftrade;

            if (symbolmap.TryGetValue(key, out symbolvalue))
            {
                MtyVolume = symbolvalue.MtyVolume;
                MtyPrice = symbolvalue.MtyPrice;
                BoSymbol = symbolvalue.BOSymbol;
                round = (int) symbolvalue.Round;
                key = BoSymbol + "." + GetMonthLetter(month) + year;
                if (type != "")
                {
                    if (symbolvalue.MtyStrike != null)
                        strike =
                            (Math.Round((decimal) (Convert.ToInt32(strike)*symbolvalue.MtyStrike), 5)).ToString()
                                                                                                      .Replace('.', '_');
                    key = key + "." + type + strike;
                }

                Contract mapContract;
                if (contractdetails.TryGetValue(key, out mapContract))
                {
                    int Digitmonth = GetMonthFromLetter(GetMonthLetter(month));
                    if (Digitmonth < 10) month = "0" + Digitmonth;
                    var db = new EXANTE_Entities(_currentConnection);
                    string t = "update Ctrades SET value_date= '" + year + "-" + month + "-01' where symbol_id='" + key +
                               "'";
                    //  db.Database.ExecuteSqlCommand("CALL updatecontract('" + key + "','" + year + "-" + month + "-01')");
                    //  db.Database.ExecuteSqlCommand("update Ctrades SET value_date= '" + year + "-" + month + "-01' where symbol_id='"+key+"'");
                    //  db.Database.ExecuteSqlCommand("update Contracts SET ValueDate= '" + year + "-" + month + "-01' where id='" + key + "'");
                    db.Dispose();
                    // valuedate = (DateTime) mapContract.ValueDate;
                    Leverage = mapContract.Leverage;
                    BoSymbol = key;
                }
                else
                {
                    LogTextBox.AppendText("\r\n" + "Lek: No Map in Contracts for " + key);
                }
            }
            else
            {
                LogTextBox.AppendText("\r\n" + "Lek: No Map in Mapping table for " + symbol_id);
            }
            return BoSymbol;
        }


        private static double GetLekStrike(string symbol_id)
        {
            string t = symbol_id.Substring(CustomIndexOf(symbol_id, ' ', 3) + 1,
                                           CustomIndexOf(symbol_id, ' ', 4) - CustomIndexOf(symbol_id, ' ', 3) - 1);
            return
                double.Parse(symbol_id.Substring(CustomIndexOf(symbol_id, ' ', 3) + 1,
                                                 CustomIndexOf(symbol_id, ' ', 4) - CustomIndexOf(symbol_id, ' ', 3) - 1));
        }

        private static string GetLekDayofOption(string symbol_id)
        {
            string str = null;
            if (symbol_id != null)
            {
                int index = symbol_id.IndexOf(" ", StringComparison.Ordinal);
                index = symbol_id.IndexOf(" ", index + 1, StringComparison.Ordinal);
                string daystr = symbol_id.Substring(index + 1, 2);
                short daystr2 = Convert.ToInt16(daystr);
                str = daystr2.ToString(CultureInfo.InvariantCulture);
                string month = symbol_id.Substring(index + 3, 3);
                string year = "20" + symbol_id.Substring(index + 6, 2);
                /*   switch (month)
                   {
                       case "DEC":
                           str = String.Concat(str, "Z");
                           break;
                   }*/
                //   var t = DateTime.TryParseExact(symbol_id.Substring(index + 1, 7), "ddMMMyy",CultureInfo.InvariantCulture);
                string t = GetMonthLetter(month);
                str = String.Concat(str, t);
                /*   else if (month.Contains("DEC"))
                   {
                       str = String.Concat(str, "Z");
                   }*/


                /*     else if (month == "Mar")
                         {
                             str = str + "H" ;
                         }*/
                str = str + year;
                /*  case "Apr":
                        str =str + "J" + year;
                        break;
                /*    case "May":
                        return str + "K" + year;
                    case "Jun":
                        return str + "M" + year;
                    case "Jul":
                        return str + "N" + year;
                    case "Aug":
                        return str + "Q" + year;
                    case "Sep":
                        return str + "U" + year;
                /*    case "Oct":
                        return str + "V" + year;
                    case "Nov":
                        return str + "X" + year;
                    case "Dec":
                        return str + "Z" + year;
                    default:
                        return str + month + year;
                }*/
            }
            return str;
        }

        private static string GetMonthLetter(string month)
        {
            string letter;
            switch (month)
            {
                case "JAN":
                    letter = "F";
                    break;
                case "FEB":
                    letter = "G";
                    break;
                case "MAR":
                    letter = "H";
                    break;
                case "APR":
                    letter = "J";
                    break;
                case "MAY":
                    letter = "K";
                    break;
                case "JUN":
                    letter = "M";
                    break;
                case "JUL":
                    letter = "N";
                    break;
                case "AUG":
                    letter = "Q";
                    break;
                case "SEP":
                    letter = "U";
                    break;
                case "OCT":
                    letter = "V";
                    break;
                case "NOV":
                    letter = "X";
                    break;
                    /*  case 'M':
if (month.Contains("MAR"))
{
  return "H";
}
else
{
  return "K";
}
break;*/
                    /*    case 'A':
return "G";
break;*/

                case "DEC":
                    letter = "Z";
                    break;
                default:
                    letter = month;
                    break;
            }
            return letter;
        }

        private void GetABNPos(Dictionary<string, List<string>> cliffdict, DateTime reportdate)
        {
            List<string> rowlist;
            DateTime TimeFuturepositionStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeFuturepositionStart + ": " + "start ABN future position parsing");

            if (cliffdict.TryGetValue("320", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeFutureParsing = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN future position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeFutureParsing - TimeFuturepositionStart).ToString() + "s");

            DateTime TimeStockPositionStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStockPositionStart + ": " + "start ABN stock position parsing");
            if (cliffdict.TryGetValue("420", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeStockParsing = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN stock position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeStockParsing - TimeStockPositionStart).ToString() + "s");

            DateTime TimeOptionPositionStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeOptionPositionStart + ": " + "start ABN option position parsing");
            if (cliffdict.TryGetValue("220", out rowlist))
            {
                reportdate = ExtractPositionFromCliff(rowlist);
            }
            DateTime TimeOptionParsing = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                  "ABN stock position parsing completed for " + reportdate.ToShortDateString() +
                                  ". Time:" +
                                  (TimeOptionParsing - TimeOptionPositionStart).ToString() + "s");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var reportdate = new DateTime(2012, 05, 14);
            var prevdate = new DateTime(2012, 05, 04);
            DateTime TimeStart = DateTime.Now;
            List<Ftbo> ftboitems =
                (from ct in db.Ftboes
                 where
                     ct.botimestamp >= prevdate && ct.botimestamp <= reportdate &&
                     (ct.symbolId == "" || ct.symbolId == null) && ct.tradeNumber != null
                 select ct).ToList();
            //ToDictionary(k => (k.tradeNumber.ToString()+k.gatewayId), k => k);
            int index = 0;
            Dictionary<string, string> ctradeitems =
                (from ct in db.Ctrades
                 where ct.BOtradeTimestamp <= reportdate.Date && ct.BOtradeTimestamp >= prevdate.Date
                 select ct).ToDictionary(k => (k.tradeNumber.ToString() + k.gatewayId), k => k.symbol_id);
            foreach (Ftbo ftbo in ftboitems)
            {
                string symbolid;
                if (ctradeitems.TryGetValue(ftbo.tradeNumber.ToString() + ftbo.gatewayId, out symbolid))
                {
                    ftbo.symbolId = symbolid;
                    db.Ftboes.Attach(ftbo);
                    db.Entry(ftbo).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    index++;
                }
                else
                {
                    LogTextBox.AppendText("\r\n" + "Didn't find trade for this id:" + ftbo.id + " " + ftbo.tradeNumber);
                }
            }
            db.SaveChanges();
            //  var n = queryable.Count();
            //  var m = queryable2.Count();
            DateTime TimeFutureParsing = DateTime.Now;
            db.Dispose();
            LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + " Updating symbol completed for " +
                                  index + " items. Time: " + (TimeFutureParsing - TimeStart).ToString() + "s");
        }


        private void Jsontodictionary(string json)
        {
            JArray objects = JArray.Parse(json); // parse as array  
            foreach (JObject root in objects)
            {
                foreach (var app in root)
                {
                    string appName = app.Key;
                    var description = (String) app.Value["Description"];
                    var value = (String) app.Value["Value"];
                }
            }
        }

        private static string ClearString(string str)
        {
            str = str.Trim();

            int ind0 = str.IndexOf("\"");
            int ind1 = str.LastIndexOf("\"");

            if (ind0 != -1 && ind1 != -1)
            {
                str = str.Substring(ind0 + 1, ind1 - ind0 - 1);
            }
            else if (str[str.Length - 1] == ',')
            {
                str = str.Substring(0, str.Length - 1);
            }

            str = HttpUtility.UrlDecode(str);

            return str;
        }

        private static Dictionary<string, string> ParseJson(string res)
        {
            string[] lines = res.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var ht = new Dictionary<string, string>(20);
            var st = new Stack<string>(20);

            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                string[] pair = line.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length == 2)
                {
                    string key = ClearString(pair[0]);
                    string val = ClearString(pair[1]);

                    if (val == "{")
                    {
                        st.Push(key);
                    }
                    else
                    {
                        if (st.Count > 0)
                        {
                            key = string.Join("_", st) + "_" + key;
                        }

                        if (ht.ContainsKey(key))
                        {
                            ht[key] += "&" + val;
                        }
                        else
                        {
                            ht.Add(key, val);
                        }
                    }
                }
                else if (line.IndexOf('}') != -1 && st.Count > 0)
                {
                    st.Pop();
                }
            }

            return ht;
        }

        private BOjson JsonfromCpTrade(CpTrade cptrade, string account, string accountclientid)
        {
            var p = new BOjson();
            p.tradeType = "TRADE";
            p.symbolId = cptrade.BOSymbol;
            p.quantity = Math.Abs((double) cptrade.Qty).ToString();
            p.price = cptrade.Price.ToString();
            p.gwTime = ((DateTime) cptrade.TradeDate).ToString("yyyy-MM-dd HH:mm:ss");
            if (((DateTime) cptrade.ValueDate).ToString("yyyy-MM-dd") == "2011-01-01")
            {
                p.valueDate = ((DateTime) cptrade.TradeDate).ToString("yyyy-MM-dd");
            }
            else
            {
                p.valueDate = ((DateTime) cptrade.ValueDate).ToString("yyyy-MM-dd");
            }
            p.side = cptrade.Qty > 0 ? "buy" : "sell";
            p.userId = "az";
            p.counterparty = cptrade.BOcp;
            //    p.counterparty = cptrade.Comment;
            //   p.settlementCounterparty = "LEK";

            p.settlementBrokerAccountId = account;
            //            p.settlementBrokerAccountId = "IUM1307.001";
            //  p.settlementCounterparty = "EXANTE";
            p.settlementCounterparty = cptrade.Comment;//убрать
            //  p.brokerAccountId = accountclientid;
           // p.comment = "Reverse trade 18.11.2016";//убрать
            p.internalComment = cptrade.exchangeOrderId;
            //p.commission = (-cptrade.ExchangeFees).ToString();
            // p.commissionCurrency = "USD";
            p.takeCommission = true;
            //   p.takeCommission = false;//убрать
           //  p.comment = "Reversal of trade  " + cptrade.exchangeOrderId.ToString();//убрать
            p.redemption = false;
            p.isManual = true;
            return p;
        }

        private BOjson JsonfromCtrade(Ctrade ctrade,bool takecommission)
        {
            var p = new BOjson();
            p.tradeType =ctrade.tradeType ;
            p.symbolId = ctrade.symbol_id;
            p.quantity = Math.Abs((double)ctrade.qty).ToString();
            p.price = ctrade.price.ToString();
            p.gwTime = ((DateTime)ctrade.Date).ToString("yyyy-MM-dd HH:mm:ss");
            if (((DateTime)ctrade.value_date).ToString("yyyy-MM-dd") == "2011-01-01")
            {
                p.valueDate = ((DateTime)ctrade.Date).ToString("yyyy-MM-dd");
            }
            else
            {
                p.valueDate = ((DateTime)ctrade.value_date).ToString("yyyy-MM-dd");
            }
            p.side = ctrade.qty > 0 ? "buy" : "sell";
            p.userId = "az";
            p.counterparty = ctrade.cp_id;
            p.settlementCounterparty = ctrade.SettlementCp;
            p.settlementBrokerAccountId = ctrade.account_id;
            p.takeCommission = true;
            if (!takecommission)
            {
                p.takeCommission = false;
            }
            p.redemption = false;
            p.isManual = true;
            return p;
        }

        private FTjson FeeJsonfromCpTrade(CpTrade cptrade)
        {
            var p = new FTjson();
            p.operationType = "COMMISSION";
            p.symbolId = cptrade.BOSymbol;
            p.asset = cptrade.ExchFeeCcy;
            p.accountId = cptrade.account;
            double amount = 0;
            if (cptrade.ExchangeFees != null)
            {
                amount = -Math.Abs((double) cptrade.ExchangeFees);
            }
            if (cptrade.Fee != null)
            {
                amount = amount - Math.Abs((double) cptrade.Fee);
            }
            p.amount = amount.ToString();
            p.timestamp = ((DateTime) cptrade.TradeDate).ToString("yyyy-MM-dd HH:mm:ss");
            p.comment = cptrade.exchangeOrderId;
            p.internalComment = cptrade.Symbol;
            return p;
        }

        private string GetToken(string connectionstring, string service,string typeofconnection)
        {
            var DBurl = new Uri(connectionstring);
            var dbReq = WebRequest.Create(DBurl) as HttpWebRequest;
            dbReq.ContentType = "application/json";
            dbReq.UserAgent = "curl/7.37.0";
            List<string> credential = getcredentials(typeofconnection);
            string requestokenstr = "{\"username\":\"" + credential[0] + "\", \"password\" : \"" + credential[1] +"\",\"service\":\"";
            // string requestokenstr = "{\"username\":\"" + "alr@exante.eu" + "\", \"password\" : \"" + "Zarevo1932334346" + "\",\"service\":\"";
          // string requestokenstr = "{\"username\":\"" + "az" + "\", \"password\" : \"" + "AF*(*HBfdfacb" + "\",\"service\":\"";
            string requestoken = requestokenstr + service + "\"}";
            dbReq.Method = "POST";
            var encoding = new UTF8Encoding();
            dbReq.ContentLength = encoding.GetByteCount(requestoken);
            string token = "";
            using (Stream requestStream = dbReq.GetRequestStream())
            {
                requestStream.Write(encoding.GetBytes(requestoken), 0,
                                    encoding.GetByteCount(requestoken));
            }
            try
            {
                var response = dbReq.GetResponse() as HttpWebResponse;
                string responseBody = "";
                using (Stream rspStm = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(rspStm))
                    {
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Description: " + response.StatusDescription;
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + response.StatusCode;
                        LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                        responseBody = reader.ReadToEnd();
                        if (!ParseJson(responseBody).TryGetValue("sessionid", out token))
                        {
                            LogTextBox.AppendText("\r\n" + "Key sessionid is not existed");
                        }
                    }
                }
                LogTextBox.Text = "Success: " + response.StatusCode.ToString();
            }
            catch (WebException ex)
            {
                LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                var reader = new StreamReader(ex.Response.GetResponseStream());
                LogTextBox.Text = LogTextBox.Text + reader.ReadToEnd();
            }

            return token;
        }

        private List<string> getcredentials(string type)
        {
            var reader = new StreamReader(@"C:\logins.txt");
            var allfromfile = new List<string>();
            while (!reader.EndOfStream)
            {
                string[] text = reader.ReadLine().Split(';');
                if (text[0] == type)
                {
                    allfromfile.Add(text[1]);
                    allfromfile.Add(text[2]);
                    return allfromfile;
                }
            }
            return null;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //var strZamTransaction = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ZAM1452.001/transaction";
            //    var strAdsTrade = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ADS1450.002/trade";
            BOaccount acc = GetAccount();
            bool sendFee = true;
            bool sendPL = false;
            string token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice", "prod");
            if (!checkBoxAllDates.Checked)
            {
                DateTime reportdate = InputDate.Value;
                postTradesforDate(acc, reportdate, sendFee, sendPL, token, conStr, acc.BOaccountId, null);
            }
            else
            {
                DateTime reportdate = InputDate.Value;
                DateTime enddate = DateTime.Today;
                while (reportdate < enddate)
                {
                    postTradesforDate(acc, reportdate, sendFee, sendPL, token, conStr, acc.BOaccountId, null);
                    reportdate = reportdate.AddDays(1);
                }
            }
        }

        private void postTradesforDate(BOaccount acc, DateTime reportdate, bool sendFee, bool sendPL, string token,
                                       string conStr, string account, string Broker)
        {
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            IQueryable<CpTrade> cptradefromDb = from Cptrade in db.CpTrades
                                                where Cptrade.valid == 1 && Cptrade.BrokerId == Broker &&
                                                      Cptrade.ReportDate >= reportdate.Date &&
                                                      Cptrade.ReportDate < (nextdate.Date)
                                                //&& Cptrade.ReconAccount == null
                                                select Cptrade;
            List<CpTrade> cptradeitem = cptradefromDb.ToList();
            int tradesqty = 0;
            foreach (CpTrade cpTrade in cptradeitem)
            {
                if (cpTrade.ReconAccount == null)
                {
                    tradesqty = BoReconPostTrade(cpTrade, acc, conStr, token, tradesqty);

                    if (sendFee)
                    {
                        BoReconPostFee(cpTrade, conStr, acc, token);
                    }
                }
            }
            //json = FeeJsonfromCpTrade(cpTrade, accountnumber, "60002000000 - Exante Trading Account");

            if (sendPL)
            {
                IQueryable<FT> FTfromDb = from ft in db.FT
                                          where ft.valid == 1 && ft.brocker == acc.DBcpName &&
                                                ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                                ft.account_id == acc.BOaccountId && ft.Type == "PL"
                                          select ft;
                List<FT> FTfromDbeitem = FTfromDb.ToList();
                foreach (FT ft in FTfromDbeitem)
                {
                    BoReconPostPnL(ft, conStr, acc, token);
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradeitem.Count);
            }
        }

        private static BOaccount GetAccount()
        {
            var db = new EXANTE_Entities(_currentConnection);
            List<DBBORecon_mapping> brockerlist = (from rec in db.DBBORecon_mapping
                                                   where rec.valid == 1 && rec.NameProcess == _currentAcc
                                                   select rec).ToList();
            var result = new BOaccount
                {
                    accountNameCP = brockerlist[0].accountNameCP,
                    BOaccountId = brockerlist[0].boaccountid,
                    DBcpName = brockerlist[0].dbcp
                };
            return result;
        }

        private void BoReconPostPnL(FT ft, string conStr, BOaccount acc, string token)
        {
            FTjson bjson;
            bjson = PnlLeftJsonfromFt(ft, "PNL SETTLEMENT");
            string requestFTload = JsonConvert.SerializeObject(bjson);
            if (!SendJson(requestFTload, conStr + acc.BOaccountId + "/transaction", token))
            {
                LogTextBox.AppendText("\r\n Error in sending Left side VM to BO for fullid: " + ft.fullid);
            }
            bjson = PnlRightJsonfromFt(ft, "PNL SETTLEMENT");
            requestFTload = JsonConvert.SerializeObject(bjson);
            if (!SendJson(requestFTload, conStr + acc.BOaccountId + "/transaction", token))
            {
                LogTextBox.AppendText("\r\n Error in sending Right side VM to BO for fullid: " + ft.fullid);
            }
        }

        private FTjson PnlRightJsonfromFt(FT ft, string operationtype)
        {
            var p = new FTjson();
            p.operationType = operationtype;
            p.symbolId = ft.BOSymbol;
            p.asset = ft.counterccy;
            p.amount = ft.ValueCCY.ToString();
            p.timestamp = ((DateTime) ft.TradeDate).ToString("yyyy-MM-dd HH:mm:ss");
            p.comment = ft.Comment;
            p.internalComment = ft.symbol;
            return p;
        }

        private FTjson PnlLeftJsonfromFt(FT ft, string operationtype)
        {
            var p = new FTjson();
            p.operationType = operationtype;
            p.symbolId = ft.BOSymbol;
            p.asset = ft.ccy;
            p.amount = ft.value.ToString();
            p.timestamp = ((DateTime) ft.TradeDate).ToString("yyyy-MM-dd HH:mm:ss");
            p.comment = ft.Comment;
            p.internalComment = ft.symbol;
            return p;
        }

        private int BoReconPostTrade(CpTrade cpTrade, BOaccount acc, string conStr, string token, int tradesqty)
        {
            string accountnumber = null;
            if (cpTrade.BOTradeNumber != null)
            {
                int? tradenumber = Convert.ToInt32(cpTrade.BOTradeNumber.Split(';')[0]);
                accountnumber = GetAccountIdFromTradeNumber(tradenumber);
            }
            BOjson json = JsonfromCpTrade(cpTrade, accountnumber, acc.accountNameCP);
            string requestPayload = JsonConvert.SerializeObject(json);
            //      if (SendJson(requestPayload, conStr + acc.BOaccountId + "/trade", token))
            if (SendJson(requestPayload, conStr + cpTrade.account + "/trade", token))
            {
                cpTrade.ReconAccount = cpTrade.account;
                tradesqty++;
            }
            else
            {
                LogTextBox.AppendText("\r\n Error in sending to BO for fullid: " + cpTrade.FullId);
            }
            return tradesqty;
        }

        private void BoReconPostFee(CpTrade cpTrade, string conStr, BOaccount acc, string token)
        {
            FTjson bjson = null;
            bjson = FeeJsonfromCpTrade(cpTrade);
            string requestFTload = JsonConvert.SerializeObject(bjson);
            if (!SendJson(requestFTload, conStr + acc.BOaccountId + "/transaction", token))
            {
                LogTextBox.AppendText("\r\n Error in sending to fee to BO for fullid: " + cpTrade.FullId);
            }
        }

        private static string GetAccountIdFromTradeNumber(int? tradenumber)
        {
            var db = new EXANTE_Entities(_currentConnection);
            string accountnumber = (from ctrade in db.Ctrades
                                    where ctrade.valid == 1 && ctrade.tradeNumber == tradenumber
                                    select ctrade.account_id).ToList()[0];
            db.Dispose();
            return accountnumber;
        }

        private bool SendJson(string requestPayload, string constr, string token)
        {
            var uri = new Uri(constr);
            var encoding = new UTF8Encoding();
            var r = WebRequest.Create(uri) as HttpWebRequest;
            r.Method = "PUT";
            r.UserAgent = "curl/7.37.0";
            r.ContentLength = encoding.GetByteCount(requestPayload);
            r.Credentials = CredentialCache.DefaultCredentials;
            List<string> credential = getcredentials("prod");
            var Credentials = new NetworkCredential(credential[0], credential[1]); //bo
            r.Credentials = Credentials;
            r.Accept = "application/json";
            r.ContentType = "application/json";
            r.Headers.Add("X-Auth-Username", "az");
            r.Headers.Add("X-Auth-SessionId", token);
            using (Stream requestStream = r.GetRequestStream())
            {
                requestStream.Write(encoding.GetBytes(requestPayload), 0, encoding.GetByteCount(requestPayload));
            }

            try
            {
                var response = r.GetResponse() as HttpWebResponse;
                string responseBody = "";
                using (Stream rspStm = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(rspStm))
                    {
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Description: " + response.StatusDescription;
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + response.StatusCode;
                        LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                        responseBody = reader.ReadToEnd();
                    }
                }
                LogTextBox.Text = LogTextBox.Text + "\r\nSuccess: " + response.StatusCode.ToString();
                return true;
            }
            catch (WebException ex)
            {
                LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                // get error details sent from the server
                var reader = new StreamReader(ex.Response.GetResponseStream());
                LogTextBox.Text = LogTextBox.Text + reader.ReadToEnd();
                return false;
            }
        }

        private bool SendJsonGET(string requestPayload, string constr, string token)
        {
            


            
            
            var uri = new Uri(constr);
            var encoding = new UTF8Encoding();
            var r = WebRequest.Create(uri) as HttpWebRequest;
            r.Method = "GET";
            r.UserAgent = "curl/7.37.0";
            r.ContentLength = encoding.GetByteCount(requestPayload);
            r.Credentials = CredentialCache.DefaultCredentials;
            List<string> credential = getcredentials("prod");
            var Credentials = new NetworkCredential(credential[0], credential[1]); //bo
            r.Credentials = Credentials;
            r.Accept = "application/json";
            r.ContentType = "application/json";
            r.Headers.Add("X-Auth-Username", "az");
            r.Headers.Add("X-Auth-SessionId", token);
            using (Stream requestStream = r.GetRequestStream())
            {
                requestStream.Write(encoding.GetBytes(requestPayload), 0, encoding.GetByteCount(requestPayload));
            }

            try
            {
                var response = r.GetResponse() as HttpWebResponse;
                string responseBody = "";
                using (Stream rspStm = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(rspStm))
                    {
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Description: " + response.StatusDescription;
                        LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + response.StatusCode;
                        LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                        responseBody = reader.ReadToEnd();
                    }
                }
                LogTextBox.Text = LogTextBox.Text + "\r\nSuccess: " + response.StatusCode.ToString();
                return true;
            }
            catch (WebException ex)
            {
                LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                // get error details sent from the server
                var reader = new StreamReader(ex.Response.GetResponseStream());
                LogTextBox.Text = LogTextBox.Text + reader.ReadToEnd();
                return false;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText(TimeStart + ": " + "Getting ccy prices from MOEX");
            // var FORTSDate = InputDate.Value.ToString("dd.MM.yyyy");
            string FORTSDate = InputDate.Value.ToString("dd.MM.yyyy");
            //  updateFORTSccyrates(FORTSDate);
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + "CCY FORTS rates for " + FORTSDate +
                                  " uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());

            calcualteVM(InputDate.Value, "ATON");
            DateTime TimeEndVMCalculation = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndVMCalculation + ": " + "VM calculation " + FORTSDate +
                                  " completed. Time:" + (TimeEndVMCalculation - TimeEndUpdating).ToString());
        }

        private void aBNPositionParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime reportdate = InputDate.Value;
                    Dictionary<string, List<string>> cliffdict = LoadCliff(openFileDialog2.FileName, reportdate);
                    GetABNPos(cliffdict, reportdate);
                }
            }
        }

        private void aBNFTParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime reportdate = InputDate.Value;
                    Dictionary<string, List<string>> cliffdict = LoadCliff(oFilename, reportdate);
                    /*  var dbentity = new EXANTE_Entities(_currentConnection);
                      var testdate = reportdate.ToShortDateString();
                      var cpidfromDb = from cp in dbentity.DailyChecks
                                       where cp.Table == "FT" && cp.date.ToString() == testdate
                                       select cp.status;*/
                    List<string> rowlist;
                    DateTime TimeUpdateBalanceStart = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start FT parsing reconciliation");
                    if (cliffdict.TryGetValue("600", out rowlist))
                    {
                        reportdate = getcashmovements(rowlist);
                    }
                    DateTime TimeFutureParsing = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                          "FT parsing completed for " + reportdate.ToShortDateString() + ". Time:" +
                                          (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");
                }
            }
        }

        private void bOFTUploadingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime TimeUpdateBalanceStart = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start FT BO uploading");

                    var reportdate = new DateTime(2016, 09, 20);
                    DateTime TimeStart = DateTime.Now;
                    var db = new EXANTE_Entities(_currentConnection);
                    db.Database.CommandTimeout = 300;
                    var reader = new StreamReader(oFilename);
                    string lineFromFile = reader.ReadLine();
                    int index = 0;
                    int Rowindex = 0;
                    if (lineFromFile != null)
                    {
                        string[] rowstring = lineFromFile.Split(Delimiter);

                        int idid = 0;
                        int idaccountId = 0;
                        int idtimestamp = 0;
                        int idoperationType = 0;
                        int idasset = 0;
                        int idsum = 0;
                        int idwho = 0;
                        int idsymbolId = 0;
                        int idtradeNumber = 0;
                        int idcomment = 0;
                        int idinternalComment = 0;
                        int idsymbolType = 0;
                        int idvalueDate = 0;
                        int idorderId = 0;
                        int idorderPos = 0;
                        int idprice = 0;
                        int idclientType = 0;
                        int idexecutionCounterparty = 0;
                        int idcategory = 0;
                        int idbaseCurrency = 0;
                        int idsettlementCurrency = 0;
                        int idsettlementCurrencyMovement = 0;
                        int idexchangeCommission = 0;
                        int idsettlementCounterparty = 0;
                        int idtransferId = 0;
                        int idclientCounterparty = 0;
                        int idexanteCounterparty = 0;
                        for (int i = 0; i < rowstring.Length; i++)
                        {
                            switch (rowstring[i])
                            {
                                case "timestamp":
                                    idtimestamp = i;
                                    break;
                                case "asset":
                                    idasset = i;
                                    break;
                                case "accountId":
                                    idaccountId = i;
                                    break;
                                case "sum":
                                    idsum = i;
                                    break;
                                case "price":
                                    idprice = i;
                                    break;
                                case "id":
                                    idid = i;
                                    break;
                                case "operationType":
                                    idoperationType = i;
                                    break;
                                case "who":
                                    idwho = i;
                                    break;
                                case "tradeNumber":
                                    idtradeNumber = i;
                                    break;
                                case "orderId":
                                    idorderId = i;
                                    break;
                                case "symbolId":
                                    idsymbolId = i;
                                    break;
                                case "comment":
                                    idcomment = i;
                                    break;
                                case "internalComment":
                                    idinternalComment = i;
                                    break;
                                case "orderPos":
                                    idorderPos = i;
                                    break;
                                case "valueDate":
                                    idvalueDate = i;
                                    break;
                                case "clientType":
                                    idclientType = i;
                                    break;
                                case "executionCounterparty":
                                    idexecutionCounterparty = i;
                                    break;
                                case "category":
                                    idcategory = i;
                                    break;
                                case "symbolType":
                                    idsymbolType = i;
                                    break;
                                case "baseCurrency":
                                    idbaseCurrency = i;
                                    break;
                                case "transferId":
                                    idtransferId = i;
                                    break;
                                case "clientCounterparty":
                                    idclientCounterparty = i;
                                    break;
                                case "exanteCounterparty":
                                    idexanteCounterparty = i;
                                    break;
                                case "settlementCounterparty":
                                    idsettlementCounterparty = i;
                                    break;
                                case "exchangeCommission":
                                    idexchangeCommission = i;
                                    break;
                                case "settlementCurrency":
                                    idsettlementCurrency = i;
                                    break;
                                case "settlementCurrencyMovement":
                                    idsettlementCurrencyMovement = i;
                                    break;
                                default:
                                    LogTextBox.AppendText("Additional fields in the FT.file!");
                                    break;
                            }
                        }
                        Dictionary<long, long> checkId =
                            (from ct in db.Ftboes
                             where ct.botimestamp.ToString().Contains("2016-09")
                             select ct.id).ToDictionary(k => k, k => k);
                        while (!reader.EndOfStream)
                        {
                            Rowindex++;
                            lineFromFile = reader.ReadLine();
                            if (lineFromFile == null) continue;
                            rowstring = lineFromFile.Split(Delimiter);
                            long id = Convert.ToInt64(rowstring[idid]);
                            if (!checkId.ContainsKey(id))
                            {
                                index++;
                              /*     var id1 = Convert.ToInt64(rowstring[idid]);
                                var accountId = rowstring[idaccountId];
                                var baseCurrency = rowstring[idbaseCurrency];
                                var transferId = rowstring[idtransferId] == ""
                                                   ? (int?)null
                                                   : Convert.ToInt32(rowstring[idtransferId]);
                                var settlementCurrencyMovement = rowstring[idsettlementCurrencyMovement] == ""
                                                   ? (double?)null
                                                   : Convert.ToDouble(rowstring[idsettlementCurrencyMovement]);
                                var settlementCurrency = rowstring[idsettlementCurrency];
                                var clientCounterparty = rowstring[idclientCounterparty];
                                var exchangeCommission = rowstring[idexchangeCommission] == ""
                                                   ? (double?)null
                                                   : Convert.ToDouble(rowstring[idexchangeCommission]);
                                var settlementCounterparty = rowstring[idsettlementCounterparty];
                                var exanteCounterparty = rowstring[idexanteCounterparty];
                                var asset = rowstring[idasset];
                                var botimestamp = Convert.ToDateTime(rowstring[idtimestamp]);
                                var clientType = rowstring[idclientType];
                                var comment = rowstring[idcomment] + rowstring[idinternalComment];
                                var executionCounterparty = rowstring[idexecutionCounterparty];
                                var symbolType = rowstring[idsymbolType];
                                var category = rowstring[idcategory];
                                var operationType = rowstring[idoperationType];
                                var orderId = rowstring[idorderId];
                                var orderPos = rowstring[idorderPos] == ""
                                                   ? (long?) null
                                                   : Int64.Parse(rowstring[idorderPos]);
                                var price = rowstring[idprice] == "" ? (double?) null : double.Parse(rowstring[idprice]);
                                var sum = double.Parse(rowstring[idsum]);
                                var who = rowstring[idwho];
                                var tradeNumber =
                                    rowstring[idtradeNumber] == ""
                                        ? (long?) null
                                        : Int64.Parse(rowstring[idtradeNumber]);
                                var symbolId = rowstring[idsymbolId];
                                var valueDate =
                                    rowstring[idvalueDate] == ""
                                        ? (DateTime?) null
                                        : DateTime.Parse(rowstring[idvalueDate]);*/

                                db.Ftboes.Add(new Ftbo
                                    {
                                        id = Convert.ToInt64(rowstring[idid]),
                                        accountId = rowstring[idaccountId],
                                        baseCurrency = rowstring[idbaseCurrency],
                                        transferId = rowstring[idtransferId] ,
                                        settlementCurrencyMovement = rowstring[idsettlementCurrencyMovement] == ""
                                                                         ? (double?) null
                                                                         : Convert.ToDouble(
                                                                             rowstring[idsettlementCurrencyMovement]),
                                        settlementCurrency = rowstring[idsettlementCurrency],
                                        clientCounterparty = rowstring[idclientCounterparty],
                                        exchangeCommission = rowstring[idexchangeCommission] == ""
                                                                 ? (double?) null
                                                                 : Convert.ToDouble(rowstring[idexchangeCommission]),
                                        settlementCounterparty = rowstring[idsettlementCounterparty],
                                        exanteCounterparty = rowstring[idexanteCounterparty],
                                        asset = rowstring[idasset],
                                        botimestamp = Convert.ToDateTime(rowstring[idtimestamp]),
                                        clientType = rowstring[idclientType],
                                        comment = rowstring[idcomment] + rowstring[idinternalComment],
                                        executionCounterparty = rowstring[idexecutionCounterparty],
                                        symbolType = rowstring[idsymbolType],
                                        category = rowstring[idcategory],
                                        operationType = rowstring[idoperationType],
                                        orderId = rowstring[idorderId],
                                        orderPos =
                                            rowstring[idorderPos] == ""
                                                ? (long?) null
                                                : Int64.Parse(rowstring[idorderPos]),
                                        price =
                                            rowstring[idprice] == "" ? (double?) null : double.Parse(rowstring[idprice]),
                                        sum = double.Parse(rowstring[idsum]),
                                        who = rowstring[idwho],
                                        tradeNumber =
                                            rowstring[idtradeNumber] == ""
                                                ? (long?) null
                                                : Int64.Parse(rowstring[idtradeNumber]),
                                        symbolId = rowstring[idsymbolId],
                                        valueDate =
                                            rowstring[idvalueDate] == ""
                                                ? (DateTime?) null
                                                : DateTime.Parse(rowstring[idvalueDate]),
                                        timestamp = DateTime.UtcNow,
                                        user = "TradeParser"
                                    });

                                if (index%200 == 0)
                                {
                                    SaveDBChanges(ref db);
                                }
                            }
                        }
                    }

                    TradesParserStatus.Text = "DB updating";
                    SaveDBChanges(ref db);
                    TradesParserStatus.Text = "Done";
                    DateTime TimeFutureParsing = DateTime.Now;
                    db.Dispose();
                    LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                          "FT parsing completed for " + oFilename + "." + index +
                                          " items have been uploaded. Time: " +
                                          (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");
                }
            }
        }

        private void BrockerComboBox_TextChanged(object sender, EventArgs e)
        {
            _currentAcc = BrockerComboBox.Text;
        }

        private void comboBoxEnviroment_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private List<CpTrade> OpenConverting(List<InitialTrade> lInitTrades, string cp)
        {
            DateTime TimeStartConvert = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStartConvert.ToLongTimeString() + ": " + "start " + cp +
                                  " trades Converting");
            var db = new EXANTE_Entities(_currentConnection);

            Dictionary<string, Map> symbolmap = getMapping(cp);
            Dictionary<string, string> typemap =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == cp && ct.Type == "Type"
                 select ct).ToDictionary(k => k.BrockerSymbol, k => k.BOSymbol);

            var lCpTrade = new List<CpTrade>();
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = initTrade.Type;
                if (typemap.ContainsKey(initTrade.Type)) type = typemap[initTrade.Type];
                if (initTrade.Comment != null && initTrade.Comment.Contains("REPO")) type = "REPO";
                double? Price = initTrade.Price;
                double? Qty = initTrade.Qty;
                double? value = initTrade.value;
                DateTime? ValueDate = initTrade.ValueDate;
                String BOSymbol = null;
                if (symbolmap.ContainsKey(initTrade.Symbol + type))
                {
                    Map map = symbolmap[initTrade.Symbol + type];
                    BOSymbol = map.BOSymbol;
                    Price = Price*map.MtyPrice;
                    Qty = Qty*map.MtyVolume;
                    value = value*map.Leverage;
                    if (type != "FX") ValueDate = map.ValueDate;
                    type = map.Type;
                }
                if ((Qty > 0) && (value != null)) value = -Math.Abs((double) value);
                double? fee = null;
                if (initTrade.Fee != null) fee = -Math.Abs((double) initTrade.Fee);
                lCpTrade.Add(new CpTrade
                    {
                        ReportDate = initTrade.ReportDate,
                        TradeDate = initTrade.TradeDate,
                        BrokerId = initTrade.BrokerId,
                        Symbol = initTrade.Symbol,
                        Type = type,
                        Qty = Qty,
                        Price = Price,
                        ValueDate = ValueDate,
                        cp_id = initTrade.cp_id,
                        ExchangeFees = initTrade.ExchangeFees,
                        Fee = fee,
                        BOSymbol = BOSymbol,
                        //?BOTradeNumber = 
                        value = value,
                        Timestamp = DateTime.UtcNow,
                        valid = 1,
                        username = "script",
                        //?BOcp = 
                        exchangeOrderId = initTrade.exchangeOrderId,
                        //  TypeOfTrade = initTrade.Comment.Contains("REPO")?"REPO": initTrade.TypeOfTrade,
                        TypeOfTrade = initTrade.TypeOfTrade,
                        Comment = initTrade.Comment,
                        ExchFeeCcy = initTrade.ExchFeeCcy,
                        ClearingFeeCcy = initTrade.ClearingFeeCcy,
                        ccy = initTrade.ccy,
                        Fee2 = initTrade.Fee2,
                        Fee3 = initTrade.Fee3,
                        Interest = initTrade.AccruedInterest,
                        account = initTrade.Account
                    }
                    );
            }

            db.Dispose();
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + cp + " trades converting completed." +
                                  (TimeEnd - TimeStartConvert).ToString());
            return lCpTrade;
        }

        private List<CpTrade> CFHConverting(List<InitialTrade> lInitTrades)
        {
            DateTime TimeStartConvert = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStartConvert.ToLongTimeString() + ": " + "start CFH trades Converting");
            var db = new EXANTE_Entities(_currentConnection);

            Dictionary<string, Map> symbolmap = getMapping("CFH");
            Dictionary<string, string> typemap =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "CFH" && ct.Type == "Type"
                 select ct).ToDictionary(k => k.BrockerSymbol, k => k.BOSymbol);
            var lCpTrade = new List<CpTrade>();
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = "ST";
                if (typemap.ContainsKey(initTrade.Type)) type = typemap[initTrade.Type];
                double? Price = initTrade.Price;
                double? Qty = initTrade.Qty;
                double? value = initTrade.value;
                DateTime? ValueDate = initTrade.ValueDate;
                String BOSymbol = null;
                if (symbolmap.ContainsKey(initTrade.Symbol))
                {
                    Map map = symbolmap[initTrade.Symbol];
                    BOSymbol = map.BOSymbol;
                    Price = Price*map.MtyPrice;
                    Qty = Qty*map.MtyVolume;
                    value = value*map.Leverage;
                    ValueDate = map.ValueDate;
                    type = map.Type;
                }
                lCpTrade.Add(new CpTrade
                    {
                        ReportDate = initTrade.ReportDate,
                        TradeDate = initTrade.TradeDate,
                        BrokerId = initTrade.BrokerId,
                        Symbol = initTrade.Symbol,
                        Type = type,
                        Qty = Qty,
                        Price = Price,
                        ValueDate = ValueDate,
                        cp_id = initTrade.cp_id,
                        ExchangeFees = initTrade.ExchangeFees,
                        Fee = initTrade.Fee,
                        BOSymbol = BOSymbol,
                        //?BOTradeNumber = 
                        value = value,
                        Timestamp = DateTime.UtcNow,
                        valid = 1,
                        username = "script",
                        //?BOcp = 
                        exchangeOrderId = initTrade.exchangeOrderId,
                        //  TypeOfTrade = initTrade.Comment.Contains("REPO")?"REPO": initTrade.TypeOfTrade,
                        TypeOfTrade = initTrade.TypeOfTrade,
                        Comment = initTrade.Comment,
                        ExchFeeCcy = initTrade.ExchFeeCcy,
                        ClearingFeeCcy = initTrade.ClearingFeeCcy,
                        ccy = initTrade.ccy,
                        Fee2 = initTrade.Fee2,
                        Fee3 = initTrade.Fee3,
                    }
                    );
            }

            db.Dispose();
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "CFH trades converting completed." +
                                  (TimeEnd - TimeStartConvert).ToString());
            return lCpTrade;
        }


        private void OSL_Click(object sender, EventArgs e)
        {
            FORTSReconciliation("OPEN", null,true);
        }

        private void FORTSReconciliation(string cp, string identify,bool maltaentity)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                List<InitialTrade> lInitTrades = OpenParsing(cp, identify);
                List<CpTrade> lCptrades = OpenConverting(lInitTrades, cp);
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.ReportDate = reportdate.Date;
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping(cp);
                //var symbolmap = getMap("OPEN");
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where
                                                        cptrade.valid == 1 &&
                                                        (cptrade.BrokerId == cp) &&
                                                        // || cptrade.BrokerId == "MOEX-SPECTRA") &&
                                                        cptrade.ReportDate >= reportdate.Date &&
                                                        cptrade.ReportDate < (nextdate.Date) &&
                                                        cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.Comment != null && cpTrade.Comment.Contains("REPO"))
                    {
                        int type = 1;
                    }


                    if (cpTrade.BOSymbol == null)
                    {
                        if (symbolmap.ContainsKey(cpTrade.Symbol + cpTrade.Type))
                        {
                            Map map = symbolmap[cpTrade.Symbol + cpTrade.Type];
                            cpTrade.BOSymbol = map.BOSymbol;
                            cpTrade.Price = cpTrade.Price*map.MtyPrice;
                            cpTrade.Qty = cpTrade.Qty*map.MtyVolume;
                            cpTrade.value = cpTrade.value*map.Leverage;
                            if (contractdetails.ContainsKey(map.BOSymbol))
                            {
                                cpTrade.ValueDate = contractdetails[map.BOSymbol].ValueDate;
                            }
                            else
                            {
                                cpTrade.ValueDate = map.ValueDate;
                            }
                            db.CpTrades.Attach(cpTrade);
                            db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                        }
                        else
                        {
                            string symbol = cpTrade.Symbol;
                            if (symbol.Contains("A ") && (cpTrade.Type != "REPO")) //indetify option
                            {
                                cpTrade.Type = "OP";
                                string keysymbol = symbol.Substring(0, symbol.IndexOf("-")) + "OP";
                                Map map;
                                if (symbolmap.TryGetValue(keysymbol, out map))
                                {
                                    int startindex = symbol.IndexOf("M", symbol.IndexOf("-"));
                                    int endindex = symbol.IndexOf(" ", startindex);
                                    cpTrade.ValueDate =
                                        DateTime.ParseExact(
                                            symbol.Substring(startindex + 1, endindex - 2 - (startindex + 1)), "ddMMyy",
                                            CultureInfo.CurrentCulture);
                                    int strikeindex = symbol.IndexOf("A ");
                                    string bosymbol = map.BOSymbol + ".";
                                    if (map.Round == 1)
                                    {
                                        bosymbol = bosymbol + cpTrade.ValueDate.Value.Day.ToString();
                                    }
                                    bosymbol = bosymbol + getLetterOfMonth(cpTrade.ValueDate.Value.Month) +
                                               cpTrade.ValueDate.Value.Year.ToString() + ".";
                                    cpTrade.BOSymbol = bosymbol + symbol.Substring(strikeindex - 1, 1) +
                                                       symbol.Substring(strikeindex + 2);
                                }
                            }
                            else
                            {
                                if ((symbol.Contains("17PA")||(symbol.Contains("17CA"))) && (cpTrade.Type != "REPO")) //indetify option
                                {
                                    cpTrade.Type = "OP";
                                    string keysymbol = symbol.Substring(0, symbol.IndexOf("-")) + "OP";
                                    Map map;
                                    if (symbolmap.TryGetValue(keysymbol, out map))
                                    {
                                        int startindex = symbol.IndexOf("M", symbol.IndexOf("-"));
                                        int endindex =-1;
                                        if (symbol.Contains("17PA"))
                                        {
                                            endindex = symbol.IndexOf("17PA", startindex)+4;
                                        }
                                        else
                                        {
                                            endindex = symbol.IndexOf("17CA", startindex)+4;
                                        }
                                         cpTrade.ValueDate =
                                            DateTime.ParseExact(
                                                symbol.Substring(startindex + 1, endindex - 2 - (startindex + 1)), "ddMMyy",
                                                CultureInfo.CurrentCulture);
                                         int strikeindex = endindex-2;
                                        string bosymbol = map.BOSymbol + ".";
                                        if (map.Round == 1)
                                        {
                                            bosymbol = bosymbol + cpTrade.ValueDate.Value.Day.ToString();
                                        }
                                        bosymbol = bosymbol + getLetterOfMonth(cpTrade.ValueDate.Value.Month) +
                                                   cpTrade.ValueDate.Value.Year.ToString() + ".";
                                        cpTrade.BOSymbol = bosymbol + symbol.Substring(strikeindex, 1) +
                                                           symbol.Substring(strikeindex + 2);
                                    }
                                }
                            }
                        }
                    }
                }
                SaveDBChanges(ref db);
            }

            RecProcess(reportdate, cp, maltaentity);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private List<InitialTrade> OpenParsing(string cp, string identify)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();
            if (result == DialogResult.OK) // Test result.
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start " + cp + " trades uploading");

                var db = new EXANTE_Entities(_currentConnection);
                Dictionary<string, ColumnMapping> cMapping = (from ct in db.ColumnMappings
                                                              where
                                                                  ct.Brocker == cp && ct.FileType == "EXCEL" &&
                                                                  ct.Account == identify
                                                              select ct).ToDictionary(k => k.Type, k => k);
                //if (cMapping["FU"].cTabName == null || CheckTabExist(openFileDialog2.FileName, cMapping["FU"].cTabName))removeOverallRows(openFileDialog2.FileName, cMapping["FU"].cTabName, cMapping["FU"].cLineStart);
                List<InitialTrade> inittrades;
                if (cMapping.ContainsKey("ST") && cMapping["ST"].Brocker != "Renesource")
                {
                    inittrades = ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping["ST"]);
                    if (inittrades != null) lInitTrades.AddRange(inittrades);
                }
                   if (cMapping.ContainsKey("FX"))
                {
                    inittrades = ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping["FX"]);
                    if (inittrades != null) lInitTrades.AddRange(inittrades);
                }
                if (cMapping.ContainsKey("FU"))
                {
                    inittrades = ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping["FU"]);
                    if (inittrades != null)
                    {
                        foreach (InitialTrade initialTrade in inittrades)
                        {
                            initialTrade.ccy = "RUR";
                            if (cp == "OPEN")
                            {
                                initialTrade.Account = "UEX6678";
                            }
                            else
                            {
                                if (cp == "Renesource")
                                {
                                    initialTrade.Account = "RUFO0288";
                                    initialTrade.value = -Math.Sign((long) initialTrade.Qty)*initialTrade.value;
                                    if (initialTrade.Type == "FUT") initialTrade.Type = "FU";
                                    if (initialTrade.Type == "OPT") initialTrade.Type = "OP";
                                }
                                else
                                {
                                    if (cp == "ITInvest")
                                    {
                                        initialTrade.Account = "BC16686-MO-01";
                                        initialTrade.TypeOfTrade = "Trade";
                                    }
                                }
                            }
                        }
                        lInitTrades.AddRange(inittrades);
                    }
                }

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + cp + " trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
                return lInitTrades;
            }
            else return lInitTrades;
        }

        private void removeOverallRows(string fileName, string name, int? startline)
        {
            var ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "",
                                                           false,
                                                           XlPlatform.xlWindows,
                                                           "",
                                                           true, false, 0, true,
                                                           false, false);
            //Выбираетам таблицу(лист).
            Worksheet ObjWorkSheet;
            if (name != null)
            {
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets[name];
            }
            else
            {
                ObjWorkSheet = ObjWorkBook.Worksheets[1];
            }
            Range xlRange = ObjWorkSheet.UsedRange;
            int? i = startline;
            while ((i <= xlRange.Rows.Count) &&
                   !((xlRange.Cells[i, 1].value2 == null) && (xlRange.Cells[i, 3].value2 == null)))
            {
                dynamic t = xlRange.Cells[i, 1].value2;
                if ((xlRange.Cells[i, 1].value2 == null) || (xlRange.Cells[i, 3].value2 == null))
                {
                    xlRange.Rows[i].Delete();
                    i--;
                }
                i++;
            }
            ObjWorkBook.Close();
            ObjExcel.Quit();
            Marshal.FinalReleaseComObject(ObjWorkBook);
            Marshal.FinalReleaseComObject(ObjExcel);
        }

        private List<InitialTrade> CFHParsing()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();
            if (result == DialogResult.OK) // Test result.
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start CFH trades uploading");

                var db = new EXANTE_Entities(_currentConnection);
                Dictionary<string, ColumnMapping> cMapping = (from ct in db.ColumnMappings
                                                              where ct.Brocker == "CFH" && ct.FileType == "EXCEL"
                                                              select ct).ToDictionary(k => k.Type, k => k);
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    //    var startline = getStartRowCFH(oFilename, cMapping["FX"].cTabName);
                    int startline = 2;
                    //if(startline!=-1)lInitTrades.AddRange(ParseBrockerExcelToCpTrade(oFilename, cMapping["FX"], startline));
                    if (startline != -1)
                        lInitTrades.AddRange(ParseBrockerExcelToCpTrade(oFilename, cMapping["ST"], startline));
                }
                foreach (InitialTrade initialTrade in lInitTrades)
                {
                    initialTrade.Type = "FX";
                    initialTrade.Symbol = initialTrade.Symbol + initialTrade.ccy;
                }

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "CFH trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
                return lInitTrades;
            }
            else return lInitTrades;
        }

        private int getStartRowCFH(string fileName, string tabname)
        {
            var ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "",
                                                           false,
                                                           XlPlatform.xlWindows,
                                                           "",
                                                           true, false, 0, true,
                                                           false, false);
            //Выбираетам таблицу(лист).
            Worksheet ObjWorkSheet;
            try
            {
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets[tabname];
            }
            catch (COMException)
            {
                return -1;
            }
            Range xlRange = ObjWorkSheet.UsedRange;
            int i = 3;
            while ((i <= xlRange.Rows.Count) &&
                   ((xlRange.Cells[i, 1].value2 == null) || !(xlRange.Cells[i, 1].value2.ToString() == "Trade Blotter")))
                i++;
            if (i > xlRange.Rows.Count)
            {
                i = 0;
            }
            else
            {
                i = i + 2;
            }


            ObjWorkBook.Close();
            ObjExcel.Quit();
            Marshal.FinalReleaseComObject(ObjWorkBook);
            Marshal.FinalReleaseComObject(ObjExcel);
            return i;
        }

        private bool CheckTabExist(string filename, string tabname)
        {
            var ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(filename, 0, false, 5, "", "",
                                                           false,
                                                           XlPlatform.xlWindows,
                                                           "",
                                                           true, false, 0, true,
                                                           false, false);
            //Выбираетам таблицу(лист).
            Worksheet ObjWorkSheet;
            ObjWorkSheet =
                ObjWorkBook.Worksheets.Cast<Worksheet>().FirstOrDefault(worksheet => worksheet.Name == tabname);
            if (ObjWorkSheet != null)
            {
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                return true;
            }
            else
            {
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                return false;
            }
        }

        private List<InitialTrade> ParseBrockerExcelToCpTrade(string filename, ColumnMapping cMapping, int startline = 0)
        {
            var ObjExcel = new Application();
            //Открываем книгу.                                                                                                                                                        
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(filename, 0, false, 5, "", "",
                                                           false,
                                                           XlPlatform.xlWindows,
                                                           "",
                                                           true, false, 0, true,
                                                           false, false);
            //Выбираетам таблицу(лист).
            Worksheet ObjWorkSheet;
            if (cMapping.cTabName != null)
            {
                ObjWorkSheet =
                    ObjWorkBook.Worksheets.Cast<Worksheet>()
                               .FirstOrDefault(worksheet => worksheet.Name == cMapping.cTabName);
            }
            else
            {
                ObjWorkSheet = ObjWorkBook.Worksheets[1];
                // .Cast<Worksheet>().FirstOrDefault(worksheet => worksheet.Name == cMapping.cTabName)
            }
            if (ObjWorkSheet != null)
            {
                //    ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets[cMapping.cTabName];
                Range xlRange = ObjWorkSheet.UsedRange;
                var tradescounter = new Dictionary<DateTime, int>();
                int i = startline;
                if (startline == 0) i = (int) cMapping.cLineStart;
                var lInitTrades = new List<InitialTrade>();
                int n = xlRange.Rows.Count;
                int add = 0;
                if (i != 1)
                {
                    var curr = (string) xlRange.Cells[i - 1, 12].value2;
                    if ((curr != null) && (curr.IndexOf("Place of Settlement") > -1)) add = 1;
                }
                while ((i <= n) && ((Convert.ToString(xlRange.Cells[i, 1].value2) != "Total")) && (xlRange.Cells[i, 1].value2!=null))
                    //( !(Convert.ToString(xlRange.Cells[i, 1].value2)).Contains("Buy/Sell Back trade")))
                {
                    if (xlRange.Cells[i, cMapping.cTradeDate].value2 != null)
                    {
                        DateTime tradeDate = getDate(cMapping.DateFormat, xlRange.Cells[i, cMapping.cTradeDate].value2);
                        dynamic reportdate = cMapping.cReportDate != null
                                                 ? getDate(cMapping.ReportDateFormat,
                                                           xlRange.Cells[i, cMapping.cReportDate].value2)
                                                 : tradeDate.Date;
                        dynamic valueDate = cMapping.cValuedate != null
                                                ? getDate(cMapping.ValueDateFormat,
                                                          xlRange.Cells[i, cMapping.cValuedate].value2)
                                                : null;
                        if (cMapping.cTradeTime != null)
                        {
                            string crtFormat = "HH:mm:ss";
                            dynamic crtValue = xlRange.Cells[i, cMapping.cTradeTime].value2;
                            if (cMapping.TimeFormat != null)
                            {
                                crtFormat = cMapping.TimeFormat;
                            }
                            if (crtFormat.Length == 6)
                            {
                                dynamic diffdigit = crtFormat.Length - crtValue.ToString().Length;
                                if (diffdigit > 0) crtValue = "0" + crtValue;
                            }
                            dynamic time = DateFromExcelCell(crtValue, crtFormat);
                            //       : DateFromExcelCell(xlRange.Cells[i, cMapping.cTradeTime].value2, "HH:mm:ss");
                            var ts = new TimeSpan(time.Hour, time.Minute, time.Second);
                            tradeDate = tradeDate.Date + ts;
                        }
                        double qty;

                        if (cMapping.cQtySell == null)
                        {
                            qty = xlRange.Cells[i, cMapping.cQty].value2;
                            if (cMapping.cSide != null)
                            {
                                dynamic side = xlRange.Cells[i, cMapping.cSide].value2;
                                if (side != null)
                                {
                                    side = side.ToUpper();
                                    if ((side == "SELL") || (side == "S") || (side.Contains("ПРОДАЖА")))
                                        qty = -Math.Abs(qty);
                                }
                            }
                        }
                        else
                        {
                            double qtybuy = 0;
                            if (xlRange.Cells[i, cMapping.cQty].value2 != null)
                                qtybuy = xlRange.Cells[i, cMapping.cQty].value2;
                            double qtysell = 0;
                            if (xlRange.Cells[i, cMapping.cQtySell].value2 != null)
                                qtysell = xlRange.Cells[i, cMapping.cQtySell].value2;
                            qty = qtybuy - qtysell;
                        }

                        dynamic ReportDate = reportdate;
                        DateTime TradeDate = tradeDate;
                        dynamic BrokerId =
                            cMapping.cBrokerId != null
                                ? xlRange.Cells[i, cMapping.cBrokerId].value2
                                : cMapping.Brocker;
                        dynamic Symbol = Convert.ToString(xlRange.Cells[i, cMapping.cSymbol].value2);
                        dynamic Type = cMapping.cType != null ? xlRange.Cells[i, cMapping.cType].value2 : cMapping.Type;
                        double Qty = qty;
                        double Price = Math.Round(xlRange.Cells[i, cMapping.cPrice + add].value2, 10);
                        dynamic ValueDate = valueDate;

                        // var t = xlRange.Cells[i, cMapping.cExchangeFees + add].value2;
                      //  double tt = Convert.ToDouble(t);
                        double? ExchangeFees =
                            cMapping.cExchangeFees != null
                                ? Convert.ToDouble(xlRange.Cells[i, cMapping.cExchangeFees + add].value2)
                                : null;
                        double? Fee = cMapping.cFee != null ? xlRange.Cells[i, cMapping.cFee + add].value2 : null;
                        double? Fee2 = cMapping.cFee2 != null ? xlRange.Cells[i, cMapping.cFee2 + add].value2 : null;
                        double? Fee3 = cMapping.cFee3 != null ? xlRange.Cells[i, cMapping.cFee3 + add].value2 : null;
                        dynamic value = cMapping.cValue != null ? xlRange.Cells[i, cMapping.cValue + add].value2 : null;
                        DateTime Timestamp = DateTime.UtcNow;
                        dynamic exchangeOrderId =
                            cMapping.cExchangeOrderId != null
                                ? Convert.ToString(xlRange.Cells[i, cMapping.cExchangeOrderId].value2)
                                : null;
                        dynamic ClearingFeeCcy =
                            cMapping.cClearingFeeCcy != null
                                ? xlRange.Cells[i, cMapping.cClearingFeeCcy + add].value2
                                : null;
                        dynamic ccy = cMapping.cCcy != null ? xlRange.Cells[i, cMapping.cCcy + add].value2 : null;
                        dynamic ExchFeeCcy =
                            cMapping.cExchFeeCcy != null
                                ? xlRange.Cells[i, cMapping.cExchFeeCcy + add].value2
                                : null;
                        dynamic TypeOfTrade =
                            cMapping.cTypeOfTrade != null
                                ? xlRange.Cells[i, cMapping.cTypeOfTrade].value2
                                : null;
                        dynamic Comment = cMapping.cComment != null ? xlRange.Cells[i, cMapping.cComment].value2 : null;
                        double? Strike = cMapping.cStrike != null ? xlRange.Cells[i, cMapping.cStrike].value2 : null;
                        double? AccruedInterest =
                            cMapping.cInterest != null ? xlRange.Cells[i, cMapping.cInterest].value2 : null;
                        dynamic Account =
                            cMapping.cAccount != null ? xlRange.Cells[i, cMapping.cAccount + add].value2 : null;
                        dynamic TradeId =
                            cMapping.cTradeId != null
                                ? Convert.ToString(xlRange.Cells[i, cMapping.cTradeId + add].value2)
                                : null;

                        lInitTrades.Add(new InitialTrade
                            {
                                ReportDate = reportdate,
                                TradeDate = tradeDate,
                                BrokerId =
                                    cMapping.cBrokerId != null
                                        ? xlRange.Cells[i, cMapping.cBrokerId].value2
                                        : cMapping.Brocker,
                                Symbol = Convert.ToString(xlRange.Cells[i, cMapping.cSymbol].value2),
                                Type = cMapping.cType != null ? xlRange.Cells[i, cMapping.cType].value2 : cMapping.Type,
                                Qty = qty,
                                Price = Math.Round(xlRange.Cells[i, cMapping.cPrice + add].value2, 10),
                                ValueDate = valueDate,
                                ExchangeFees =
                                    cMapping.cExchangeFees != null
                                        ? xlRange.Cells[i, cMapping.cExchangeFees + add].value2
                                        : null,
                                Fee = cMapping.cFee != null ? xlRange.Cells[i, cMapping.cFee + add].value2 : null,
                                Fee2 = cMapping.cFee2 != null ? xlRange.Cells[i, cMapping.cFee2 + add].value2 : null,
                                Fee3 = cMapping.cFee3 != null ? xlRange.Cells[i, cMapping.cFee3 + add].value2 : null,
                                value = cMapping.cValue != null ? xlRange.Cells[i, cMapping.cValue + add].value2 : null,
                                Timestamp = DateTime.UtcNow,
                                exchangeOrderId =
                                    cMapping.cExchangeOrderId != null
                                        ? Convert.ToString(xlRange.Cells[i, cMapping.cExchangeOrderId].value2)
                                        : null,
                                ClearingFeeCcy =
                                    cMapping.cClearingFeeCcy != null
                                        ? xlRange.Cells[i, cMapping.cClearingFeeCcy + add].value2
                                        : null,
                                ccy = cMapping.cCcy != null ? xlRange.Cells[i, cMapping.cCcy + add].value2 : null,
                                ExchFeeCcy =
                                    cMapping.cExchFeeCcy != null
                                        ? xlRange.Cells[i, cMapping.cExchFeeCcy + add].value2
                                        : null,
                                TypeOfTrade =
                                    cMapping.cTypeOfTrade != null
                                        ? xlRange.Cells[i, cMapping.cTypeOfTrade].value2
                                        : null,
                                Comment = cMapping.cComment != null ? xlRange.Cells[i, cMapping.cComment].value2 : null,
                                Strike = cMapping.cStrike != null ? xlRange.Cells[i, cMapping.cStrike].value2 : null,
                                AccruedInterest =
                                    cMapping.cInterest != null ? xlRange.Cells[i, cMapping.cInterest].value2 : null,
                                Account =
                                    cMapping.cAccount != null ? xlRange.Cells[i, cMapping.cAccount + add].value2 : null,
                                TradeId =
                                    cMapping.cTradeId != null
                                        ? Convert.ToString(xlRange.Cells[i, cMapping.cTradeId + add].value2)
                                        : null
                            });
                        if (tradescounter.ContainsKey(reportdate))
                        {
                            tradescounter[reportdate] = tradescounter[reportdate] + 1;
                        }
                        else
                        {
                            tradescounter.Add(reportdate, 1);
                        }
                    }
                    i++;
                }
                var db = new EXANTE_Entities(_currentConnection);
                i = 1;
                int batchsize = 300;
                DateTime TimeStartInternal = DateTime.Now;
                foreach (InitialTrade initialTrade in lInitTrades)
                {
                    db.InitialTrades.Add(initialTrade);
                    if (i % batchsize == 0)
                    {
                        SaveDBChanges(ref db);
                        DateTime TimeEndInternal = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + "InitTrade uploading time for " + batchsize.ToString() + " :" + (TimeEndInternal - TimeStartInternal).ToString());
                        TimeStartInternal = DateTime.Now;
                    }
                    i++;
                }
               SaveDBChanges(ref db);
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                LogTextBox.AppendText("\r\nTrades uploaded:");
                foreach (var pair in tradescounter)
                {
                    LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
                }
                //reportdate = tradescounter.FirstOrDefault().Key
                return lInitTrades;
            }
            else
            {
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                return null;
            }
        }

        private dynamic getDate(string format, object rowDate)
        {
            if (format.Length == 8) rowDate = rowDate.ToString();
            DateTime formatDate = DateFromExcelCell(rowDate, format);
            return formatDate;
        }

        private DateTime DateFromExcelCell(object t, string Dateformat)
        {
            if (t.GetType().Name == "String")
            {
                return DateTime.ParseExact(t as string, Dateformat, CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.FromOADate((double) t);
            }
        }

        private void atonrecstartbutton_Click(object sender, EventArgs e)
        {
        }

        private void RjoClick(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start RJO trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("RJO", "CSV", "FU", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "RJO");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "RJO trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("RJO");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                string type = "FU";
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "RJO" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        //cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.BOSymbol = GetSymbolRJO(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage, ref type);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        cpTrade.Type = type;
                        //   cpTrade.value = cpTrade.value*Leverage;
                        cpTrade.ValueDate = valuedate;
                    }
                }
                SaveDBChanges(ref db);
            }
           RecProcess(reportdate, "RJO",true);
        }

        private List<CpTrade> InitTradesConverting(List<InitialTrade> lInitTrades, string cp, bool checkIdflag = false,
                                                   string checkIdCp = "")
        {
            DateTime TimeStartConvert = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStartConvert.ToLongTimeString() + ": " + "start " + cp +
                                  " trades Converting");
            var db = new EXANTE_Entities(_currentConnection);
            Dictionary<string, Map> symbolmap = getMapping(cp);
            var lCpTrade = new List<CpTrade>();
            Dictionary<string, long> checkId = null;
            if (checkIdflag)
            {
                checkId = (from ct in db.CpTrades
                           where ct.TradeDate.ToString().Contains("2016-") && ct.BrokerId == checkIdCp
                           select ct).ToDictionary(k => k.exchangeOrderId, k => k.FullId);
            }
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = "FU";
                if (initTrade.Type == "O") type = "OP";
                double? Price = initTrade.Price;
                double? Qty = initTrade.Qty;
                double? value = initTrade.value;

                DateTime? ValueDate = initTrade.ValueDate;
                if (ValueDate == null) ValueDate = new DateTime(2011, 01, 01);
                String BOSymbol = null;
                string key = initTrade.Symbol + type; // +ValueDate.Value.ToShortDateString();
                if (symbolmap.ContainsKey(key))
                {
                    Map map = symbolmap[key];
                    BOSymbol = map.BOSymbol;
                    Price = Price*map.MtyPrice;
                    Qty = Qty*map.MtyVolume;
                    value = value*map.Leverage;
                    if (type == "OP")
                    {
                        BOSymbol = BOSymbol + ".";
                        if (map.UseDayInTicker == true)
                        {
                            BOSymbol = BOSymbol + initTrade.ValueDate.Value.Day.ToString();
                        }
                        if (map.MtyStrike == null) map.MtyStrike = 1;
                        BOSymbol = BOSymbol + getLetterOfMonth(initTrade.ValueDate.Value.Month) +
                                   initTrade.ValueDate.Value.Year + "." + initTrade.OptionType +
                                   (initTrade.Strike*map.MtyStrike).ToString();
                    }
                    else
                    {
                        if (map.calendar == 1)
                        {
                            BOSymbol = BOSymbol + "." + getLetterOfMonth(initTrade.ValueDate.Value.Month) +
                                       initTrade.ValueDate.Value.Year;
                        }
                    }
                }
                if (checkIdflag)
                {
                    if (!checkId.ContainsKey(initTrade.exchangeOrderId))
                    {
                        lCpTrade.Add(new CpTrade
                            {
                                ReportDate = initTrade.ReportDate,
                                TradeDate = initTrade.TradeDate,
                                BrokerId = initTrade.BrokerId,
                                Symbol = initTrade.Symbol,
                                Type = type,
                                Qty = Qty,
                                Price = Price,
                                ValueDate = ValueDate,
                                cp_id = initTrade.cp_id,
                                ExchangeFees = initTrade.ExchangeFees,
                                Fee = initTrade.Fee,
                                BOSymbol = BOSymbol,
                                value = value,
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "script",
                                exchangeOrderId = initTrade.exchangeOrderId,
                                TypeOfTrade = initTrade.TypeOfTrade,
                                Comment = initTrade.Comment,
                                ExchFeeCcy = initTrade.ExchFeeCcy,
                                ClearingFeeCcy = initTrade.ClearingFeeCcy,
                                ccy = initTrade.ccy,
                                account = initTrade.Account,
                                TradeId = initTrade.TradeId
                            });
                    }
                }
                else
                {
                    lCpTrade.Add(new CpTrade
                        {
                            ReportDate = initTrade.ReportDate,
                            TradeDate = initTrade.TradeDate,
                            BrokerId = initTrade.BrokerId,
                            Symbol = initTrade.Symbol,
                            Type = type,
                            Qty = Qty,
                            Price = Price,
                            ValueDate = ValueDate,
                            cp_id = initTrade.cp_id,
                            ExchangeFees = initTrade.ExchangeFees,
                            Fee = initTrade.Fee,
                            BOSymbol = BOSymbol,
                            value = value,
                            Timestamp = DateTime.UtcNow,
                            valid = 1,
                            username = "script",
                            exchangeOrderId = initTrade.exchangeOrderId,
                            TypeOfTrade = initTrade.TypeOfTrade,
                            Comment = initTrade.Comment,
                            ExchFeeCcy = initTrade.ExchFeeCcy,
                            ClearingFeeCcy = initTrade.ClearingFeeCcy,
                            ccy = initTrade.ccy,
                            account = initTrade.Account,
                            TradeId = initTrade.TradeId
                        });
                }
            }

            db.Dispose();
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + cp + " trades converting completed." +
                                  (TimeEnd - TimeStartConvert).ToString());
            return lCpTrade;
        }

        private List<InitialTrade> TradeParsing(string brocker, string filetype, string mappingtype, string identify)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {
                //   var symbolmap = getMapping("RJO");
                var db = new EXANTE_Entities(_currentConnection);
                Dictionary<string, ColumnMapping> cMapping = (from ct in db.ColumnMappings
                                                              where
                                                                  ct.Brocker == brocker && ct.FileType == filetype &&
                                                                  ct.Account == identify
                                                              // "CSV"
                                                              select ct).ToDictionary(k => k.Type, k => k);
                if (filetype == "CSV")
                {
                    lInitTrades.AddRange(ParseBrockerCsvToCpTrade(openFileDialog2.FileName, cMapping[mappingtype]));
                }
                else
                {
                    lInitTrades.AddRange(ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping[mappingtype]));
                }

                return lInitTrades;
            }
            else return lInitTrades;
        }

        private List<InitialTrade> ParseBrockerCsvToCpTrade(string filename, ColumnMapping cMapping)
        {
            var tradescounter = new Dictionary<DateTime, int>();
            var lInitTrades = new List<InitialTrade>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reader = new StreamReader(openFileDialog2.FileName);
            string lineFromFile;
            IQueryable<Contract> contractrow =
                from ct in db.Contracts
                where ct.valid == 1
                select ct;
            //  var contractdetails = contractrow.ToDictionary(k => k.id, k => k);
            int i = 1;

            while ((i < cMapping.cLineStart) && (!reader.EndOfStream))
            {
                lineFromFile = reader.ReadLine();
                i++;
            }
            while (!reader.EndOfStream)
            {
                lineFromFile = reader.ReadLine();
               if (cMapping.Replacesymbols == "ST")
                {
                    lineFromFile = lineFromFile.Replace("\"", "");
                }
                else
                {
                    lineFromFile = lineFromFile.Replace(cMapping.Replacesymbols, "");
                }
                string[] rowstring = lineFromFile.Split(Convert.ToChar(cMapping.Delimeter));
                DateTime tradeDate = cMapping.cTradeDate != null
                                         ? DateTime.ParseExact(rowstring[(int) cMapping.cTradeDate], cMapping.DateFormat,
                                                               CultureInfo.CurrentCulture)
                                         : new DateTime(2011, 01, 01);

                DateTime reportdate = cMapping.cReportDate != null
                                          ? DateTime.ParseExact(rowstring[(int) cMapping.cReportDate],
                                                                cMapping.ReportDateFormat, CultureInfo.CurrentCulture)
                                          : tradeDate;
                //     var reportdate = DateTime.ParseExact(rowstring[(int)cMapping.cReportDate], cMapping.DateFormat, CultureInfo.CurrentCulture);
                if (cMapping.cTradeTime != null)
                {
                    DateTime time = DateTime.ParseExact(rowstring[(int) cMapping.cTradeTime], "HH:mm:ss",
                                                        CultureInfo.CurrentCulture);
                    var ts = new TimeSpan(time.Hour, time.Minute, time.Second);
                    tradeDate = tradeDate.Date + ts;
                }
                double qty;
                if (cMapping.cQtySell == null)
                {
                    qty = Convert.ToDouble(rowstring[(int) cMapping.cQty]);
                }
                else
                {
                    qty = Convert.ToDouble(rowstring[(int) cMapping.cQty]) -
                          Convert.ToDouble(rowstring[(int) cMapping.cQtySell]);
                }
                if (cMapping.cSide != null)
                {
                    if (rowstring[(int) cMapping.cSide] == "2") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToUpper() == "SELL") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToUpper() == "SLD") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToUpper() == "S") qty = -qty;
                }
                string symbol_id = rowstring[(int) cMapping.cSymbol].TrimEnd();

                double price = 0;
                if (cMapping.cPriceSell == null)
                {
                    price =
                        Math.Round(double.Parse(rowstring[(int) cMapping.cPrice], CultureInfo.InvariantCulture), 7);
                }
                else
                {
                    if (qty < 0)
                    {
                        price =
                            Math.Round(
                                double.Parse(rowstring[(int) cMapping.cPriceSell], CultureInfo.InvariantCulture), 7);
                    }
                    else
                    {
                        price = Math.Round(
                            double.Parse(rowstring[(int) cMapping.cPrice], CultureInfo.InvariantCulture), 7);
                    }
                }
                double? Fee;
                if (cMapping.cFee != null)
                {
                    Fee = double.Parse(rowstring[(int) cMapping.cFee], CultureInfo.InvariantCulture);
                    if (cMapping.cClearingFee != null)
                    {
                        Fee =
                            Math.Round(
                                (double)
                                (Fee +
                                 double.Parse(rowstring[(int) cMapping.cClearingFee], CultureInfo.InvariantCulture)), 2);
                    }
                }
                else
                {
                    if (cMapping.cClearingFee != null)
                    {
                        Fee =
                            Math.Round(
                                double.Parse(rowstring[(int) cMapping.cClearingFee], CultureInfo.InvariantCulture), 2);
                    }
                    else Fee = null;
                }

                double? value;
                if (cMapping.cValue != null)
                {
                    value = Math.Abs(double.Parse(rowstring[(int) cMapping.cValue], CultureInfo.InvariantCulture));
                    if (qty > 0) value = -value;
                }
                else
                {
                    value = -price*qty;
                    if (cMapping.Mty != null)
                    {
                        value = value*double.Parse(rowstring[(int) cMapping.Mty], CultureInfo.InvariantCulture);
                    }
                    value = Math.Round((double) value, 2);
                }
                //? double.Parse(rowstring[(int)cMapping.cValue], CultureInfo.InvariantCulture) * double.Parse(rowstring[(int)cMapping.Mty], CultureInfo.InvariantCulture)
                //: null;
                //   var cp_id = getCPid(rowstring[idcp].Trim(), cpdic);
                /*   if (symbol_id.Contains("PUT") || symbol_id.Contains("CALL"))
                    {
                        typeofInstrument = "OP";
                    }*/

                DateTime ReportDate = reportdate;
                DateTime TradeDate = tradeDate;
                string BrokerId = cMapping.cBrokerId != null ? rowstring[(int) cMapping.cBrokerId] : cMapping.Brocker;
                string Symbol = symbol_id;
                double Qty = qty;
                double Price = price;
                DateTime? ValueDate = cMapping.cValuedate != null
                                          ? DateTime.ParseExact(rowstring[(int) cMapping.cValuedate],
                                                                cMapping.ValueDateFormat,
                                                                CultureInfo.CurrentCulture)
                                          : (DateTime?) null;
                double? ExchangeFees =
                    cMapping.cExchangeFees != null
                        ? double.Parse(rowstring[(int) cMapping.cExchangeFees], CultureInfo.InvariantCulture)
                        : (double?) null;
                double? Fee22 = Fee;
                string TypeOfTrade = cMapping.cTypeOfTrade != null ? rowstring[(int) cMapping.cTypeOfTrade] : null;
                string Type = cMapping.cType != null ? rowstring[(int) cMapping.cType] : cMapping.Type;
                double? value2 = value;
                DateTime Timestamp = DateTime.UtcNow;
                string exchangeOrderId =
                    cMapping.cExchangeOrderId != null
                        ? Convert.ToString(rowstring[(int) cMapping.cExchangeOrderId])
                        : null;
                string Comment = cMapping.cComment != null ? rowstring[(int) cMapping.cComment] : null;
                string ExchFeeCcy =
                    cMapping.cExchFeeCcy != null ? rowstring[(int) cMapping.cExchFeeCcy].TrimEnd() : null;
                string ClearingFeeCcy =
                    cMapping.cClearingFeeCcy != null
                        ? rowstring[(int) cMapping.cClearingFeeCcy].TrimEnd()
                        : null;
                string ccy = cMapping.cCcy != null ? rowstring[(int) cMapping.cCcy].TrimEnd() : null;
                double? Strike =
                    cMapping.cStrike != null
                        ? double.Parse(rowstring[(int) cMapping.cStrike], CultureInfo.InvariantCulture)
                        : (double?) null;
                string OptionType =
                    cMapping.cOptionType != null ? rowstring[(int) cMapping.cOptionType].TrimEnd() : null;
                double? Fee2 =
                    cMapping.cFee2 != null
                        ? double.Parse(rowstring[(int) cMapping.cFee2], CultureInfo.InvariantCulture)
                        : (double?) null;
                double? Fee3 =
                    cMapping.cFee3 != null
                        ? double.Parse(rowstring[(int) cMapping.cFee3], CultureInfo.InvariantCulture)
                        : (double?) null;

                string test = cMapping.cAccount != null
                                  ? rowstring[(int) cMapping.cAccount]
                                  : null;

                lInitTrades.Add(new InitialTrade
                    {
                        ReportDate = reportdate,
                        TradeDate = tradeDate,
                        BrokerId = cMapping.cBrokerId != null ? rowstring[(int) cMapping.cBrokerId] : cMapping.Brocker,
                        Symbol = symbol_id,
                        Qty = qty,
                        Price = price,
                        ValueDate = cMapping.cValuedate != null
                                        ? DateTime.ParseExact(rowstring[(int) cMapping.cValuedate],
                                                              cMapping.ValueDateFormat,
                                                              CultureInfo.CurrentCulture)
                                        : (DateTime?) null,
                        ExchangeFees =
                            cMapping.cExchangeFees != null
                                ? double.Parse(rowstring[(int) cMapping.cExchangeFees], CultureInfo.InvariantCulture)
                                : (double?) null,
                        Fee = Fee,
                        TypeOfTrade = cMapping.cTypeOfTrade != null ? rowstring[(int) cMapping.cTypeOfTrade] : null,
                        Type = cMapping.cType != null ? rowstring[(int) cMapping.cType] : cMapping.Type,
                        value = value,
                        Timestamp = DateTime.UtcNow,
                        exchangeOrderId =
                            cMapping.cExchangeOrderId != null
                                ? Convert.ToString(rowstring[(int) cMapping.cExchangeOrderId])
                                : null,
                        Comment = cMapping.cComment != null ? rowstring[(int) cMapping.cComment] : null,
                        ExchFeeCcy =
                            cMapping.cExchFeeCcy != null ? rowstring[(int) cMapping.cExchFeeCcy].TrimEnd() : null,
                        ClearingFeeCcy =
                            cMapping.cClearingFeeCcy != null
                                ? rowstring[(int) cMapping.cClearingFeeCcy].TrimEnd()
                                : null,
                        ccy = cMapping.cCcy != null ? rowstring[(int) cMapping.cCcy].TrimEnd() : null,
                        Strike =
                            cMapping.cStrike != null
                                ? double.Parse(rowstring[(int) cMapping.cStrike], CultureInfo.InvariantCulture)
                                : (double?) null,
                        OptionType =
                            cMapping.cOptionType != null ? rowstring[(int) cMapping.cOptionType].TrimEnd() : null,
                        Fee2 =
                            cMapping.cFee2 != null
                                ? double.Parse(rowstring[(int) cMapping.cFee2], CultureInfo.InvariantCulture)
                                : (double?) null,
                        Fee3 =
                            cMapping.cFee3 != null
                                ? double.Parse(rowstring[(int) cMapping.cFee3], CultureInfo.InvariantCulture)
                                : (double?) null,
                        Account =
                            cMapping.cAccount != null
                                ? rowstring[(int) cMapping.cAccount]
                                : null,
                        TradeId =
                            cMapping.cTradeId != null ? rowstring[(int) cMapping.cTradeId] : null
                    });
                if (tradescounter.ContainsKey(reportdate))
                {
                    tradescounter[reportdate] = tradescounter[reportdate] + 1;
                }
                else
                {
                    tradescounter.Add(reportdate, 1);
                }
                i++;
            }
        
            foreach (InitialTrade initialTrade in lInitTrades)
            {
                db.InitialTrades.Add(initialTrade);
            }
            SaveDBChanges(ref db);
            db.Dispose();
            LogTextBox.AppendText("\r\nTrades uploaded:");
            foreach (var pair in tradescounter)
            {
                LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
            }
            return lInitTrades;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");
            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 &&
                                       (
                                          ft.brocker == "M&L" ||
                                           ft.brocker == "MOEX" ||
                                           ft.brocker == "INSTANT" || ft.brocker == "EXANTE" ||
                                           ft.brocker == "MOEX-SPECTRA" ||
                                              ft.brocker == "OPEN"
                                       ) &&
                                       ft.Type == "VM" &&
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                       ft.ValueCCY != 0
                                       && ft.Reference == null
                                 group ft by new {ft.account_id, ft.symbol, ft.Type, ft.ccy, ft.counterccy}
                                 into g
                                 select new
                                     {
                                         g.Key.account_id,
                                         g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         type = g.Key.Type,
                                         g.Key.ccy,
                                         g.Key.counterccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            int tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                if (Math.Abs((double) VARIABLE.value) > 0.0099)
                {
                    var p = new FTjson();
                    if (VARIABLE.type == "VM")
                    {
                        p.operationType = "VARIATION MARGIN";
                        p.comment = "VM " + VARIABLE.BOSymbol+ " " + reportdate.ToShortDateString();
                        p.asset = "USD";
                    }
                    else
                    {
                        p.operationType = "VARIATION MARGIN";
                        p.comment = "Additional fees from cp:  " + VARIABLE.BOSymbol + "  for " +
                                    reportdate.ToShortDateString();
                    }
                    p.symbolId = VARIABLE.BOSymbol;
                    p.accountId = VARIABLE.account_id;
                    p.amount = Math.Round((double) VARIABLE.ValueCCY, 2).ToString();
                    p.timestamp = reportdate.ToString("yyyy-MM-dd HH:mm:ss");

                    string requestFTload = JsonConvert.SerializeObject(p);
                    if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                    {
                        LogTextBox.AppendText("\r\n Error in sending Left side VM to BO for : " + VARIABLE.account_id +
                                              " " +
                                              VARIABLE.symbol);
                    }
                    else
                    {
                        //  db.Database.ExecuteSqlCommand("update FT SET Posted= NOW() where fullid=" + VARIABLE.id);
                    }
                    var p2 = new FTjson();
                    p2.operationType = "VARIATION MARGIN";
                    p2.symbolId = VARIABLE.BOSymbol;
                    p2.asset = VARIABLE.ccy;
                    p2.amount = Math.Round((double) VARIABLE.value, 2).ToString();
                    p2.timestamp = reportdate.ToString("yyyy-MM-dd HH:mm:ss");
                    p2.comment = "VM " + VARIABLE.BOSymbol + " " + reportdate.ToShortDateString();
                    p2.accountId = VARIABLE.account_id;
                    requestFTload = JsonConvert.SerializeObject(p2);
                    if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                        //     if (!SendJson(requestFTload, conStr + "TST1149.TST" + "/transaction", token))
                    {
                        LogTextBox.AppendText("\r\n Error in sending Right side VM to BO for : " + VARIABLE.account_id +
                                              " " +
                                              VARIABLE.symbol);
                    }
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradefromDb.Count);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            DialogResult result = openFileDialog2.ShowDialog();
            DateTime reportDate = InputDate.Value;
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime TimeUpdateBalanceStart = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start FT Balance uploading for ");


                    var ObjExcel =
                        new Application();
                    //Открываем книгу.                                                                                                                                                        
                    Workbook ObjWorkBook = ObjExcel.Workbooks.Open(oFilename,
                                                                   0, false, 5, "", "",
                                                                   false,
                                                                   XlPlatform
                                                                       .xlWindows,
                                                                   "",
                                                                   true, false, 0, true,
                                                                   false, false);
                    //Выбираем таблицу(лист).
                    Worksheet ObjWorkSheet;
                    ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Sheet1"];
                    Range xlRange = ObjWorkSheet.UsedRange;
                    IFormatProvider theCultureInfo = new CultureInfo("en-GB", true);
                    int jj = 1;
                    dynamic account = xlRange.Cells[5 + jj, 2].value2.ToString();
                    int idReportDate = 1,
                        idLabel = 2,
                        idPrice = 3,
                        idOpType = 4,
                        idDebit = 5,
                        idCredit = 6;
                    int batchsize = 400;
                    string ccy = "";
                    ccy = xlRange.Cells[8 + jj, 2].value2;
                    LogTextBox.AppendText(ccy);
                    int i = 13 + jj;
                    int index = 0;
                    dynamic tempreportdate = xlRange.Cells[i, idReportDate].value2;
                    if (tempreportdate != null)
                    {
                        reportDate = DateTime.ParseExact(xlRange.Cells[i, idReportDate].value2.ToString(), "dd/MM/yyyy",
                                                         theCultureInfo);
                    }
                    else
                    {
                        reportDate = InputDate.Value.Date;
                    }
                    /* var listtodelete = from ft in db.FT
                                       where ft.ccy == ccy && ft.cp == "ADSS" && reportDate.Date == ft.ReportDate
                                       select ft;
                    db.FT.RemoveRange(listtodelete);
                    db.SaveChanges();*/
                    CleanOldValue(db, ccy, "ADSS", reportDate.Date);

                    while (xlRange.Cells[i, 1].value2 != null)
                    {
                        string type = "";
                        string orderid = "";
                        string label = "";
                        if (xlRange.Cells[i, idOpType].value2 == "Comm.")
                        {
                            type = "Commission";
                        }
                        else
                        {
                            if (xlRange.Cells[i, idOpType].value2 == "Cash")
                            {
                                type = "Cash";
                            }
                            else
                            {
                                label = xlRange.Cells[i, idLabel].value2;
                                type = label.Substring(label.IndexOf('/') + 1, 4);
                                if (type == "ESWP") type = "Swap";
                                if (type == "ADSS" && xlRange.Cells[i, idOpType].value2 == "Trade")
                                {
                                    type = "Trade";
                                }
                                orderid = label.Substring(label.IndexOf('/') + 1);
                            }
                        }
                        //  reportDate = DateTime.ParseExact(xlRange.Cells[i, idReportDate].value2.ToString(), "dd/MM/yyyy",
                        //                                   theCultureInfo);
                        /*    var t = xlRange.Cells[i, idCredit].Text.ToString();
                            t = xlRange.Cells[i, idCredit].value2 != null ? Convert.ToDouble(xlRange.Cells[i, idCredit].Text.ToString().Replace(" ", "")) : 0;
                            var t3 = xlRange.Cells[i, idDebit].Text.ToString();
                            t3=t3.Replace(" ", "");
                            var t2 = xlRange.Cells[i, idDebit].value2 != null ? Convert.ToDouble(xlRange.Cells[i, idDebit].Text.ToString().Replace(" ", "")) : 0;
                            t = t - t2;*/
                        
                        db.FT.Add(new FT
                            {
                                ReportDate = reportDate.Date,
                                cp = "ADSS",
                                account_id = account,
                                ccy = ccy,
                                Type = "FT",
                                symbol = type,
                                value =
                                    (xlRange.Cells[i, idCredit].value2 != null
                                         ? Convert.ToDouble(xlRange.Cells[i, idCredit].Text.ToString().Replace(" ", ""))
                                         : 0) -
                                    (xlRange.Cells[i, idDebit].value2 != null
                                         ? Convert.ToDouble(xlRange.Cells[i, idDebit].Text.ToString().Replace(" ", ""))
                                         : 0),
                                Comment = label + ";" + xlRange.Cells[i, idPrice].value2,
                                timestamp = DateTime.UtcNow,
                                valid = 1,
                                User = "script",
                                orderId = orderid
                            });
                        i++;
                        if (i%batchsize == 0)
                        {
                            SaveDBChanges(ref db);
                        }
                        index++;
                    }
                    SaveDBChanges(ref db);
                    dynamic OpenCash = Convert.ToDouble(xlRange.Cells[10 + jj, 2].value2);
                    dynamic CloseCash = Convert.ToDouble(xlRange.Cells[i + 1, 2].value2);
                    double? OpenCashFromDb = GetCloseCashFromPrevDate(db, ccy, "ADSS");
                    string comment = "";
                    if (Math.Abs(OpenCash - OpenCashFromDb) > 0.01)
                    {
                        LogTextBox.AppendText("\r\n" + "Inccorect open cash for " + ccy + " " +
                                              reportDate.ToShortDateString());
                        comment = "Discrepancy in open cash and close cash of previous day";
                    }
                    var movements = (from ft in db.FT
                                     where ft.ccy == ccy && ft.cp == "ADSS" && reportDate.Date == ft.ReportDate
                                     group ft by new {ft.symbol}
                                     into g
                                     select new
                                         {
                                             type = g.Key.symbol,
                                             Sum = g.Sum(t => t.value)
                                         }).ToList();
                    double sum = 0;
                    double sumswap = 0;
                    double sumtrade = 0;
                    double sumfee = 0;
                    double sumcash = 0;
                    foreach (var movement in movements)
                    {
                        sum = sum + movement.Sum.Value;
                        switch (movement.type)
                        {
                            case "Swap":
                                sumswap = movement.Sum.Value;
                                break;
                            case "Trade":
                                sumtrade = movement.Sum.Value;
                                break;
                            case "Commission":
                                sumfee = movement.Sum.Value;
                                break;
                            case "Cash":
                                sumcash = movement.Sum.Value;
                                break;
                        }
                    }
                    if (Math.Abs(CloseCash - OpenCash - sum) > 0.01)
                    {
                        LogTextBox.AppendText("\r\n" + "Inccorect difference between open and close cash for " + ccy +
                                              " " +
                                              reportDate.ToShortDateString());
                        comment = comment + ";Inccorect difference between open and close cash";
                    }

                    IQueryable<ADSSCashGroupped> todelete = from ft in db.ADSSCashGroupped
                                                            where
                                                                ft.Currency == ccy && reportDate.Date == ft.ReportDate &&
                                                                ft.Cp == "ADSS"
                                                            select ft;
                    db.ADSSCashGroupped.RemoveRange(todelete);
                    SaveDBChanges(ref db);

                    db.ADSSCashGroupped.Add(new ADSSCashGroupped
                        {
                            ClosingCash = Math.Round(CloseCash, 2),
                            Commission = Math.Round(sumfee, 2),
                            Currency = ccy,
                            Deposit = Math.Round(sumcash, 2),
                            OpeningCash = OpenCash,
                            ReportDate = reportDate.Date,
                            SWAPs = Math.Round(sumswap, 2),
                            Trades = Math.Round(sumtrade, 2),
                            comment = comment,
                            Cp = "ADSS"
                        });
                    SaveDBChanges(ref db);


                    DateTime TimeFutureParsing = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + ": " +
                                          "FT parsing completed for " + ccy + ":" + oFilename + "." + "\r\n" + index +
                                          " items have been uploaded. Time: " +
                                          (TimeFutureParsing - TimeUpdateBalanceStart).ToString() + "s");
                    ObjWorkBook.Close();
                    ObjExcel.Quit();
                    Marshal.FinalReleaseComObject(ObjWorkBook);
                    Marshal.FinalReleaseComObject(ObjExcel);
                }
            }
            AddCcyFromPreviousReports(db, "ADSS");

            SaveDBChanges(ref db);
            db.Dispose();
        }

        private static void SaveDBChanges(ref EXANTE_Entities db)
        {
            try
            {
                db.SaveChangesAsync();
            }
            catch (DbEntityValidationException er)
            {
                foreach (DbEntityValidationResult eve in er.EntityValidationErrors)
                {
                    Console.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                          ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        private static double? GetCloseCashFromPrevDate(EXANTE_Entities db, string ccy, string cp)
        {
            List<double?> OpenCashFromDb = (from ft in db.ADSSCashGroupped
                                            where ft.Currency == ccy && ft.Cp == cp
                                            orderby ft.ReportDate descending
                                            select ft.ClosingCash).ToList();
            if (OpenCashFromDb.Count > 0)
            {
                return OpenCashFromDb[0];
            }
            else
            {
                return 0;
            }
        }

        private static void AddCcyFromPreviousReports(EXANTE_Entities db, string cp)
        {
            DateTime reportDate = (from ft in db.ADSSCashGroupped
                                   where ft.Cp == cp
                                   orderby ft.ReportDate descending
                                   select ft.ReportDate).ToList()[0];

            DateTime prevreportDate = (from ft in db.ADSSCashGroupped
                                       where ft.ReportDate < reportDate.Date && ft.Cp == cp
                                       orderby ft.ReportDate descending
                                       select ft.ReportDate).ToList()[0];

            List<string> listCcyReportdate = (from ft in db.ADSSCashGroupped
                                              where ft.ReportDate == reportDate.Date && ft.Cp == cp
                                              select ft.Currency).ToList();
            List<ADSSCashGroupped> PreviousReport = (from ft in db.ADSSCashGroupped
                                                     where ft.ReportDate == prevreportDate.Date && ft.Cp == cp
                                                     select ft).ToList();
            foreach (ADSSCashGroupped adssCashGroupped in PreviousReport)
            {
                if (!listCcyReportdate.Any(a => a == adssCashGroupped.Currency))
                {
                    db.ADSSCashGroupped.Add(new ADSSCashGroupped
                        {
                            ClosingCash = Math.Round(adssCashGroupped.ClosingCash.Value, 2),
                            Commission = 0,
                            Currency = adssCashGroupped.Currency,
                            Deposit = 0,
                            Cp = cp,
                            OpeningCash = adssCashGroupped.ClosingCash.Value,
                            ReportDate = reportDate.Date,
                            SWAPs = 0,
                            Trades = 0,
                            comment = "Copied from " + prevreportDate.ToShortDateString()
                        });
                }
            }
            SaveDBChanges(ref db);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            DialogResult result = openFileDialog2.ShowDialog();
            DateTime reportDate = InputDate.Value;
            if (result == DialogResult.OK)
            {
                DateTime TimeUpdateBalanceStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start MAC Balance uploading for ");
                int idccy = 4,
                    idCashGroup = 2,
                    idType = 3,
                    idValue = 5;
                string ccy = "";
                LogTextBox.AppendText(ccy);
                var reader = new StreamReader(openFileDialog2.FileName);
                var filedata = new Dictionary<string, List<string[]>>();
                while (!reader.EndOfStream)
                {
                    string lineFile = reader.ReadLine();
                    string[] splitstring = lineFile.Replace("\"", "").Split(CSVDelimeter);
                    ccy = splitstring[idccy].TrimEnd();
                    if (ccy == "")
                    {
                        if ((splitstring[idCashGroup].TrimEnd().Contains("Nett USD")) ||
                            (splitstring[idType].TrimEnd().Contains("Nett USD"))) ccy = "NetUSD";
                    }
                    if (filedata.ContainsKey(ccy))
                    {
                        filedata[ccy].Add(splitstring);
                    }
                    else
                    {
                        filedata.Add(ccy, new List<string[]> {splitstring});
                    }
                }
                CleanOldValue(db, ccy, "Mac", reportDate.Date);

                foreach (var pair in filedata)
                {
                    double CloseBalance = 0;
                    double ExcessShortage = 0;
                    double sumfees = 0;
                    double sumtrades = 0;
                    double sumoptions = 0;
                    double sumdeposit = 0;
                    double openBalance = 0;
                    double sumInterest = 0;
                    double nlv = 0;
                    string comment = "";
                    foreach (var item in pair.Value)
                    {
                        //   var account = item[idaccount];
                        string CashGroup = item[idCashGroup].Trim();
                        double value = double.Parse(item[idValue], CultureInfo.InvariantCulture);
                        string type = item[idType].Trim();
                        if (CashGroup == "")
                        {
                            //      type = type.Replace(" ", String.Empty);
                            if (type.Contains("Excess Shortage"))
                            {
                                ExcessShortage = ExcessShortage + value;
                            }
                            else
                            {
                                if (type.Contains("NLV"))
                                {
                                    nlv = nlv + value;
                                }
                                else
                                {
                                    if (type.Contains("Option premiums"))
                                    {
                                        sumoptions = sumoptions + value;
                                    }
                                    else
                                    {
                                        if (type.Contains("Settlements"))
                                        {
                                            sumtrades = sumtrades + value;
                                        }
                                        else
                                        {
                                            if (type.Contains("Commissions and fees"))
                                            {
                                                sumfees = sumfees + value;
                                            }
                                            else
                                            {
                                                if (type.Contains("Cash journals"))
                                                {
                                                    sumdeposit = sumdeposit + value;
                                                }
                                                else
                                                {
                                                    if (type.Contains("Interest positings"))
                                                    {
                                                        sumInterest = sumInterest + value;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (CashGroup.Contains("Opening Balance"))
                            {
                                openBalance = openBalance + value;
                            }
                            else
                            {
                                if (CashGroup.Contains("Closing Balance"))
                                {
                                    CloseBalance = CloseBalance + value;
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                        }
                    }
                    //     if (pair.Key=="")ccy = "NetUSD";
                    IQueryable<ADSSCashGroupped> todelete = from ft in db.ADSSCashGroupped
                                                            where
                                                                ft.Currency == pair.Key &&
                                                                reportDate.Date == ft.ReportDate &&
                                                                ft.Cp == "Mac"
                                                            select ft;

                    db.ADSSCashGroupped.RemoveRange(todelete);
                    SaveDBChanges(ref db);
                    double? prevclose = GetCloseCashFromPrevDate(db, pair.Key, "Mac");
                    double closebalance =
                        Math.Round((double) (prevclose + sumfees + sumtrades + sumoptions + sumdeposit + sumInterest), 2);
                    if (Math.Abs(Math.Round((CloseBalance - closebalance), 2)) > 0.01)
                    {
                        comment = comment + ";" + "Discrepancy in close cash.In File:" + CloseBalance.ToString();
                    }

                    db.ADSSCashGroupped.Add(new ADSSCashGroupped
                        {
                            ClosingCash = closebalance,
                            Commission = Math.Round(sumfees, 2),
                            Currency = pair.Key,
                            Deposit = Math.Round(sumdeposit, 2),
                            OpeningCash = prevclose,
                            ReportDate = reportDate.Date,
                            Trades = Math.Round(sumtrades, 2),
                            comment = comment,
                            Cp = "Mac",
                            OptionPremium = sumoptions,
                            timestamp = DateTime.UtcNow,
                            ExcessShortage = ExcessShortage,
                            Interest = sumInterest,
                            NAV = (nlv == 0)
                                      ? (double?) null
                                      : nlv,
                        });
                    SaveDBChanges(ref db);
                }

                /*db.FT.Add(new FT
                            {
                                ReportDate = reportDate.Date,
                                cp = "Mac",
                                account_id = "Mac",
                                ccy = ccy,
                                Type = "FT",
                                symbol = type,
                                value = value,
                                Comment = "",
                                timestamp = DateTime.UtcNow,
                                valid = 1,
                                User = "script"
                            });
                  //     SaveDBChanges(ref db);             }
                         }
                     }
                    while (!reader.EndOfStream)
                     {
                         lineFromFile = reader.ReadLine();
                         rowstring = lineFromFile.Replace("\"", "").Split(CSVDelimeter);
                         CashGroup = rowstring[idCashGroup].TrimEnd();

                         if (CashGroup.Contains("Opening Balance"))
                         {
                             if (ccy == "")
                             {
                                 if (CashGroup.Contains("Nett USD")) ccy = "NetUSD";
                             }
                             CloseBalance = value;
                             prevclose = GetCloseCashFromPrevDate(db, ccy, account);
                             openBalance = CloseBalance - sumfees - sumtrades - sumoptions - sumdeposit;
                             if (Math.Abs((double) (prevclose - openBalance)) > 0.01)
                             {
                                     comment = comment + ";" + "Discrepancy in open cash and close cash of previous day";
                             }
                                 var todelete = from ft in db.ADSSCashGroupped
                                                where
                                                    ft.Currency == ccy && reportDate.Date == ft.ReportDate &&
                                                    ft.Cp == account
                                                select ft;
                             db.ADSSCashGroupped.RemoveRange(todelete);
                             SaveDBChanges(ref db);

                             db.ADSSCashGroupped.Add(new ADSSCashGroupped
                                     {
                                         ClosingCash = Math.Round(CloseBalance, 2),
                                         Commission = Math.Round(sumfees, 2),
                                         Currency = ccy,
                                         Deposit = Math.Round(sumdeposit, 2),
                                         OpeningCash = openBalance,
                                         ReportDate = reportDate.Date,
                                         Trades = Math.Round(sumtrades, 2),
                                         comment = comment,
                                         Cp = account,
                                         OptionPremium = sumoptions,
                                         timestamp = DateTime.UtcNow,
                                         ExcessShortage = ExcessShortage,
                                         NAV = (nlv == 0)
                                                   ? (double?) null
                                                   : nlv,
                                     });
                             SaveDBChanges(ref db);
                             CleanOldValue(db, ccy, account, reportDate.Date);
                             CloseBalance = 0;
                             ExcessShortage = 0;
                             sumfees = 0;
                             sumtrades = 0;
                             sumoptions = 0;
                             sumdeposit = 0;
                             nlv = 0;
                             comment = "";
                             openBalance = 0;
                         }
                         else
                         {
                             account = rowstring[idaccount].TrimEnd();
                             ccy = rowstring[idccy].TrimEnd();
                             type = rowstring[idType].Trim();
                             if ((CashGroup == "") || (type.Contains("Nett")))
                             {
                                 if (type.Contains("Excess Shortage"))
                                 {
                                     ExcessShortage = value;
                                 }
                                 else
                                 {
                                     if (type.Contains("NLV"))
                                     {
                                         nlv = value;
                                     }
                                     else
                                     {
                                         if (type.Contains("Option premiums"))
                                         {
                                             sumoptions = sumoptions + value;
                                         }
                                         else
                                         {
                                             if (type.Contains("Settlements"))
                                             {
                                                 sumtrades = sumtrades + value;
                                             }
                                             else
                                             {
                                                 if (type.Contains("Commissions and fees"))
                                                 {
                                                     sumfees = sumfees + value;
                                                 }
                                                 else
                                                 {
                                                     if (type.Contains("Cash journals"))
                                                     {
                                                         sumdeposit = sumdeposit + value;
                                                     }
                                                 }
                                             }
                                         }
                                         if (ccy != "")
                                         {
                                             db.FT.Add(new FT
                                             {
                                                 ReportDate = reportDate.Date,
                                                 cp = "Mac",
                                                 account_id = account,
                                                         ccy = ccy,
                                                         Type = "FT",
                                                         symbol = type,
                                                         value = value,
                                                         Comment = "",
                                                         timestamp = DateTime.UtcNow,
                                                         valid = 1,
                                                         User = "script"
                                             });
                                             SaveDBChanges(ref db);
                                         }
                                     }
                                 }
                             }
                         }
                     } */
            }
        }


        private static void CleanOldValue(EXANTE_Entities db, string ccy, string account, DateTime reportDate)
        {
            IQueryable<FT> listtodelete = from ft in db.FT
                                          where ft.ccy == ccy && ft.cp == account && reportDate.Date == ft.ReportDate
                                          select ft;
            db.FT.RemoveRange(listtodelete);
            SaveDBChanges(ref db);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //   var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");

            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 && ft.brocker == "OPEN" &&
                                       ft.Type == "AF" &&
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                       ft.ValueCCY != 0
                                       && ft.Reference == null
                                 group ft by new {ft.account_id, ft.symbol, ft.ccy}
                                 into g
                                 select new
                                     {
                                         g.Key.account_id,
                                         g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         g.Key.ccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            int tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                var p = new FTjson();
                p.operationType = "COMMISSION";
                p.comment = "Additional fees from cp:  " + VARIABLE.BOSymbol + "  for " + reportdate.ToShortDateString();
                p.asset = VARIABLE.ccy;
                p.symbolId = VARIABLE.BOSymbol;
                //               p.asset = VARIABLE.counterccy;
                p.accountId = VARIABLE.account_id;
                p.amount = Math.Round((double) VARIABLE.value, 2).ToString();
                p.timestamp = reportdate.ToString("yyyy-MM-dd HH:mm:ss");

                string requestFTload = JsonConvert.SerializeObject(p);
                if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                    //    if (!SendJson(requestFTload, conStr + "TST1149.TST" + "/transaction", token))
                    //      if (!SendJson(requestFTload, conStr + "ZAM1452.001" + "/transaction", token))
                {
                    LogTextBox.AppendText("\r\n Error in sending Left side VM to BO for : " + VARIABLE.account_id + " " +
                                          VARIABLE.symbol);
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradefromDb.Count);
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
        }

        private void GetOslBalance(object sender, EventArgs e)
        {
            GetRowBalance();
        }

        private void GetRowBalance()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start OPEN Balance uploading");

                var db = new EXANTE_Entities(_currentConnection);
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                               0, false,
                                                               5, "", "",
                                                               false,
                                                               XlPlatform
                                                                   .xlWindows,
                                                               "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
                Worksheet ObjWorkSheet;
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
                Range xlRange = ObjWorkSheet.UsedRange;
                string account = xlRange.Cells[11, 8].value2;
                if (account == null) account = xlRange.Cells[12, 7].value2;
                dynamic ccy = xlRange.Cells[14, 8].value2;
                if (ccy == null) ccy = xlRange.Cells[15, 7].value2;
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Cash flow"];
                xlRange = ObjWorkSheet.UsedRange;
                var reportdate = (DateTime) DateFromExcelCell(xlRange.Cells[3, 1].value2, "dd.MM.yyyy");
                RemoveRecordFromRowBalance(db, reportdate, "Open", account);

                GetCashFlowOSL(ObjWorkBook, db, account, reportdate);
                GetPortfolioOSL(ObjWorkBook, db, reportdate, account, ccy);
                GetOSLBalanceData("Gross amount of non-settled trades", ObjWorkBook, ref xlRange, db, ccy, reportdate,
                                  account);
                GetOSLBalanceData("Planned brokerage commission", ObjWorkBook, ref xlRange, db, ccy, reportdate, account);
                GetOSLBalanceData("Other planned fees", ObjWorkBook, ref xlRange, db, ccy, reportdate, account);
                PutNAVOSL(ObjWorkBook, ref xlRange, db, ccy, reportdate, account);


                // ObjectParameter qty = new ObjectParameter("Name", typeof(Int16));
                /*    var idParam = new SqlParameter {ParameterName = "cp",Value = "OPEN"};
                     var CountParam = new SqlParameter { ParameterName = "number", Value = 0, Direction = ParameterDirection.Output };
                     mSqlCmdInsertCustomers.Parameters.Clear();
      mSqlCmdInsertCustomers.Parameters.AddWithValue("param1", "value1");
      mSqlCmdInsertCustomers.Parameters.AddWithValue("param2", "value2");
      .
      .
      .
      mSqlCmdInsertCustomers.Parameters.AddWithValue("paramN", "valueN");*/

                //var t= db.
                // var results = db.Database.SqlQuery<int>("exec CheckMappingBalance @cp, @number out", idParam, CountParam);
                //     db.call

                // remove comments var results = db.Database.ExecuteSqlCommand("exec CheckMappingBalance @cp, @number out", idParam, CountParam);

                //var person = results;
                // remove comments     var votes = (int)CountParam.Value;

                /*
                                var date = new SqlParameter("@date", _msg.MDate);
                                var subject = new SqlParameter("@subject", _msg.MSubject);
                                var body = new SqlParameter("@body", _msg.MBody);
                                var fid = new SqlParameter("@fid", _msg.FID);
                                this.Database.ExecuteSqlCommand("exec messageinsert @Date , @Subject , @Body , @Fid", date, subject, body, fid);

                                */
                //  db.Database.SqlQuery<int>("CheckMappingBalance", name).SingleOrDefault();

                //to get this to work, you will need to change your select inside dbo.insert_department to include name in the resultset
                //var department = db.Database.SqlQuery<Department>("dbo.insert_department @name", name).SingleOrDefault();

                //context.GetDepartmentName(, name);
                //  Console.WriteLine(name.Value);


                SaveDBChanges(ref db);
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "OPEN Balance Completed for+" +
                                      reportdate.ToShortDateString() + openFileDialog2.FileName);
            }
        }

        private static void RemoveRecordFromRowBalance(EXANTE_Entities db, DateTime reportdate, string cp,
                                                       string account)
        {
            IQueryable<RowBalance> todelete = from ft in db.RowBalance
                                              where
                                                  ft.cp == cp && ft.ReportDate == reportdate.Date &&
                                                  ft.account == account
                                              select ft;
            db.RowBalance.RemoveRange(todelete);
            SaveDBChanges(ref db);
        }

        private static void RemoveRecordFromRowBalanceCcy(EXANTE_Entities db, DateTime reportdate, string cp, string ccy)
        {
            IQueryable<RowBalance> todelete = from ft in db.RowBalance
                                              where ft.cp == cp && ft.ReportDate == reportdate.Date && ft.ccy == ccy
                                              select ft;
            db.RowBalance.RemoveRange(todelete);
            SaveDBChanges(ref db);
        }

        private static void GetPortfolioOSL(Workbook ObjWorkBook, EXANTE_Entities db, DateTime reportdate,
                                            dynamic account,
                                            dynamic ccy)
        {
            Range xlRange;
            Worksheet ObjWorkSheet;
            //  ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Securities"];
            ObjWorkSheet =
                ObjWorkBook.Worksheets.Cast<Worksheet>().FirstOrDefault(worksheet => worksheet.Name == "Securities");
            if (ObjWorkSheet != null)
            {
                xlRange = ObjWorkSheet.UsedRange;
                int add = 0;
                var curr = (string) xlRange.Cells[2, 5].value2;
                if (curr.IndexOf("Place of keeping") > -1) add = 1;
                //Open balance
                int i = 4;
                while ((xlRange.Cells[i, 6 + add].value2 != null) & ((xlRange.Cells[i, 6 + add].value2 != "")))
                {
                    db.RowBalance.Add(new RowBalance
                        {
                            ccy = xlRange.Cells[i, 6 + add].value2,
                            cp = "OPEN",
                            Type = "Securities",
                            Value = xlRange.Cells[i, 18 + add].value2,
                            Timestamp = DateTime.UtcNow,
                            ReportDate = reportdate,
                            Exchange = xlRange.Cells[i, 5 + add].value2,
                            Comment = "Qty:" + xlRange.Cells[i, 17 + add].value2,
                            account = account
                        });
                    i++;
                }
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = ccy,
                        cp = "OPEN",
                        Type = "TotalSecurities",
                        Value = Convert.ToDouble(xlRange.Cells[i, 19 + add].value2),
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        Comment = "Planned portfolio value",
                        account = account
                    });
            }
        }

        private void GetCashFlowOSL(Workbook ObjWorkBook, EXANTE_Entities db, dynamic account, DateTime reportdate)
        {
            int i = 3;
            Worksheet ObjWorkSheet;
            Range xlRange;
            ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Cash flow"];
            xlRange = ObjWorkSheet.UsedRange;
            while (xlRange.Cells[i, 1].value2 != null)
            {
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = xlRange.Cells[i, 2].value2,
                        cp = "OPEN",
                        Type = "Open Balance",
                        Value = xlRange.Cells[i, 5].value2,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
                i++;
            }
            while (xlRange.Cells[i, 1].value2 == null)
            {
                i++;
            }
            i++;
            while (xlRange.Cells[i, 1].value2 != null)
            {
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = xlRange.Cells[i, 4].value2,
                        cp = "OPEN",
                        Type = xlRange.Cells[i, 2].value2,
                        Value = xlRange.Cells[i, 5].value2,
                        Exchange = xlRange.Cells[i, 3].value2,
                        Comment = xlRange.Cells[i, 6].value2,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
                i++;
            }
            while (xlRange.Cells[i, 1].value2 == null)
            {
                i++;
            }
            if (((string) xlRange.Cells[i, 2].value2).IndexOf("Closing balance in report currency") > -1) i++;
            while (xlRange.Cells[i, 1].value2 != null)
            {
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = xlRange.Cells[i, 2].value2,
                        cp = "OPEN",
                        Type = "Close Balance",
                        Value = xlRange.Cells[i, 5].value2,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
                double? openvalue =
                    db.RowBalance.Local.Where(o => o.Type == "Open Balance" && o.ccy == xlRange.Cells[i, 2].value2)
                      .FirstOrDefault()
                      .Value;
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = xlRange.Cells[i, 2].value2,
                        cp = "OPEN",
                        Type = "Cash Movement",
                        Value = xlRange.Cells[i, 5].value2 - openvalue,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
                i++;
            }
        }


        private static void PutNAVOSL(Workbook ObjWorkBook, ref Range xlRange, EXANTE_Entities db, dynamic ccy,
                                      DateTime reportdate, string account)
        {
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
            xlRange = ObjWorkSheet.UsedRange;
            int i = 15;
            var currsubject = (string) xlRange.Cells[i, 1].value2;
            while ((currsubject == null) || (currsubject.IndexOf("Net liquidation value") == -1))
            {
                i++;
                currsubject = Convert.ToString(xlRange.Cells[i, 1].value2);
            }
            if (currsubject.IndexOf("Net liquidation value") != -1)
            {
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = ccy,
                        cp = "OPEN",
                        Type = "Closing NAV",
                        Value = xlRange.Cells[i, 5].value2,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
                RowBalance prevNav = (from ft in db.RowBalance
                                      where
                                          ft.cp == "Open" && ft.ReportDate < reportdate.Date && ft.account == account &&
                                          ft.Type == "Closing Nav"
                                      select ft).OrderByDescending(o => o.ReportDate).FirstOrDefault();
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = ccy,
                        cp = "OPEN",
                        Type = "Opening NAV",
                        Value = prevNav.Value,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account,
                        Comment = prevNav.ReportDate.Value.ToShortDateString()
                    });
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = ccy,
                        cp = "OPEN",
                        Type = "NAV Movements",
                        Value = xlRange.Cells[i, 5].value2 - prevNav.Value,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
            }
        }

        private static void GetOSLBalanceData(string TypeOfBalance, Workbook ObjWorkBook, ref Range xlRange,
                                              EXANTE_Entities db, dynamic ccy, DateTime reportdate, dynamic account)
        {
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
            xlRange = ObjWorkSheet.UsedRange;
            int i = 15;
            int add = 0;
            dynamic currsubject = Convert.ToString(xlRange.Cells[18, 5].value2);
            if (currsubject.IndexOf("Place of keeping") > -1) add = 1;

            currsubject = (string) xlRange.Cells[i, 1].value2;
            while ((currsubject == null) ||
                   (currsubject.IndexOf("Net liquidation value") == -1) && (currsubject.IndexOf(TypeOfBalance) == -1))
            {
                i++;
                currsubject = Convert.ToString(xlRange.Cells[i, 1].value2);
            }
            if (currsubject.IndexOf(TypeOfBalance) != -1)
            {
                i++;
                while ((xlRange.Cells[i, 4].value2 != null))
                {
                    db.RowBalance.Add(new RowBalance
                        {
                            ccy = xlRange.Cells[i, 4].value2,
                            cp = "OPEN",
                            Type = TypeOfBalance,
                            Value = xlRange.Cells[i, 5 + add].value2,
                            Timestamp = DateTime.UtcNow,
                            ReportDate = reportdate,
                            account = account
                        });
                    i++;
                }
            }
            else
            {
                db.RowBalance.Add(new RowBalance
                    {
                        ccy = ccy,
                        cp = "OPEN",
                        Type = TypeOfBalance,
                        Value = 0,
                        Timestamp = DateTime.UtcNow,
                        ReportDate = reportdate,
                        account = account
                    });
            }
        }

        private void CFHReconciliation(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);

            if (!noparsingCheckbox.Checked)
            {
                List<InitialTrade> lInitTrades = CFHParsing();
                List<CpTrade> lCptrades = OpenConverting(lInitTrades, "CFH");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                db.SaveChanges();
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("CFH");
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "CFH" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        Map map = symbolmap[cpTrade.Symbol];
                        cpTrade.BOSymbol = map.BOSymbol;
                        cpTrade.Price = cpTrade.Price*map.MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*map.MtyVolume;
                        cpTrade.value = cpTrade.value*map.Leverage;
                        if (contractdetails.ContainsKey(map.BOSymbol))
                        {
                            cpTrade.ValueDate = contractdetails[map.BOSymbol].ValueDate;
                        }
                        else
                        {
                            cpTrade.ValueDate = map.ValueDate;
                        }
                        db.CpTrades.Attach(cpTrade);
                        db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }

            RecProcess(reportdate, "CFH",false);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Parsing xls from bloomberg to DB and calculating Qty by accounts   
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Bloomberg uploading");
            BloombergParsing();
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Bloomberg uploading completed." +
                                  (TimeEnd - TimeStart).ToString());
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start CFH Balance uploading");
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    getRowBalance(db, oFilename);
                }
            }

            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "CFH Balance uploading completed." +
                                  (TimeEnd - TimeStart).ToString());
        }

        private void getRowBalance(EXANTE_Entities db, string ofilename)
        {
            IFormatProvider theCultureInfo = new CultureInfo("en-GB", true);
            int startline = 2;
            int idfee = 11;
            int idFeeCcy = 12;
            int idDate = 3;
            int idpnl = 15;
            int idpnlccy = 16;
            int idType = 6;
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start CFH Balance uploading");
            DateTime reportdate = InputDate.Value.Date;
            var ObjExcel = new Application();
            Workbook ObjWorkBook = ObjExcel.Workbooks.Open(ofilename, 0, false, 5, "", "",
                                                           false,
                                                           XlPlatform.xlWindows,
                                                           "", true, false, 0, true,
                                                           false, false);
            Worksheet ObjWorkSheet;
            ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets["Trade Blotter"];
            Range xlRange = ObjWorkSheet.UsedRange;
            int i = startline;
            string type = "";
            string ccy = "USD";
            if (xlRange.Cells[i, idpnlccy] != null) ccy = xlRange.Cells[i, idpnlccy].value2;
            if (xlRange.Cells[i, idDate].value != null)
            {
                reportdate = DateTime.FromOADate(xlRange.Cells[i, idDate].value2);
                // reportdate = DateTime.ParseExact(xlRange.Cells[i, idDate].value2, "dd/MM/yyyy", theCultureInfo);
            }
            while (xlRange.Cells[i, 2].value2 != null)
            {
                if (xlRange.Cells[i, idType].value2.ToString().Contains("Rollover"))
                {
                    type = "Rollover";
                }
                else
                {
                    type = "Trade";
                }
                if (xlRange.Cells[i, idpnl].value2 != 0)
                {
                    db.RowBalance.Add(new RowBalance
                        {
                            ccy = xlRange.Cells[i, idpnlccy].value2,
                            cp = "CFH",
                            Type = type,
                            Value = xlRange.Cells[i, idpnl].value2,
                            Timestamp = DateTime.UtcNow,
                            //ReportDate = DateTime.ParseExact(xlRange.Cells[i, idDate].value2, "dd/MM/yyyy", theCultureInfo),
                            ReportDate = DateTime.FromOADate(xlRange.Cells[i, idDate].value2),
                            Comment = xlRange.Cells[i, 1].value2
                        });
                }
                if (xlRange.Cells[i, idfee].value2 != 0)
                {
                    db.RowBalance.Add(new RowBalance
                        {
                            ccy = xlRange.Cells[i, idFeeCcy].value2,
                            cp = "CFH",
                            Type = "Fee",
                            Value = xlRange.Cells[i, idfee].value2,
                            Timestamp = DateTime.UtcNow,
                            //ReportDate = DateTime.ParseExact(xlRange.Cells[i, idDate].value2, "dd/MM/yyyy", theCultureInfo),
                            ReportDate = DateTime.FromOADate(xlRange.Cells[i, idDate].value2),
                            Comment = xlRange.Cells[i, 1].value2
                        });
                }
                i++;
            }
            RemoveRecordFromRowBalanceCcy(db, reportdate, "CFH", ccy);
            IQueryable<RowBalance> temp = from r in db.RowBalance
                                          where r.cp == "CFH"
                                                && r.ccy.Contains(ccy) && r.Type == "Close balance"
                                          select r;
            double? openbalance = 0;
            if (temp.Count() > 0)
            {
                DateTime lastreportdate = temp.Max(o => o.ReportDate).Value.Date;
                openbalance = (from r in db.RowBalance
                               where
                                   r.cp == "CFH" && r.ccy == ccy && r.ReportDate == lastreportdate.Date &&
                                   r.Type == "Close balance"
                               select r).FirstOrDefault().Value;
            }
            db.RowBalance.Add(new RowBalance
                {
                    ccy = ccy,
                    cp = "CFH",
                    Type = "Open Balance",
                    Value = openbalance,
                    Timestamp = DateTime.UtcNow,
                    ReportDate = reportdate
                });

            double cashmovement =
                db.RowBalance.Local.Where(
                    o => o.ccy == ccy && (o.Type == "Trade" || o.Type == "Rollover" || o.Type == "Fee"))
                  .Sum(o => o.Value)
                  .Value;
            db.RowBalance.Add(new RowBalance
                {
                    ccy = ccy,
                    cp = "CFH",
                    Type = "Cash Movement",
                    Value = cashmovement,
                    Timestamp = DateTime.UtcNow,
                    ReportDate = reportdate
                });
            db.RowBalance.Add(new RowBalance
                {
                    ccy = ccy,
                    cp = "CFH",
                    Type = "Close balance",
                    Value = openbalance + cashmovement,
                    Timestamp = DateTime.UtcNow,
                    ReportDate = reportdate
                });

            SaveDBChanges(ref db);
            ObjWorkBook.Close();
            ObjExcel.Quit();
            Marshal.FinalReleaseComObject(ObjWorkBook);
            Marshal.FinalReleaseComObject(ObjExcel);
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "start OPEN Balance Completed");
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
        }

        private void cpCostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var db = new EXANTE_Entities(_currentConnection);
            /*var ctradeslist = (from r in db.Ctrades
                                where r.BOtradeTimestamp.ToString().Contains("2015-12") && r.valid == 1
                                select r).ToList();*/
            List<cpCost_cTrade> ctradeslist = (from r in db.Ctrades
                                               where r.BOtradeTimestamp.ToString().Contains("2016-10") && r.valid == 1
                                               select new cpCost_cTrade
                                                   {
                                                       symbol_id = r.symbol_id,
                                                       cp_id = r.cp_id,
                                                       account_id = r.account_id,
                                                       fees = r.fees,
                                                       currency = r.currency,
                                                       qty = r.qty,
                                                       tradeNumber = r.tradeNumber
                                                   }).ToList();

            var dCpCost = new Dictionary<string, CpCost>();
            int i = 0;
            List<cpCost_cpTrade> allcptrades = (from cp in db.CpTrades
                                                where
                                                    cp.TradeDate.ToString().Contains("2016-10") && cp.valid == 1 &&
                                                    cp.BOTradeNumber != null
                                                select new cpCost_cpTrade
                                                    {
                                                        Symbol = cp.Symbol,
                                                        BrokerId = cp.BrokerId,
                                                        ccy = cp.ccy,
                                                        ExchFeeCcy = cp.ExchFeeCcy,
                                                        ExchangeFees = cp.ExchangeFees,
                                                        Fee = cp.Fee,
                                                        Fee2 = cp.Fee2,
                                                        Fee3 = cp.Fee3,
                                                        Qty = cp.Qty,
                                                        BOTradeNumber = cp.BOTradeNumber
                                                    }).ToList();
            int n = ctradeslist.Count;

            foreach (cpCost_cTrade ctrade in ctradeslist)
            {
                i++;
                string trnumber = ctrade.tradeNumber.ToString();
                /* if (trnumber == "30123135")
                 {
                     var t = 1;
                 }*/
                IEnumerable<cpCost_cpTrade> cptrades = allcptrades.Where(cp => cp.BOTradeNumber.Contains(trnumber));
                    //.ToList();
                List<cpCost_cpTrade> listcptrades = cptrades.ToList();
                /*(from cp in allcptrades
                                where  cp.BOTradeNumber.Contains(ctrade.tradeNumber.ToString())
                                select cp).ToList();
                    
              /*  allcptrades.Where()
                (from cp in db.CpTrades
                                where cp.TradeDate.ToString().Contains("2015-12") && cp.BOTradeNumber.Contains(ctrade.tradeNumber.ToString()) && cp.valid == 1
                                select cp).ToList();*/
                double ExchFee = 0, cpFee = 0, sumQty = 0;
                cpCost_cpTrade item = null;
                if (listcptrades.Count > 0)
                {
                    foreach (cpCost_cpTrade trade in listcptrades)
                    {
                        if (trade.ExchangeFees != null)
                            ExchFee = Math.Abs(ExchFee) + Math.Abs((double) trade.ExchangeFees);
                        if (trade.Fee != null) cpFee = Math.Abs(cpFee) + Math.Abs((double) trade.Fee);
                        if (trade.Fee2 != null) cpFee = Math.Abs(cpFee) + Math.Abs((double) trade.Fee2);
                        if (trade.Fee3 != null) cpFee = Math.Abs(cpFee) + Math.Abs((double) trade.Fee3);
                        sumQty = sumQty + Math.Abs((double) trade.Qty);
                    }
                    if (sumQty != 0)
                    {
                        ExchFee = -(ExchFee*Math.Abs((double) ctrade.qty)/sumQty);
                        cpFee = -(cpFee*Math.Abs((double) ctrade.qty)/sumQty);
                    }
                    else
                    {
                        ExchFee = -(ExchFee);
                        cpFee = -(cpFee);
                    }
                    item = listcptrades[0];
                }
                string id = ctrade.account_id + ctrade.symbol_id + ctrade.cp_id;
                CpCost ElementCpcost;
                if (dCpCost.TryGetValue(id, out ElementCpcost))
                {
                    ElementCpcost.BOFee = Math.Round((double) (ElementCpcost.BOFee + Math.Abs((double) ctrade.fees)), 2);
                    ElementCpcost.CpFee = Math.Round((double) (ElementCpcost.CpFee + cpFee), 2);
                    ElementCpcost.ExchFee = Math.Round((double) (ElementCpcost.ExchFee + ExchFee), 2);
                    ElementCpcost.SumQty = ElementCpcost.SumQty + sumQty;
                    ElementCpcost.NumberOfTrades = ElementCpcost.NumberOfTrades + listcptrades.Count();
                    if ((item != null) && (ElementCpcost.CPsymbol != null))
                    {
                        ElementCpcost.CP = item.BrokerId;
                        ElementCpcost.CpClearingCCY = item.ccy;
                        ElementCpcost.CpExchCcy = item.ExchFeeCcy;
                        ElementCpcost.CPsymbol = item.Symbol;
                        ElementCpcost.NumberOfTrades = listcptrades.Count();
                    }
                }
                else
                {
                    dCpCost.Add(id, new CpCost
                        {
                            Date = new DateTime(2016, 10, 1),
                            account = ctrade.account_id,
                            BOCcy = ctrade.currency,
                            BOCp = ctrade.cp_id,
                            BOFee = Math.Round(Math.Abs((double) ctrade.fees), 2),
                            BOsymbol = ctrade.symbol_id,
                            CP = item == null ? null : item.BrokerId,
                            CpClearingCCY = item == null ? null : item.ccy,
                            CpExchCcy = item == null ? null : item.ExchFeeCcy,
                            CpFee = Math.Round(cpFee, 2),
                            ExchFee = Math.Round(ExchFee, 2),
                            CPsymbol = item == null ? null : item.Symbol,
                            SumQty = sumQty,
                            NumberOfTrades = item == null ? 0 : listcptrades.Count()
                        });
                }
            }
            foreach (var pair in dCpCost)
            {
                db.CpCost.Add(pair.Value);
                SaveDBChanges(ref db);
            }
            db.Database.ExecuteSqlCommand("UPDATE CpCost SET CpExchCcy = 'RUB' WHERE BOSymbol LIKE '%FORTS%'");
            db.Database.ExecuteSqlCommand("UPDATE CpCost SET CpClearingCCY = 'RUB' WHERE BOSymbol LIKE '%FORTS%'");
            db.Database.ExecuteSqlCommand("UPDATE CpCost SET CpExchCcy = 'RUB' WHERE CpExchCcy='RUR'");
            db.Database.ExecuteSqlCommand("UPDATE CpCost SET CpClearingCCY = 'RUB' WHERE CpClearingCCY='RUR'");
            db.Dispose();

            // SaveDBChanges(ref db);
            db.Dispose();
        }

        private void updateOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<CpTrade>();
            if (result == DialogResult.OK) // Test result.
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start OPEN trades uploading");

                var db = new EXANTE_Entities(_currentConnection);
                Dictionary<string, ColumnMapping> cMapping = (from ct in db.ColumnMappings
                                                              where ct.Brocker == "OPEN" && ct.FileType == "EXCEL"
                                                              select ct).ToDictionary(k => k.Type, k => k);
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                               0, false, 5, "", "",
                                                               false,
                                                               XlPlatform
                                                                   .xlWindows,
                                                               "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
                Worksheet ObjWorkSheet;
                ObjWorkSheet = (Worksheet) ObjWorkBook.Sheets[cMapping["ST"].cTabName];
                Range xlRange = ObjWorkSheet.UsedRange;
                var tradescounter = new Dictionary<DateTime, int>();
                int? i = cMapping["ST"].cLineStart;
                int n = xlRange.Rows.Count;
                int numberofchanges = 0;
                while (i <= n)
                {
                    if (xlRange.Cells[i, cMapping["ST"].cTradeDate].value2 != null)
                    {
                        if ((xlRange.Cells[i, cMapping["ST"].cFee2].value2 != null) ||
                            (xlRange.Cells[i, cMapping["ST"].cFee3].value2 != null))
                        {
                            string currExchorder = xlRange.Cells[i, cMapping["ST"].cExchangeOrderId].value2;
                            CpTrade currcptrade = (from ct in db.CpTrades
                                                   where
                                                       ct.BrokerId == "OPEN" &&
                                                       ct.exchangeOrderId.Contains(currExchorder)
                                                   select ct).FirstOrDefault();
                            if (currcptrade != null)
                            {
                                currcptrade.Fee2 = cMapping["ST"].cFee2 != null
                                                       ? xlRange.Cells[i, cMapping["ST"].cFee2].value2
                                                       : null;
                                currcptrade.Fee3 = cMapping["ST"].cFee3 != null
                                                       ? xlRange.Cells[i, cMapping["ST"].cFee3].value2
                                                       : null;
                                db.CpTrades.Attach(currcptrade);
                                db.Entry(currcptrade).State = (EntityState) System.Data.Entity.EntityState.Modified;
                            }
                            SaveDBChanges(ref db);
                            numberofchanges++;
                        }
                    }
                    i++;
                }
                db.Dispose();
                LogTextBox.AppendText("\r\n Updated trades " + numberofchanges.ToString());
                //***
            }
        }

        private void uploadFTBOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var reportdate = new DateTime(2016, 09, 20);
            var prevdate = new DateTime(2016, 09, 01);
            DateTime TimeStart = DateTime.Now;
            List<Ftbo> ftboitems =
                (from ct in db.Ftboes
                 where
                     ct.botimestamp >= prevdate && ct.botimestamp <= reportdate &&
                     (ct.symbolId == "" || ct.symbolId == null) && ct.tradeNumber != null
                 select ct).ToList();
            int index = 0;
            Dictionary<string, string> ctradeitems =
                (from ct in db.Ctrades
                 where ct.BOtradeTimestamp <= reportdate.Date && ct.BOtradeTimestamp >= prevdate.Date
                 select ct).ToDictionary(k => (k.tradeNumber.ToString() + k.gatewayId), k => k.symbol_id);
            foreach (Ftbo ftbo in ftboitems)
            {
                string symbolid;
                if (ctradeitems.TryGetValue(ftbo.tradeNumber.ToString() + ftbo.gatewayId, out symbolid))
                {
                    ftbo.symbolId = symbolid;
                    db.Ftboes.Attach(ftbo);
                    db.Entry(ftbo).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    index++;
                }
                else
                {
                    LogTextBox.AppendText("\r\n" + "Didn't find trade for this id:" + ftbo.id + " " + ftbo.tradeNumber);
                }
            }
            SaveDBChanges(ref db);
            DateTime TimeFutureParsing = DateTime.Now;
            db.Dispose();
            LogTextBox.AppendText("\r\n" + TimeFutureParsing.ToLongTimeString() + " Updating symbol completed for " +
                                  index + " items. Time: " + (TimeFutureParsing - TimeStart).ToString() + "s");
        }

        private void NissanButtonClick(object sender, EventArgs e)
        {
            //Nissan file parsing and reconciliation
            DateTime reportdate = InputDate.Value; //todo Get report date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start NISSAN trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("NISSAN", "CSV", "FU", "Main");
                //**
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "NISSAN");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "NISSAN trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("NISSAN");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "NISSAN" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        //   cpTrade.value = cpTrade.value*Leverage;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "NISSAN",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac position uploading");

                List<InitialTrade> LInitPos = TradeParsing("Mac", "CSV", "PO", "Main");


                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Mac position uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button12_Click_2(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start IS-PRIME trades uploading");

                List<InitialTrade> LInitTrades = TradeParsing("IS-PRIME", "CSV", "FX", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "IS-PRIME");
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.Type = "FX";
                    cptrade.value = -cptrade.Qty*cptrade.Price;
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "IS-PRIME trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }

            RecProcess(reportdate, "IS-PRIME",true);
            db.Database.ExecuteSqlCommand("UPDATE CpTrades Set value = -Qty*Price WHERE BrokerId LIKE '%is-%'");
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac trades uploading");

                List<InitialTrade> LInitTrades = TradeParsing("MAC_EMIR", "CSV", "FU", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "IS-PRIME");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "IS-PRIME trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }

            RecProcess(reportdate, "IS-PRIME",true);
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void ParseBrockerCsvToEmir(string filename, Dictionary<string, Emir_Mapping> cMapping)
        {
            var tradescounter = new Dictionary<DateTime, int>();
            var lInitTrades = new List<Emir>();
            var db = new EXANTE_Entities(_currentConnection);
            IQueryable<counterparty> cpfromDb = from cp in db.counterparties
                                                select cp;
            Dictionary<string, int> cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reader = new StreamReader(openFileDialog2.FileName);
            string lineFromFile;
            IQueryable<Contract> contractrow =
                from ct in db.Contracts
                where ct.valid == 1
                select ct;
            int i = 1;
            Emir_Mapping parameters = cMapping.First().Value;
            while ((i < parameters.cLineStart) && (!reader.EndOfStream))
            {
                lineFromFile = reader.ReadLine();
                i++;
            }
            while (!reader.EndOfStream)
            {
                lineFromFile = reader.ReadLine();

                string[] rowstring = lineFromFile.Split(Convert.ToChar(parameters.Delimeter));
                DateTime cpValueDate;
                if (rowstring[6].Length == 4)
                {
                    cpValueDate = DateTime.ParseExact(rowstring[6], "yyMM", CultureInfo.CurrentCulture);
                }
                else
                {
                    cpValueDate = DateTime.ParseExact(rowstring[6], "yyyyMMdd", CultureInfo.CurrentCulture);
                }
                string map_id = rowstring[5];
                if (rowstring[7] == "O")
                {
                    map_id = map_id + "OP";
                }
                map_id = map_id + cpValueDate.ToShortDateString();
                Emir_Mapping map = cMapping[map_id];
                var timedifference = new TimeSpan((int) map.TimeDifference, 0, 0);
                string Buy___Sell_Indicator = rowstring[parameters.cBuySell];
                string Instrument_ID_Taxonomy = map.InstrumentIDTaxonomy;
                string Instrument_ID = map.InstrumentID;
                string Instrument_Classification = map.InstrumentClassification;
                string Underlying_Instrument_ID = map.InstrumentType;
                string Notional_Currency_1 = map.NotionalCurrency1;
                string Deliverable_Currency = map.DeliverableCurrency;
                string UTI = rowstring[24] + rowstring[25];
                string MiFID_Transaction_Reference_Number = rowstring[28];
                string Venue_ID = map.VenueId;
                double? Price___Rate = (Convert.ToDouble(rowstring[13]) + Convert.ToDouble(rowstring[12]))*
                                       map.CpMtyPrice;
                string Price_Notation = map.PriceNotation;
                string Price_Multiplier = map.PriceMultiplier.ToString();
                string Notional =
                    (map.CpMtyPrice*map.PriceMultiplier*Convert.ToDouble(rowstring[11])*
                     (Convert.ToDouble(rowstring[12]) + Convert.ToDouble(rowstring[13]))).ToString();
                string Quantity = rowstring[11];
                string Delivery_Type = map.DeliveryType;
                DateTime Execution_Timestamp = Convert.ToDateTime(rowstring[27]) - timedifference;
                DateTime Effective_Date = Convert.ToDateTime(rowstring[0]);
                DateTime? Maturity_Date = map.MaturityDate;
                DateTime Confirmation_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference;
                DateTime Clearing_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference;
                string CCP_ID = parameters.cp;
                string Floating_Rate_Payment_Frequency = map.FloatingRatePaymentFrequency;
                string Floating_Rate_Reset_Frequency = map.FloatingRateResetFrequency;
                string Floating_Rate_Leg_2 = map.FloatingRateLeg2;
                string Currency_2 = map.Currency2;
                string Exchange_Rate_Basis = map.ExchangeRateBasis;
                string Commodity_Base = map.CommodityBase;
                string Commodity_Details = map.CommodityDetails;
                string Put_Call = null;
                string Option_Exercise_Type = null;
                string Strike_Price = null;
                string ForwardExchangeRate = null;
                if (map.ForwardExchangeRateMty != null)
                {
                    ForwardExchangeRate = (map.ForwardExchangeRateMty*Price___Rate).ToString();
                }
                if (map.cPutCall != null)
                {
                    Put_Call = rowstring[(int) map.cPutCall];
                    //  Option_Exercise_Type =map.
                    Strike_Price = Convert.ToDouble(rowstring[(int) map.cStrikePrice]).ToString();
                    ForwardExchangeRate =
                        (Convert.ToDouble(rowstring[(int) map.cStrikePrice])*map.ForwardExchangeRateMty).ToString();
                }

                lInitTrades.Add(new Emir
                    {
                        ReportDate = Effective_Date,
                        cp = map.Brocker,
                        Timestamp = DateTime.Now,
                        Common_Data_Delegated = "N",
                        Reporting_Firm_ID = "635400MMGYK7HLRQGV31",
                        Other_Counterparty_ID = parameters.cp,
                        Other_Counterparty_ID_Type = "L",
                        Reporting_Firm_Country_Code_of_Branch = "MT",
                        Reporting_Firm_Corporate_Sector = "F",
                        Reporting_Firm_Financial_Status = "F",
                        Beneficiary_ID = "635400MMGYK7HLRQGV31",
                        Beneficiary_ID_Type = "L",
                        Trading_Capacity = "P",
                        Buy___Sell_Indicator = rowstring[parameters.cBuySell],
                        Counterparty_EEA_Status = "N",
                        Instrument_ID_Taxonomy = map.InstrumentIDTaxonomy,
                        Instrument_ID = map.InstrumentID,
                        Instrument_Classification = map.InstrumentClassification,
                        Underlying_Instrument_ID = map.UnderlyingInstrumentID,
                        Underlying_Instrument_ID_Type = map.UnderlyingInstrumentIDType,
                        Notional_Currency_1 = map.NotionalCurrency1,
                        Deliverable_Currency = map.DeliverableCurrency,
                        UTI = rowstring[24] + rowstring[25],
                        MiFID_Transaction_Reference_Number = rowstring[28],
                        Venue_ID = map.VenueId,
                        Compression_Exercise = "N",
                        Price___Rate = Price___Rate.ToString(),
                        Price_Notation = map.PriceNotation,
                        Price_Multiplier = map.PriceMultiplier.ToString(),
                        Notional = (map.PriceMultiplier*Convert.ToDouble(rowstring[11])*Price___Rate).ToString(),
                        Quantity = Convert.ToDouble(rowstring[11]).ToString(),
                        Delivery_Type = map.DeliveryType,
                        Execution_Timestamp = Convert.ToDateTime(rowstring[27]) - timedifference,
                        Effective_Date = Convert.ToDateTime(rowstring[0]),
                        Maturity_Date = map.MaturityDate,
                        Confirmation_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference,
                        Confirmation_Type = "E",
                        Clearing_Obligation = "Y",
                        Cleared = "Y",
                        Clearing_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference,
                        CCP_ID = parameters.cp,
                        CCP_ID_Type = "L",
                        Intragroup = "N",
                        Floating_Rate_Payment_Frequency = map.FloatingRatePaymentFrequency,
                        Floating_Rate_Reset_Frequency = map.FloatingRateResetFrequency,
                        Floating_Rate_Leg_2 = map.FloatingRateLeg2,
                        Currency_2 = map.Currency2,
                        Forward_Exchange_Rate = ForwardExchangeRate,
                        Exchange_Rate_Basis = map.ExchangeRateBasis,
                        Commodity_Base = map.CommodityBase,
                        Commodity_Details = map.CommodityDetails,
                        Put___Call = Put_Call,
                        Option_Exercise_Type = map.OptionExerciseType,
                        Strike_Price = Strike_Price,
                        Action_Type = "N",
                        Message_Type = "T",
                        Instrument_Description = map.InstrumentDescription,
                        Fixed_Rate_Leg_1 = map.FixedRateLeg1.ToString(),
                        Fixed_Rate_Day_Count = map.FixedRateDayCount,
                        Fixed_Leg_Payment_Frequency = map.FixedLegPaymentFrequency
                    });
                if (tradescounter.ContainsKey(Effective_Date))
                {
                    tradescounter[Effective_Date] = tradescounter[Effective_Date] + 1;
                }
                else
                {
                    tradescounter.Add(Effective_Date, 1);
                }
            }
            foreach (Emir emir in lInitTrades)
            {
                db.Emir.Add(emir);
            }
            db.SaveChanges();
            db.Dispose();
            LogTextBox.AppendText("\r\nTrades uploaded:");
            foreach (var pair in tradescounter)
            {
                LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac Emir uploading");

                DialogResult result = openFileDialog2.ShowDialog();
                if (result == DialogResult.OK) // Test result.
                {
                    Dictionary<string, Emir_Mapping> cMapping = (from ct in db.Emir_Mapping
                                                                 where ct.Brocker == "Mac" && ct.filetype == "CSV"
                                                                 select ct).ToDictionary(
                                                                     k =>
                                                                     removeNewlineSymbols(k.CpSymbol + k.OptionType +
                                                                                          k.CPValueDate.Value
                                                                                           .ToShortDateString()), k => k);

                    ParseBrockerCsvToEmir(openFileDialog2.FileName, cMapping);
                }
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Emir Mac uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private string removeNewlineSymbols(string s)
        {
            return Regex.Replace(s, @"\t|\n|\r", "");
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //   var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");

            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 && ft.brocker == "OPEN" &&
                                       ft.Type == "AI" &&
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                       ft.ValueCCY != 0
                                       && ft.Reference == null
                                 group ft by new {ft.account_id, ft.symbol, ft.ccy}
                                 into g
                                 select new
                                     {
                                         g.Key.account_id,
                                         g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         g.Key.ccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            int tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                var p = new FTjson();
                p.operationType = "COUPON PAYMENT";
                p.comment = "Accrued interest from cp:  " + VARIABLE.BOSymbol + "  for " +
                            reportdate.ToShortDateString();
                p.asset = VARIABLE.ccy;
                p.symbolId = VARIABLE.BOSymbol;
                //               p.asset = VARIABLE.counterccy;
                p.accountId = VARIABLE.account_id;
                p.amount = Math.Round((double) VARIABLE.value, 2).ToString();
                p.timestamp = reportdate.ToString("yyyy-MM-dd HH:mm:ss");

                string requestFTload = JsonConvert.SerializeObject(p);
                if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                    //    if (!SendJson(requestFTload, conStr + "TST1149.TST" + "/transaction", token))
                    //      if (!SendJson(requestFTload, conStr + "ZAM1452.001" + "/transaction", token))
                {
                    LogTextBox.AppendText("\r\n Error in sending interest to BO for : " + VARIABLE.account_id + " " +
                                          VARIABLE.symbol);
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradefromDb.Count);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);

            Dictionary<string, long> checkId =
                (from ct in db.CpTrades
                 where ct.TradeDate.ToString().Contains("2016-") && ct.BrokerId == "Belarta"
                 select ct).ToDictionary(k => (k.exchangeOrderId.ToString() + (Math.Sign((double) k.Qty)).ToString()),
                                         k => k.FullId);

            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Belarta trades uploading");

                List<InitialTrade> LInitTrades = TradeParsing("Belarta", "EXCEL", "FX", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "Belarta");
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.ReportDate = reportdate;
                    cptrade.ValueDate = cptrade.TradeDate.Value.Date;
                    cptrade.BOcp = "EXANTE";
                    cptrade.Type = "FX";
                    cptrade.Qty = 100000*cptrade.Qty;
                    cptrade.value = -cptrade.Price*cptrade.Qty;
                    string id = cptrade.exchangeOrderId + (Math.Sign((double) cptrade.Qty)).ToString();
                    // db.CpTrades.Add(cptrade);
                    if (!checkId.ContainsKey(id))
                    {
                        db.CpTrades.Add(cptrade);
                    }
                }
                SaveDBChanges(ref db);

                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Belarta trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }

            RecProcess(reportdate, "Belarta",false);
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void button18_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //var strZamTransaction = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ZAM1452.001/transaction";
            //    var strAdsTrade = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ADS1450.002/trade";
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");
            //var token = GetToken("https://authdb.prod.ghcg.com/api/1.0/auth/session", "backoffice");

            DateTime reportdate = InputDate.Value;
            var acc = new BOaccount
                {
                    accountNameCP = null, // "EXANTE",
                    //   BOaccountId = "FQJ5082.001", // "ELC5351.001",UGN6015.001, "FQJ5082.001"
                    //  DBcpName = "Belarta"
                };


            //        var account = "FQJ5082.001";// "ELC5351.001",
            string broker = "Belarta";
            bool sendFee = false;
            //  var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            IQueryable<CpTrade> cptradefromDb = from Cptrade in db.CpTrades
                                                where Cptrade.valid == 1 && Cptrade.BrokerId == broker &&
                                                      Cptrade.ReportDate >= reportdate.Date &&
                                                      Cptrade.ReportDate < (nextdate.Date)
                                                      && Cptrade.ReconAccount == null
                                                select Cptrade;
            List<CpTrade> cptradeitem = cptradefromDb.ToList();
            int tradesqty = 0;

            foreach (CpTrade cpTrade in cptradeitem)
            {
                acc.BOaccountId = cpTrade.account;
                if (cpTrade.ReconAccount == null)
                {
                    tradesqty = BoReconPostTrade(cpTrade, acc, conStr, token, tradesqty);
                    if (sendFee)
                    {
                        BoReconPostFee(cpTrade, conStr, acc, token);
                    }
                }
                SaveDBChanges(ref db);
            }
            if (tradesqty > 0)
            {
                SaveDBChanges(ref db);
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradeitem.Count);
            }
        }

        private void DEXParsing(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();
                string lineFromFile = reader.ReadLine();
                if (lineFromFile != null)
                {
                    while (!reader.EndOfStream &&
                           !lineFromFile.Contains("F U T U R E S / O P T I O N S    C O N F I R M A T I O N S"))
                    {
                        lineFromFile = reader.ReadLine();
                    }
                    if (!reader.EndOfStream)
                    {
                        lineFromFile = reader.ReadLine();
                        lineFromFile = reader.ReadLine();
                        if (lineFromFile.Contains("The following option positions have expired."))
                            lineFromFile = reader.ReadLine();
                        while (!reader.EndOfStream && !lineFromFile.Contains("Recap Of Confirm Activity") &&
                               !lineFromFile.Contains("Total Value in Base Currency") &&
                               !lineFromFile.Contains("F U T U R E S  /  O P T I O N S    O P E N    P O S I T I O N S"))
                        {
                            DateTime tradedate = DateTime.ParseExact(lineFromFile.Substring(0, 8).Replace(" ", "0"),
                                                                     "dd/MM/yy", CultureInfo.CurrentCulture);
                            double qty = OSLExtractQty(lineFromFile);
                            string symbol = lineFromFile.Substring(33, 32).TrimStart().TrimEnd();
                            string OptionType = lineFromFile.Substring(55, 1).Trim();
                            string OptionStrike = lineFromFile.Substring(57, 9).Trim();
                            string ccy = lineFromFile.Substring(94, 3);
                            double price = Convert.ToDouble(lineFromFile.Substring(72, 6).Trim());
                            DateTime valuedate = DateTime.ParseExact(lineFromFile.Substring(33, 5), "MMMyy",
                                                                     CultureInfo.CurrentCulture);
                            string ExchFeeCcy = "";
                            double ExchangeFees = 0;
                            string ClearingFeeCcy = "";
                            double Fee = 0;

                            lineFromFile = reader.ReadLine();
                            string vt = lineFromFile.Substring(2, 1);

                            while (!reader.EndOfStream && !lineFromFile.Contains("COMMISSION") &&
                                   !lineFromFile.Contains("TOTAL FEES") && lineFromFile.Substring(2, 1) != "/" &&
                                   !lineFromFile.Contains(
                                       "F U T U R E S  /  O P T I O N S    O P E N    P O S I T I O N S"))
                            {
                                lineFromFile = reader.ReadLine();
                            }

                            if (lineFromFile.Contains("COMMISSION"))
                            {
                                ExchFeeCcy = lineFromFile.Substring(94, 3).Trim();
                                ExchangeFees = -Convert.ToDouble(lineFromFile.Substring(103, 12).Trim());
                            }
                            lineFromFile = reader.ReadLine();

                            while (!reader.EndOfStream && !lineFromFile.Contains("COMMISSION") &&
                                   !lineFromFile.Contains("TOTAL FEES") && lineFromFile.Substring(2, 1) != "/" &&
                                   !lineFromFile.Contains(
                                       "F U T U R E S  /  O P T I O N S    O P E N    P O S I T I O N S"))
                            {
                                lineFromFile = reader.ReadLine();
                            }

                            if (lineFromFile.Contains("TOTAL FEES"))
                            {
                                ClearingFeeCcy = lineFromFile.Substring(94, 3).Trim();
                                Fee = -Convert.ToDouble(lineFromFile.Substring(103, 12).Trim());
                            }

                            allfromfile.Add(new CpTrade
                                {
                                    ReportDate = InputDate.Value.Date,
                                    account = "DEX2565",
                                    BrokerId = "OPEN",
                                    Symbol = symbol,
                                    Qty = qty,
                                    Price = price,
                                    ccy = ccy,
                                    ValueDate = valuedate,
                                    TradeDate = tradedate,
                                    Type = (OptionType == "") ? "FU" : "OP",
                                    ExchFeeCcy = ExchFeeCcy,
                                    ExchangeFees = ExchangeFees,
                                    ClearingFeeCcy = ClearingFeeCcy,
                                    Fee = Fee,
                                    Timestamp = DateTime.Now,
                                    valid = 1,
                                    username = "script"
                                });
                            if (lineFromFile.Substring(2, 1) != "/" &&
                                !lineFromFile.Contains("F U T U R E S  /  O P T I O N S    O P E N    P O S I T I O N S"))
                            {
                                lineFromFile = reader.ReadLine();
                            }
                        }
                    }
                    foreach (CpTrade cpTrade in allfromfile)
                    {
                        db.CpTrades.Add(cpTrade);
                    }
                    SaveDBChanges(ref db);
                    db.Dispose();
                }
            }
        }

        private static double OSLExtractQty(string lineFromFile)
        {
            string longqty = lineFromFile.Substring(10, 6).Replace(" ", "");
            string shortqty = lineFromFile.Substring(18, 6).Replace(" ", "");
            if (longqty == "")
            {
                return -Convert.ToDouble(shortqty);
            }
            else
            {
                return Convert.ToDouble(longqty);
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            //  var path = "c:/statement_dstm_20160310.pdf";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start RJO Cash uploading");
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {
                var reader = new PdfReader(openFileDialog2.FileName);
                var db = new EXANTE_Entities(_currentConnection);
                List<string> dbccylist = (from ccy in db.RJO_listccy
                                          where ccy.valid == 1
                                          select ccy.Ccy).ToList();
                DateTime reportdate = InputDate.Value;
                int count = reader.NumberOfPages;
                string txt = "";
                string currentaccount = "";
                for (int i = 1; i <= count; i++)
                {
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    currentaccount = getAccountofPage(txt);
                    string[] rows = txt.Split('\n');
                    int i_row = getStartCcy(rows, 1, dbccylist);
                    while ((i_row < rows.Length) && (i_row > 0))
                    {
                        Dictionary<string, int> listofccy = Getlistofccy_modified(rows[i_row], ref dbccylist);
                        i_row++;
                        string cnttxt = rows[i_row].TrimStart();
                        while ((i_row < rows.Length) && (i_row != getStartCcy(rows, i_row, dbccylist)) &&
                               (cnttxt.Substring(0, 3) != "You") && (cnttxt.Substring(0, 3) != "+++"))
                        {
                            int startvaluesindex = cnttxt.IndexOf("  ") + 1;
                            string type = cnttxt.Substring(0, startvaluesindex).TrimStart().TrimEnd();
                            foreach (var valuePair in listofccy)
                            {
                                int countletters = valuePair.Value;
                                if (valuePair.Value > cnttxt.Length) countletters = cnttxt.Length + 1;
                                countletters = countletters - startvaluesindex - 1;
                                string value = cnttxt.Substring(startvaluesindex, countletters).TrimStart().TrimEnd();
                                if (value.Contains("D"))
                                {
                                    value = "-" + value.Substring(0, value.IndexOf("D"));
                                }
                                startvaluesindex = valuePair.Value + 1;
                                db.RowBalance.Add(new RowBalance
                                    {
                                        ccy = valuePair.Key,
                                        cp = "RJO",
                                        Type = type,
                                        Value = Convert.ToDouble(value),
                                        Timestamp = DateTime.UtcNow,
                                        ReportDate = reportdate,
                                        account = currentaccount
                                    });
                            }
                            i_row++;
                            if (rows[i_row].Trim() == "") i_row++;
                            cnttxt = rows[i_row].TrimStart().TrimEnd();
                        }
                        if (i_row < rows.Length)
                        {
                            i_row = getStartCcy(rows, i_row, dbccylist);
                        }
                    }
                }
                SaveDBChanges(ref db);
                db.Dispose();
            }
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "RJO Cash uploading completed." +
                                  (TimeEnd - TimeStart).ToString());
        }

        private static int getStartCcy(string[] rows, int start, List<string> dbccylist)
        {
            int i_row = start;
            bool found = false;
            while (i_row < rows.Count() && !found)
            {
                foreach (string ccy in dbccylist)
                {
                    if (rows[i_row].Contains(ccy)) found = true;
                }
                i_row++;
            }
            if (found)
            {
                i_row--;
            }
            else
            {
                i_row = -1;
            }
            return i_row;
        }


        private static Dictionary<string, int> Getlistofccy_modified(string txt, ref List<string> ccy)
        {
            int lastindexofstar = txt.IndexOf('*');
            var listofccy = new Dictionary<string, int>();
            while (lastindexofstar > -1)
            {
                int endstar = txt.IndexOf("*", lastindexofstar + 1);
                string cnt_ccy = txt.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd();
                listofccy.Add(txt.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd(),
                              endstar + 1);

                string match = ccy.FirstOrDefault(stringToCheck => stringToCheck.Contains(cnt_ccy));
                if (match == null)
                {
                    ccy.Add(cnt_ccy);
                    var db = new EXANTE_Entities(_currentConnection);
                    db.RJO_listccy.Add(new RJO_listccy {Ccy = cnt_ccy, valid = 1});
                    SaveDBChanges(ref db);
                    db.Dispose();
                }
                lastindexofstar = txt.IndexOf("*", endstar + 1);
            }
            return listofccy;
        }

        private static Dictionary<string, int> Getlistofccy(string txt)
        {
            int indexofbeginning = txt.IndexOf("CONVERTED TO USD");
            int indexccy = txt.LastIndexOf("\n", indexofbeginning - 5);
            string ccys = txt.Substring(indexccy);
            ccys = ccys.Substring(0, ccys.IndexOf("\n", 3)).TrimEnd(); // , indexofbeginning - indexccy).TrimEnd();
            int lastindexofstar = ccys.IndexOf('*');
            var listofccy = new Dictionary<string, int>();
            while (lastindexofstar > -1)
            {
                int endstar = ccys.IndexOf("*", lastindexofstar + 1);
                listofccy.Add(ccys.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd(),
                              endstar);
                lastindexofstar = ccys.IndexOf("*", endstar + 1);
            }
            return listofccy;
        }

        private static string getAccountofPage(string txt)
        {
            int indexofaccount = txt.IndexOf("ACCOUNT NUMBER:") + 15;
            int test = txt.IndexOf("\n", indexofaccount);
            if (indexofaccount > 0)
            {
                return
                    txt.Substring(indexofaccount, txt.IndexOf("\n", indexofaccount) - indexofaccount)
                       .TrimStart()
                       .TrimEnd();
            }
            else
            {
                return "";
            }
        }

        private void BelartaClick(object sender, EventArgs e)
        {
            //     DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            //    if (result == DialogResult.OK) // Test result.
            //    {
            var db = new EXANTE_Entities(_currentConnection);
            //    var reader = new StreamReader(openFileDialog2.FileName);
            /*      HtmlWeb web = new HtmlWeb();
                     HtmlAgilityPack.HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
                     HtmlNodeCollection tags = doc.DocumentNode.SelectNodes("//abc//tag");
                */


            var document = new HtmlDocument();
            string htmlString = "c:/test.htm";
            document.LoadHtml(htmlString);
            //   HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//a");
            //Closed Transactions:
            //  var dom = CsQuery.CQ.CreateFromFile(htmlString);


            // var trNodes = document.GetElementbyId("Closed Transactions"); //.ChildNodes.Where(x => x.Name == «tr»);


            HtmlNode a = document.DocumentNode.SelectSingleNode("<b>Closed Transactions:</b>");
            HtmlNode table = document.GetElementbyId("table5");
            IEnumerable<HtmlNode> tableRows = table.ChildNodes
                                                   .Where(cn => cn.NodeType == HtmlNodeType.Element)
                                                   .Skip(2);
            HtmlNodeCollection nodes =
                document.DocumentNode.SelectNodes("//h3[contains(concat(' ', @class, ' '), ' r ')]/a");
            if (nodes != null)
                foreach (HtmlNode node in nodes)
                    Console.WriteLine(node.GetAttributeValue("href", null));

            List<HtmlNode> toftitle =
                document.DocumentNode.Descendants()
                        .Where(
                            x =>
                            (x.Name == "tr" && x.Attributes["class"] != null &&
                             x.Attributes["class"].Value.Contains("block_content")))
                        .ToList();

            /*   HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//h3[contains(concat(' ', @class, ' '), ' r ')]/a");
            if (nodes != null)
                foreach (HtmlNode node in nodes)
                    yield return node.GetAttributeValue("href", null);
*/

            int t = 1;

            /*foreach (HtmlNode link in collection)
                {
                    string target = link.Attributes["href"].Value;
                }*/

            // var currate = 1; // = GetPage(initialstring, "/tr", "</td", list);
            /*             var index = 15;
                     //  while ((index < currate.Count()) && (currate[index][0].IndexOf("Курс основного") == -1)) index++;
                     while ((index < currate.Count()) && (currate[index][0].IndexOf("18.08.2014") == -1)) index++;
                     var temp = "";
                     if (index != currate.Count() + 1)
                     {
                         temp = currate[index][2].Replace(',', '.');
                         temp = temp.Replace("<td>", "");
                         temp = temp.Replace(">", "");
                         temp = temp.Replace(" ", "");

                     }
                HtmlWeb web = new HtmlWeb();
                     HtmlAgilityPack.HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
                     HtmlNodeCollection tags = doc.DocumentNode.SelectNodes("//abc//tag");
                         */
            //   }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            FORTSReconciliation("Renesource", "Main",true);
            var db = new EXANTE_Entities(_currentConnection);
            db.Database.ExecuteSqlCommand(
                "UPDATE CpTrades AS cp INNER JOIN Contracts AS c ON c.id = cp.BOSymbol SET cp.value = - cp.Qty*cp.Price*c.Leverage WHERE cp.BrokerId LIKE '%Rene%' AND ReportDate > '2016-06-01'");
            db.Dispose();
        }

        private void fastmatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();
                string lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                string reportDate = openFileDialog2.FileName.Substring(openFileDialog2.FileName.IndexOf("_") + 1,
                                                                       openFileDialog2.FileName.LastIndexOf("-") -
                                                                       openFileDialog2.FileName.IndexOf("_") - 1);
                int idTradeDate = 13,
                    idSymbol = 4,
                    idQty = 6,
                    idSide = 5,
                    idPrice = 8,
                    idValueDate = 12,
                    idValue = 9;
                IFormatProvider theCultureInfo = new CultureInfo("en-GB", true);
                while (!reader.EndOfStream)
                {
                    lineFromFile = reader.ReadLine().Replace("\"", "");
                    string[] rowstring = lineFromFile.Split(Delimiter);
                    if (rowstring[1] != "")
                    {
                        allfromfile.Add(new CpTrade
                            {
                                ReportDate = Convert.ToDateTime(reportDate),
                                TradeDate = Convert.ToDateTime(rowstring[idTradeDate], theCultureInfo),
                                BrokerId = "ADSSOREX",
                                Symbol = rowstring[idSymbol],
                                Type = "FX",
                                Qty = rowstring[idSide].IndexOf("Buy") == -1
                                          ? Convert.ToDouble(rowstring[idQty].Replace(" ", ""))*(-1)
                                          : Convert.ToDouble(rowstring[idQty].Replace(" ", "")),
                                Price = Convert.ToDouble(rowstring[idPrice].Replace(" ", "")),
                                ValueDate = Convert.ToDateTime(rowstring[idValueDate], theCultureInfo),
                                cp_id = 19,
                                ExchangeFees = null,
                                Fee = null,
                                Id = null,
                                BOSymbol = null,
                                BOTradeNumber = null,
                                value = Convert.ToDouble(rowstring[idValue].Replace(" ", "")),
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "tradesparser",
                                //  FullId = null,
                                BOcp = null,
                                exchangeOrderId = null
                            });
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                }
                db.SaveChanges();
            }
        }

        private void aBNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Создаём приложение.
                TradesParserStatus.Text = "Processing";
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog1.FileName,
                                                               0, false, 5, "", "", false,
                                                               XlPlatform
                                                                   .xlWindows, "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
                Worksheet ObjWorkSheet;
                ObjWorkSheet =
                    (Worksheet) ObjWorkBook.Sheets["Derivative Trades_Деривативы"];
                Range xlRange = ObjWorkSheet.UsedRange;

                int rowCount = xlRange.Rows.Count + 1;
                int colCount = xlRange.Columns.Count;
                DateTime reportdate = DateTime.FromOADate(xlRange.Cells[3, 8].value2);
                // reportdate = reportdate.AddDays(-1);
                var db = new EXANTE_Entities(_currentConnection);
                DateTime nextdate = Fortsnextday.Value.AddDays(1);
                var queryable =
                    from ct in db.Ctrades
                    where ct.Date >= reportdate && ct.Date < (nextdate) && ct.cp_id == "FORTS_TR"
                    select
                        new
                            {
                                ct.ExchangeOrderId,
                                ct.tradeNumber,
                                ct.qty,
                                ct.price,
                                ct.symbol_id,
                                ct.fullid,
                                ct.RecStatus
                            };
                var botrades = new Dictionary<string, List<BOtrade>>();
                int n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    string Ctrade_id = ctrade.ExchangeOrderId.Replace("DC:F:", "");
                    var tempBotrade = new BOtrade
                        {
                            TradeNumber = (long) ctrade.tradeNumber,
                            Qty = (double) ctrade.qty,
                            Price = (double) ctrade.price,
                            symbol = ctrade.symbol_id,
                            ctradeid = ctrade.fullid,
                            RecStatus = ctrade.RecStatus
                        };

                    if (botrades.ContainsKey(Ctrade_id))
                    {
                        botrades[Ctrade_id].Add(tempBotrade);
                    }
                    else botrades.Add(Ctrade_id, new List<BOtrade> {tempBotrade}); //tempBotrade});
                }

                var allfromfile = new List<CpTrade>();
                for (int i = 10; i < rowCount; i++)
                {
                    if (xlRange.Cells[i, 4].value2 != null)
                    {
                        dynamic tradeDate = DateTime.FromOADate(xlRange.Cells[i, 4].value2);
                        if (tradeDate.Date == reportdate.Date)
                        {
                            dynamic time = DateTime.FromOADate(xlRange.Cells[i, 5].value2);
                            var ts = new TimeSpan(time.Hour, time.Minute, time.Second);
                            tradeDate = tradeDate.Date + ts;
                            allfromfile.Add(new CpTrade
                                {
                                    ReportDate = reportdate,
                                    TradeDate = tradeDate,
                                    BrokerId = "Aton",
                                    Symbol = xlRange.Cells[i, 10].value2,
                                    Type = "FUTURES",
                                    Qty = xlRange.Cells[i, 6].value2.IndexOf("Buy") == -1
                                              ? Convert.ToInt64(xlRange.Cells[i, 11].value2)*(-1)
                                              : Convert.ToInt64(xlRange.Cells[i, 11].value2),
                                    Price = xlRange.Cells[i, 12].value2,
                                    ValueDate = null,
                                    cp_id = 2,
                                    ExchangeFees = xlRange.Cells[i, 19].value2 - xlRange.Cells[i, 16].value2,
                                    Fee = 0,
                                    Id = null,
                                    BOSymbol = null,
                                    BOTradeNumber = null,
                                    value = xlRange.Cells[i, 16].value2,
                                    Timestamp = DateTime.UtcNow,
                                    valid = 1,
                                    username = "tradesparser",
                                    //  FullId = null,
                                    BOcp = null,
                                    exchangeOrderId = Convert.ToString(xlRange.Cells[i, 2].value2)
                                });
                        }
                    }
                }

                List<Reconcilation> recon = Reconciliation(allfromfile, botrades, "exchangeOrderId", "2");

                foreach (var botrade in botrades)
                {
                    foreach (BOtrade botradeItemlist in botrade.Value)
                    {
                        if (botradeItemlist.RecStatus)
                        {
                            using (var data = new EXANTE_Entities(_currentConnection))
                            {
                                data.Database.ExecuteSqlCommand(
                                    "UPDATE Ctrades Set RecStatus ={0}  WHERE fullid = {1}", true,
                                    botradeItemlist.ctradeid);
                            }
                        }
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                }
                db.SaveChanges();

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpFull_id = allfromfile[(int) reconitem.CpFull_id].FullId;
                    db.Reconcilations.Add(reconitem);
                }
                db.SaveChanges();
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
                TradesParserStatus.Text = "Done:" + openFileDialog1.FileName;
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            DateTime TimeStart = DateTime.Now;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime reportdate = InputDate.Value;
            LogTextBox.AppendText(TimeStart + ": " + "Updating links for " + reportdate.ToShortDateString());

            DateTime nextdate = reportdate.AddDays(1);
            List<CpTrade> cptradefromDb = (from cptrade in db.CpTrades
                                           where
                                               cptrade.valid == 1 && cptrade.ReportDate >= reportdate.Date &&
                                               cptrade.ReportDate < (nextdate.Date) && cptrade.BOTradeNumber != null
                                           select cptrade).ToList();
            foreach (CpTrade cpTrade in cptradefromDb)
            {
                db.Database.ExecuteSqlCommand("Delete FROM  Reconcilation WHERE CpFull_id =" + cpTrade.FullId.ToString());
            }
            Dictionary<string, long> reclist = (from rec in db.Reconcilations
                                                where rec.Timestamp >= reportdate.Date
                                                select rec).ToDictionary(
                                                    k => (k.CpFull_id.ToString() + ';' + k.BOTradenumber.ToString()),
                                                    k => k.id);
            int i = 0;
            foreach (CpTrade cpTrade in cptradefromDb)
            {
                string[] ctrades = cpTrade.BOTradeNumber.Split(';');
                foreach (string ctrade in ctrades)
                {
                    string key = cpTrade.FullId.ToString() + ';' + ctrade;
                    if (!reclist.ContainsKey(key))
                    {
                        db.Reconcilations.Add(new Reconcilation
                            {
                                CpFull_id = cpTrade.FullId,
                                BOTradenumber = Convert.ToInt64(ctrade),
                                Timestamp = DateTime.UtcNow,
                                valid = 1,
                                username = "script"
                            });
                        SaveDBChanges(ref db);
                        i++;
                    }
                }
            }
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + i.ToString() + " links have added.Time:" +
                                  (TimeEndUpdating - TimeStart).ToString());
        }

        private void button24_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Renesource trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("Renesource", "EXCEL", "ST", "RUEQ0288");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "Renesource");
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.account = "RUEQ0288";
                    if (cptrade.Symbol == "0")
                    {
                        cptrade.Type = "REPO";
                        cptrade.Symbol = cptrade.Comment;
                    }
                    else
                    {
                        cptrade.Type = "ST";
                    }
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "Renesource trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("Renesource");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "Renesource" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "Renesource",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button25_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start IB Belarta trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("BelartaIB", "EXCEL", "ST", "Main");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "BelartaIB", true, "BelartaIB");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "IB Belarta trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("BelartaIB");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "BelartaIB" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "BelartaIB",false);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button26_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Renesource trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("Renesource", "EXCEL", "FX", "GLFO0288");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "Renesource");
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.account = "GLFO288";
                    if (cptrade.Type.Contains("OPTION"))
                    {
                        cptrade.Comment = cptrade.Type;
                        cptrade.Type = "OP";
                        cptrade.Symbol = cptrade.Symbol.TrimEnd();
                    }
                    else
                    {
                        cptrade.Comment = cptrade.Type;
                        cptrade.Type = "ST";
                    }
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "Renesource trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("Renesource");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "Renesource" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime) cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price*MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*MtyVolume;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "Renesource",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button27_Click(object sender, EventArgs e)
        {
            string path = "c:/20160229.txt";
            var reader = new StreamReader(path);
            string lineFromFile = reader.ReadLine();
            DateTime date;
            string type = "";
            int qty = 0;
            if (lineFromFile != null)
            {
                while (!reader.EndOfStream && !lineFromFile.Contains("Y O U R   A C T I V I T Y   T H I S   M O N T H "))
                {
                    lineFromFile = reader.ReadLine();
                }


                if ((!reader.EndOfStream))
                {
                    lineFromFile = reader.ReadLine();
                }

                if ((!reader.EndOfStream) && !lineFromFile.Contains(" * * * * * * * * * *"))
                {
                    lineFromFile = reader.ReadLine();
                }


                while (!reader.EndOfStream && !lineFromFile.Contains("Y O U R   A C T I V I T Y   T H I S   M O N T H "))
                {
                    type = lineFromFile.Substring(9, 2);
                    if (type == "F1")
                    {
                        date = Convert.ToDateTime(lineFromFile.Substring(1, 6) + "201" + lineFromFile.Substring(7, 1));
                        qty = Convert.ToInt32(lineFromFile.Substring(14, 9)) -
                              Convert.ToInt32(lineFromFile.Substring(25, 9));
                    }
                    int t = 1;
                }

                if (!reader.EndOfStream)
                {
                }
            }
            /*   DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {
                var reader = new StreamReader(openFileDialog2.FileName);
                var db = new EXANTE_Entities(_currentConnection);
                var dbccylist = (from ccy in db.RJO_listccy
                                 where ccy.valid == 1
                                 select ccy.Ccy).ToList();
                var reportdate = InputDate.Value;
                var count = reader.NumberOfPages;
                string txt = "";
                string currentaccount = "";
                // var results = new Dictionary<string,List<string>>
                string lineFromFile;
                //  var contractdetails = contractrow.ToDictionary(k => k.id, k => k);
                var i = 1;
                while  (!reader.EndOfStream)
                {
                    lineFromFile = reader.ReadLine();
                    i++;
                }
                while (!reader.EndOfStream)
                {
                    lineFromFile = reader.ReadLine();
                    if (cMapping.Replacesymbols == "ST")
                    {
                        lineFromFile = lineFromFile.Replace("\"", "");
                    }
                    else
                    {
                        lineFromFile = lineFromFile.Replace(cMapping.Replacesymbols, "");
                    }
                    var rowstring = lineFromFile.Split(Convert.ToChar(cMapping.Delimeter));





                for (var i = 1; i <= count; i++)
                {
                    currentaccount = getAccountofPage(txt);
                    var rows = txt.Split('\n');
                    var i_row = getStartCcy(rows, 1, dbccylist);
                    while ((i_row < rows.Length) && (i_row > 0))
                    {
                        var listofccy = Getlistofccy_modified(rows[i_row], ref dbccylist);
                        i_row++;
                        var cnttxt = rows[i_row].TrimStart();
                        while ((i_row < rows.Length) && (i_row != getStartCcy(rows, i_row, dbccylist)) && (cnttxt.Substring(0, 3) != "You") && (cnttxt.Substring(0, 3) != "+++"))
                        {
                            var startvaluesindex = cnttxt.IndexOf("  ") + 1;
                            //var startvaluesindex = listofccy.ElementAt(0).Value;
                            var type = cnttxt.Substring(0, startvaluesindex).TrimStart().TrimEnd();
                            foreach (KeyValuePair<string, int> valuePair in listofccy)
                            {
                                var countletters = valuePair.Value;
                                if (valuePair.Value > cnttxt.Length) countletters = cnttxt.Length + 1;
                                countletters = countletters - startvaluesindex - 1;
                                var value = cnttxt.Substring(startvaluesindex, countletters).TrimStart().TrimEnd();
                                if (value.Contains("D"))
                                {
                                    value = "-" + value.Substring(0, value.IndexOf("D"));
                                }
                                startvaluesindex = valuePair.Value + 1;
                                db.RowBalance.Add(new RowBalance
                                {
                                    ccy = valuePair.Key,
                                    cp = "RJO",
                                    Type = type,
                                    Value = Convert.ToDouble(value),
                                    Timestamp = DateTime.UtcNow,
                                    ReportDate = reportdate,
                                    account = currentaccount
                                });
                            }
                            //  endrowindex = cnttxt.IndexOf("\n");
                            i_row++;
                            if (rows[i_row].Trim() == "") i_row++;
                            cnttxt = rows[i_row].TrimStart().TrimEnd();
                        }
                        if (i_row < rows.Length)
                        {
                            i_row = getStartCcy(rows, i_row, dbccylist);
                        }
                    }
                }
                //  BEGINNING BALANCE 
                var t = 1;
                SaveDBChanges(ref db);
                db.Dispose();
            }*/
        }

        private void cFHReconciliationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                List<InitialTrade> lInitTrades = CFHParsing();
                List<CpTrade> lCptrades = OpenConverting(lInitTrades, "CFH");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("CFH");
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "CFH" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        Map map = symbolmap[cpTrade.Symbol];
                        cpTrade.BOSymbol = map.BOSymbol;
                        cpTrade.Price = cpTrade.Price*map.MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*map.MtyVolume;
                        cpTrade.value = cpTrade.value*map.Leverage;
                        if (contractdetails.ContainsKey(map.BOSymbol))
                        {
                            cpTrade.ValueDate = contractdetails[map.BOSymbol].ValueDate;
                        }
                        else
                        {
                            cpTrade.ValueDate = map.ValueDate;
                        }
                        db.CpTrades.Attach(cpTrade);
                        db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }
            RecProcess(reportdate, "CFH",false);
        }

        private void cFHBalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start CFH Balance uploading");
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    getRowBalance(db, oFilename);
                }
            }
            DateTime TimeEnd = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "CFH Balance uploading completed." +
                                  (TimeEnd - TimeStart).ToString());
        }

        private void vMAtonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText(TimeStart + ": " + "Getting ccy prices from MOEX");
            // var FORTSDate = InputDate.Value.ToString("dd.MM.yyyy");
            string FORTSDate = InputDate.Value.ToString("dd.MM.yyyy");
            //  updateFORTSccyrates(FORTSDate);
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + "CCY FORTS rates for " + FORTSDate +
                                  " uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());

            calcualteVM(InputDate.Value, "ATON");
            DateTime TimeEndVMCalculation = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndVMCalculation + ": " + "VM calculation " + FORTSDate +
                                  " completed. Time:" + (TimeEndVMCalculation - TimeEndUpdating).ToString());
        }

        private void atonReconciliationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Создаём приложение.
                TradesParserStatus.Text = "Processing";
                var ObjExcel = new Application();
                //Открываем книгу.                                                                                                                                                        
                Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog1.FileName,
                                                               0, false, 5, "", "", false,
                                                               XlPlatform
                                                                   .xlWindows, "",
                                                               true, false, 0, true,
                                                               false, false);
                //Выбираем таблицу(лист).
                Worksheet ObjWorkSheet;
                ObjWorkSheet =
                    (Worksheet) ObjWorkBook.Sheets["Derivative Trades_Деривативы"];
                Range xlRange = ObjWorkSheet.UsedRange;

                int rowCount = xlRange.Rows.Count + 1;
                int colCount = xlRange.Columns.Count;
                DateTime reportdate = DateTime.FromOADate(xlRange.Cells[3, 8].value2);
                // reportdate = reportdate.AddDays(-1);
                var db = new EXANTE_Entities(_currentConnection);
                DateTime nextdate = Fortsnextday.Value.AddDays(1);
                var queryable =
                    from ct in db.Ctrades
                    where ct.Date >= reportdate && ct.Date < (nextdate) && ct.cp_id == "FORTS_TR"
                    select
                        new
                            {
                                ct.ExchangeOrderId,
                                ct.tradeNumber,
                                ct.qty,
                                ct.price,
                                ct.symbol_id,
                                ct.fullid,
                                ct.RecStatus
                            };
                var botrades = new Dictionary<string, List<BOtrade>>();
                int n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    string Ctrade_id = ctrade.ExchangeOrderId.Replace("DC:F:", "");
                    var tempBotrade = new BOtrade
                        {
                            TradeNumber = (long) ctrade.tradeNumber,
                            Qty = (double) ctrade.qty,
                            Price = (double) ctrade.price,
                            symbol = ctrade.symbol_id,
                            ctradeid = ctrade.fullid,
                            RecStatus = ctrade.RecStatus
                        };

                    if (botrades.ContainsKey(Ctrade_id))
                    {
                        botrades[Ctrade_id].Add(tempBotrade);
                    }
                    else botrades.Add(Ctrade_id, new List<BOtrade> {tempBotrade}); //tempBotrade});
                }

                var allfromfile = new List<CpTrade>();
                for (int i = 10; i < rowCount; i++)
                {
                    if (xlRange.Cells[i, 4].value2 != null)
                    {
                        dynamic tradeDate = DateTime.FromOADate(xlRange.Cells[i, 4].value2);
                        if (tradeDate.Date == reportdate.Date)
                        {
                            dynamic time = DateTime.FromOADate(xlRange.Cells[i, 5].value2);
                            var ts = new TimeSpan(time.Hour, time.Minute, time.Second);
                            tradeDate = tradeDate.Date + ts;
                            allfromfile.Add(new CpTrade
                                {
                                    ReportDate = reportdate,
                                    TradeDate = tradeDate,
                                    BrokerId = "Aton",
                                    Symbol = xlRange.Cells[i, 10].value2,
                                    Type = "FUTURES",
                                    Qty = xlRange.Cells[i, 6].value2.IndexOf("Buy") == -1
                                              ? Convert.ToInt64(xlRange.Cells[i, 11].value2)*(-1)
                                              : Convert.ToInt64(xlRange.Cells[i, 11].value2),
                                    Price = xlRange.Cells[i, 12].value2,
                                    ValueDate = null,
                                    cp_id = 2,
                                    ExchangeFees = xlRange.Cells[i, 19].value2 - xlRange.Cells[i, 16].value2,
                                    Fee = 0,
                                    Id = null,
                                    BOSymbol = null,
                                    BOTradeNumber = null,
                                    value = xlRange.Cells[i, 16].value2,
                                    Timestamp = DateTime.UtcNow,
                                    valid = 1,
                                    username = "tradesparser",
                                    //  FullId = null,
                                    BOcp = null,
                                    exchangeOrderId = Convert.ToString(xlRange.Cells[i, 2].value2)
                                });
                        }
                    }
                }

                List<Reconcilation> recon = Reconciliation(allfromfile, botrades, "exchangeOrderId", "2");

                foreach (var botrade in botrades)
                {
                    foreach (BOtrade botradeItemlist in botrade.Value)
                    {
                        if (botradeItemlist.RecStatus)
                        {
                            using (var data = new EXANTE_Entities(_currentConnection))
                            {
                                data.Database.ExecuteSqlCommand(
                                    "UPDATE Ctrades Set RecStatus ={0}  WHERE fullid = {1}", true,
                                    botradeItemlist.ctradeid);
                            }
                        }
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                }
                SaveDBChanges(ref db);

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpFull_id = allfromfile[(int) reconitem.CpFull_id].FullId;
                    db.Reconcilations.Add(reconitem);
                }
                SaveDBChanges(ref db);
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                Marshal.FinalReleaseComObject(ObjWorkBook);
                Marshal.FinalReleaseComObject(ObjExcel);
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            FORTSReconciliation("ITInvest", null,false);
            var db = new EXANTE_Entities(_currentConnection);
            db.Database.ExecuteSqlCommand(
                "UPDATE CpTrades AS cp INNER JOIN Contracts AS c ON c.id = cp.BOSymbol SET cp.value = - cp.Qty*cp.Price*c.Leverage WHERE cp.BrokerId LIKE '%ITInvest' AND ReportDate > '2016-06-01'");
            db.Dispose();
        }

        private void button11_Click_2(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                reportdate = AxiPdfParser(reportdate);
            }
            RecProcess(reportdate, "Axi",false);
        }

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


        private DateTime AxiPdfParser(DateTime reportdate)
        {
            DateTime TimeStart = DateTime.Now;
            var db = new EXANTE_Entities(_currentConnection);
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    Dictionary<string, long> checkId = (from ct in db.CpTrades
                                                        where
                                                            ct.TradeDate.ToString().Contains("2017-") &&
                                                            ct.BrokerId == "Axi"
                                                        select ct).ToDictionary(k => k.exchangeOrderId.ToString(),
                                                                                k => k.FullId);
                    var reader = new PdfReader(oFilename);
                    int count = reader.NumberOfPages;
                    string txt = "";
                    int i = 1;
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    int indexDate = txt.IndexOf("Date: ") + 6;
                    int indexDateEnd = txt.IndexOf(" ", indexDate);
                    string tempdate = txt.Substring(indexDate, indexDateEnd - indexDate);
                    if (tempdate.Length < 11) tempdate = "0" + tempdate;
                    //reportdate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    reportdate = InputDate.Value.Date;

                    while (i <= count && !txt.Contains("ROLLOVER TRADE DETAILS") &&
                           !txt.Contains("NEW TRADING ACTIVITY"))
                    {
                        i++;
                        txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    }
                    var dicCpCtrades = new Dictionary<string, List<CpTrade>>();
                    bool flagStop = false;
                    int i_row = 0;
                    string[] rows;
                    string account;
                    string type;

                    if (txt.Contains("ROLLOVER TRADE DETAILS"))
                    {
                        rows = txt.Split('\n');
                        i_row = AxiPdfGetStarRow(rows, "ROLLOVER TRADE DETAILS");
                        if (i_row != -1)
                        {
                            account = rows[i_row - 1];
                            dicCpCtrades = new Dictionary<string, List<CpTrade>>();
                            type = "FX";
                            while ((i < count) && (!flagStop))
                            {
                                txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                                rows = txt.Split('\n');
                                if (i_row != -1)
                                {
                                    while ((i_row < rows.Length) && (!rows[i_row].Contains("Page")) &&
                                           (!rows[i_row].Contains("CASH MOVEMENTS")) &&
                                           (!rows[i_row].Contains("SETTLING TRADE DETAILS")) &&
                                           (!rows[i_row].Contains("NEW TRADING ACTIVITY"))
                                            && (!flagStop))
                                    {
                                        string[] traderow =
                                            rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                        if (traderow.Count() == 1)
                                        {
                                            string tempvalue = traderow[0];
                                            i_row++;
                                            traderow = rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                            Array.Resize(ref traderow, traderow.Length + 1);
                                            traderow[traderow.Length - 1] = tempvalue;
                                            LogTextBox.AppendText("\r\n" + "Pay attention to value: " + tempvalue);
                                            //  traderow.SetValue(tempvalue, traderow.Count());
                                        }
                                        if ((traderow[0] == "SETTLED")||(traderow[1] == "SETTLED"))
                                        {
                                            flagStop = true;
                                        }
                                        else
                                        {
                                            int add = 0;
                                            string tradenumber;
                                            string[] sp = traderow[0].Split('-');
                                            if (traderow[0].Split('-').Count() > 1)
                                            {
                                                add = 1;
                                                tradenumber = rows[i_row + 1].TrimStart();
                                            }
                                            else
                                            {
                                                tradenumber = traderow[0];
                                            }

                                            tempdate = traderow[1 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime tradedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                     CultureInfo.InvariantCulture);
                                            tempdate = traderow[2 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime nearvaluedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                            tempdate = traderow[3 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime farrvaluedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                            double Qty = AxiPdfGetNegativeValue(traderow[9 - add]);
                                            if ((!checkId.ContainsKey(tradenumber + "NearLeg")))
                                            {
                                                dicCpCtrades.Add(tradenumber + "NearLeg", new List<CpTrade>());
                                                dicCpCtrades[tradenumber + "NearLeg"].Add(new CpTrade
                                                    {
                                                        account = account,
                                                        BrokerId = "Axi",
                                                        BOcp = null,
                                                        BOSymbol = null,
                                                        BOTradeNumber = null,
                                                        valid = 1,
                                                        Timestamp = DateTime.UtcNow,
                                                        exchangeOrderId = tradenumber + "NearLeg",
                                                        ccy = traderow[8 - add],
                                                        ReportDate = reportdate,
                                                        TradeDate = tradedate,
                                                        Symbol = traderow[4 - add],
                                                        Type = type,
                                                        TypeOfTrade = "Swap",
                                                        Qty = Qty,
                                                        Price = Convert.ToDouble(traderow[10 - add]),
                                                        ValueDate = nearvaluedate,
                                                        value = AxiPdfGetNegativeValue(traderow[12 - add])
                                                    });
                                            }
                                            if ((!checkId.ContainsKey(tradenumber + "FarLeg")))
                                            {
                                                dicCpCtrades.Add(tradenumber + "FarLeg", new List<CpTrade>());
                                                double farprice = AxiPdfGetNegativeValue(traderow[11 - add]);
                                                if (traderow[11 - add].Contains("("))
                                                {
                                                    farprice = AxiPdfGetNegativeValue(traderow[12 - add]);
                                                    LogTextBox.AppendText("\r\n" + "Pay attention to value: " +
                                                                          farprice.ToString());
                                                }
                                                if ((Math.Abs(Convert.ToDouble(traderow[10 - add])/farprice) > 1.2) ||
                                                    (Math.Abs(Convert.ToDouble(traderow[10 - add])/farprice) < 0.8))
                                                {
                                                    farprice = AxiPdfGetNegativeValue(traderow[12 - add]);
                                                    LogTextBox.AppendText("\r\n" + "Pay attention to value: " +
                                                                          farprice.ToString());
                                                }


                                                dicCpCtrades[tradenumber + "FarLeg"].Add(new CpTrade
                                                    {
                                                        account = account,
                                                        BrokerId = "Axi",
                                                        BOcp = null,
                                                        BOSymbol = null,
                                                        BOTradeNumber = null,
                                                        valid = 1,
                                                        Timestamp = DateTime.UtcNow,
                                                        exchangeOrderId = tradenumber + "FarLeg",
                                                        ccy = traderow[8 - add],
                                                        ReportDate = reportdate,
                                                        TradeDate = tradedate,
                                                        Symbol = traderow[4 - add],
                                                        Type = type,
                                                        TypeOfTrade = "Swap",
                                                        Qty = -Qty,
                                                        Price = farprice,
                                                        ValueDate = farrvaluedate,
                                                        value = Qty*farprice
                                                    });
                                            }
                                            i_row = i_row + add + 1;
                                        }
                                    }
                                    if ((i_row < rows.Length) &&
                                        ((rows[i_row].Contains("CASH MOVEMENTS")) ||
                                         (txt.Contains("NEW TRADING ACTIVITY")) ||
                                         (txt.Contains("SETTLING TRADE DETAILS"))))
                                    {
                                        flagStop = true;
                                    }
                                }
                                i_row = 0;
                                if(!flagStop)i++;
                            }
                        }
                    }
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    while (i < count && !txt.Contains("NEW TRADING ACTIVITY"))
                    {
                        i++;
                        txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    }
                    if (txt.Contains("NEW TRADING ACTIVITY"))
                    {
                        flagStop = false;
                        i_row = 0;
                        rows = txt.Split('\n');
                        i_row = AxiPdfGetStarRow(rows, "NEW TRADING ACTIVITY");
                        if (i_row > -1)
                        {
                            account = rows[i_row - 1];
                        while ((i < count) && (!flagStop))
                        {
                            txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                            rows = txt.Split('\n');
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
                                            type = traderow[1 - add];
                                            tempdate = traderow[3 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime tradedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                     CultureInfo.InvariantCulture);
                                            tempdate = traderow[4 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime valuedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                     CultureInfo.InvariantCulture);
                                            //   if (!dicCpCtrades.ContainsKey(tradenumber) && (type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                            if ((type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                            {
                                                if (!dicCpCtrades.ContainsKey(tradenumber))
                                                {
                                                    dicCpCtrades.Add(tradenumber, new List<CpTrade>());
                                                }
                                                dicCpCtrades[tradenumber].Add(new CpTrade
                                                    {
                                                        account = account,
                                                        BrokerId = "Axi",
                                                        BOcp = null,
                                                        BOSymbol = null,
                                                        BOTradeNumber = null,
                                                        valid = 1,
                                                        Timestamp = DateTime.UtcNow,
                                                        exchangeOrderId = tradenumber,
                                                        ccy = traderow[7 - add],
                                                        ReportDate = reportdate,
                                                        TradeDate = tradedate,
                                                        Symbol = traderow[5 - add],
                                                        Type = traderow[2 - add],
                                                        TypeOfTrade = "Trade",
                                                        Qty = AxiPdfGetNegativeValue(traderow[8 - add]),
                                                        Price = Convert.ToDouble(traderow[9 - add]),
                                                        ValueDate = valuedate,
                                                        value = AxiPdfGetNegativeValue(traderow[10 - add])
                                                    });
                                            }
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
                            i++;
                        }
                    }
                        else flagStop = true;
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
                                     (valuePair.Value[0].Symbol != "RUB/USD") ))
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
                        SaveDBChanges(ref db);
                    }


                    DateTime TimeEnd = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + dicCpCtrades.Count +
                                          " trades Axi uploading completed." +
                                          (TimeEnd - TimeStart).ToString());
                    LogTextBox.AppendText("\r\n" + oFilename);

                    /*  foreach (KeyValuePair<string, CpTrade> keyValuePair in dicCpCtrades)
                      {
                          db.CpTrades.Add(keyValuePair.Value);
                      }*/
                }
                SaveDBChanges(ref db);
                db.Dispose();
            }
            return reportdate;
        }

        private void button28_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                List<InitialTrade> lInitTrades = TradeParsing("LMAX", "CSV", "FX", "Main");
                List<CpTrade> lCptrades = OpenConverting(lInitTrades, "LMAX");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("LMAX");
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "LMAX" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        Map map = symbolmap[cpTrade.Symbol];
                        cpTrade.BOSymbol = map.BOSymbol;
                        cpTrade.Price = cpTrade.Price*map.MtyPrice;
                        cpTrade.Qty = cpTrade.Qty*map.MtyVolume;
                        cpTrade.value = cpTrade.value*map.Leverage;
                        if (contractdetails.ContainsKey(map.BOSymbol))
                        {
                            cpTrade.ValueDate = contractdetails[map.BOSymbol].ValueDate;
                        }
                        else
                        {
                            cpTrade.ValueDate = map.ValueDate;
                        }
                        db.CpTrades.Attach(cpTrade);
                        db.Entry(cpTrade).State = (EntityState)System.Data.Entity.EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }
            RecProcess(reportdate, "LMAX",false);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice","prod");
            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 &&
                                       ft.cp == "Manual" &&
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date)
                                       && ft.Posted == null
                                 select new
                                     {
                                         ft.account_id,
                                         ft.symbol,
                                         BOSymbol = ft.symbol,
                                         ft.value,
                                         type = ft.Type,
                                         ft.ccy,
                                         ft.counterccy,
                                         ft.ValueCCY,
                                         ft.Comment,
                                         tradeDate = ft.TradeDate,
                                         id = ft.fullid
                                     }).ToList();
            int tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                var p = new FTjson();
                p.operationType = VARIABLE.type;
                p.comment = VARIABLE.Comment;
                p.asset = VARIABLE.ccy;
                p.symbolId = VARIABLE.BOSymbol;
                p.accountId = VARIABLE.account_id;
                p.amount = Math.Round((double) VARIABLE.value, 2).ToString();
                p.timestamp = VARIABLE.tradeDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string requestFTload = JsonConvert.SerializeObject(p);
                if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                {
                    LogTextBox.AppendText("\r\n Error in sending FT for : " + VARIABLE.id);
                }
                else
                {
                    db.Database.ExecuteSqlCommand("update FT SET Posted= NOW() where fullid=" + VARIABLE.id);
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded FT for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradefromDb.Count);
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
          FORTSReconciliation("Renesource", "UMAC0288",true);
            var db = new EXANTE_Entities(_currentConnection);
         //   db.Database.ExecuteSqlCommand(
         //       "UPDATE CpTrades AS cp INNER JOIN Contracts AS c ON c.id = cp.BOSymbol SET cp.value = - cp.Qty*cp.Price*c.Leverage WHERE cp.BrokerId LIKE '%Rene%' AND ReportDate > '2016-06-01'");
         //   db.Dispose();

            DateTime reportdate = InputDate.Value; //todo Get report date from xml Processing date
           if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Renesource trades uploading");
                List<InitialTrade> LInitTrades = TradeParsing("Renesource", "EXCEL", "ST", "UMAC0288");
                List<CpTrade> lCptrades = InitTradesConverting(LInitTrades, "Renesource");
                int i = 0;
                int batchsize = 200;
                DateTime TimeStartInternal = DateTime.Now;
                foreach (CpTrade cptrade in lCptrades)
                {
                    i++;
                    cptrade.account = "UMAC0288";
                    if (cptrade.Symbol == "0")
                    {
                        cptrade.Type = "REPO";
                        cptrade.Symbol = cptrade.Comment;
                    }
                    else
                    {
                        cptrade.Type = "ST";
                    }
                    db.CpTrades.Add(cptrade);
                    if (i % batchsize == 0)
                    {
                        SaveDBChanges(ref db);
                        DateTime TimeEndInternal = DateTime.Now;
                        LogTextBox.AppendText("\r\n" + "Cptrades uploading time for " + batchsize.ToString() + " :" + (TimeEndInternal - TimeStartInternal).ToString());
                        TimeStartInternal = DateTime.Now;
                    }
                }
                SaveDBChanges(ref db);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " +
                                      "Renesource trades uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            else
            {
                DateTime nextdate = reportdate.AddDays(1);
                Dictionary<string, Map> symbolmap = getMapping("Renesource");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                IQueryable<CpTrade> cptradefromDb = from cptrade in db.CpTrades
                                                    where cptrade.valid == 1 && cptrade.BrokerId == "Renesource" &&
                                                          cptrade.ReportDate >= reportdate.Date &&
                                                          cptrade.ReportDate < (nextdate.Date) &&
                                                          cptrade.BOTradeNumber == null
                                                    select cptrade;
                IQueryable<Contract> contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                Dictionary<string, Contract> contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    var valuedate = (DateTime)cpTrade.ValueDate;
                    if (cpTrade.BOSymbol == null)
                    {
                        cpTrade.BOSymbol = GetSymbolLek(symbolmap, cpTrade.Symbol, ref MtyVolume, contractdetails,
                                                        ref MtyPrice, ref valuedate, ref Leverage);
                        cpTrade.Price = cpTrade.Price * MtyPrice;
                        cpTrade.Qty = cpTrade.Qty * MtyVolume;
                        cpTrade.ValueDate = valuedate;
                    }
                }
            }
            RecProcess(reportdate, "Renesource",true);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */


        }

        public class BOtrade
        {
            public Double Price;
            public double Qty;
            public Boolean RecStatus;
            public long TradeNumber;
            public long ctradeid;
            public string symbol;
        }

        internal class FullTrade
        {
            public string Account { get; set; }
            public string Symbol { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
            public double Value { get; set; }
        }

        public class Map
        {
            // private string BrockerSymbol { get; set; }
            public string BOSymbol { get; set; }
            public double? MtyPrice { get; set; }
            public double? MtyVolume { get; set; }
            public string Type { get; set; }
            public int? Round { get; set; }
            public DateTime? ValueDate { get; set; }
            public double? Leverage { get; set; }
            public double? MtyStrike { get; set; }
            public Boolean? UseDayInTicker { get; set; }
            public SByte? calendar { get; set; }
        }

        internal class Trade
        {
            public double qty { get; set; }
            public long id { get; set; }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");
            //var token = GetToken("https://authdb.prod.ghcg.com/api/1.0/auth/session", "backoffice");
            DateTime reportdate = InputDate.Value;
            var acc = new BOaccount
            {
                accountNameCP = null, // "EXANTE",
            };
            bool sendFee = false;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            IQueryable<Ctrade> cptradefromDb = from Ctrade in db.Ctrades
                                                where Ctrade.valid == 1 &&
                                                      Ctrade.Date >= reportdate.Date &&
                                                      Ctrade.Date < (nextdate.Date) && 
                                                      Ctrade.tradeType == "EXPIRATION" &&
                                                      Ctrade.order_id == "toPost"
                                                select Ctrade;
            List<Ctrade> cptradeitem = cptradefromDb.ToList();
            int tradesqty = 0;

            foreach (Ctrade ctrade in cptradeitem)
            {
               BOjson json =JsonfromCtrade(ctrade,false);
               string requestPayload = JsonConvert.SerializeObject(json);
               //      if (SendJson(requestPayload, conStr + acc.BOaccountId + "/trade", token))
               if (SendJson(requestPayload, conStr + ctrade.account_id + "/trade", token))
               {
                   ctrade.order_id = "POSTED"+ctrade.fullid.ToString();
                   tradesqty++;
               }
               else
               {
                   LogTextBox.AppendText("\r\n Error in sending to BO for fullid: " + ctrade.fullid);
               }
               SaveDBChanges(ref db);
            }
            if (tradesqty > 0)
            {
                SaveDBChanges(ref db);
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded trades for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradeitem.Count);
            }
        }


        private void button32_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //   var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            const string conStr = "https://backoffice-demo.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
          //  const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");
        //   string token = GetToken("https://authdb-demo.exante.eu/api/1.0/auth/session", "backoffice", "test");
            string initialstring = "https://backoffice.exante.eu/api/v2.0/trades?";
            DateTime reportdate = InputDate.Value;
            var checkingId = false;
            string[] checkMaltaArray = new string[] {"Malta", "Cyprus" };//"Malta", 
            foreach (string checkMalta in checkMaltaArray)
            {
                var teststring = "https://backoffice.exante.eu/api/v2.0/trades?role=manager&legalEntity=" + checkMalta +
                                 "&order=desc&limit=10000&beginDate=" + reportdate.Year + "-" + reportdate.Month + "-" +
                                 reportdate.Day;
                var DBurl = new Uri(teststring);
                var dbReq = WebRequest.Create(DBurl) as HttpWebRequest;
                dbReq.ContentType = "application/json";
                dbReq.UserAgent = "curl/7.37.0";
                dbReq.Method = "GET";
                var encoding = new UTF8Encoding();
                dbReq.Credentials = CredentialCache.DefaultCredentials;
                List<string> credential = getcredentials("prod");
                var Credentials = new NetworkCredential(credential[0], credential[1]); //bo
                //var Credentials = new NetworkCredential("az", "AF*(*HBfdfacb"); //bo
                dbReq.Credentials = Credentials;
                dbReq.Accept = "application/json";
                dbReq.ContentType = "application/json";
                dbReq.Headers.Add("X-Auth-Username", "az");
                dbReq.Headers.Add("X-Auth-SessionId", token);
                try
                {
                    using (Stream requestStream = dbReq.GetResponse().GetResponseStream())
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(requestStream);
                        string sLine = sr.ReadLine();
                        var db = new EXANTE_Entities(_currentConnection);
                        while (sLine != null)
                        {
                            Dictionary<string, long> checkId = null;
                            if (checkingId)
                            {
                                var test = reportdate.Year + "-" + reportdate.Month + "-" + reportdate.Day;
                                checkId = (from ct in db.Ctrades
                                           where
                                               ct.BOtradeTimestamp.ToString()
                                                 .Contains(reportdate.Year + "-" + reportdate.Month + "-" +
                                                           reportdate.Day)
                                           select ct).ToDictionary(
                                               k => (k.order_id.ToString() + k.orderPos.ToString()), k => k.fullid);
                            }
                            JObject jObject = JObject.Parse(sLine);
                            var i = 0;
                            IQueryable<Contract> contractrow = from ct in db.Contracts
                                                               where ct.valid == 1
                                                               select ct;
                            Dictionary<string, DateTime?> contractdetails = contractrow.ToDictionary(k => k.id,
                                                                                                     k => k.ValueDate);
                            foreach (var VARIABLE in jObject["trades"])
                            {
                                i++;
                                string id = string.Concat((string) VARIABLE["orderId"], (string) VARIABLE["orderPos"]);
                                if (!checkingId || !checkId.ContainsKey(id))
                                {
                                    int side = 1;
                                    const int GMToffset = 4; //gmt offset from BO
                                    const int nextdaystarthour = 20; //start new day for FORTS
                                    const string template = "FORTS";
                                    DateTime nextdayvalueform = Fortsnextday.Value;
                                    var symbol = (string) VARIABLE["symbolId"];
                                    DateTime? valuedate;
                                    if (!contractdetails.TryGetValue(symbol, out valuedate))
                                    {
                                        valuedate = new DateTime(2011, 01, 01);
                                        //todo fill correct value date from file
                                        var test = new Contract
                                            {
                                                id = symbol,
                                                Contract1 = symbol,
                                                Exchange = "Needtoupdate",
                                                Type = "Needtoupdate",
                                                Leverage =
                                                    double.Parse((string) VARIABLE["contractMultiplier"],
                                                                 CultureInfo.InvariantCulture),
                                                ValueDate = valuedate, //Convert.ToDateTime(rowstring[idvalueDate]),
                                                Currency = (string) VARIABLE["currency"],
                                                Margin = 0,
                                                FlatMargin = 0,
                                                Canbesettled = true,
                                                UpdateDate = DateTime.UtcNow,
                                                commission =
                                                    double.Parse((string) VARIABLE["commission"],
                                                                 CultureInfo.InvariantCulture)/
                                                    double.Parse((string) VARIABLE["quantity"],
                                                                 CultureInfo.InvariantCulture),
                                                Timestamp = DateTime.UtcNow,
                                                valid = 1,
                                                username = "TradeParser"
                                            };
                                        db.Contracts.Add(test);
                                        SaveDBChanges(ref db);
                                        contractrow =
                                            from ct in db.Contracts
                                            where ct.valid == 1
                                            select ct;
                                        contractdetails = contractrow.ToDictionary(k => k.id, k => k.ValueDate);
                                    }

                                    if ((string) VARIABLE["side"] == "sell") side = -1;
                                    DateTime vBOtradeTimestamp = Convert.ToDateTime(VARIABLE["tradeTime"]);
                                    string vDate = vBOtradeTimestamp.ToString();
                                    if (symbol.IndexOf(template) > 0)
                                    {
                                        DateTime fortscurrentDate = vBOtradeTimestamp;
                                        string initialdate = fortscurrentDate.ToShortDateString();
                                        fortscurrentDate = fortscurrentDate.AddHours(24 - nextdaystarthour + GMToffset);
                                        if (initialdate != fortscurrentDate.ToShortDateString())
                                            fortscurrentDate = nextdayvalueform;
                                        vDate = fortscurrentDate.ToShortDateString();
                                    }
                                    db.Ctrades.Add(new Ctrade
                                        {
                                            ExchangeOrderId = (string) VARIABLE["exchangeOrderId"],
                                            account_id = (string) VARIABLE["accountId"],
                                            Date = Convert.ToDateTime(vDate),
                                            symbol_id = symbol,
                                            qty = ((string) VARIABLE["quantity"]).IndexOf(".") == -1
                                                      ? Convert.ToInt64(VARIABLE["quantity"])*side
                                                      : double.Parse((string) VARIABLE["quantity"],
                                                                     CultureInfo.InvariantCulture)*side,
                                            price =
                                                double.Parse((string) VARIABLE["price"], CultureInfo.InvariantCulture),
                                            cp_id = (string) VARIABLE["executionCounterparty"],
                                            fees =
                                                double.Parse((string) VARIABLE["commission"],
                                                             CultureInfo.InvariantCulture),
                                            value_date = valuedate,
                                            currency = (string) VARIABLE["currency"],
                                            orderPos = Convert.ToInt32(VARIABLE["orderPos"]),
                                            Timestamp = DateTime.UtcNow,
                                            valid = 1,
                                            username = (string) VARIABLE["userId"],
                                            order_id = (string) VARIABLE["orderId"],
                                            // gatewayId = rowstring[idgateway],
                                            BOtradeTimestamp = Convert.ToDateTime(VARIABLE["tradeTime"]),
                                            tradeType = (string) VARIABLE["tradeType"],
                                            SettlementCp = (string) VARIABLE["settlementCounterparty"],
                                            Value =
                                                -side*
                                                Math.Abs(double.Parse((string) VARIABLE["tradedVolume"],
                                                                      CultureInfo.InvariantCulture)),
                                            mty =
                                                (Int64)
                                                double.Parse((string) VARIABLE["contractMultiplier"],
                                                             CultureInfo.InvariantCulture),
                                            deliveryDate = Convert.ToDateTime(VARIABLE["valueDate"]),
                                            EntityLegalMalta = checkMalta == "Malta"
                                                                   ? true
                                                                   : false
                                        });
                                }
                            }
                            sLine = sr.ReadLine();
                        }
                        SaveDBChanges(ref db);
                        db.Database.ExecuteSqlCommand("CALL updateTradeNumbers()");
                        sr.Close();
                    }
                }
                catch (WebException ex)
                {
                    LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                    LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                    LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                    var reader = new StreamReader(ex.Response.GetResponseStream());
                    LogTextBox.Text = LogTextBox.Text + reader.ReadToEnd();
                }
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            DateTime reportdate = InputDate.Value;
            if (!noparsingCheckbox.Checked)
            {
                reportdate = AxiPdfParser(reportdate);
            }
            RecProcess(reportdate, "BCS", false);
        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void procedures_Click(object sender, EventArgs e)
        {

        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            string token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice", "prod");
            DateTime reportdate = InputDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            DateTime nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 &&
                                       ft.Type == "PERFORMANCE FEE" && 
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date)
                                       && ft.Posted == null
                                 select new
                                 {
                                     ft.account_id,
                                     ft.symbol,
                                     BOSymbol = ft.symbol,
                                     ft.value,
                                     type = ft.Type,
                                     ft.ccy,
                                     ft.counterccy,
                                     ft.ValueCCY,
                                     ft.Comment,
                                     tradeDate = ft.TradeDate,
                                     id = ft.fullid
                                 }).ToList();
            int tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                var p = new FTjson();
                p.operationType = VARIABLE.type;
                p.comment = VARIABLE.Comment;
                p.asset = VARIABLE.ccy;
                p.symbolId = VARIABLE.BOSymbol;
                p.accountId = VARIABLE.account_id;
                p.amount = Math.Round((double)VARIABLE.value, 2).ToString();
                p.timestamp = VARIABLE.tradeDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string requestFTload = JsonConvert.SerializeObject(p);
                if (!SendJson(requestFTload, conStr + VARIABLE.account_id + "/transaction", token))
                {
                    LogTextBox.AppendText("\r\n Error in sending FT for : " + VARIABLE.id);
                }
                else
                {
                    db.Database.ExecuteSqlCommand("update FT SET Posted= NOW() where fullid=" + VARIABLE.id);
                }
            }
            if (tradesqty > 0)
            {
                db.SaveChanges();
                db.Dispose();
                LogTextBox.AppendText("\r\n Uploaded FT for " + reportdate.ToShortDateString() + ": " +
                                      tradesqty.ToString() + "/" + cptradefromDb.Count);
            }
        }

        private void RJO_belarta_click(object sender, EventArgs e)
        {
           parsingProcess("RJOBelarta");
           RecProcess(InputDate.Value, "RJOBelarta", false);
        }

        private void parsingProcess(string brokername)
        {
           DateTime TimeStart = DateTime.Now;
           var db = new EXANTE_Entities(_currentConnection);
           DialogResult result = openFileDialog2.ShowDialog();
           if (result == DialogResult.OK) // Test result.
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
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
                    var reportdate = getReportDateFromRjo(txt);
                    int indexStart = txt.IndexOf("T R A D E S   C O N F I R M A T I O N S");
                    if (indexStart < 0) return;
                    txt = txt.Substring(indexStart);
                    ParsingToTrades(ref txt,ref db);

                   string[] rows;
                   
                    var dicCpCtrades = new Dictionary<string, List<CpTrade>>();
                    bool flagStop = false;
                    int i_row = 0;
                    
                    string account;
                    string type;

                    if (txt.Contains("ROLLOVER TRADE DETAILS"))
                    {
                        rows = txt.Split('\n');
                        i_row = AxiPdfGetStarRow(rows, "ROLLOVER TRADE DETAILS");
                        if (i_row != -1)
                        {
                            account = rows[i_row - 1];
                            dicCpCtrades = new Dictionary<string, List<CpTrade>>();
                            type = "FX";
                            while ((i < count) && (!flagStop))
                            {
                                txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                                rows = txt.Split('\n');
                                if (i_row != -1)
                                {
                                    while ((i_row < rows.Length) && (!rows[i_row].Contains("Page")) &&
                                           (!rows[i_row].Contains("CASH MOVEMENTS")) &&
                                           (!rows[i_row].Contains("SETTLING TRADE DETAILS")) &&
                                           (!rows[i_row].Contains("NEW TRADING ACTIVITY"))
                                            && (!flagStop))
                                    {
                                        string[] traderow =
                                            rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                        if (traderow.Count() == 1)
                                        {
                                            string tempvalue = traderow[0];
                                            i_row++;
                                            traderow = rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                            Array.Resize(ref traderow, traderow.Length + 1);
                                            traderow[traderow.Length - 1] = tempvalue;
                                            LogTextBox.AppendText("\r\n" + "Pay attention to value: " + tempvalue);
                                            //  traderow.SetValue(tempvalue, traderow.Count());
                                        }
                                        if ((traderow[0] == "SETTLED") || (traderow[1] == "SETTLED"))
                                        {
                                            flagStop = true;
                                        }
                                        else
                                        {
                                            int add = 0;
                                            string tradenumber;
                                            string[] sp = traderow[0].Split('-');
                                            if (traderow[0].Split('-').Count() > 1)
                                            {
                                                add = 1;
                                                tradenumber = rows[i_row + 1].TrimStart();
                                            }
                                            else
                                            {
                                                tradenumber = traderow[0];
                                            }

                                            var tempdate = traderow[1 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime tradedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                     CultureInfo.InvariantCulture);
                                            tempdate = traderow[2 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime nearvaluedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                            tempdate = traderow[3 - add];
                                            if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                            DateTime farrvaluedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                            double Qty = AxiPdfGetNegativeValue(traderow[9 - add]);
                                            if ((!checkId.ContainsKey(tradenumber + "NearLeg")))
                                            {
                                                dicCpCtrades.Add(tradenumber + "NearLeg", new List<CpTrade>());
                                                dicCpCtrades[tradenumber + "NearLeg"].Add(new CpTrade
                                                {
                                                    account = account,
                                                    BrokerId = "Axi",
                                                    BOcp = null,
                                                    BOSymbol = null,
                                                    BOTradeNumber = null,
                                                    valid = 1,
                                                    Timestamp = DateTime.UtcNow,
                                                    exchangeOrderId = tradenumber + "NearLeg",
                                                    ccy = traderow[8 - add],
                                                    ReportDate = reportdate,
                                                    TradeDate = tradedate,
                                                    Symbol = traderow[4 - add],
                                                    Type = type,
                                                    TypeOfTrade = "Swap",
                                                    Qty = Qty,
                                                    Price = Convert.ToDouble(traderow[10 - add]),
                                                    ValueDate = nearvaluedate,
                                                    value = AxiPdfGetNegativeValue(traderow[12 - add])
                                                });
                                            }
                                            if ((!checkId.ContainsKey(tradenumber + "FarLeg")))
                                            {
                                                dicCpCtrades.Add(tradenumber + "FarLeg", new List<CpTrade>());
                                                double farprice = AxiPdfGetNegativeValue(traderow[11 - add]);
                                                if (traderow[11 - add].Contains("("))
                                                {
                                                    farprice = AxiPdfGetNegativeValue(traderow[12 - add]);
                                                    LogTextBox.AppendText("\r\n" + "Pay attention to value: " +
                                                                          farprice.ToString());
                                                }
                                                if ((Math.Abs(Convert.ToDouble(traderow[10 - add]) / farprice) > 1.2) ||
                                                    (Math.Abs(Convert.ToDouble(traderow[10 - add]) / farprice) < 0.8))
                                                {
                                                    farprice = AxiPdfGetNegativeValue(traderow[12 - add]);
                                                    LogTextBox.AppendText("\r\n" + "Pay attention to value: " +
                                                                          farprice.ToString());
                                                }


                                                dicCpCtrades[tradenumber + "FarLeg"].Add(new CpTrade
                                                {
                                                    account = account,
                                                    BrokerId = "Axi",
                                                    BOcp = null,
                                                    BOSymbol = null,
                                                    BOTradeNumber = null,
                                                    valid = 1,
                                                    Timestamp = DateTime.UtcNow,
                                                    exchangeOrderId = tradenumber + "FarLeg",
                                                    ccy = traderow[8 - add],
                                                    ReportDate = reportdate,
                                                    TradeDate = tradedate,
                                                    Symbol = traderow[4 - add],
                                                    Type = type,
                                                    TypeOfTrade = "Swap",
                                                    Qty = -Qty,
                                                    Price = farprice,
                                                    ValueDate = farrvaluedate,
                                                    value = Qty * farprice
                                                });
                                            }
                                            i_row = i_row + add + 1;
                                        }
                                    }
                                    if ((i_row < rows.Length) &&
                                        ((rows[i_row].Contains("CASH MOVEMENTS")) ||
                                         (txt.Contains("NEW TRADING ACTIVITY")) ||
                                         (txt.Contains("SETTLING TRADE DETAILS"))))
                                    {
                                        flagStop = true;
                                    }
                                }
                                i_row = 0;
                                if (!flagStop) i++;
                            }
                        }
                    }
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    while (i < count && !txt.Contains("NEW TRADING ACTIVITY"))
                    {
                        i++;
                        txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    }
                    if (txt.Contains("NEW TRADING ACTIVITY"))
                    {
                        flagStop = false;
                        i_row = 0;
                        rows = txt.Split('\n');
                        i_row = AxiPdfGetStarRow(rows, "NEW TRADING ACTIVITY");
                        if (i_row > -1)
                        {
                            account = rows[i_row - 1];
                            while ((i < count) && (!flagStop))
                            {
                                txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                                rows = txt.Split('\n');
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
                                                type = traderow[1 - add];
                                                var tempdate = traderow[3 - add];
                                                if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                                DateTime tradedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                                tempdate = traderow[4 - add];
                                                if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                                DateTime valuedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                                         CultureInfo.InvariantCulture);
                                                //   if (!dicCpCtrades.ContainsKey(tradenumber) && (type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                                if ((type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                                {
                                                    if (!dicCpCtrades.ContainsKey(tradenumber))
                                                    {
                                                        dicCpCtrades.Add(tradenumber, new List<CpTrade>());
                                                    }
                                                    dicCpCtrades[tradenumber].Add(new CpTrade
                                                    {
                                                        account = account,
                                                        BrokerId = "Axi",
                                                        BOcp = null,
                                                        BOSymbol = null,
                                                        BOTradeNumber = null,
                                                        valid = 1,
                                                        Timestamp = DateTime.UtcNow,
                                                        exchangeOrderId = tradenumber,
                                                        ccy = traderow[7 - add],
                                                        ReportDate = reportdate,
                                                        TradeDate = tradedate,
                                                        Symbol = traderow[5 - add],
                                                        Type = traderow[2 - add],
                                                        TypeOfTrade = "Trade",
                                                        Qty = AxiPdfGetNegativeValue(traderow[8 - add]),
                                                        Price = Convert.ToDouble(traderow[9 - add]),
                                                        ValueDate = valuedate,
                                                        value = AxiPdfGetNegativeValue(traderow[10 - add])
                                                    });
                                                }
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
                                i++;
                            }
                        }
                        else flagStop = true;
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
                                     (valuePair.Value[0].Symbol != "RUB/USD")))
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
                        SaveDBChanges(ref db);
                    }


                    DateTime TimeEnd = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + dicCpCtrades.Count +
                                          " trades Axi uploading completed." +
                                          (TimeEnd - TimeStart).ToString());
                    LogTextBox.AppendText("\r\n" + oFilename);

                }
                SaveDBChanges(ref db);
                db.Dispose();
            }
        }

private DateTime getReportDateFromRjo(string txt)
{
 	throw new NotImplementedException();
}

        private void ParsingToTrades(ref string txt, ref EXANTE_Entities db)
        {
            string[] rows;
            rows = txt.Split('\n');
            int j = 4;
            while (j <= rows.Count() && !txt.Contains("TOTAL"))
            {
                string[] tabs = rows[j].Split(' ');
                db.InitialTrades.Add(new InitialTrade{Account =null,
                AccruedInterest = null,
                BrokerId = "RJOBelarta",
                ccy=null,
                ClearingFeeCcy =null,
                Comment =null,
                cp_id =null,
                ExchangeFees = null,
                exchangeOrderId = null,
                ExchFeeCcy = null,
                ReportDate=null,
                TradeDate= null
                });
                txt = txt + tabs[1];
                j++;
            }
        }
    }


    internal class BOaccount
    {
        internal string BOaccountId;
        internal string DBcpName;
        internal string accountNameCP;
    }


    [DataContract]
    internal class FTjson
    {
        [DataMember] internal string accountId;

        [DataMember] internal string amount;
        [DataMember] internal string asset;
        [DataMember] internal string comment;
        [DataMember] internal string internalComment;
        [DataMember] internal string operationType;
        [DataMember] internal string symbolId;
        [DataMember] internal string timestamp;
    }


    [DataContract]
    internal class BOjson
    {
        [DataMember] internal string accountId;
        [DataMember] internal string brokerAccountId;
        [DataMember] internal string brokerClientId;
        [DataMember] internal string comment;
        [DataMember] internal string commission;
        [DataMember] internal string commissionCurrency;
        [DataMember] internal string counterparty;
        [DataMember] internal string exchangeOrderId;
        [DataMember] internal string gwTime;
        [DataMember] internal string internalComment;
        [DataMember] internal Boolean isManual;
        [DataMember] internal string price;
        [DataMember] internal string quantity;
        [DataMember] internal Boolean redemption;
        [DataMember] internal string settlementBrokerAccountId;
        [DataMember] internal string settlementBrokerClientId;
        [DataMember] internal string settlementCounterparty;
        [DataMember] internal string side;
        [DataMember] internal string symbolId;
        [DataMember] internal Boolean takeCommission;
        [DataMember] internal string tradeType;
        [DataMember] internal string userId;
        [DataMember] internal string valueDate;
    }

    internal class cpCost_cpTrade
    {
        internal string BOTradeNumber;
        internal string BrokerId;
        internal string ExchFeeCcy;
        internal double? ExchangeFees;
        internal double? Fee;
        internal double? Fee2;
        internal double? Fee3;
        internal double? Qty;
        internal string Symbol;
        internal string ccy;
    }

    internal class cpCost_cTrade
    {
        internal string ExchFeeCcy;
        internal string account_id;
        internal string cp_id;
        internal string currency;
        internal double? fees;
        internal double? qty;
        internal string symbol_id;
        internal long? tradeNumber;
    }
}