'use strict'
const React = require('react-native');
const {StyleSheet, Dimensions} = React;

const colors = {
  active: '#00A4EE',
  background: '#eeeeee',
  status: {
    low: '#0fad82',
    medium: '#a7b50a',
    high: '#458bcc'
  },
  text: '#158A12',
  panic: '#C95F5F',
  gray: {
    darker: '#484748',
    dark: '#5a5a5a',
    neutral: '#858585',
    light: '#dbdbdb',
    lighter: '#eaeaea',
    lightest: '#fafafa'
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
    position: 'relative'
  }
})

module.exports = styles;
module.exports.colors = colors;
module.exports.dimensions = dimensions;
