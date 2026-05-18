import axios from 'axios'

const BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')

export const turnosApi = {
  getAll:           ()          => axios.get(`${BASE_URL}/Turnos`),
  getById:          (id)        => axios.get(`${BASE_URL}/Turnos/${id}`),
  create:           (data)      => axios.post(`${BASE_URL}/Turnos`, data),
  cancelar:         (id)        => axios.put(`${BASE_URL}/Turnos/${id}/cancelar`),
  marcarAusencia:   (id)        => axios.post(`${BASE_URL}/Turnos/${id}/ausencia`),
  actualizarEstado: (id, data)  => axios.put(`${BASE_URL}/Turnos/${id}/estado`, data)
}

export const pacientesApi = {
  getAll:  ()          => axios.get(`${BASE_URL}/Pacientes`),
  getById: (id)        => axios.get(`${BASE_URL}/Pacientes/${id}`),
  create:  (data)      => axios.post(`${BASE_URL}/Pacientes`, data),
  update:  (id, data)  => axios.put(`${BASE_URL}/Pacientes/${id}`, data),
  delete:  (id)        => axios.delete(`${BASE_URL}/Pacientes/${id}`)
}

export const medicosApi = {
  getAll: () => axios.get(`${BASE_URL}/Medicos`)
}

export const sucursalesApi = {
  getAll: () => axios.get(`${BASE_URL}/Sucursales`)
}
