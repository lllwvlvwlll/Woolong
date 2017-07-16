using AntShares.SmartContract.Framework;
using AntShares.SmartContract.Framework.Services.AntShares;
using System;
using System.Numerics;

namespace Woolong
{
    public class Woolong : FunctionCode
    {
        public static string name = "Woolong";
        public static string symbol = "WLG";
        public static uint supply = 100000000;
        //public static byte[] admin = Encoding.ASCII.GetBytes("ALbGSR4A7DDr2FqsZyC4SiYKmjc3naWkcZ");
        public static byte[] originator = new byte[] { };
        public static byte[] sig = new byte[] { };

        public static object Main(string operation, params object[] args)
        {
            originator = (byte[])args[0];
            sig = (byte[])args[1];

            if (!VerifySignature(originator, sig))
            {
                return false;
            }

            switch (operation)
            {
                case "TotalSupply":
                    return TotalSupply();

                case "BalanceOf":
                    return BalanceOf((byte[])args[2]);

                case "Transfer":
                    return Transfer((byte[])args[2], (uint)args[3]);

                case "TransferFrom":
                    return TransferFrom((byte[])args[2], (byte[])args[3], (uint)args[4]);

                case "Approve":
                    return Approve((byte[])args[2], (uint)args[3]);

                case "Allowance":
                    return Allowance((byte[])args[2], (byte[])args[3]);

                case "Deploy":
                    return Deploy();

                default:
                    return false;
            }

        }

        ///  <summary>
        ///  Calculates the total circulating supply of tokens.
        ///  Returns:
        ///    uint: the total supply of tokens in circulation.
        ///  </summary>
        private static uint TotalSupply()
        {
            return supply;
        }


        ///  <summary>
        ///    Identifies the balance of a user 
        ///    Args:
        ///      owner: The account address to look up.
        ///    Returns:
        ///      uint: The account holdings of the input address.
        ///  </summary>        
        private static uint BalanceOf(byte[] owner)
        {
            byte[] balance = Storage.Get(Storage.CurrentContext, owner);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(balance);

            return BitConverter.ToUInt32(balance, 0);
        }


        ///  <summary>
        ///    Transfers a balance to an address
        ///    Args:
        ///      to: The address to transfer tokens to.
        ///      transValue: The amount of tokens to transfer.
        ///    Returns: 
        ///      bool: Transaction Successful?.   
        ///  </summary>
        private static bool Transfer(byte[] to, uint transValue)
        {
            uint originatorValue = BalanceOf(originator);
            uint toValue = BalanceOf(to);

            if ((originatorValue >= transValue) &&
                (transValue > 0))
            {

                byte[] toByteVal = BitConverter.GetBytes(toValue + transValue);
                byte[] fromByteVal = BitConverter.GetBytes(originatorValue - transValue);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(toByteVal);
                    Array.Reverse(fromByteVal);
                }

                Storage.Put(Storage.CurrentContext, originator, fromByteVal);
                Storage.Put(Storage.CurrentContext, to, toByteVal);

                return true;

            };

            return false;
        }


        ///  <summary>
        ///    Transfers a balance from one address to another.
        ///    Args:
        ///      from: The address to transfer funds from.
        ///      to: The adress to transfer funds to.
        ///      value: The amount of tokens to transfer.
        ///    Returns:
        ///      bool: Transaction Successful?   
        ///  </summary>
        private static bool TransferFrom(byte[] from, byte[] to, uint value)
        {

            byte[] allocated = Storage.Get(Storage.CurrentContext, from.Concat(originator));
            byte[] fromValue = Storage.Get(Storage.CurrentContext, from);
            byte[] toValue = Storage.Get(Storage.CurrentContext, to);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(allocated);
                Array.Reverse(fromValue);
                Array.Reverse(toValue);
            }

            uint allValInt = BitConverter.ToUInt32(allocated, 0);
            uint fromValInt = BitConverter.ToUInt32(fromValue, 0);
            uint toValInt = BitConverter.ToUInt32(toValue, 0);

            if ((fromValInt >= value) &&
                (value > 0) &&
                (allValInt > 0))
            {
                byte[] newFromVal = BitConverter.GetBytes(fromValInt - value);
                byte[] newAll = BitConverter.GetBytes(allValInt - value);
                byte[] newToVal = BitConverter.GetBytes(toValInt + value);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(newFromVal);
                    Array.Reverse(newAll);
                    Array.Reverse(newToVal);
                }

                Storage.Put(Storage.CurrentContext, from.Concat(originator), newAll);
                Storage.Put(Storage.CurrentContext, to, newToVal);
                Storage.Put(Storage.CurrentContext, from, newFromVal);

                return true;
            }

            return false;
        }


        ///  <summary>
        ///    Allows a user to withdraw multiple times from an account up to a limit.
        ///    Args:
        ///      spender: the account that will be allowed access
        ///      value: The amount the spender can withdraw up to.
        ///    Returns:
        ///      bool: Transaction Successful?
        ///  </summary>
        private static bool Approve(byte[] spender, uint value)
        {
            byte[] val = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(val);
            }

            Storage.Put(Storage.CurrentContext, originator.Concat(spender), val);

            return true;
        }


        ///  <summary>
        ///    Returns the amount that a spender is allowed to spend on an owner's account.
        ///    Args:
        ///      owner: the acount that is being allowed access to
        ///      spender: The account that is authorized to spend.
        ///    Returns:
        ///      uint: The number of tokens available to the user.
        ///  </summary>
        private static uint Allowance(byte[] owner, byte[] spender)
        {
            byte[] balance = Storage.Get(Storage.CurrentContext, owner.Concat(spender));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(balance);
            }

            return BitConverter.ToUInt32(balance, 0);
        }


        /// <summary>
        ///   Deploys the contract tokens. 
        /// </summary>
        private static bool Deploy()
        {
            //if (originator == admin)
            if (true)
            {
                byte[] total = BitConverter.GetBytes(supply);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(total);
                }

                Storage.Put(Storage.CurrentContext, originator, total);

                return true;
            }
            return false;
        }
    }
}
