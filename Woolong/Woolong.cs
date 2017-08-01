using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Linq;
using System.Numerics;


/// TODO: endian support
/// TODO: Allowance support (unknown call exception when adding another case to Main)

namespace Woolong
{
    public class Woolong : FunctionCode
    {
   
        public const int Supply = 10000;

        //Parameter List: 060005
        //Return List: 05
        

        /// <summary>
        ///   Main deployment function for the Woolong ERC20 token.
        ///   For this token, we use an object array for input to suppose the 
        ///   needs of multiple different event types.
        /// </summary>
        /// <param name="originator">
        ///   Public Key (param code 06)
        /// </param>
        /// <param name="signature">
        ///   Signature (param code 00)
        /// </param>
        /// <param name="args">
        ///   args[0] string: The event type
        ///   args[1] byte[]: Event target A
        ///   args[2] byte[]: Event target B
        ///   args[3] int32:  Amount
        /// </param>
        /// <returns></returns>
        public static byte[] Main(byte[] originator, byte[] signature, params object[] args)
        {
            //Verify that the user is who they say they are
            if (!VerifySignature(originator, signature))
            {
                Runtime.Log("The input public key and signature did not match.  Please try again.");
                return new byte[] {0};
            }

            switch ( (string)args[0] )
            {
                case "Deploy":
                    Runtime.Log("Attempting to deploy the smart contract tokens");
                    return Deploy(originator);

                case "TotalSupply":
                    Runtime.Log("Successfully invoked TotalSupply");
                    return IntToBytes(Supply);

                case "BalanceOf":
                    Runtime.Log("Successfully invoked BalanceOf");
                    return Storage.Get(Storage.CurrentContext, (byte[])args[1] );

                case "Transfer":
                    return Transfer(originator, (byte[])args[1], (int)args[2]);

                case "TransferFrom":
                    return TransferFrom(originator, (byte[])args[1], (byte[])args[2], (int)args[2]);

                case "Approve":
                    return Approve(originator, (byte[])args[1], (int)args[2]);

                //case "Allowance":
                //    Runtime.Log("Successfully invoked Allowance");
                //    return Allowance(originator, (byte[])args[1]);
                    
                default:
                    Runtime.Log("Invalid Event Input");
                    return new byte[] {0};
            }
        }

        
        /// <summary>
        ///   Deploys the contract tokens. 
        /// </summary>
        private static byte[] Deploy(byte[] originator)
        {
            //Define the admin public key in byte format (the same format as the one
            //input to invote the Smart Contract.
            var adminKey = new byte[]
            {
                3, 84, 174, 73, 130, 33, 4, 108, 102,
                110, 254, 187, 174, 233, 189, 14, 180,
                130, 52, 105, 201, 142, 116, 132, 148,
                169, 42, 113, 243, 70, 177, 166, 97
            };

            if (originator != adminKey)
            {
                Runtime.Log("Please use an admin account to access this Event");
                return new byte[] {0};
            }
            
            //deploy the tokens to the admin
            Storage.Put(Storage.CurrentContext, originator, IntToBytes(Supply));
            Runtime.Log("Tokens deployed to your account");
            return new byte[] {1};
        }


        ///  <summary>
        ///    Transfers a balance to an address
        ///    Args:
        ///      originator: The address to transfer tokens from.
        ///      to: The address to transfer tokens to.
        ///      amount: The amount of tokens to transfer.
        ///    Returns: 
        ///      byte[]: Transaction Successful?.   
        ///  </summary>
        private static byte[] Transfer(byte[] originator, byte[] to, int amount)
        {
            var originatorValue = Storage.Get(Storage.CurrentContext, originator);
            var targetValue = Storage.Get(Storage.CurrentContext, to);

            var nOriginatorValue = BytesToInt(originatorValue) - amount;
            var nTargetValue = BytesToInt(targetValue) + amount;
            
            if (((nOriginatorValue) >= 0) &&
                (amount > 0))
            {

                var targetByteVal = IntToBytes(nTargetValue);
                var byteVal = IntToBytes(nOriginatorValue);

                Storage.Put(Storage.CurrentContext, originator, byteVal);
                Storage.Put(Storage.CurrentContext, to, targetByteVal);

                Runtime.Log("Tokens successfully transferred");
                return new byte[] { 1 };

            };
            
            Runtime.Log("Tokens failed to transfer");
            return new byte[] { 0 };
        }


        ///  <summary>
        ///    Transfers a balance from one address to another.
        ///    Args:
        ///      originator: the transaction originator
        ///      from: The address to transfer funds from.
        ///      to: The adress to transfer funds to.
        ///      amount: The amount of tokens to transfer.
        ///    Returns:
        ///      byte[]: Transaction Successful?   
        ///  </summary>
        private static byte[] TransferFrom(byte[] originator, byte[] from, byte[] to, int amount)
        {

            var allValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from.Concat(originator)));
            var fromValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from));
            var toValInt = BytesToInt(Storage.Get(Storage.CurrentContext, to));

            if ((fromValInt >= amount) &&
                (amount > 0) &&
                (allValInt > 0))
            {
                var newFromVal = IntToBytes(fromValInt - amount);
                var newAll = IntToBytes(allValInt - amount);
                var newToVal = IntToBytes(toValInt + amount);


                Storage.Put(Storage.CurrentContext, from.Concat(originator), newAll);
                Storage.Put(Storage.CurrentContext, to, newToVal);
                Storage.Put(Storage.CurrentContext, from, newFromVal);

                Runtime.Log("Amount successfully transferred");
                return new byte[] { 1 };
            }

            Runtime.Log("Transfer Failed");
            return new byte[] { 0 };
        }


        private static byte[] Allowance(byte[] originator, byte[] target)
        {
            return Storage.Get(Storage.CurrentContext, originator.Concat(target));
        }

        
        ///  <summary>
        ///    Allows a user to withdraw multiple times from an account up to a limit.
        ///    Args:
        ///      originator: the transaction originator
        ///      spender: the account that will be allowed access
        ///      amount: The amount the spender can withdraw up to.
        ///    Returns:
        ///      byte[]: Transaction Successful?
        ///  </summary>
        private static byte[] Approve(byte[] originator, byte[] spender, int amount)
        {
            var val = IntToBytes(amount);
            
            Storage.Put(Storage.CurrentContext, originator.Concat(spender), val);
            Runtime.Log("Amount successfully approved");
            return new byte[] { 1 };
        }


        private static byte[] IntToBytes(int value)
        {
            var buffer = new byte[] { };
            buffer[0] = (byte) value;
            buffer[1] = (byte) (value >> 8);
            buffer[2] = (byte) (value >> 0x10);
            buffer[3] = (byte) (value >> 0x18);

            return buffer;
        }
        
        
        private static int BytesToInt(byte[] array)
        {
            var value = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
            return value;
        }
    }
}
