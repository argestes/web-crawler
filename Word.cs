using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace web_crawler
{
    class Word
    {
        public Word(String name)
        {
            this.Name = name;
            this.Meanings = new List<string>();
        }
        public String Name
        {
            get;
            set;
        }
        public List<String> Meanings
        {
            get;
            set;
        }

    }
}
