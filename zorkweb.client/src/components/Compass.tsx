import React from "react";
import {Mixpanel} from "../Mixpanel.ts";
import {Direction} from "../model/Directions.ts";

interface CompassProps extends React.SVGProps<SVGSVGElement> {
    onCompassClick?: (angle: string) => void; // Callback for compass click angle
    exits?: string[]; // Available exits
}

const Compass: React.FC<CompassProps> = ({onCompassClick, exits = [], className, ...rest }) => {

    // Function to check if a direction is available
    const isDirectionAvailable = (directionId: string): boolean => {
        // Map SVG IDs to Direction enum values
        const directionMap: Record<string, Direction> = {
            "North": Direction.North,
            "South": Direction.South,
            "East": Direction.East,
            "West": Direction.West,
            "NorthEast": Direction.Northeast,
            "NorthWest": Direction.Northwest,
            "SouthEast": Direction.Southeast,
            "SouthWest": Direction.Southwest
        };

        // Get the Direction enum value for this ID
        const directionValue = directionMap[directionId];

        // Map integer indices to Direction enum values
        const directionIndexMap: Record<number, Direction> = {
            0: Direction.North,
            1: Direction.South,
            2: Direction.East,
            3: Direction.West,
            4: Direction.Northeast,
            5: Direction.Northwest,
            6: Direction.Southwest,
            7: Direction.Southeast,
            8: Direction.In,
            9: Direction.Out,
            10: Direction.Up,
            11: Direction.Down
        };

        // Convert exits from strings to integers and map to Direction enum values
        const availableDirections = exits.map(exit => {
            const exitIndex = parseInt(exit, 10);
            return directionIndexMap[exitIndex];
        });

        // Check if this direction is in the available directions
        return availableDirections.includes(directionValue);
    };
    const handleClick = (event: React.MouseEvent<SVGSVGElement, MouseEvent>) => {
        if (!onCompassClick) return;

        // Stop event propagation to prevent ClickableText from handling this click
        event.stopPropagation();
        event.preventDefault();

        // Get the bounding box for the SVG
        const rect = event.currentTarget.getBoundingClientRect();

        // Find the center of the SVG
        const cx = rect.width / 2;
        const cy = rect.height / 2;

        // Mouse click coordinates relative to the SVG
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        // Vector from center
        const dx = x - cx;
        const dy = cy - y; // Note: Invert y-axis for SVG coordinate system

        // Calculate angle in radians and convert to degrees
        const angleRad = Math.atan2(dy, dx);
        const angleDeg = (angleRad * 180) / Math.PI;

        // Adjust to compass degrees (0° = North, 90° = East, etc.)
        const compassAngle = (90 - angleDeg + 360) % 360;

        const direction = getClosestDirection(compassAngle);

        Mixpanel.track('Click Compass', {
            "direction": direction,
        });

        // Emit the angle to the callback
        onCompassClick(direction);
    };

    function getClosestDirection(degrees: number): string {
        // Define the 8 compass directions
        const directions = [
            "North",     // 0° (or 360°)
            "Northeast", // 45°
            "East",      // 90°
            "Southeast", // 135°
            "South",     // 180°
            "Southwest", // 225°
            "West",      // 270°
            "Northwest", // 315°
        ];

        // Each direction covers a range of 45° (360° divided by 8 segments).
        const segmentSize = 360 / directions.length;

        // Calculate the closest index for the given degrees
        const index = Math.round(degrees / segmentSize) % directions.length;

        // Return the corresponding direction
        return directions[index];
    }

    return (

        <svg
            xmlns="http://www.w3.org/2000/svg"
            version="1.1"
            id="Layer_1"
            data-name="Layer 1"
            viewBox="0 0 50.4 50.4"
            className={className}
            onClick={handleClick}
            {...rest}
        >
            <defs>
                <style>{`
      .cls-1 { fill: #4d4d4d; transition: fill 0.2s; }
      .cls-1.highlight { fill: rgba(255, 99, 71, 0.5); }
      .cls-1.available { fill: #d3d3d3; }
    `}</style>
            </defs>

            {/* Background rectangle to catch all clicks */}
            <rect x="0" y="0" width="50.4" height="50.4" fill="transparent" style={{ pointerEvents: 'all' }} />

            {/* compass wedges */}
            <polygon
                id="West"
                className={`cls-1 ${isDirectionAvailable("West") ? "available" : ""}`}
                points="25.2 25.2 13.56 21.76 0 25.2 13.56 28.65 25.2 25.2"
            />
            <polygon
                id="East"
                className={`cls-1 ${isDirectionAvailable("East") ? "available" : ""}`}
                points="50.4 25.2 38.76 21.76 25.2 25.2 38.76 28.65 50.4 25.2"
            />
            <polygon
                id="North"
                className={`cls-1 ${isDirectionAvailable("North") ? "available" : ""}`}
                points="25.2 0 21.76 11.64 25.2 25.2 28.65 11.64 25.2 0"
            />
            <polygon
                id="South"
                className={`cls-1 ${isDirectionAvailable("South") ? "available" : ""}`}
                points="25.2 25.2 21.76 36.84 25.2 50.4 28.65 36.84 25.2 25.2"
            />
            <polygon
                id="NorthEast"
                className={`cls-1 ${isDirectionAvailable("NorthEast") ? "available" : ""}`}
                points="36.06 14.35 29.45 17.76 25.2 25.2 32.64 20.96 36.06 14.35"
            />
            <polygon
                id="NorthWest"
                className={`cls-1 ${isDirectionAvailable("NorthWest") ? "available" : ""}`}
                points="25.2 25.2 21.78 18.59 14.35 14.35 18.59 21.78 25.2 25.2"
            />
            <polygon
                id="SouthEast"
                className={`cls-1 ${isDirectionAvailable("SouthEast") ? "available" : ""}`}
                points="25.2 25.2 28.62 31.81 36.06 36.06 31.81 28.62 25.2 25.2"
            />
            <polygon
                id="SouthWest"
                className={`cls-1 ${isDirectionAvailable("SouthWest") ? "available" : ""}`}
                points="14.35 36.06 20.96 32.64 25.2 25.2 17.76 29.44 14.35 36.06"
            />
        </svg>

    );
};

export default Compass;
