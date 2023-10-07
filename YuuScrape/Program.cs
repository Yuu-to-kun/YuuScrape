using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using YuuScrape;
using static System.Net.WebRequestMethods;

public class Program
{
    private static void Main(string[] args)
    {
        string link = "https://mangasee123.com/search/?sort=s&desc=false&name=akame";

        var driver = startDriver();
        string html = getSeriesHtml(link,driver);

        List<string> classes = new List<string> {
            "top-15","ng-scope","SeriesName","ng-binding","NoResults"
        };

        List<Serie> Series = new List<Serie>();

        HtmlDocument htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        if (htmlDocument.DocumentNode.SelectSingleNode($"//div[contains(@class, '{classes[4]}')]") != null)
        {
            Console.WriteLine("No series");
            return;
        }

        HtmlNodeCollection specificLink = htmlDocument.DocumentNode.SelectNodes($"//div[contains(@class, '{classes[0]}') and contains(@class, '{classes[1]}')]");
        
        foreach ( HtmlNode node in specificLink )
        {
            Serie serie = new Serie();

            //Load the whole div
            HtmlDocument div = new HtmlDocument();
            div.LoadHtml(node.OuterHtml);

            //Fetch Manga Name
            HtmlNode SerieName = div.DocumentNode.SelectSingleNode($"//a[contains(@class, '{classes[2]}') and contains(@class, '{classes[3]}')]");
            serie.Name = SerieName.InnerText;

            //Fetch Manga link
            HtmlNode SerieHyperLink = div.DocumentNode.SelectSingleNode($"//a[contains(@class, '{classes[2]}')]");
            serie.Link = SerieHyperLink.GetAttributeValue("href", "");

            //Fetch Manga Image link
            HtmlDocument hyperLink = new HtmlDocument();
            hyperLink.LoadHtml(SerieHyperLink.OuterHtml);
            HtmlNode Img = hyperLink.DocumentNode.SelectSingleNode($"//img");
            serie.ImageLink = Img.GetAttributeValue("src", "");

            //Fetch Manga Author
            HtmlNode SerieAuthorSpan = div.DocumentNode.SelectSingleNode($"//span[contains(@ng-repeat, 'Author in Series.a')]");
            HtmlDocument SerieAuthor = new HtmlDocument();
            SerieAuthor.LoadHtml(SerieAuthorSpan.OuterHtml);
            HtmlNode Author = SerieAuthor.DocumentNode.SelectSingleNode("//a");
            serie.Author = Author.InnerText;

            Series.Add(serie);
        }
        Console.Clear();
        foreach (var serie in Series)
        {
            Console.WriteLine($"Name: {serie.Name}");
            Console.WriteLine($"Author: {serie.Author}");
            Console.WriteLine($"MangaLink: https://mangasee123.com{serie.Link}");
            Console.WriteLine($"ImageLink: {serie.ImageLink}\n");
        }

        Console.WriteLine("Done");
        Console.ReadKey();
    }

    public static string getSeriesHtml(string link,ChromeDriver driver)
    {
        driver.Navigate().GoToUrl(link);
        WebDriverWait waitDriver = new WebDriverWait(driver, TimeSpan.FromMicroseconds(1));
        waitDriver.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        ((IJavaScriptExecutor)driver).ExecuteScript(@"
            document.querySelectorAll('style, script, comment, head').forEach(el => el.remove());
            document.querySelectorAll('*').forEach(el => {
                for (let i = el.childNodes.length - 1; i >= 0; i--) {
                    const child = el.childNodes[i];
                    if (child.nodeType === 8) {
                        el.removeChild(child);
                    }
                }
            });
        ");
        return ((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.outerHTML").ToString() ?? String.Empty;
        
    }

    public static ChromeDriver startDriver()
    {
        ChromeOptions options = new ChromeOptions();
        options.AddArgument("--headless");
        IWebDriver driver = new ChromeDriver(options);
        
        return (ChromeDriver)driver;
    }

}