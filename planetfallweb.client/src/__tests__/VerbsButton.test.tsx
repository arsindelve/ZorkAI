import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import VerbsButton from '../components/VerbsButton';

// Mock the Mixpanel module
jest.mock('../Mixpanel.ts', () => ({
  Mixpanel: {
    track: jest.fn()
  }
}));

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
    
    // Check if menu items are displayed
    expect(screen.getByText('eat')).toBeInTheDocument();
    expect(screen.getByText('drink')).toBeInTheDocument();
    expect(screen.getByText('examine')).toBeInTheDocument();
    expect(screen.getByText('take')).toBeInTheDocument();
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
    
    // Click a verb in the menu
    const examineVerb = screen.getByText('examine');
    fireEvent.click(examineVerb);
    
    // Check if onVerbClick was called with the correct verb
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
    
    // Click a verb in the menu
    const takeVerb = screen.getByText('take');
    fireEvent.click(takeVerb);
    
    // Menu should be closed, so the verb should no longer be visible
    expect(screen.queryByText('take')).not.toBeVisible();
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
    
    // Check if all expected verbs are in the menu
    const expectedVerbs = [
      "eat", "drink", "drop", "attack", "turn on", "turn off", 
      "read", "open", "close", "examine", "take"
    ];
    
    expectedVerbs.forEach(verb => {
      expect(screen.getByText(verb)).toBeInTheDocument();
    });
  });
});