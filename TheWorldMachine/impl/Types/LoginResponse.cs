using System;
using System.Collections.Generic;
using System.Text;

namespace TheWorldMachine.impl.Types
{
    [Serializable]
    public class LoginResponse
    {
        public string ExternalProviderId = string.Empty;
        public string ExternalProviderUsername = string.Empty;
        public bool IsPrimaryId;
        public string PlayerId = string.Empty;
        public string[] Tags = new string[0];
        public string Token = string.Empty;
        public long ExpirationTime;
    }
}
