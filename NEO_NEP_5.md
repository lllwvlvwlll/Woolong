<pre><code>
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NEO_NEP_5
{
    public class NEO_NEP_5 : FunctionCode
    {
        public static Object Main(string operation, params object[] args)
        {
            switch(operation)
            {
                case "totalSupply":
                    return totalSupply();
                case "name":
                    return name();
                case "symbol":
                    return symbol();
                case "transfer":
                    return transfer(args);
                case "balanceOf":
                    return balanceOf(args);
                case "decimals":
                    return decimals();
                default:
                    return false;
            }
        }
        private static BigInteger totalSupply()
        {
            byte[] totalSupply = Storage.Get(Storage.CurrentContext, "totalSypply");
            return BytesToInt(totalSupply);
        }

        private static string name()
        {
            byte[] name = Storage.Get(Storage.CurrentContext, "name");
            return name.AsString();
        }

        private static string symbol()
        {
            byte[] symbol = Storage.Get(Storage.CurrentContext, "symbol");
            return symbol.AsString();
        }

        private static bool transfer(object[] args)
        {
            if (args.Length != 3) return false;
            byte[] from = (byte[])args[0];
            if (!Runtime.CheckWitness(from)) return false;
            byte[] to = (byte[])args[1];
            BigInteger value = BytesToInt((byte[])args[2]);
            if (value < 0) return false;
            byte[] from_value = Storage.Get(Storage.CurrentContext, from);
            byte[] to_value = Storage.Get(Storage.CurrentContext, to);
            BigInteger n_from_value = BytesToInt(from_value) - value;
            if (n_from_value < 0) return false;
            BigInteger n_to_value = BytesToInt(to_value) + value;
            Storage.Put(Storage.CurrentContext, from, IntToBytes(n_from_value));
            Storage.Put(Storage.CurrentContext, to, IntToBytes(n_to_value));
            Transfer(args);
            return true;
        }

        private static void Transfer(object[] args)
        {
            Runtime.Notify(args);
        }

        private static BigInteger balanceOf(object[] args)
        {
            if (args.Length != 1) return 0;
            byte[] address = (byte[])args[0];
            byte[] balance = Storage.Get(Storage.CurrentContext, address);
            return BytesToInt(balance);
        }

        private static BigInteger decimals()
        {
            byte[] decimals = Storage.Get(Storage.CurrentContext, "decimals");
            return BytesToInt(decimals);
        }
        
        private static BigInteger BytesToInt(byte[] array)
        {
            var buffer = new BigInteger(array);
            return buffer;
        }

        private static byte[] IntToBytes(BigInteger value)
        {
            byte[] buffer = value.ToByteArray();
            return buffer;
        }
    }
}

</pre></code>
