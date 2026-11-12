import api from './api'
import { ref } from 'vue'

export const estaAutenticado = ref(!!localStorage.getItem('token'))

export const setToken = (token) => {
  localStorage.setItem('token', token)
  estaAutenticado.value = true
}

export const removeToken = () => {
  localStorage.removeItem('token')
  estaAutenticado.value = false
}

export const authService = {
  register(data) {
    return api.post('/auth/register', data)
  },
  login(data) {
    return api.post('/auth/login', data)
  },
}
