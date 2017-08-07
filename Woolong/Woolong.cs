﻿using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;


/// TODO: params flag support (this requires additional funtionality on the GUI.
/// TODO: Allowance support (unknown call exception when adding another case to Main)

namespace Woolong
{
    public class Woolong : FunctionCode
    {
   
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
        /// <param name="Event">
        ///   The ERC20 Event being invoked.
        /// <param name="args">
        ///   Optional input parameters used by the ERC20 methods.  These will be collapsed with a params
        ///   input once the GUI is enhanced to support option inputs.
        /// </param>
        public static object Main(byte[] originator, string Event, byte[] args0, byte[] args1, byte[]args2)
       {
            Runtime.Log("Contract Invoked by " + originator);
            Runtime.Log("Event: " + Event);
            Runtime.Log("Arg0: " + args0);
            Runtime.Log("Arg1:  " + args1);
            Runtime.Log("Args2: " + args2);

           if (!Runtime.CheckWitness(originator))
           {
               Runtime.Log("Verification Failed");
               return false;
           }
            
            switch ( Event )
            {
                case "Deploy":
                    return Deploy(originator, args0);

                case "TotalSupply":
                    int Supply = 10000;
                    Runtime.Log("Successfully invoked TotalSupply Expecting: " + Supply);
                    return Supply;

                case "BalanceOf":
                    Runtime.Log("Successfully invoked BalanceOf");
                    byte[] value = Storage.Get(Storage.CurrentContext, args0 );
                    BigInteger v = BytesToInt(value);
                    Runtime.Log("Value: " + v);
                    return v;

                case "Transfer":
                    return Transfer(originator, args0, args1);

                case "TransferFrom":
                    return TransferFrom(originator, args0, args1, BytesToInt(args2));

                case "Approve":
                    return Approve(originator, args0, BytesToInt(args1));

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
        private static bool Deploy(byte[] originator, byte[] args0)
        {
            //Define the admin public key in byte format (the same format as the one
            //input to invoke the Smart Contract.
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
            Storage.Put(Storage.CurrentContext, originator, args0);
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
        private static bool Transfer(byte[] originator, byte[] to, byte[] amount)
        {
            Runtime.Log("Successfully Invoked Transfer");
            
            var Amount =  BytesToInt(amount);
            
            var originatorValue = Storage.Get(Storage.CurrentContext, originator);
            var targetValue = Storage.Get(Storage.CurrentContext, to);
            
            BigInteger nOriginatorValue = BytesToInt(originatorValue);
            BigInteger nTargetValue = BytesToInt(targetValue);
            
            Runtime.Log("Originator Value: " + nOriginatorValue);
            Runtime.Log("To Value: " + nTargetValue);
            Runtime.Log("Transfer Amount: " + Amount);
            
            nOriginatorValue -= Amount;
            nTargetValue += Amount;
            
            Runtime.Log("New Originator Value: " + nOriginatorValue);
            Runtime.Log("New To Value: " + nTargetValue);
            
            if ((nOriginatorValue >= 0) &&
                 (Amount >= 0))
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
        private static bool TransferFrom(byte[] originator, byte[] from, byte[] to, BigInteger amount)
        {

            Runtime.Log("Successfully Invoked TransferFrom");
            var allValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from.Concat(originator)));
            var fromValInt = BytesToInt(Storage.Get(Storage.CurrentContext, from));
            var toValInt = BytesToInt(Storage.Get(Storage.CurrentContext, to));
            Runtime.Log("allValInt: " + allValInt);
            Runtime.Log("fromValInt: " + fromValInt);
            Runtime.Log("toValInt: " + toValInt);
            
            if ((fromValInt >= amount) &&
                (amount >= 0)  &&
                (allValInt >= 0))
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


        private static BigInteger Allowance(byte[] originator, byte[] target)
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
        private static bool Approve(byte[] originator, byte[] spender, BigInteger amount)
        {
            Runtime.Log("Successfully invoked Approve");
            var val = IntToBytes(amount);
            Runtime.Log("Val: " + val);
            
            Storage.Put(Storage.CurrentContext, originator.Concat(spender), val);
            Runtime.Log("Amount successfully approved");
            return true;
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
