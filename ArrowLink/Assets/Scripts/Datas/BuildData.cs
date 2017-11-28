using UnityEngine;
using System.Collections;

namespace AntonMakesGames.Tools
{
    public class BuildData : ScriptableObject
    {
        public int Version = 0;
        public int Revision = 0;
        public int Patch = 0;

        const string c_number_format = "00";
        const string c_version_pattern = "TileLink{0:D}.{1:D2}.{2:D3}";
        public string GetVersionLabel()
        {
            return string.Format(c_version_pattern, Version.ToString(), Revision.ToString(), Patch.ToString());
        }
    }
}