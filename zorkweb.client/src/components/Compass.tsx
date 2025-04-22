import React from "react";
import {Mixpanel} from "../Mixpanel.ts";

interface CompassProps extends React.SVGProps<SVGSVGElement> {
    onCompassClick?: (angle: string) => void; // Callback for compass click angle
}

const Compass: React.FC<CompassProps> = ({onCompassClick }) => {
    const handleClick = (event: React.MouseEvent<SVGSVGElement, MouseEvent>) => {
        if (!onCompassClick) return;

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
            id="svg2"
            viewBox="0 0 64 64"
            width="200"
            height="200"
            onClick={handleClick}
        >
            <defs>
                <style>{`
      .compass-bg { fill: #fff; }
      .compass-wedge { fill: none; stroke: #231f20; stroke-miterlimit: 10; stroke-width: 0.25px; transition: fill 0.2s; }
      .compass-wedge.highlight { fill: rgba(255, 99, 71, 0.5); }
    `}</style>
            </defs>

            {/* background */}
            <rect className="compass-bg" width="64" height="64" />

            {/* compass wedges */}
            <polygon
                id="North"
                className="compass-wedge"
                points="32 7.71 28.56 19.35 32 32.91 35.44 19.35 32 7.71"
            />
            <polygon
                id="NorthEast"
                className="compass-wedge"
                points="40.9 29.28 45.4 20.47 35.93 24.66 32.42 32.56 40.9 29.28"
            />
            <polygon
                id="East"
                className="compass-wedge"
                points="57.07 32.91 45.43 29.47 31.87 32.91 45.43 36.36 57.07 32.91"
            />
            <polygon
                id="SouthEast"
                className="compass-wedge"
                points="36.83 42.09 32.33 33.27 41.8 37.47 45.32 45.36 36.83 42.09"
            />
            <polygon
                id="South"
                className="compass-wedge"
                points="32 32.91 28.56 44.55 32 58.11 35.44 44.55 32 32.91"
            />
            <polygon
                id="SouthWest"
                className="compass-wedge"
                points="27.14 42.17 31.64 33.35 22.17 37.54 18.66 45.44 27.14 42.17"
            />
            <polygon
                id="West"
                className="compass-wedge"
                points="32.17 32.91 20.53 29.47 6.97 32.91 20.53 36.36 32.17 32.91"
            />
            <polygon
                id="NorthWest"
                className="compass-wedge"
                points="23.16 29.28 18.66 20.46 28.13 24.65 31.64 32.55 23.16 29.28"
            />
        </svg>


    );
};

export default Compass;