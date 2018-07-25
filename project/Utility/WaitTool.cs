using OpenQA.Selenium.Winium;
using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;


namespace project.Utility
{
    class WaitTool
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        readonly IJavaScriptExecutor jsDriver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        public WaitTool(IWebDriver driver)
        {
            jsDriver = (IJavaScriptExecutor)driver;
        }


        /// <summary>
        /// Wait for ajax
        /// </summary>
        public void waitAjax()
        {
            jsDriver.ExecuteScript("var callback = arguments[arguments.length - 1];"
            + "var xhr = new XMLHttpRequest();" + "xhr.open('GET', '/Ajax_call', true);"
            + "xhr.onreadystatechange = function() {" + "  if (xhr.readyState == 4) {"
            + "    callback(xhr.responseText);" + "  }" + "};" + "xhr.send();");
        }

        /// <summary>
        /// Wait for jQuery
        /// </summary>
        public void waitJQuery()
        {
            //try
            //{
            //    WebDriverWait jqueryWait = new WebDriverWait(StepImplementation.Driver, TimeSpan.FromMinutes(10));
            //    jqueryWait.Until<bool>((d) => { return jsDriver.ExecuteScript("return jQuery.active").ToString().Equals("0"); });
            //}
            //catch (Exception e)
            //{
            //    log.Error(e.Message, e);
            //    Assert.Fail("Belirlenen süreler içinde jQuery işlemleri gerçekleşmedi!");

            //}
        }

        /// <summary>
        /// Dom ready
        /// </summary>
        public void waitDomReady()
        {
            try
            {
                if (jsDriver.ExecuteScript("return document.readyState").ToString().Equals("complete"))
                {

                    return;
                }

                for (int i = 0; i < 1000; i++)
                {
                    Thread.Sleep(250);
                    if (jsDriver.ExecuteScript("return document.readyState").ToString().Equals("complete"))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }
        }

        /// <summary>
        /// Frame geçişleri öncesi geçiş yapılacak frame'in var olmasını bekler.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Func<IWebDriver, IWebDriver> FrameToBeAvailableAndSwitchToItWıthIndex(int index)
        {
            return (driver) =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(250));
                try
                {
                    return driver.SwitchTo().Frame(index);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
                catch (NoSuchFrameException)
                {
                    return null;
                }
            };
        }
    }
}
