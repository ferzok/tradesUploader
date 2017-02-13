using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class CommonFunctions
    {
        private static string _currentConnection;
      
       
        public CommonFunctions(string s)
        {
            _currentConnection = s;
        }
        
        public void SaveDBChanges(ref EXANTE_Entities db)
        {
            
            try
            {
                Task<int> result= db.SaveChangesAsync();
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

        public Dictionary<string, Map> getSymbolMap(string brockertype, string types)
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
            db.Dispose();
            return results;
        }

        public  Dictionary<string, Map> getMap(string brocker)
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
            var results = new Dictionary<string, CommonFunctions.Map>();
            var mapfromDblist = mapfromDb.ToList();
            foreach (var item in mapfromDblist)
            {
                string key = item.BrockerSymbol;

                if (brocker != "BO")
                {
                    key = item.BrockerSymbol + item.Type;
                }

                if (item.Type == "FU") key = key + item.ValueDate.Value.ToShortDateString();
                results.Add(key, new CommonFunctions.Map()
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

        public DateTime? getDatefromString(string lineFromFile, bool time = false)
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
        public void convertCptradesToInitTrades(List<CpTrade> ff, ref EXANTE_Entities db)
        {
            foreach (var cpTrade in ff)
            {
                db.InitialTrades.Add(new InitialTrade
                    {
                        Account = cpTrade.account,
                        BrokerId = cpTrade.BrokerId,
                        Symbol = cpTrade.Symbol,
                        Qty = cpTrade.Qty,
                        Price = cpTrade.Price,
                        ReportDate = cpTrade.ReportDate,
                        TradeDate = cpTrade.TradeDate,
                        Type = cpTrade.Type,
                        ValueDate = cpTrade.ValueDate,
                        value = cpTrade.value,
                        TypeOfTrade = cpTrade.TypeOfTrade,
                        exchangeOrderId = cpTrade.exchangeOrderId,
                        Timestamp = DateTime.UtcNow
                    });
            }
            SaveDBChanges(ref db);
        }

        public  void SendToDb<T>(ref EXANTE_Entities db, List<T> data)
        {
            foreach (T VARIABLE in data)
            {
                db.Set(typeof (T)).Add(VARIABLE);
            }
            SaveDBChanges(ref db);
        }
    }
}