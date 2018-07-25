using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium.Winium;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using project.Utility;

namespace project.Pages
{
    public class BasePage
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly WaitTool wTool;
        //  protected IWebDriver driver;
        protected WebDriverWait wait;

        public WiniumDriver driver;


        public BasePage(IWebDriver driver)
        {
            var service = WiniumDriverService.CreateDesktopService(@"C:\oto");
            var options = new DesktopOptions { ApplicationPath = @"C:\Windows\System32\calc.exe" };
            driver = new WiniumDriver(service, options, TimeSpan.FromMinutes(30));
        }

        public bool hasNumber(string input)
        {
            return input.Where(x => Char.IsDigit(x)).Any();
        }

        /// <summary>
        /// Get javascript executor
        /// </summary>
        /// <returns></returns>
        public IJavaScriptExecutor getJSExecutor()
        {
            return (IJavaScriptExecutor)driver;
        }
        /// <summary>
        /// Javascript executor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public Object executeJS(string script, Boolean wait)
        {
            return wait ? getJSExecutor().ExecuteScript(script, "") : getJSExecutor().ExecuteAsyncScript(script, "");
        }
        /// <summary>
        /// Javascript executor
        /// </summary>
        /// <param name="script"></param>
        /// <param name="obj"></param>
        public void executeJS(string script, params Object[] obj)
        {
            getJSExecutor().ExecuteScript(script, obj);
        }

        /// <summary>
        /// Javascript executor boolean
        /// </summary>
        /// <param name="script"></param>
        /// <returns>boolean</returns>
        public Boolean executeBoolJS(string script)
        {
            return "true".Equals(executeJS(script, true).ToString());

        }

        /// <summary>
        /// Null element refence message
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void nullElementException(By by, params int[] index)
        {
            StringBuilder nullException = new StringBuilder();
            nullException.Append("ELEMENT (");
            nullException.Append(by);
            nullException.Append(",");
            nullException.Append(index.Length > 0 ? index[0] : 0);
            nullException.Append(") NOT EXISTS; AUTOMATION DATAS MAY BE INVALID!");
#pragma warning disable S112 // General exceptions should never be thrown
            throw new NullReferenceException(nullException.ToString());
#pragma warning restore S112 // General exceptions should never be thrown
        }
        /// <summary>
        /// Wait for the page to load
        /// </summary>
        public void waitAll()
        {
            wTool.waitDomReady();
            wTool.waitJQuery();
            wTool.waitAjax();
        }

        /// <summary>
        /// Find WebElement Content
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public IWebElement findElement(By by, params int[] index)
        {
            waitAll();
            if (index.Length == 0 || index[0].Equals(-1))
            {
                log.Info("Aranılan Element : " + by);
                return wait.Until(ExpectedConditions.ElementExists(by));
            }
            else if (index[0] >= 0)
            {
                IList<IWebElement> elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
                if (elements.Any() && index[0] <= elements.Count)
                {
                    return elements[index[0]];
                }
            }
            return null;
        }

        /// <summary>
        /// Find WebElement Content No Wait
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public IWebElement findElementNoWait(By by, params int[] index)
        {
            //waitAll();
            if (index.Length == 0 || index[0].Equals(-1))
            {
                return driver.FindElement(by);
            }
            else if (index[0] >= 0)
            {
                IList<IWebElement> elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
                if (elements.Any() && index[0] <= elements.Count)
                {
                    return elements[index[0]];
                }
            }
            return null;
        }

        /// <summary>
        /// Find WebElements
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public IList<IWebElement> findElements(By by, params int[] index)
        {
            waitAll();
            return driver.FindElements(by);
        }

        /// <summary>
        /// Click web element (By by, int index(getIndex))
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void click(By by, params int[] index)
        {
            IWebElement element = null;
            try
            {
                element = findElement(by, index);
            }
            catch (Exception e)
            {
                log.Error("HATA ! :", e);
                assertFail("Element Bulunamadı ! :" + e.Message);
            }
            if (element == null)
            {
                nullElementException(by, index);
            }
            else
            {
                if (!isElementDisplayed(by, index))
                {
                    scrollTo(element.Location.X, element.Location.Y);
                }
                log.Info("Tıklanılan element :" + element);
                element.Click();
            }
        }


        /// <summary>
        /// Click web element with JavaScript main method
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void clickJS(IWebElement element)
        {
            executeJS("arguments[0].click();", element);
        }

        //var elem=arguments[0]; setTimeout(function() {elem.click();}, 100)

        /// <summary>
        /// Click web element with JavaScript main method
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void clickJSAnother(IWebElement element)
        {
            string mouseOverScript = "if(document.createEvent){var evObj = document.createEvent('MouseEvents');evObj.initEvent('mouseover',true, false); arguments[0].dispatchEvent(evObj);} else if(document.createEventObject){arguments[0].fireEvent('onmouseover');}";
            string onClickScript = "if(document.createEvent){var evObj = document.createEvent('MouseEvents');evObj.initEvent('click',true, false); arguments[0].dispatchEvent(evObj);} else if(document.createEventObject){ arguments[0].fireEvent('onclick');}";
            executeJS(mouseOverScript, element);
            executeJS(onClickScript, element);

        }

        /// <summary>
        /// Click web element with native mouse event
        /// </summary>
        /// <param name="element"></param>
        public void clickNative(IWebElement element)
        {
            waitAll();
            Actions builder = new Actions(driver);
            waitAll();
            builder.MoveToElement(element).Perform();
            waitAll();
            builder.Click(element).Perform();
            waitAll();
        }
        /// <summary>
        /// Click web element with JavaScript
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void clickJS(By by, params int[] index)
        {
            clickJS(findElement(by, index));
        }

        /// <summary>
        /// Click web element with JavaScript
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void clickElementJS(IWebElement element)
        {
            executeJS("arguments[0].click();", element);
        }

        /// <summary>
        /// Click element with java script
        /// </summary>
        /// <param name="clickElement"></param>
        public void clickWithJS(IWebElement clickElement)
        {
            executeJS("var evt = document.createEvent('MouseEvents');"
                + "evt.initMouseEvent('click', true, true, windows, 0, 0, 0, 0, 0, false, false, false, false, 0, null);"
                + "arguments[0].dispatchEvent(evt);", clickElement);
        }

        /// <summary>
        /// Click element with java script override by
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void clickWithJS(By by, params int[] index)
        {
            clickWithJS(findElement(by, index));
        }

        /// <summary>
        /// Doubleclick element with actions override by
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void doubleClick(By by, params int[] index)
        {
            Actions builder = new Actions(driver);
            builder.DoubleClick(findElement(by, index)).Build().Perform();
        }

        /// <summary>
        /// Send keys element
        /// </summary>
        /// <param name="by"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public void sendKeys(By by, string value, params int[] index)
        {
            IWebElement element = null;
            try
            {
                element = findElement(by, index);
            }
            catch (Exception e)
            {
                log.Error("HATA ! :", e);
                assertFail("Element Bulunamadı ! :" + e.Message);
            }
            if (element == null)
            {
                nullElementException(by, index);
            }
            else if (element.Enabled)
            {
                log.Info("Element dolduruluyor :" + value + "-" + element);
                element.Clear();
                Thread.Sleep(500);
                untilElementClickable(by);
                element.SendKeys(value);
            }
        }


        /// <summary>
        /// Find Select Menu 
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public SelectElement selectOption(By by, params int[] index)
        {
            return new SelectElement(findElement(by, index));
        }

        /// <summary>
        /// Dropdown menu select by text
        /// </summary>
        /// <param name="by"></param>
        /// <param name="text"></param>
        /// <param name="index"></param>
        public void selectOptionText(By by, string text, params int[] index)
        {
            try
            {
                selectOption(by, index).SelectByText(text);
            }
            catch (NoSuchElementException exText)
            {
                log.Debug(exText.Message, exText);
                try
                {
                    selectOption(by, index).SelectByValue(text);
                }
                catch (NoSuchElementException exValue)
                {
                    log.Error(exValue.Message, exValue);
                    assertFail(exValue.Message);
                }
            }
        }

        /// <summary>
        /// Dropdown menu select by value
        /// </summary>
        /// <param name="by"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public void selectOptionValue(By by, string value, params int[] index)
        {
            selectOption(by, index).SelectByValue(value);
        }


        /// <summary>
        /// Get select menu list
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public IList<IWebElement> selectOptionsList(By by)
        {
            return selectOption(by).Options;
        }

        /// <summary>
        /// Dropdown menu first select get
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public IWebElement selectOptionFirstSelect(By by)
        {
            return selectOption(by).SelectedOption;
        }

        /// <summary>
        /// Dropdown menu get selected text
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public String selectOptionFirstSelectText(By by)
        {
            return selectOption(by).SelectedOption.Text;
        }

        /// <summary>
        /// Get element attribute
        /// </summary>
        /// <param name="by"></param>
        /// <param name="attr"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getAttribute(By by, string attr, params int[] index)
        {
            log.Info("Özelliği Aranılan Element :" + attr + "-" + by);
            return findElement(by, index).GetAttribute(attr);
        }

        /// <summary>
        /// Get element attribute for value
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getAttributeValue(By by, params int[] index)
        {
            log.Info("Özelliği Aranılan Element : value -" + by);
            return getAttribute(by, "value", index);
        }

        /// <summary>
        /// Get page url
        /// </summary>
        /// <returns></returns>
        public string getCurrentUrl()
        {
            string currentUrl = driver.Url.Trim();
            log.Info("Bulunan Url :" + currentUrl);
            return currentUrl;
        }

        /// <summary>
        /// Get page source
        /// </summary>
        /// <returns></returns>
        public string getPageSource()
        {
            return driver.PageSource;
        }

        /// <summary>
        /// Get page title
        /// </summary>
        /// <returns></returns>
        public string getTitle()
        {
            string title = driver.Title;
            log.Info("Bulunan Sayfa Başlığı :" + title);
            return title;
        }

        /// <summary>
        /// Get element text
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getText(By by, params int[] index)
        {
            return findElement(by, index).Text;
        }

        /// <summary>
        /// Get element list size
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public int getSize(By by)
        {
            return findElements(by).Count;
        }

        /// <summary>
        /// Get lenght text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int getLenghtText(string text)
        {
            return text.Length;
        }

        /// <summary>
        /// Handle alert popup
        /// </summary>
        public void alertPopup(Boolean acceptAndDismiss)
        {
            wait.Until(ExpectedConditions.AlertIsPresent());
            IAlert alert = driver.SwitchTo().Alert();
            if (acceptAndDismiss)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
        }


        /// <summary>
        /// Switch to iframe by
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public IWebDriver switchIFrame(By by)
        {
            // TODO : Test edilmesi gerekiyor...
            driver.SwitchTo().DefaultContent();
            //wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(by))
            return driver.SwitchTo().Frame(findElement(by));
        }
        public IWebDriver switchIFrameSpecific(By by)
        {
            // TODO : Test edilmesi gerekiyor...

            //wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(by))
            return driver.SwitchTo().Frame(findElement(by));
        }

        /// <summary>
        /// Switch to iframe
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IWebDriver switchIFrame(int index)
        {
            driver.SwitchTo().DefaultContent();
            return driver.SwitchTo().Frame(index);
        }

        /// <summary>
        /// Navigate Url
        /// </summary>
        /// <param name="url"></param>
        public void navigate(string url)
        {
            log.Info("Gidilen Url :" + url);
            driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        ///  Get Page
        /// </summary>
        /// <param name="url"></param>
        public void getPage(string url)
        {
            driver.Url = url;
        }

        /// <summary>
        ///  Call the Page
        /// </summary>
        /// <param name="url"></param>
        public void callPage(string page)
        {
            driver.Navigate().GoToUrl(getCurrentUrl() + page);
        }

        /// <summary>
        /// Refresh Page
        /// </summary>
        public void refreshPage()
        {
            log.Info("Sayfa Yenilendi.");
            driver.Navigate().Refresh();
        }

        /// <summary>
        /// Back Page
        /// </summary>
        public void goBack()
        {
            log.Info("Geri Gidildi.");
            driver.Navigate().Back();
        }

        /// <summary>
        /// Scroll to element
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void scrollTo(int x, int y)
        {
            string jsStmt = String.Format("window.scrollTo({0}, {1})", x, y);
            executeJS(jsStmt, true);
        }

        /// <summary>
        /// Scroll to element
        /// </summary>
        /// <param name="element"></param>
        public void scrollToElement(IWebElement element)
        {
            if (element != null)
            {
                scrollTo(element.Location.X, element.Location.Y);
            }
        }

        /// <summary>
        /// Assertion control(true)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="condition"></param>
        public void assertionTrue(string message, Boolean condition)
        {
            Assert.IsTrue(condition, message);
        }

        /// <summary>
        /// Assertion control(true)
        /// </summary>
        /// <param name="condition"></param>
        public void assertionTrue(Boolean condition)
        {
            Assert.IsTrue(condition);
        }

        /// <summary>
        /// Assertion control(false)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="condition"></param>
        public void assertionFalse(string message, Boolean condition)
        {
            Assert.IsFalse(condition, message);
        }

        /// <summary>
        /// Assertion control(false)
        /// </summary>
        /// <param name="condition"></param>
        public void assertionFalse(Boolean condition)
        {
            Assert.IsFalse(condition);
        }

        /// <summary>
        /// Assertion control(Equals)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public void assertionEquals(string message, object expected, object actual)
        {
            Assert.AreEqual(expected, actual, message);
        }

        /// <summary>
        /// Assertion Fail
        /// </summary>
        /// <param name="message"></param>
        public void assertFail(string message)
        {
            Assert.Fail(message);
        }

        /// <summary>
        /// Checking the presence of qualification
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Boolean isElementDisplayed(By by, params int[] index)
        {
            try
            {
                return findElementNoWait(by, index).Displayed;

            }
            catch (NoSuchElementException e)
            {
                log.Debug(e.Message, e);
                return false;
            }
        }

        /// <summary>
        /// Checking the presence of qualification
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Boolean isElemenEnabled(By by, params int[] index)
        {
            try
            {
                return findElement(by, index).Enabled;
            }
            catch (NoSuchElementException e)
            {
                log.Debug(e.Message, e);
                return false;
            }
        }

        /// <summary>
        /// Checking the presence of qualification
        /// </summary>
        /// <param name="by"></param>
        /// <returns></returns>
        public Boolean isElementPresent(By by)
        {
            return findElements(by).Any();
        }

        /// <summary>
        /// Text Contains Control
        /// </summary>
        /// <param name="by"></param>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Boolean isTextContains(By by, string text, params int[] index)
        {
            try
            {
                return getText(by, index).Contains(text);
            }
            catch (NullReferenceException e)
            {
                log.Error(e.Message, e);
                return false;
            }
            finally
            {
                log.Info("Aranılan Metin :" + text);
                log.Info("Bulunan Metin :" + getText(by, index));
            }
        }

        /// <summary>
        /// Text Equals Control
        /// </summary>
        /// <param name="by"></param>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Boolean isTextEquals(By by, string text, params int[] index)
        {
            try
            {
                return getText(by, index).Equals(text);
            }
            catch (NullReferenceException e)
            {
                log.Error(e.Message, e);
                return false;
            }
            finally
            {
                log.Info("Aranılan Metin :" + text);
                log.Info("Bulunan Metin :" + getText(by, index));
            }
        }

        /// <summary>
        /// Text Present Control
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Boolean isTextPresent(String text)
        {
            try
            {
                return getPageSource().Contains(text);
            }
            catch (NullReferenceException e)
            {
                log.Error("Does Not Exists Text", e);
                return false;
            }
        }

        /// <summary>
        /// Element Appear
        /// </summary>
        public void untilElementAppear(By by)
        {
            log.Info(by + " ilgili elementin görünür olması bekleniyor...");
            wait.Until(ExpectedConditions.ElementIsVisible(by));

        }

        /// <summary>
        /// Element Appear Specific
        /// </summary>
        public void untilElementAppearSpecific(By by)
        {
            try
            {
                log.Info(by + " ilgili elementin görünür olması bekleniyor...");
                WebDriverWait waitSpecific = new WebDriverWait(this.driver, TimeSpan.FromMinutes(1));
                waitSpecific.Until(ExpectedConditions.ElementIsVisible(by));

            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                assertFail(by.ToString() + " bulunamadı");
            }

        }

        public bool isElementAppear(By by)
        {
            try
            {
                log.Info(by + " ilgili elementin görünür olması bekleniyor...");
                wait.Until(ExpectedConditions.ElementIsVisible(by));
                return true;
            }
            catch (Exception e)
            {
                log.Debug(e.Message, e);
                return false;
            }

        }

        /// <summary>
        /// Element Appear
        /// </summary>
        public void untilElementClickable(By by)
        {
            try
            {
                log.Info(by + " ilgili elementin tıklanabilir olması bekleniyor...");
                wait.Until(ExpectedConditions.ElementToBeClickable(by));
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                assertFail(by.ToString() + " tıklanır duruma gelmedi.");
            }

        }

        /// <summary>
        /// Element Appear
        /// </summary>
        public void untilTextToBePresent(By by, String text)
        {
            try
            {
                log.Info(by + " ilgili elementte değerin yazılması bekleniyor...");
                wait.Until(ExpectedConditions.TextToBePresentInElementValue(by, text));

            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                assertFail(by.ToString() + " bulunamadı");
            }

        }

        /// <summary>
        /// Mouse hover with element
        /// </summary>
        /// <param name="element"></param>
        public void hoverElement(IWebElement element)
        {
            Actions hoverAction = new Actions(driver);
            hoverAction.MoveToElement(element).Build().Perform();
        }

        /// <summary>
        /// Mouse hover with by locators
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void hoverElement(By by, params int[] index)
        {
            hoverElement(findElement(by, index));
        }

        /// <summary>
        /// Element selected kontrol
        /// </summary>
        /// <param name="by"></param>
        /// <param name="index"></param>
        public void elementSelectedControl(By by, params int[] index)
        {
            if (findElement(by, index).Selected)
            {
                log.Info(by + ": element seçilidir.");
            }
            else
            {
                log.Info("Element seçili değildir.");
                assertFail(by + " : kontrol edilen element seçili değildir.");
            }
        }

        /// <summary>
        /// Separate a string based on separator
        /// Pick a separated string by arrayNumber
        /// </summary>
        public string stringSeparator(string str, string separator, int arrayNumber)
        {
            string[] stringSeparators = new string[] { separator };
            string[] separateString;
            separateString = str.Split(stringSeparators, StringSplitOptions.None);
            List<string> separatedStrings = new List<string>();
            foreach (string s in separateString)
            {
                separatedStrings.Add(String.IsNullOrEmpty(s) ? "<>" : s);
            }
            return separatedStrings[arrayNumber];
        }


        /// <summary>
        /// Frame in varolmasını bekler ve frame'e geçiş yapar.
        /// </summary>
        /// <param name="index"></param>
        public void waitFrameSwicth(int index)
        {
            wait.Until(wTool.FrameToBeAvailableAndSwitchToItWıthIndex(1));
        }

        public int stringToInteger(string str)
        {
            return Int32.Parse(str);
        }

    }
}
