import { flushPromises, mount } from '@vue/test-utils'
import PacientesList from '../src/views/PacientesList.vue'

const { pacientesApiMock } = vi.hoisted(() => ({
  pacientesApiMock: {
    getAll: vi.fn(),
    delete: vi.fn()
  }
}))

vi.mock('../src/services/api', () => ({
  pacientesApi: pacientesApiMock,
  isPacienteBloqueadoByFecha: paciente => !!paciente?.bloqueadoVigente
}))

describe('PacientesList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    pacientesApiMock.getAll.mockResolvedValue({
      data: [
        {
          id: 1,
          nombreCompleto: 'Paciente Bloqueado',
          dni: '1',
          email: 'a@a.com',
          telefono: '111',
          bloqueadoVigente: true
        },
        {
          id: 2,
          nombreCompleto: 'Paciente Libre',
          dni: '2',
          email: 'b@b.com',
          telefono: '222',
          bloqueadoVigente: false
        }
      ]
    })
  })

  it('renderiza bloqueo basado en bloqueadoVigente de API', async () => {
    const wrapper = mount(PacientesList)
    await flushPromises()

    const rows = wrapper.findAll('tbody tr')
    expect(rows).toHaveLength(2)
    expect(rows[0].text()).toContain('Sí')
    expect(rows[1].text()).toContain('No')
  })
})
