/*eslint-disable react/prop-types*/

import React from 'react';
import CounterViewContainer from './counter/CounterViewContainer';
import ColorViewContainer from './colors/ColorViewContainer';
import HomepageViewContainer from './homepage/HomepageViewContainer';
import HomepageViewContainerCaregiver from './homepage-caregiver/HomepageViewContainer';

/**
 * AppRouter is responsible for mapping a navigator scene to a view
 */
export default function AppRouter(props) {
  const key = props.scene.route.key;

  if (key === 'Counter') {
    return <CounterViewContainer />;
  }

  if (key.indexOf('Color') === 0) {
    const index = props.scenes.indexOf(props.scene);
    return (
      <ColorViewContainer
        index={index}
      />
    );
  }

  if (key === 'Homepage') {
    return <HomepageViewContainer />;
  }
  
  if (key === 'HomepageCaregiver') {
    return <HomepageViewContainerCaregiver />;
  }

  throw new Error('Unknown navigation key: ' + key);
}
