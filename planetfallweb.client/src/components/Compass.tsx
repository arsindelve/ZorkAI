import React from "react";
import {Mixpanel, Direction} from "@zork-ai/shared-types";
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';

interface CompassProps extends React.HTMLAttributes<HTMLDivElement> {
    onCompassClick?: (direction: string) => void; // Callback with the chosen direction
    exits?: string[]; // Available exits (string indices into the Direction map)
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

    // Up/Down are not angular, so they get discrete controls beside the rose rather
    // than wedges. Indices: 10 = Up, 11 = Down (see directionIndexMap above).
    const exitIndices = exits.map(exit => parseInt(exit, 10));
    const upAvailable = exitIndices.includes(10);
    const downAvailable = exitIndices.includes(11);

    const handleVerticalClick = (event: React.MouseEvent<HTMLButtonElement>, direction: string) => {
        event.stopPropagation();
        if (!onCompassClick) return;
        Mixpanel.track('Click Compass', {direction});
        onCompassClick(direction);
    };

    // Unavailable controls stay in the DOM but `visibility: hidden` so the rose
    // never shifts when an exit appears or disappears.
    const verticalButtonStyle = (available: boolean): React.CSSProperties => ({
        visibility: available ? 'visible' : 'hidden',
        borderColor: available ? 'var(--planetfall-compass)' : 'transparent',
        color: available ? '#cfeaff' : 'transparent',
        background: available ? 'color-mix(in srgb, var(--planetfall-compass) 18%, transparent)' : 'transparent',
        animation: available ? 'compassPulse 1.4s ease-in-out infinite' : undefined,
    });

    return (
        <div className={className} {...rest}>
            {/* The rose stays pinned to the right edge; the Up/Down lift grows to its
                left so appearing/disappearing controls never shift the rose. */}
            <div className="flex items-center gap-1.5">
                <div className="flex flex-col gap-1.5">
                    <button
                        type="button"
                        aria-label="up"
                        data-testid="compass-up"
                        disabled={!upAvailable}
                        onClick={(e) => handleVerticalClick(e, "up")}
                        className="flex items-center justify-center w-9 h-9 rounded-md border-2 cursor-pointer transition-all"
                        style={verticalButtonStyle(upAvailable)}
                    >
                        <KeyboardArrowUpIcon fontSize="small"/>
                    </button>
                    <button
                        type="button"
                        aria-label="down"
                        data-testid="compass-down"
                        disabled={!downAvailable}
                        onClick={(e) => handleVerticalClick(e, "down")}
                        className="flex items-center justify-center w-9 h-9 rounded-md border-2 cursor-pointer transition-all"
                        style={verticalButtonStyle(downAvailable)}
                    >
                        <KeyboardArrowDownIcon fontSize="small"/>
                    </button>
                </div>

                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    version="1.1"
                    id="Layer_1"
                    data-name="Layer 1"
                    data-testid="compass-rose"
                    viewBox="-10 -10 70.4 70.4"
                    className="cursor-pointer w-36 lg:w-44 h-auto"
                    onClick={handleClick}
                >
                    <defs>
                        <style>{`
      .cls-1 { fill: var(--planetfall-bg-medium); transition: fill 0.2s; opacity: 0.4; }
      .cls-1.highlight { fill: color-mix(in srgb, var(--planetfall-compass) 60%, transparent); }
      .cls-1.available { fill: var(--planetfall-compass); animation: compassPulse 1.4s ease-in-out infinite; }
      .cls-1.available:hover { animation: none; opacity: 1; fill: color-mix(in srgb, var(--planetfall-compass) 80%, white); }
      .compass-ring { fill: none; stroke: color-mix(in srgb, var(--planetfall-compass) 35%, transparent); stroke-width: 0.8; }
      .compass-label { fill: color-mix(in srgb, var(--planetfall-text) 65%, transparent); font-size: 7px; font-weight: bold; font-family: monospace; text-anchor: middle; dominant-baseline: central; }
    `}</style>
                    </defs>

                    {/* Background rectangle to catch all clicks (covers the padded viewBox) */}
                    <rect x="-10" y="-10" width="70.4" height="70.4" fill="transparent" style={{ pointerEvents: 'all' }} />

                    {/* Framing ring just outside the wedge tips so the dial reads as a compass */}
                    <circle className="compass-ring" cx="25.2" cy="25.2" r="27" style={{ pointerEvents: 'none' }} />

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

                    {/* Cardinal labels in the padded margin outside the dial
                        (decorative; clicks fall through) */}
                    <g style={{ pointerEvents: 'none' }}>
                        <text className="compass-label" x="25.2" y="-5.8">N</text>
                        <text className="compass-label" x="56.2" y="25.2">E</text>
                        <text className="compass-label" x="25.2" y="56.2">S</text>
                        <text className="compass-label" x="-5.8" y="25.2">W</text>
                    </g>
                </svg>
            </div>
        </div>
    );
};

export default Compass;
