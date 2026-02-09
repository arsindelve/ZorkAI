import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import {VerbsButton} from '@zork-ai/shared-types';

describe('VerbsButton Component', () => {
  const mockOnVerbClick = jest.fn();

  beforeEach(() => {
    // Clear mock function calls before each test
    mockOnVerbClick.mockClear();
    // Setup fake timers for all tests
    jest.useFakeTimers();
  });

  afterEach(() => {
    // Cleanup timers after each test
    jest.useRealTimers();
  });

  test('renders the verbs button', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Check if the button is rendered
    const buttonElement = screen.getByText('Verbs');
    expect(buttonElement).toBeInTheDocument();
  });

  test('opens menu when clicked', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Verbs');
    fireEvent.click(buttonElement);

    // Check if menu items are displayed (Sentence Case)
    expect(screen.getByText('Eat')).toBeInTheDocument();
    expect(screen.getByText('Drink')).toBeInTheDocument();
    expect(screen.getByText('Examine')).toBeInTheDocument();
    expect(screen.getByText('Take')).toBeInTheDocument();
  });

  test('calls onVerbClick when a verb is selected', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Verbs');
    fireEvent.click(buttonElement);

    // Click a verb in the menu (displayed in Sentence Case)
    const examineVerb = screen.getByText('Examine');
    fireEvent.click(examineVerb);

    // Check if onVerbClick was called with the original lowercase verb
    expect(mockOnVerbClick).toHaveBeenCalledWith('examine');
  });

  test('closes menu after selecting a verb', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Verbs');
    fireEvent.click(buttonElement);

    // Click a verb in the menu (displayed in Sentence Case)
    const takeVerb = screen.getByText('Take');
    fireEvent.click(takeVerb);

    // Menu should be closed, so the verb should no longer be visible
    expect(screen.queryByText('Take')).not.toBeVisible();
  });

  test('button is enabled after loading', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Get the button element
    const buttonElement = screen.getByText('Verbs');

    // After useEffect runs, button should be enabled
    expect(buttonElement).not.toBeDisabled();
    expect(buttonElement).toBeVisible();
  });

  test('displays all expected verbs in the menu', () => {
    render(<VerbsButton onVerbClick={mockOnVerbClick} />);

    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });

    // Click the button to open the menu
    const buttonElement = screen.getByText('Verbs');
    fireEvent.click(buttonElement);

    // Check if all expected verbs are in the menu (Sentence Case)
    const expectedVerbs = [
      "Eat", "Drink", "Drop", "Attack", "Turn on", "Turn off",
      "Read", "Open", "Close", "Examine", "Take"
    ];

    expectedVerbs.forEach(verb => {
      expect(screen.getByText(verb)).toBeInTheDocument();
    });
  });
});
