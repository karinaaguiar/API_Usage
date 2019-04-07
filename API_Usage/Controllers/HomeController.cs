using Microsoft.AspNetCore.Mvc;
using API_Usage.DataAccess;
using API_Usage.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

/*
 * Acknowledgments
 *  v1 of the project was created for the Fall 2018 class by Dhruv Dhiman, MS BAIS '18
 *    This example showed how to use v1 of the IEXTrading API
 *    
 *  Kartikay Bali (MS BAIS '19) extended the project for Spring 2019 by demonstrating 
 *    how to use similar methods to access Azure ML models
*/

namespace API_Usage.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;

        //Base URL for the IEXTrading API. Method specific URLs are appended to this base URL.
        string BASE_URL = "https://api.iextrading.com/1.0/";
        HttpClient httpClient;

        /// <summary>
        /// Initialize the database connection and HttpClient object
        /// </summary>
        /// <param name="context"></param>
        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BASE_URL);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new
                System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        //Q-STOCK WEBSITE METHODS

        public IActionResult Index()
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            List<News> news = GetNews();

            //Save news in TempData, so they do not have to be retrieved again
            TempData["News"] = JsonConvert.SerializeObject(news);

            return View(news);
        }

        public List<News> GetNews()
        {
            string IEXTrading_API_PATH = "stock/market/news";
            string newsList = "";
            List<News> news = null;

            // connect to the IEXTrading API and retrieve information
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                newsList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!newsList.Equals(""))
            {
                news = JsonConvert.DeserializeObject<List<News>>(newsList);
            }

            return news;
        }

        public IActionResult Watchlists()
        {
            //Set ViewBag variable first
            /*ViewBag.dbSucessComp = 0;
            List<News> news = GetNews();

            //Save companies in TempData, so they do not have to be retrieved again
            TempData["News"] = JsonConvert.SerializeObject(news);

            return View(news);*/
            return View();
        }

        /****
        * The Symbols action calls the GetSymbols method that returns a list of Companies.
        * This list of Companies is passed to the Symbols View.
        ****/
        public IActionResult Symbols(string symbol)
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;

            //If the Company table is empty, it populates it from API
            List<Company> companies = new List<Company>();
            int symbolsCounter = dbContext.Companies.Count();
            if (symbolsCounter == 0)
            {
                companies = GetSymbols();
                foreach (Company comp in companies)
                {
                    dbContext.Companies.Add(comp);
                }
                dbContext.SaveChanges();
            }
            //If the Company table has data already, then just pull it
            if (companies.Count() == 0)
            {
                companies = dbContext.Companies.ToList();
            }

            //Check if the user requested information for a specific company
            Company company = null;
            string dates = "";
            string prices = "";
            string volumes = "";
            float avgprice = 0;
            double avgvol = 0;
            if (symbol != null)
            {
                //Get all the symbol information
                company = dbContext.Companies.Where(c => c.symbol == symbol).First();

                //Verify if the company has all information complete
                if (company.ChartElements == null)
                {
                    //Get info from API and automatically store it on the database.
                    company.ChartElements = getChartElements(symbol);
                }
                if (company.Financials == null)
                {
                    //Get info from API and automatically store it on the database.
                    company.Financials = getFinancials(symbol);
                }
                if (company.KeyStats == null)
                {
                    //Get info from API and automatically store it on the database.
                    company.KeyStats = getKeyStats(symbol);
                }
                if (company.Dividends == null)
                {
                    //Get info from API and automatically store it on the database.
                    company.Dividends = getDividends(symbol);
                }

                //Use the company's ChartElement to Generate appropriately formatted 
                //strings for use by chart.js
                dates = string.Join(",", company.ChartElements.Select(e => e.date));
                prices = string.Join(",", company.ChartElements.Select(e => e.high));
                avgprice = company.ChartElements.Average(e => e.high);
                //Divide volumes by million to scale appropriately
                volumes = string.Join(",", company.ChartElements.Select(e => e.volume / 1000000));
                avgvol = company.ChartElements.Average(e => e.volume) / 1000000;
            }

            //Create the View model empty
            SymbolsInfo symbolsInfo = new SymbolsInfo(companies, company, dates, prices, volumes, avgprice, avgvol);

            return View(symbolsInfo);
        }

        /// <summary>
        /// Calls the IEX reference API to get the list of symbols
        /// </summary>
        /// <returns>A list of the companies whose information is available</returns>
        public List<Company> GetSymbols()
        {
            string IEXTrading_API_PATH = "ref-data/symbols";
            string companyList = "";
            List<Company> companies = null;

            // connect to the IEXTrading API and retrieve information
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                companyList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // now, parse the Json strings as C# objects
            if (!companyList.Equals(""))
            {
                // https://stackoverflow.com/a/46280739
                //JObject result = JsonConvert.DeserializeObject<JObject>(companyList);
                companies = JsonConvert.DeserializeObject<List<Company>>(companyList);
                companies = companies.GetRange(0, 500);
            }

            return companies;
        }

        public List<ChartElement> getChartElements(string symbol)
        {
            // string to specify information to be retrieved from the API
            string IEXTrading_API_PATH = "stock/" + symbol + "/chart/1y";

            // initialize objects needed to gather data
            string charts = "";
            List<ChartElement> ChartElements = new List<ChartElement>();

            // connect to the API and obtain the response
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // now, obtain the Json objects in the response as a string
            if (response.IsSuccessStatusCode)
            {
                charts = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // parse the string into appropriate objects
                if (!charts.Equals(""))
                {
                    List<ChartElement> root = JsonConvert.DeserializeObject<List<ChartElement>>(charts,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    //Make sure the data is in ascending order of date
                    ChartElements = root.OrderBy(c => c.date).ToList();

                    // The symbol serves as the foreign key in the database and connects the 
                    // chartElement to the company
                    // Also save the data on the Database
                    foreach (ChartElement ChartElement in ChartElements)
                    {
                        ChartElement.symbol = symbol;
                        dbContext.ChartElements.Add(ChartElement);
                    }
                    dbContext.SaveChanges();
                }
            }

            return ChartElements;
        }

        public Financial getFinancials(string symbol) {
            // string to specify information to be retrieved from the API
            string IEXTrading_API_PATH = "stock/" + symbol + "/financials";

            // initialize objects needed to gather data
            string financialsString = "";
            Financial Financials = new Financial();

            // connect to the API and obtain the response
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // now, obtain the Json objects in the response as a string
            if (response.IsSuccessStatusCode)
            {
                financialsString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                
                // parse the string into appropriate objects
                if (!financialsString.Equals(""))
                {
                    FinanceResponse  FinanResponse = JsonConvert.DeserializeObject<FinanceResponse>(financialsString,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Financials = FinanResponse.financials.OrderBy(f => f.reportDate).Last();
                    // The symbol serves as the foreign key in the database and connects the 
                    // financials to the company
                    // Also save the data on the Database
                    Financials.symbol = symbol;
                    dbContext.Financials.Add(Financials);
                    dbContext.SaveChanges();
                }
            }

            return Financials;
        }

        public KeyStat getKeyStats(string symbol)
        {
            // string to specify information to be retrieved from the API
            string IEXTrading_API_PATH = "stock/" + symbol + "/stats";

            // initialize objects needed to gather data
            string keyStatString = "";
            KeyStat KeyStats = new KeyStat();

            // connect to the API and obtain the response
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // now, obtain the Json objects in the response as a string
            if (response.IsSuccessStatusCode)
            {
                keyStatString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // parse the string into appropriate objects
                if (!keyStatString.Equals(""))
                {
                     KeyStats = JsonConvert.DeserializeObject<KeyStat>(keyStatString,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    // The symbol serves as the foreign key in the database and connects the 
                    // keyStat to the company
                    // Also save the data on the Database
                    KeyStats.symbol = symbol;
                    dbContext.KeyStats.Add(KeyStats);
                    dbContext.SaveChanges();
                }
            }

            return KeyStats;
        }

        public List<Dividend> getDividends(string symbol)
        {
            // string to specify information to be retrieved from the API
            string IEXTrading_API_PATH = "stock/" + symbol + "/dividends/1y";

            // initialize objects needed to gather data
            string dividendList = "";
            List<Dividend> Dividends = new List<Dividend>();


            // connect to the API and obtain the response
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // now, obtain the Json objects in the response as a string
            if (response.IsSuccessStatusCode)
            {
                dividendList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                // parse the string into appropriate objects
                if (!dividendList.Equals(""))
                {
                    Dividends = JsonConvert.DeserializeObject<List<Dividend>>(dividendList,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    //Make sure the data is in ascending order of date
                    Dividends = Dividends.OrderBy(c => c.paymentDate).ToList();
                }

                // The symbol serves as the foreign key in the database and connects the 
                // Dividends to the company
                // Also save the data on the Database
                foreach (Dividend Dividend in Dividends)
                {
                    Dividend.symbol = symbol;
                    dbContext.Dividends.Add(Dividend);
                }
                dbContext.SaveChanges();
            }

            return Dividends;
        }

        
        public IActionResult About()
        {
            return View();
        }

        //METHODS FOR REFERENCE

        /****
         * The Chart action calls the GetChart method that returns 1 year's equities for the passed symbol.
         * A ViewModel CompaniesEquities containing the list of companies, prices, volumes, avg price and volume.
         * This ViewModel is passed to the Chart view.
        ****/
        /// <summary>
        /// The Chart action calls the GetChart method that returns 1 year's equities for the passed symbol.
        /// A ViewModel CompaniesEquities containing the list of companies, prices, volumes, avg price and volume.
        /// This ViewModel is passed to the Chart view.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /*public IActionResult Chart(string iexId)
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessChart = 0;
            List<Equity> equities = new List<Equity>();

            if (iexId != null)
            {
                equities = GetChart(iexId);
                equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
            }

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View(companiesEquities);
        }*/

        /// <summary>
        /// Calls the IEX stock API to get 1 year's chart for the supplied symbol
        /// </summary>
        /// <param name="symbol">Stock symbol of the company whose quotes are to be retrieved</param>
        /// <returns></returns>
        /*public List<Equity> GetChart(string symbol)
        {
            // string to specify information to be retrieved from the API
            string IEXTrading_API_PATH = BASE_URL + "stock/" + symbol + "/batch?types=chart&range=1y";

            // initialize objects needed to gather data
            string charts = "";
            List<Equity> Equities = new List<Equity>();
            httpClient.BaseAddress = new Uri(IEXTrading_API_PATH);

            // connect to the API and obtain the response
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // now, obtain the Json objects in the response as a string
            if (response.IsSuccessStatusCode)
            {
                charts = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            // parse the string into appropriate objects
            if (!charts.Equals(""))
            {
                ChartRoot root = JsonConvert.DeserializeObject<ChartRoot>(charts,
                  new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Equities = root.chart.ToList();
            }

            // fix the relations. By default the quotes do not have the company symbol
            //  this symbol serves as the foreign key in the database and connects the quote to the company
            foreach (Equity Equity in Equities)
            {
                Equity.symbol = symbol;
            }

            return Equities;
        }*/

        /// <summary>
        /// Call the ClearTables method to delete records from a table or all tables.
        ///  Count of current records for each table is passed to the Refresh View
        /// </summary>
        /// <param name="tableToDel">Table to clear</param>
        /// <returns>Refresh view</returns>
        /*public IActionResult Refresh(string tableToDel)
        {
            ClearTables(tableToDel);
            Dictionary<string, int> tableCount = new Dictionary<string, int>();
            tableCount.Add("Companies", dbContext.Companies.Count());
            tableCount.Add("Charts", dbContext.Equities.Count());
            return View(tableCount);
        }*/

        /// <summary>
        /// save the quotes (equities) in the database
        /// </summary>
        /// <param name="symbol">Company whose quotes are to be saved</param>
        /// <returns>Chart view for the company</returns>
        /*public IActionResult SaveCharts(string symbol)
        {
            List<Equity> equities = GetChart(symbol);

            // save the quote if the quote has not already been saved in the database
            foreach (Equity equity in equities)
            {
                if (dbContext.Equities.Where(c => c.date.Equals(equity.date)).Count() == 0)
                {
                    dbContext.Equities.Add(equity);
                }
            }

            // persist the data
            dbContext.SaveChanges();

            // populate the models to render in the view
            ViewBag.dbSuccessChart = 1;
            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View("Chart", companiesEquities);
        }*/

        /// <summary>
        /// Use the data provided to assemble the ViewModel
        /// </summary>
        /// <param name="equities">Quotes to dsiplay</param>
        /// <returns>The view model to include </returns>
        /*public CompaniesEquities getCompaniesEquitiesModel(List<Equity> equities)
        {
            List<Company> companies = dbContext.Companies.ToList();

            if (equities.Count == 0)
            {
                return new CompaniesEquities(companies, null, "", "", "", 0, 0);
            }

            Equity current = equities.Last();

            // create appropriately formatted strings for use by chart.js
            string dates = string.Join(",", equities.Select(e => e.date));
            string prices = string.Join(",", equities.Select(e => e.high));
            float avgprice = equities.Average(e => e.high);

            //Divide volumes by million to scale appropriately
            string volumes = string.Join(",", equities.Select(e => e.volume / 1000000));
            double avgvol = equities.Average(e => e.volume) / 1000000;

            return new CompaniesEquities(companies, equities.Last(), dates, prices, volumes, avgprice, avgvol);
        }*/

        /// <summary>
        /// Save the available symbols in the database
        /// </summary>
        /// <returns></returns>
        public IActionResult PopulateSymbols()
        {
            // retrieve the companies that were saved in the symbols method
            // saving in TempData is extremely inefficient - the data circles back from the browser
            // better methods would be to serialize to the hard disk, or save directly into the database
            //  in the symbols method. This example has been structured to demonstrate one way to save object data
            //  and retrieve it later
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());

            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;
            return View("Symbols", companies);
        }

        /// <summary>
        /// Delete all records from tables
        /// </summary>
        /// <param name="tableToDel">Table to clear</param>
        /*public void ClearTables(string tableToDel)
        {
            if ("all".Equals(tableToDel))
            {
                //First remove equities and then the companies
                dbContext.Equities.RemoveRange(dbContext.Equities);
                dbContext.Companies.RemoveRange(dbContext.Companies);
            }
            else if ("Companies".Equals(tableToDel))
            {
                //Remove only those companies that don't have related quotes stored in the Equities table
                dbContext.Companies.RemoveRange(dbContext.Companies
                                                         .Where(c => c.Equities.Count == 0)
                                                                      );
            }
            else if ("Charts".Equals(tableToDel))
            {
                dbContext.Equities.RemoveRange(dbContext.Equities);
            }
            dbContext.SaveChanges();
        }*/

        public List<UpcomingIPO> GetUpcomingIPOs()
        {
            string IEXTrading_API_PATH = "stock/market/upcoming-ipos";
            string upcomingIPOsList = "";
            List<UpcomingIPO> upcomingIPOs = null;

            // connect to the IEXTrading API and retrieve information
            HttpResponseMessage response = httpClient.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();

            // read the Json objects in the API response
            if (response.IsSuccessStatusCode)
            {
                upcomingIPOsList = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            string[] data = upcomingIPOsList.Split("viewData");
            // now, parse the Json strings as C# objects
            if (!upcomingIPOsList.Equals(""))
            {
                upcomingIPOs = JsonConvert.DeserializeObject<List<UpcomingIPO>>(data[1].Substring(2, data[1].Length - 3));
            }
            List<string> symbols = dbContext.UpcomingIPOs.Select(x => x.symbol).ToList();
            foreach (string symbol in symbols)
            {
                dbContext.UpcomingIPOs.Remove(dbContext.UpcomingIPOs.Where(c => c.symbol == symbol).First());
            }
            dbContext.SaveChanges();
            foreach (UpcomingIPO ipo in upcomingIPOs)
            {
                dbContext.UpcomingIPOs.Add(ipo);
            }
            dbContext.SaveChanges();

            return upcomingIPOs;
        }


        public IActionResult IPO()
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            List<UpcomingIPO> ipos = GetUpcomingIPOs();
            return View("ipo", ipos);
        }
    }
}