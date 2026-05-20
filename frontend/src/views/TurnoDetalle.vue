<template>
  <div>
    <router-link to="/turnos" style="font-size:14px; color:#1a73e8">← Volver a turnos</router-link>
    <div v-if="turno" class="card" style="margin-top: 20px; max-width: 560px">
      <h2>Turno #{{ turno.id }}</h2>
      <div class="detail-row"><span class="label">Paciente</span><span>{{ turno.paciente?.nombreCompleto }}</span></div>
      <div class="detail-row"><span class="label">DNI</span><span>{{ turno.paciente?.dni }}</span></div>
      <div class="detail-row"><span class="label">Médico</span><span>{{ turno.medico?.nombreCompleto }}</span></div>
      <div class="detail-row"><span class="label">Especialidad</span><span>{{ turno.medico?.especialidad }}</span></div>
      <div class="detail-row"><span class="label">Fecha y hora</span><span>{{ formatFecha(turno.fechaHora) }}</span></div>
      <div class="detail-row"><span class="label">Estado</span><span>{{ turno.estado }}</span></div>
      <div class="detail-row"><span class="label">Motivo</span><span>{{ turno.motivo }}</span></div>

      <div style="margin-top: 24px">
        <div class="form-group">
          <label>Cambiar estado (endpoint genérico)</label>
          <select v-model="nuevoEstado">
            <option v-for="e in estadosDisponibles" :key="e" :value="e">{{ e }}</option>
          </select>
        </div>
        <button @click="cambiarEstado" style="margin-bottom: 16px">Actualizar estado</button>
      </div>

      <div style="display: flex; gap: 10px">
        <button v-if="puedeAtender" @click="atender">Marcar atendido</button>
        <button class="btn-danger" @click="cancelar">Cancelar turno</button>
        <button @click="marcarAusencia">Marcar ausencia</button>
      </div>
    </div>
    <p v-else>Cargando...</p>
  </div>
</template>

<script>
import { getApiErrorMessage, turnosApi } from '../services/api'

export default {
  name: 'TurnoDetalle',
  data() {
    return {
      turno: null,
      nuevoEstado: 'Pendiente'
    }
  },
  computed: {
    estadosDisponibles() {
      if (!this.turno) return []

      if (this.turno.estado === 'Pendiente') return ['Pendiente', 'Confirmado']
      if (this.turno.estado === 'Confirmado') return ['Confirmado']
      return [this.turno.estado]
    },
    puedeAtender() {
      return this.turno && (this.turno.estado === 'Pendiente' || this.turno.estado === 'Confirmado')
    }
  },
  async mounted() {
    await this.cargarTurno()
  },
  methods: {
    async cargarTurno() {
      try {
        const res = await turnosApi.getById(this.$route.params.id)
        this.turno = res.data
        this.nuevoEstado = this.turno.estado
      } catch (error) {
        alert(getApiErrorMessage(error))
      }
    },
    formatFecha(fecha) {
      return new Date(fecha).toLocaleString('es-AR')
    },
    async cambiarEstado() {
      try {
        await turnosApi.actualizarEstado(this.turno.id, { estado: this.nuevoEstado })
        await this.cargarTurno()
      } catch (error) {
        alert(getApiErrorMessage(error))
      }
    },
    async atender() {
      try {
        await turnosApi.atender(this.turno.id)
        await this.cargarTurno()
      } catch (error) {
        alert(getApiErrorMessage(error, 'No se pudo marcar el turno como atendido.'))
      }
    },
    async cancelar() {
      try {
        await turnosApi.cancelar(this.turno.id)
        await this.cargarTurno()
      } catch (error) {
        alert(getApiErrorMessage(error, 'No se pudo cancelar el turno.'))
      }
    },
    async marcarAusencia() {
      try {
        await turnosApi.marcarAusencia(this.turno.id)
        await this.cargarTurno()
      } catch (error) {
        alert(getApiErrorMessage(error, 'No se pudo marcar la ausencia.'))
      }
    }
  }
}
</script>

<style scoped>
.detail-row {
  display: flex;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
  font-size: 14px;
}
.label {
  width: 130px;
  font-weight: 600;
  color: #666;
  flex-shrink: 0;
}
</style>
