using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public abstract class REvent
    {
        public REvents REventType { get; protected set; }
        public int REvent_id { get; protected set; }


        public bool isCopOnly { get; protected set; }

        public abstract void Summon();
        public abstract void Handle();

    }

    public enum REvents
    {
        DriveByShooting = 0
    }
}
