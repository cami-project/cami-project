import {Map, List} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
  Image,
  TouchableOpacity,
  Dimensions,
  Component
} from 'react-native';
import Color from 'color';

import StatusChart from './components/StatusChart';
import JournalEntry from './components/JournalEntry';
import variables from '../variables/CaregiverGlobalVariables';

const HomepageView = React.createClass({
  propTypes: {
    username: PropTypes.string.isRequired,
    status: PropTypes.instanceOf(Map).isRequired,
    events: PropTypes.instanceOf(List).isRequired
  },

  render() {
    const heartRateData = this.props.status.get('heart').get('rate');
    const weightData = this.props.status.get('weigth').get('amount');

    return (
      <View style={styles.container}>

        <View style={styles.headerContainer}>
          <Image style={styles.headerBackgroundImage} source={require('../../../images/old-man.jpg')}/>
          <View style={styles.headerContainerInner}>
            <Image style={styles.avatar} source={require('../../../images/old-man.jpg')}/>
            <View style={styles.headerTextContainer}>
              <Text style={[styles.headerText, {fontWeight: 'bold'}]}>{this.props.username + '\'s'}</Text>
              <Text style={[styles.headerText, {fontSize: 18}]}>doing fine</Text>
            </View>
          </View>
        </View>

        <View style={styles.mainContainer}>
          <View style={{flexDirection: 'row'}}>
            <StatusChart
              data={heartRateData}
              text="Heart rate"
              icon="heartbeat"
              unit="bpm"
              status="ok"
            />

            <StatusChart
              data={weightData}
              text="Weight"
              icon="dashboard"
              unit="kg"
              status="warning"
            />
          </View>

          <View style={{flex: 1}}>
            <Text style={[styles.mainText, {fontWeight: 'bold'}]}>
              Latest Journal Entries
            </Text>

            <ScrollView style={{flex: 1}}>
              {/* TODO */}
              {/* Limit somehow the latest entries count */}
              {/* Maybe show only for today, or only last 5 */}
              {this.props.events.map((event, index) =>
                <JournalEntry
                  key={index}
                  type={event.get('type')}
                  status={event.get('status')}
                  timestamp={event.get('timestamp')}
                  title={event.get('title')}
                />
              )}
            </ScrollView>

          </View>

        </View>
      </View>
    );
  }
});

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: variables.colors.background
  },
  headerContainer: {
    flex: 1,
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative'
  },
  headerContainerInner: {
    flex: 1,
    justifyContent: 'center',
    backgroundColor: Color(variables.colors.status.ok).clearer(.1).rgbaString(),
    width: variables.dimensions.width,
    paddingTop: 20
  },
  headerBackgroundImage: {
    flex: 1,
    width: variables.dimensions.width + 100,
    height: variables.dimensions.height/3 - 50 - 20,
    position: 'absolute',
    resizeMode: 'cover',
    top: 0,
    left: -50,
    bottom: 0,
    right: -50,
    alignSelf: 'center'
  },
  headerTextContainer: {
    flex: 1,
    paddingLeft: 20,
    paddingBottom: 20,
    paddingRight: 20,
    width: variables.dimensions.width*.5,
    justifyContent: 'flex-start',
    alignItems: 'flex-start',
    alignSelf: 'flex-start'
  },
  headerText: {
    fontSize: 20,
    color: 'white',
  },
  avatar: {
    paddingTop: 20,
    borderWidth: 0,
    borderRadius: 40,
    width: 80,
    height: 80,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    alignSelf: 'center',
    justifyContent: 'center',
    borderWidth: 5,
    borderColor: 'white',
    marginBottom: -15,
    marginTop: 10
  },
  mainContainer: {
    flex: 3,
    backgroundColor: '#eeeeee',
    zIndex: 1
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
});

export default HomepageView;
