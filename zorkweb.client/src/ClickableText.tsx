import React, {forwardRef, ReactNode} from "react";

interface ClickableTextProps extends React.HTMLAttributes<HTMLDivElement> {
    children: ReactNode;
    onWordClick?: (word: string) => void; // Callback to pass clicked word to parent
}

const ClickableText = forwardRef<HTMLDivElement, ClickableTextProps>(
    ({children, onWordClick, ...props}, ref) => {
        const handleClick = (_: React.MouseEvent<HTMLDivElement>) => {
            const selection = window.getSelection();

            if (!selection || selection.rangeCount === 0) return;

            const range = selection.getRangeAt(0);
            const node = selection.anchorNode;

            if (!node || node.nodeType !== Node.TEXT_NODE) return;

            // Clone the range to avoid modifying the user's actual selection
            const clonedRange = range.cloneRange();

            // Find starting point
            while (
                clonedRange.toString().indexOf(" ") !== 0 &&
                clonedRange.startOffset > 0
                ) {
                clonedRange.setStart(node, clonedRange.startOffset - 1);
            }
            clonedRange.setStart(node, clonedRange.startOffset + 1);

            // Find ending point
            // Keep extending until we find a space or reach the end of the text
            while (
                clonedRange.toString().indexOf(" ") === -1 &&
                clonedRange.toString().trim() !== "" &&
                clonedRange.endOffset < node.textContent!.length
                ) {
                clonedRange.setEnd(node, clonedRange.endOffset + 1);
            }

            // If we've reached the end of the text without finding a space,
            // make sure we include the entire word
            if (clonedRange.endOffset >= node.textContent!.length) {
                clonedRange.setEnd(node, node.textContent!.length);
            }

            // Get the selected word
            const selectedWord = clonedRange.toString().trim();
            if (selectedWord && onWordClick) {
                onWordClick(selectedWord); // Pass the word to the parent via the callback
            }
        };

        return (
            <div
                ref={ref}
                className="clickable"
                onClick={handleClick}
                style={{cursor: "pointer"}}
                {...props}
            >
                {children}
            </div>
        );
    }
);

export default ClickableText;
