using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
//using System.Data.Entity.Core.Common.;
using System.Data.Entity.Core.Objects;
//using System.Data.Objects; 
//using System.Data.Entity.Core.EntityClient;
//Objects.SqlClient;
//using System.DaSqlClient;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Marshal = System.Runtime.InteropServices.Marshal;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Application = Microsoft.Office.Interop.Excel.Application;
using EntityState = System.Data.EntityState;
using HtmlDocument = System.Windows.Forms.HtmlDocument;
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
            var connection = System.Configuration.ConfigurationManager.ConnectionStrings;
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
            var brockerlist = (from rec in db.DBBORecon_mapping
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
                var nextdayvalueform = Fortsnextday.Value;
                var lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText(TimeStart.ToLongTimeString() + ": " + "start BO trades uploading");
                var index = 1;
                if (lineFromFile != null)
                {
                    var rowstring = lineFromFile.Split(Delimiter);
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
                    for (var i = 0; i < rowstring.Length; i++)
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

                    var stringindex = Convert.ToString(reportdate.Year);
                    if (reportdate.Month < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Month));
                    if (reportdate.Day < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Day));
                    var initialindex = Convert.ToInt64(stringindex);
                    var contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    var contractdetails = contractrow.ToDictionary(k => k.id, k => k.ValueDate);
                    var currntmonth = reportdate.Year + "-" + reportdate.Month;
                    var checkId =
                        (from ct in db.Ctrades
                         where ct.BOtradeTimestamp.ToString().Contains("2016-02-12")
                         select ct).ToDictionary(k => (k.order_id.ToString() + k.orderPos.ToString()), k => k.fullid);
                    ;

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
                            var side = 1;
                            if (rowstring[idside] == "sell") side = -1;
                            var vBOtradeTimestamp = Convert.ToDateTime(rowstring[idDate]);
                            if (rowstring[idSymbol].IndexOf(template) > 0)
                            {
                                var fortscurrentDate = Convert.ToDateTime(rowstring[idDate]);
                                var initialdate = fortscurrentDate.ToShortDateString();
                                fortscurrentDate = fortscurrentDate.AddHours(24 - nextdaystarthour + GMToffset);
                                if (initialdate != fortscurrentDate.ToShortDateString())
                                    fortscurrentDate = nextdayvalueform;
                                rowstring[idDate] = fortscurrentDate.ToShortDateString();
                            }
                            index++;
                            if (index > 0)
                            {
                                var ExchangeOrderId = rowstring[idexchangeOrderId];
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
                                                           : Convert.ToDateTime(rowstring[idvalueDate])
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
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
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
                                             List<string> settCp)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var prevreportdate = reportdate.AddDays(-(double) (numericUpDown1.Value));
            var ts = new TimeSpan(16, 00, 0);

            prevreportdate = prevreportdate.Date + ts;

            var nextdate = reportdate.AddDays(4);
            var boTradeNumberlist = new List<long?>();
            if (removeReconciled)
            {
                var boTradeNumbers = db.CpTrades.Where(
                    cptrade => cptrade.valid == 1 && cptrade.ReportDate >= reportdate.Date &&
                               cptrade.ReportDate < (nextdate.Date) && cptrade.BOTradeNumber != null)
                                       .Select(cptrade => cptrade.BOTradeNumber);
                foreach (string boTradeNumber in boTradeNumbers)
                {
                    var templist = boTradeNumber.Split(';');
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
            var queryable = from ct in db.Ctrades
                            where
                                ct.valid == 1 && ct.RecStatus == false && ct.BOtradeTimestamp >= prevreportdate &&
                                ct.Date < (nextdate.Date)
                                //&& cplist.Contains(ct.cp_id)   && settCp.Contains(ct.SettlementCp)
                                && settCp.Contains(ct.cp_id)
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
            var cpfromDb = from cp in db.counterparties
                           select cp;
            return cpfromDb.ToDictionary(k => k.cp_id, k => k.Name);
        }


        private string FXFWDupdate(string str)
        {
            var indexE2 = str.IndexOf('.') + 1;
            if (indexE2 == 0)
            {
                indexE2 = str.IndexOf("A3");
                if (indexE2 == 0)
                {
                    indexE2 = str.IndexOf("E4");
                }
            }
            var currency = str.Substring(0, indexE2 - 1);
            //  currency=currency.Replace('/');
            if ((str.IndexOf("SPOT") == -1) && (str.IndexOf("EXANTE") == -1) && (str.IndexOf("E6") == -1) &&
                (str.IndexOf("E5") == -1))
            {
                var Date = str.Substring(indexE2 + 3, str.Length - indexE2 - 3);
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

        private void AbnRecon(DateTime reportdate, List<CpTrade> trades, string ccp)
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
                    "CFH",
                    "MOEX-SPECTRA",
                    "MOEX-ASTS",
                    "IS-PRIME",
                    "IB",
                    "INSTANT",
                    "LMAX",
                    ""
                };
            var mltytrades = MultyTradesCheckBox.Checked;
            var skipspr = SkipspreadcheckBox.Checked;
            var db = new EXANTE_Entities(_currentConnection);
            List<string> SettCp = (from ct in db.cpmapping
                                   where
                                       ct.cp.Contains(ccp)
                                   select ct.bosettcp).ToList();


            var boTradeslist = CreateIdForBoTrades(getTradesFromDB(reportdate, cplist, true, SettCp));
            var numberBoTrades = boTradeslist.Count;
            var cpmapping = getBOtoABNMapping();
            var bomap = getMap(ccp);
            var abnTradeslist = CreateIdForCpTrades(getOnlyTrades(trades), ccp);
            var recon = new List<Reconcilation>();

            foreach (var cpTrade in abnTradeslist)
            {
                List<Ctrade> ctrade;
                if (cpTrade.BOSymbol != null)
                {
                    if (cpTrade.BOSymbol.Contains("FORTS"))
                    {
                        var t = 1;
                    }
                }

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
                        var reclist = CheckMultitrades(cpTrade, boTradeslist.Values.SelectMany(x => x).ToList());
                        if (reclist != null)
                        {
                            var n = reclist.Count;
                            for (var i = 0; i < n; i++)
                            {
                                var keysWithMatchingValues =
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
                SaveDBChanges(ref db);
            }


            for (int j = boTradeslist.Count - 1; j >= 0; j--)
            {
                var currentkey = boTradeslist.Keys.ElementAt(j);
                List<Ctrade> valuePair = boTradeslist[currentkey];
                for (var listindex = 0; listindex < valuePair.Count; listindex++)
                {
                    var ctrade = valuePair[listindex];
                    var reclist = new List<CpTrade>();

                    if (!SkipspreadcheckBox.Checked)
                    {
                        if ((ctrade.symbol_id.Contains(".CS/")) || (ctrade.symbol_id.Contains(".RS/")))
                        {
                            var reclistids = workeithCS(ctrade, abnTradeslist, false);
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
                        var n = reclist.Count;
                        for (var i = 0; i < n; i++)
                        {
                            var templist = new List<Ctrade> {ctrade};
                            UpdateRecTrades(reclist[i], templist, db, recon);
                        }
                        SaveDBChanges(ref db);
                        boTradeslist[currentkey].RemoveAt(listindex);
                        listindex--;
                    }

                }
                if (valuePair.Count == 0)
                {
                    boTradeslist.Remove(currentkey);
                }
            }

            if (mltytrades)
            {
                for (int j = boTradeslist.Count - 1; j >= 0; j--)
                {
                    var currentkey = boTradeslist.Keys.ElementAt(j);
                    List<Ctrade> valuePair = boTradeslist[currentkey];
                    for (var listindex = 0; listindex < valuePair.Count; listindex++)
                    {
                        var ctrade = valuePair[listindex];
                        var reclist = new List<CpTrade>();

                        if (!SkipspreadcheckBox.Checked)
                        {
                            if ((ctrade.symbol_id.Contains(".CS/")) || ctrade.symbol_id.Contains(".RS/"))
                            {
                                var reclistids = workeithCS(ctrade, abnTradeslist, true);
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
                            var n = reclist.Count;
                            for (var i = 0; i < n; i++)
                            {
                                var templist = new List<Ctrade> {ctrade};
                                UpdateRecTrades(reclist[i], templist, db, recon);
                            }
                            SaveDBChanges(ref db);
                            boTradeslist[currentkey].RemoveAt(listindex);
                            listindex--;
                        }

                    }
                    if (valuePair.Count == 0)
                    {
                        boTradeslist.Remove(currentkey);
                    }
                }
            }
            foreach (Reconcilation reconcilation in recon)
            {
                db.Reconcilations.Add(reconcilation);
            }
            SaveDBChanges(ref db);
        }

        private void MacRecon(DateTime reportdate, List<CpTrade> trades)
        {
            var cplist = new List<string> {"CQG", "PATS"};
            var boTradeslist = CreateIdForBoTrades(getTradesFromDB(reportdate, cplist, true, null));
            var cpmapping = getBOtoABNMapping();
            var bomap = getMap("Mac");
            var TradeList = CreateIdForCpTrades(getOnlyTrades(trades), "Mac");
            var recon = new List<Reconcilation>();
            var db = new EXANTE_Entities(_currentConnection);
            foreach (var cpTrade in TradeList)
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
            var inndexcs = ctrade.symbol_id.IndexOf(".CS/");
            var mty = 1;
            if (inndexcs == -1)
            {
                inndexcs = ctrade.symbol_id.IndexOf(".RS/");
                mty = -1;
            }
            var indexseparate = ctrade.symbol_id.IndexOf("-");
            var leftside = ctrade.symbol_id.Substring(0, inndexcs + 1) +
                           ctrade.symbol_id.Substring(inndexcs + 4, indexseparate - inndexcs - 4);
            var vd = getValueDate(leftside);
            var Cqty = (double) ctrade.qty*mty;
            var spreadprice = ctrade.price*mty;
            var rightside = ctrade.symbol_id.Substring(0, inndexcs + 1) +
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
            var pricelist = leftalltrades.Select(item => item.price).Distinct().ToList();
            var indexprice = 0;
            var pairfound = false;
            var reclist = new List<long>();
            while (indexprice < pricelist.Count && !pairfound)
            {
                var currentprice = pricelist[indexprice];
                var leftossibleletrades =
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
                    var leftreclist = CheckMultitradesNew(Cqty, leftossibleletrades);
                    if (leftreclist != null)
                    {
                        reclist.Clear();
                        for (var i = 0; i < leftreclist.Count; i++)
                        {
                            reclist.Add(leftossibleletrades[leftreclist[i]].id);
                        }
                        var rightpathprice = (currentprice - spreadprice);
                        rightpathprice = Math.Round((double) rightpathprice, 8);
                        var rightpossibleletrades =
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
                            var rightcty = -Cqty;
                            var rightreclist = CheckMultitradesNew(rightcty, rightpossibleletrades);
                            if (rightreclist != null)
                            {
                                for (var i = 0; i < rightreclist.Count; i++)
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
                var allpossibleletrades =
                    trades.Where(item => (item.qty > 0 && Math.Abs((double) item.qty) <= Math.Abs(qty)));
                possibleletrades = allpossibleletrades.OrderByDescending(o => o.qty).ToList();
            }
            else
            {
                var allpossibleletrade =
                    trades.Where(item => (item.qty < 0 && Math.Abs((double) item.qty) <= Math.Abs(qty)));
                possibleletrades = allpossibleletrade.OrderBy(o => o.qty).ToList();
            }
            return possibleletrades;
        }

        private string getValueDate(string leftside)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var mapfromDb = from c in db.Contracts
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

                var possibletrades =
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
                            sequence.Add((long) possibletrades.ElementAt(0).FullId);
                            listBoTrades.Add(possibletrades.ElementAt(0));
                        }

                    }
                    else
                    {
                        var i = 0;
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
            var botradenumber = ctrade[0].tradeNumber;
            if (cpTrade.BOTradeNumber == null)
            {
                cpTrade.BOTradeNumber = botradenumber.ToString();
            }
            else
            {

                cpTrade.BOTradeNumber = cpTrade.BOTradeNumber + ";" + botradenumber.ToString();
            }
            cpTrade.BOcp = ctrade[0].cp_id;
            cpTrade.BOSymbol = ctrade[0].symbol_id;
            cpTrade.Comment = ctrade[0].BOtradeTimestamp.Value.ToShortDateString();
            ctrade[0].RecStatus = true;
            db.CpTrades.Attach(cpTrade);
            db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
            db.Ctrades.Attach(ctrade[0]);
            db.Entry(ctrade[0]).State = (System.Data.Entity.EntityState) EntityState.Modified;


            recon.Add(new Reconcilation
                {
                    CpTrade_id = cpTrade.FullId,
                    Ctrade_id = botradenumber,
                    Timestamp = DateTime.UtcNow,
                    username = "TradeParser",
                    valid = 1
                });

            SaveDBChanges(ref db);
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
                        var i = 0;
                        double qty = 0;
                        while ((i < possibletrades.Count()) && (qty != initialQty))
                        {
                            qty = (double) possibletrades.ElementAt(i).qty;
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
                nextValue = (double) possibletrades[i].qty;
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
                        var nextlevelqty = calculateQtyNew(InitialQty, qty, i + 1, possibletrades, sequence, level + 1);
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
                    if (i < possibletrades.Count) nextValue = (double) possibletrades[i].qty;
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
                var symbol = trade.BOSymbol;
                var price = trade.Price;
                //   bool positiveqtyflag = !(trade.Qty < 0);
                var initialQty = trade.Qty;
                //      if ((boTrades[i].symbol_id == symbol && boTrades[i].price == price) && (boTrades[i].qty > 0 && positiveqtyflag && (Math.Abs((double)boTrades[i].qty) < qtyflag))) possibletrades.Add(boTrades[i]);
                //      var accounts = boTrades.GroupBy(x => x.account_id).Select(g => g.First().account_id).ToList();
                var possibletrades = boTrades.Where(item => (item.symbol_id == symbol && item.price == price));
                var accounts = possibletrades.GroupBy(x => x.account_id).Select(g => g.First().account_id).ToList();

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
                            sequence.Add((long) possibletrades.ElementAt(0).fullid);
                            listBoTrades.Add(possibletrades.ElementAt(0));
                        }
                    }
                    else
                    {
                        var i = 0;
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
                        for (var j = sequence.Count - 1; j > level; j--) sequence.RemoveAt(j);
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
            var ABNMap = getMap(Brocker);
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
                        var key = "";
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
                                var vd = cpTrade.ValueDate.GetValueOrDefault().ToShortDateString();
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
            var defaltvd = defaultvalue.ToShortDateString();
            var bomap = getMap("BO");
            Map symbolvalue;

            foreach (Ctrade botrade in boTradeslist)
            {
                var vd = botrade.value_date.GetValueOrDefault().ToShortDateString();
                var key = botrade.symbol_id;
                if (vd == defaltvd)
                {
                    if (bomap.TryGetValue(key, out symbolvalue))
                    {
                        key = symbolvalue.BOSymbol + symbolvalue.Type;
                    }
                    else
                    {
                        // ((dateindex > -1)&& (Regex.Match(key.Substring(dateindex+3, 1), "[0-9]").Value != ""))
                        var dateindex = botrade.symbol_id.IndexOf("E2");
                        if (!IsOption(botrade.symbol_id))
                        {
                            if (IsFw(botrade.symbol_id) > -1)
                            {
                                dateindex = dateindex + 3;
                                var date = key.Substring(dateindex);
                                var Monthletter = Regex.Match(date, "[A-Z]").Value;
                                var Day = Convert.ToInt32(date.Substring(0, date.IndexOf(Monthletter)));
                                var Year = Convert.ToInt32(date.Substring(date.IndexOf(Monthletter) + 1));
                                var Month = GetMonthFromLetter(Monthletter);
                                var valuedate = new DateTime(Year, Month, Day);
                                var testtt = key.Substring(0, 7).Replace("/", "");
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
            var dateindex = symbolId.IndexOf("E2");
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
                var key = item.BOSymbol;
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
            for (var index = 0; index < strArray.Count; index++)
            {
                var tempstr = strArray[index];
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
                var key = item.BrockerSymbol;
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
                var key = item.BrockerSymbol;

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
            var reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            var symbolmap = getMap("ABN");
            TradesParserStatus.Text = "Processing";
            if (noparsingCheckbox.Checked)
            {
                RecProcess(reportdate, "ABN");
            }
            else
            {
                var allfromfile = new List<CpTrade>();
                var futtrades = new List<CpTrade>();
                var result = openFileDialog2.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (CliffCheckBox.Checked)
                    {
                        var cliffdict = LoadCliff(openFileDialog2.FileName, reportdate);
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
                    AbnRecon(reportdate, allfromfile, "ABN");
                    DateTime TimeEndReconciliation = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeEndReconciliation.ToLongTimeString() + ": " +
                                          "Reconciliation completed. Time:" +
                                          (TimeEndReconciliation - TimeStartReconciliation).ToString() + "s");

                }
            }
            TradesParserStatus.Text = "Done";

        }

        private void RecProcess(DateTime reportdate, string ccp)
        {
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart + ": " + "start " + ccp + " reconciliation");
            var db = new EXANTE_Entities(_currentConnection);
            var symbolmap = getMap(ccp);
            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = from cptrade in db.CpTrades
                                where
                                    cptrade.valid == 1 && cptrade.BrokerId == ccp &&
                                    cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                    cptrade.BOTradeNumber == null
                                select cptrade;
            if (ccp == "ABN")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "01"); //.Contains(o.StatusCode))
            if (ccp == "Mac")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "A");
            if (ccp == "CFH")
                cptradefromDb = cptradefromDb.Where(o => o.TypeOfTrade == "OnlineTrade");
            //var filteredOrders = orders.Order.Where(o => allowedStatus.Contains(o.StatusCode));
            var cptradelist = cptradefromDb.ToList();
            foreach (CpTrade cpTrade in cptradelist)
            {
                if (cpTrade.BOSymbol == null)
                {
                    Map symbolvalue;
                    var key = cpTrade.Symbol + cpTrade.Type;
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
                    db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
                }
            }

            SaveDBChanges(ref db);
            db.Dispose();

            DateTime TimeStartReconciliation = DateTime.Now;
            AbnRecon(reportdate, cptradelist, ccp);
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
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);

            //var results = products.ToDictionary(product => product.Id);
            //   var authors = Linkdoc.Root.Elements().Select(x => x.Element("UnsettledMovement"));
            var row = -1;
            {
                //XmlNodeList nodes = doc.SelectNodes("/Transactions/AccountTransactions");
                foreach (XmlNode mainnode in doc.DocumentElement.ChildNodes)
                {
                    //  var test = Mainnode.SelectNodes("UnsettledMovement/MovementCode[@Value = '01']");
                    foreach (XmlNode itemNode in mainnode.SelectNodes("UnsettledMovement"))
                    {
                        var list = itemNode.ChildNodes;
                        var MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //    if (new [] {"01", "23", "24"}.Contains(MovementCode)){
                        row++;
                        var Pricemty = 1;
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
                        var typeOftrade = GetTypeOfTradeFromXml(itemNode);
                        if (typeOftrade == "FW" || typeOftrade == "FX")
                        {
                            if (itemNode.SelectSingleNode("TransactionPriceCurrency/CurrencyPricingUnit") !=
                                null)
                            {
                                Pricemty = 10000/Convert.ToInt32(itemNode.SelectSingleNode(
                                    "TransactionPriceCurrency/CurrencyPricingUnit").InnerText);
                            }
                        }

                        var symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + typeOftrade;
                        Map symbolvalue;
                        var bosymbol = "";
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
                        var list = itemNode.ChildNodes;
                        var MovementCode = itemNode.SelectSingleNode("MovementCode").InnerText;
                        //  if (new[] { "01", "23", "24" }.Contains(MovementCode)){
                        var Pricemty = 1;
                        var price = Convert.ToDouble(itemNode.SelectSingleNode("TransactionPrice").InnerText)/
                                    Pricemty;
                        var qty = (itemNode.SelectSingleNode("QuantityShort") == null)
                                      ? Convert.ToInt64(itemNode.SelectSingleNode("QuantityLong").InnerText)
                                      : -1*Convert.ToInt64(itemNode.SelectSingleNode("QuantityShort").InnerText);
                        var symbolid = itemNode.SelectSingleNode("Product/Symbol").InnerText + "FU" +
                                       Convert.ToDateTime(GetValueDate(itemNode)).ToShortDateString();
                        Map symbolvalue;
                        var bosymbol = "";
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
            var lineFromFile = reader.ReadLine();
            if (lineFromFile != null)
            {
                reportdate = (DateTime) getDatefromString(lineFromFile.Substring(6, 8));
            }
            var cliffdict = new Dictionary<string, List<string>>();
            while (!reader.EndOfStream)
            {
                if (lineFromFile != null)
                {
                    var code = lineFromFile.Substring(0, 3);
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
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
            foreach (var row in rowlist)
            {
                var code = row.Substring(124, 2);
                var typeoftrade = row.Substring(60, 2);
                var tradedate = getDatefromString(row.Substring(554), true) ??
                                getDatefromString(row.Substring(562), true);
                var symbol = row.Substring(66, 6).Trim();
                var Counterparty = row.Substring(54, 6).Trim();
                var valuedate = getDatefromString(row.Substring(73, 8).Trim());
                var type = row.Substring(72, 1);
                var strike = double.Parse(row.Substring(81, 8) + '.' + row.Substring(89, 7),
                                          CultureInfo.InvariantCulture);
                var volumelong = double.Parse(row.Substring(128, 10) + '.' + row.Substring(138, 2),
                                              CultureInfo.InvariantCulture);
                var volume = volumelong -
                             double.Parse(row.Substring(141, 10) + '.' + row.Substring(151, 2),
                                          CultureInfo.InvariantCulture);
                var price = double.Parse(row.Substring(247, 8) + '.' + row.Substring(255, 7),
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
                var symbol_id = symbol + "." + type + strike;

                var exchfee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2),
                                           CultureInfo.InvariantCulture);
                if (row.Substring(165, 1) == "D") exchfee = -exchfee;
                var exchfeeccy = row.Substring(166, 3);

                var fee = double.Parse(row.Substring(169, 10) + '.' + row.Substring(179, 2),
                                       CultureInfo.InvariantCulture);
                if (row.Substring(181, 1) == "D") fee = -fee;
                var clearingfeeccy = row.Substring(182, 3);

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
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
            foreach (var row in rowlist)
            {
                var typeoftrade = row.Substring(60, 2);
                var tradedate = getDatefromString(row.Substring(582), true) ??
                                getDatefromString(row.Substring(295), true);
                var symbol = row.Substring(66, 6).Trim();
                var type = row.Substring(60, 2);
                Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;
                int round = 10;
                var symbol_id = symbol + type;
                var valuedate = getDatefromString(row.Substring(303)) ?? getDatefromString(row.Substring(72));

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

                var exchfee = double.Parse(row.Substring(137, 10) + '.' + row.Substring(147, 2),
                                           CultureInfo.InvariantCulture);
                if (row.Substring(149, 1) == "D") exchfee = -exchfee;
                var exchfeeccy = row.Substring(150, 3);

                var fee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2),
                                       CultureInfo.InvariantCulture);
                if (row.Substring(165, 1) == "D") fee = -fee;
                var clearingfeeccy = row.Substring(166, 3);
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
            var symbolmap = getMap("ABN");
            var db = new EXANTE_Entities(_currentConnection);
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime) getDatefromString(rowlist[0].Substring(6, 8));
            foreach (var row in rowlist)
            {
                var type = row.Substring(60, 2);
                var symbol = row.Substring(66, 6).Trim();
                var symbol_id = symbol + type;
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
                    valuedate = (DateTime) getDatefromString(row.Substring(72, 8));
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
                        valuedate = (DateTime) getDatefromString(row.Substring(73, 8));
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
                foreach (var eve in e2.EntityValidationErrors)
                {
                    Console.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
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
            var volumelong = double.Parse(row.Substring(0, 10) + "." + row.Substring(10, 2),
                                          CultureInfo.InvariantCulture);
            var volumeshort = row.Substring(13, 10);
            var resvolume = volumelong -
                            double.Parse(row.Substring(13, 10) + "." + row.Substring(23, 2),
                                         CultureInfo.InvariantCulture);
            return resvolume;
        }

        private void updateBalance(List<string> rowlist, DateTime reportdate)
        {
            var dbentity = new EXANTE_Entities(_currentConnection);
            var cpidfromDb = from cp in dbentity.DailyChecks
                             where cp.Table == "Daily" && cp.date == reportdate
                             select cp.status;
            var listforDb = new List<ABN_cashposition>();
            foreach (var row in rowlist)
            {
                var value = row.Substring(90, 18);
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
                    var cpidfromDb = from cp in dbentity.counterparties
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
            var prop_cpTrades = typeof (CpTrade).GetProperty(cpColumn);
            //var prop_boTrades = typeof (Ctrade).GetProperty(boColumn);
            var recon = new List<Reconcilation>();
            for (var i = 0; i < cpTrades.Count; i++)
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
                                CpTrade_id = i,
                                Ctrade_id = boitemlist[iBoitemlist].TradeNumber,
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
        public class BOtrade
        {
            public long TradeNumber;
            public double Qty;
            public Double Price;
            public string symbol;
            public long ctradeid;
            public Boolean RecStatus;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
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
            RecProcess(reportdate, "ADSS");
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
            var reportDate = ABNDate.Value.Date;
            if (result == DialogResult.OK) // Test result.
            {
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                                                              0, false, 5, "", "", false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel.XlPlatform
                                                                                                       .xlWindows, "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Calendar"];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
                var db = new EXANTE_Entities(_currentConnection);
                var delta = 0;
                int startrow = 17,
                    idIsin = 7 + delta,
                    idDeclDate = 8 + delta,
                    idExDate = 9 + delta,
                    idReDate = 10 + delta,
                    idPayDate = 11 + delta,
                    idDVDAmount = 12 + delta,
                    idDVDFR = 13 + delta,
                    idDvdType = 14 + delta;
                var i = startrow;
                var contracts = (from cp in db.Contracts
                                 where cp.isin != null
                                 select cp).ToList().GroupBy(x => x.isin).ToDictionary(x => x.Key, x => x.ToList());
                var isinExcel = xlRange.Cells[i, idIsin].value2;
                while (isinExcel != null)
                {
                    List<Contract> contractDetails;
                    if (!contracts.TryGetValue(isinExcel, out contractDetails))
                    {
                        LogTextBox.AppendText("\r\n" + "There isin in contracts: " + xlRange.Cells[i, idIsin].value2);
                        db.CorporateActions.Add(new CorporateActions
                            {
                                isin = xlRange.Cells[i, idIsin].value2,
                                DeclaredDate = DateTime.FromOADate(xlRange.Cells[i, idDeclDate].value2),
                                ExDate = DateTime.FromOADate(xlRange.Cells[i, idExDate].value2),
                                RecordDate = DateTime.FromOADate(xlRange.Cells[i, idReDate].value2),
                                PayableDate = DateTime.FromOADate(xlRange.Cells[i, idPayDate].value2),
                                DividendAmount = xlRange.Cells[i, idDVDAmount].value2,
                                DividendType = xlRange.Cells[i, idDvdType].value2,
                                DividendFrqncy = xlRange.Cells[i, idDVDFR].value2,
                                symbolId = null,
                                Timestamp = DateTime.UtcNow
                            });
                    }
                    else
                    {
                        foreach (var contractDetail in contractDetails)
                        {
                            DateTime? lastdate = new DateTime();
                            var DeclaredDate = DateTime.FromOADate(xlRange.Cells[i, idDeclDate].value2);
                            var ExDate = DateTime.FromOADate(xlRange.Cells[i, idExDate].value2);
                            var RecordDate = DateTime.FromOADate(xlRange.Cells[i, idReDate].value2);
                            var PayableDate = DateTime.FromOADate(xlRange.Cells[i, idPayDate].value2);
                            var DividendAmount = xlRange.Cells[i, idDVDAmount].value2;
                            var DividendType = xlRange.Cells[i, idDvdType].value2;
                            var DividendFrqncy = xlRange.Cells[i, idDVDFR].value2;
                            var isin = xlRange.Cells[i, idIsin].value2;
                            var qty = getQtyFromCtrade(db, contractDetail.Contract1, ExDate, ref lastdate, isin,
                                                       reportDate.Date);
                            string comment = null;
                            if ((RecordDate.Year > 1900) && (PayableDate.Year > 1900))
                            {
                                if (DividendAmount == null) DividendAmount = 0;
                                comment = getLastCommentFromCorporateAction(db, reportDate, isin, DeclaredDate,
                                                                            ExDate, RecordDate,
                                                                            PayableDate, DividendAmount,
                                                                            DividendType,
                                                                            DividendFrqncy);
                            }

                            db.CorporateActions.Add(new CorporateActions
                                {
                                    isin = xlRange.Cells[i, idIsin].value2,
                                    DeclaredDate = DateTime.FromOADate(xlRange.Cells[i, idDeclDate].value2),
                                    ExDate = DateTime.FromOADate(xlRange.Cells[i, idExDate].value2),
                                    RecordDate = DateTime.FromOADate(xlRange.Cells[i, idReDate].value2),
                                    PayableDate = DateTime.FromOADate(xlRange.Cells[i, idPayDate].value2),
                                    DividendAmount = xlRange.Cells[i, idDVDAmount].value2,
                                    DividendType = xlRange.Cells[i, idDvdType].value2,
                                    DividendFrqncy = xlRange.Cells[i, idDVDFR].value2,
                                    symbolId = contractDetail.Contract1,
                                    Timestamp = DateTime.UtcNow,
                                    BOQty = qty,
                                    LastTradeDate = lastdate,
                                    ReportDate = reportDate.Date,
                                    Comment = comment
                                });
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
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
            }
        }

        private string getLastCommentFromCorporateAction(EXANTE_Entities db, DateTime reportDate, string isin,
                                                         DateTime declaredDate, DateTime exDate, DateTime recordDate,
                                                         DateTime payableDate, double dividendAmount,
                                                         string dividendType, string dividendFrqncy)
        {
            var t =
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
                var starttime = DateTime.Now;

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
                var endtime = DateTime.Now;
                var delta = endtime - starttime;




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
                var overall = sum.Sum(o => o.qty);


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
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                                                              0, false, 5, "", "", false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel.XlPlatform
                                                                                                       .xlWindows, "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Activity Log"];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
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
                reportDate = ABNDate.Value;
                var account = xlRange.Cells[6, 2].Value2;
                var prevDate = reportDate.AddDays(-7);
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
                    idccy = 10;
                var i = 19;
                var checkId = (from ct in db.CpTrades
                               where
                                   ct.BrokerId.Contains("ADSS") && ct.ReportDate <= (reportDate.Date) &&
                                   ct.ReportDate >= prevDate.Date
                               select ct).ToDictionary(k => (k.Qty.ToString() + k.exchangeOrderId), k => k);
                var checkIdFT = (from ct in db.FT
                                 where
                                     ct.brocker.Contains("ADSS") && ct.Type.Contains("PL") &&
                                     ct.ReportDate >= prevDate.Date
                                 select ct).ToDictionary(k => (k.Comment), k => k.fullid);
                // && ctrade.Date >= reportdate.Date && cptrade.ReportDate < (nextdate.Date)
                while (xlRange.Cells[i, 1].value2 != null)
                {
                    string exchorderid = xlRange.Cells[i, exchangeid].value2.ToString();
                    var qty = xlRange.Cells[i, idSide].value2.IndexOf("Buy") == -1
                                  ? Convert.ToDouble(xlRange.Cells[i, idQty].value2)*(-1)
                                  : Convert.ToDouble(xlRange.Cells[i, idQty].value2);
                    if (!checkId.ContainsKey(qty.ToString() + exchorderid))
                    {
                        var tradedate = DateTime.ParseExact(xlRange.Cells[i, idTradeDate].value2.ToString(),
                                                            "dd/MM/yyyy HH:mm:ss", theCultureInfo);

                        var ValueDate = DateTime.ParseExact(xlRange.Cells[i, idValueDate].value2.ToString(),
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
                        SaveDBChanges(ref db);
                    }
                    i++;
                }
                i = i + 5;

                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
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
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
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
            var reportdate = ABNDate.Value;
            var prevreportdate = reportdate.AddDays(-3);
            var ts = new TimeSpan(20, 00, 0);
            prevreportdate = prevreportdate.Date + ts;
            var db = new EXANTE_Entities(_currentConnection);
            var cplist = new List<string> {"LEK", "CQG", "FASTMATCH", "CURRENEX", "EXANTE", ""};
            TradesParserStatus.Text = "Processing";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText(TimeStart + ": " + "Preparing ABN View");
            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = from cptrade in db.CpTrades
                                where
                                    cptrade.valid == 1 && cptrade.BrokerId == "ABN" &&
                                    cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date)
                                select cptrade;
            var cptradelist = cptradefromDb.ToList();
            var cpmappings = GetCPmapping();
            var contractdetailstable = getContractDetails();
            var updatelist = new List<ABNReconResult>();
            var queryable = from ct in db.Ctrades
                            where
                                ct.valid == 1 && ct.BOtradeTimestamp >= prevreportdate &&
                                ct.BOtradeTimestamp < (nextdate.Date)
                            select ct;
            var boTradeslist = queryable.ToDictionary(k => k.tradeNumber, k => k);


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
                    var BOTrNrs = cpTrade.BOTradeNumber.Split(';');
                    Ctrade ctradevalue;
                    foreach (string boTrNr in BOTrNrs)
                    {
                        var currenttradenumber = Convert.ToInt64(boTrNr);
                        if (!boTradeslist.TryGetValue(currenttradenumber, out ctradevalue))
                        {
                            LogTextBox.AppendText("\r\n" + "Didn't find Ctrade with tradenumber = " +
                                                  currenttradenumber.ToString());
                        }

                        var ctradefromDb = from ctrade in db.Ctrades
                                           where ctrade.valid == 1 && ctrade.tradeNumber == currenttradenumber
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
                var listtodelete = from recon in db.ABNReconResults
                                   where recon.ReportDate >= reportdate.Date && recon.ReportDate < nextdate.Date
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
            var cpfromDb = from cp in db.Contracts
                           select cp;
            return cpfromDb.ToDictionary(k => k.id, k => k);
        }

        private DateTime getcashmovements(List<string> rowlist)
        {
            var dbentity = new EXANTE_Entities(_currentConnection);
            var listforDb = new List<FT>();
            var reportdate = DateTime.ParseExact(rowlist[0].Substring(6, 8), "yyyyMMdd",
                                                 System.Globalization.CultureInfo.InvariantCulture);
            var bomap = getMap("ABN");
            Map symbolvalue;
            foreach (var row in rowlist)
            {
                var symbol = row.Substring(62, 6).Trim();
                var symbol2 = row.Substring(106, 4).Trim();
                /*      if (Convert.ToInt64(row.Substring(135, 9).Trim()) == 587856)
                      {
                          var t = 1;
                      }*/
                var type = row.Substring(60, 2).Trim();
                var bosymbol = "";
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
                                                System.Globalization.CultureInfo.InvariantCulture),
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
                                                System.Globalization.CultureInfo.InvariantCulture),
                        TradeDate =
                            DateTime.ParseExact(row.Substring(71, 8), "yyyyMMdd",
                                                System.Globalization.CultureInfo.InvariantCulture),
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
            var cptradefromDb = from Contr in db.Contracts
                                where Contr.valid == 1
                                select Contr;
            var test = cptradefromDb.ToList();
            LogTextBox.AppendText("\r\n" + "Good connection with " + _currentConnection);
        }

        private void comboBoxEnviroment_TextChanged(object sender, EventArgs e)
        {
            _currentConnection = comboBoxEnviroment.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            updateFORTSccyrates();
           // calcualteVM(ABNDate.Value, "MOEX");
            calcualteVM(ABNDate.Value, "EXANTE");
            calcualteVM(ABNDate.Value, "MOEX-SPECTRA");
            calcualteVM(ABNDate.Value, "OPEN");
           calcualteVM(ABNDate.Value, "INSTANT");

            var db = new EXANTE_Entities(_currentConnection);
            db.Database.ExecuteSqlCommand(
                "UPDATE FT Set Account_id = {0}  WHERE Account_id LIKE {1} AND ReportDate = {2}", "UJL5180.INV",
                "UJL5180.001%", ABNDate.Value.Date);
            db.Dispose();   
        }

        private double? GetVM(DateTime vmDate, string brocker)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = vmDate.AddDays(1);
            var sum =
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

            var listofaccountpositions = Getlistofaccountposition(VMDate, Brocker);
            listofaccountpositions = udpateVMforaccount(listofaccountpositions, VMDate, Brocker);

            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + " End VM Calculation.VM = " +
                                  GetVM(ABNDate.Value, Brocker).ToString() + ". Time:" +
                                  (TimeStart - TimeEndUpdating).ToString());
        }

        private List<FullTrade> udpateVMforaccount(List<FullTrade> listofaccountpositions, DateTime VMDate,
                                                   string Brocker)
        {
            var i = 0;
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = VMDate.AddDays(1);
            var listtodelete = from recon in db.FT
                               where recon.ReportDate >= VMDate.Date && recon.ReportDate < nextdate.Date
                                     && recon.Type.Contains("VM") && recon.cp.Contains(Brocker)
                               select recon;
            db.FT.RemoveRange(listtodelete);
            SaveDBChanges(ref db);

            while (i < listofaccountpositions.Count)
            {
                var fullTrade = listofaccountpositions[i];
                double valueccy = 0;
                if (fullTrade.Value == 0)
                {
                    var currentAtomOfVM = getatomofVM(fullTrade.Symbol, VMDate);
                    var priceFromDb = GetPrice(VMDate, fullTrade.Symbol);
                    var closeAtomOfVM = Math.Round(Math.Round(currentAtomOfVM*priceFromDb, 5), 2,
                                                   MidpointRounding.AwayFromZero);
                    fullTrade.Value =
                        Math.Round(
                            Math.Round(
                                fullTrade.Qty*
                                (closeAtomOfVM -
                                 Math.Round(Math.Round(currentAtomOfVM*fullTrade.Price, 5), 2,
                                            MidpointRounding.AwayFromZero)), 5), 2, MidpointRounding.AwayFromZero);
                    var j = i + 1;

                    while (j < listofaccountpositions.Count)
                    {
                        if ((listofaccountpositions[j].Value == 0) &&
                            (listofaccountpositions[j].Symbol == fullTrade.Symbol))
                        {
                            var t0 = currentAtomOfVM*listofaccountpositions[j].Price;
                            var t1 = Math.Round(currentAtomOfVM*listofaccountpositions[j].Price, 2,
                                                MidpointRounding.AwayFromZero);
                            var t2 = closeAtomOfVM - t1;
                            var t3 = listofaccountpositions[j].Qty*t2;
                            var t4 = Math.Round(t3, 2);



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

            var indexofOption = CustomIndexOf(symbol, '.', 3);
            var key = "";
            if (indexofOption > 0)
            {
                key = symbol.Substring(0, indexofOption) + ".";
            }
            else key = symbol;




            var map =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "OPEN" && ct.Type == "FORTS" && ct.BOSymbol == key
                 select ct.Round).ToList();

            if ((map.Count > 0) && (map[0] == 1))
            {
                var ccyrateFromDblinq =
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
            var pricelinq = from ct in db.Prices
                            where ct.Valid == 1 && ct.Type == "FORTS" && ct.Ticker == symbol && ct.Date == VMDate.Date
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
            var indexofOption = CustomIndexOf(symbol, '.', 3);
            var key = symbol;
            if (indexofOption > 0)
            {
                key = symbol.Substring(0, indexofOption + 1);
            }
            var map =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "OPEN" && ct.Type == "FORTS" && ct.BOSymbol == key
                 select ct).ToList();
            if (map.Count == 1)
            {
                atomvalue = (double) (map[0].MtyPrice/map[0].MtyVolume);
                if (map[0].Round == 1)
                {
                    var ccyrateFromDblinq =
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

            var nextdate = fortsDate.AddDays(1);
            var positionbefore =
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

            var tradesToday =
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

            var lastprice =
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
            var map = getSymbolMap("OPEN", "FORTS");
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
            var indexofOption = CustomIndexOf(currentInstrument, '.', 3);
            var key = "";
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
                var mappingsymbol = symbolvalue.BOSymbol;
                var vd = (DateTime) symbolvalue.ValueDate;
                if (indexofOption > 0)
                {
                    mappingsymbol = mappingsymbol + currentInstrument[indexofOption + 1] + "A " +
                                    currentInstrument.Substring(indexofOption + 2);
                    //+ "M" + vd.ToString("ddMMyy") + currentInstrument[indexofOption + 1] +"A " + currentInstrument.Substring(indexofOption + 2);
                }
                var webpage = GetPage(initialstring + mappingsymbol, "/tr", "</td", list);
                pricefw = getpricefromhtml(webpage, fortsDate);
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

                db.SaveChanges();
            }
            db.Dispose();
            return pricefw;
        }

        private double getpricefromhtml(List<List<string>> pagelist, DateTime fortsDate)
        {
            var index = 1;
            var Datestring = fortsDate.ToString("dd.MM.yyyy");
            while ((index < pagelist.Count) && (pagelist[index].Count < 4 || pagelist[index][3].IndexOf("CSV") == -1))
                index++;
            index++;

            while ((index < pagelist.Count()) && (pagelist[index][0].IndexOf(Datestring) == -1)) index++;
            var temp = "";
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
            var nextdate = fortsDate.AddDays(1);
            var contractrow =
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
            var Date = ABNDate.Value.ToString("yyyy-MM-dd");

            // const string initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx?currency=";
            const string initialstring = "http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=";
            // http://moex.com/export/derivatives/currency-rate.aspx?language=ru&currency=USD/RUB&moment_start=2014-07-24&moment_end=2014-07-24
            var listccy = new List<string>();
            listccy.Add("USD/RUB");
            listccy.Add("EUR/RUB");
            var db = new EXANTE_Entities(_currentConnection);
            foreach (string ccy in listccy)
            {
                var ccystring = initialstring + ccy + "&moment_start=" + Date + "&Date&moment_end=" + Date;
                var doc = new XmlDocument();

                doc.Load(ccystring);
                var upnode = doc.SelectSingleNode("rtsdata");
                string temp = "";
                if (upnode != null)
                {
                    temp = upnode.SelectSingleNode("rates").FirstChild.Attributes[1].Value;
                }

                db.Prices.Add(new Price
                    {
                        Ticker = ccy.Replace("/", ""),
                        Tenor =
                            DateTime.ParseExact(Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        Price1 = Convert.ToDouble(temp),
                        Date =
                            DateTime.ParseExact(Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
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
            var initialstring = "http://moex.com/ru/derivatives/currency-rate.aspx";
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
            var currate = GetPage(initialstring, "/tr", "</td", list);
            var index = 15;
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
        }

        private static void GetHtmlPage(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
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
            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString(page);
            }

            // var strArray = htmlCode.Split(rowsplitter);
            var strArray = htmlCode.Split(new[] {rowsplitter}, StringSplitOptions.None);
            //  return str.Split(new[] { splitter }, StringSplitOptions.None);
            var result = new List<List<string>>();
            //    string[,] result = new string[,] {};
            string row = null;
            for (var i = 0; i < strArray.Count(); i++)
            {
                row = strArray[i];
                var lastlength = 0;
                while (lastlength != row.Count())
                {
                    lastlength = row.Count();
                    for (var index = 0; index < unusefulltags.Count(); index++)
                    {
                        row = row.Replace(unusefulltags[index], "");
                    }
                }
                // var rowlist = row.Split(cellsplitter);
                var rowlist = row.Split(new[] {cellsplitter}, StringSplitOptions.None);

                result.Add(new List<string>(rowlist));
            }
            return result;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac trades uploading");

                var LInitTrades = TradeParsing("Mac", "CSV", "FU");
                var lCptrades = InitTradesConverting(LInitTrades, "Mac");


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

            RecProcess(reportdate, "Mac");
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
            var cptradefromDb = from cptrade in db.CpTrades
                                where
                                    cptrade.valid == 1 && cptrade.BrokerId == "Mac" &&
                                    cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate <= (reportdate.Date) &&
                                    cptrade.BOSymbol == null
                                select cptrade;
            var cptradelist = cptradefromDb.ToList();
            /*   var symbolmap = getMapping("Mac");
               var contractrow =
                       from ct in db.Contracts
                       where ct.valid == 1
                       select ct;
               var contractdetails = contractrow.ToDictionary(k => k.id, k => k);*/
            var symbolmap = GetMapSymbol(cp, db);

            foreach (CpTrade cpTrade in cptradelist)
            {
                Map symbolvalue;
                if (symbolmap.TryGetValue(cpTrade.Symbol + cpTrade.Type, out symbolvalue))
                {
                    var key = symbolvalue.BOSymbol + "." + getLetterOfMonth(cpTrade.ValueDate.Value.Month) +
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
                db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
                SaveDBChanges(ref db);
            }
        }

        private Dictionary<string, Map> GetMapSymbol(string cp, EXANTE_Entities db)
        {
            var mapfromDb =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == cp
                 select ct).ToList();

            var results = new Dictionary<string, Map>();
            var mapfromDblist = mapfromDb.ToList();
            foreach (var item in mapfromDblist)
            {
                var key = item.BrockerSymbol;
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
            var idSymbol = 6;
            var idMacside = 11;
            var idReportDate = 0;
            var idAccount = 1;
            var idcurrency = 4;
            var idTradeDate = 10;
            var idqty = 12;
            var idcp = 19;
            var idSellprice = 15;
            var idBuyPrice = 13;
            var idTypeofTrade = 8;
            var iddeliverydate = 7;
            var idcat = 5;
            var idexchfees = 24;
            var idfees = 23;
            var idoftrade = 32;
            var symbolmap = getMapping("Mac");
            var idTypeofOption = 9;
            var idstrike = 20;
            var idvalue = 39;
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                var cpfromDb = from cp in db.counterparties
                               select cp;
                var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();

                var lineFromFile = reader.ReadLine();
                Map symbolvalue;
                DateTime reportdate = new DateTime();
                if (lineFromFile != null)
                {
                    var rowstring = lineFromFile.Replace("\"", "").Split(Delimiter);
                    var contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    var contractdetails = contractrow.ToDictionary(k => k.id, k => k);
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

                        var side = 1;
                        double price = 0;
                        var symbol_id = rowstring[idSymbol].TrimEnd();
                        var key = symbol_id;
                        var typeoftrade = rowstring[idTypeofTrade].TrimEnd();
                        if (typeoftrade == "O")
                        {
                            key = key + "OP";
                        }
                        var deliveryDate = DateTime.ParseExact(rowstring[iddeliverydate], "yyMM",
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
                        var account_id = rowstring[idAccount].TrimEnd();

                        var ccy = rowstring[idcurrency].TrimEnd();
                        var TradeDate = Convert.ToDateTime(rowstring[idTradeDate]);
                        var qty = rowstring[idqty].IndexOf(".") == -1
                                      ? Convert.ToInt64(rowstring[idqty])*side*MtyVolume
                                      : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)*side*MtyVolume;
                        var cp_id = getCPid(rowstring[idcp].Trim(), cpdic);

                        var category = rowstring[idcat];
                        var value = double.Parse(rowstring[idvalue], CultureInfo.InvariantCulture);
                        var exchFees = double.Parse(rowstring[idexchfees], CultureInfo.InvariantCulture);
                        var Fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture);
                        var exchangeOrderId = rowstring[idoftrade].TrimEnd();

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
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("LEK");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "LEK" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "LEK");
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private DateTime LekTradeUploading()
        {
            DialogResult result = openFileDialog2.ShowDialog();
            //  var idSymbol = 7;
            var idMacside = 5;
            var idAccount = 1;
            //    var idcurrency = 10;
            //     var idTradeDate = 2;
            //   var idqty = 6;
            var idcp = 8;
            //    var idprice = 9;
            var idTypeofTrade = 8;
            var iddeliverydate = 4;
            var idvalue = 11;
            //   var idexchfees = 12;
            //     var idfees = 13;
            var idoftrade = 0;
            var symbolmap = getMapping("Lek");
            // var idTypeofOption = 9;
            //  var idstrike = 20;
            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                var cMapping = (from ct in db.ColumnMappings
                                where ct.Brocker == "LEK" && ct.FileType == "CSV"
                                select ct).FirstOrDefault();

                var cpfromDb = from cp in db.counterparties
                               select cp;
                var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();

                var lineFromFile = reader.ReadLine();
                // Map symbolvalue;
                DateTime reportdate = new DateTime();
                if (lineFromFile != null)
                {
                    var rowstring = lineFromFile.Replace("\"", "").Split(Delimiter);
                    var contractrow =
                        from ct in db.Contracts
                        where ct.valid == 1
                        select ct;
                    var contractdetails = contractrow.ToDictionary(k => k.id, k => k);
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
                        var side = -1;
                        double price = 0;
                        var symbol_id = rowstring[(int) cMapping.cSymbol].TrimEnd();
                        var typeoftrade = rowstring[idTypeofTrade].TrimEnd();
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
                        var account_id = rowstring[idAccount].TrimEnd();

                        var ccy = rowstring[(int) cMapping.cCcy].TrimEnd();
                        var TradeDate = DateTime.ParseExact(rowstring[(int) cMapping.cTradeDate], cMapping.DateFormat,
                                                            CultureInfo.CurrentCulture);
                        var qty = rowstring[(int) cMapping.cQty].IndexOf(".") == -1
                                      ? Convert.ToInt64(rowstring[(int) cMapping.cQty])*side*MtyVolume
                                      : double.Parse(rowstring[(int) cMapping.cQty], CultureInfo.InvariantCulture)*side*
                                        MtyVolume;
                        var cp_id = getCPid(rowstring[idcp].Trim(), cpdic);
                        var exchFees = double.Parse(rowstring[(int) cMapping.cExchangeFees],
                                                    CultureInfo.InvariantCulture);
                        var value =
                            Math.Round(
                                -side*double.Parse(rowstring[(int) cMapping.cValue], CultureInfo.InvariantCulture), 2,
                                MidpointRounding.AwayFromZero);
                        var Fees = double.Parse(rowstring[(int) cMapping.cFee], CultureInfo.InvariantCulture);
                        var exchangeOrderId = rowstring[idoftrade].TrimEnd();
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
                var i = 0;
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    db.CpTrades.Add(tradeIndex);
                    i++;
                }

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                                   validationError.ErrorMessage);
                        }
                    }
                }
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
            var key = symbol_id;

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
            var key = symbol_id;
            var type = "";
            var strike = "";
            typeoftrade = "FU";
            if (symbol_id.Contains("CALL") || symbol_id.Contains("PUT"))
            {
                type = symbol_id.Substring(0, symbol_id.IndexOf(" ")).Substring(0, 1);
                key = key.Substring(symbol_id.IndexOf(" ") + 1);
                typeoftrade = "OP";
            }
            var nextspace = key.IndexOf(" ");
            var month = key.Substring(0, nextspace);
            key = key.Substring(nextspace + 1);
            nextspace = key.IndexOf(" ");
            var year = "20" + key.Substring(0, nextspace);
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
                    var Digitmonth = GetMonthFromLetter(GetMonthLetter(month));
                    if (Digitmonth < 10) month = "0" + Digitmonth;
                    var db = new EXANTE_Entities(_currentConnection);
                    var t = "update Ctrades SET value_date= '" + year + "-" + month + "-01' where symbol_id='" + key +
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
            var t = symbol_id.Substring(CustomIndexOf(symbol_id, ' ', 3) + 1,
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
                var index = symbol_id.IndexOf(" ", System.StringComparison.Ordinal);
                index = symbol_id.IndexOf(" ", index + 1, System.StringComparison.Ordinal);
                var daystr = symbol_id.Substring(index + 1, 2);
                var daystr2 = Convert.ToInt16(daystr);
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
                var t = GetMonthLetter(month);
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

        internal class Trade
        {
            public double qty { get; set; }
            public long id { get; set; }
        }

        internal class FullTrade
        {
            public string Account { get; set; }
            public string Symbol { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
            public double Value { get; set; }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var db = new EXANTE_Entities(_currentConnection);
            var reportdate = new DateTime(2012, 05, 14);
            var prevdate = new DateTime(2012, 05, 04);
            DateTime TimeStart = DateTime.Now;
            var ftboitems =
                (from ct in db.Ftboes
                 where
                     ct.botimestamp >= prevdate && ct.botimestamp <= reportdate &&
                     (ct.symbolId == "" || ct.symbolId == null) && ct.tradeNumber != null
                 select ct).ToList();
            //ToDictionary(k => (k.tradeNumber.ToString()+k.gatewayId), k => k);
            var index = 0;
            var ctradeitems =
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
                    db.Entry(ftbo).State = (System.Data.Entity.EntityState) EntityState.Modified;
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
            var objects = JArray.Parse(json); // parse as array  
            foreach (JObject root in objects)
            {
                foreach (KeyValuePair<String, JToken> app in root)
                {
                    var appName = app.Key;
                    var description = (String) app.Value["Description"];
                    var value = (String) app.Value["Value"];
                }
            }
        }

        private static string ClearString(string str)
        {
            str = str.Trim();

            var ind0 = str.IndexOf("\"");
            var ind1 = str.LastIndexOf("\"");

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
            var lines = res.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var ht = new Dictionary<string, string>(20);
            var st = new Stack<string>(20);

            for (int i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                var pair = line.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length == 2)
                {
                    var key = ClearString(pair[0]);
                    var val = ClearString(pair[1]);

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
            BOjson p = new BOjson();
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
            //  p.settlementCounterparty = cptrade.BOcp;
            p.settlementCounterparty = cptrade.Comment;
            //  p.brokerAccountId = accountclientid;
            //  p.comment = cptrade.BOTradeNumber;
            p.internalComment = cptrade.exchangeOrderId;
            //p.commission = (-cptrade.ExchangeFees).ToString();
            // p.commissionCurrency = "USD";
            p.takeCommission = true;
            //   p.takeCommission = false;
            //      p.comment = "Correct reversal of trade dd " + ((DateTime)cptrade.TradeDate).ToString("dd.MM.yyyy");
            p.redemption = false;
            p.isManual = true;
            return p;
        }

        private FTjson FeeJsonfromCpTrade(CpTrade cptrade)
        {
            FTjson p = new FTjson();
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

        private string GetToken(string connectionstring, string service)
        {
            Uri DBurl = new Uri(connectionstring);
            HttpWebRequest dbReq = WebRequest.Create(DBurl) as HttpWebRequest;
            dbReq.ContentType = "application/json";
            dbReq.UserAgent = "curl/7.37.0";
            var credential = getcredentials("prod");
            string requestokenstr = "{\"username\":\"" + credential[0] + "\", \"password\" : \"" + credential[1] +
                                    "\",\"service\":\"";
            string requestoken = requestokenstr + service + "\"}";
            dbReq.Method = "POST";
            UTF8Encoding encoding = new UTF8Encoding();
            dbReq.ContentLength = encoding.GetByteCount(requestoken);
            var token = "";
            using (Stream requestStream = dbReq.GetRequestStream())
            {
                requestStream.Write(encoding.GetBytes(requestoken), 0,
                                    encoding.GetByteCount(requestoken));
            }
            try
            {
                HttpWebResponse response = dbReq.GetResponse() as HttpWebResponse;
                string responseBody = "";
                using (Stream rspStm = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(rspStm))
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
            catch (System.Net.WebException ex)
            {
                LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                StreamReader reader = new StreamReader(ex.Response.GetResponseStream());
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
                var text = reader.ReadLine().Split(';');
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
            var acc = GetAccount();
            var sendFee = true;
            var sendPL = false;
            var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            if (!checkBoxAllDates.Checked)
            {
                var reportdate = ABNDate.Value;
                postTradesforDate(acc, reportdate, sendFee, sendPL, token, conStr, acc.BOaccountId, null);
            }
            else
            {
                var reportdate = ABNDate.Value;
                var enddate = DateTime.Today;
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
            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = from Cptrade in db.CpTrades
                                where Cptrade.valid == 1 && Cptrade.BrokerId == Broker &&
                                      Cptrade.ReportDate >= reportdate.Date && Cptrade.ReportDate < (nextdate.Date)
                                //&& Cptrade.ReconAccount == null
                                select Cptrade;
            var cptradeitem = cptradefromDb.ToList();
            var tradesqty = 0;
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
                var FTfromDb = from ft in db.FT
                               where ft.valid == 1 && ft.brocker == acc.DBcpName &&
                                     ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                     ft.account_id == acc.BOaccountId && ft.Type == "PL"
                               select ft;
                var FTfromDbeitem = FTfromDb.ToList();
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
            var brockerlist = (from rec in db.DBBORecon_mapping
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
            FTjson p = new FTjson();
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
            FTjson p = new FTjson();
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
            var accountnumber = (from ctrade in db.Ctrades
                                 where ctrade.valid == 1 && ctrade.tradeNumber == tradenumber
                                 select ctrade.account_id).ToList()[0];
            db.Dispose();
            return accountnumber;
        }

        private bool SendJson(string requestPayload, string constr, string token)
        {
            Uri uri = new Uri(constr);
            UTF8Encoding encoding = new UTF8Encoding();
            var r = WebRequest.Create(uri) as HttpWebRequest;
            r.Method = "PUT";
            r.UserAgent = "curl/7.37.0";
            r.ContentLength = encoding.GetByteCount(requestPayload);
            r.Credentials = CredentialCache.DefaultCredentials;
            var credential = getcredentials("prod");
            NetworkCredential Credentials = new NetworkCredential(credential[0], credential[1]); //bo
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
                HttpWebResponse response = r.GetResponse() as HttpWebResponse;
                string responseBody = "";
                using (Stream rspStm = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(rspStm))
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
            catch (System.Net.WebException ex)
            {
                LogTextBox.Text = LogTextBox.Text + "\r\nException message: " + ex.Message;
                LogTextBox.Text = LogTextBox.Text + "\r\nResponse Status Code: " + ex.Status;
                LogTextBox.Text = LogTextBox.Text + "\r\n\r\n";
                // get error details sent from the server
                StreamReader reader = new StreamReader(ex.Response.GetResponseStream());
                LogTextBox.Text = LogTextBox.Text + reader.ReadToEnd();
                return false;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText(TimeStart + ": " + "Getting ccy prices from MOEX");
            // var FORTSDate = ABNDate.Value.ToString("dd.MM.yyyy");
            var FORTSDate = ABNDate.Value.ToString("dd.MM.yyyy");
            //  updateFORTSccyrates(FORTSDate);
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + "CCY FORTS rates for " + FORTSDate +
                                  " uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());

            calcualteVM(ABNDate.Value, "ATON");
            DateTime TimeEndVMCalculation = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndVMCalculation + ": " + "VM calculation " + FORTSDate +
                                  " completed. Time:" + (TimeEndVMCalculation - TimeEndUpdating).ToString());
        }

        private void aBNPositionParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    var reportdate = ABNDate.Value;
                    var cliffdict = LoadCliff(openFileDialog2.FileName, reportdate);
                    GetABNPos(cliffdict, reportdate);
                }
            }
        }

        private void aBNFTParsingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    var reportdate = ABNDate.Value;
                    var cliffdict = LoadCliff(oFilename, reportdate);
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
            var result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime TimeUpdateBalanceStart = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start FT BO uploading");

                    var reportdate = new DateTime(2011, 01, 01);
                    var db = new EXANTE_Entities(_currentConnection);
                    db.Database.CommandTimeout = 300;
                    var reader = new StreamReader(oFilename);
                    var allfromFile = new List<Ctrade>();
                    var lineFromFile = reader.ReadLine();
                    var index = 0;
                    var Rowindex = 0;
                    if (lineFromFile != null)
                    {
                        var rowstring = lineFromFile.Split(Delimiter);

                        int idid = 0;
                        int idaccountId = 0;
                        int idtimestamp = 0;
                        int idoperationType = 0;
                        int idasset = 0;
                        int idsum = 0;
                        int idwho = 0;
                        int idgatewayId = 0;
                        int idtradeNumber = 0;
                        int idcomment = 0;
                        int idinternalComment = 0;
                        int idsymbolId = 0;
                        int idvalueDate = 0;
                        int idorderId = 0;
                        int idorderPos = 0;
                        int idprice = 0;
                        int idclientType = 0;
                        int idexecutionCounterparty = 0;

                        for (var i = 0; i < rowstring.Length; i++)
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
                                case "gatewayId":
                                    idgatewayId = i;
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
                                case "symbolId":
                                    idsymbolId = i;
                                    break;
                                default:
                                    LogTextBox.AppendText("Additional fields in the FT.file!");
                                    break;
                            }
                        }
                        var checkId =
                            (from ct in db.Ftboes
                             where ct.botimestamp.ToString().Contains("2015-01")
                             select ct.id).ToDictionary(k => k, k => k);
                        //        select ct).ToDictionary(k => (k.tradeNumber.ToString()+k.gatewayId+k.asset+k.operationType), k => k); ;
                        while (!reader.EndOfStream)
                        {
                            Rowindex++;
                            lineFromFile = reader.ReadLine();
                            if (lineFromFile == null) continue;
                            rowstring = lineFromFile.Split(Delimiter);
                            //    string id = string.Concat(rowstring[idtradeNumber], rowstring[idgatewayId],rowstring[idasset],rowstring[idoperationType]);
                            var id = Convert.ToInt64(rowstring[idid]);
                            if (!checkId.ContainsKey(id))
                            {
                                index++;
                                db.Ftboes.Add(new Ftbo()
                                    {
                                        id = Convert.ToInt64(rowstring[idid]),
                                        accountId = rowstring[idaccountId],
                                        asset = rowstring[idasset],
                                        botimestamp = Convert.ToDateTime(rowstring[idtimestamp]),
                                        clientType = rowstring[idclientType],
                                        comment = rowstring[idcomment] + rowstring[idinternalComment],
                                        executionCounterparty = rowstring[idexecutionCounterparty],
                                        gatewayId = rowstring[idgatewayId],
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
                                    try
                                    {
                                        db.SaveChanges();
                                    }
                                    catch (DbEntityValidationException dbEx)
                                    {
                                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                                        {
                                            foreach (var validationError in validationErrors.ValidationErrors)
                                            {
                                                Trace.TraceInformation("Property: {0} Error: {1}",
                                                                       validationError.PropertyName,
                                                                       validationError.ErrorMessage);
                                            }
                                        }
                                    }
                                }
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
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                                       validationError.ErrorMessage);
                            }
                        }
                    }



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

            var symbolmap = getMapping(cp);
            var typemap =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == cp && ct.Type == "Type"
                 select ct).ToDictionary(k => k.BrockerSymbol, k => k.BOSymbol);

            var lCpTrade = new List<CpTrade>();
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = initTrade.Type;
                if (typemap.ContainsKey(initTrade.Type)) type = typemap[initTrade.Type];
                if (initTrade.Comment != null && initTrade.Comment.Contains("REPO")) type = "REPO";
                var Price = initTrade.Price;
                var Qty = initTrade.Qty;
                var value = initTrade.value;
                var ValueDate = initTrade.ValueDate;
                String BOSymbol = null;
                if (symbolmap.ContainsKey(initTrade.Symbol + type))
                {
                    var map = symbolmap[initTrade.Symbol + type];
                    BOSymbol = map.BOSymbol;
                    Price = Price*map.MtyPrice;
                    Qty = Qty*map.MtyVolume;
                    value = value*map.Leverage;
                    ValueDate = map.ValueDate;
                    type = map.Type;
                }
                //= GetSymbolLek(symbolmap, initTrade.Symbol, ref MtyVolume, contractdetails, ref MtyPrice, ref valuedate, ref Leverage);

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

            var symbolmap = getMapping("CFH");
            var typemap =
                (from ct in db.Mappings
                 where ct.valid == 1 && ct.Brocker == "CFH" && ct.Type == "Type"
                 select ct).ToDictionary(k => k.BrockerSymbol, k => k.BOSymbol);
            var lCpTrade = new List<CpTrade>();
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = "ST";
                if (typemap.ContainsKey(initTrade.Type)) type = typemap[initTrade.Type];
                var Price = initTrade.Price;
                var Qty = initTrade.Qty;
                var value = initTrade.value;
                var ValueDate = initTrade.ValueDate;
                String BOSymbol = null;
                if (symbolmap.ContainsKey(initTrade.Symbol))
                {
                    var map = symbolmap[initTrade.Symbol];
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
            FORTSReconciliation("OPEN");
        }

        private void FORTSReconciliation(string cp)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                var lInitTrades = OpenParsing(cp);
                var lCptrades = OpenConverting(lInitTrades, cp);
                foreach (CpTrade cptrade in lCptrades)
                {
                    cptrade.ReportDate = reportdate.Date;
                    if (cptrade.Type == "FUT") cptrade.Type = "FU";
                    if (cptrade.Type == "OPT") cptrade.Type = "OP";
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping(cp);
                //var symbolmap = getMap("OPEN");
                var cptradefromDb = from cptrade in db.CpTrades
                                    where
                                        cptrade.valid == 1 &&
                                        (cptrade.BrokerId == cp) && // || cptrade.BrokerId == "MOEX-SPECTRA") &&
                                        cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                        cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.Comment != null && cpTrade.Comment.Contains("REPO"))
                    {
                        var type = 1;
                    }


                    if (cpTrade.BOSymbol == null)
                    {
                        if (symbolmap.ContainsKey(cpTrade.Symbol + cpTrade.Type))
                        {
                            var map = symbolmap[cpTrade.Symbol + cpTrade.Type];
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
                            db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
                        }
                        else
                        {
                            var symbol = cpTrade.Symbol;
                            if (symbol.Contains("A ") && (cpTrade.Type != "REPO")) //indetify option
                            {
                                cpTrade.Type = "OP";
                                var keysymbol = symbol.Substring(0, symbol.IndexOf("-")) + "OP";
                                Map map;
                                if (symbolmap.TryGetValue(keysymbol, out map))
                                {
                                    var startindex = symbol.IndexOf("M", symbol.IndexOf("-"));
                                    var endindex = symbol.IndexOf(" ", startindex);
                                    cpTrade.ValueDate =
                                        DateTime.ParseExact(
                                            symbol.Substring(startindex + 1, endindex - 2 - (startindex + 1)), "ddMMyy",
                                            CultureInfo.CurrentCulture);
                                    var strikeindex = symbol.IndexOf("A ");
                                    var bosymbol = map.BOSymbol + ".";
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
                        }
                    }
                }
                db.SaveChanges();
            }

            RecProcess(reportdate, cp);
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private List<InitialTrade> OpenParsing(string cp)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();
            if (result == DialogResult.OK) // Test result.
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start " + cp + " trades uploading");

                var db = new EXANTE_Entities(_currentConnection);
                var cMapping = (from ct in db.ColumnMappings
                                where ct.Brocker == cp && ct.FileType == "EXCEL"
                                select ct).ToDictionary(k => k.Type, k => k);
                if (cMapping["FU"].cTabName == null || CheckTabExist(openFileDialog2.FileName, cMapping["FU"].cTabName))
                    removeOverallRows(openFileDialog2.FileName, cMapping["FU"].cTabName, cMapping["FU"].cLineStart);
                List<InitialTrade> inittrades;
                if (cMapping.ContainsKey("ST") && cMapping["ST"].Brocker != "Renesource")
                {
                    inittrades = ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping["ST"]);
                    if (inittrades != null) lInitTrades.AddRange(inittrades);
                }
                /*   if (cMapping.ContainsKey("FX"))
                {
                    inittrades = ParseBrockerExcelToCpTrade(openFileDialog2.FileName, cMapping["FX"]);
                    if (inittrades != null) lInitTrades.AddRange(inittrades);
                }*/
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
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "",
                                                                                          false,
                                                                                          Microsoft.Office.Interop.Excel
                                                                                                   .XlPlatform.xlWindows,
                                                                                          "",
                                                                                          true, false, 0, true,
                                                                                          false, false);
            //Выбираетам таблицу(лист).
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
            if (name != null)
            {
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets[name];
            }
            else
            {
                ObjWorkSheet = ObjWorkBook.Worksheets[1];
            }
            Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
            var i = startline;
            while ((i <= xlRange.Rows.Count) &&
                   !((xlRange.Cells[i, 1].value2 == null) && (xlRange.Cells[i, 3].value2 == null)))
            {
                var t = xlRange.Cells[i, 1].value2;
                if ((xlRange.Cells[i, 1].value2 == null) || (xlRange.Cells[i, 3].value2 == null))
                {
                    xlRange.Rows[i].Delete();
                    i--;
                }
                i++;
            }
            ObjWorkBook.Close();
            ObjExcel.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
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
                var cMapping = (from ct in db.ColumnMappings
                                where ct.Brocker == "CFH" && ct.FileType == "EXCEL"
                                select ct).ToDictionary(k => k.Type, k => k);
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    //    var startline = getStartRowCFH(oFilename, cMapping["FX"].cTabName);
                    var startline = 2;
                    //if(startline!=-1)lInitTrades.AddRange(ParseBrockerExcelToCpTrade(oFilename, cMapping["FX"], startline));
                    if (startline != -1)
                        lInitTrades.AddRange(ParseBrockerExcelToCpTrade(oFilename, cMapping["ST"], startline));
                }
                foreach (var initialTrade in lInitTrades)
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
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(fileName, 0, false, 5, "", "",
                                                                                          false,
                                                                                          Microsoft.Office.Interop.Excel
                                                                                                   .XlPlatform.xlWindows,
                                                                                          "",
                                                                                          true, false, 0, true,
                                                                                          false, false);
            //Выбираетам таблицу(лист).
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
            try
            {
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets[tabname];
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return -1;
            }
            Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
            var i = 3;
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
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
            return i;
        }

        private bool CheckTabExist(string filename, string tabname)
        {
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(filename, 0, false, 5, "", "",
                                                                                          false,
                                                                                          Microsoft.Office.Interop.Excel
                                                                                                   .XlPlatform.xlWindows,
                                                                                          "",
                                                                                          true, false, 0, true,
                                                                                          false, false);
            //Выбираетам таблицу(лист).
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
            ObjWorkSheet =
                ObjWorkBook.Worksheets.Cast<Worksheet>().FirstOrDefault(worksheet => worksheet.Name == tabname);
            if (ObjWorkSheet != null)
            {
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                return true;

            }
            else
            {
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                return false;
            }

        }

        private List<InitialTrade> ParseBrockerExcelToCpTrade(string filename, ColumnMapping cMapping, int startline = 0)
        {
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            //Открываем книгу.                                                                                                                                                        
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(filename, 0, false, 5, "", "",
                                                                                          false,
                                                                                          Microsoft.Office.Interop.Excel
                                                                                                   .XlPlatform.xlWindows,
                                                                                          "",
                                                                                          true, false, 0, true,
                                                                                          false, false);
            //Выбираетам таблицу(лист).
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
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
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                var tradescounter = new Dictionary<DateTime, int>();
                var i = startline;
                if (startline == 0) i = (int) cMapping.cLineStart;
                var lInitTrades = new List<InitialTrade>();
                var n = xlRange.Rows.Count;
                var add = 0;
                if (i != 1)
                {
                    var curr = (string) xlRange.Cells[i - 1, 12].value2;
                    if ((curr != null) && (curr.IndexOf("Place of Settlement") > -1)) add = 1;
                }
                while (i <= n)
                {
                    if (xlRange.Cells[i, cMapping.cTradeDate].value2 != null)
                    {
                        DateTime tradeDate = getDate(cMapping.DateFormat, xlRange.Cells[i, cMapping.cTradeDate].value2);
                        var reportdate = cMapping.cReportDate != null
                                             ? getDate(cMapping.ReportDateFormat,
                                                       xlRange.Cells[i, cMapping.cReportDate].value2)
                                             : tradeDate.Date;
                        var valueDate = cMapping.cValuedate != null
                                            ? getDate(cMapping.ValueDateFormat,
                                                      xlRange.Cells[i, cMapping.cValuedate].value2)
                                            : null;
                        if (cMapping.cTradeTime != null)
                        {
                            var crtFormat = "HH:mm:ss";
                            var crtValue = xlRange.Cells[i, cMapping.cTradeTime].value2;
                            if (cMapping.TimeFormat != null)
                            {
                                crtFormat = cMapping.TimeFormat;
                            }
                            if (crtFormat.Length == 6)
                            {
                                var diffdigit = crtFormat.Length - crtValue.ToString().Length;
                                if (diffdigit > 0) crtValue = "0" + crtValue;
                            }
                            var time = DateFromExcelCell(crtValue, crtFormat);
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
                                var side = xlRange.Cells[i, cMapping.cSide].value2;
                                if (side != null)
                                {
                                    side = side.ToUpper();
                                    if ((side == "SELL") || (side == "S") || (side.Contains("ПРОДАЖА"))) qty = -qty;
                                }
                            }
                        }
                        else
                        {
                            qty = xlRange.Cells[i, cMapping.cQty].value2 - xlRange.Cells[i, cMapping.cQtySell].value2;
                        }

                        var Price = Math.Round(xlRange.Cells[i, cMapping.cPrice + add].value2, 10);
                        var ExchangeFees =
                            cMapping.cExchangeFees != null
                                ? xlRange.Cells[i, cMapping.cExchangeFees + add].value2
                                : null;
                        var Fee = cMapping.cFee != null ? xlRange.Cells[i, cMapping.cFee + add].value2 : null;
                        var Fee2 = cMapping.cFee2 != null ? xlRange.Cells[i, cMapping.cFee2 + add].value2 : null;
                        var Fee3 = cMapping.cFee3 != null ? xlRange.Cells[i, cMapping.cFee3 + add].value2 : null;
                        var value = cMapping.cValue != null ? xlRange.Cells[i, cMapping.cValue + add].value2 : null;
                        var exchangeOrderId =
                            cMapping.cExchangeOrderId != null
                                ? Convert.ToString(xlRange.Cells[i, cMapping.cExchangeOrderId].value2)
                                : null;
                        var Strike = cMapping.cStrike != null ? xlRange.Cells[i, cMapping.cStrike].value2 : null;
                        var AccruedInterest =
                            cMapping.cInterest != null ? xlRange.Cells[i, cMapping.cInterest].value2 : null;
                     //   var traddeid = xlRange.Cells[i, cMapping.cTradeId + add].value2;        

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
                                cMapping.cTradeId != null ? Convert.ToString(xlRange.Cells[i, cMapping.cTradeId + add].value2) : null

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

                foreach (InitialTrade initialTrade in lInitTrades)
                {
                    db.InitialTrades.Add(initialTrade);
                }
                db.SaveChanges();
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                LogTextBox.AppendText("\r\nTrades uploaded:");
                foreach (KeyValuePair<DateTime, int> pair in tradescounter)
                {
                    LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
                }
                //reportdate = tradescounter.FirstOrDefault().Key
                return lInitTrades;
            }
            else
            {

                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                return null;
            }
        }

        private dynamic getDate(string format, object rowDate)
        {
            if (format.Length == 8) rowDate = rowDate.ToString();
            var formatDate = DateFromExcelCell(rowDate, format);
            return formatDate;
        }

        private DateTime DateFromExcelCell(object t, string Dateformat)
        {
            if (t.GetType().Name == "String")
            {

                return DateTime.ParseExact(t as string, Dateformat, CultureInfo.CurrentCulture);
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start RJO trades uploading");
                var LInitTrades = TradeParsing("RJO", "CSV", "FU");
                var lCptrades = InitTradesConverting(LInitTrades, "RJO");
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("RJO");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                string type = "FU";
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "RJO" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "RJO");
        }

        private List<CpTrade> InitTradesConverting(List<InitialTrade> lInitTrades, string cp,bool checkIdflag=false,string checkIdCp = "")
        {
            DateTime TimeStartConvert = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStartConvert.ToLongTimeString() + ": " + "start " + cp +
                                  " trades Converting");
            var db = new EXANTE_Entities(_currentConnection);
            var symbolmap = getMapping(cp);
            var lCpTrade = new List<CpTrade>();
            Dictionary<string, long> checkId = null;
            if (checkIdflag)
            {
                
                checkId = (from ct in db.CpTrades
                           where ct.TradeDate.ToString().Contains("2016-0") && ct.BrokerId == checkIdCp
                           select ct).ToDictionary(k => k.exchangeOrderId,k => k.FullId);
            }
            foreach (InitialTrade initTrade in lInitTrades)
            {
                string type = "FU";
                if (initTrade.Type == "O") type = "OP";
                var Price = initTrade.Price;
                var Qty = initTrade.Qty;
                var value = initTrade.value;

                var ValueDate = initTrade.ValueDate;
                if (ValueDate == null) ValueDate = new DateTime(2011, 01, 01);
                String BOSymbol = null;
                var key = initTrade.Symbol + type; // +ValueDate.Value.ToShortDateString();
                if (symbolmap.ContainsKey(key))
                {
                    var map = symbolmap[key];
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

        private List<InitialTrade> TradeParsing(string brocker, string filetype, string mappingtype)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {
                //   var symbolmap = getMapping("RJO");
                var db = new EXANTE_Entities(_currentConnection);
                var cMapping = (from ct in db.ColumnMappings
                                where ct.Brocker == brocker && ct.FileType == filetype
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
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reader = new StreamReader(openFileDialog2.FileName);
            string lineFromFile;
            var contractrow =
                from ct in db.Contracts
                where ct.valid == 1
                select ct;
            //  var contractdetails = contractrow.ToDictionary(k => k.id, k => k);
            var i = 1;

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
                var rowstring = lineFromFile.Split(Convert.ToChar(cMapping.Delimeter));
                var tradeDate = cMapping.cTradeDate != null
                                    ? DateTime.ParseExact(rowstring[(int) cMapping.cTradeDate], cMapping.DateFormat,
                                                          CultureInfo.CurrentCulture)
                                    : new DateTime(2011, 01, 01);

                var reportdate = cMapping.cReportDate != null
                                     ? DateTime.ParseExact(rowstring[(int) cMapping.cReportDate],
                                                           cMapping.ReportDateFormat, CultureInfo.CurrentCulture)
                                     : tradeDate;
                //     var reportdate = DateTime.ParseExact(rowstring[(int)cMapping.cReportDate], cMapping.DateFormat, CultureInfo.CurrentCulture);
                if (cMapping.cTradeTime != null)
                {
                    var time = DateTime.ParseExact(rowstring[(int) cMapping.cTradeTime], "HH:mm:ss",
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
                    if (rowstring[(int) cMapping.cSide].ToString() == "2") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToString().ToUpper() == "SELL") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToString().ToUpper() == "SLD") qty = -qty;
                    if (rowstring[(int) cMapping.cSide].ToString().ToUpper() == "S") qty = -qty;

                }
                var symbol_id = rowstring[(int) cMapping.cSymbol].TrimEnd();

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

                var ReportDate = reportdate;
                var TradeDate = tradeDate;
                var BrokerId = cMapping.cBrokerId != null ? rowstring[(int) cMapping.cBrokerId] : cMapping.Brocker;
                var Symbol = symbol_id;
                var Qty = qty;
                var Price = price;
                var ValueDate = cMapping.cValuedate != null
                                    ? DateTime.ParseExact(rowstring[(int) cMapping.cValuedate],
                                                          cMapping.ValueDateFormat,
                                                          CultureInfo.CurrentCulture)
                                    : (DateTime?) null;
                var ExchangeFees =
                    cMapping.cExchangeFees != null
                        ? double.Parse(rowstring[(int) cMapping.cExchangeFees], CultureInfo.InvariantCulture)
                        : (double?) null;
                var Fee22 = Fee;
                var TypeOfTrade = cMapping.cTypeOfTrade != null ? rowstring[(int) cMapping.cTypeOfTrade] : null;
                var Type = cMapping.cType != null ? rowstring[(int)cMapping.cType] : cMapping.Type;
                var value2 = value;
                var Timestamp = DateTime.UtcNow;
                var exchangeOrderId =
                    cMapping.cExchangeOrderId != null
                        ? Convert.ToString(rowstring[(int) cMapping.cExchangeOrderId])
                        : null;
                var Comment = cMapping.cComment != null ? rowstring[(int) cMapping.cComment] : null;
                var ExchFeeCcy =
                    cMapping.cExchFeeCcy != null ? rowstring[(int) cMapping.cExchFeeCcy].TrimEnd() : null;
                var ClearingFeeCcy =
                    cMapping.cClearingFeeCcy != null
                        ? rowstring[(int) cMapping.cClearingFeeCcy].TrimEnd()
                        : null;
                var ccy = cMapping.cCcy != null ? rowstring[(int) cMapping.cCcy].TrimEnd() : null;
                var Strike =
                    cMapping.cStrike != null
                        ? double.Parse(rowstring[(int) cMapping.cStrike], CultureInfo.InvariantCulture)
                        : (double?) null;
                var OptionType =
                    cMapping.cOptionType != null ? rowstring[(int) cMapping.cOptionType].TrimEnd() : null;
                var Fee2 =
                    cMapping.cFee2 != null
                        ? double.Parse(rowstring[(int) cMapping.cFee2], CultureInfo.InvariantCulture)
                        : (double?) null;
                var Fee3 =
                    cMapping.cFee3 != null
                        ? double.Parse(rowstring[(int) cMapping.cFee3], CultureInfo.InvariantCulture)
                        : (double?) null;

                var test = cMapping.cAccount != null
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
                        Type = cMapping.cType != null ? rowstring[(int)cMapping.cType] : cMapping.Type,
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
                        cMapping.cTradeId != null ? rowstring[(int)cMapping.cTradeId] : null

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
            db.SaveChanges();
            db.Dispose();
            LogTextBox.AppendText("\r\nTrades uploaded:");
            foreach (KeyValuePair<DateTime, int> pair in tradescounter)
            {
                LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
            }
            return lInitTrades;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //   var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            var token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice");

            var reportdate = ABNDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from ft in db.FT
                                 where ft.valid == 1 &&
                                       (
                                         ft.brocker=="MOEX"
                                        /*  ft.brocker == "INSTANT" || ft.brocker == "EXANTE" ||
                                           ft.brocker == "MOEX-SPECTRA" ||
                                           ft.brocker == "OPEN"*/
                                       ) &&
                                       ft.Type == "VM" &&
                                       ft.ReportDate >= reportdate.Date && ft.ReportDate < (nextdate.Date) &&
                                       ft.ValueCCY != 0
                                       && ft.Reference == null
                                 group ft by new {ft.account_id, ft.symbol, ft.Type, ft.ccy, ft.counterccy}
                                 into g
                                 select new
                                     {
                                         account_id = g.Key.account_id,
                                         symbol = g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         type = g.Key.Type,
                                         ccy = g.Key.ccy,
                                         counterccy = g.Key.counterccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            var tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                if (Math.Abs((double) VARIABLE.value) > 0.0099)
                {
                    FTjson p = new FTjson();
                    if (VARIABLE.type == "VM")
                    {
                        p.operationType = "VARIATION MARGIN";
                        p.comment = "VM " + VARIABLE.BOSymbol + " for " + reportdate.ToShortDateString();
                        p.asset = "USD";
                    }
                    else
                    {
                        p.operationType = "VARIATION MARGIN";
                        p.comment = "Additional fees from cp:  " + VARIABLE.BOSymbol + "  for" +
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
                    FTjson p2 = new FTjson();
                    p2.operationType = "VARIATION MARGIN";
                    p2.symbolId = VARIABLE.BOSymbol;
                    p2.asset = VARIABLE.ccy;
                    p2.amount = Math.Round((double) VARIABLE.value, 2).ToString();
                    p2.timestamp = reportdate.ToString("yyyy-MM-dd HH:mm:ss");
                    p2.comment = "VM " + VARIABLE.BOSymbol + " for " + reportdate.ToShortDateString();
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
            DateTime reportDate = ABNDate.Value;
            if (result == DialogResult.OK)
            {
                foreach (string oFilename in openFileDialog2.FileNames)
                {
                    DateTime TimeUpdateBalanceStart = DateTime.Now;
                    LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start FT Balance uploading for ");


                    Microsoft.Office.Interop.Excel.Application ObjExcel =
                        new Microsoft.Office.Interop.Excel.Application();
                    //Открываем книгу.                                                                                                                                                        
                    Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(oFilename,
                                                                                                  0, false, 5, "", "",
                                                                                                  false,
                                                                                                  Microsoft.Office
                                                                                                           .Interop
                                                                                                           .Excel
                                                                                                           .XlPlatform
                                                                                                           .xlWindows,
                                                                                                  "",
                                                                                                  true, false, 0, true,
                                                                                                  false, false);
                    //Выбираем таблицу(лист).
                    Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                    ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Sheet1"];
                    Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                    IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
                    var jj = 1;
                    var account = xlRange.Cells[5 + jj, 2].value2.ToString();
                    int idReportDate = 1,
                        idLabel = 2,
                        idPrice = 3,
                        idOpType = 4,
                        idDebit = 5,
                        idCredit = 6;
                    var ccy = "";
                    ccy = xlRange.Cells[8 + jj, 2].value2;
                    LogTextBox.AppendText(ccy);
                    var i = 13 + jj;
                    var index = 0;
                    var tempreportdate = xlRange.Cells[i, idReportDate].value2;
                    if (tempreportdate != null)
                    {
                        reportDate = DateTime.ParseExact(xlRange.Cells[i, idReportDate].value2.ToString(), "dd/MM/yyyy",
                                                         theCultureInfo);
                    }
                    else
                    {
                        reportDate = ABNDate.Value.Date;
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
                                type = xlRange.Cells[i, idLabel].value2;
                                type = type.Substring(type.IndexOf('/') + 1, 4);
                                if (type == "ESWP") type = "Swap";
                                if (type == "ADSS" && xlRange.Cells[i, idOpType].value2 == "Trade") type = "Trade";
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
                                         : (int) 0) -
                                    (xlRange.Cells[i, idDebit].value2 != null
                                         ? Convert.ToDouble(xlRange.Cells[i, idDebit].Text.ToString().Replace(" ", ""))
                                         : (int) 0),
                                Comment = xlRange.Cells[i, idLabel].value2 + ";" + xlRange.Cells[i, idPrice].value2,
                                timestamp = DateTime.UtcNow,
                                valid = 1,
                                User = "script"
                            });
                        i++;
                        SaveDBChanges(ref db);
                        index++;
                    }
                    var OpenCash = Convert.ToDouble(xlRange.Cells[10 + jj, 2].value2);
                    var CloseCash = Convert.ToDouble(xlRange.Cells[i + 1, 2].value2);
                    var OpenCashFromDb = GetCloseCashFromPrevDate(db, ccy, "ADSS");
                    var comment = "";
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

                    var todelete = from ft in db.ADSSCashGroupped
                                   where ft.Currency == ccy && reportDate.Date == ft.ReportDate && ft.Cp == "ADSS"
                                   select ft;
                    db.ADSSCashGroupped.RemoveRange(todelete);
                    db.SaveChanges();

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
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
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
                foreach (var eve in er.EntityValidationErrors)
                {
                    Console.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
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
            var OpenCashFromDb = (from ft in db.ADSSCashGroupped
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
            var reportDate = (from ft in db.ADSSCashGroupped
                              where ft.Cp == cp
                              orderby ft.ReportDate descending
                              select ft.ReportDate).ToList()[0];

            var prevreportDate = (from ft in db.ADSSCashGroupped
                                  where ft.ReportDate < reportDate.Date && ft.Cp == cp
                                  orderby ft.ReportDate descending
                                  select ft.ReportDate).ToList()[0];

            var listCcyReportdate = (from ft in db.ADSSCashGroupped
                                     where ft.ReportDate == reportDate.Date && ft.Cp == cp
                                     select ft.Currency).ToList();
            var PreviousReport = (from ft in db.ADSSCashGroupped
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
            db.SaveChanges();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            DialogResult result = openFileDialog2.ShowDialog();
            DateTime reportDate = ABNDate.Value;
            if (result == DialogResult.OK)
            {
                DateTime TimeUpdateBalanceStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeUpdateBalanceStart + ": " + "start MAC Balance uploading for ");
                int idccy = 4,
                    idCashGroup = 2,
                    idType = 3,
                    idValue = 5;
                var ccy = "";
                LogTextBox.AppendText(ccy);
                var reader = new StreamReader(openFileDialog2.FileName);
                var filedata = new Dictionary<string, List<string[]>>();
                while (!reader.EndOfStream)
                {
                    var lineFile = reader.ReadLine();
                    var splitstring = lineFile.Replace("\"", "").Split(CSVDelimeter);
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

                foreach (KeyValuePair<string, List<string[]>> pair in filedata)
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
                    var comment = "";
                    foreach (var item in pair.Value)
                    {
                        //   var account = item[idaccount];
                        var CashGroup = item[idCashGroup].Trim();
                        var value = double.Parse(item[idValue], CultureInfo.InvariantCulture);
                        var type = item[idType].Trim();
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
                    var todelete = from ft in db.ADSSCashGroupped
                                   where
                                       ft.Currency == pair.Key && reportDate.Date == ft.ReportDate &&
                                       ft.Cp == "Mac"
                                   select ft;

                    db.ADSSCashGroupped.RemoveRange(todelete);
                    SaveDBChanges(ref db);
                    var prevclose = GetCloseCashFromPrevDate(db, pair.Key, "Mac");
                    var closebalance =
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
            var listtodelete = from ft in db.FT
                               where ft.ccy == ccy && ft.cp == account && reportDate.Date == ft.ReportDate
                               select ft;
            db.FT.RemoveRange(listtodelete);
            db.SaveChanges();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //   var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            var token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice");

            var reportdate = ABNDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = reportdate.AddDays(1);
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
                                         account_id = g.Key.account_id,
                                         symbol = g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         ccy = g.Key.ccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            var tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                FTjson p = new FTjson();
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
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                                                              0, false,
                                                                                              5, "", "",
                                                                                              false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel
                                                                                                       .XlPlatform
                                                                                                       .xlWindows,
                                                                                              "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                string account = xlRange.Cells[11, 8].value2;
                if (account == null) account = xlRange.Cells[12, 7].value2;
                var ccy = xlRange.Cells[14, 8].value2;
                if (ccy == null) ccy = xlRange.Cells[15, 7].value2;
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Cash flow"];
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
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "OPEN Balance Completed for+" +
                                      reportdate.ToShortDateString() + openFileDialog2.FileName);
            }
        }

        private static void RemoveRecordFromRowBalance(EXANTE_Entities db, DateTime reportdate, string cp,
                                                       string account)
        {
            var todelete = from ft in db.RowBalance
                           where ft.cp == cp && ft.ReportDate == reportdate.Date && ft.account == account
                           select ft;
            db.RowBalance.RemoveRange(todelete);
            SaveDBChanges(ref db);
        }

        private static void RemoveRecordFromRowBalanceCcy(EXANTE_Entities db, DateTime reportdate, string cp, string ccy)
        {
            var todelete = from ft in db.RowBalance
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
                var add = 0;
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
            ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Cash flow"];
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
                var openvalue =
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
            ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
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
                var prevNav = (from ft in db.RowBalance
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
            ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Portfolio evaluation"];
            xlRange = ObjWorkSheet.UsedRange;
            int i = 15;
            var add = 0;
            var currsubject = Convert.ToString(xlRange.Cells[18, 5].value2);
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);

            if (!noparsingCheckbox.Checked)
            {
                var lInitTrades = CFHParsing();
                var lCptrades = OpenConverting(lInitTrades, "CFH");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                db.SaveChanges();
            }
            else
            {
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("CFH");
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "CFH" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        var map = symbolmap[cpTrade.Symbol];
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
                        db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }

            RecProcess(reportdate, "CFH");
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
            IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
            var startline = 2;
            var idfee = 11;
            var idFeeCcy = 12;
            var idDate = 3;
            var idpnl = 15;
            var idpnlccy = 16;
            var idType = 6;
            DateTime TimeStart = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start CFH Balance uploading");
            var reportdate = ABNDate.Value.Date;
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(ofilename, 0, false, 5, "", "",
                                                                                          false,
                                                                                          Microsoft.Office.Interop.Excel
                                                                                                   .XlPlatform.xlWindows,
                                                                                          "", true, false, 0, true,
                                                                                          false, false);
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
            ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Trade Blotter"];
            Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
            var i = startline;
            var type = "";
            var ccy = "USD";
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
            var temp = from r in db.RowBalance
                       where r.cp == "CFH"
                             && r.ccy.Contains(ccy) && r.Type == "Close balance"
                       select r;
            double? openbalance = 0;
            if (temp.Count() > 0)
            {
                var lastreportdate = temp.Max(o => o.ReportDate).Value.Date;
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

            var cashmovement =
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
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
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
            var ctradeslist = (from r in db.Ctrades
                               where r.BOtradeTimestamp.ToString().Contains("2016-01") && r.valid == 1
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
            var i = 0;
            var allcptrades = (from cp in db.CpTrades
                               where
                                   cp.TradeDate.ToString().Contains("2016-01") && cp.valid == 1 &&
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
            //foreach()

            foreach (cpCost_cTrade ctrade in ctradeslist)
            {
                i++;
                var trnumber = ctrade.tradeNumber.ToString();
                /* if (trnumber == "30123135")
                 {
                     var t = 1;
                 }*/
                var cptrades = allcptrades.Where(cp => cp.BOTradeNumber.Contains(trnumber)); //.ToList();
                var listcptrades = cptrades.ToList();
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
                        ExchFee = -(double) (ExchFee*Math.Abs((double) ctrade.qty)/sumQty);
                        cpFee = -(double) (cpFee*Math.Abs((double) ctrade.qty)/sumQty);
                    }
                    else
                    {
                        ExchFee = -(double) (ExchFee);
                        cpFee = -(double) (cpFee);
                    }
                    item = listcptrades[0];
                }
                var id = ctrade.account_id + ctrade.symbol_id + ctrade.cp_id;
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
                            Date = new DateTime(2016, 01, 1),
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
            foreach (KeyValuePair<string, CpCost> pair in dCpCost)
            {
                db.CpCost.Add(pair.Value);
                SaveDBChanges(ref db);
            }
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
                var cMapping = (from ct in db.ColumnMappings
                                where ct.Brocker == "OPEN" && ct.FileType == "EXCEL"
                                select ct).ToDictionary(k => k.Type, k => k);
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog2.FileName,
                                                                                              0, false, 5, "", "",
                                                                                              false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel
                                                                                                       .XlPlatform
                                                                                                       .xlWindows,
                                                                                              "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets[cMapping["ST"].cTabName];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;
                var tradescounter = new Dictionary<DateTime, int>();
                var i = cMapping["ST"].cLineStart;
                var n = xlRange.Rows.Count;
                var numberofchanges = 0;
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
                                db.Entry(currcptrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
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
            var reportdate = new DateTime(2012, 05, 14);
            var prevdate = new DateTime(2012, 05, 04);
            DateTime TimeStart = DateTime.Now;
            var ftboitems =
                (from ct in db.Ftboes
                 where
                     ct.botimestamp >= prevdate && ct.botimestamp <= reportdate &&
                     (ct.symbolId == "" || ct.symbolId == null) && ct.tradeNumber != null
                 select ct).ToList();
            var index = 0;
            var ctradeitems =
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
                    db.Entry(ftbo).State = (System.Data.Entity.EntityState) EntityState.Modified;
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
            DateTime reportdate = ABNDate.Value; //todo Get report date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start NISSAN trades uploading");
                var LInitTrades = TradeParsing("NISSAN", "CSV", "FU");
                //**
                var lCptrades = InitTradesConverting(LInitTrades, "NISSAN");
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("NISSAN");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "NISSAN" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "NISSAN");
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac position uploading");

                var LInitPos = TradeParsing("Mac", "CSV", "PO");


                DateTime TimeEnd = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + "Mac position uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
            }
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button12_Click_2(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start IS-PRIME trades uploading");

                var LInitTrades = TradeParsing("IS-PRIME", "CSV", "FX");
                var lCptrades = InitTradesConverting(LInitTrades, "IS-PRIME");
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

            RecProcess(reportdate, "IS-PRIME");
            db.Database.ExecuteSqlCommand("UPDATE CpTrades Set value = -Qty*Price WHERE BrokerId LIKE '%is-%'");
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac trades uploading");

                var LInitTrades = TradeParsing("MAC_EMIR", "CSV", "FU");
                var lCptrades = InitTradesConverting(LInitTrades, "IS-PRIME");
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

            RecProcess(reportdate, "IS-PRIME");
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void ParseBrockerCsvToEmir(string filename, Dictionary<string, Emir_Mapping> cMapping)
        {
            var tradescounter = new Dictionary<DateTime, int>();
            var lInitTrades = new List<Emir>();
            var db = new EXANTE_Entities(_currentConnection);
            var cpfromDb = from cp in db.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reader = new StreamReader(openFileDialog2.FileName);
            string lineFromFile;
            var contractrow =
                from ct in db.Contracts
                where ct.valid == 1
                select ct;
            var i = 1;
            var parameters = cMapping.First().Value;
            while ((i < parameters.cLineStart) && (!reader.EndOfStream))
            {
                lineFromFile = reader.ReadLine();
                i++;
            }
            while (!reader.EndOfStream)
            {
                lineFromFile = reader.ReadLine();

                var rowstring = lineFromFile.Split(Convert.ToChar(parameters.Delimeter));
                DateTime cpValueDate;
                if (rowstring[6].Length == 4)
                {
                    cpValueDate = DateTime.ParseExact(rowstring[6], "yyMM", CultureInfo.CurrentCulture);
                }
                else
                {
                    cpValueDate = DateTime.ParseExact(rowstring[6], "yyyyMMdd", CultureInfo.CurrentCulture);
                }
                var map_id = rowstring[5];
                if (rowstring[7] == "O")
                {
                    map_id = map_id + "OP";
                }
                map_id = map_id + cpValueDate.ToShortDateString();
                var map = cMapping[map_id];
                var timedifference = new TimeSpan((int) map.TimeDifference, 0, 0);
                var Buy___Sell_Indicator = rowstring[(int) parameters.cBuySell];
                var Instrument_ID_Taxonomy = map.InstrumentIDTaxonomy;
                var Instrument_ID = map.InstrumentID;
                var Instrument_Classification = map.InstrumentClassification;
                var Underlying_Instrument_ID = map.InstrumentType;
                var Notional_Currency_1 = map.NotionalCurrency1;
                var Deliverable_Currency = map.DeliverableCurrency;
                var UTI = rowstring[24] + rowstring[25];
                var MiFID_Transaction_Reference_Number = rowstring[28];
                var Venue_ID = map.VenueId;
                var Price___Rate = (Convert.ToDouble(rowstring[13]) + Convert.ToDouble(rowstring[12]))*map.CpMtyPrice;
                var Price_Notation = map.PriceNotation;
                var Price_Multiplier = map.PriceMultiplier.ToString();
                var Notional =
                    (map.CpMtyPrice*map.PriceMultiplier*Convert.ToDouble(rowstring[11])*
                     (Convert.ToDouble(rowstring[12]) + Convert.ToDouble(rowstring[13]))).ToString();
                var Quantity = rowstring[11];
                var Delivery_Type = map.DeliveryType;
                var Execution_Timestamp = Convert.ToDateTime(rowstring[27]) - timedifference;
                var Effective_Date = Convert.ToDateTime(rowstring[0]);
                var Maturity_Date = map.MaturityDate;
                var Confirmation_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference;
                var Clearing_Timestamp = Convert.ToDateTime(rowstring[26]) - timedifference;
                var CCP_ID = parameters.cp;
                var Floating_Rate_Payment_Frequency = map.FloatingRatePaymentFrequency;
                var Floating_Rate_Reset_Frequency = map.FloatingRateResetFrequency;
                var Floating_Rate_Leg_2 = map.FloatingRateLeg2;
                var Currency_2 = map.Currency2;
                var Exchange_Rate_Basis = map.ExchangeRateBasis;
                var Commodity_Base = map.CommodityBase;
                var Commodity_Details = map.CommodityDetails;
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
                    Put_Call = rowstring[(int) map.cPutCall].ToString();
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
                        Buy___Sell_Indicator = rowstring[(int) parameters.cBuySell],
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
            foreach (KeyValuePair<DateTime, int> pair in tradescounter)
            {
                LogTextBox.AppendText("\r\n" + pair.Key.ToShortDateString() + ":" + pair.Value);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Mac Emir uploading");

                DialogResult result = openFileDialog2.ShowDialog();
                if (result == DialogResult.OK) // Test result.
                {
                    var cMapping = (from ct in db.Emir_Mapping
                                    where ct.Brocker == "Mac" && ct.filetype == "CSV"
                                    select ct).ToDictionary(
                                        k =>
                                        removeNewlineSymbols(k.CpSymbol + k.OptionType +
                                                             k.CPValueDate.Value.ToShortDateString()), k => k);

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
            var token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice");

            var reportdate = ABNDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = reportdate.AddDays(1);
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
                                         account_id = g.Key.account_id,
                                         symbol = g.Key.symbol,
                                         BOSymbol = g.Key.symbol,
                                         value = g.Sum(t => t.value),
                                         ccy = g.Key.ccy,
                                         ValueCCY = g.Sum(t => t.ValueCCY)
                                     }).ToList();
            var tradesqty = 0;
            foreach (var VARIABLE in cptradefromDb)
            {
                FTjson p = new FTjson();
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);

            var checkId =
                (from ct in db.CpTrades
                 where ct.TradeDate.ToString().Contains("2016-0") && ct.BrokerId == "Belarta"
                 select ct).ToDictionary(k => (k.exchangeOrderId.ToString() + (Math.Sign((double) k.Qty)).ToString()),
                                         k => k.FullId);

            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Belarta trades uploading");

                var LInitTrades = TradeParsing("Belarta", "EXCEL", "FX");
                var lCptrades = InitTradesConverting(LInitTrades, "Belarta");
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

            RecProcess(reportdate, "Belarta");
            db.Dispose();
            TradesParserStatus.Text = "Done";
        }

        private void button18_Click(object sender, EventArgs e)
        {
            //   const string conStr = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            //var strZamTransaction = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ZAM1452.001/transaction";
            //    var strAdsTrade = "https://backoffice-recon.exante.eu:443/api/v1.5/accounts/ADS1450.002/trade";
            const string conStr = "https://backoffice.exante.eu:443/api/v1.5/accounts/"; // "ZAM1452.001/trade";
            var token = GetToken("https://authdb.exante.eu/api/1.0/auth/session", "backoffice");

            var reportdate = ABNDate.Value;
            var acc = new BOaccount
                {
                    accountNameCP = null, // "EXANTE",
                    //   BOaccountId = "FQJ5082.001", // "ELC5351.001",UGN6015.001, "FQJ5082.001"
                    //  DBcpName = "Belarta"
                };


            //        var account = "FQJ5082.001";// "ELC5351.001",
            var broker = "Belarta";
            var sendFee = false;
            //  var token = GetToken("https://authdb-recon.exante.eu/api/1.0/auth/session", "backoffice");
            var db = new EXANTE_Entities(_currentConnection);
            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = from Cptrade in db.CpTrades
                                where Cptrade.valid == 1 && Cptrade.BrokerId == broker &&
                                      Cptrade.ReportDate >= reportdate.Date && Cptrade.ReportDate < (nextdate.Date)
                                      && Cptrade.ReconAccount == null
                                select Cptrade;
            var cptradeitem = cptradefromDb.ToList();
            var tradesqty = 0;

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
                var lineFromFile = reader.ReadLine();
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
                            var tradedate = DateTime.ParseExact(lineFromFile.Substring(0, 8).Replace(" ", "0"),
                                                                "dd/MM/yy", CultureInfo.CurrentCulture);
                            var qty = OSLExtractQty(lineFromFile);
                            var symbol = lineFromFile.Substring(33, 32).TrimStart().TrimEnd();
                            var OptionType = lineFromFile.Substring(55, 1).Trim();
                            var OptionStrike = lineFromFile.Substring(57, 9).Trim();
                            var ccy = lineFromFile.Substring(94, 3);
                            var price = Convert.ToDouble(lineFromFile.Substring(72, 6).Trim());
                            var valuedate = DateTime.ParseExact(lineFromFile.Substring(33, 5), "MMMyy",
                                                                CultureInfo.CurrentCulture);
                            var ExchFeeCcy = "";
                            double ExchangeFees = 0;
                            var ClearingFeeCcy = "";
                            double Fee = 0;

                            lineFromFile = reader.ReadLine();
                            var vt = lineFromFile.Substring(2, 1);

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
                                    ReportDate = ABNDate.Value.Date,
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
            var longqty = lineFromFile.Substring(10, 6).Replace(" ", "");
            var shortqty = lineFromFile.Substring(18, 6).Replace(" ", "");
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
            var path = "c:/statement_dstm_20160310.pdf";

            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {

                PdfReader reader = new PdfReader(openFileDialog2.FileName);
                var db = new EXANTE_Entities(_currentConnection);
                var dbccylist = (from ccy in db.RJO_listccy
                                 where ccy.valid == 1
                                 select ccy.Ccy).ToList();
                var reportdate = ABNDate.Value;
                var count = reader.NumberOfPages;
                string txt = "";
                string currentaccount = "";
                // var results = new Dictionary<string,List<string>>
                for (var i = 1; i <= count; i++)
                {
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    currentaccount = getAccountofPage(txt);
                    var rows = txt.Split('\n');
                    var i_row = getStartCcy(rows, 1, dbccylist);
                    while ((i_row < rows.Length) && (i_row > 0))
                    {
                        var listofccy = Getlistofccy_modified(rows[i_row], ref dbccylist);
                        i_row++;
                        var cnttxt = rows[i_row].TrimStart();
                        while ((i_row < rows.Length) && (i_row != getStartCcy(rows, i_row, dbccylist)) &&
                               (cnttxt.Substring(0, 3) != "You") && (cnttxt.Substring(0, 3) != "+++"))
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
            }

        }

        private static int getStartCcy(string[] rows, int start, List<string> dbccylist)
        {
            var i_row = start;
            var found = false;
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
            var lastindexofstar = txt.IndexOf('*');
            var listofccy = new Dictionary<string, int>();
            while (lastindexofstar > -1)
            {
                var endstar = txt.IndexOf("*", lastindexofstar + 1);
                var cnt_ccy = txt.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd();
                listofccy.Add(txt.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd(),
                              endstar + 1);

                var match = ccy.FirstOrDefault(stringToCheck => stringToCheck.Contains(cnt_ccy));
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
            var indexofbeginning = txt.IndexOf("CONVERTED TO USD");
            var indexccy = txt.LastIndexOf("\n", indexofbeginning - 5);
            var ccys = txt.Substring(indexccy);
            ccys = ccys.Substring(0, ccys.IndexOf("\n", 3)).TrimEnd(); // , indexofbeginning - indexccy).TrimEnd();
            var lastindexofstar = ccys.IndexOf('*');
            var listofccy = new Dictionary<string, int>();
            while (lastindexofstar > -1)
            {
                var endstar = ccys.IndexOf("*", lastindexofstar + 1);
                listofccy.Add(ccys.Substring(lastindexofstar + 1, endstar - lastindexofstar - 1).TrimStart().TrimEnd(),
                              endstar);
                lastindexofstar = ccys.IndexOf("*", endstar + 1);
            }
            return listofccy;
        }

        private static string getAccountofPage(string txt)
        {
            var indexofaccount = txt.IndexOf("ACCOUNT NUMBER:") + 15;
            var test = txt.IndexOf("\n", indexofaccount);
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
            DialogResult result = openFileDialog2.ShowDialog();
            var lInitTrades = new List<InitialTrade>();

            if (result == DialogResult.OK) // Test result.
            {
                var db = new EXANTE_Entities(_currentConnection);
                var reader = new StreamReader(openFileDialog2.FileName);
                /*      HtmlWeb web = new HtmlWeb();
                     HtmlAgilityPack.HtmlDocument doc = web.Load("http://moex.com/ru/derivatives/currency-rate.aspx");
                     HtmlNodeCollection tags = doc.DocumentNode.SelectNodes("//abc//tag");
                */


                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                //     string htmlString = "c:/test.htm";
                document.LoadHtml(openFileDialog2.FileName);
                HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//a");
                foreach (HtmlNode link in collection)
                {
                    string target = link.Attributes["href"].Value;
                }

                var currate = 1; // = GetPage(initialstring, "/tr", "</td", list);
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
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            FORTSReconciliation("Renesource");
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
                var lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                var reportDate = openFileDialog2.FileName.Substring(openFileDialog2.FileName.IndexOf("_") + 1,
                                                                    openFileDialog2.FileName.LastIndexOf("-") -
                                                                    openFileDialog2.FileName.IndexOf("_") - 1);
                int idTradeDate = 13,
                    idSymbol = 4,
                    idQty = 6,
                    idSide = 5,
                    idPrice = 8,
                    idValueDate = 12,
                    idValue = 9;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
                while (!reader.EndOfStream)
                {
                    lineFromFile = reader.ReadLine().Replace("\"", "");
                    var rowstring = lineFromFile.Split(Delimiter);
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
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog1.FileName,
                                                                                              0, false, 5, "", "", false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel.XlPlatform
                                                                                                       .xlWindows, "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet =
                    (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Derivative Trades_Деривативы"];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;

                int rowCount = xlRange.Rows.Count + 1;
                int colCount = xlRange.Columns.Count;
                DateTime reportdate = DateTime.FromOADate(xlRange.Cells[3, 8].value2);
                // reportdate = reportdate.AddDays(-1);
                var db = new EXANTE_Entities(_currentConnection);
                var nextdate = Fortsnextday.Value.AddDays(1);
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
                var n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    var Ctrade_id = ctrade.ExchangeOrderId.Replace("DC:F:", "");
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
                        var tradeDate = DateTime.FromOADate(xlRange.Cells[i, 4].value2);
                        if (tradeDate.Date == reportdate.Date)
                        {
                            var time = DateTime.FromOADate(xlRange.Cells[i, 5].value2);
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
                    reconitem.CpTrade_id = allfromfile[(int) reconitem.CpTrade_id].FullId;
                    db.Reconcilations.Add(reconitem);
                }
                db.SaveChanges();
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
                TradesParserStatus.Text = "Done:" + openFileDialog1.FileName;
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            DateTime TimeStart = DateTime.Now;
            var db = new EXANTE_Entities(_currentConnection);
            var reportdate = ABNDate.Value;
            LogTextBox.AppendText(TimeStart + ": " + "Updating links for " + reportdate.ToShortDateString());

            var nextdate = reportdate.AddDays(1);
            var cptradefromDb = (from cptrade in db.CpTrades
                                 where
                                     cptrade.valid == 1 && cptrade.ReportDate >= reportdate.Date &&
                                     cptrade.ReportDate < (nextdate.Date) && cptrade.BOTradeNumber != null
                                 select cptrade).ToList();
            var reclist = (from rec in db.Reconcilations
                           where rec.Timestamp >= reportdate.Date
                           select rec).ToDictionary(k => (k.CpTrade_id.ToString() + ';' + k.Ctrade_id.ToString()),
                                                    k => k.id);
            var i = 0;
            foreach (CpTrade cpTrade in cptradefromDb)
            {
                var ctrades = cpTrade.BOTradeNumber.Split(';');
                foreach (string ctrade in ctrades)
                {
                    var key = cpTrade.FullId.ToString() + ';' + ctrade;
                    if (!reclist.ContainsKey(key))
                    {
                        db.Reconcilations.Add(new Reconcilation
                            {
                                CpTrade_id = cpTrade.FullId,
                                Ctrade_id = Convert.ToInt64(ctrade),
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Renesource trades uploading");
                var LInitTrades = TradeParsing("Renesource", "EXCEL", "ST");
                var lCptrades = InitTradesConverting(LInitTrades, "Renesource");
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("Renesource");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "Renesource" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "Renesource");
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button25_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start IB Belarta trades uploading");
                var LInitTrades = TradeParsing("BelartaIB", "EXCEL", "ST");
                var lCptrades = InitTradesConverting(LInitTrades, "BelartaIB", true, "BelartaIB");
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("BelartaIB");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "BelartaIB" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "BelartaIB");
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button26_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            TradesParserStatus.Text = "Processing";
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                DateTime TimeStart = DateTime.Now;
                LogTextBox.AppendText("\r\n" + TimeStart.ToLongTimeString() + ": " + "start Renesource trades uploading");
                var LInitTrades = TradeParsing("Renesource", "EXCEL", "FX");
                var lCptrades = InitTradesConverting(LInitTrades, "Renesource");
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
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("Renesource");
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                double? Leverage = 1;
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "Renesource" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    DateTime valuedate = (DateTime) cpTrade.ValueDate;
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
            RecProcess(reportdate, "Renesource");
            TradesParserStatus.Text = "Done";
            Console.WriteLine(""); // <-- For debugging use. */
        }

        private void button27_Click(object sender, EventArgs e)
        {
            var path = "c:/20160229.txt";
            var reader = new StreamReader(path);
            var lineFromFile = reader.ReadLine();
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
                    var t = 1;
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
                var reportdate = ABNDate.Value;
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
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                var lInitTrades = CFHParsing();
                var lCptrades = OpenConverting(lInitTrades, "CFH");
                foreach (CpTrade cptrade in lCptrades)
                {
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("CFH");
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "CFH" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        var map = symbolmap[cpTrade.Symbol];
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
                        db.Entry(cpTrade).State = (System.Data.Entity.EntityState) EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }
            RecProcess(reportdate, "CFH");
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
            // var FORTSDate = ABNDate.Value.ToString("dd.MM.yyyy");
            var FORTSDate = ABNDate.Value.ToString("dd.MM.yyyy");
            //  updateFORTSccyrates(FORTSDate);
            DateTime TimeEndUpdating = DateTime.Now;
            LogTextBox.AppendText("\r\n" + TimeEndUpdating + ": " + "CCY FORTS rates for " + FORTSDate +
                                  " uploaded. Time:" + (TimeEndUpdating - TimeStart).ToString());

            calcualteVM(ABNDate.Value, "ATON");
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
                Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
                //Открываем книгу.                                                                                                                                                        
                Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(openFileDialog1.FileName,
                                                                                              0, false, 5, "", "", false,
                                                                                              Microsoft.Office.Interop
                                                                                                       .Excel.XlPlatform
                                                                                                       .xlWindows, "",
                                                                                              true, false, 0, true,
                                                                                              false, false);
                //Выбираем таблицу(лист).
                Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet;
                ObjWorkSheet =
                    (Microsoft.Office.Interop.Excel.Worksheet) ObjWorkBook.Sheets["Derivative Trades_Деривативы"];
                Microsoft.Office.Interop.Excel.Range xlRange = ObjWorkSheet.UsedRange;

                int rowCount = xlRange.Rows.Count + 1;
                int colCount = xlRange.Columns.Count;
                DateTime reportdate = DateTime.FromOADate(xlRange.Cells[3, 8].value2);
                // reportdate = reportdate.AddDays(-1);
                var db = new EXANTE_Entities(_currentConnection);
                var nextdate = Fortsnextday.Value.AddDays(1);
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
                var n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    var Ctrade_id = ctrade.ExchangeOrderId.Replace("DC:F:", "");
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
                        var tradeDate = DateTime.FromOADate(xlRange.Cells[i, 4].value2);
                        if (tradeDate.Date == reportdate.Date)
                        {
                            var time = DateTime.FromOADate(xlRange.Cells[i, 5].value2);
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
                SaveDBChanges(ref db);

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpTrade_id = allfromfile[(int) reconitem.CpTrade_id].FullId;
                    db.Reconcilations.Add(reconitem);
                }
                SaveDBChanges(ref db);
                db.Dispose();
                ObjWorkBook.Close();
                ObjExcel.Quit();
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjWorkBook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(ObjExcel);
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            FORTSReconciliation("ITInvest");
            var db = new EXANTE_Entities(_currentConnection);
            db.Database.ExecuteSqlCommand(
                "UPDATE CpTrades AS cp INNER JOIN Contracts AS c ON c.id = cp.BOSymbol SET cp.value = - cp.Qty*cp.Price*c.Leverage WHERE cp.BrokerId LIKE '%ITInvest' AND ReportDate > '2016-06-01'");
            db.Dispose();
        }

        private void button11_Click_2(object sender, EventArgs e)
        {
            var reportdate = ABNDate.Value;
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                reportdate = AxiPdfParser(reportdate);
            }
            RecProcess(reportdate, "Axi");
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

        private static int AxiPdfGetStarRow(string[] rows)
        {
            int i_row = 0;
            while ((i_row < rows.Length) && (!rows[i_row].Contains("NEW TRADING ACTIVITY")))
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
            if (rows[i_row].Contains("LONP100 ML INVEST"))
            {
                return i_row + 1;
            }
            else
            {
                return -1;
            }
        }


        private DateTime AxiPdfParser(DateTime reportdate)
        {
            DateTime TimeStart = DateTime.Now;
            var db = new EXANTE_Entities(_currentConnection);
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var checkId = (from ct in db.CpTrades
                               where ct.TradeDate.ToString().Contains("2016-0") && ct.BrokerId == "Axi"
                               select ct).ToDictionary(k => k.exchangeOrderId.ToString(), k => k.FullId);
                PdfReader reader = new PdfReader(openFileDialog2.FileName);
                var count = reader.NumberOfPages;
                string txt = "";
                var i = 1;
                txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                var indexDate = txt.IndexOf("Date: ") + 6;
                var indexDateEnd = txt.IndexOf(" ", indexDate);
                string tempdate = txt.Substring(indexDate, indexDateEnd - indexDate);
                if (tempdate.Length < 11) tempdate = "0" + tempdate;
                reportdate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                while (i <= count && !txt.Contains("NEW TRADING ACTIVITY"))
                {
                    i++;
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                }
                var dicCpCtrades = new Dictionary<string,List< CpTrade>>();
                var flagStop = false;
                var i_row = 0;
                var rows = txt.Split('\n');
                i_row = AxiPdfGetStarRow(rows);
                var account = rows[i_row - 1];
                while ((i < count) && (!flagStop))
                {
                    txt = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    rows = txt.Split('\n');
                    if (i_row != -1)
                    {
                         while ((i_row < rows.Length) &&(!rows[i_row].Contains("CASH MOVEMENTS")))
                        {
                            if (rows[i_row].Contains("Amount"))
                            {
                                i_row = i_row + 2;
                            }
                            else
                            {
                                if ((rows[i_row].Contains("NEW")) || (rows[i_row].Contains("SETTLED")))
                                {
                                    var traderow = rows[i_row].TrimStart().TrimEnd().Replace("  ", " ").Split(' ');
                                    var add = 0;
                                    string tradenumber = "";
                                    if ((traderow[0].TrimStart().TrimEnd() == "NEW") || (traderow[0].TrimEnd().TrimStart() == "SETTLED"))
                                    {
                                        add = 1;
                                        tradenumber = rows[i_row + 1].TrimStart();
                                    }
                                    else
                                    {
                                        tradenumber = traderow[0];
                                    }
                                    var type = traderow[1-add];
                                    tempdate = traderow[3 - add];
                                    if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                    var tradedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                        CultureInfo.InvariantCulture);
                                    tempdate =traderow[4 - add];
                                    if (tempdate.Length < 11) tempdate = "0" + tempdate;
                                    var valuedate = DateTime.ParseExact(tempdate, "dd-MMM-yyyy",
                                                                        CultureInfo.InvariantCulture);
                                 //   if (!dicCpCtrades.ContainsKey(tradenumber) && (type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                    if ((type == "NEW") && (!checkId.ContainsKey(tradenumber)))
                                    {
                                        if (!dicCpCtrades.ContainsKey(tradenumber))
                                        {
                                            dicCpCtrades.Add(tradenumber, new List<CpTrade>());
                                        }
                                        dicCpCtrades[tradenumber].Add(new CpTrade()
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
                                                TradeDate =tradedate,
                                                Symbol = traderow[5 - add],
                                                Type = traderow[2 - add],
                                                Qty = AxiPdfGetNegativeValue(traderow[8 - add]),
                                                Price = Convert.ToDouble(traderow[9 - add]),
                                                ValueDate =valuedate,
                                                value = AxiPdfGetNegativeValue(traderow[10 - add])
                                            });
                                    }
                                }
                            }
                            i_row++;
                        }
                        if ((i_row<rows.Length)&&(rows[i_row].Contains("CASH MOVEMENTS")))
                        {
                            flagStop = true;
                        }
                    }
                    i_row = 0;
                    i++;
                }
                foreach (KeyValuePair<string, List<CpTrade>> valuePair in dicCpCtrades)
                {
                    if (valuePair.Value.Count == 1)
                    {
                        db.CpTrades.Add(valuePair.Value[0]);
                    }
                    else
                    {
                        if (valuePair.Value.Count == 2)
                        {
                            if (((valuePair.Value[0].Symbol != "JPY/USD") && (valuePair.Value[0].Symbol != "CHF/USD") && (valuePair.Value[0].Symbol != "CAD/USD") && (!valuePair.Value[0].Symbol.Contains("THB/"))&& (!valuePair.Value[0].Symbol.Contains("TRY/"))
                                && (valuePair.Value[0].Symbol != "MXN/USD") && (valuePair.Value[0].Symbol != "NOK/USD") &&
                                (valuePair.Value[0].Symbol.Contains("/USD"))) || (!valuePair.Value[0].Symbol.Contains("USD")))
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
                 DateTime TimeEnd = DateTime.Now;
                 LogTextBox.AppendText("\r\n" + TimeEnd.ToLongTimeString() + ": " + dicCpCtrades.Count + " trades Axi uploading completed." +
                                      (TimeEnd - TimeStart).ToString());
              /*  foreach (KeyValuePair<string, CpTrade> keyValuePair in dicCpCtrades)
                {
                    db.CpTrades.Add(keyValuePair.Value);
                }*/
                SaveDBChanges(ref db);
                db.Dispose();
            }
            return reportdate;
        }

        private void button28_Click(object sender, EventArgs e)
        {
            DateTime reportdate = ABNDate.Value; //todo Get report date from xml Processing date
            var db = new EXANTE_Entities(_currentConnection);
            if (!noparsingCheckbox.Checked)
            {
                var lInitTrades = TradeParsing("LMAX", "CSV", "FX");
                var lCptrades = OpenConverting(lInitTrades, "LMAX");
                foreach (CpTrade cptrade in lCptrades)
                {
                    if (cptrade.Qty > 0) cptrade.value = -cptrade.value;
                    cptrade.Qty = cptrade.Qty*10000;
                    cptrade.Fee = -Math.Abs((double) cptrade.Fee);
                    db.CpTrades.Add(cptrade);
                }
                SaveDBChanges(ref db);
            }
            else
            {
                var nextdate = reportdate.AddDays(1);
                var symbolmap = getMapping("LMAX");
                var cptradefromDb = from cptrade in db.CpTrades
                                    where cptrade.valid == 1 && cptrade.BrokerId == "LMAX" &&
                                          cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                          cptrade.BOTradeNumber == null
                                    select cptrade;
                var contractrow =
                    from ct in db.Contracts
                    where ct.valid == 1
                    select ct;
                var contractdetails = contractrow.ToDictionary(k => k.id, k => k);

                foreach (CpTrade cpTrade in cptradefromDb)
                {
                    if (cpTrade.BOSymbol == null && symbolmap.ContainsKey(cpTrade.Symbol))
                    {
                        var map = symbolmap[cpTrade.Symbol];
                        cpTrade.BOSymbol = map.BOSymbol;
                        cpTrade.Price = cpTrade.Price * map.MtyPrice;
                        cpTrade.Qty = cpTrade.Qty * map.MtyVolume;
                        cpTrade.value = cpTrade.value * map.Leverage;
                        if (contractdetails.ContainsKey(map.BOSymbol))
                        {
                            cpTrade.ValueDate = contractdetails[map.BOSymbol].ValueDate;
                        }
                        else
                        {
                            cpTrade.ValueDate = map.ValueDate;
                        }
                        db.CpTrades.Attach(cpTrade);
                        db.Entry(cpTrade).State = (System.Data.Entity.EntityState)EntityState.Modified;
                    }
                }
                SaveDBChanges(ref db);
            }
            RecProcess(reportdate, "LMAX");
        }
    }


    internal class BOaccount
    {
        internal string accountNameCP;
        internal string BOaccountId;
        internal string DBcpName;
    }



    [DataContract]
    internal class FTjson
     {
         [DataMember] 
         internal string asset;

         [DataMember]
         internal string accountId;

         [DataMember] 
         internal string timestamp;

         [DataMember] 
         internal string operationType;
         
         [DataMember] 
         internal string amount;
         [DataMember] 
         internal string comment;
         [DataMember] 
         internal string internalComment;
         [DataMember] 
         internal string symbolId;
     }


    [DataContract]
    internal class BOjson
    {
        [DataMember]
        internal string accountId;
        [DataMember]
        internal string tradeType;
        [DataMember]
        internal string symbolId;
        [DataMember]
        internal string quantity;
        [DataMember]
        internal string price;
        [DataMember]
        internal string gwTime;
        [DataMember]
        internal string valueDate;
        [DataMember]
        internal string side;
        [DataMember]
        internal string userId;
        [DataMember]
        internal string counterparty;
        [DataMember]
        internal string settlementBrokerAccountId;
        [DataMember]
        internal string settlementBrokerClientId;
        [DataMember]
        internal string settlementCounterparty;
        [DataMember]
        internal string comment;
        [DataMember]
        internal string internalComment;
        [DataMember]
        internal Boolean takeCommission;
        [DataMember]
        internal Boolean redemption;
        [DataMember]
        internal Boolean isManual;
        [DataMember]
        internal string exchangeOrderId;
        [DataMember] 
        internal string brokerAccountId;
        [DataMember] 
        internal string commission;
        [DataMember] 
        internal string commissionCurrency;
        [DataMember] 
        internal string brokerClientId;
    }

     internal class cpCost_cpTrade
     {
         internal Nullable<double> ExchangeFees;
         internal Nullable<double> Fee;
         internal Nullable<double> Fee2;
         internal Nullable<double> Fee3;
         internal Nullable<double> Qty;
         internal string Symbol;
         internal string BrokerId;
         internal string ccy;
         internal string ExchFeeCcy;
         internal string BOTradeNumber;
     }

    internal class cpCost_cTrade
     {
         internal Nullable<double> fees;
         internal Nullable<double> qty;
         internal string symbol_id;
         internal string cp_id;
         internal string account_id;
         internal string ExchFeeCcy;
         internal string currency;
         internal Nullable<long> tradeNumber;
     }
    }