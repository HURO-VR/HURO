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
        public int numGames = 0;
        public string uid = "";
        
        public static CentennialData CopyFrom(CentennialData original)
        {
            return new CentennialData
            {
                timeCreated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // reset to current time
                roundTrips = original.roundTrips,
                collisions = original.collisions,
                maxVelocity = original.maxVelocity,
                clearance = original.clearance,
                numMachines = original.numMachines,
                name = original.name,
                numGames = original.numGames,
                uid = original.uid
            };
        }

        public CentennialData Copy()
        {
            return CopyFrom(this);
        }

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
                {"numGames", this.numGames },
                {"uid", this.uid}
            };
        }

        public void Reset()
        {
            this.roundTrips = 0;
            this.collisions = 0;
        }
    }
    

}