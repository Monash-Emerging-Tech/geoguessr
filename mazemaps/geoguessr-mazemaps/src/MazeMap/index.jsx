import React from 'react';
import * as Mazemap from 'mazemap';
import 'mazemap/mazemap.min.css';
import "./mazemap-wrapper.css";

export function makeMazeMapInstance(options) {
    const mazemapRoot = document.createElement('div');
    mazemapRoot.className = 'mapRoot';
    const defaultOptions = {
        container: mazemapRoot,
        campuses: 'default',
        center: {lng: 30, lat: 30},
        zoom: 1,
        zLevel: 0, // Default to ground level (LG)
        minZLevel: 0, // Minimum level (0 = ground, exclude basement)
        maxZLevel: 12, // Maximum level allowed (Clayton campus has up to 12 levels)
    };

    const mapOptions = Object.assign({}, defaultOptions, options);

    const map = new Mazemap.Map(mapOptions);
    
    // Enforce z-level restrictions to exclude basement levels but allow parking
    if (mapOptions.minZLevel !== undefined) {
        console.log(`MazeMap restricted to levels P4 to 11 (including parking levels P1-P4)`);
        
        // Override any attempts to set invalid levels
        const originalSetZLevel = map.setZLevel;
        map.setZLevel = function(level) {
            // Allow parking levels (-4 to -1) and building levels (0 to 12)
            if (level < -4 || level > 12) {
                console.warn(`Cannot set z-level to ${level}. Only parking levels (P1-P4) and building levels (LG-11) are allowed.`);
                return;
            }
            return originalSetZLevel.call(this, level);
        };
    }
    
    /* For debugging, it helps to add the map to global window
       to quickly access methods like window.mazemapinstance.getZoom(), etc.
       To do so, add the line below

       window.mazemapinstance = map;
    */

    map._clickHandler = (event) => {
        // Will be set by wrapper component
    };

    map.on('click', (event) => {
        if (map._clickHandler) {
            map._clickHandler(event);
        }
    });

    // Customize the z-level control to hide basement levels
    map.on('load', () => {
        if (mapOptions.minZLevel !== undefined && mapOptions.minZLevel >= 0) {
            // Wait for the control to be fully initialized
            setTimeout(() => {
                createCustomZLevelControl(map, mapOptions);
            }, 1000);
        }
    });

    return map;
}

// Function to create a custom z-level control
function createCustomZLevelControl(map, options) {
    try {
        // Remove the default z-level control if it exists
        if (map.zLevelControl) {
            map.zLevelControl.remove();
        }
        
        // Create custom control container
        const customControl = document.createElement('div');
        customControl.className = 'maplibregl-ctrl maplibregl-ctrl-group';
        customControl.style.cssText = `
            position: absolute;
            top: 10px;
            right: 10px;
            background: white;
            border-radius: 4px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            padding: 5px;
            z-index: 1000;
        `;
        
        // Create level selector
        const select = document.createElement('select');
        select.style.cssText = `
            padding: 5px;
            border: 1px solid #ccc;
            border-radius: 3px;
            font-size: 12px;
            min-width: 120px;
        `;
        
        // Add only the levels we want (LG and above, plus parking levels)
        const levelNames = [
            { value: -4, label: 'P4 (Parking Level 4)' },
            { value: -3, label: 'P3 (Parking Level 3)' },
            { value: -2, label: 'P2 (Parking Level 2)' },
            { value: -1, label: 'P1 (Parking Level 1)' },
            { value: 0, label: 'LG (Lower Ground)' },
            { value: 1, label: 'G (Ground)' },
            { value: 2, label: '1 (First Floor)' },
            { value: 3, label: '2 (Second Floor)' },
            { value: 4, label: '3 (Third Floor)' },
            { value: 5, label: '4 (Fourth Floor)' },
            { value: 6, label: '5 (Fifth Floor)' },
            { value: 7, label: '6 (Sixth Floor)' },
            { value: 8, label: '7 (Seventh Floor)' },
            { value: 9, label: '8 (Eighth Floor)' },
            { value: 10, label: '9 (Ninth Floor)' },
            { value: 11, label: '10 (Tenth Floor)' },
            { value: 12, label: '11 (Eleventh Floor)' }
        ];
        
        levelNames.forEach(level => {
            const option = document.createElement('option');
            option.value = level.value;
            option.textContent = level.label;
            select.appendChild(option);
        });
        
        // Handle level changes
        select.addEventListener('change', (e) => {
            const newLevel = parseInt(e.target.value);
            if (newLevel >= options.minZLevel && newLevel <= options.maxZLevel) {
                map.setZLevel(newLevel);
            }
        });
        
        // Set initial value
        select.value = options.zLevel || 0;
        
        // Add to control
        customControl.appendChild(select);
        
        // Add to map
        map.getContainer().appendChild(customControl);
        
        // Store reference for cleanup
        map._customZLevelControl = customControl;
        
        console.log('Custom z-level control created successfully - basement levels hidden');
    } catch (error) {
        console.warn('Could not create custom z-level control:', error);
    }
}

export class MazeMapWrapper extends React.Component {
    constructor(props) {
        super(props);
        this._onResizeBound = null;
    }

    componentDidMount(){
        this.props.map.on('resize', this._onResize);
        // Wire up the click handler from props to the map's click handler
        if (this.props.onMapClick) {
            this.props.map._clickHandler = this.props.onMapClick;
        }
        this._onResize();
    }

    componentWillUnmount(){
        this.props.map.off('resize', this._onResize);
        // Clean up the click handler
        this.props.map._clickHandler = null;
        
        // Clean up custom z-level control if it exists
        if (this.props.map._customZLevelControl) {
            this.props.map._customZLevelControl.remove();
            this.props.map._customZLevelControl = null;
        }
    }

    _onResize = () => {
        this._updateZLevelControlHeight();
    }

    _updateZLevelControlHeight(){
        // Update the zLevelControl maxHeight, if it exists
        const map = this.props.map;

        if(map.zLevelControl){
            var height = map.getCanvas().clientHeight;
            var maxHeight = height - 50; // 50 pixels account for margins and spacing
            map.zLevelControl.setMaxHeight(maxHeight);
        }
    }

    render() {
        if( !this.props.map ){
            return null;
        }

        return (
            <div ref={ (ref) => {
                    ref && ref.appendChild(this.props.map.getContainer() );
                    this.props.map.resize();
                }
            } className={['mazemapWrapper', this.props.className].join(' ')}> {this.props.children}</div>
        );
    }
}