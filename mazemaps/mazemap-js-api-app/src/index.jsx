//@flow

import { MazeMapWrapper, makeMazeMapInstance } from 'src/MazeMap';

import React from 'node_modules/react';
import ReactDOM from 'node_modules/react-dom';

import './style.css'

const rootElement = document.createElement('div');
rootElement.className='pageRoot';
if (document.body) {
    document.body.appendChild(rootElement);
} else {
    throw new Error('No body found');
}

ReactDOM.render(<div>Loading...</div>, rootElement);

window.addEventListener('load', () => {

    /* Instantiate a MazeMap JS API Map */
    // TODO: Add your correct campus id number or tag string here
    const campusId = 0;
    const map = makeMazeMapInstance({campuses: campusId});

    ReactDOM.render(<div className={'appRoot'}>
        <header className={'header'}>
            <h2>Hello Map!</h2>
            <p>This page uses an npm package of MazeMap JS API</p>
        </header>
        <MazeMapWrapper map={map} />
    </div>, rootElement);
});
