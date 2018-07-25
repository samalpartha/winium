using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project.Mapping
{
    public class MapValues
    {
        private string id { get; set; }
        private string class_name { get; set; }
        private string link_text { get; set; }
        private string css_selector { get; set; }
        private string xpath { get; set; }
        private string contains { get; set; }
        private int index { get; set; }

        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public string ClassName
        {
            get
            {
                return class_name;
            }

            set
            {
                class_name = value;
            }
        }

        public string LinkText
        {
            get
            {
                return link_text;
            }

            set
            {
                link_text = value;
            }
        }

        public string CssSelector
        {
            get
            {
                return css_selector;
            }

            set
            {
                css_selector = value;
            }
        }

        public string Xpath
        {
            get
            {
                return xpath;
            }

            set
            {
                xpath = value;
            }
        }

        public string Contains
        {
            get
            {
                return contains;
            }

            set
            {
                contains = value;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }
    }
}
