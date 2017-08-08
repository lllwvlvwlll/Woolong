using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;


namespace Woolong
{
    public class Woolong : FunctionCode
    {

        /// <summary>
        ///   This smart contract is designed to replacate the 
        ///   functionality of an ERC20 token.
        /// 
        ///   Parameter List: 0505050505
        ///   Return List: 05
        /// 
        /// </summary>
        /// <param name="originator">
        ///   Public Key (param code 06)
        /// </param>
        /// <param name="Event">
        ///   The ERC20 Event being invoked.
        /// </param>
        /// <param name="args0">
        ///   Optional input parameters used by the ERC20 methods.  These will be collapsed with a params
        ///   input once the GUI is enhanced to support option inputs.
        /// </param>
        /// <param name="args1">
        ///   Optional input parameters used by the ERC20 methods.
        /// </param>
        /// <param name="args2">
        ///   Optional input parameters used by the ERC20 methods.
        /// </param>

        public static object Main(byte[] originator, string Event, byte[] args0, byte[] args1, byte[] args2)
       {
           BigInteger supply = 1000000;
           
           if (!Runtime.CheckWitness(originator)) return false;

           if (Event == "deploy") return Deploy(originator, supply);

           if (Event == "totalSupply") return supply;
           
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
            //Define the admin public key in byte format
            // Reference: https://github.com/neo-project/docs/blob/master/en-us/sc/tutorial/Lock2.md
            var adminKey = new byte[] {};

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
            var fromValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from));
            var toValInt = BytesToInt(Storage.Get(Storage.CurrentContext, to));
 
            if (fromValInt >= amount &&
                amount >= 0  &&
                allValInt >= 0)
            {
                Storage.Put(Storage.CurrentContext, from.Concat(originator), IntToBytes(allValInt - amount));
                Storage.Put(Storage.CurrentContext, to, IntToBytes(toValInt + amount));
                Storage.Put(Storage.CurrentContext, from, IntToBytes(fromValInt - amount));
                return true;
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
