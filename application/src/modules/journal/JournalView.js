import {List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity,
  Dimensions,
  Component
} from 'react-native';
import Color from 'color';

import JournalEntry from './components/JournalEntry';

const JournalView = React.createClass({
  propTypes: {
    events: PropTypes.instanceOf(List).isRequired,
    username: PropTypes.string.isRequired
  },

  render() {
    return (
      <View style={styles.container}>
        <View style={styles.mainContainer}>
          {this.props.events.map((event, index) =>
            <JournalEntry
              key={index}
              type={event.get('type')}
              status={event.get('status')}
              timestamp={event.get('timestamp')}
              title={event.get('title')}
              message={event.get('message')}
            />
          )}
        </View>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'moccasin'
  },
  iconContainer: {
    flex: 1,
    backgroundColor: '#658d51',
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 20
  },
  outerRing: {
    borderWidth: 2,
    borderRadius: 72,
    width: 140,
    height: 140,
    borderColor: Color('white').clearer(.75).rgbaString(),
    marginBottom: -70,
    justifyContent: 'center'
  },
  iconRing: {
    borderWidth: 0,
    borderRadius: 60,
    width: 120,
    height: 120,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    alignSelf: 'center',
    justifyContent: 'center'
  },
  mainContainer: {
    flex: 3,
    backgroundColor: '#dbdbdb',
    // alignItems: 'center',
    zIndex: 1
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
});

export default JournalView;
