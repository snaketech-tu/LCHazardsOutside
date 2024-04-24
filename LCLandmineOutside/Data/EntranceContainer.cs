using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LCHazardsOutside.Data
{
    public class EntranceContainer(Vector3 mainEntrancePosition, List<Vector3> fireExitPositions)
    {
        public Vector3 MainEntrancePosition { get; set; } = mainEntrancePosition;
        public List<Vector3> FireExitPositions { get; set; } = fireExitPositions;

        public bool IsInitialized()
        {
            return MainEntrancePosition != Vector3.zero && FireExitPositions.All(x => x != Vector3.zero);
        }
    }
}
