﻿using System;
using System.Text.RegularExpressions;
using BankingBot.Attributes;
using BankingBot.Contracts;
using BankingBot.LoginCredentials;
using OpenQA.Selenium;

namespace BankingBot.ActionManagers.LoginManagers
{
    [ProviderIdentifier(Enums.Provider.Lloyds)]
    public class LloydsLoginManager : IProviderLoginManager
    {
        private static class Urls
        {
            public const string Login = "https://online.lloydsbank.co.uk/personal/logon/login.jsp";
            public const string MemorableInfo = "https://secure.lloydsbank.co.uk/personal/a/logon/entermemorableinformation.jsp";
            public const string AccountOverview = "https://secure.lloydsbank.co.uk/personal/a/account_overview_personal/";
        }

        private readonly IBrowserBot _browserBot;
        
        public LloydsLoginManager(IBrowserBot browserBot)
        {
            _browserBot = browserBot;
        }

        public void Login(ILoginCredentials credentials)
        {
            var lloydsCreds = (LloydsLoginCredentials)credentials;

            LoginStep1(lloydsCreds);

            if (!_browserBot.WebDriver.Url.Contains(Urls.MemorableInfo))
                throw new Exception("An error occured");

            LoginStep2(lloydsCreds);

            if (!_browserBot.WebDriver.Url.Contains(Urls.AccountOverview))
                throw new Exception("An error occured");
        }

        private void LoginStep1(LloydsLoginCredentials credentials)
        {
            _browserBot.WebDriver.Url = Urls.Login;
            _browserBot.WebDriver.Navigate();

            _browserBot.WebDriver.FindElement(By.Id("frmLogin:strCustomerLogin_userID")).SendKeys(credentials.Username);
            _browserBot.WebDriver.FindElement(By.Id("frmLogin:strCustomerLogin_pwd")).SendKeys(credentials.Password);

            _browserBot.WebDriver.FindElement(By.Id("frmLogin:btnLogin2")).Click();
        }

        private void LoginStep2(LloydsLoginCredentials credentials)
        {
            var passphraseIndexes = GetPassphraseIndexes();

            var maxPassphraseLength = passphraseIndexes[2];
            if (credentials.Passphrase.Length < maxPassphraseLength)
                throw new IndexOutOfRangeException("Paspshrase is too short");

            _browserBot.WebDriver.FindElement(By.Id(GetPassphraseDdlId(1))).SendKeys(
                credentials.Passphrase[passphraseIndexes[0]].ToString());

            _browserBot.WebDriver.FindElement(By.Id(GetPassphraseDdlId(2))).SendKeys(
                credentials.Passphrase[passphraseIndexes[1]].ToString());

            _browserBot.WebDriver.FindElement(By.Id(GetPassphraseDdlId(3))).SendKeys(
                credentials.Passphrase[passphraseIndexes[2]].ToString());

            _browserBot.WebDriver.FindElement(By.Id("frmentermemorableinformation1:btnContinue")).Click();
        }

        private int[] GetPassphraseIndexes()
        {
            if (_browserBot.WebDriver.Url != Urls.MemorableInfo)
                throw new InvalidOperationException("Must be on the memorable info page");

            var charIndexes = new int[3];
            for (var i = 0; i < 3; i++)
            {
                var cssSelector = $"label[for='{GetPassphraseDdlId(i + 1)}']";
                var labelText = _browserBot.WebDriver.FindElement(By.CssSelector(cssSelector)).Text;
                var labelIndex = int.Parse(Regex.Replace(labelText, "[^0-9]", ""));

                charIndexes[i] = labelIndex;
            }

            return charIndexes;
        }

        private string GetPassphraseDdlId(int index)
        {
            if (index < 1 || index > 3)
                throw new ArgumentException("Must be between 1 and 3");

            return $"frmentermemorableinformation1:strEnterMemorableInformation_memInfo{index}";
        }
    }
}