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

namespace web_crawler
{
    public partial class WebCrawler : Form
    {
        delegate void SetTextCallback(string text);
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
            }
        }

        private void CrawlNamePage(Uri uri, String link)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument subDoc = new HtmlAgilityPack.HtmlDocument();

            subDoc = web.Load(uri.Scheme + "://" + uri.Host + link);
            
            HtmlNode name = subDoc.GetElementbyId("firstHeading");
            String word = name.SelectNodes(".//span").First().InnerText;
            HtmlNodeCollection wordMeaningNodes = subDoc.DocumentNode.SelectNodes("//dl//dd");

            AddText(word + ": \n");
            foreach (HtmlNode meaningNode in wordMeaningNodes)
            {
                AddText(meaningNode.InnerText + "\n");
            }
        }
        List<String> FindTopPages(String firstPageUri,String nextPageLinkValue)
        {
            List<String> topPages = new List<string>();
            Uri uri = new Uri(firstPageUri);
            topPages.Add(uri.LocalPath);
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            return topPages;
        }
        public WebCrawler()
        {
            InitializeComponent();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            Uri uri = new Uri("http://tr.wiktionary.org/wiki/Kategori:Soyad%C4%B1_(T%C3%BCrk%C3%A7e)");
            HtmlWeb web = new HtmlWeb();
            doc = web.Load(uri.OriginalString);
            htmlBox.Text += "Host:" + uri.Host + "\n";
            htmlBox.Text += "Local path:" + uri.LocalPath + "\n";
            HtmlNode node = doc.GetElementbyId("mw-pages");

            HtmlNodeCollection nextPageNodes = doc.DocumentNode.SelectNodes("//a[. = \"sonraki 200\"]");
            foreach(HtmlNode nextPageNode in nextPageNodes)
            htmlBox.Text += "Sonraki = " + nextPageNode.GetAttributeValue("href","/") + "\n";

            HtmlNodeCollection nameListNodes = node.SelectNodes(".//li");
            foreach (HtmlNode listNode in nameListNodes)
            {
                HtmlNode aNode = listNode.SelectNodes(".//a[@href]").First();
                String link = aNode.GetAttributeValue("href", "/");
                //  htmlBox.Text += uri.Scheme +"://" + uri.Host + link + "\n";
                Thread myThread = new Thread(() => this.CrawlNamePage(uri,link));
                myThread.Start();
            }
        }
    }
}
