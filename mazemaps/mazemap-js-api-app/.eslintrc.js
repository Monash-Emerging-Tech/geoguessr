module.exports = {
    "parser": "babel-eslint",
    "env": {
        "browser": true,
        "commonjs": true,
        "es6": true,
    },
    "plugins": [
        "flowtype",
    ],
    "globals": {
    },
    "extends": [
        "eslint:recommended",
        "plugin:react/recommended",
        "plugin:flow/recommended",
    ],
    "parserOptions": {
        "sourceType": "module"
    },
    "rules": {
        "no-case-declarations": "warn",
        "no-unused-vars": "warn",
        "no-console": "error"
    },
    "root": true
};