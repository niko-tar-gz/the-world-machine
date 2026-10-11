using System;
using System.Collections.Generic;
using System.Text;

namespace TheWorldMachine.impl.Types
{
    [Serializable]
    public class PlayfabAuthRequestData
    {
        public string Nonce;
        public string OculusId;
        public string AppId;
        public string AgeCategory;
        public string Platform;
        public string MothershipEnvId;
        public string MothershipDeploymentId;
        public string MothershipToken;
        public string MothershipId;
    }
}
