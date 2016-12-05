// Gruntfile.js
//
// // all configuration goes inside this function
module.exports = function(grunt) {

  // ===========================================================================
  // CONFIGURE GRUNT ===========================================================
  // ===========================================================================
  grunt.initConfig ({

    // get the configuration info from package.json ----------------------------
    // this way we can use things like name and version (pkg.name)
    pkg: grunt.file.readJSON('package.json'),
    projectName: {
      data: '<%= pkg.name %>'
    },
    // all of our configuration will go here

    // compile sass stylesheets to css -----------------------------------------
    sass: {
      dist: {
        options: {
          precision: 5, // How many digits of precision to use when outputting decimal numbers.
          style: 'expanded' // Output style. Can be nested, compact, compressed, expanded.
        },
        files: {
          'src/css/main.css': 'src/sass/main.scss',
        }
      },
    },

    // concatinate js and css files ------------------------------------
    concat: {
      options: {
        separator: '\n\r',
      },
      styles: {
        files: {
          'src/css/styles-overrides-bundle.css' : [
            'src/css/main.css'
          ],
        }
      }
    },

    // configure cssmin to minify css files ------------------------------------
    cssmin: {
      build: {
        files: {
          'assets/styles/styles-overrides-bundle.min.css': 'src/css/styles-overrides-bundle.css'
        }
      }
    },

    // configure watch to auto update ----------------
    watch: {
      options: {
        livereload: true,
        dateFormat: function(time) {
          grunt.log.writeln('The watch finished in ' + time + 'ms at' + (new Date()).toString());
          grunt.log.writeln('Waiting for more changes...');
        },
      },

      // for stylesheets, watch css and sass files
      // only run sass and cssmin
      stylesheets: {
        files: ['src/sass//*.scss'],
        tasks: ['sass', 'concat:styles', 'cssmin', 'notify:stylesheets'],
        options: {
          livereload: true,
          atBegin: false,
          spawn: true
        }
      },
    },

    notify: {
      stylesheets: {
        options: {
          title: 'Task Complete',  // optional
          message: 'Styles are 5/5!' //required
        }
      },
      assemble: {
        options: {
          title: 'ListPremier Landing',
          message: 'Assembly Complete. Styles & Scrips are good to go!'
        }
      }
    }
  });

  // ===========================================================================
  // LOAD GRUNT PLUGINS ========================================================
  // ===========================================================================
  // we can only load these if they are in our package.json
  // make sure you have run npm install so our app can find these

  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-contrib-sass');
  grunt.loadNpmTasks('grunt-contrib-cssmin');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-notify');

  // ============= // CREATE TASKS ========== //
  grunt.registerTask('assemble', ['sass', 'concat:styles', 'cssmin', 'notify:stylesheets']);
  grunt.registerTask('dev', ['watch']);

  // configure default log messages -----------------------------------
  grunt.registerTask('default', 'Text Banner', function() {
    var message = '\n' + grunt.config.get('projectName').data + '\n-----------------------------------------------------\nProudly handcrafted by Maronqua <http://maronqua.ro/>\n\nTo get started see the README.md\n\r';
    grunt.log.write(message);
  });

  grunt.event.on('watch', function(action, filepath, target) {
    console.log(target + ': ' + filepath + ' has ' + action);
  });
}
