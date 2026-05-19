import axios from 'axios'

const BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')
const parsedBloqueoDias = Number.parseInt(import.meta.env.VITE_NOSHOW_BLOQUEO_DIAS || '30', 10)
const BLOQUEO_DIAS = Number.isNaN(parsedBloqueoDias) || parsedBloqueoDias < 0 ? 30 : parsedBloqueoDias

export function getApiErrorMessage(error, fallback = 'Error al procesar la solicitud')
{
  const data = error?.response?.data
  return data?.mensaje || data?.message || fallback
}

export function isPacienteBloqueadoByFecha(paciente) {
  if (!paciente?.fechaBloqueo) return false

  const fechaBloqueo = new Date(paciente.fechaBloqueo)
  if (Number.isNaN(fechaBloqueo.getTime())) return false

  const bloqueoHastaMs = fechaBloqueo.getTime() + BLOQUEO_DIAS * 24 * 60 * 60 * 1000
  return bloqueoHastaMs > Date.now()
}

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
