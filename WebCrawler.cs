using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Threading;
using System.Web;
using System.IO;
using System.Diagnostics;
namespace web_crawler
{
    public partial class WebCrawler : Form
    {
        delegate void SetTextCallback(string text);
        private static Semaphore _pool;
        FileStream fs;
        StreamWriter sw;
        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }
        private void AddText(string text)
        {

            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.htmlBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AddText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.htmlBox.Text += text;
                this.sw.Write(text);
                this.sw.Flush();
            }
        }

        private void CrawlNamePage(Uri uri, String link)
        {
            try
            {
                //AddText("Crawling Name Page: " + uri + "\n\r");
                HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument subDoc = new HtmlAgilityPack.HtmlDocument();
                Boolean processed = false;

                while (!processed)
                {
                    try
                    {
                        subDoc = web.Load(uri.Scheme + "://" + uri.Host + link);
                        processed = true;
                    }
                    catch (Exception e)
                    {
                        AddText("reconnecting \n" + uri.AbsolutePath + "\n");
                    }
                }

                HtmlNode name = subDoc.GetElementbyId("firstHeading");
                String word = name.SelectNodes(".//span").First().InnerText;
                HtmlNodeCollection wordMeaningNodes = subDoc.DocumentNode.SelectNodes("//dl//dd");
                String wordAndMeanings = "";
                wordAndMeanings += word + ": \n";
                foreach (HtmlNode meaningNode in wordMeaningNodes)
                {
                    wordAndMeanings += meaningNode.InnerText + "\n";
                }

                AddText(wordAndMeanings);
            }
            catch (Exception e)
            {
                AddText(e.StackTrace);
            }

            _pool.Release();
        }

        /// <summary>   Searches for the first names. </summary>
        ///
        /// <param name="topPagePath">  Full pathname of the top page html. </param>
        void FindNames(String topPagePath)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HtmlWeb web = new HtmlWeb();
            doc = web.Load(topPagePath);
            Uri topPageUri = new Uri(topPagePath);
            HtmlNode node = doc.GetElementbyId("mw-pages");

            HtmlNodeCollection nameListNodes = node.SelectNodes(".//li");
            foreach (HtmlNode listNode in nameListNodes)
            {
                HtmlNode aNode = listNode.SelectNodes(".//a[@href]").First();
                String link = aNode.GetAttributeValue("href", "/");
                Uri uri = new Uri(topPageUri.Scheme + "://" + topPageUri.Host + HttpUtility.HtmlDecode(link));
                _pool.WaitOne();
                Thread myThread = new Thread(() => this.CrawlNamePage(uri,link));
                //CrawlNamePage(uri, link);
                myThread.Start();
            }
            _pool.Release();
        }

        /// <summary>   Searches for the top pages. </summary>
        ///
        /// <param name="firstPageUri">         URI of the first page. </param>
        /// <param name="nextPageLinkValue">    The next page link value. </param>
        ///
        /// <returns>   The found top pages. </returns>

        List<String> FindTopPages(String firstPageUri,String nextPageLinkValue)
        {
            AddText("Top Pages Working");
            /// <summary>   The top pages. </summary>
            List<String> topPages = new List<string>();
            /// <summary>   URI of the document. </summary>
            Uri uri = new Uri(firstPageUri);
            topPages.Add(uri.LocalPath);
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(uri.OriginalString);
            //doc.DocumentNode.SelectSingleNode("//a[. = \"sonraki 200\"]");
           // AddText(nextPageXPath);
           // AddText(doc.DocumentNode.InnerHtml);
            HtmlNode nextPageNode = doc.DocumentNode.SelectSingleNode("//a[. = \"sonraki 200\"]");
           
            while (nextPageNode != null)
            {
                String topPage = nextPageNode.GetAttributeValue("href", "/");
                topPages.Add(topPage);
                String requestUri = uri.Scheme + "://" + uri.Host +HttpUtility.HtmlDecode(topPage) + "\n";
                HtmlAgilityPack.HtmlDocument doc2 = web.Load(requestUri);
                
                AddText("Toppage URI:" + requestUri);
                
                nextPageNode = doc2.DocumentNode.SelectSingleNode("//a[. = \"sonraki 200\"]");
                
            }

            foreach (String topPage in topPages)
            {
                _pool.WaitOne();
                Thread worker = new Thread(() => this.FindNames(uri.Scheme + "://" + uri.Host + HttpUtility.HtmlDecode(topPage)));

                worker.Start();
            }
            return topPages;
        }

        public WebCrawler()
        {
            InitializeComponent();
            _pool = new Semaphore(1000, 1000);
            fs = new FileStream("logfile.txt", FileMode.Create, FileAccess.ReadWrite);
            if (fs == null)
                htmlBox.Text = "File Stream Error!!";
            else
            {
                sw = new StreamWriter(fs);
                if (sw != null)
                {
                    AddText("INIT");
                    Thread crawlerThread = new Thread(() => this.Crawl());
                    crawlerThread.Start();
                }
            }
        }
        private void Crawl(){
            AddText("Crawling");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            Uri uri = new Uri("http://tr.wiktionary.org/wiki/Kategori:Soyad%C4%B1_(T%C3%BCrk%C3%A7e)");
            HtmlWeb web = new HtmlWeb();

            FindTopPages(uri.OriginalString, "sonraki 200");
        }

        private void htmlBox_TextChanged(object sender, EventArgs e)
        {
            
            htmlBox.ScrollToCaret();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           // threadCountLabel.Text += "aa";
            threadCountLabel.Text= Process.GetCurrentProcess().Threads.Count.ToString();
        }
    }
}
