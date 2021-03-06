﻿using System.Collections.Generic;
using BankingBot.Models;

namespace BankingBot.Contracts
{
    public interface IClient
    {
        void Login(ILoginCredentials credentials);

        decimal GetBalance();

        IEnumerable<Account> GetAccounts();

        IEnumerable<Transaction> GetTransactions(string accountNumber);
    }
}