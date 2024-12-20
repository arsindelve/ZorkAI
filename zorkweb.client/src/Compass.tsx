import React from "react";
import {Mixpanel} from "./Mixpanel.ts";

interface CompassProps extends React.SVGProps<SVGSVGElement> {
    onCompassClick?: (angle: string) => void; // Callback for compass click angle
}

const Compass: React.FC<CompassProps> = ({onCompassClick, ...props}) => {
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

        let direction = getClosestDirection(compassAngle);
        
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
            viewBox="695 695 1345 1345"
            width="200"
            height="200"
            onClick={handleClick} // Attach click handler
            {...props} // Pass down props for flexibility
        >
            <g
                id="g8"
                transform="matrix(1.3333333,0,0,-1.3333333,0,2666.6667)"
            >
                <g id="g10" transform="scale(0.1)">
                    <path
                        d="M 20000,0 H 0 V 20000 H 20000 V 0"
                        style={{
                            fill: "blue",
                            fillOpacity: 0,
                            fillRule: "nonzero",
                            stroke: "none",
                        }}
                        id="path12"
                    />
                    <path
                        d="m 11183.6,10388.8 2368.8,-388.8 h -2282.1 c 0,139.1 -31,270.8 -86.7,388.8 z m -826.8,-3584.4 v 2281.8 c 139,0 270.7,31 388.8,86.7 z M 7161.13,10000 h 2282.12 c 0,-139.1 30.99,-270.8 86.74,-388.8 z m 3195.67,3195.7 v -2281.9 c -139.1,0 -270.8,-31 -388.82,-86.7 z m 0,-3882 c -378.29,0 -686.02,308 -686.02,686.3 0,378.3 307.73,686.3 686.02,686.3 378.2,0 686,-308 686,-686.3 0,-378.3 -307.8,-686.3 -686,-686.3 z m 4600.3,686.3 -3652.4,599.5 886.5,1234.9 -1234.9,-886.5 -599.5,3652.5 -599.56,-3652.5 -1234.92,886.5 886.51,-1234.9 -3652.41,-599.5 3652.41,-599.5 -886.51,-1235 1234.92,886.6 599.56,-3652.5 599.5,3652.5 1234.9,-886.6 -886.5,1235 3652.4,599.5"
                        style={{
                            fill: "#FFFFFF",
                            fillOpacity: 0.2,
                            fillRule: "nonzero",
                            stroke: "none",
                        }}
                        id="path14"
                    />
                    <path
                        d="m 10356.8,10477.8 c -72.8,0 -141.7,-16.2 -203.4,-45.5 -162.09,-76.2 -274.15,-241.2 -274.15,-432.3 0,-72.8 16.21,-141.6 45.22,-203.3 76.23,-162.2 241.23,-274.5 432.33,-274.5 72.8,0 141.6,16.2 203.3,45.5 162.1,76.2 274.2,241.2 274.2,432.3 0,72.8 -16.2,141.6 -45.2,203.4 -76.2,162.1 -241.2,274.4 -432.3,274.4"
                        style={{
                            fill: "#FFFFFF",
                            fillOpacity: 0.6,
                            fillRule: "nonzero",
                            stroke: "none",
                        }}
                        id="path16"
                    />
                    {/* Continue adding other paths and groups here */}
                </g>
            </g>
        </svg>
    );
};

export default Compass;