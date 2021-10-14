using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amazon_WebScraper
{
    public abstract class PageGetter
    {
        internal IWebDriver Driver;
        internal void LoadPage(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }
        public void Dispose()
        {
            Driver.Quit();
        }
    }

    public interface IPageQuery
    {
        List<string> Results { get; }
        void Run(string url);
    }

    public class TestQuery : PageGetter, IPageQuery
    {
        public List<string> Results { get; } = new List<string>();
        public TestQuery()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments(new string[] { "--headless", "--disable-gpu" });
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            Driver = new ChromeDriver(ChromeDriverService.CreateDefaultService("C:\\WebDriver\\bin", "chromedriver.exe"), options);
        }

        public void Run(string pUrll)
        {
            string url = pUrll;
            int pageNum = 1;

            string GetString(IWebElement e, string t, string a, string s)
            {
                IWebElement r = ((RemoteWebElement)e).FindElementsByTagName(t).Where(e1 =>
                {
                    string s1 = e1.GetAttribute(a);
                    return s1.Equals(s);
                }).First();
                return t == "a" ? r.GetAttribute("href") : r.Text;
            }
            List<IWebElement> GetList(string t, string a, string s)
            {
                return new List<IWebElement>(((ChromeDriver)Driver).FindElementsByTagName(t).Where(e =>
                {
                    string s1 = e.GetAttribute(a);

                    return t == "li" ? s1.Equals(s) : !string.IsNullOrEmpty(s1) && s1.StartsWith(s);
                }));
            }

            try
            {

                while (!(url is null))
                {
                    Console.Clear();
                    Console.WriteLine("Processing page " + pageNum.ToString());
                    LoadPage(url);
                    GetList("span", "cel_widget_id", "MAIN-SEARCH_RESULTS").ForEach(e =>
                     {
                         string desc = GetString(e, "span", "class", "a-size-medium a-color-base a-text-normal");

                         if (desc.Contains("M+A Matting") && desc.Contains("Eco Elite Fashion") && desc.Contains("10' Length x 3' Width"))
                         {
                             string url = GetString(e, "a", "class", "a-link-normal a-text-normal");
                             string price = GetString(e, "span", "class", "a-price");
                             price = price.Replace("\r\n", ".");
                             Results.Add(desc + ";" + url + ";" + price);
                         }
                     });
                    url = GetList("li", "class", "a-last").Count == 0 ? 
                        null : 
                        ((RemoteWebElement)GetList("li", "class", "a-last").First()).FindElementByTagName("a").GetAttribute("href");
                    ++pageNum;
                }
            }

            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine("Error!");
                Console.WriteLine(e.Message);
                Console.WriteLine("Stack Trace");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Press any key to continue");
                Console.Read();
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            StreamWriter writer = new StreamWriter(string.Concat("c:\\Users\\", Environment.UserName, "\\Documents\\results.txt"), false);
            string url = string.Empty;
            TestQuery query = new TestQuery();


            while (url == string.Empty)
            {
                Console.Clear();
                Console.WriteLine("Please enter the search link.");
                url = Console.ReadLine();
            }
 
            try
            {
                query.Run(url);
                query.Results.ForEach(e => writer.WriteLine(e));
            }

            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine("Error!");
                Console.WriteLine(e.Message);
                Console.WriteLine("Stack Trace");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Press any key to continue");
                Console.Read();
            }

            finally
            {
                //Quit the web driver, which will close the browser and disposes of the web driver.
                query.Dispose();
                //Close the results file.
                writer.Close();
                Console.Clear();
                Console.WriteLine("Press any key to continue...");
                Console.Read();
            }
        }
    }
}
