using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Usage.Models
{
    public class Company
    {
        [Key]
        public string symbol { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string logo { get; set; }
        public List<ChartElement> ChartElements { get; set; }
        public Financial Financials { get; set; }
        public KeyStat KeyStats { get; set; }
        public List <Dividend> Dividends { get; set; }
    }

    public class ChartElement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChartElementId { get; set; }
        public string symbol { get; set; }
        public string date { get; set; }
        public float open { get; set; }
        public float high { get; set; }
        public float low { get; set; }
        public float close { get; set; }
        public long volume { get; set; }
        public long unadjustedVolume { get; set; }
        public float change { get; set; }
        public float changePercent { get; set; }
        public float vwap { get; set; }
        public string label { get; set; }
        public float changeOverTime { get; set; }
    }

    public class Financial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FinancialId { get; set; }
        public string symbol { get; set; }
        public string reportDate { get; set; }
        public long grossProfit { get; set; }
        public long costOfRevenue { get; set; }
        public long operatingRevenue { get; set; }
        public long totalRevenue { get; set; }
        public long operatingIncome { get; set; }
        public long netIncome { get; set; }
        public long researchAndDevelopment { get; set; }
        public long operatingExpense { get; set; }
        public long currentAssets { get; set; }
        public long totalAssets { get; set; }
        public long totalLiabilities { get; set; }
        public long currentCash { get; set; }
        public long currentDebt { get; set; }
        public long totalCash { get; set; }
        public long totalDeb { get; set; }
        public long shareholderEquity { get; set; }
        public long cashChange { get; set; }
        public long cashFlow { get; set; }
        public long operatingGainsLosses { get; set; }
    }

    public class KeyStat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeyStatId { get; set; }
        public string symbol { get; set; }
        public long marketcap { get; set; }
        public float beta { get; set; }
        public float week52high { get; set; }
        public float week52low { get; set; }
        public float week52change { get; set; }
        public long shortInterest { get; set; }
        public string shortDate { get; set; }
        public float dividendRate { get; set; }
        public float dividendYield { get; set; }
        public string exDividendDate { get; set; }
        public float latestEPS { get; set; }
        public string latestEPSDate { get; set; }
        public long sharesOutstanding { get; set; }
        public float returnOnEquity { get; set; }
        public float consensusEPS { get; set; }
        public long numberOfEstimates { get; set; }
        public long EBITDA { get; set; }
        public long revenue { get; set; }
        public long grossProfit { get; set; }
        public long cash { get; set; }
        public long debt { get; set; }
        public float ttmEPS { get; set; }
        public float revenuePerShare { get; set; }
        public float revenuePerEmployee { get; set; }
        public float peRatioHigh { get; set; }
        public float peRatioLow { get; set; }
        public float EPSSurpriseDollar { get; set; }
        public float EPSSurprisePercent { get; set; }
        public float returnOnAssets { get; set; }
        public float returnOnCapital { get; set; }
        public float profitMargin { get; set; }
        public float priceToSales { get; set; }
        public float priceToBook { get; set; }
        public float day200MovingAvg { get; set; }
        public float day50MovingAvg { get; set; }
        public float institutionPercent { get; set; }
        public float insiderPercent { get; set; }
        public float shortRatio { get; set; }
        public float year5ChangePercent { get; set; }
        public float year2ChangePercen { get; set; }
        public float year1ChangePercent { get; set; }
        public float ytdChangePercent { get; set; }
        public float month6ChangePercent { get; set; }
        public float month3ChangePercent { get; set; }
        public float month1ChangePercent { get; set; }
        public float day5ChangePercent { get; set; }
    }

    public class Dividend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DividendId { get; set; }
        public string symbol { get; set; }
        public string exDate { get; set; }
        public string paymentDate { get; set; }
        public string recordDate { get; set; }
        public string declaredDate { get; set; }
        public float amount { get; set; }
        public string type { get; set; }
        public string qualified { get; set; }
    }

    public class UpcomingIPO
    {
        [Key]
        public string symbol { get; set; }
        public string company { get; set; }
        public string price { get; set; }
        public string shares { get; set; }
        public string amount { get; set; }
        public string floatValue { get; set; }
        public string percent { get; set; }
        public string market { get; set; }
        public string expected { get; set; }
        
    }
}