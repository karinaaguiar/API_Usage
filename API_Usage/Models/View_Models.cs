using System.Collections.Generic;

namespace API_Usage.Models
{
    //Class created for the purpose of handle Financials response
    public class FinanceResponse
    {
        public string symbol;
        public List<Financial> financials;
    }

    public class SymbolsInfo
    {
        public List<Company> Companies { get; set; }
        public Company Current { get; set; }
        public string Dates { get; set; }
        public string Prices { get; set; }
        public string Volumes { get; set; }
        public float AvgPrice { get; set; }
        public double AvgVolume { get; set; }
        public float openPrice { get; set; }
        public float closePrice { get; set; }
        public float lowPrice { get; set; }
        public float highPrice { get; set; }

        public SymbolsInfo()
        {
        }

        public SymbolsInfo(List<Company> companies, Company current,
                                              string dates, string prices, string volumes,
                                              float avgprice, double avgvolume)
        {
            Companies = companies;
            Current = current;
            Dates = dates;
            Prices = prices;
            Volumes = volumes;
            AvgPrice = avgprice;
            AvgVolume = avgvolume;
        }
    }

    public class News
    {
        public string datetime { get; set; }
        public string headline { get; set; }
        public string source { get; set; }
        public string url { get; set; }
        public string summary { get; set; }
        public string related { get; set; }
        public string image { get; set; }
    }
}