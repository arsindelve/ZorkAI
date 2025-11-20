import React from 'react';
import { render, screen } from '@testing-library/react';
import HeaderComponent from '../components/Header';

describe('Header Component', () => {
  const defaultProps = {
    locationName: 'West of House',
    moves: '10',
    score: '5'
  };

  test('renders the header component', () => {
    render(<HeaderComponent {...defaultProps} />);
    
    // Check if the component is in the document
    const headerElement = screen.getByTestId('header-location').closest('div');
    expect(headerElement).toBeInTheDocument();
  });

  test('displays the correct location name', () => {
    render(<HeaderComponent {...defaultProps} />);
    
    const locationElement = screen.getByTestId('header-location');
    expect(locationElement).toHaveTextContent('West of House');
  });

  test('displays the correct moves count', () => {
    render(<HeaderComponent {...defaultProps} />);
    
    const movesElement = screen.getByTestId('header-moves');
    expect(movesElement).toHaveTextContent('Moves: 10');
  });

  test('displays the correct score', () => {
    render(<HeaderComponent {...defaultProps} />);
    
    const scoreElement = screen.getByTestId('header-score');
    expect(scoreElement).toHaveTextContent('Score: 5');
  });

  test('renders with different props values', () => {
    const newProps = {
      locationName: 'Inside Cave',
      moves: '25',
      score: '15'
    };
    
    render(<HeaderComponent {...newProps} />);
    
    expect(screen.getByTestId('header-location')).toHaveTextContent('Inside Cave');
    expect(screen.getByTestId('header-moves')).toHaveTextContent('Moves: 25');
    expect(screen.getByTestId('header-score')).toHaveTextContent('Score: 15');
  });
});