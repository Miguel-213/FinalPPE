import { test, expect } from '@playwright/test'

const apiUrl = process.env.VITE_API_URL || 'http://localhost:5013'

test('Scalar envía Bearer y rechaza escrituras sin token', async ({ page }) => {
  const credentials = { email: `scalar-${Date.now()}@example.com`, password: 'Prueba123456' }
  expect((await page.request.post(`${apiUrl}/auth/register`, { data: credentials })).status()).toBe(201)
  const login = await page.request.post(`${apiUrl}/auth/login`, { data: credentials })
  expect(login.status()).toBe(201)
  const token = (await login.json()).access_token
  let createdId

  const body = JSON.stringify({ nombre: 'Prueba Scalar', juego: 'Minecraft', dificultad: 'Fácil',
    requisito: 'Probar documentación', estado: false, imagen: 'http://localhost:5173/favicon.ico' })

  async function send() {
    const [response] = await Promise.all([
      page.waitForResponse(r => r.url() === `${apiUrl}/trofeos` && r.request().method() === 'POST'),
      page.getByRole('button', { name: /^Send post request/ }).click(),
    ])
    return response
  }

  try {
    await page.goto(`${apiUrl}/scalar/v1`)
    await page.getByText('Select Auth Type', { exact: true }).click()
    await page.getByText('Bearer', { exact: true }).click()
    await page.getByPlaceholder('Token', { exact: true }).fill(token)
    await page.getByRole('button', { name: 'Open Group - Trofeos', exact: true }).click()
    await page.locator('a[href="#tag/trofeos/POST/trofeos"]').first().click()
    await page.getByRole('button', { name: /Test Request.*post.*trofeos/ }).click()
    await page.locator('.cm-content[contenteditable=true]').fill(body)
    const created = await send()
    expect(created.status()).toBe(201)
    expect(created.request().headers().authorization).toBe(`Bearer ${token}`)
    createdId = (await created.json()).id

    await page.getByRole('button', { name: 'Close Client', exact: true }).click()
    await page.getByRole('region', { name: 'Introduction', exact: true }).getByPlaceholder('Token', { exact: true }).fill('')
    await page.getByRole('button', { name: /Test Request.*post.*trofeos/ }).click()
    await page.locator('.cm-content[contenteditable=true]').fill(body)
    expect((await send()).status()).toBe(401)
  } finally {
    if (createdId) {
      await page.request.delete(`${apiUrl}/trofeos/${createdId}`, { headers: { Authorization: `Bearer ${token}` } })
    }
  }
})
