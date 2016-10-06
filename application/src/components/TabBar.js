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
    var tabIndexes = {};
    const filteredTabs = this.props.tabs.filter((tab, index) => {
      tabIndexes[tab.key] = index;
      return tab.showInTabBar;
    });
    const buttons = filteredTabs.map((tab, index) => (
      <TabBarButton
        key={'tab-bar-button-' + tab.title}
        text={tab.title}
        image={tab.image}
        action={() => this.props.switchTab(tabIndexes[tab.key])}
        isSelected={tabIndexes[tab.key] === this.props.currentTabIndex}
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
