using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace Meatyceiver2
{
    [MoonSharpUserData]
    public readonly struct ConfigScript
    {
        [MoonSharpUserData]
        public readonly struct JamConfig
        {
            public readonly int Chance;

            public JamConfig()
            {
                Chance = 0;
            }

            public JamConfig(int chance)
            {
                Chance = chance;
            }
        }
        
        public readonly int                             Priority;
        public readonly string                          ObjectID;
        public readonly Dictionary<string, JamConfig[]> Jams;

        public ConfigScript()
        {
            Priority = 0;
            ObjectID = String.Empty;
            Jams = new Dictionary<string, JamConfig[]>();
        }

        public ConfigScript(Script lua, string code)
        {
            UserData.RegisterType<ConfigScript>();
            UserData.RegisterType<JamConfig>();
            this = lua.DoString($"return {{${code}}}").ToObject<ConfigScript>();
        }
    }
}