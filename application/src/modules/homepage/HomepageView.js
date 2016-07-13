import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View
} from 'react-native';

const HomepageView = React.createClass({
  propTypes: {
    notification: PropTypes.instanceOf(Map).isRequired
  },

  render() {
    return (
      <View>
        <Text>
          {this.props.notification.get("message")}
        </Text>
      </View>
    );
  }
});

const styles = StyleSheet.create({});

export default HomepageView;
