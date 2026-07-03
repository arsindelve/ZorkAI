import React, { useEffect, useRef, useState } from 'react';

interface CountUpProps {
    value: string;
    className?: string;
    style?: React.CSSProperties;
    durationMs?: number;
}

// Animates the displayed number from its previous value to the new value whenever
// `value` changes (so a score jump or move tick is noticeable instead of snapping).
// Non-numeric values render verbatim, and the first render shows the value directly.
const CountUp: React.FC<CountUpProps> = ({ value, className, style, durationMs = 450 }) => {
    const target = Number(value);
    const isNumeric = value.trim() !== '' && !Number.isNaN(target);
    const [display, setDisplay] = useState<number>(isNumeric ? target : 0);
    const fromRef = useRef<number>(isNumeric ? target : 0);
    const frameRef = useRef<number | null>(null);

    useEffect(() => {
        if (!isNumeric) return;
        const from = fromRef.current;
        if (from === target) {
            setDisplay(target);
            return;
        }
        const start = performance.now();
        const tick = (now: number) => {
            const t = Math.min(1, (now - start) / durationMs);
            const eased = 1 - Math.pow(1 - t, 3); // easeOutCubic
            setDisplay(Math.round(from + (target - from) * eased));
            if (t < 1) {
                frameRef.current = requestAnimationFrame(tick);
            } else {
                fromRef.current = target;
            }
        };
        frameRef.current = requestAnimationFrame(tick);
        return () => {
            if (frameRef.current) cancelAnimationFrame(frameRef.current);
        };
    }, [target, isNumeric, durationMs]);

    return (
        <span className={className} style={style}>
            {isNumeric ? display : value}
        </span>
    );
};

export default CountUp;
