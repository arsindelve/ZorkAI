import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import LocationButton from '../components/LocationButton';

// Mock the Mixpanel module
jest.mock('../Mixpanel.ts', () => ({
  Mixpanel: {
    track: jest.fn()
  }
}));

describe('LocationButton Component', () => {
  const mockOnItemClick = jest.fn();
  const mockOnActionClick = jest.fn();
  const sampleLocationActions: Record<string, string[]> = {
    'mailbox': ['open mailbox', 'examine mailbox'],
    'door': ['open door', 'examine door', 'knock on door'],
    'tree': ['climb tree', 'examine tree'],
    'window': ['open window', 'examine window']
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

    expect(screen.getByText('mailbox')).toBeInTheDocument();
    expect(screen.getByText('door')).toBeInTheDocument();
    expect(screen.getByText('tree')).toBeInTheDocument();
    expect(screen.getByText('window')).toBeInTheDocument();
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

    const mailboxItem = screen.getByText('mailbox');
    fireEvent.click(mailboxItem);

    expect(mockOnItemClick).toHaveBeenCalledWith('mailbox');
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

    const doorItem = screen.getByText('door');
    fireEvent.click(doorItem);

    expect(screen.queryByText('door')).not.toBeVisible();
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

    // Hover over the mailbox item
    const mailboxItem = screen.getByText('mailbox');
    fireEvent.mouseEnter(mailboxItem);

    // Check that submenu actions are displayed
    expect(screen.getByText('open mailbox')).toBeInTheDocument();
    expect(screen.getByText('examine mailbox')).toBeInTheDocument();

    // Click an action
    fireEvent.click(screen.getByText('open mailbox'));

    expect(mockOnActionClick).toHaveBeenCalledWith('open mailbox');
  });
});
