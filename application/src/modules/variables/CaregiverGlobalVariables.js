'use strict'
const React = require('react-native');
const {StyleSheet, Dimensions} = React;

const colors = {
  warning: '#D2B52E',
  ok: '#658d51',
  active: '#00A4EE',
  background: '#eeeeee',
  grayDark: '#484748',
  grayLight: '#dbdbdb',
  gray: '#858585'
};

let {height, width} = Dimensions.get('window');

const dimensions = {
  height: height,
  width: width
}

var styles = StyleSheet.create({
  container: {
    flex: 1,
  },
})

module.exports = styles;
module.exports.colors = colors;
module.exports.dimensions = dimensions;
