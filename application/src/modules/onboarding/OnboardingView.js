import {Map} from 'immutable'
import React, {PropTypes} from 'react';
import {
  StyleSheet,
  Text,
  View,
  Image,
  TouchableOpacity
} from 'react-native';

import Swiper from 'react-native-swiper';

import Icon from 'react-native-vector-icons/FontAwesome';
import icons from 'Cami/src/icons-fa';
import variables from 'Cami/src/modules/variables/ElderGlobalVariables';
import variablesCaregiver from 'Cami/src/modules/variables/CaregiverGlobalVariables';

var Color = require("color");

var Sound = require('react-native-sound');
var tapButtonSound = new Sound('sounds/just-like-that.mp3', Sound.MAIN_BUNDLE, (error) => {
  if (error) {
    console.log('failed to load the sound', error);
  }
});

const OnboardingView = React.createClass({
  getInitialState() {
      return {
        lastSlide: false
      };
  },

  handleScrollEnd: function (e, state, context) {
    if (context.state.index == (context.state.total - 1)) {
      this.setState({lastSlide: true});
    } else {
      this.setState({lastSlide: false});
    }
  },

  render() {
    return (
      <View style={[variables.container, {flex: 1}]}>
        <Image
          style={[styles.background, {zIndex: 1, resizeMode: 'cover'}]}
          source={require('../../../images/elder-bg-default.jpg')}
        />
        <View
          style={[
            styles.background,
            {
              zIndex: 2,
              backgroundColor: Color(variables.colors.status.low).clearer(.25).rgbaString()
            }
          ]}
        />
        <View style={styles.carouselWrapper}>
          <Swiper
            style={styles.carousel}
            loop={false}
            showsButtons={true}
            showsPagination={false}
            removeClippedSubviews={false}
            nextButton={
              <Icon
                name={icons.right}
                size={20}
                color={Color('white').clearer(.5).rgbaString()}
              />
            }
            prevButton={
              <Icon
                name={icons.left}
                size={20}
                color={Color('white').clearer(.5).rgbaString()}
              />
            }
            buttonWrapperStyle={{marginTop: variables.dimensions.height / 6}}
            onMomentumScrollEnd={this.handleScrollEnd}
          >
            <View style={[styles.carouselSlide, styles.carouselSlide1]}>
              <View style={styles.mainContainer}>
                <Icon
                  style={{marginBottom: 30}}
                  name={icons.heart_solid}
                  size={80}
                  color={Color('white').clearer(.25).rgbaString()}
                />
                <Text style={styles.greeting}>Welcome</Text>
                <Text style={styles.copy}>Thank you for chosing CAMI to help you care for your loved one! Before you get started, here are a few things that will help you out.</Text>
              </View>
              <View style={styles.footerContainer}>
                <View style={[styles.line, styles.lineRight, {backgroundColor: 'white'}]}></View>
                <View style={[styles.statusIconContainer, {backgroundColor: 'white'}]}>
                  <Icon
                    style={[styles.statusIcon, {paddingLeft: 2}]}
                    name={icons.right}
                    size={16}
                    color={variablesCaregiver.colors.status.ok}
                  />
                </View>
              </View>
            </View>

            <View style={[styles.carouselSlide, styles.carouselSlide2]}>
              <View style={styles.mainContainer}>
                <Text style={styles.greeting}>Stay Updated</Text>
                <Text style={styles.copy}>You get regular updates on the activity of the person you care for. Automatic Journal Entries inform you about their state and health changes.</Text>
                <Image
                  style={{
                    resizeMode: 'contain',
                    width: variables.dimensions.width,
                    height: 300,
                  }}
                  source={require('../../../images/onboarding-slide-2.png')}
                />
              </View>
              <View style={styles.footerContainer}>
                <View style={[styles.line, styles.lineFull, {backgroundColor: variablesCaregiver.colors.status.ok}]}></View>
                <View style={[styles.statusIconContainer, {backgroundColor: variablesCaregiver.colors.status.ok}]}>
                  <Icon
                    style={styles.statusIcon}
                    name={icons.ok}
                    size={16}
                    color="white"
                  />
                </View>
              </View>
            </View>

            <View style={[styles.carouselSlide, styles.carouselSlide3]}>
              <View style={styles.mainContainer}>
                <Text style={styles.greeting}>Monitor Health</Text>
                <Text style={styles.copy}>CAMI turns the data it collects from sensors into quick reference diagrams. That's how you always know what's going on, without having to worry.</Text>
                <Image
                  style={{
                    resizeMode: 'contain',
                    width: variables.dimensions.width,
                    height: 300,
                  }}
                  source={require('../../../images/onboarding-slide-3.png')}
                />
              </View>
              <View style={styles.footerContainer}>
                <View style={[styles.line, styles.lineFull, {backgroundColor: variablesCaregiver.colors.status.warning}]}></View>
                <View style={[styles.statusIconContainer, {backgroundColor: variablesCaregiver.colors.status.warning}]}>
                  <Icon
                    style={styles.statusIcon}
                    name={icons.warning}
                    size={16}
                    color="white"
                  />
                </View>
              </View>
            </View>

            <View style={[styles.carouselSlide, styles.carouselSlide4]}>
              <View style={styles.mainContainer}>
                <Text style={styles.greeting}>React in Time</Text>
                <Text style={styles.copy}>As soon as the app detects that something is wrong, it alerts you immediately. Emergency notifications empower you to take action fast.</Text>
                <Image
                  style={{
                    resizeMode: 'contain',
                    width: variables.dimensions.width,
                    height: 300,
                  }}
                  source={require('../../../images/onboarding-slide-4.png')}
                />
              </View>
              <View style={styles.footerContainer}>
                <View style={[styles.line, styles.lineFull, {backgroundColor: variablesCaregiver.colors.status.alert}]}></View>
                <View style={[styles.statusIconContainer, {backgroundColor: variablesCaregiver.colors.status.alert}]}>
                  <Icon
                    style={styles.statusIcon}
                    name={icons.warning}
                    size={16}
                    color="white"
                  />
                </View>
              </View>
            </View>

            <View style={[styles.carouselSlide, styles.carouselSlide5]}>
              <View style={styles.mainContainer}>
                <Text style={styles.greeting}>Get Started!</Text>
                <Text style={styles.copy}>Connect CAMI with the devices that monitor the person you care for. Together we'll make sure they stay well and healthy!</Text>
                <View style={{flexDirection: 'row', padding: 40}}>
                  <Icon
                    style={{padding: 10}}
                    name={icons.weight}
                    size={28}
                    color={Color('white').clearer(.5).rgbaString()}
                  />
                  <Icon
                    style={{padding: 10}}
                    name={icons.heart}
                    size={28}
                    color={Color('white').clearer(.5).rgbaString()}
                  />
                  <Icon
                    style={{padding: 10}}
                    name={icons.camera}
                    size={28}
                    color={Color('white').clearer(.5).rgbaString()}
                  />
                  <Icon
                    style={{padding: 10}}
                    name={icons.tv}
                    size={28}
                    color={Color('white').clearer(.5).rgbaString()}
                  />
                </View>
                <TouchableOpacity
                  style={[
                    styles.button,
                    styles.buttonConfirm,
                    {
                      shadowColor: Color(variables.colors.status.low).darken(.8).hexString()
                    }
                  ]}
                  onPress={() => tapButtonSound.setVolume(1.0).play()}
                >
                  <Icon
                    style={{paddingBottom: 4, marginTop: -8}}
                    name={icons.smartphone}
                    size={40}
                    color={variables.colors.status.low}
                  />
                  <Text style={styles.buttonText}>Let's Go!</Text>
                </TouchableOpacity>
              </View>
              <View style={styles.footerContainer}>
                <View style={[styles.line, styles.lineLeft, {backgroundColor: 'white'}]}></View>
                <View style={[styles.statusIconContainer, {backgroundColor: 'white'}]}>
                  <Icon
                    style={styles.statusIcon}
                    name={icons.ok}
                    size={16}
                    color={variablesCaregiver.colors.status.ok}
                  />
                </View>
              </View>
            </View>
          </Swiper>
          <View style={styles.skipContainer}>
            <TouchableOpacity
              style={styles.skipButton}
              onPress={() => tapButtonSound.setVolume(1.0).play()}
            >
              <Text style={styles.skipText}>{!this.state.lastSlide ? 'SKIP' : 'Need help?'}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    );
  }
});

const buttonCircle = {
  borderWidth: 0,
  borderRadius: 60,
  width: 120,
  height: 120
};

const styles = StyleSheet.create({
  background: {
    position: 'absolute',
    flex: 1,
    width: variables.dimensions.width,
    height: variables.dimensions.height,
    top: 0,
    left: 0,
    alignSelf: 'center',
  },
  carouselWrapper: {
    zIndex: 5,
    position: 'relative',
    backgroundColor: 'transparent',
    flex: 1,
    paddingTop: 60,
    paddingBottom: 89
  },
  carousel: {
    backgroundColor: 'transparent',
  },
  carouselSlide: {
    // substracting wrapper's top/bottom padding & half of footer container's height
    height: variables.dimensions.height - 60 - 89 - 25,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'transparent',
    position: 'relative',
  },
  skipContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 6,
    backgroundColor: 'transparent',
    height: 34,
    width: variables.dimensions.width,
    position: 'absolute',
    bottom: 0,
  },
  skipButton: {
    paddingTop: 10,
    paddingBottom: 10,
    paddingRight: 15,
    paddingLeft: 15
  },
  skipText: {
    fontSize: 14,
    fontWeight: 'bold',
    lineHeight: 14,
    color: Color('white').clearer(.25).rgbaString()
  },
  mainContainer: {
    alignItems: 'center',
    justifyContent: 'flex-start',
    backgroundColor: 'transparent',
    flex: 1
  },
  greeting: {
    fontSize: 28,
    color: Color('white').clearer(.1).rgbaString()
  },
  copy: {
    marginTop: 30,
    width: (variables.dimensions.width * 0.75),
    fontSize: 14,
    fontWeight: 'bold',
    lineHeight: 14 * 1.5,
    textAlign: 'center',
    color: Color('white').clearer(.1).rgbaString()
  },
  footerContainer: {
    alignItems: 'center',
    backgroundColor: 'transparent',
    position: 'absolute',
    height: 50,
    width: variables.dimensions.width
  },
  linesContainer: {
    alignSelf: 'center',
    position: 'relative',
    flexDirection: 'column',
    flex: 1,
  },
  line: {
    alignSelf: 'center',
    height: 2
  },
  lineRight: {
    width: variables.dimensions.width / 2,
    marginLeft: variables.dimensions.width / 2
  },
  lineLeft: {
    width: variables.dimensions.width / 2,
    marginRight: variables.dimensions.width / 2
  },
  lineFull: {
    width: variables.dimensions.width
  },
  statusIconContainer: {
    position: 'absolute',
    left: (variables.dimensions.width / 2) - 15,
    top: -14,
    width: 30,
    height: 30,
    borderRadius: 15,
    justifyContent: 'center'
  },
  statusIcon: {
    alignSelf: 'center',
  },
  button: {
    ...buttonCircle,
    alignSelf: 'center',
    justifyContent: 'center',
    alignItems: 'center',
    shadowRadius: 40,
    shadowOffset: {width: 0, height: 0},
    shadowOpacity: 0.5,
    backgroundColor: 'white',
    marginTop: 20
  },
  buttonText: {
    backgroundColor: 'transparent',
    textAlign: 'center',
    alignSelf: 'center',
    fontSize: 16,
    fontWeight: 'bold',
    color: Color(variables.colors.status.low).clearer(.05).rgbaString()
  }
});

export default OnboardingView;
