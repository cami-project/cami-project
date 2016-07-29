'use strict'
const React = require('react-native');
const {StyleSheet, Dimensions} = React;

const colors = {
  active: '#00A4EE',
  background: '#eeeeee',
  status: {
    warning: '#D2B52E',
    ok: '#658d51',
    wakeup: '#658d51'
  },
  gray: {
    darker: '#484748',
    dark: '#5a5a5a',
    neutral: '#858585',
    light: '#dbdbdb',
    lighter: '#eaeaea'
  }
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
