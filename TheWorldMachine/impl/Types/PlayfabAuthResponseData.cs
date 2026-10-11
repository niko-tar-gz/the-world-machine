using System;
using System.Collections.Generic;
using System.Text;

namespace TheWorldMachine.impl.Types
{
    [Serializable]
    public class PlayfabAuthResponseData
    {
        public string PlayFabId;
        public string SessionTicket;
        public string EntityToken;
    }
}
