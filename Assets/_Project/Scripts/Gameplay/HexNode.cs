using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game.Core;
using Game.Data;
using Game.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Gameplay
{
    // Represents a Hexagonal Node. 
    // Members have functionality for rendering, rotation, power state, and connection queries.
    [RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
    public class HexNode : MonoBehaviour
    {

        #region Sprite References

        [Header("Static Parent Sprites")]
        [Tooltip("White hex outline sprite (tile_stroke.png) - Never rotates")]
        [SerializeField] private SpriteRenderer parentSpriteRenderer;

        [Header("Rotating Container")]
        [Tooltip("Child GameObject that rotates with connections")]
        [SerializeField] private Transform rotatingContainer;

        [Tooltip("Dark hex background sprite (tile_bg.png)")]
        [SerializeField] private SpriteRenderer bgSpriteRenderer;

        [Tooltip("Connection line pattern sprite (rotates)")]
        [SerializeField] private SpriteRenderer elementSpriteRenderer;

        [Header("Static Decorations (Source/Goal only)")]
        [Tooltip("Parent container for center decorations")]
        [SerializeField] private GameObject staticDecorations;

        [Tooltip("Decorative ring sprite")]
        [SerializeField] private SpriteRenderer ringSpriteRenderer;

        [Tooltip("Icon sprite (bulb for goal, lightning for source)")]
        [SerializeField] private SpriteRenderer iconSpriteRenderer;

        [Header("Visual Settings")]
        [SerializeField]
        [Tooltip("Cooldown between rotations in seconds")]
        private float rotationCooldown = 0.3f;

        [SerializeField]
        [Tooltip("Brightness multiplier when powered")]
        private float poweredBrightness = 1.2f;

        #endregion

        #region Public Properties

        public Vector2Int GridPosition { get; private set; }
        public int CurrentRotation { get; private set; }
        public bool IsPowered { get; private set; }
        public bool CanRotate { get; private set; }
        public bool IsSource { get; private set; }
        public bool IsGoal { get; private set; }
        public bool IsObstacle { get; private set; }
        public NodeConnectionData ConnectionData { get; private set; }
        public int RotationCount { get; private set; }

        #endregion

        #region Private Fields

        private Sprite sprite_Off;
        private Sprite sprite_On;
        private Sprite iconSprite_Off;
        private Sprite iconSprite_On;
        private Sprite ringSprite;
        private bool isRotating;
        private float lastRotationTime;

        #endregion

        #region Initialization

        // Initialize node with grid position, sprites, and initial rotation
        public void Initialize(Vector2Int gridPosition, NodeConnectionData connectionData, int initialRotation, Sprite bgSprite)
        {
            if (connectionData == null)
            {
                Debug.LogError($"[HexNode] connectionData is null at {gridPosition}!");
                return;
            }

            GridPosition = gridPosition;
            ConnectionData = connectionData;

            // Set static parent sprite (white outline)
            if (parentSpriteRenderer != null && connectionData.ParentSprite != null)
                parentSpriteRenderer.sprite = connectionData.ParentSprite;

            // Set background sprite
            if (bgSpriteRenderer != null)
                bgSpriteRenderer.sprite = bgSprite;

            // Set node type flags
            CanRotate = connectionData.CanRotate;
            IsSource = connectionData.IsSource;
            IsGoal = connectionData.IsGoal;
            IsObstacle = connectionData.NodeType == NodeType.Obstacle;

            // Cache sprites based on node type
            if (IsSource || IsGoal)
            {
                // For Source/Goal: element sprite is the center circle
                sprite_Off = connectionData.CenterSpriteOff;
                sprite_On = connectionData.CenterSpriteOn;
                iconSprite_Off = connectionData.IconSpriteOff;
                iconSprite_On = connectionData.IconSpriteOn;
                ringSprite = connectionData.RingSprite;
            }
            else
            {
                // For Connectors: element sprite is the connection pattern
                sprite_Off = connectionData.SpriteOff;
                sprite_On = connectionData.SpriteOn;
            }

            if (elementSpriteRenderer != null)
                elementSpriteRenderer.sprite = sprite_Off;

            // Setup decorations for Source/Goal nodes
            if (IsSource)
            {
                if (staticDecorations != null)
                {
                    staticDecorations.SetActive(true);
                    if (iconSpriteRenderer != null && iconSprite_On != null)
                        iconSpriteRenderer.sprite = iconSprite_On;
                    if (ringSpriteRenderer != null && ringSprite != null)
                        ringSpriteRenderer.sprite = ringSprite;
                }
                IsPowered = true;
            }
            else if (IsGoal)
            {
                if (staticDecorations != null)
                {
                    staticDecorations.SetActive(true);
                    if (iconSpriteRenderer != null && iconSprite_Off != null)
                        iconSpriteRenderer.sprite = iconSprite_Off;
                    if (ringSpriteRenderer != null && ringSprite != null)
                        ringSpriteRenderer.sprite = ringSprite;
                }
                IsPowered = false;
            }
            else
            {
                if (staticDecorations != null)
                    staticDecorations.SetActive(false);
                IsPowered = false;
            }

            // Apply initial rotation to container only
            CurrentRotation = Mathf.Clamp(initialRotation, 0, 5);
            rotatingContainer.localRotation = Quaternion.Euler(0, 0, -CurrentRotation * 60f);
            transform.rotation = Quaternion.identity;

            UpdateVisuals();
            RotationCount = 0;
            lastRotationTime = -rotationCooldown;
        }

        #endregion

        #region Rotation

        private void OnMouseDown()
        {
            if (!CanRotate || isRotating) return;
            if (Time.time - lastRotationTime < rotationCooldown) return;

            GameEvents.TriggerNodeTapped(this);
            RotateNode();
        }

        // Rotate node 60° clockwise with smooth animation
        public void RotateNode()
        {
            if (!CanRotate || isRotating) return;

            CurrentRotation = (CurrentRotation + 1) % 6;
            RotationCount++;
            isRotating = true;

            float targetRotation = -CurrentRotation * 60f;

            // Animate rotation on container only
            rotatingContainer.DOLocalRotate(new Vector3(0, 0, targetRotation), 0.25f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    isRotating = false;
                    lastRotationTime = Time.time;
                    GameEvents.TriggerNodeRotated(this, RotationCount);
                    GameEvents.TriggerConnectionsUpdated();
                });

            // Scale bounce for feedback
            transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        #endregion

        #region Connections Checks

        // Get connections accounting for current rotation
        public bool[] GetConnectionsAtCurrentRotation()
        {
            if (ConnectionData == null)
                return new bool[6];

            bool[] baseConnections = ConnectionData.GetConnections();
            bool[] rotatedConnections = new bool[6];

            // Rotate array by CurrentRotation positions
            for (int i = 0; i < 6; i++)
            {
                int rotatedIndex = (i + CurrentRotation) % 6;
                rotatedConnections[i] = baseConnections[rotatedIndex];
            }

            return rotatedConnections;
        }

        // Check if node has connection in specified direction
        public bool HasConnectionInDirection(HexDirection direction)
        {
            bool[] connections = GetConnectionsAtCurrentRotation();
            int directionIndex = (int)direction;

            if (directionIndex >= 0 && directionIndex < 6)
                return connections[directionIndex];

            return false;
        }

        #endregion

        #region Power Function

        // Update power state (sources always stay powered)
        public void SetPowered(bool powered)
        {
            if (IsSource)
            {
                IsPowered = true;
                UpdateVisuals();
                return;
            }

            if (IsPowered == powered) return;

            IsPowered = powered;
            UpdateVisuals();

            if (powered)
                GameEvents.TriggerNodePowered(this);
            else
                GameEvents.TriggerNodeUnpowered(this);
        }

        #endregion

        #region Visuals

        // Update sprite and color based on power state
        private void UpdateVisuals()
        {
            if (elementSpriteRenderer == null) return;

            elementSpriteRenderer.sprite = IsPowered ? sprite_On : sprite_Off;
            elementSpriteRenderer.color = IsPowered 
                ? Color.white * poweredBrightness 
                : Color.white;

            // Update decorations for Source nodes
            if (IsSource)
            {
                if (iconSpriteRenderer != null && iconSprite_On != null)
                    iconSpriteRenderer.sprite = iconSprite_On;
            }
            // Update decorations for Goal nodes
            else if (IsGoal)
            {
                if (iconSpriteRenderer != null)
                    iconSpriteRenderer.sprite = IsPowered ? iconSprite_On : iconSprite_Off;
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (ConnectionData == null) return;

            Gizmos.color = IsPowered ? Color.yellow : Color.gray;
            bool[] connections = GetConnectionsAtCurrentRotation();

            // Draw lines for active connections
            for (int i = 0; i < 6; i++)
            {
                if (connections[i])
                {
                    HexDirection direction = (HexDirection)i;
                    float angle = ConnectionChecker.GetDirectionAngle(direction);
                    float angleRad = angle * Mathf.Deg2Rad;
                    Vector3 directionVector = new Vector3(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0);
                    Gizmos.DrawLine(transform.position, transform.position + directionVector * 0.5f);
                }
            }

            Handles.Label(transform.position + Vector3.up * 0.7f, $"({GridPosition.x}, {GridPosition.y})");
            
            if (RotationCount > 0)
                Handles.Label(transform.position + Vector3.down * 0.7f, $"Rotations: {RotationCount}");
        }

        private void OnDrawGizmosSelected()
        {
            if (ConnectionData == null) return;

            string nodeInfo = $"{ConnectionData.NodeName}\n{ConnectionData.NodeType}";
            if (IsSource) nodeInfo += "\n[SOURCE]";
            if (IsGoal) nodeInfo += "\n[GOAL]";
            if (!CanRotate) nodeInfo += "\n[LOCKED]";

            Handles.Label(transform.position + Vector3.left * 1f, nodeInfo);
        }
#endif

        #endregion
    }
}
