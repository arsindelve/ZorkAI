import React, {forwardRef, ReactNode, useImperativeHandle} from "react";

interface ClickableTextProps extends React.HTMLAttributes<HTMLDivElement> {
    children: ReactNode;
    exits: string[]
    onWordClick?: (word: string) => void; // Callback to pass clicked word to parent
}

// Highlight registry name shared with the `::highlight(word-hover)` CSS rule.
const WORD_HOVER_HIGHLIGHT = "word-hover";

// Feature-detect the CSS Custom Highlight API. Absent in jsdom (tests) and older
// browsers, in which case hover highlighting is simply skipped.
const supportsHighlightApi = (): boolean =>
    typeof CSS !== "undefined" &&
    // @ts-expect-error - highlights is not in older TS lib.dom typings
    !!CSS.highlights &&
    typeof (globalThis as { Highlight?: unknown }).Highlight !== "undefined";

// Grow a collapsed (node, offset) position out to the whitespace-delimited word
// that contains it. Returns null when the position isn't inside a word.
const expandToWordRange = (node: Node | null, offset: number): Range | null => {
    if (!node || node.nodeType !== Node.TEXT_NODE) return null;
    const text = node.textContent ?? "";
    if (!text) return null;

    let start = offset;
    let end = offset;
    while (start > 0 && !/\s/.test(text[start - 1])) start--;
    while (end < text.length && !/\s/.test(text[end])) end++;
    if (start === end) return null;

    const range = document.createRange();
    range.setStart(node, start);
    range.setEnd(node, end);
    return range;
};

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

                    // Try to use modern Clipboard API with HTML support
                    if (navigator.clipboard && navigator.clipboard.write) {
                        try {
                            const clipboardItems = [
                                new ClipboardItem({
                                    'text/plain': new Blob([textContent], { type: 'text/plain' }),
                                    'text/html': new Blob([htmlContent], { type: 'text/html' })
                                })
                            ];
                            await navigator.clipboard.write(clipboardItems);
                            return true;
                        } catch (clipboardError) {
                            console.warn('Clipboard API write failed, falling back to alternative method:', clipboardError);
                            // Fall through to alternative methods
                        }
                    }

                    // Fallback to writeText API for plain text if available
                    if (navigator.clipboard && navigator.clipboard.writeText) {
                        await navigator.clipboard.writeText(textContent);
                    }

                    // Fallback for HTML content using legacy approach
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

                        // We've already tried the modern Clipboard API methods above
                        // Instead of using the deprecated document.execCommand('copy'),
                        // we'll inform the user that copying with formatting isn't supported in their browser
                        console.warn('Clipboard API not fully supported in this browser. Formatted copy not available.');

                        // Clean up
                        selection.removeAllRanges();
                        document.body.removeChild(tempElem);

                        // Return true if we at least managed to copy plain text earlier
                        return navigator.clipboard && navigator.clipboard.writeText !== undefined;
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

        const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
            // Check if the event has already been handled by a parent component
            if (event.defaultPrevented) return;

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
            // Only step past the leading space when the loop actually stopped ON one.
            // If it stopped because we hit the start of the text node (offset 0), the
            // first character IS part of the word and stepping forward would drop it.
            if (clonedRange.toString().indexOf(" ") === 0) {
                clonedRange.setStart(node, clonedRange.startOffset + 1);
            }

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

        const clearWordHighlight = () => {
            if (!supportsHighlightApi()) return;
            // @ts-expect-error - highlights is not in older TS lib.dom typings
            CSS.highlights.delete(WORD_HOVER_HIGHLIGHT);
        };

        // Highlight only the single word under the cursor (not the whole line),
        // signalling that individual words are clickable.
        const handleMouseMove = (event: React.MouseEvent<HTMLDivElement>) => {
            if (!supportsHighlightApi()) return;

            let node: Node | null = null;
            let offset = 0;

            const doc = document as Document & {
                caretPositionFromPoint?: (x: number, y: number) => { offsetNode: Node; offset: number } | null;
            };
            if (typeof doc.caretRangeFromPoint === "function") {
                const range = doc.caretRangeFromPoint(event.clientX, event.clientY);
                if (range) {
                    node = range.startContainer;
                    offset = range.startOffset;
                }
            } else if (typeof doc.caretPositionFromPoint === "function") {
                const pos = doc.caretPositionFromPoint(event.clientX, event.clientY);
                if (pos) {
                    node = pos.offsetNode;
                    offset = pos.offset;
                }
            }

            const wordRange = expandToWordRange(node, offset);
            if (!wordRange || !wordRange.toString().trim()) {
                clearWordHighlight();
                return;
            }

            const HighlightCtor = (globalThis as { Highlight?: new (range: Range) => unknown }).Highlight!;
            // @ts-expect-error - highlights is not in older TS lib.dom typings
            CSS.highlights.set(WORD_HOVER_HIGHLIGHT, new HighlightCtor(wordRange));
        };

        return (
            <div
                ref={divRef}
                className="clickable"
                onClick={handleClick}
                onMouseMove={handleMouseMove}
                onMouseLeave={clearWordHighlight}
                style={{cursor: "pointer"}}
                {...props}
            >
                {children}
            </div>
        );
    }
);

export default ClickableText;
