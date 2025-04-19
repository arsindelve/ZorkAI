import React, {forwardRef, ReactNode, useImperativeHandle} from "react";

interface ClickableTextProps extends React.HTMLAttributes<HTMLDivElement> {
    children: ReactNode;
    onWordClick?: (word: string) => void; // Callback to pass clicked word to parent
}

// Define the ref handle type
export interface ClickableTextHandle {
    copyToClipboardAsRTF: () => Promise<boolean>;
    scrollToBottom: () => void;
}

const ClickableText = forwardRef<HTMLDivElement & ClickableTextHandle, ClickableTextProps>(
    ({children, onWordClick, ...props}, ref) => {
        // Create a local ref to store the div element
        const divRef = React.useRef<HTMLDivElement>(null);

        // Expose methods to parent components
        useImperativeHandle(ref, () => ({
            ...divRef.current!,
            scrollToBottom: () => {
                if (divRef.current) {
                    divRef.current.scrollTop = divRef.current.scrollHeight;
                }
            },
            copyToClipboardAsRTF: async () => {
                if (!divRef.current) return false;

                try {
                    // Get the HTML content of the div
                    const htmlContent = divRef.current.innerHTML;

                    // Get plain text content (for fallback)
                    const textContent = divRef.current.textContent || '';

                    // Use the Clipboard API to write plain text if available
                    if (navigator.clipboard && navigator.clipboard.writeText) {
                        await navigator.clipboard.writeText(textContent);
                    }

                    // Create a temporary element to copy HTML content
                    const tempElem = document.createElement('div');
                    tempElem.innerHTML = htmlContent;
                    document.body.appendChild(tempElem);

                    // Select the content
                    const range = document.createRange();
                    range.selectNodeContents(tempElem);

                    const selection = window.getSelection();
                    if (selection) {
                        selection.removeAllRanges();
                        selection.addRange(range);

                        // Execute copy command to get HTML formatting
                        document.execCommand('copy');

                        // Clean up
                        selection.removeAllRanges();
                        document.body.removeChild(tempElem);

                        return true;
                    }

                    // Clean up if selection failed
                    document.body.removeChild(tempElem);
                    return false;
                } catch (error) {
                    console.error('Failed to copy text to clipboard:', error);
                    return false;
                }
            }
        }));

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
                ref={divRef}
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
