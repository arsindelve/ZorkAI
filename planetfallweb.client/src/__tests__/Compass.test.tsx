import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Compass from '../components/Compass';
import { Direction } from '../model/Directions';

// Mock the Mixpanel module
jest.mock('../Mixpanel.ts', () => ({
  Mixpanel: {
    track: jest.fn()
  }
}));

describe('Compass Component', () => {
  // Helper function to find a polygon by its ID
  const findPolygonById = (id: string) => {
    const svg = document.querySelector('svg');
    return svg?.querySelector(`#${id}`);
  };

  test('renders compass with all directions', () => {
    render(<Compass />);
    
    // Check that all direction polygons exist
    expect(findPolygonById('North')).toBeInTheDocument();
    expect(findPolygonById('South')).toBeInTheDocument();
    expect(findPolygonById('East')).toBeInTheDocument();
    expect(findPolygonById('West')).toBeInTheDocument();
    expect(findPolygonById('NorthEast')).toBeInTheDocument();
    expect(findPolygonById('NorthWest')).toBeInTheDocument();
    expect(findPolygonById('SouthEast')).toBeInTheDocument();
    expect(findPolygonById('SouthWest')).toBeInTheDocument();
  });

  test('highlights available exits', () => {
    // Map direction indices to Direction enum values
    const exits = ['0', '2']; // North and East
    
    render(<Compass exits={exits} />);
    
    // Check that North and East are highlighted as available
    expect(findPolygonById('North')?.classList.contains('available')).toBe(true);
    expect(findPolygonById('East')?.classList.contains('available')).toBe(true);
    
    // Check that other directions are not highlighted
    expect(findPolygonById('South')?.classList.contains('available')).toBe(false);
    expect(findPolygonById('West')?.classList.contains('available')).toBe(false);
  });

  test('calls onCompassClick with correct direction when clicked', () => {
    const mockOnCompassClick = jest.fn();
    
    render(<Compass onCompassClick={mockOnCompassClick} />);
    
    const svg = document.querySelector('svg');
    if (!svg) throw new Error('SVG not found');
    
    // Mock getBoundingClientRect to return a fixed size
    // This allows us to simulate clicks at specific positions
    const mockRect = {
      width: 100,
      height: 100,
      left: 0,
      top: 0,
      right: 100,
      bottom: 100,
      x: 0,
      y: 0,
      toJSON: () => {}
    };
    
    svg.getBoundingClientRect = jest.fn().mockReturnValue(mockRect);
    
    // Simulate click on North (top center)
    fireEvent.click(svg, { 
      clientX: 50, // Center X
      clientY: 10  // Near top
    });
    expect(mockOnCompassClick).toHaveBeenLastCalledWith('North');
    
    // Simulate click on East (right center)
    fireEvent.click(svg, { 
      clientX: 90, // Near right
      clientY: 50  // Center Y
    });
    expect(mockOnCompassClick).toHaveBeenLastCalledWith('East');
    
    // Simulate click on South (bottom center)
    fireEvent.click(svg, { 
      clientX: 50, // Center X
      clientY: 90  // Near bottom
    });
    expect(mockOnCompassClick).toHaveBeenLastCalledWith('South');
    
    // Simulate click on West (left center)
    fireEvent.click(svg, { 
      clientX: 10, // Near left
      clientY: 50  // Center Y
    });
    expect(mockOnCompassClick).toHaveBeenLastCalledWith('West');
  });

  test('does not call onCompassClick when not provided', () => {
    render(<Compass />);
    
    const svg = document.querySelector('svg');
    if (!svg) throw new Error('SVG not found');
    
    // Mock getBoundingClientRect
    const mockRect = {
      width: 100,
      height: 100,
      left: 0,
      top: 0,
      right: 100,
      bottom: 100,
      x: 0,
      y: 0,
      toJSON: () => {}
    };
    
    svg.getBoundingClientRect = jest.fn().mockReturnValue(mockRect);
    
    // This should not throw an error
    fireEvent.click(svg, { 
      clientX: 50,
      clientY: 10
    });
  });

  test('applies additional className and props', () => {
    const testClass = 'test-class';
    const testId = 'test-compass';
    
    render(<Compass className={testClass} data-testid={testId} />);
    
    const svg = screen.getByTestId(testId);
    expect(svg).toHaveClass(testClass);
  });
});