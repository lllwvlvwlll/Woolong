using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;


namespace Woolong
{
    public class Woolong : FunctionCode
    {

        /// <summary>
        ///   This smart contract is designed as the example
        ///   token for the NEP5 standard.
        /// 
        ///   Parameter List: 0505050505
        ///   Return List: 05
        /// 
        /// </summary>
        /// <param name="originator">
        ///   Public Key (param code 05)
        /// </param>
        /// <param name="Event">
        ///   The NEP5 Event being invoked.
        /// </param>
        /// <param name="args0">
        ///   In this SC we diverge from the NEP5 template to aid in deployment using the desktop GUI.
        ///   When params support in the GUI is adding, the 'args' inputs will be collapsed down to 
        ///   align with the standard.
        /// </param>
        /// <param name="args1">
        /// </param>
        /// <param name="args2">
        /// </param>

        public static object Main(byte[] originator, string Event, byte[] args0, byte[] args1, byte[] args2)
       {
           BigInteger supply = 1000000;
           string name = "Woolong";
           string symbol = "WNG";
           BigInteger decimals = 1;
           
           if (!Runtime.CheckWitness(originator)) return false;

           if (Event == "deploy") return Deploy(originator, supply);

           if (Event == "totalSupply") return supply;
           
           if (Event == "name") return name;
 
           if (Event == "symbol") return symbol;

           if (Event == "decimals") return decimals;
           
           if (Event == "balanceOf") return Storage.Get(Storage.CurrentContext, args0);
            
           if (Event == "transfer") return Transfer(originator, args0, BytesToInt(args1));
           
           if (Event == "transferFrom") return TransferFrom(originator, args0, args1, BytesToInt(args2));

           if (Event == "approve") return Approve(originator, args0, args1);

           if (Event == "allowance") return Allowance(args0, args1);

           return false;
  }

        
        /// <summary>
        ///   Deploys the tokens to the admin account
        /// </summary>
        /// <param name="originator">
        ///   The contract invoker.
        /// </param>
        /// <param name="supply">
        ///   The supply of tokens to deploy.
        /// </param>
        /// <returns>
        ///   Transaction Successful?
        /// </returns>
        private static bool Deploy(byte[] originator, BigInteger supply)
        {
            // Define the admin public key in byte format.  
            // This is my testnet account...so dont think you found anything cool...
            // Reference: https://github.com/neo-project/docs/blob/master/en-us/sc/tutorial/Lock2.md
            var adminKey = new byte[] {
                3, 84, 174, 73, 130, 33, 4, 108, 102,
                110, 254, 187, 174, 233, 189, 14, 180,
                130, 52, 105, 201, 142, 116, 132, 148,
                169, 42, 113, 243, 70, 177, 166, 97
            };

            if (originator != adminKey) return false;
            
            //deploy the tokens to the admin
            Storage.Put(Storage.CurrentContext, originator, IntToBytes(supply));
            return true;
        }


        /// <summary>
        ///   Transfer a balance to another account.
        /// </summary>
        /// <param name="originator">
        ///   The contract invoker.
        /// </param>
        /// <param name="to">
        ///   The account to transfer to.
        /// </param>
        /// <param name="amount">
        ///   The amount to transfer.
        /// </param>
        /// <returns>
        ///   Transaction Successful?
        /// </returns>
        private static bool Transfer(byte[] originator, byte[] to, BigInteger amount)
        {           
            var originatorValue = Storage.Get(Storage.CurrentContext, originator);
            var targetValue = Storage.Get(Storage.CurrentContext, to);
            
            BigInteger nOriginatorValue = BytesToInt(originatorValue) - amount;
            BigInteger nTargetValue = BytesToInt(targetValue) + amount;
            
            if (nOriginatorValue >= 0 &&
                 amount >= 0)
            {
                Storage.Put(Storage.CurrentContext, originator, IntToBytes(nOriginatorValue));
                Storage.Put(Storage.CurrentContext, to, IntToBytes(nTargetValue));
                Transferred(originator, to, amount);
                return true;
            }
            return false;
        }


        /// <summary>
        ///   Transfers a balance from one account to another
        ///   on behalf of the account owner.
        /// </summary>
        /// <param name="originator">
        ///   The contract invoker.
        /// </param>
        /// <param name="from">
        ///   The account to transfer a balance from.
        /// </param>
        /// <param name="to">
        ///   The account to transfer a balance to.
        /// </param>
        /// <param name="amount">
        ///   The amount to transfer
        /// </param>
        /// <returns>
        ///   Transaction successful?
        /// </returns>
        private static bool TransferFrom(byte[] originator, byte[] from, byte[] to, BigInteger amount)
        {
            var allValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from.Concat(originator)));
 
            if (allValInt >= amount)
            {
                if (Transfer(from, to, amount))
                {
                    Storage.Put(Storage.CurrentContext, from.Concat(originator), IntToBytes(allValInt - amount));
                    return true;
                }
            }
            return false;
        }

        
        /// <summary>
        ///   Approves another user to use the TransferFrom
        ///   function on the invoker's account.
        /// </summary>
        /// <param name="originator">
        ///   The contract invoker.
        /// </param>
        /// <param name="to">
        ///   The account to grant TransferFrom access to.
        /// </param>
        /// <param name="amount">
        ///   The amount to grant TransferFrom access for.
        /// </param>
        /// <returns>
        ///   Transaction Successful?
        /// </returns>
        private static bool Approve(byte[] originator, byte[] to, byte[] amount)
        {
            Storage.Put(Storage.CurrentContext, originator.Concat(to), amount);
            return true;
        }
        
        /// <summary>
        ///   Checks the TransferFrom approval of two accounts.
        /// </summary>
        /// <param name="from">
        ///   The account which funds can be transfered from.
        /// </param>
        /// <param name="to">
        ///   The account which is granted usage of the account.
        /// </param>
        /// <returns>
        ///   The amount allocated for TransferFrom.
        /// </returns>
        private static BigInteger Allowance(byte[] from, byte[] to)
        {
            return BytesToInt(Storage.Get(Storage.CurrentContext, from.Concat(to)));
        }

        private static void Transferred(byte[] originator, byte[] to, BigInteger amount)
        {
            Runtime.Log("Transfer Event");
        }  
        
        private static byte[] IntToBytes(BigInteger value)
        {
            byte[] buffer = value.ToByteArray();
            return buffer;
        }
        
        private static BigInteger BytesToInt(byte[] array)
        {
            var buffer = new BigInteger(array);
            return buffer;
        }

        
    }
}
