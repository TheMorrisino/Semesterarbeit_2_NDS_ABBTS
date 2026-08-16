import vuetify from 'eslint-config-vuetify'

export default vuetify(
  { ts: true },
  {
    rules: {
      '@stylistic/quotes': ['error', 'single'],
    },
  },
  {
    // Pinia-Stores nutzen bewusst die Options-API (defineStore mit `actions`), wo `this` von
    // Pinia an den Store gebunden wird - kein Klassenkontext, aber ein gültiges Pattern.
    files: ['src/stores/**/*.ts'],
    rules: {
      'unicorn/no-this-outside-of-class': 'off',
    },
  },
)
