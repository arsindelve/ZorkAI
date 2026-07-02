import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { Compass } from '@zork-ai/shared-types';


describe('Compass Component', () => {
  // Helper function to find a polygon by its ID (scoped to the rose svg, since
  // the up/down controls render their own MUI icon <svg> elements too)
  const findPolygonById = (id: string) => {
    const svg = document.querySelector('[data-testid="compass-rose"]');
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

  test('renders a framing ring and cardinal labels', () => {
    render(<Compass />);

    const svg = document.querySelector('[data-testid="compass-rose"]');
    expect(svg?.querySelector('.compass-ring')).toBeInTheDocument();

    const labels = Array.from(svg?.querySelectorAll('.compass-label') ?? []).map(
      (el) => el.textContent
    );
    expect(labels).toEqual(expect.arrayContaining(['N', 'E', 'S', 'W']));
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
    
    const svg = document.querySelector('[data-testid="compass-rose"]');
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
    
    const svg = document.querySelector('[data-testid="compass-rose"]');
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

  describe('up/down lift controls', () => {
    // Index 10 = Up, 11 = Down in the Direction map. Both controls always render;
    // they are enabled only when that exit is available (disabled + dimmed otherwise).
    test('enables the up control only when Up is an available exit', () => {
      render(<Compass exits={['10']} />);
      expect(screen.getByTestId('compass-up')).toBeEnabled();
      expect(screen.getByTestId('compass-down')).toBeDisabled();
    });

    test('enables the down control when Down is an available exit', () => {
      render(<Compass exits={['11']} />);
      expect(screen.getByTestId('compass-down')).toBeEnabled();
      expect(screen.getByTestId('compass-up')).toBeDisabled();
    });

    test('disables both controls when neither is an exit', () => {
      render(<Compass exits={['0', '2']} />);
      expect(screen.getByTestId('compass-up')).toBeDisabled();
      expect(screen.getByTestId('compass-down')).toBeDisabled();
    });

    test('clicking the up control calls onCompassClick with "up"', () => {
      const mockOnCompassClick = jest.fn();
      render(<Compass exits={['10']} onCompassClick={mockOnCompassClick} />);
      fireEvent.click(screen.getByTestId('compass-up'));
      expect(mockOnCompassClick).toHaveBeenCalledWith('up');
    });

    test('clicking the down control calls onCompassClick with "down"', () => {
      const mockOnCompassClick = jest.fn();
      render(<Compass exits={['11']} onCompassClick={mockOnCompassClick} />);
      fireEvent.click(screen.getByTestId('compass-down'));
      expect(mockOnCompassClick).toHaveBeenCalledWith('down');
    });
  });
});