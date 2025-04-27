import React from 'react';

interface SimpleButtonProps {
  text: string;
  onClick?: () => void;
}

const SimpleButton: React.FC<SimpleButtonProps> = ({ text, onClick }) => {
  return (
    <button 
      className="simple-button" 
      onClick={onClick}
      data-testid="simple-button"
    >
      {text}
    </button>
  );
};

export default SimpleButton;