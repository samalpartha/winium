using log4net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace project.Mapping
{
    public static class Mapper
    {
        public static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string BASE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"../../") + "MapJSON/";
        private static string BASE_EXT = ".json";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventActivity"></param>
        /// <param name="elementFound"></param>
        /// <returns></returns>
        public static JToken readJSON(MapMethod eventActivity, string elementFound)
        {
            JToken foundElement = null;
            var json = File.ReadAllText(BASE_PATH + MapMethodTyper.jsonConvert(eventActivity) + BASE_EXT);
            JObject objects = JObject.Parse(json);
            foundElement = objects.GetValue(clearTurkishCharsAndUpperCase(elementFound));
            Assert.IsFalse(foundElement == null, elementFound + " is not found in " + Mapping.MapMethodTyper.jsonConvert(eventActivity) + " file");
            return foundElement;
        }

        /// <summary>
        /// Turkish Character Converter
        /// </summary>
        /// <param name="elementString"></param>
        /// <returns></returns>
        public static string clearTurkishCharsAndUpperCase(string elementString)
        {
            string returnString = elementString;

            Encoding srcEncoding = Encoding.UTF8;
            Encoding destEncoding = Encoding.GetEncoding(1252); // Latin alphabet

            returnString = destEncoding.GetString(Encoding.Convert(srcEncoding, destEncoding, srcEncoding.GetBytes(returnString)));

            string normalizedString = returnString.Normalize(NormalizationForm.FormD);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                if (!CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]).Equals(UnicodeCategory.NonSpacingMark))
                {
                    result.Append(normalizedString[i]);
                }
            }
            returnString = result.ToString();
            return returnString.ToUpper(new CultureInfo("en-US", false)).Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventActivity"></param>
        /// <param name="elementFound"></param>
        /// <returns></returns>
        public static IDictionary<By, int> foundActivity(MapMethod eventActivity, string elementFound)
        {
            var set = new Dictionary<By, int>();
            By by = null;
            int index = -1;
            JToken foundToken = readJSON(eventActivity, elementFound);
            foreach (JToken jobject in foundToken)
            {
                JProperty typeProperty = (JProperty)jobject;
                if (typeProperty.Name.Equals("index"))
                {
                    int intIndex = (Int32)typeProperty.Value;
                    index = intIndex;
                }
                else
                {
                    by = generateByElement(typeProperty.Name, typeProperty.Value.ToString());
                }

            }
            set.Add(by, index);
            return set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byType"></param>
        /// <param name="byValue"></param>
        /// <returns></returns>
        static By generateByElement(string byType, string byValue)
        {
            By byElement = null;
            if (byType.Contains("id"))
            {
                byElement = By.Id(byValue);
            }
            else if (byType.Contains("className"))
            {
                byElement = By.ClassName(byValue);
            }
            else if (byType.Contains("cssSelector"))
            {
                byElement = By.CssSelector(byValue);
            }
            else if (byType.Contains("xpath"))
            {
                byElement = By.XPath(byValue);
            }
            else if (byType.Contains("link-text"))
            {
                byElement = By.LinkText(byValue);
            }
            else if (byType.Contains("contains"))
            {
                byElement = By.XPath("//*[contains(@id, '" + byValue + "')]");
            }
            else
            {
                Assert.Fail("No such selector.");
            }
            return byElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventActivity"></param>
        /// <param name="queryFound"></param>
        /// <returns></returns>
        public static IList<String> foundSQLActivity(MapMethod eventActivity, string queryFound)
        {
            var set = new List<String>();
            JToken foundToken = readJSON(eventActivity, queryFound);
            foreach (JToken jobject in foundToken)
            {
                JProperty typeProperty = (JProperty)jobject;
                if (typeProperty.Name.Equals("query"))
                {
                    set.Add(typeProperty.Value.ToString());
                }
            }
            return set;
        }
    }
    }
