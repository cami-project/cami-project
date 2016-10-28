/*eslint-disable react/prop-types*/

import React from 'react';
import CounterViewContainer from './counter/CounterViewContainer';
import ColorViewContainer from './colors/ColorViewContainer';
import HomepageViewContainer from './homepage/HomepageViewContainer';
import HomepageViewContainerCaregiver from './homepage-caregiver/HomepageViewContainer';
import JournalViewContainer from './journal/JournalViewContainer';
import StatusViewContainer from './status/StatusViewContainer';
import LoginViewContainer from './login/LoginViewContainer';
import OnboardingViewContainer from './onboarding/OnboardingViewContainer';

/**
 * AppRouter is responsible for mapping a navigator scene to a view
 */
export default function AppRouter(props) {
  const key = props.scene.route.key;
  switch (key) {
    case 'HomepageCaregiver':
      return <HomepageViewContainerCaregiver />;
    case 'Status':
      return <StatusViewContainer />;
    case 'Journal':
      return <JournalViewContainer />;
    case 'Homepage':
      return <HomepageViewContainer />;
    case 'Login':
    case 'Logout':
      return <LoginViewContainer />;
    case 'Onboarding':
      return <OnboardingViewContainer />;
    default:
      throw new Error('Unknown navigation key: ' + key);
  }
}
