using AntShares.SmartContract.Framework;
using AntShares.SmartContract.Framework.Services.AntShares;
using System;

namespace Woolong
{
    public class Woolong : FunctionCode
    {

        public static byte[] Main(byte[] originator, byte[] signature, byte[] byteEvent, byte[] targetPubKeyA, byte[] targetPubKeyB, int amount)
        {
            //Verify that the user is who they say they are
            if (!VerifySignature(originator, signature))
            {
                Runtime.Log("The input public key and signature did not match.  Please try again.");
                return new byte[] { 0 };
            }
  
            var Event = System.Text.Encoding.UTF8.GetString(byteEvent);
            switch (Event)
            {
                case "Deploy":
                    Runtime.Log("Attempting to deploy the smart contract tokens");
                    return Deploy(originator);

                case "TotalSupply":
                    Runtime.Log("Successfully invoked TotalSupply");
                    return BitConverter.GetBytes(10000);

                case "BalanceOf":
                    Runtime.Log("Successfully invoked BalanceOf");
                    return Storage.Get(Storage.CurrentContext, targetPubKeyA);

                case "Transfer":
                    return Transfer(originator, targetPubKeyA, amount);

                case "TransferFrom":
                    return TransferFrom(originator, targetPubKeyA, targetPubKeyB, amount);

                case "Approve":
                    return Approve(originator, targetPubKeyA, amount);

                case "Allowance":
                    Runtime.Log("Successfully invoked Allowance");
                    return Storage.Get(Storage.CurrentContext, targetPubKeyA.Concat(targetPubKeyB));

                default:
                    Runtime.Log("Invalid Event Input");
                    return new byte[] { 0 };
            }
        }


        /// <summary>
        ///   Deploys the contract tokens. 
        /// </summary>
        private static byte[] Deploy(byte[] originator)
        {
            //Define the admin public key in byte format (the same format as the one
            //input to invote the Smart Contract.
            var adminKey = new byte[] { 3, 84, 174, 73, 130, 33, 4, 108, 102,
                                        110, 254, 187, 174, 233, 189, 14, 180,
                                        130, 52, 105, 201, 142, 116, 132, 148,
                                        169, 42, 113, 243, 70, 177, 166, 97 };

            if (originator != adminKey)
            {
                Runtime.Log("Please use an admin account to access this Event");
                return new byte[] { 0 };
            }

            //deploy the tokens to the admin
            var supply = BitConverter.GetBytes(10000);
            Storage.Put(Storage.CurrentContext, originator, supply);
            Runtime.Log("Tokens deployed to your account");
            return new byte[] { 1 };
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

            var nOriginatorValue = BitConverter.ToInt32(originatorValue, 0) - amount;
            var nTargetValue = BitConverter.ToInt32(targetValue, 0) + amount;
            if (((nOriginatorValue) >= 0) &&
                (amount > 0))
            {

                var targetByteVal = BitConverter.GetBytes(nTargetValue);
                var byteVal = BitConverter.GetBytes(nOriginatorValue);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(targetByteVal);
                    Array.Reverse(byteVal);
                }

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

            var allocated = Storage.Get(Storage.CurrentContext, from.Concat(originator));
            var fromValue = Storage.Get(Storage.CurrentContext, from);
            var toValue = Storage.Get(Storage.CurrentContext, to);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(allocated);
                Array.Reverse(fromValue);
                Array.Reverse(toValue);
            }

            var allValInt = BitConverter.ToInt32(allocated, 0);
            var fromValInt = BitConverter.ToInt32(fromValue, 0);
            var toValInt = BitConverter.ToInt32(toValue, 0);

            if ((fromValInt >= amount) &&
                (amount > 0) &&
                (allValInt > 0))
            {
                var newFromVal = BitConverter.GetBytes(fromValInt - amount);
                var newAll = BitConverter.GetBytes(allValInt - amount);
                var newToVal = BitConverter.GetBytes(toValInt + amount);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(newFromVal);
                    Array.Reverse(newAll);
                    Array.Reverse(newToVal);
                }

                Storage.Put(Storage.CurrentContext, from.Concat(originator), newAll);
                Storage.Put(Storage.CurrentContext, to, newToVal);
                Storage.Put(Storage.CurrentContext, from, newFromVal);

                Runtime.Log("Amount successfully transferred");
                return new byte[] { 1 };
            }

            Runtime.Log("Transfer Failed");
            return new byte[] { 0 };
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
            byte[] val = BitConverter.GetBytes(amount);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(val);
            }

            Storage.Put(Storage.CurrentContext, originator.Concat(spender), val);
            Runtime.Log("Ammount successfully approved");
            return new byte[] { 1 };
        }
    }
}
