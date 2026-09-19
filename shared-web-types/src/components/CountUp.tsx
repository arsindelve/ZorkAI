import React, {useEffect, useRef, useState} from 'react';

export interface CountUpProps {
    value: string;
    className?: string;
    style?: React.CSSProperties;
    durationMs?: number;
}

export default function CountUp({value, className, style, durationMs = 450}: CountUpProps) {
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
            const progress = Math.min(1, (now - start) / durationMs);
            const eased = 1 - Math.pow(1 - progress, 3);
            setDisplay(Math.round(from + (target - from) * eased));
            if (progress < 1) frameRef.current = requestAnimationFrame(tick);
            else fromRef.current = target;
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
}
