import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import {LocationButton} from '@zork-ai/shared-types';

describe('LocationButton Component', () => {
  const mockOnItemClick = jest.fn();
  const mockOnActionClick = jest.fn();
  const sampleLocationActions: Record<string, string[]> = {
    'console': ['examine console', 'use console'],
    'door': ['open door', 'examine door'],
    'panel': ['examine panel', 'press panel'],
    'window': ['look through window', 'examine window']
  };

  beforeEach(() => {
    mockOnItemClick.mockClear();
    mockOnActionClick.mockClear();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('renders the location button', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    expect(buttonElement).toBeInTheDocument();
    expect(buttonElement).toHaveTextContent('Location');
  });

  test('displays the correct badge count', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
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
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    // Items displayed in Sentence Case
    expect(screen.getByText('Console')).toBeInTheDocument();
    expect(screen.getByText('Door')).toBeInTheDocument();
    expect(screen.getByText('Panel')).toBeInTheDocument();
    expect(screen.getByText('Window')).toBeInTheDocument();
  });

  test('calls onItemClick when an item is clicked', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    // Click item displayed in Sentence Case
    const consoleItem = screen.getByText('Console');
    fireEvent.click(consoleItem);

    // Callback receives original lowercase value
    expect(mockOnItemClick).toHaveBeenCalledWith('console');
  });

  test('closes menu after selecting an item', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    // Click item displayed in Sentence Case
    const doorItem = screen.getByText('Door');
    fireEvent.click(doorItem);

    expect(screen.queryByText('Door')).not.toBeVisible();
  });

  test('button is enabled after loading', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');

    expect(buttonElement).not.toBeDisabled();
    expect(buttonElement).toBeVisible();
  });

  test('renders empty location correctly', () => {
    render(
      <LocationButton
        locationActions={{}}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const badge = document.querySelector('.MuiBadge-badge');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveTextContent('0');

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    const menuItems = document.querySelectorAll('.MuiMenuItem-root');
    expect(menuItems.length).toBe(0);
  });

  test('shows chevron icon for items with actions', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    // Items with actions should show chevron icons
    const chevronIcons = document.querySelectorAll('[data-testid="ChevronRightIcon"]');
    expect(chevronIcons.length).toBe(4); // All items have actions
  });

  test('opens submenu on hover and calls onActionClick when action is clicked', () => {
    render(
      <LocationButton
        locationActions={sampleLocationActions}
        onItemClick={mockOnItemClick}
        onActionClick={mockOnActionClick}
      />
    );

    act(() => {
      jest.runAllTimers();
    });

    const buttonElement = screen.getByTestId('location-button');
    fireEvent.click(buttonElement);

    // Hover over the console item (displayed in Sentence Case)
    const consoleItem = screen.getByText('Console');
    fireEvent.mouseEnter(consoleItem);

    // Check that submenu actions are displayed (Sentence Case)
    expect(screen.getByText('Examine console')).toBeInTheDocument();
    expect(screen.getByText('Use console')).toBeInTheDocument();

    // Click an action
    fireEvent.click(screen.getByText('Examine console'));

    // Callback receives original lowercase value
    expect(mockOnActionClick).toHaveBeenCalledWith('examine console');
  });
});
