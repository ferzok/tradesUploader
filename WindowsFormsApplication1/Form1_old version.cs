using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
//using System.Data.Entity.Core.Common.;
using System.Data.Entity.Core.Objects;
//using System.Data.Objects; 
//using System.Data.Entity.Core.EntityClient;
//Objects.SqlClient;
//using System.DaSqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private const char Delimiter = ';';
        private const string MysqlConnection =
            "data source=ext-padma.ghcg.com; user id=az; password=GftwV4Be3QtKQt; database=EXANTE_test;pooling=false";
        //   "data source=az.dev.ghcg.com; user id=az; password=GftwV4Be3QtKQt; database=test;pooling=false";
        public Form1()
        {
            InitializeComponent();
        }
        
        private void TradesParser_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var reportdate = new DateTime(2013, 04, 24);
                var testexample = new EXANTE_Entities();
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromFile = new List<Ctrade>();

                const int GMToffset = 4; //gmt offset from BO
                const int nextdaystarthour = 19; //start new day for FORTS
                const string template = "FORTS";
                var nextdayvalueform = dateTimePicker1.Value;
                var lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                if (lineFromFile != null)
                {
                    var rowstring = lineFromFile.Split(Delimiter);
                    int idDate = 0,
                        idSymbol = 0,
                        idAccount = 0,
                        idqty = 0,
                        idprice = 0,
                        idside = 0,
                        idfees = 0,
                        iduser = 0,
                        idcurrency = 0,
                        idorderid = 0,
                        idbrokerTimeDelta = 0,
                        idexchangeOrderId = 0,
                        idcontractMultiplier = 0,
                        idtradeNumber = 0,
                        idcounterparty = 0,
                        idgateway = 0,
                        idtradeType=0,
                        idvalueDate=0;
                    for (var i = 0; i < rowstring.Length; i++)
                    {
                        switch (rowstring[i])
                        {
                            case "gwTime":
                                idDate = i;
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
                            case "tradeNumber":
                                idtradeNumber = i;
                                break;
                            case "orderId":
                                idorderid = i;
                                break;
                            case "brokerTimeDelta":
                                idbrokerTimeDelta = i;
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
                            case "tradeType":
                                idtradeType = i;
                                break;
                                
                            default:
                                Console.WriteLine("Additional fields in the tr.file!");
                                break;
                        }
                    }
                    var index = 1;
                    var stringindex = Convert.ToString(reportdate.Year);
                    if (reportdate.Month < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Month));
                    if (reportdate.Day < 10) stringindex = string.Concat(stringindex, "0");
                    stringindex = string.Concat(stringindex, Convert.ToString(reportdate.Day));
                    var initialindex = Convert.ToInt64(stringindex);
                    var contractrow =
                        from ct in testexample.Contracts
                        where ct.valid == 1
                        select ct;
                    var contractdetails = contractrow.ToDictionary(k => k.id, k => k.ValueDate);
                    while (!reader.EndOfStream)
                    {
                        lineFromFile = reader.ReadLine();
                        if (lineFromFile == null) continue;
                        rowstring = lineFromFile.Split(Delimiter);
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
                                            : double.Parse(rowstring[idcontractMultiplier], CultureInfo.InvariantCulture),
                                    ValueDate = valuedate,//Convert.ToDateTime(rowstring[idvalueDate]),
                                    Currency =
                                        idcontractMultiplier > (rowstring.Length - 1)
                                            ? "USD"
                                            : rowstring[idcurrency],
                                    Margin = 0,
                                    FlatMargin = 0,
                                    Canbesettled = true,
                                    UpdateDate = DateTime.UtcNow,
                                    commission = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture)/double.Parse(rowstring[idqty], CultureInfo.InvariantCulture),
                                    Timestamp = DateTime.UtcNow,
                                    valid = 1,
                                    username = "TradeParser"
                                };
                            testexample.Contracts.Add(test);
                            testexample.SaveChanges();
                            contractrow =
                                from ct in testexample.Contracts
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
                            var trade_id = rowstring[idexchangeOrderId];
                            var account_id = rowstring[idAccount];
                            var Date = Convert.ToDateTime(rowstring[idDate]);
                            var symbol_id = rowstring[idSymbol];
                            var qty = rowstring[idqty].IndexOf(".") == -1
                                      ? Convert.ToInt64(rowstring[idqty])*side
                                      : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)*side;
                            var price = double.Parse(rowstring[idprice], CultureInfo.InvariantCulture);
                            var cp_id = rowstring[idcounterparty];
                            var fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture);
                            var value_date = valuedate;//Convert.ToDateTime(rowstring[idvalueDate]),
                            var currency = idcontractMultiplier > (rowstring.Length - 1)
                                           ? "USD"
                                           : rowstring[idcurrency];
                            var tradeNumber = Convert.ToInt64(rowstring[idtradeNumber]);
                            var Timestamp = DateTime.UtcNow;
                            var valid = 1;
                            var  username = rowstring[iduser];
                            var order_id = rowstring[idorderid];
                            var gatewayId = rowstring[idgateway];
                            var BOtradeTimestamp = vBOtradeTimestamp;

                           allfromFile.Add(new Ctrade
                                {
                                    trade_id = rowstring[idexchangeOrderId],
                                    account_id = rowstring[idAccount],
                                    Date = Convert.ToDateTime(rowstring[idDate]),
                                    symbol_id = rowstring[idSymbol],
                                    qty = rowstring[idqty].IndexOf(".") == -1
                                      ? Convert.ToInt64(rowstring[idqty])*side
                                      : double.Parse(rowstring[idqty], CultureInfo.InvariantCulture)* side,
                                    price = double.Parse(rowstring[idprice], CultureInfo.InvariantCulture), 
                                    cp_id = rowstring[idcounterparty],
                                    fees = double.Parse(rowstring[idfees], CultureInfo.InvariantCulture),
                                    value_date = valuedate,//Convert.ToDateTime(rowstring[idvalueDate]),
                                    currency = idcontractMultiplier > (rowstring.Length - 1)
                                           ? "USD"
                                           : rowstring[idcurrency],
                                    tradeNumber = Convert.ToInt64(rowstring[idtradeNumber]),
                                    Timestamp = DateTime.UtcNow,
                                    valid = 1,
                                    username = rowstring[iduser],
                                    order_id = rowstring[idorderid],
                                    gatewayId = rowstring[idgateway],
                                    BOtradeTimestamp = vBOtradeTimestamp,
                                    tradeType = rowstring[idtradeType],
                                    deliveryDate = Convert.ToDateTime(rowstring[idvalueDate])
                                });
                        }
                    }
                }
                TradesParserStatus.Text = "DB updating";
                //  CheckUniqueTrades(allfromFile);
               foreach (Ctrade tradeIndex in allfromFile)
               {
                   testexample.Ctrades.Add(tradeIndex);
               }
                testexample.SaveChanges();
            }
            TradesParserStatus.Text = "Done";
            Console.WriteLine(result); // <-- For debugging use. 
        }
    //todo get trades from DB BO   
    private List<Ctrade> getTradesFromDB (DateTime reportdate, List<string> cplist,bool removeReconciled){      
        var testexample = new EXANTE_Entities();
        var prevreportdate = reportdate.AddDays(-1);
        var ts = new TimeSpan(20, 00, 0);
        prevreportdate = prevreportdate.Date + ts;

        var nextdate = reportdate.AddDays(1);
        var boTradeNumberlist = new List<long?>();
        if (removeReconciled)
        {
            var boTradeNumbers = testexample.CpTrades.Where(
                cptrade => cptrade.valid == 1 && cptrade.ReportDate >= reportdate.Date &&
                           cptrade.ReportDate < (nextdate.Date) && cptrade.BOTradeNumber != null)
                                            .Select(cptrade => cptrade.BOTradeNumber);
            foreach (string boTradeNumber in boTradeNumbers)
            {
                var templist = boTradeNumber.Split(';');
                boTradeNumberlist.AddRange(templist.Select(s => (long?) Convert.ToInt64(s)));
            }
            //   boTradeNumberlist.AddRange(boTradeNumbers.ToList().Select(s => (long?) Convert.ToInt64(s)));
        }
       /* var queryable = from ct in testexample.Ctrades
                        where ct.valid == 1 && ct.Date >= reportdate.Date && ct.Date < (nextdate.Date) &&
                              cplist.Contains(ct.cp_id) && !boTradeNumberlist.Contains(ct.tradeNumber)
                        select ct;*/
        var queryable = from ct in testexample.Ctrades
                        where ct.valid == 1 && ct.RecStatus == false && ct.BOtradeTimestamp >= prevreportdate && ct.Date < (nextdate.Date)
                        //&&cplist.Contains(ct.cp_id)
                        select ct;
        return queryable.ToList();
    }
      private Array getBOtoABNMapping(){
        var testexample = new EXANTE_Entities();
          var queryable =
              from ct in testexample.Mappings
              where ct.valid == 1 && ct.Type == "Cp"
              select new {ct.BrockerSymbol,ct.BOSymbol};
          return queryable.ToArray();
      }
    
        private string FXFWDupdate(string str){
          var indexE2=str.IndexOf('.')+1;
          if (indexE2==0){
            indexE2=str.IndexOf("A3");
            if (indexE2==0){
              indexE2=str.IndexOf("E4");
            }
          }
          var currency= str.Substring(0,indexE2-1);
        //  currency=currency.Replace('/');
          if((str.IndexOf("SPOT")==-1)&&(str.IndexOf("EXANTE")==-1)&&(str.IndexOf("E6")==-1)&&(str.IndexOf("E5")==-1)){
            var Date= str.Substring(indexE2+3,str.Length-indexE2-3);
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
  else {currency=currency+"FX";}
  return currency;
}

        private void AbnRecon(DateTime reportdate, List<CpTrade> trades)
        {
            var cplist = new List<string> {"LEK", "CQG", "FASTMATCH", "CURRENEX","EXANTE", ""};
            var mltytrades = MultyTradesCheckBox.Checked;
            var boTradeslist = CreateIdForBoTrades(getTradesFromDB(reportdate, cplist, true));
            var numberBoTrades = boTradeslist.Count;
            var cpmapping = getBOtoABNMapping();
            var symbolMap = getSymbolMap();

            var abnTradeslist = CreateIdForAbnTrades(getOnlyTrades(trades));
            var recon = new List<Reconcilation>();
            var db = new EXANTE_Entities();
            foreach (var cpTrade in abnTradeslist)
            {
                List<Ctrade> ctrade;
               
                if (boTradeslist.TryGetValue(cpTrade.Id, out ctrade))
                {
                    cpTrade.BOTradeNumber = ctrade[0].tradeNumber.ToString();
                    cpTrade.BOcp = ctrade[0].cp_id;
                    cpTrade.Comment = ctrade[0].BOtradeTimestamp.Value.ToShortDateString();
                    ctrade[0].RecStatus = true;
                    db.CpTrades.Attach(cpTrade);
                    db.Entry(cpTrade).State = EntityState.Modified;
                    db.Ctrades.Attach(ctrade[0]);
                    db.Entry(ctrade[0]).State = EntityState.Modified;

                    ctrade.RemoveAt(0);
                    if (ctrade.Count == 0)
                    {
                        boTradeslist.Remove(cpTrade.Id);
                    }
                    recon.Add(new Reconcilation
                    {
                        CpTrade_id = cpTrade.FullId,
                        Ctrade_id = Convert.ToInt64(cpTrade.BOTradeNumber),
                        Timestamp = DateTime.UtcNow,
                        username = "TradeParser",
                        valid = 1
                    });
                }
                else {
                    var t = 1;
                 //   CheckMultitrades(cpTrade,boTradeslist.Values.SelectMany(x=>x).ToList());
               }
            }
            db.SaveChanges();
     /*       List<Ctrade> bolist = null;
            foreach (KeyValuePair<string, List<Ctrade>> keyValuePair in boTradeslist)
            {
                if ((keyValuePair.Value[0].RecStatus == false)&&(keyValuePair.Value[0].symbol_id.Contains("%/%.%.%20%")))
                {
                    var t = 1;
                }
                bolist.Add(keyValuePair.Value[0]);
            }*/
        /*    foreach (CpTrade cpTrade in abnTradeslist)
            {
                db.CpTrades.Attach(cpTrade);
                db.Entry(cpTrade).State = EntityState.Modified;
            }*/
            foreach (Reconcilation reconcilation in recon)
            {
                db.Reconcilations.Add(reconcilation);
            }
           db.SaveChanges();
  }
      
      private  void CheckMultitrades(CpTrade trade,List <Ctrade> boTrades){
       //  var Sequence= new Array();
         if (trade !=null){
             var possibletrades = boTrades.Where(item => (item.symbol_id==trade.BOSymbol && item.price==trade.Price) ); ;
              if (trade.Qty > 0)
             {
               possibletrades = possibletrades.Where(item => item.qty > 0);
               possibletrades = possibletrades.OrderByDescending(o => o.qty);
             }
             else{
               possibletrades = possibletrades.Where(item => item.qty < 0);
               possibletrades = possibletrades.OrderBy(o => o.qty);
              }
              var Sequence= new List<long>();
              if (possibletrades.Count()>0){
              if (trade.Qty==possibletrades.ElementAt(0).qty){
                if(possibletrades.ElementAt(0).tradeNumber!=null)Sequence.Add((long) possibletrades.ElementAt(0).tradeNumber);

              }
              else {
                var i=0;
                double qty=0;
                while ((i<possibletrades.Count())&&(qty!=trade.Qty)){
                  if(Math.Abs((double) possibletrades.ElementAt(i).qty)<Math.Abs((double) trade.Qty)){
                    qty = (double) possibletrades.ElementAt(i).qty;
                    if (Sequence.Count == 0) Sequence.Add((long)possibletrades.ElementAt(i).tradeNumber);
                    else Sequence[0] = (long)possibletrades.ElementAt(i).tradeNumber;
               //     qty=calculateQty(trade.Qty,qty,i+1,possibletrades,Sequence,1);
                  }
                  else i++;
                }
                //for (i = 0;i<Sequence.Count;i++)Sequence[i]=possibletrades.ElementAt(Sequence[i]);
              }
              }
              }
            //  return Sequence;
}/*
        private double calculateQty(double InitialQty,qty,i,possibletrades,Sequence,level){
  if (i<possibletrades.length){var nextValue= possibletrades[i][5]}
  while ((i<possibletrades.length)&&((qty)!=InitialQty)){
   if(Math.abs(nextValue+qty)<=Math.abs(InitialQty)){
      qty = nextValue+qty;
      if(Sequence[level]==undefined) Sequence.push(i)
      else Sequence[level]=i;
      if(qty!=InitialQty)qty=calculateQty(InitialQty,qty,i+1,possibletrades,Sequence,level+1);
    }
    else {
      i++;
      if (i<possibletrades.length)nextValue= possibletrades[i][5];
    }
  }
  return qty;
}*/
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


        private static List<CpTrade> CreateIdForAbnTrades(List<CpTrade> trades)
        {
            var symbolmap = getMap("ABN");
          
        /*    if (symbolmap.TryGetValue(symbol + "OP", out symbolvalue))
            {
                MtyVolume = symbolvalue.MtyVolume;
                MtyPrice = symbolvalue.MtyPrice;
                BoSymbol = symbolvalue.BOSymbol + "." + getLetterOfMonth(valuedate.Value.Month) + valuedate.Value.Year + "." + type + strike * MtyPrice;
            }*/
            foreach (CpTrade cpTrade in trades)
            {
                var key = cpTrade.BOSymbol;
                if (cpTrade.Symbol == "XSGD")
                {
                    var t = 1;
                }
                //todo убрать эти условия
                Map symbolvalue;      
                if (cpTrade.Type == "OP") {
                    
                    if (symbolmap.TryGetValue( (cpTrade.Symbol.Substring(0, cpTrade.Symbol.IndexOf(".") - 1) + "OP", out symbolvalue))
                    {
                        var digit = symbolvalue.Round;
                        var BoSymbol = symbolvalue.BOSymbol + "." + getLetterOfMonth(valuedate.Value.Month) + valuedate.Value.Year + "." + type + strike * MtyPrice;
                    }
                    //cpTrade.Symbol.Substring(0, cpTrade.Symbol.IndexOf(".") - 1)
                    key = key + "ST" + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                }else{     if ((cpTrade.Type == "ST") || (cpTrade.Type == "FX") || (cpTrade.Type == "FW-E") || (cpTrade.Type == "PM"))
                {
                    key = key + cpTrade.Type + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                }
                else
                {
                    var vd = cpTrade.ValueDate.GetValueOrDefault().ToShortDateString();
                    key = key + vd + cpTrade.Qty.ToString() + cpTrade.Price.ToString();
                }}
                cpTrade.Id = key;
            }
            return trades;
        }

        private static Dictionary<string, List<Ctrade>> CreateIdForBoTrades(List<Ctrade> boTradeslist)
        {
            var result = new Dictionary<string, List<Ctrade>>();
            var defaultvalue = new DateTime(2011, 1, 1);
            var defaltvd = defaultvalue.ToShortDateString();
            var bomap = getMap("BO");
           /* var ABNMap = getMap("ABN");
            foreach (var item in ABNMap)
            {
                bomap[item.Key] = item.Value;
            }
            */
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
                     //       if(botrade)
                            key = key + "ST";
                        }
                        key = key+botrade.qty.ToString() + botrade.price.ToString();
                    }
                    else
                    {
                      /*  if (botrade.symbol_id.IndexOf('.',0,1) > -1){
                           BOTrades[i][f_id]=FXFWDupdate(botrade.symbol_id).concat(botrade.qty,botrade.price);
                           var validRegExp = /[A-z]+/;
                           BOTrades[i][10] = validRegExp.exec(BOTrades[i][f_id])[0];
                        }*/
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

        private static object getSymbolMap()
        {
            var testexample = new EXANTE_Entities();
            var Mapping = from m in testexample.Mappings
                                where m.valid == 1 && m.Brocker=="ABN" 
                                select m;
            var result = Mapping.ToList();
            testexample.Dispose();
            return result;
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

        private List<Array> getABNMapping(string filter)
        {
            var mapping = new List<Array>();
            var testexample =new EXANTE_Entities(); 
            var mappings = from map in testexample.Mappings
                                 where map.valid == 1
                                 select map;
            var dictMap = new List<Array>();// mappings.ToList();
            return dictMap;
        }

        /*  
          private List<Array> ABNTradesParser(List<string> BodyStrArray)
          {
            var RawTradesArray = ABNgetRowsFromCliff(BodyStrArray,0,3,"410");
            var result = new List<Array>();
            if((RawTradesArray!=null)&&(RawTradesArray.Count>0)){ 
            var mappingST = getABNMapping("STOCK&FX");    
        /*    var mappingFW = getABNMapping("FW");
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
                     mappingST = getABNMapping("STOCK&FX");
                       mappingFW = getABNMapping("FW");
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
                    return DateTime.ParseExact(itemNode.SelectSingleNode("ValueDate").InnerText, "yyyyMMdd", CultureInfo.CurrentCulture);
                
                }
                else
                {
                    return DateTime.ParseExact(itemNode.SelectSingleNode("Product/Expiry").InnerText, "yyyyMMdd", CultureInfo.CurrentCulture);
                }
            }
            else
            {
                return DateTime.ParseExact(itemNode.SelectSingleNode("SettlementDate").InnerText, "yyyyMMdd", CultureInfo.CurrentCulture);
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
                            case "FORWARD CONF":return "FW";
                            case "FX CONF":return "FX";
                            case "TRADE":
                                if (itemNode.SelectSingleNode("Depot/DepotId") != null)
                                {
                                    if (itemNode.SelectSingleNode("Depot/DepotId").InnerText == "METALS")
                                    {
                                        return "METALS";
                                        break;
                                    }
                                    return "Others";
                                }
                                else
                                {
                                    return "Others";
                                }
                                break;
                            default :
                                return "Others";
                        }
                    }
                    break;
                default :
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
            //private string BrockerSymbol { get; set; }
            public string BOSymbol { get; set; }
            public double? MtyPrice { get; set; }
            public double? MtyVolume { get; set; }
            public string Type { get; set; }
            public int? Round { get; set; }
            public DateTime? ValueDate { get; set; }
        }

        private static Dictionary<string, Map> getMap(string brocker)
        {
            var testexample = new EXANTE_Entities();
            var mapfromDb = from m in testexample.Mappings
                            join c in testexample.Contracts on m.BOSymbol equals c.id
                            where m.Brocker == brocker
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
                  var key = item.BrockerSymbol;

                  if (brocker != "BO") {
                     key = item.BrockerSymbol + item.Type;
                  }

                  if (item.Type == "FU") key = key + item.ValueDate.Value.ToShortDateString();          
                  results.Add(key,new Map{BOSymbol = item.BOSymbol,
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
            var reportdate = ABNDate.Value;//todo Get report date from xml Processing date
            var testexample = new EXANTE_Entities();
            var symbolmap = getMap("ABN");
            
            if (noparsingCheckbox.Checked)
            {
                var nextdate = reportdate.AddDays(1);
                var cptradefromDb = from cptrade in testexample.CpTrades
                                    where
                                        cptrade.TypeOfTrade == "01" && cptrade.valid == 1 && cptrade.BrokerId=="ABN" &&
                                        cptrade.ReportDate >= reportdate.Date && cptrade.ReportDate < (nextdate.Date) &&
                                        cptrade.BOTradeNumber == null
                                    select cptrade;
                var cptradelist = cptradefromDb.ToList();
                foreach (CpTrade cpTrade in cptradelist)
                {
                     if (cpTrade.BOSymbol == null)
                    {
                        Map symbolvalue;
                        if (cpTrade.Type == "FW")  
                        {
                            var t = 1;
                        }
                        var key = cpTrade.Symbol + cpTrade.Type;
                        if (cpTrade.Type == "FU") key = key + cpTrade.ValueDate.Value.ToShortDateString();
                        if (symbolmap.TryGetValue(key, out symbolvalue))
                        {
                            cpTrade.BOSymbol = symbolvalue.BOSymbol;
                        }
                        if (cpTrade.Type == "FW") cpTrade.BOSymbol = cpTrade.BOSymbol + cpTrade.ValueDate.Value.ToShortDateString();
                        var tt = "GBP/USD.E2.23M2014";
                        var p=tt.IndexOf('.',tt.IndexOf('.')+1);// > -1)
                    }
                }
                AbnRecon(reportdate, cptradelist);
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
                      
               //         if (cliffdict.TryGetValue("610", out rowlist)) updateBalance(rowlist, reportdate);
                //        if (cliffdict.TryGetValue("310", out rowlist))allfromfile = ExtractTradesFromCliff(rowlist, symbolmap);
                 //       if (cliffdict.TryGetValue("410", out rowlist))allfromfile.AddRange(ExtractTradesFromCliff(rowlist, symbolmap));
                        if (cliffdict.TryGetValue("210", out rowlist)) allfromfile.AddRange(ExtractOptionTradesFromCliff(rowlist, symbolmap));

                    }
                    else
                    {
                        allfromfile = ExtractTradesFromXml(symbolmap);
                    }
                    foreach (CpTrade tradeIndex in allfromfile)
                    {
                        testexample.CpTrades.Add(tradeIndex);
                    }
                    testexample.SaveChanges();
                    allfromfile = allfromfile.Where(s => s.TypeOfTrade == "01").ToList(); 
                    AbnRecon(reportdate, allfromfile);
                }
            }
        }

        private List<CpTrade> ExtractTradesFromXml(Dictionary<string, Map> symbolmap)
        {
            //todo: unzip file
            var doc = new XmlDocument();
            //doc.Load(@"C:\20140214.xml");
            doc.Load(openFileDialog2.FileName);
            var testexample = new EXANTE_Entities();
            var allfromfile = new List<CpTrade>();
            var cpfromDb = from cp in testexample.counterparties
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

        private Dictionary<string, List<string>> LoadCliff(string fileName,DateTime reportdate)
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

        private static DateTime? getDatefromString(string lineFromFile,bool time =false)
        {
            if ((lineFromFile[0] != ' ')&&(lineFromFile[0] != '0'))
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
            var testexample = new EXANTE_Entities();
            var cpfromDb = from cp in testexample.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime)getDatefromString(rowlist[0].Substring(6, 8));
            foreach (var row in rowlist)
            {
                var code = row.Substring(124, 2);
                var typeoftrade = row.Substring(60, 2);
                var tradedate = getDatefromString(row.Substring(554), true) ?? getDatefromString(row.Substring(562), true);              
                var symbol = row.Substring(66, 6).Trim();
                var Counterparty = row.Substring(54, 6).Trim();
                var valuedate = getDatefromString(row.Substring(73, 8).Trim());
                var type = row.Substring(72, 1);
                var strike = double.Parse( row.Substring(81, 8) + '.' + row.Substring(89, 7), CultureInfo.InvariantCulture);
                var volumelong = double.Parse(row.Substring(128, 10) + '.' + row.Substring(138, 2), CultureInfo.InvariantCulture);
                var volume =  volumelong - double.Parse(row.Substring(141, 10) + '.' + row.Substring(151, 2), CultureInfo.InvariantCulture);
                var price = double.Parse(row.Substring(247, 8) + '.' + row.Substring(255, 7), CultureInfo.InvariantCulture);
         
                Map symbolvalue;
                double? MtyVolume = 1;
                double? MtyPrice = 1;
                string BoSymbol = null;
                var symbol_id = symbol +"."+type+ strike;

                if (symbolmap.TryGetValue(symbol + "OP", out symbolvalue))
                {
                    MtyVolume = symbolvalue.MtyVolume;
                    MtyPrice = symbolvalue.MtyPrice;
                    BoSymbol = symbolvalue.BOSymbol + "." + getLetterOfMonth(valuedate.Value.Month) + valuedate.Value.Year + "." + type + strike * MtyPrice;
                }

                var exchfee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2), CultureInfo.InvariantCulture);
                if (row.Substring(165, 1) == "D") exchfee = -exchfee;
                var exchfeeccy = row.Substring(166, 3);

                var fee = double.Parse(row.Substring(169, 10) + '.' + row.Substring(179, 2), CultureInfo.InvariantCulture);
                if (row.Substring(181, 1) == "D") fee = -fee;
                var clearingfeeccy = row.Substring(182, 3);
                
                allfromfile.Add(new CpTrade
                {
                    ReportDate = reportdate,
                    TradeDate = tradedate,
                    BrokerId = "ABN",
                    Symbol = symbol_id,
                    Type = typeoftrade,
                    Qty = volume * MtyVolume,
                    Price = price,
                    ValueDate = valuedate,
                    cp_id = getCPid(Counterparty, cpdic),
                    ExchangeFees = exchfee,
                    Fee = fee,
                    Id = null,
                    BOSymbol = BoSymbol,
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
                    ClearingFeeCcy = clearingfeeccy
                });

            }
            return allfromfile;
        }

        private string getLetterOfMonth(int month)
        {
            switch(month){
      case  1:return "F";
        break;   
      case 2: return "G";
        break;    
      case 3: return "H";
        break;   
      case 4: return "J";
        break; 
      case 5: return "K";
        break;   
      case 6:return "M";
                    break; 
      case 7:return "N";
        break; 
      case 8:return "O";
        break;    
      case 9:return "U";
        break;
      case 10:return "V";
        break;    
      case 11:return "X";
        break;
      case 12:return "Z";
        break;
      default:
                    return "";
            }
        }


        private List<CpTrade> ExtractTradesFromCliff(List<string> rowlist,  Dictionary<string, Map> symbolmap)
        {
            var allfromfile = new List<CpTrade>();
            var testexample = new EXANTE_Entities();
            var cpfromDb = from cp in testexample.counterparties
                           select cp;
            var cpdic = cpfromDb.ToDictionary(k => k.Name, k => k.cp_id);
            var reportdate = (DateTime)getDatefromString(rowlist[0].Substring(6, 8));
            foreach (var row in rowlist)
            {
                var typeoftrade = row.Substring(60, 2);
                var tradedate = getDatefromString(row.Substring(582),true) ?? getDatefromString(row.Substring(295), true);
                var symbol = row.Substring(66, 6).Trim();
                var type = row.Substring(60, 2);
                if (row.Substring(405, 4) == "FW-E")
                {
                    type = "FW-E";
                }

        Map symbolvalue;
        double? MtyVolume = 1;
        double? MtyPrice = 1;
        string BoSymbol = null;
        var symbol_id = symbol+type;
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
        }
     
        var exchfee = double.Parse(row.Substring(137, 10) + '.' + row.Substring(147, 2), CultureInfo.InvariantCulture);
        if (row.Substring(149, 1) == "D") exchfee = -exchfee;
        var exchfeeccy = row.Substring(150, 3);
        
        var fee = double.Parse(row.Substring(153, 10) + '.' + row.Substring(163, 2), CultureInfo.InvariantCulture);
        if (row.Substring(165, 1) == "D") fee = -fee;
        var clearingfeeccy = row.Substring(166, 3);
        double value;
        double transacPrice;
        if (typeoftrade != "FU")
        {
            value = double.Parse(row.Substring(276, 16) + '.' + row.Substring(292, 2), CultureInfo.InvariantCulture);
            if (row.Substring(294, 1) == "D") value = -value;
            transacPrice = Math.Round(double.Parse(row.Substring(360, 8) + "." + row.Substring(368, 7), CultureInfo.InvariantCulture)*(double) MtyPrice, 10);
        }
        else
        {
            transacPrice = Math.Round(double.Parse(row.Substring(230, 8) + "." + row.Substring(238, 7), CultureInfo.InvariantCulture) * (double)MtyPrice, 10);
            value = -Math.Round(GetValueFromCliff(row.Substring(112)) * (double)MtyVolume * transacPrice, 10);
        }
        allfromfile.Add(new CpTrade
                               {
                                   ReportDate = reportdate,
                                   TradeDate = typeoftrade=="FU"
                                       ? getDatefromString(row.Substring(496), true)
                                       : getDatefromString(row.Substring(582), true) ?? getDatefromString(row.Substring(295), true),
                                   BrokerId = "ABN",
                                   Symbol = symbol,
                                   Type = (row.Substring(405, 4) == "FW-E")
                                              ? "FW-E"
                                              : type,                
                                   Qty = GetValueFromCliff(row.Substring(112))*MtyVolume,
                                   Price = transacPrice,
                                   ValueDate = valuedate,
                                   cp_id =getCPid(row.Substring(54,6).Trim(), cpdic),
                                   ExchangeFees = exchfee,
                                   Fee = fee,
                                   Id = null,
                                   BOSymbol = BoSymbol,
                                   BOTradeNumber = null,
                                   value = value,
                                   Timestamp = DateTime.UtcNow,
                                   valid = 1,
                                   username = "cliffparser",
                                   //  FullId = null,
                                   BOcp = null,
                                   exchangeOrderId = null,
                                   TypeOfTrade = row.Substring(108,2),
                                   Comment = null,
                                   ExchFeeCcy = exchfeeccy,
                                   ClearingFeeCcy = clearingfeeccy
                               });

            }
            return allfromfile;
        }

        private static double GetValueFromCliff(string row)
        {
            var volumelong = double.Parse(row.Substring(0, 10) + "." + row.Substring(10, 2), CultureInfo.InvariantCulture);
            var volumeshort = row.Substring(13, 10);
            var resvolume = volumelong - double.Parse(row.Substring(13, 10) + "." + row.Substring(23, 2), CultureInfo.InvariantCulture); 
            return resvolume;
        }

        private void updateBalance(List<string> rowlist,DateTime reportdate)
        {
          var dbentity = new EXANTE_Entities();
          var cpidfromDb = from cp in dbentity.DailyChecks
                           where cp.Table == "Daily" && cp.date== reportdate
                           select cp.status;
          var listforDb = new List<ABN_cashposition>();
          foreach (var row in rowlist)
          {
              var value = row.Substring(90, 18);
              value = value.Substring(0, value.Count() - 2) + "."+value.Substring(value.Count() - 2, 2);
              dbentity.ABN_cashposition.Add(new ABN_cashposition
                  {
                     ReportDate =reportdate,
                     Currency= row.Substring(54, 3),
                     Value = row[108] != 'C' 
                                              ? -1*double.Parse(value, CultureInfo.InvariantCulture)
                                              : double.Parse(value, CultureInfo.InvariantCulture),
                     valid=1,
                     User = "parser",
                     TimeStamp =DateTime.Now,
                     Description =  row.Substring(109, 40).Trim()
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
            File.AppendAllText("log_" + timestamp.ToShortDateString() + ".txt", timestamp.ToShortDateString()+" "+message);
        }

        private int? getCPid(string cpname,Dictionary<string,int> cpdic)
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
                  var dbentity = new EXANTE_Entities();
                  dbentity.counterparties.Add(new counterparty
                  {Name =cpname});
                  dbentity.SaveChanges();
                  var cpidfromDb = from cp in dbentity.counterparties
                                      where cp.Name == cpname
                                      select cp.cp_id;
                  cpdic.Add(cpname,cpidfromDb.First());
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
                            (boitemlist[iBoitemlist].Qty.Equals(cpTrades[i].Qty)) && (!boitemlist[iBoitemlist].RecStatus))
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
            //    boTrades.Find(x => (string) prop_boTrades.GetValue(x, null) == value);
        }
        //        public int Method(Bar bar, string propertyName)
        // var prop = typeof(Bar).GetProperty(propertyName);
        //   int value = (int)prop.GetValue(bar,null);
        public class BOtrade
        {
            public long TradeNumber;
            public double Qty;
            public Double Price;
            public string symbol;
            public long ctradeid;
            public Boolean RecStatus;
        }

        private void button2_Click(object sender, EventArgs e)
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

                int rowCount = xlRange.Rows.Count+1;
                int colCount = xlRange.Columns.Count;
                DateTime reportdate = DateTime.FromOADate(xlRange.Cells[3, 8].value2);
               // reportdate = reportdate.AddDays(-1);
                var testexample = new EXANTE_Entities();
                var nextdate = AtonDate.Value.AddDays(1);
                var queryable =
                    from ct in testexample.Ctrades
                    where ct.Date >= reportdate && ct.Date < (nextdate) && ct.cp_id == "FORTS_TR"
                    select new {ct.trade_id, ct.tradeNumber,ct.qty, ct.price, ct.symbol_id, ct.fullid, ct.RecStatus};
                var botrades = new Dictionary<string, List<BOtrade>>();
                var n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    var ctrade_id = ctrade.trade_id.Replace("DC:F:", "");
                    var tempBotrade = new BOtrade
                        {
                            TradeNumber = (long) ctrade.tradeNumber,
                            Qty = (double) ctrade.qty,
                            Price = (double) ctrade.price,
                            symbol = ctrade.symbol_id,
                            ctradeid = ctrade.fullid,
                            RecStatus = ctrade.RecStatus
                        };
                    
                    if (botrades.ContainsKey(ctrade_id))
                    {
                        botrades[ctrade_id].Add(tempBotrade);
                    }
                    else botrades.Add(ctrade_id, new List<BOtrade> {tempBotrade}); //tempBotrade});
                }
             
                var allfromfile = new List<CpTrade>();
                for (int i = 10; i < rowCount; i++)
                {
                    if (xlRange.Cells[i, 4].value2 != null)
                    {
                        var tradeDate = DateTime.FromOADate(xlRange.Cells[i, 4].value2);
                        if (tradeDate.Date==reportdate.Date)
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
           
                foreach (var botrade in botrades){
                    foreach (var botradeItemlist in botrade.Value){
                      if (botradeItemlist.RecStatus){
                        using (var data = new EXANTE_Entities()){
                          data.Database.ExecuteSqlCommand("UPDATE Ctrades Set RecStatus ={0}  WHERE fullid = {1}", true, botradeItemlist.ctradeid);
                        }
                      }
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    testexample.CpTrades.Add(tradeIndex);
                }
                testexample.SaveChanges();

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpTrade_id = allfromfile[(int) reconitem.CpTrade_id].FullId;
                    testexample.Reconcilations.Add(reconitem);
                }
                testexample.SaveChanges();
                testexample.Dispose();
                ObjExcel.Quit(); 
                TradesParserStatus.Text = "Done:"+openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var testexample = new EXANTE_Entities();
                var reader = new StreamReader(openFileDialog2.FileName);
                var allfromfile = new List<CpTrade>();
                //Ticket Ref	Party	Type	Symbol	B/S	Amount	Currency	Rate	Counter Amount	Currency	Tenor	Value Date	Ticket Creation	Order Ref	GRID
                //EOD SWAP 201311190000/1127 FAR LEG	60002000000		NZDUSD	Sell	15 857.00	NZD	0.83218	13 195.88	USD	SPOT	21/11/2013	19/11/2013 06:18:55		
                var lineFromFile = reader.ReadLine();
                TradesParserStatus.Text = "Processing";
                var reportDate = openFileDialog2.FileName.Substring(openFileDialog2.FileName.IndexOf("_") + 1,
                                                                    openFileDialog2.FileName.LastIndexOf("-") -
                                                                    openFileDialog2.FileName.IndexOf("_") - 1);
                int idTradeDate = 13,
                    idSymbol = 3,
                    idQty = 5,
                    idSide = 4,
                    idPrice = 7,
                    idValueDate = 12,
                    idValue = 9,
                    idType = 11;
                IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-GB", true);
                var minDate = Convert.ToDateTime(reportDate);
                while (!reader.EndOfStream)
                {
                    lineFromFile = reader.ReadLine().Replace("\"", "");
                    var rowstring = lineFromFile.Split(',');
                    if (rowstring[1] != "")
                    {
                        var tradedate = Convert.ToDateTime(rowstring[idTradeDate], theCultureInfo);
                        var qty = rowstring[idSide].IndexOf("Buy") == -1
                                      ? Convert.ToDouble(rowstring[idQty].Replace(" ", ""))*(-1)
                                      : Convert.ToDouble(rowstring[idQty].Replace(" ", ""));
                        var ValueDate = Convert.ToDateTime(rowstring[idValueDate], theCultureInfo);
                        allfromfile.Add(new CpTrade
                            {
                                ReportDate = Convert.ToDateTime(reportDate),
                                TradeDate = tradedate,
                                BrokerId = "ADSSOREX",
                                Symbol = rowstring[idSymbol],
                                Type =  rowstring[idType],
                                Qty = qty,
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
                                exchangeOrderId = rowstring[idSymbol]+qty.ToString()+rowstring[idPrice].Replace(" ", "")
                            });

                        if ((rowstring[idType]=="Spot") && (tradedate < minDate)) minDate = tradedate;
                       
                    }
                }
                var nextdate = Convert.ToDateTime(reportDate);
                var startdate = new DateTime(minDate.Year,minDate.Month,minDate.Day,0,0,0);
                var queryable =
                  from ct in testexample.Ctrades
                  where ct.Date >=startdate  && ct.Date < (nextdate) && ct.cp_id == "ADSS_V2"
                  select new { ct.trade_id, ct.tradeNumber, ct.qty, ct.price, ct.symbol_id, ct.fullid, ct.RecStatus };
                var botrades = new Dictionary<string, List<BOtrade>>();
                var n = queryable.Count();
                foreach (var ctrade in queryable)
                {
                    var ctrade_id = ctrade.symbol_id.Replace(".EXANTE","")+ctrade.qty.ToString()+ctrade.price.ToString();
                    ctrade_id = ctrade_id.Replace("/", "");

                    var tempBotrade = new BOtrade
                    {
                        TradeNumber = (long)ctrade.tradeNumber,
                        Qty = (double)ctrade.qty,
                        Price = (double)ctrade.price,
                        symbol = ctrade.symbol_id,
                        ctradeid = ctrade.fullid,
                        RecStatus = ctrade.RecStatus
                    };

                    if (botrades.ContainsKey(ctrade_id))
                    {
                        botrades[ctrade_id].Add(tempBotrade);
                    }
                    else botrades.Add(ctrade_id, new List<BOtrade> { tempBotrade }); //tempBotrade});
                }
                var recon = Reconciliation(allfromfile, botrades, "exchangeOrderId", "2");
               
                 foreach (var botrade in botrades){
                    foreach (var botradeItemlist in botrade.Value){
                      if (botradeItemlist.RecStatus){
                        using (var data = new EXANTE_Entities()){
                          data.Database.ExecuteSqlCommand("UPDATE Ctrades Set RecStatus ={0}  WHERE fullid = {1}", true, botradeItemlist.ctradeid);
                        }
                      }
                    }
                }
                foreach (CpTrade tradeIndex in allfromfile)
                {
                    testexample.CpTrades.Add(tradeIndex);
                }
                testexample.SaveChanges();

                foreach (Reconcilation reconitem in recon)
                {
                    reconitem.CpTrade_id = allfromfile[(int) reconitem.CpTrade_id].FullId;
                    testexample.Reconcilations.Add(reconitem);
                }
                testexample.SaveChanges();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                var testexample = new EXANTE_Entities();
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
                                      ? Convert.ToDouble(rowstring[idQty].Replace(" ", "")) * (-1)
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
                    testexample.CpTrades.Add(tradeIndex);
                }
                testexample.SaveChanges();

            }
        }
    }
}
