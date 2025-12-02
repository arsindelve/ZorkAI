import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import InventoryButton from '../components/InventoryButton';

// Mock the Mixpanel module
jest.mock('../Mixpanel.ts', () => ({
  Mixpanel: {
    track: jest.fn()
  }
}));

describe('InventoryButton Component', () => {
  const mockOnInventoryClick = jest.fn();
  const sampleInventory = ['sword', 'lantern', 'leaflet', 'food'];

  beforeEach(() => {
    // Clear mock function calls before each test
    mockOnInventoryClick.mockClear();
    // Setup fake timers for all tests
    jest.useFakeTimers();
  });

  afterEach(() => {
    // Cleanup timers after each test
    jest.useRealTimers();
  });

  test('renders the inventory button', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Check if the button is rendered
    const buttonElement = screen.getByTestId('inventory-button');
    expect(buttonElement).toBeInTheDocument();
    expect(buttonElement).toHaveTextContent('Inventory');
  });

  test('displays the correct badge count', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Check if the badge shows the correct count
    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent('4'); // sampleInventory.length
  });

  test('opens menu when clicked', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Click the button to open the menu
    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);
    
    // Check if menu items are displayed
    expect(screen.getByText('sword')).toBeInTheDocument();
    expect(screen.getByText('lantern')).toBeInTheDocument();
    expect(screen.getByText('leaflet')).toBeInTheDocument();
    expect(screen.getByText('food')).toBeInTheDocument();
  });

  test('calls onInventoryClick when an item is selected', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Click the button to open the menu
    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);
    
    // Click an item in the menu
    const swordItem = screen.getByText('sword');
    fireEvent.click(swordItem);
    
    // Check if onInventoryClick was called with the correct item
    expect(mockOnInventoryClick).toHaveBeenCalledWith('sword');
  });

  test('closes menu after selecting an item', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Click the button to open the menu
    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);
    
    // Click an item in the menu
    const lanternItem = screen.getByText('lantern');
    fireEvent.click(lanternItem);
    
    // Menu should be closed, so the item should no longer be visible
    expect(screen.queryByText('lantern')).not.toBeVisible();
  });

  test('button is enabled after loading', () => {
    render(<InventoryButton inventory={sampleInventory} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Get the button element
    const buttonElement = screen.getByTestId('inventory-button');
    
    // After useEffect runs, button should be enabled
    expect(buttonElement).not.toBeDisabled();
    expect(buttonElement).toBeVisible();
  });

  test('renders empty inventory correctly', () => {
    render(<InventoryButton inventory={[]} onInventoryClick={mockOnInventoryClick} />);
    
    // Run useEffect to set isLoaded to true
    act(() => {
      jest.runAllTimers();
    });
    
    // Check if the badge shows zero
    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent('0');
    
    // Click the button to open the menu
    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);
    
    // Menu should be empty
    const menuItems = document.querySelectorAll('.MuiMenuItem-root');
    expect(menuItems.length).toBe(0);
  });
});