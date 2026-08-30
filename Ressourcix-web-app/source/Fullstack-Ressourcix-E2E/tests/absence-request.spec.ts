import { test, expect } from '@playwright/test'

test('Ferienantrag kann erstellt werden', async ({ page }) => {
  await page.goto('/#/login')

  await page
    .getByTestId('login-username')
    .locator('input')
    .fill('admin')

  await page
    .getByTestId('login-password')
    .locator('input')
    .fill('Test.admin1')

  await page
    .getByTestId('login-submit')
    .click()

  await expect(page).toHaveURL(/\/$/)

  await page
    .getByTestId('new-request')
    .click()

  await expect(page).toHaveURL(/\/calender/)

  const employeeRow = page
  .locator('tbody tr')
  .filter({ hasText: 'Admin Test' })

  const firstDayCell = employeeRow
  .locator('td.clickable')
  .first()

  const startDateString = await firstDayCell.getAttribute('data-date')

  if (!startDateString) {
    throw new Error('Startdatum konnte nicht aus der Kalenderzelle gelesen werden.')
  }

  await firstDayCell.click()

  await expect(
    page.getByTestId('request-end-date')
  ).toBeVisible()

  const startDate = new Date(startDateString ?? '')
  startDate.setDate(startDate.getDate() + 7)

  const endDate = startDate.toISOString().split('T')[0]

  await page
    .getByTestId('request-end-date')
    .locator('input')
    .fill(endDate)

  await page
    .getByTestId('calendar-save-entry')
    .click()

  await expect(firstDayCell).not.toHaveText('')
})