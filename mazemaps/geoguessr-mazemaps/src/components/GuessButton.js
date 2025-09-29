import React from 'react';

const GuessButton = ({ 
  hasGuessed, 
  playerGuessMarker, 
  onGuessClick, 
  onNewGame 
}) => {
  if (hasGuessed) {
    // New Game Button - appears after guess is made
    return (
      <div style={{
        position: 'absolute',
        bottom: '20px',
        left: '50%',
        transform: 'translateX(-50%)',
        zIndex: 1000
      }}>
        <button
          onClick={onNewGame}
          style={{
            padding: '12px 24px',
            fontSize: '16px',
            fontWeight: 'bold',
            backgroundColor: '#2196F3',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            cursor: 'pointer',
            boxShadow: '0 4px 8px rgba(0,0,0,0.2)',
            transition: 'all 0.3s ease',
            fontFamily: 'OffBit-101Bold'
          }}
          onMouseOver={(e) => {
            e.target.style.backgroundColor = '#1976D2';
            e.target.style.transform = 'translateY(-2px)';
          }}
          onMouseOut={(e) => {
            e.target.style.backgroundColor = '#2196F3';
            e.target.style.transform = 'translateY(0)';
          }}
        >
          Next Round
        </button>
      </div>
    );
  }

  // Guess Button - appears before guess is made
  return (
    <div style={{
      position: 'absolute',
      bottom: '20px',
      left: '50%',
      transform: 'translateX(-50%)',
      zIndex: 1000
    }}>
      <button
        onClick={onGuessClick}
        disabled={!playerGuessMarker}
        style={{
          padding: '12px 24px',
          fontSize: '16px',
          fontWeight: 'bold',
          backgroundColor: playerGuessMarker ? '#4CAF50' : '#cccccc',
          color: 'white',
          border: 'none',
          borderRadius: '8px',
          cursor: playerGuessMarker ? 'pointer' : 'not-allowed',
          boxShadow: '0 4px 8px rgba(0,0,0,0.2)',
          transition: 'all 0.3s ease',
          fontFamily: 'OffBit-101Bold'
        }}
        onMouseOver={(e) => {
          if (playerGuessMarker) {
            e.target.style.backgroundColor = '#45a049';
            e.target.style.transform = 'translateY(-2px)';
          }
        }}
        onMouseOut={(e) => {
          if (playerGuessMarker) {
            e.target.style.backgroundColor = '#4CAF50';
            e.target.style.transform = 'translateY(0)';
          }
        }}
      >
        {playerGuessMarker ? 'GUESS' : 'PLACE YOUR PIN ON THE MAP'}
      </button>
    </div>
  );
};

export default GuessButton;
