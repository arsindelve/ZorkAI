import React from 'react';
import { render, fireEvent } from '@testing-library/react';
import { ClickableText } from '@zork-ai/shared-types';

// Helper: place a collapsed caret at `offset` inside the first text node of the
// rendered clickable div, then click it (handleClick reads window.getSelection()).
function clickAtOffset(container: HTMLElement, offset: number) {
    const div = container.querySelector('.clickable') as HTMLElement;
    const textNode = div.firstChild as Node;

    const range = document.createRange();
    range.setStart(textNode, offset);
    range.setEnd(textNode, offset);

    const selection = window.getSelection()!;
    selection.removeAllRanges();
    selection.addRange(range);

    fireEvent.click(div);
    return div;
}

describe('ClickableText word selection', () => {
    it('returns the whole first word when it starts the text node (no leading space)', () => {
        const onWordClick = jest.fn();
        const { container } = render(
            <ClickableText exits={[]} onWordClick={onWordClick}>
                {'north of house'}
            </ClickableText>
        );

        // Caret inside "north" (offset 2 = between 'o' and 'r').
        clickAtOffset(container, 2);

        expect(onWordClick).toHaveBeenCalledWith('north');
    });

    it('returns the whole word for a word in the middle of the text node', () => {
        const onWordClick = jest.fn();
        const { container } = render(
            <ClickableText exits={[]} onWordClick={onWordClick}>
                {'north of house'}
            </ClickableText>
        );

        // Caret inside "house" (text "north of house"; 'house' starts at index 9).
        clickAtOffset(container, 11);

        expect(onWordClick).toHaveBeenCalledWith('house');
    });
});
