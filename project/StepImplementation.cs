using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using OpenQA.Selenium.Winium;
using OpenQA.Selenium;
using log4net;
using System.Threading;
using System.Text;
using System.Diagnostics;
using project.Pages;
using project.Mapping;

namespace project
{
    public class StepImplementation
    {
        public WiniumDriver driver;
        public static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected Dictionary<string, string> userVariables = new Dictionary<string, string>();
        protected Dictionary<string, string> saveEnv = new Dictionary<string, string>();
        static WiniumDriverService service = null;
        static DesktopOptions options = null;
        private BasePage bsp;

        [BeforeScenario]
        public void setup()
        {
            service = WiniumDriverService.CreateDesktopService(@"C:\oto");
            options = new DesktopOptions { ApplicationPath = @"C:\Windows\System32\calc.exe" };
            driver = new WiniumDriver(service, options, TimeSpan.FromSeconds(30));
            bsp = new BasePage(driver);
        }

        /// <summary>
        /// Click by element
        /// </summary>
        /// <param name="button"></param>
        [Step("Click --- <by>")]
        public void clickElementBy(string button)
        {
            //bsp.untilElementAppear(getElementFromJSON(MapMethod.Click, button));
            driver.FindElement(getElementFromJSON(MapMethod.Click, button)).Click();
            //bsp.clickJS(getElementFromJSON(MapMethod.Click, button));
            //bsp.waitAll();
        }

        /// <summary>
        /// Fill in by area with text
        /// </summary>
        /// <param name="by"></param>
        /// <param name="text"></param>
        [Step("Type <text> to --- <by>")]
        public void fillInputFields(string by, string text)
        {
            bsp.sendKeys(getElementFromJSON(MapMethod.Input, by), isTextAParameter(text));
        }

        /// <summary>
        /// Wait by element to be visible
        /// </summary>
        /// <param name="by"></param>
        [Step("Check element visible --- <by>")]
        public void checkElementVisible(string by)
        {
            try
            {
                bsp.untilElementAppear(getElementFromJSON(MapMethod.IsElement, by));
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                bsp.assertFail(by + " elementi bulunamadý");
            }
            //bsp.assertionTrue(by + " elementi bulunamadý", bsp.isElementPresent(getElementFromJSON(MapMethod.IsElement, by))
        }

        /// <summary>
        /// Wait by element to be invisible
        /// </summary>
        /// <param name="by"></param>
        [Step("Check element invisible --- <by>")]
        public void checkElementNotVisible(string by)
        {
            bool isVisible = false;
            if (!bsp.isElementPresent(getElementFromJSON(MapMethod.IsElement, by)))
            {
                isVisible = true;
            }
            else
            {
                bsp.assertionTrue(by + " elementi görülmemesi gerekirken görüldü", isVisible);
            }
        }

        /// <summary>
        /// Switch iFrame
        /// </summary>
        /// <param name="area"></param>
        [Step("Switch frame --- <by>")]
        public void switchSpecificFrame(string area)
        {
            // Json dosyasýna taþýnabilir ve parametrik olan method kullanýlabilir.
            //bsp.switchIFrameSpecific(getElementFromJSON(MapMethod.Frame, "ZERO FRAME"));
            try
            {
                bsp.switchIFrameSpecific(getElementFromJSON(MapMethod.Frame, area));
                //bsp.waitFrameSwicth(1);
            }
            catch (Exception e)
            {
                driver.SwitchTo().DefaultContent().SwitchTo().Frame(bsp.findElement(getElementFromJSON(MapMethod.Frame, "MAIN")));
                bsp.waitFrameSwicth(1);
                driver.SwitchTo().Frame(bsp.findElement(getElementFromJSON(MapMethod.Frame, area)));
                log.Info(e.Message);
            }
        }

        /// <summary>
        /// Pick text string from table/list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="text"></param>
        [Step("Pick <text> from --- <list>")]
        public void selectElementFromList(string list, string text)
        {
            string selectedString = isTextAParameter(text);
            bsp.untilElementAppear(getElementFromJSON(MapMethod.IsElement, list));
            if (getElementFromJSON(MapMethod.IsElement, list).ToString().Contains("DataTable") || getElementFromJSON(MapMethod.IsElement, list).ToString().Contains("table"))
            {
                bool isClickable = true;
                for (int i = 1; i < 3; i++)
                {
                    if (isClickable)
                    {
                        IList<IWebElement> listElement = driver.FindElement(getElementFromJSON(MapMethod.IsElement, list)).FindElements(By.CssSelector("tbody > tr > td:nth-child(" + i + ")"));
                        foreach (var elementInList in listElement)
                        {
                            log.Info(elementInList.Text);
                            if (elementInList.Text.Contains(selectedString))
                            {
                                bsp.clickNative(elementInList);
                                isClickable = false;
                                break;
                            }
                        }
                    }
                }

            }
            else
            {
                bsp.selectOptionText(getElementFromJSON(MapMethod.IsElement, list), selectedString);
            }

        }
        
        /// <summary>
        /// Check text that comes from element equal to desired text
        /// </summary>
        /// <param name="by"></param>
        /// <param name="text"></param>
        [Step("Get element's text --- <by> & check text is equal to <text>")]
        public void getElementText(string by, string text)
        {
            bsp.untilElementAppear(getElementFromJSON(MapMethod.IsElement, by));
            string getText = null;
            if (!String.IsNullOrEmpty(bsp.getText(getElementFromJSON(MapMethod.IsElement, by))))
            {
                getText = bsp.findElement(getElementFromJSON(MapMethod.IsElement, by)).Text;
            }
            if (!String.IsNullOrEmpty(driver.FindElement(getElementFromJSON(MapMethod.IsElement, by)).GetAttribute("innerText")))
            {
                getText = driver.FindElement(getElementFromJSON(MapMethod.IsElement, by)).GetAttribute("innerText");
            }
            if (!String.IsNullOrEmpty(bsp.getAttributeValue(getElementFromJSON(MapMethod.IsElement, by))))
            {
                getText = bsp.getAttributeValue(getElementFromJSON(MapMethod.IsElement, by));
            }
            bsp.assertionFalse(by + " alanýndan yazý null geldi", String.IsNullOrEmpty(getText));
            log.Info(getText);
            bsp.assertionTrue(by + " alanýndan gelen deðer " + text + " deðerine eþit deðil. Gelen text : " + getText, getText.Contains(text));
        }

        /// <summary>
        /// Wait i second
        /// </summary>
        /// <param name="i"></param>
        [Step("Static wait in <i> second")]
        public void waitSeconds(int i)
        {
            Thread.Sleep(TimeSpan.FromSeconds(i));
        }

        /// <summary>
        /// Check is element selected
        /// </summary>
        /// <param name="button"></param>
        [Step("Is element selected? --- <by>")]
        public void isElementSelected(string button)
        {
            bsp.elementSelectedControl(getFromJSON(MapMethod.IsElement, button).Key, getFromJSON(MapMethod.IsElement, button).Value);
        }


        /// <summary>
        /// Pick i'th element from the table/list 
        /// </summary>
        /// <param name="byList"></param>
        /// <param name="index"></param>

        [Step("Pick <i>'th element from --- <by>")]
        public void clickNthElementFromList(string byList, int index)
        {
            //bsp.untilElementAppear(getElementFromJSON(MapMethod.IsElement, byList));
            bsp.assertionTrue(byList + " listesi bulunamadý", bsp.isElementPresent(getElementFromJSON(MapMethod.IsElement, byList)));
            IList<IWebElement> listElement = null;
            if (!getElementFromJSON(MapMethod.IsElement, byList).ToString().Contains("Link"))
            {
                listElement = bsp.findElement(getElementFromJSON(MapMethod.IsElement, byList)).FindElements(By.CssSelector("tbody > tr"));
            }
            else
            {
                listElement = bsp.findElements(getElementFromJSON(MapMethod.IsElement, byList));
            }
            log.Info(listElement.Count);
            bsp.assertionFalse(byList + " listesinde " + index + " numaralý element mevcut deðildir", (index - 1) > listElement.Count);
            bsp.clickNative(listElement[index - 1]);

        }


        public void switchIframe(string frame)
        {
            driver.SwitchTo().Frame(bsp.findElement(getElementFromJSON(MapMethod.Frame, frame)));
        }

        /// <summary>
        /// Double click to by element
        /// </summary>
        /// <param name="by"></param>
        [Step("Double click --- <by>")]
        public void doubleClick(string by)
        {
            bsp.doubleClick(getElementFromJSON(MapMethod.Click, by));
        }


        /// <summary>
        /// By return from Json
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public By getElementFromJSON(MapMethod activity, string text)
        {
            List<By> byList = new List<By>(Mapper.foundActivity(activity, text).Keys);
            return byList[0];

        }



        /// <summary>
        /// B - Eliminates extra whitespace.
        /// </summary>
        protected string replaceSQL(string sql)
        {
            StringBuilder replacedSQL = new StringBuilder(sql);
            foreach (var element in userVariables)
            {
                replacedSQL.Replace(element.Key, element.Value);
            }

            return replacedSQL.ToString();
        }

        /// <summary>
        /// 
        /// By return from Json with index
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public KeyValuePair<By, int> getFromJSON(Mapping.MapMethod activity, string text)
        {
            List<By> byList = new List<By>(Mapper.foundActivity(activity, text).Keys);
            List<int> byIndex = new List<int>(Mapper.foundActivity(activity, text).Values);

            return new KeyValuePair<By, int>(byList[0], byIndex[0]);
        }


        /// <summary>
        /// Check element is in parameter status
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string isTextAParameter(string text)
        {
            string typo = text;
            if (text.StartsWith("@"))
            {
                //typo = saveEnv[text];
                typo = replaceSQL(text);
                log.Info(typo);
            }
            return typo;
        }

        /// <summary>
        /// Teardown
        /// </summary>
        [AfterScenario]
        public void tearDown()
        {
            //String cmdText = "/C taskkill /IM calculator.exe /F";
            //System.Diagnostics.Process.Start("cmd.exe", cmdText);

            foreach (var process in Process.GetProcessesByName("Calculator"))
            {
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("Winium.Desktop.Driver"))
            {
                process.Kill();
            }
            //driver.Quit();
        }


        //public static WiniumDriver Driver
        //{
        //    get
        //    {
        //        return driver;
        //    }

        //    set
        //    {
        //        driver = value;
        //    }
        //}
    }
}
