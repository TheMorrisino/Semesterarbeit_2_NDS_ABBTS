import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests',

  timeout: 5000,

  expect: {
    timeout: 5000,
  },

  fullyParallel: false,

  forbidOnly: false,

  retries: 0,

  workers: 1,

  reporter: 'html',

  use: {
    baseURL: 'http://localhost:5167',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
      },
    },
  ],
})