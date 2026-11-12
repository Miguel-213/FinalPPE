import api from './api'

export const trofeosService = {
  getAll() {
    return api.get('/trofeos')
  },
  getOne(id) {
    return api.get(`/trofeos/${id}`)
  },
  create(data) {
    return api.post('/trofeos', data)
  },
  update(id, data) {
    return api.patch(`/trofeos/${id}`, data)
  },
  remove(id) {
    return api.delete(`/trofeos/${id}`)
  },
}