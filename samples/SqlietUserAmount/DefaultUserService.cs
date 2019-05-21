using System;
using System.Collections.Generic;
using System.Text;
using Peanut;
namespace SqlietUserAmount
{
    public class DefaultUserService
    {
        public long Income(string name, int amount)
        {

            SQL sql = "update User set Amount=Amount+" + amount + " where Name=@p2";
            sql["p2", name].Execute();
            sql = "select Amount from User where Name=@p1";
            var result = sql["p1", name].GetValue<long>();
            return result;
        }

        public long Pay(string name, int amount)
        {
            SQL sql = "update User set Amount=Amount-" + amount + " where Name=@p2";
            sql["p2", name].Execute();
            sql = "select Amount from User where Name=@p1";
            var result = sql["p1", name].GetValue<long>();
            return result;
        }

        public long GetAmount(string name)
        {
            SQL sql = "select Amount from User where Name=@p1";
            var result = sql["p1", name].GetValue<long>();
            return result;
        }
    }
}
