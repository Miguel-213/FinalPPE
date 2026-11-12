import { test, expect } from '@playwright/test'

const webUrl = 'http://localhost:5173'
const apiUrl = process.env.VITE_API_URL || 'http://localhost:5013'

test('registro, sesión, CRUD, búsqueda y paginación', async ({ page }, testInfo) => {
  test.setTimeout(60000)
  const prefix = `PPE-${Date.now()}`
  const email = `${prefix}@example.com`
  const password = 'Prueba123456'
  const createdIds = []
  const errors = []
  let token
  page.on('pageerror', error => errors.push(error.message))

  async function credentials(pass = password) {
    await page.locator('input[type=email]').fill(email)
    await page.locator('input[type=password]').fill(pass)
  }

  async function submit(endpoint, action) {
    const [response] = await Promise.all([
      page.waitForResponse(r => r.url() === `${apiUrl}${endpoint}` && r.request().method() !== 'OPTIONS'),
      action(),
    ])
    return response
  }

  async function fillTrophy(form, name) {
    const inputs = form.locator('input[type=text]')
    for (const [i, value] of [name, 'Minecraft', 'Fácil', 'Completar misión', `${webUrl}/favicon.ico`].entries()) {
      await inputs.nth(i).fill(value)
    }
  }

  try {
    await page.goto(webUrl)
    await expect(page.locator('form')).toHaveCount(0)
    await expect(page.getByText('Cargando trofeos...')).not.toBeVisible()
    await page.getByRole('link', { name: 'Registrarse', exact: true }).click()
    await credentials()
    const registration = await submit('/auth/register', () => page.getByRole('button', { name: 'Registrarse', exact: true }).click())
    expect(registration.status()).toBe(201)
    await expect(page.getByText('Usuario registrado correctamente')).toBeVisible()
    await expect(page).toHaveURL(`${webUrl}/login`)

    await page.getByRole('link', { name: 'Registrarse', exact: true }).click()
    await credentials()
    const duplicate = await submit('/auth/register', () => page.getByRole('button', { name: 'Registrarse', exact: true }).click())
    expect(duplicate.status()).toBe(409)
    await expect(page.getByText('El email ya está registrado')).toBeVisible()
    await page.getByRole('link', { name: 'Iniciar sesión', exact: true }).click()
    await credentials('incorrecta')
    const badLogin = await submit('/auth/login', () => page.getByRole('button', { name: 'Iniciar sesión', exact: true }).click())
    expect(badLogin.status()).toBe(401)
    await expect(page.getByText('Credenciales inválidas')).toBeVisible()
    await credentials()
    const login = await submit('/auth/login', () => page.getByRole('button', { name: 'Iniciar sesión', exact: true }).click())
    expect(login.status()).toBe(201)
    token = (await login.json()).access_token
    await expect(page).toHaveURL(`${webUrl}/`)
    await expect(page.locator('form')).toHaveCount(1)
    expect(await page.evaluate(() => localStorage.getItem('token'))).toBe(token)
    await page.reload()
    await expect(page.locator('form')).toHaveCount(1)
    await expect(page.getByText('Cargando trofeos...')).not.toBeVisible()

    const search = page.getByPlaceholder('Buscar trofeo...')
    await search.fill(prefix)
    for (let n = 1; n <= 5; n++) {
      const form = page.locator('form').first()
      await fillTrophy(form, `${prefix} ${n}`)
      const response = await submit('/trofeos', () => form.getByRole('button', { name: 'Agregar trofeo', exact: true }).click())
      expect(response.status()).toBe(201)
      expect(response.request().headers().authorization).toBe(`Bearer ${token}`)
      createdIds.push((await response.json()).id)
      await expect(form.locator('input[type=text]').first()).toHaveValue('')
      await expect(page.locator('.trofeo')).toHaveCount(Math.min(n, 4))
    }
    await expect(page.getByText('Página 1 de 2')).toBeVisible()
    await page.getByRole('button', { name: 'Siguiente', exact: true }).click()
    await expect(page.getByText('Página 2 de 2')).toBeVisible()
    await expect(page.locator('.trofeo')).toHaveCount(1)
    await search.fill(`${prefix.toLowerCase()} 3`)
    await expect(page.locator('.trofeo')).toHaveCount(1)
    await expect(page.locator('.trofeo h3')).toHaveText(`${prefix} 3`)
    await search.fill(`${prefix}-sin-resultados`)
    await expect(page.getByText('No se encontraron trofeos con ese nombre.')).toBeVisible()
    await search.fill(prefix)
    await page.locator('.trofeo').first().getByRole('button', { name: 'Editar', exact: true }).click()
    const edit = page.locator('form').nth(1)
    await edit.locator('input[type=text]').first().fill(`${prefix} editado`)
    await edit.locator('input[type=checkbox]').check()
    const patch = await submit(`/trofeos/${createdIds[0]}`, () => edit.getByRole('button', { name: 'Guardar cambios', exact: true }).click())
    expect(patch.status()).toBe(200)
    await expect(page.locator('.trofeo').first()).toHaveClass(/conseguido/)
    await expect(page.locator('.trofeo h3').first()).toHaveText(`${prefix} editado`)
    await page.screenshot({ path: testInfo.outputPath('crud.png'), fullPage: true })

    page.once('dialog', dialog => dialog.dismiss())
    await page.locator('.trofeo').first().getByRole('button', { name: 'Eliminar', exact: true }).click()
    await expect(page.locator('.trofeo h3').first()).toHaveText(`${prefix} editado`)
    page.once('dialog', dialog => dialog.accept())
    const deleted = await submit(`/trofeos/${createdIds[0]}`, () => page.locator('.trofeo').first().getByRole('button', { name: 'Eliminar', exact: true }).click())
    expect(deleted.status()).toBe(200)
    await expect(page.getByRole('heading', { name: `${prefix} editado`, exact: true })).not.toBeVisible()
    await expect(page.locator('.trofeo')).toHaveCount(4)
    await expect(page.getByText('Página 1 de 2')).not.toBeVisible()

    await page.getByRole('button', { name: /Cerrar sesión/ }).click()
    expect(await page.evaluate(() => localStorage.getItem('token'))).toBeNull()
    await expect(page.locator('form')).toHaveCount(0)
    await expect(page.getByRole('button', { name: 'Editar', exact: true })).toHaveCount(0)
    await page.getByRole('link', { name: 'Iniciar sesión', exact: true }).click()
    await credentials()
    await submit('/auth/login', () => page.getByRole('button', { name: 'Iniciar sesión', exact: true }).click())
    await expect(page.locator('form')).toHaveCount(1)
    await page.evaluate(() => localStorage.setItem('token', 'token-invalido'))
    const form = page.locator('form').first()
    await fillTrophy(form, `${prefix} rechazado`)
    const rejected = await submit('/trofeos', () => form.getByRole('button', { name: 'Agregar trofeo', exact: true }).click())
    expect(rejected.status()).toBe(401)
    await expect(page).toHaveURL(`${webUrl}/login`)
    expect(await page.evaluate(() => localStorage.getItem('token'))).toBeNull()
    await page.getByRole('link', { name: 'Trofeos', exact: true }).click()
    await expect(page.locator('form')).toHaveCount(0)
    expect(errors).toEqual([])
  } finally {
    if (token) {
      for (const id of createdIds) {
        await page.request.delete(`${apiUrl}/trofeos/${id}`, { headers: { Authorization: `Bearer ${token}` } })
      }
    }
  }
})
