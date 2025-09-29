/* global __dirname */

const path = require('path');
const ESLintPlugin = require('eslint-webpack-plugin');
const HtmlWebPackPlugin = require("html-webpack-plugin");

module.exports = {
  entry: './src/index.jsx',
  output: {
    filename: 'main.js',
    path: path.resolve(__dirname, 'build')
  },
  module: {

    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules|vendor/,
        use: ['babel-loader']
      },{
        test: /\.html$/,
        use: [
          {
            loader: "html-loader"
          }
        ]
      },
      {
        test: /\.(css)$/,
        use: [
          {
            loader: "style-loader"
          },
          {
            loader: "css-loader",
            options: {
              esModule: false,
              sourceMap: false,
              modules: false,
            }
          }
        ]
      },
      { test: /\.react\.(svg)$/,
        use: [
          { loader: "babel-loader" },
          {
            loader: "react-svg-loader",
            options: {
              svgo: {
                plugins: [
                  { removeViewBox: false },
                ],
              },
            }
          }
        ]
      },
      {
        test: /\.(png|jpg|jpeg|gif)$/,
        type: 'asset/resource'
      }
    ]
  },
  plugins: [
    new HtmlWebPackPlugin({
      template: __dirname + "/src/index.html",
      filename: "./index.html"
    }),
    new ESLintPlugin(),
  ],
  resolve: {
    extensions: ['*', '.js', '.jsx'],
    alias: {
      'vendor': path.resolve( __dirname, './vendor/'),
      'src': path.resolve( __dirname, './src/'),
      'node_modules': path.resolve( __dirname, './node_modules'),
      'react': path.resolve( __dirname, './node_modules/react'),
    }
  },
  devServer: {
    static: './public'
  },
  devtool: 'inline-source-map',
};