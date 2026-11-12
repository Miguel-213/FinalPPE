<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService, setToken } from '../services/authService'

const router = useRouter()
const email = ref('')
const password = ref('')
const error = ref('')

async function iniciarSesion() {
  error.value = ''

  try {
    const respuesta = await authService.login({
      email: email.value,
      password: password.value,
    })
    setToken(respuesta.data.access_token)
    router.push('/')
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor'
  }
}
</script>

<template>
  <main>
    <h1>Iniciar sesión</h1>

    <form @submit.prevent="iniciarSesion">
      <div>
        <label>Correo:</label>

        <input
          v-model="email"
          type="email"
          required
        />
      </div>

      <div>
        <label>Contraseña:</label>

        <input
          v-model="password"
          type="password"
          required
        />
      </div>

      <button type="submit">
        Iniciar sesión
      </button>
    </form>

    <p v-if="error">
      {{ error }}
    </p>

    <RouterLink to="/registro">
      Crear una cuenta
    </RouterLink>
  </main>
</template>

<style scoped>
main {
  width: 90%;
  max-width: 420px;

  margin: 70px auto;
  padding: 35px;

  background: white;
  border-radius: 12px;

  box-shadow:
    0 4px 18px rgba(0, 0, 0, 0.1);
}

h1 {
  margin-top: 0;
  margin-bottom: 28px;

  text-align: center;
  color: #141c2f;
}

form div {
  margin-bottom: 18px;
}

label {
  display: block;

  margin-bottom: 6px;

  font-weight: bold;
  color: #374151;
}

input {
  width: 100%;

  padding: 11px;

  border: 1px solid #cbd0da;
  border-radius: 7px;

  font-size: 15px;
}

input:focus {
  outline: none;

  border-color: #d9a928;

  box-shadow:
    0 0 0 3px rgba(217, 169, 40, 0.15);
}

button {
  width: 100%;

  padding: 11px;

  border: none;
  border-radius: 7px;

  background: #d9a928;
  color: #141c2f;

  font-size: 15px;
  font-weight: bold;

  cursor: pointer;

  margin-bottom: 20px;
}

button:hover {
  background: #f4c542;
}

p {
  color: #b91c1c;
}

a {
  display: block;

  text-align: center;

  color: #26334f;
  font-weight: bold;
}
</style>