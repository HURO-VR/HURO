using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunDataCollection.Classes
{
    [Serializable]
    public class CentennialData
    {
        public long timeCreated = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public int roundTrips = 0;
        public int collisions = 0;
        public float maxVelocity = 1;
        public float clearance = 1;
        public int numMachines = 1;
        public string name = "";
        
        public Dictionary<string, object> ToJson()
        {
            return new Dictionary<string, object>
            {
                { "timeCreated", this.timeCreated },
                { "roundTrips", this.roundTrips },
                { "collisions", this. collisions },
                { "maxVelocity", this.maxVelocity },
                { "clearance", this.clearance },
                { "numMachines", this.numMachines },
                { "name", this.name },
            };
        }
    }
    

}