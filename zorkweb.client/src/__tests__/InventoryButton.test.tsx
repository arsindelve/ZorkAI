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
  const mockOnActionClick = jest.fn();
  const sampleInventory = ['sword', 'lantern', 'leaflet', 'food'];
  const sampleInventoryActions: Record<string, string[]> = {
    'sword': ['examine sword', 'drop sword'],
    'lantern': ['examine lantern', 'turn on lantern', 'drop lantern'],
    'leaflet': ['read leaflet', 'examine leaflet', 'drop leaflet'],
    'food': ['eat food', 'examine food', 'drop food']
  };

  beforeEach(() => {
    mockOnInventoryClick.mockClear();
    mockOnActionClick.mockClear();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('renders the inventory button', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    expect(buttonElement).toBeInTheDocument();
    expect(buttonElement).toHaveTextContent('Inventory');
  });

  test('displays the correct badge count', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent('4');
  });

  test('opens menu when clicked', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    expect(screen.getByText('sword')).toBeInTheDocument();
    expect(screen.getByText('lantern')).toBeInTheDocument();
    expect(screen.getByText('leaflet')).toBeInTheDocument();
    expect(screen.getByText('food')).toBeInTheDocument();
  });

  test('calls onInventoryClick when an item is clicked', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    const swordItem = screen.getByText('sword');
    fireEvent.click(swordItem);

    expect(mockOnInventoryClick).toHaveBeenCalledWith('sword');
  });

  test('closes menu after selecting an item', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    const lanternItem = screen.getByText('lantern');
    fireEvent.click(lanternItem);

    expect(screen.queryByText('lantern')).not.toBeVisible();
  });

  test('button is enabled after loading', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');

    expect(buttonElement).not.toBeDisabled();
    expect(buttonElement).toBeVisible();
  });

  test('renders empty inventory correctly', () => {
    render(
      <InventoryButton
        inventory={[]}
        inventoryActions={{}}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent('0');

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    const menuItems = document.querySelectorAll('.MuiMenuItem-root');
    expect(menuItems.length).toBe(0);
  });

  test('shows chevron icon for items with actions', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    // Items with actions should show chevron icons
    const chevronIcons = document.querySelectorAll('[data-testid="ChevronRightIcon"]');
    expect(chevronIcons.length).toBe(4); // All items have actions
  });

  test('opens submenu on hover and calls onActionClick when action is clicked', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={sampleInventoryActions}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    // Hover over the sword item
    const swordItem = screen.getByText('sword');
    fireEvent.mouseEnter(swordItem);

    // Check that submenu actions are displayed
    expect(screen.getByText('examine sword')).toBeInTheDocument();
    expect(screen.getByText('drop sword')).toBeInTheDocument();

    // Click an action
    fireEvent.click(screen.getByText('examine sword'));

    expect(mockOnActionClick).toHaveBeenCalledWith('examine sword');
  });

  test('falls back to inventory array when inventoryActions is empty', () => {
    render(
      <InventoryButton
        inventory={sampleInventory}
        inventoryActions={{}}
        onInventoryClick={mockOnInventoryClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toHaveTextContent('4');

    const buttonElement = screen.getByTestId('inventory-button');
    fireEvent.click(buttonElement);

    expect(screen.getByText('sword')).toBeInTheDocument();
    expect(screen.getByText('lantern')).toBeInTheDocument();
  });
});
