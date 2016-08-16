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
import ActionabilityWidget from './components/ActionabilityWidget';
import icons from 'Cami/src/icons-fa';
import variables from '../variables/CaregiverGlobalVariables';
import Icon from 'react-native-vector-icons/FontAwesome';

import * as HomepageState from './HomepageState';

const HomepageView = React.createClass({
  propTypes: {
    username: PropTypes.string.isRequired,
    status: PropTypes.instanceOf(Map).isRequired,
    events: PropTypes.instanceOf(List).isRequired,
    actionability: PropTypes.instanceOf(Map).isRequired,
    dispatch: PropTypes.func.isRequired
  },

  componentDidMount() {
    setTimeout(() =>
      this.props.dispatch(HomepageState.showActionability(
        'Jim',
        icons.heart,
        1469793600,
        'Jim\'s heart rate has increased drastically!',
        'He has been reminded 3 times to take his Accupril, but did not respond.'
      )),
      3000
    );
  },

  render() {
    const heartRateData = this.props.status.get('heart').get('rate');
    const weightData = this.props.status.get('weight').get('amount');

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
        {
          this.props.actionability.get('visible')
          ?
            <ActionabilityWidget
              style={{height: 180}}
              dispatch={this.props.dispatch}
              {...this.props.actionability.get('params')}
            />
          :
          null
        }

          <View style={{flexDirection: 'row'}}>
            <StatusChart
              data={heartRateData}
              text="Heart rate"
              icon={icons.heart}
              unit="bpm"
              status="ok"
            />

            <StatusChart
              data={weightData}
              text="Weight"
              icon={icons.weight}
              unit="kg"
              status="warning"
            />
          </View>

          <View style={{flex: 1}}>
            <Text style={[variables.h2, {marginTop: 20, color: variables.colors.gray.neutral}]}>
              Latest Journal Entries
            </Text>

            <ScrollView style={{flex: 1, padding: 10}}>
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
    width: variables.dimensions.width*.5,
    justifyContent: 'flex-start',
    alignItems: 'flex-start',
    position: 'absolute',
    bottom: 10,
    left: 10
  },
  headerText: {
    fontSize: 20,
    color: 'white',
  },
  avatar: {
    top: 20,
    borderWidth: 0,
    borderRadius: 40,
    width: 80,
    height: 80,
    backgroundColor: Color('white').clearer(.25).rgbaString(),
    borderWidth: 5,
    borderColor: 'white',
    marginBottom: -15,
    marginTop: 10,
    position: 'absolute',
    marginLeft: variables.dimensions.width/2 - 40
  },
  mainContainer: {
    flex: 3,
    backgroundColor: variables.colors.background,
    zIndex: 1
  },
  mainText: {
    fontSize: 26,
    color: 'white',
    lineHeight: 1.3*26
  },
});

export default HomepageView;
