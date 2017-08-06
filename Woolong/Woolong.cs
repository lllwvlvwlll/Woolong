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

        /// <summary>
        ///   Main deployment function for the Woolong ERC20 token.
        ///   For this token, we use an object array for input to suppose the 
        ///   needs of multiple different event types.
        /// 
        ///   Parameter List: 060005
        ///   Return List: 05
        /// 
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
        /// byte[] originator, byte[] signature
        public static object Main(byte[] originator, string Event, params object[] args)
        {
            Runtime.Log("Contract Invoked by " + originator);

            Runtime.Log("running");
            //Runtime.Log("Sig: " + signature);
            
            //Verify that the user is who they say they are
            //if (!VerifySignature(originator, signature))
            //{
            //    Runtime.Log("The input public key and signature did not match.  Please try again.");
            //    return false;
            //}
         
            switch ( Event )
            {
                case "Deploy":
                    return Deploy(originator);

                case "TotalSupply":
                    Runtime.Log("Successfully invoked TotalSupply Expecting: " + Supply);
                    return IntToBytes(Supply);

                case "BalanceOf":
                    Runtime.Log("Successfully invoked BalanceOf");
                    Runtime.Log("Variables: " + (byte[]) args[0]);
                    return Storage.Get(Storage.CurrentContext, (byte[])args[0] );

                case "Transfer":
                    return Transfer(originator, (byte[])args[0], (int)args[1]);

                case "TransferFrom":
                    return TransferFrom(originator, (byte[])args[0], (byte[])args[1], (int)args[2]);

                case "Approve":
                    return Approve(originator, (byte[])args[0], (int)args[1]);

                //case "Allowance":
                //    Runtime.Log("Successfully invoked Allowance");
                //    return Allowance(originator, (byte[])args[1]);
                    
                default:
                    Runtime.Log("Invalid Event Input");
                    return false;
            }
        }

        
        /// <summary>
        ///   Deploys the contract tokens. 
        /// </summary>
        private static bool Deploy(byte[] originator)
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
                return false;
            }
            
            //deploy the tokens to the admin
            Storage.Put(Storage.CurrentContext, originator, IntToBytes(Supply));
            Runtime.Log("Tokens deployed to your account");
            return true;
        }


        ///  <summary>
        ///    Transfers a balance to an address
        ///    Args:
        ///      originator: The address to transfer tokens from.
        ///      to: The address to transfer tokens to.
        ///      amount: The amount of tokens to transfer.
        ///    Returns: 
        ///      bool: Transaction Successful?.   
        ///  </summary>
        private static bool Transfer(byte[] originator, byte[] to, int amount)
        {
            Runtime.Log("Successfully Invoked Transfer");
            Runtime.Log("Variables: Origination: " + originator + " to: " + to + " amount: " + amount);
            var originatorValue = Storage.Get(Storage.CurrentContext, originator);
            var targetValue = Storage.Get(Storage.CurrentContext, to);
            Runtime.Log("Originator Value: " + originatorValue);
            Runtime.Log("To Value: " + targetValue);
            
            var nOriginatorValue = BytesToInt(originatorValue) - amount;
            var nTargetValue = BytesToInt(targetValue) + amount;
            Runtime.Log("New Originator Value: " + nOriginatorValue);
            Runtime.Log("New To Value: " + nTargetValue);
            
            if (((nOriginatorValue) >= 0) &&
                (amount > 0))
            {

                var targetByteVal = IntToBytes(nTargetValue);
                var byteVal = IntToBytes(nOriginatorValue);

                Storage.Put(Storage.CurrentContext, originator, byteVal);
                Storage.Put(Storage.CurrentContext, to, targetByteVal);

                Runtime.Log("Tokens successfully transferred");
                return true;

            };
            
            Runtime.Log("Tokens failed to transfer");
            return false;
        }


        ///  <summary>
        ///    Transfers a balance from one address to another.
        ///    Args:
        ///      originator: the transaction originator
        ///      from: The address to transfer funds from.
        ///      to: The adress to transfer funds to.
        ///      amount: The amount of tokens to transfer.
        ///    Returns:
        ///      bool: Transaction Successful?   
        ///  </summary>
        private static bool TransferFrom(byte[] originator, byte[] from, byte[] to, int amount)
        {

            Runtime.Log("Successfully Invoked TransferFrom");
            Runtime.Log("Variables: originator: " + originator + " from: " + from + " to: " + to + " amount: " + amount);
            var allValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from.Concat(originator)));
            var fromValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from));
            var toValInt = BytesToInt(Storage.Get(Storage.CurrentContext, to));
            Runtime.Log("allValInt: " + allValInt);
            Runtime.Log("fromValInt: " + fromValInt);
            Runtime.Log("toValInt: " + toValInt);
            
            if ((fromValInt >= amount) &&
                (amount > 0) &&
                (allValInt > 0))
            {
                var newFromVal = IntToBytes(fromValInt - amount);
                var newAll = IntToBytes(allValInt - amount);
                var newToVal = IntToBytes(toValInt + amount);
                Runtime.Log("newFromVal: " + newFromVal);
                Runtime.Log("newAll: " + newAll);
                Runtime.Log("newToVal: " + newToVal);
                

                Storage.Put(Storage.CurrentContext, from.Concat(originator), newAll);
                Storage.Put(Storage.CurrentContext, to, newToVal);
                Storage.Put(Storage.CurrentContext, from, newFromVal);

                Runtime.Log("Amount successfully transferred");
                return true;
            }

            Runtime.Log("Transfer Failed");
            return true;
        }


        private static int Allowance(byte[] originator, byte[] target)
        {
            return BytesToInt(Storage.Get(Storage.CurrentContext, originator.Concat(target)));
        }

        
        ///  <summary>
        ///    Allows a user to withdraw multiple times from an account up to a limit.
        ///    Args:
        ///      originator: the transaction originator
        ///      spender: the account that will be allowed access
        ///      amount: The amount the spender can withdraw up to.
        ///    Returns:
        ///      bool: Transaction Successful?
        ///  </summary>
        private static bool Approve(byte[] originator, byte[] spender, int amount)
        {
            Runtime.Log("Successfully invoked Approve");
            Runtime.Log("Variables: originator: " + originator + " spender: " + spender + " amount: " + amount);
            var val = IntToBytes(amount);
            Runtime.Log("Val: " + val);
            
            Storage.Put(Storage.CurrentContext, originator.Concat(spender), val);
            Runtime.Log("Amount successfully approved");
            return true;
        }

/*
        private static byte[] IntToBytes(int value)
        {
            Runtime.Log("ITB int: " + value);
            var buffer = new byte[4] ;
            buffer[0] = (byte) value;
            buffer[1] = (byte) (value >> 8);
            buffer[2] = (byte) (value >> 0x10);
            buffer[3] = (byte) (value >> 0x18);
            for (int i = 0; i < buffer.Length; i++)
            {
                Runtime.Log("ITB: " + buffer[i]);
            }    
            return buffer;
        }
*/
        private static byte[] IntToBytes(int value)
        {
            
            Runtime.Log("ITB int: " + value);
            byte[] buffer = ((BigInteger)value).ToByteArray();
            
            for (int i = 0; i < buffer.Length; i++)
            {
                Runtime.Log("ITB: " + buffer[i]);
            } 
            return buffer;
        }
        
        private static int BytesToInt(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Runtime.Log("BTI: " + array[i]);
            }    
            var value = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
            Runtime.Log("BTI int: " + value);
            return value;
        }
        
    }
}
