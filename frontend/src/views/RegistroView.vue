<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '../services/authService'

const router = useRouter()
const email = ref('')
const password = ref('')
const mensaje = ref('')
const error = ref('')

async function registrar() {
  mensaje.value = ''
  error.value = ''

  try {
    await authService.register({
      email: email.value,
      password: password.value,
    })
    mensaje.value = 'Usuario registrado correctamente'
    setTimeout(() => router.push('/login'), 1000)
  } catch (err) {
    error.value = err.response?.data?.message || 'Error al conectar con el servidor'
  }
}
</script>

<template>
  <main>
    <h1>Registrarse</h1>

    <form @submit.prevent="registrar">
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
        Registrarse
      </button>
    </form>

    <p v-if="error" style="color: red">{{ error }}</p>
    <p v-if="mensaje" style="color: green">{{ mensaje }}</p>

    <RouterLink to="/login">
      Ya tengo una cuenta
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