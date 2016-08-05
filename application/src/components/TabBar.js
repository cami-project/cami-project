import React, {PropTypes} from 'react';
import TabBarButton from '../components/TabBarButton';

import {
  StyleSheet,
  View
} from 'react-native';
import variables from 'Cami/src/modules/variables/CaregiverGlobalVariables';

const TabBar = React.createClass({
  displayName: 'TabBar',
  propTypes: {
    tabs: PropTypes.array.isRequired,
    height: PropTypes.number.isRequired,
    currentTabIndex: PropTypes.number.isRequired,
    switchTab: PropTypes.func.isRequired
  },

  render() {
    const buttons = this.props.tabs.map((tab, index) => (
      <TabBarButton
        key={'tab-bar-button-' + tab.title}
        text={tab.title}
        image={tab.image}
        action={() => this.props.switchTab(index)}
        isSelected={index === this.props.currentTabIndex}
        icon={tab.icon}
      />
    ));

    return (
      <View style={[styles.navigationBar, {height: this.props.height}]}>
        {buttons}
      </View>
    );
  }
});

const styles = StyleSheet.create({
  navigationBar: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    borderTopWidth: 1,
    borderColor: variables.colors.gray.light,
    backgroundColor: variables.colors.gray.lightest,
    flexDirection: 'row',
    justifyContent: 'space-around'
  },
  buttonWrapper: {
    flex: 1,
    position: 'relative'
  }
});

export default TabBar;
