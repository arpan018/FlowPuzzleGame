using UnityEngine;
using Game.Core;

namespace Game.Data
{
    /// <summary>
    /// ScriptableObject defining hex tile visual and connection properties.
    /// Used to create reusable node configuration assets for the hex based  puzzle game.
    /// </summary>
    [CreateAssetMenu(fileName = "NodeConnection_", menuName = "Game/Node Connection Data", order = 1)]
    public class NodeConnectionData : ScriptableObject
    {
        #region Identification

        [Header("Identification")]
        [Tooltip("Friendly name for this connection type (e.g., 'Y-Junction', 'Straight Line')")]
        [SerializeField] private string nodeName;

        [Tooltip("Type of node this represents (Source, Goal, Connector, Obstacle)")]
        [SerializeField] private NodeType nodeType;

        public string NodeName => nodeName;
        public NodeType NodeType => nodeType;
        public string ConnectionPattern => connectionPattern;

        #endregion

        #region Connection Pattern

        [Header("Connection Pattern")]
        [Tooltip("6-character binary string representing connections (e.g., '101010'). Each character represents a hex direction.")]
        [SerializeField] private string connectionPattern = "000000";

        [Tooltip("Parsed connection array from pattern. [0]=Top, [1]=TopRight, [2]=BottomRight, [3]=Bottom, [4]=BottomLeft, [5]=TopLeft")]
        [SerializeField] private bool[] connections = new bool[6];

        public bool[] Connections => connections;

        #endregion

        #region Behavior

        [Header("Behavior")]
        [Tooltip("Can this node be rotated by the player?")]
        [SerializeField] private bool canRotate = true;

        [Tooltip("Is this node a power source? (Auto-set if nodeType == Source)")]
        [SerializeField] private bool isSource = false;

        [Tooltip("Is this node a goal? (Auto-set if nodeType == Goal)")]
        [SerializeField] private bool isGoal = false;

        public bool CanRotate => canRotate;
        public bool IsSource => isSource;
        public bool IsGoal => isGoal;

        #endregion

        #region Visual Sprites

        [Header("Visual Sprites")]
        [Tooltip("Main connection pattern sprite (unpowered state)")]
        [SerializeField] private Sprite spriteOff;

        [Tooltip("Main connection pattern sprite (powered state)")]
        [SerializeField] private Sprite spriteOn;

        [Tooltip("White hex outline sprite (tile_stroke.png) - static parent")]
        [SerializeField] private Sprite parentSprite;

        [Header("Source/Goal Decoration Sprites")]
        [Tooltip("Only used for Source and Goal node types - Center circle (unpowered)")]
        [SerializeField] private Sprite centerSpriteOff;

        [Tooltip("Only used for Source and Goal node types - Center circle (powered)")]
        [SerializeField] private Sprite centerSpriteOn;

        [Tooltip("Only used for Source and Goal node types - Decorative ring")]
        [SerializeField] private Sprite ringSprite;

        [Tooltip("Only used for Source and Goal node types - Icon (bulb/lightning) unpowered")]
        [SerializeField] private Sprite iconSpriteOff;

        [Tooltip("Only used for Source and Goal node types - Icon (bulb/lightning) powered")]
        [SerializeField] private Sprite iconSpriteOn;

        public Sprite SpriteOff => spriteOff;
        public Sprite SpriteOn => spriteOn;
        public Sprite ParentSprite => parentSprite;
        public Sprite CenterSpriteOff => centerSpriteOff;
        public Sprite CenterSpriteOn => centerSpriteOn;
        public Sprite RingSprite => ringSprite;
        public Sprite IconSpriteOff => iconSpriteOff;
        public Sprite IconSpriteOn => iconSpriteOn;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Validate connection pattern
            if (string.IsNullOrEmpty(connectionPattern))
                connectionPattern = "000000";

            // Pattern must be exactly 6 characters
            if (connectionPattern.Length != 6)
            {
                Debug.LogWarning($"[{nodeName}] Connection pattern must be 6 characters. Resetting to '000000'.");
                connectionPattern = "000000";
            }

            // Pattern must only contain '0' or '1'
            foreach (char c in connectionPattern)
            {
                if (c != '0' && c != '1')
                {
                    Debug.LogWarning($"[{nodeName}] Connection pattern must only contain '0' or '1'. Found '{c}'.");
                    connectionPattern = "000000";
                    break;
                }
            }

            ParseConnectionPattern();

            // Validate parent sprite
            if (parentSprite == null)
                Debug.LogWarning($"[{nodeName}] Parent sprite (white hex outline) not assigned.");

            // Auto-set behavior flags based on node type
            switch (nodeType)
            {
                case NodeType.Source:
                    canRotate = false;
                    isSource = true;
                    isGoal = false;
                    
                    // Validate decoration sprites for Source nodes
                    if (centerSpriteOff == null || centerSpriteOn == null)
                        Debug.LogWarning($"[{nodeName}] Source node missing center sprites (off/on).");
                    if (iconSpriteOff == null || iconSpriteOn == null)
                        Debug.LogWarning($"[{nodeName}] Source node missing icon sprites (off/on).");
                    if (ringSprite == null)
                        Debug.LogWarning($"[{nodeName}] Source node missing ring sprite.");
                    break;

                case NodeType.Goal:
                    isGoal = true;
                    isSource = false;
                    
                    // Validate decoration sprites for Goal nodes
                    if (centerSpriteOff == null || centerSpriteOn == null)
                        Debug.LogWarning($"[{nodeName}] Goal node missing center sprites (off/on).");
                    if (iconSpriteOff == null || iconSpriteOn == null)
                        Debug.LogWarning($"[{nodeName}] Goal node missing icon sprites (off/on).");
                    if (ringSprite == null)
                        Debug.LogWarning($"[{nodeName}] Goal node missing ring sprite.");
                    break;

                case NodeType.Connector:
                case NodeType.Empty:
                    isSource = false;
                    isGoal = false;
                    break;
            }
        }

        #endregion

        #region Public Methods

        // Get full connection array for all six hex directions
        // Index: 0=Top, 1=TopRight, 2=BottomRight, 3=Bottom, 4=BottomLeft, 5=TopLeft
        public bool[] GetConnections()
        {
            return connections;
        }

        // Get connection state at specific hex direction
        public bool GetConnectionAtDirection(HexDirection direction)
        {
            int index = (int)direction;
            if (index >= 0 && index < 6)
                return connections[index];
            return false;
        }

        // Get connection state at specific angle (converts angle to nearest hex direction)
        public bool GetConnectionAtAngle(float angleDegrees)
        {
            HexDirection direction = HexDirectionHelper.FromAngle(angleDegrees);
            return GetConnectionAtDirection(direction);
        }

        #endregion

        #region Private Methods

        // Parse connection pattern string into boolean array ('1' = true, '0' = false)
        private void ParseConnectionPattern()
        {
            if (connections == null || connections.Length != 6)
                connections = new bool[6];

            for (int i = 0; i < 6 && i < connectionPattern.Length; i++)
                connections[i] = connectionPattern[i] == '1';
        }

        #endregion
    }
}
