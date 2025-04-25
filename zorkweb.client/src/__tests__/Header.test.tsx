import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import Header from '../Header';

describe('Header', () => {
  it('renders the location name', () => {
    render(<Header locationName="West of House" moves="10" score="50" />);
    
    expect(screen.getByText('West of House')).toBeInTheDocument();
  });
  
  it('renders the moves count', () => {
    render(<Header locationName="Inside House" moves="15" score="75" />);
    
    expect(screen.getByText('Moves: 15')).toBeInTheDocument();
  });
  
  it('renders the score', () => {
    render(<Header locationName="Forest" moves="20" score="100" />);
    
    expect(screen.getByText('Score: 100')).toBeInTheDocument();
  });
  
  it('renders all information correctly', () => {
    const locationName = 'Maze';
    const moves = '25';
    const score = '125';
    
    render(<Header locationName={locationName} moves={moves} score={score} />);
    
    expect(screen.getByText(locationName)).toBeInTheDocument();
    expect(screen.getByText(`Moves: ${moves}`)).toBeInTheDocument();
    expect(screen.getByText(`Score: ${score}`)).toBeInTheDocument();
  });
  
  it('renders with empty values', () => {
    render(<Header locationName="" moves="" score="" />);
    
    expect(screen.getByText('Moves:')).toBeInTheDocument();
    expect(screen.getByText('Score:')).toBeInTheDocument();
  });
});