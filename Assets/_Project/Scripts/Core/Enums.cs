using System;
using UnityEngine;

namespace Game.Core
{

    // Defines the type of grid system used in the game.
    public enum GridType
    {
        // square grid with 4-directional movement.
        Square,

        // Hexagonal grid with 6-directional movement.
        Hex
    }

    
    // Defines the type of node in the puzzle grid.
    public enum NodeType
    {
        // Empty node that can be filled with connections.
        Empty,
        
        // Starting point of a flow path.
        Source,

        // Ending point of a flow path.
        Goal,
        
        // Pre-placed connection node.
        Connector,
        
        // Blocked node that cannot be used.
        //Obstacle
    }

    
    // Defines the six directional orientations in a hexagonal grid.
    public enum HexDirection
    {
        // Top (0° angle).
        Top = 0,
       
        // Top-right (60° angle).
        TopRight = 1,
        
        // Bottom-right (120° angle).
        BottomRight = 2,
        
        // Bottom (180° angle).
        Bottom = 3,
        
        // Bottom-left (240° angle).
        BottomLeft = 4,
        
        // Top-left (300° angle).
        TopLeft = 5
    }

    
    // Current state of the game.
    public enum GameState
    {
        Splash,
        MainMenu,
        LevelSelect,
        LoadingLevel,
        Gameplay,
        LevelComplete,
        Pause,
        Settings
    }

    public enum GameDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        VeryHard = 3,
    }
    
    // Helper class for working with hexagonal directions.
    public static class HexDirectionHelper
    {
        // Gets the angle in degrees for a given hex direction.
        public static float GetAngle(HexDirection dir)
        {
            switch (dir)
            {
                case HexDirection.Top:
                    return 0f;
                case HexDirection.TopRight:
                    return 60f;
                case HexDirection.BottomRight:
                    return 120f;
                case HexDirection.Bottom:
                    return 180f;
                case HexDirection.BottomLeft:
                    return 240f;
                case HexDirection.TopLeft:
                    return 300f;
                default:
                    return 0f;
            }
        }

        // Gets the opposite direction of a given hex direction.
        public static HexDirection GetOpposite(HexDirection dir)
        {
            return (HexDirection)(((int)dir + 3) % 6);
        }

        
        // Finds the closest hex direction for a given angle.
        public static HexDirection FromAngle(float angle)
        {
            // Normalize angle to 0-360 range
            angle = angle % 360f;
            if (angle < 0f)
                angle += 360f;

            // Find closest direction (each hex direction is 60 degrees apart)
            int direction = Mathf.RoundToInt(angle / 60f) % 6;
            return (HexDirection)direction;
        }
    }
}
