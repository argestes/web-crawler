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
        /// <summary>   Callback, called when the text is . </summary>
        ///
        /// <param name="text"> The text. </param>

        delegate void AddTextCallback(string text);

        /// <summary>   Callback, writes a word to file. </summary>
        ///
        /// <param name="word"> The word. </param>

        delegate void WriteWordCallback(Word word);

        /// <summary>   The thread pool. </summary>
        private static Semaphore _pool;


        /// <summary>   Database file. </summary>
        FileStream dBaseFile;
        
        /// <summary>   Database writer. </summary>
        StreamWriter dBaseWriter;

        /// <summary>   The file stream. </summary>
        FileStream fs;

        /// <summary>   The stream writer for log file. </summary>
        StreamWriter logFileStreamWriter;

        /// <summary>   Strips off the meaning. </summary>
        ///
        /// <param name="meaning">  The meaning. </param>
        ///
        /// <returns>   . </returns>

        private static String StripMeaning(String meaning)
        {
            int index = meaning.IndexOf(']');
            return meaning.Substring(index + 2);
        }
        private void WriteToFile(Word w)
        {
            
            AddText("Writing to file\n");
            lock (dBaseWriter)
            {
                dBaseWriter.WriteLine("<Kavram>");
                dBaseWriter.WriteLine("\t<Kavramadi>" + w.Name + "</Kavramadi>");
                dBaseWriter.WriteLine("\t<Anlamsayi>" + w.Meanings.Count.ToString() + "</Anlamsayi>");

                int i = 1;
                foreach (String meaning in w.Meanings)
                {
                    AddText(StripMeaning(meaning));
                    dBaseWriter.WriteLine("\t<Anlam" + i + ">" + StripMeaning(meaning) + "</Anlam" + i + ">");
                    i++;
                }

                dBaseWriter.WriteLine("</Kavram>\n");
                dBaseWriter.Flush();

            }
            AddText("File operation complete\n");
        }

        /// <summary>   Adds a text to htmlBox </summary>
        ///
        /// <param name="text"> The text. </param>

        private void AddText(string text)
        {

            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.htmlBox.InvokeRequired)
            {
                AddTextCallback d = new AddTextCallback(AddText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                //write to rich text box.
                this.htmlBox.Text += text;
                //write to log file.
                this.logFileStreamWriter.Write(text);
                this.logFileStreamWriter.Flush();
            }
        }

        /// <summary>   Crawl name page. </summary>
        ///
        /// <param name="uri">  URI of the first page. </param>
        /// <param name="link"> The link. </param>

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
                        AddText(e.StackTrace + "\n");
                        //AddText("reconnecting \n" + uri.AbsolutePath + "\n");
                    }
                }

                HtmlNode name = subDoc.GetElementbyId("firstHeading");
                String word = name.SelectNodes(".//span").First().InnerText;
                HtmlNodeCollection wordMeaningNodes = subDoc.DocumentNode.SelectNodes("//dl//dd");
                
                String wordAndMeanings = "";
                Word w = new Word(word);
                wordAndMeanings += word + ": \n";
                foreach (HtmlNode meaningNode in wordMeaningNodes)
                {
                    wordAndMeanings += meaningNode.InnerText + "\n";
                    w.Meanings.Add(meaningNode.InnerText);
                }
                WriteToFile(w);
                AddText(wordAndMeanings);
            }
            catch (Exception e)
            {
                AddText(e.StackTrace);
            }

            _pool.Release();
        }



        /// <summary>   Searches for names. </summary>
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
                Thread myThread = new Thread(() => this.CrawlNamePage(uri, link));
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

        List<String> FindTopPages(String firstPageUri, String nextPageLinkValue)
        {
            AddText("Top Pages Working");

            List<String> topPages = new List<string>();

            Uri uri = new Uri(firstPageUri);
            topPages.Add(uri.LocalPath);
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(uri.OriginalString);
            HtmlNode nextPageNode = doc.DocumentNode.SelectSingleNode("//a[. = \"sonraki 200\"]");

            while (nextPageNode != null)
            {
                String topPage = nextPageNode.GetAttributeValue("href", "/");
                topPages.Add(topPage);
                String requestUri = uri.Scheme + "://" + uri.Host + HttpUtility.HtmlDecode(topPage) + "\n";
                HtmlAgilityPack.HtmlDocument doc2 = web.Load(requestUri);



                nextPageNode = doc2.DocumentNode.SelectSingleNode("//a[. = \"sonraki 200\"]");

            }

            foreach (String topPage in topPages)
            {
                AddText("Toppage URI:" + uri.Scheme + "://" + uri.Host + HttpUtility.HtmlDecode(topPage));
                _pool.WaitOne();
                Thread worker = new Thread(() => this.FindNames(uri.Scheme + "://" + uri.Host + HttpUtility.HtmlDecode(topPage)));

                worker.Start();
            }
            return topPages;
        }


        /// <summary>   Default constructor. </summary>
        public WebCrawler()
        {
            InitializeComponent();
            _pool = new Semaphore(1000, 1000);
            fs = new FileStream("logfile.txt", FileMode.Create, FileAccess.ReadWrite);
            if (fs == null)
                htmlBox.Text = "File Stream Error!!";
            else
            {
                logFileStreamWriter = new StreamWriter(fs);
                if (logFileStreamWriter != null)
                {
                    AddText("INIT\n");
                    Thread crawlerThread = new Thread(() => this.Crawl());
                    crawlerThread.Start();
                }
            }
        }

        /// <summary>   Actual method </summary>
        private void Crawl()
        {

            dBaseFile = new FileStream("Soyad.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.Write);
            
            dBaseWriter = new StreamWriter(dBaseFile);
            
            if (dBaseWriter == null)
                AddText("DBASEWRITER = NULL!!!\n");
            else
                AddText("DbaseWriter initialized\n");
            AddText("Crawling");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            Uri uri = new Uri("http://tr.wiktionary.org/wiki/Kategori:Soyad%C4%B1_(T%C3%BCrk%C3%A7e)");
            HtmlWeb web = new HtmlWeb();

            FindTopPages(uri.OriginalString, "sonraki 200");
        }

        /// <summary>   Event handler. Called by htmlBox for text changed events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>

        private void htmlBox_TextChanged(object sender, EventArgs e)
        {
            htmlBox.SelectionStart = htmlBox.Text.Length;
            htmlBox.ScrollToCaret();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            threadCountLabel.Text = Process.GetCurrentProcess().Threads.Count.ToString();
        }
    }
}
