﻿using Orca.Models;

using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Rpc.Types;
using Orca.Accounts;
using Orca.Quotes;
using Orca.Ticks;
using System.Collections.Generic;
using System.Linq;

namespace Orca
{
    public class SigningCallback
    {
        private IEnumerable<Account> _accounts;
        private Account _defaultSigner;

        /// <summary> 
        /// Constructs with a list of signers and a default backup signer. 
        /// <param name="accounts">List of signers.</param> 
        /// <param name="defaultSigner">(Optional) The default signer which will sign if no matching signature found.</param> 
        /// </summary> 
        public SigningCallback(IEnumerable<Account> accounts, Account defaultSigner = null)
        {
            _accounts = accounts;
            _defaultSigner = defaultSigner;
        }

        /// <summary> 
        /// Constructs with just a single default backup signer, and no other signers.
        /// </summary> 
        /// <param name="defaultSigner">(Optional) The default signer which will sign if no matching signature found.</param> 
        public SigningCallback(Account defaultSigner = null)
            : this(Array.Empty<Account>(), defaultSigner)
        {
        }

        /// <summary> 
        /// Constructs with just a single signer, plus a default backup signer if that one is not matched.
        /// </summary> 
        /// <param name="singleSigner">A signer to match. </param> 
        /// <param name="defaultSigner">The default signer which will sign if no matching signature found.</param> 
        public SigningCallback(Account singleSigner, Account defaultSigner)
            : this(new Account[] { singleSigner }, defaultSigner)
        {
        }

        /// <summary> 
        /// Adds a signer to the list of signers held. 
        /// </summary> 
        /// <param name="signer">A signer to be matched.</param> 
        public void AddSigner(Account signer)
        {
            _accounts = _accounts.Append(signer);
        }

        /// <summary> 
        /// Callback that is called by framework to sign transactions. 
        /// Uses a list of accounts to sign the transaction.
        /// </summary> 
        /// <param name="msg">The message to sign</param> 
        /// <param name="publicKey">The public key of the requested signer</param> 
        public virtual byte[] Sign(byte[] msg, PublicKey publicKey)
        {
            foreach (Account a in _accounts)
            {
                if (a.PublicKey == publicKey)
                    return a.Sign(msg);
            }
            if (_defaultSigner != null)
                return _defaultSigner.Sign(msg);

            throw new Exception($"No signer was found to sign a message: public key is {publicKey.ToString()}");
        }
    }
}