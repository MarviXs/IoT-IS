import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { FlatCompat } from '@eslint/eslintrc';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const compat = new FlatCompat({
  baseDirectory: __dirname,
});

export default [
  {
    ignores: ['dist', 'src-capacitor', 'src-cordova', '.quasar', 'node_modules', 'src-ssr'],
  },
  ...compat.config({
    parserOptions: {
      parser: '@typescript-eslint/parser',
      extraFileExtensions: ['.vue'],
    },
    env: {
      browser: true,
      es2021: true,
      node: true,
      'vue/setup-compiler-macros': true,
    },
    extends: [
      'plugin:@typescript-eslint/recommended',
      'plugin:vue/vue3-recommended',
      'prettier',
      'plugin:@intlify/vue-i18n/recommended',
    ],
    plugins: ['@typescript-eslint', 'vue'],
    globals: {
      ga: 'readonly',
      cordova: 'readonly',
      __statics: 'readonly',
      __QUASAR_SSR__: 'readonly',
      __QUASAR_SSR_SERVER__: 'readonly',
      __QUASAR_SSR_CLIENT__: 'readonly',
      __QUASAR_SSR_PWA__: 'readonly',
      process: 'readonly',
      Capacitor: 'readonly',
      chrome: 'readonly',
    },
    rules: {
      'prefer-promise-reject-errors': 'off',
      quotes: ['warn', 'single', { avoidEscape: true }],
      '@typescript-eslint/explicit-function-return-type': 'off',
      '@typescript-eslint/no-var-requires': 'off',
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': 'warn',
      'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
      '@intlify/vue-i18n/no-dynamic-keys': 'error',
      '@intlify/vue-i18n/no-unused-keys': [
        'warn',
        {
          extensions: ['.js', '.ts', '.vue'],
        },
      ],
      '@intlify/vue-i18n/no-missing-keys-in-other-locales': ['warn'],
    },
    settings: {
      'vue-i18n': {
        localeDir: 'src/i18n/*.json',
        messageSyntaxVersion: '^9.8.0',
      },
    },
    overrides: [
      {
        files: ['*.json'],
        parser: 'jsonc-eslint-parser',
        rules: {
          quotes: ['warn', 'double'],
        },
      },
    ],
  }),
];
