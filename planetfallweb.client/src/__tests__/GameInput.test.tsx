import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import GameInput from '../components/GameInput';

describe('GameInput Component', () => {
  const mockSetInput = jest.fn();
  const mockHandleKeyDown = jest.fn();
  const mockRef = { current: null } as React.RefObject<HTMLInputElement>;

  beforeEach(() => {
    // Clear mock function calls before each test
    mockSetInput.mockClear();
    mockHandleKeyDown.mockClear();
  });

  test('renders input with correct placeholder', () => {
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={false}
        playerInput=""
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input');
    expect(inputElement).toBeInTheDocument();
    expect(inputElement).toHaveAttribute('placeholder', 'Type your command, then press enter/return...');
  });

  test('displays the current input value', () => {
    const testValue = 'test command';
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={false}
        playerInput={testValue}
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input');
    expect(inputElement).toHaveValue(testValue);
  });

  test('calls setInput when input value changes', () => {
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={false}
        playerInput=""
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input') as HTMLInputElement;
    fireEvent.change(inputElement, { target: { value: 'new command' } });
    
    expect(mockSetInput).toHaveBeenCalledWith('new command');
  });

  test('calls handleKeyDown when a key is pressed', () => {
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={false}
        playerInput=""
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input');
    fireEvent.keyDown(inputElement, { key: 'Enter', code: 'Enter' });
    
    expect(mockHandleKeyDown).toHaveBeenCalledTimes(1);
  });

  test('input is readonly when isPending is true', () => {
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={true}
        playerInput=""
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input');
    expect(inputElement).toHaveAttribute('readonly');
  });

  test('changes appearance when focused and blurred', () => {
    render(
      <GameInput
        playerInputElement={mockRef}
        isPending={false}
        playerInput=""
        setInput={mockSetInput}
        handleKeyDown={mockHandleKeyDown}
      />
    );

    const inputElement = screen.getByTestId('game-input');
    
    // Initial state (not focused)
    const container = inputElement.closest('div')?.parentElement?.querySelector('div');
    expect(container?.className).toContain('opacity-50');
    
    // Focus the input
    fireEvent.focus(inputElement);
    expect(container?.className).toContain('opacity-100');
    
    // Blur the input
    fireEvent.blur(inputElement);
    expect(container?.className).toContain('opacity-50');
  });
});