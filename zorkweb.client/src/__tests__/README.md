# React Unit Testing with Jest

This directory contains unit tests for React components using Jest and React Testing Library.

## Testing Framework

- **Jest**: JavaScript testing framework
- **React Testing Library**: Testing utilities for React components
- **@testing-library/jest-dom**: Custom Jest matchers for DOM testing

## Running Tests

```bash
# Run all unit tests
npm run test:unit

# Run tests in watch mode (for development)
npm run test:unit:watch

# Run tests with coverage report
npm run test:unit:coverage
```

## Test File Structure

Test files should be placed in the `__tests__` directory and follow the naming convention `ComponentName.test.tsx`.

Example:
```
src/
├── __tests__/
│   ├── ComponentName.test.tsx
│   └── AnotherComponent.test.tsx
├── ComponentName.tsx
└── AnotherComponent.tsx
```

## Writing Tests

Here's a basic example of a test file:

```tsx
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import YourComponent from '../YourComponent';

describe('YourComponent', () => {
  test('renders correctly', () => {
    render(<YourComponent />);
    expect(screen.getByText('Expected Text')).toBeInTheDocument();
  });

  test('handles click events', () => {
    const handleClick = jest.fn();
    render(<YourComponent onClick={handleClick} />);
    
    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});
```

## Common Testing Patterns

### Testing Rendered Content
```tsx
test('renders correctly', () => {
  render(<Component />);
  expect(screen.getByText('Expected Text')).toBeInTheDocument();
});
```

### Testing Props
```tsx
test('displays the provided title', () => {
  render(<Component title="Test Title" />);
  expect(screen.getByText('Test Title')).toBeInTheDocument();
});
```

### Testing Events
```tsx
test('calls onClick when clicked', () => {
  const handleClick = jest.fn();
  render(<Component onClick={handleClick} />);
  
  fireEvent.click(screen.getByRole('button'));
  expect(handleClick).toHaveBeenCalledTimes(1);
});
```

### Testing User Interactions
```tsx
test('updates input value when typed', async () => {
  render(<Component />);
  
  const input = screen.getByRole('textbox');
  await userEvent.type(input, 'Hello');
  
  expect(input).toHaveValue('Hello');
});
```

## Best Practices

1. Test component behavior, not implementation details
2. Use data-testid attributes for elements that don't have semantic roles
3. Prefer user-centric queries (getByRole, getByLabelText) over implementation-centric ones
4. Mock external dependencies and API calls
5. Keep tests simple and focused on a single behavior