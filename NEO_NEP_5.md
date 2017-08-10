```csharp
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
                    return TotalSupply();
                case "name":
                    return Name();
                case "symbol":
                    return Symbol();
                case "transfer":
                    return Transfer(args);
                case "balanceOf":
                    return BalanceOf(args);
                case "decimals":
                    return Decimals();
                default:
                    return false;
            }
        }
        private static BigInteger TotalSupply()
        {
            byte[] totalSupply = Storage.Get(Storage.CurrentContext, "totalSypply");
            return BytesToInt(totalSupply);
        }

        private static string Name()
        {
            byte[] name = Storage.Get(Storage.CurrentContext, "name");
            return name.AsString();
        }

        private static string Symbol()
        {
            byte[] symbol = Storage.Get(Storage.CurrentContext, "symbol");
            return symbol.AsString();
        }

        private static bool Transfer(object[] args)
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
            Transferred(args);
            return true;
        }

        private static void Transferred(object[] args)
        {
            Runtime.Notify(args);
        }

        private static BigInteger BalanceOf(object[] args)
        {
            if (args.Length != 1) return 0;
            byte[] address = (byte[])args[0];
            byte[] balance = Storage.Get(Storage.CurrentContext, address);
            return BytesToInt(balance);
        }

        private static BigInteger Decimals()
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
```
