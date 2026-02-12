using Game.Core;
using Game.Gameplay; 
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Debug component that logs HexNode connection information at runtime.
    /// Call this after rotation if needed to check values. 
    /// </summary>
    public class NodeDebugger : MonoBehaviour
    {
        void Start()
        {
            LogNodeConnectionInfo();
        }

        public void LogNodeConnectionInfo()
        {
            if (!TryGetComponent<HexNode>(out var node))
                return;

            Debug.Log($"--- DEBUG NODE {node.GridPosition} ---", gameObject);
            Debug.Log($"1. Current Rotation: {node.CurrentRotation}", gameObject);

            bool[] connections = node.GetConnectionsAtCurrentRotation();
            string activePorts = "";
            for (int i = 0; i < 6; i++)
            {
                if (connections[i]) activePorts += $"{i} ({((HexDirection)i)}), ";
            }
            Debug.Log($"2. Active Ports (Rotated): {activePorts}", gameObject);

            // Check specifically for BottomLeft (4)
            bool hasBottomLeft = node.HasConnectionInDirection(HexDirection.BottomLeft);
            Debug.Log($"3. Has BottomLeft (4)? {hasBottomLeft.ToString().ToUpper()}", gameObject);
        }
    }
}