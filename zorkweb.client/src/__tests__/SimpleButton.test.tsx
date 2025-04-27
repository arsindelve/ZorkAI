import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import SimpleButton from '../SimpleButton';

describe('SimpleButton Component', () => {
  test('renders button with correct text', () => {
    render(<SimpleButton text="Click me" />);

    const buttonElement = screen.getByTestId('simple-button');
    expect(buttonElement).toBeInTheDocument();
    expect(buttonElement).toHaveTextContent('Click me');
  });

  test('calls onClick handler when clicked', () => {
    const handleClick = jest.fn();
    render(<SimpleButton text="Click me" onClick={handleClick} />);

    const buttonElement = screen.getByTestId('simple-button');
    fireEvent.click(buttonElement);

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  test('has correct class name', () => {
    render(<SimpleButton text="Click me" />);

    const buttonElement = screen.getByTestId('simple-button');
    expect(buttonElement).toHaveClass('simple-button');
  });
});
