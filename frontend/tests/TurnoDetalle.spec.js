import { flushPromises, mount } from '@vue/test-utils'
import TurnoDetalle from '../src/views/TurnoDetalle.vue'

const { turnosApiMock } = vi.hoisted(() => ({
  turnosApiMock: {
    getById: vi.fn(),
    actualizarEstado: vi.fn(),
    atender: vi.fn(),
    cancelar: vi.fn(),
    marcarAusencia: vi.fn()
  }
}))

vi.mock('../src/services/api', () => ({
  turnosApi: turnosApiMock,
  getApiErrorMessage: (error, fallback = 'Error al procesar la solicitud') =>
    error?.response?.data?.mensaje || error?.response?.data?.message || fallback
}))

function mountView() {
  return mount(TurnoDetalle, {
    global: {
      stubs: {
        'router-link': true
      },
      mocks: {
        $route: { params: { id: 1 } }
      }
    }
  })
}

describe('TurnoDetalle', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    global.alert = vi.fn()
    turnosApiMock.getById.mockResolvedValue({
      data: {
        id: 1,
        estado: 'Pendiente',
        fechaHora: '2026-05-19T12:00:00Z',
        motivo: 'control',
        paciente: { nombreCompleto: 'Paciente Test', dni: '123' },
        medico: { nombreCompleto: 'Medico Test', especialidad: 'Clinica' }
      }
    })
    turnosApiMock.atender.mockResolvedValue({})
    turnosApiMock.actualizarEstado.mockResolvedValue({})
    turnosApiMock.cancelar.mockResolvedValue({})
    turnosApiMock.marcarAusencia.mockResolvedValue({})
  })

  it('no ofrece estados sensibles en selector genérico', async () => {
    const wrapper = mountView()
    await flushPromises()

    const options = wrapper.findAll('option').map(o => o.text())
    expect(options).toEqual(['Pendiente', 'Confirmado'])
    expect(options).not.toContain('Atendido')
    expect(options).not.toContain('Cancelado')
    expect(options).not.toContain('NoShow')
  })

  it('usa endpoint dedicado para atender', async () => {
    const wrapper = mountView()
    await flushPromises()

    const atenderButton = wrapper.findAll('button').find(b => b.text().includes('Marcar atendido'))
    expect(atenderButton).toBeTruthy()
    await atenderButton.trigger('click')
    await flushPromises()

    expect(turnosApiMock.atender).toHaveBeenCalledWith(1)
  })

  it('muestra mensaje de negocio al fallar cancelación', async () => {
    turnosApiMock.cancelar.mockRejectedValue({
      response: { data: { mensaje: 'No se puede cancelar un turno ya vencido.' } }
    })

    const wrapper = mountView()
    await flushPromises()

    const cancelButton = wrapper.findAll('button').find(b => b.text().includes('Cancelar turno'))
    await cancelButton.trigger('click')
    await flushPromises()

    expect(global.alert).toHaveBeenCalledWith('No se puede cancelar un turno ya vencido.')
  })
})
