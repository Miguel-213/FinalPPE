import axios from 'axios'
import router from '../router/index.js'
import { removeToken } from './authService'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  (response) => response,

  (error) => {
    if (error.response?.status === 401 && !error.config.url.startsWith('/auth/')) {
      removeToken()
      router.push('/login')

      return Promise.reject(
        new Error('Sesión expirada. Por favor inicia sesión.'),
      )
    }

    return Promise.reject(error)
  },
)

export default api