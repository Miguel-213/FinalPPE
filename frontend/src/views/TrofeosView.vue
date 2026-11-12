<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { trofeosService } from '../services/trofeosService'
import { removeToken, estaAutenticado } from "../services/authService";

const trofeos = ref([])
const cargando = ref(true)
const trofeoEditando = ref(null)
const router = useRouter()
const errorMsg = ref('')


// Búsqueda
const busqueda = ref('')

// Paginación
const paginaActual = ref(1)
const porPagina = 4

const nuevoTrofeo = ref({
  nombre: '',
  juego: '',
  dificultad: '',
  requisito: '',
  estado: false,
  imagen: '',
})

// Filtra los trofeos por nombre
const trofeosFiltrados = computed(() => {
  return trofeos.value.filter((trofeo) =>
    trofeo.nombre
      .toLowerCase()
      .includes(busqueda.value.toLowerCase()),
  )
})

// Calcula cuántas páginas hay
const totalPaginas = computed(() => {
  return Math.ceil(trofeosFiltrados.value.length / porPagina)
})

// Devuelve solamente los trofeos de la página actual
const trofeosPaginados = computed(() => {
  const inicio = (paginaActual.value - 1) * porPagina
  const fin = inicio + porPagina

  return trofeosFiltrados.value.slice(inicio, fin)
})

function reiniciarPagina() {
  paginaActual.value = 1
}

function paginaAnterior() {
  if (paginaActual.value > 1) {
    paginaActual.value--
  }
}

function paginaSiguiente() {
  if (paginaActual.value < totalPaginas.value) {
    paginaActual.value++
  }
}

async function cargarTrofeos() {
  try {
    const respuesta = await trofeosService.getAll()
    trofeos.value = respuesta.data
  } catch (error) {
    errorMsg.value = error.message || 'Error al cargar los trofeos'
  } finally {
    cargando.value = false
  }
}

async function crearTrofeo() {
  errorMsg.value = ''
  try {
    await trofeosService.create(nuevoTrofeo.value)
    nuevoTrofeo.value = { nombre: '', juego: '', dificultad: '', requisito: '', estado: false, imagen: '' }
    paginaActual.value = 1

    await cargarTrofeos()
  } catch (error) {
    errorMsg.value = error.message || 'No se pudo crear el trofeo. ¿Iniciaste sesión?'
  }
}

async function eliminarTrofeo(id) {
  if (!confirm('¿Seguro que desea eliminar este trofeo?')) return
  errorMsg.value = ''
  try {
    await trofeosService.remove(id)
    paginaActual.value = 1
    await cargarTrofeos()
  } catch (error) {
    errorMsg.value = error.message || 'No se pudo eliminar. ¿Iniciaste sesión?'
  }
}

function editarTrofeo(trofeo) {
  trofeoEditando.value = { ...trofeo }
}

function cancelarEdicion() {
  trofeoEditando.value = null
}

async function guardarEdicion() {
  errorMsg.value = ''
  try {
    await trofeosService.update(trofeoEditando.value.id, {
      nombre: trofeoEditando.value.nombre,
      juego: trofeoEditando.value.juego,
      dificultad: trofeoEditando.value.dificultad,
      requisito: trofeoEditando.value.requisito,
      estado: trofeoEditando.value.estado,
      imagen: trofeoEditando.value.imagen,
    })
    trofeoEditando.value = null
    paginaActual.value = 1
    await cargarTrofeos()
  } catch (error) {
    errorMsg.value = error.message || 'No se pudo editar. ¿Iniciaste sesión?'
  }
}

function logout() {
  removeToken()
  trofeoEditando.value = null
  errorMsg.value = ''
  router.push('/')
}

onMounted(() => {
  cargarTrofeos()
})
</script>
<template>
  <main>
    <div style="display: flex; justify-content: space-between; align-items: center;">
      <h1>Trophy Base</h1>
      <button v-if="estaAutenticado" @click="logout">🚪 Cerrar sesión</button>
    </div>

    <!-- Mensaje de error global -->
    <p v-if="errorMsg" style="color: red; margin-bottom: 12px;">{{ errorMsg }}</p>

    <!-- CREAR TROFEO -->
    <section v-if="estaAutenticado">
      <h2>Agregar trofeo</h2>
      <form @submit.prevent="crearTrofeo">
        <div><label>Nombre:</label><input v-model="nuevoTrofeo.nombre" type="text" required /></div>
        <div><label>Juego:</label><input v-model="nuevoTrofeo.juego" type="text" required /></div>
        <div><label>Dificultad:</label><input v-model="nuevoTrofeo.dificultad" type="text" required /></div>
        <div><label>Requisito:</label><input v-model="nuevoTrofeo.requisito" type="text" required /></div>
        <div><label>URL de imagen:</label><input v-model="nuevoTrofeo.imagen" type="text" required /></div>
        <div><label><input v-model="nuevoTrofeo.estado" type="checkbox" /> Conseguido</label></div>
        <button type="submit">Agregar trofeo</button>
      </form>
    </section>

    <p v-else>
      <RouterLink to="/login">Inicia sesión</RouterLink> para agregar, editar o eliminar trofeos.
    </p>

    <hr />

    <!-- EDITAR TROFEO -->
    <section v-if="estaAutenticado && trofeoEditando">
      <h2>Editar trofeo</h2>
      <form @submit.prevent="guardarEdicion">
        <div><label>Nombre:</label><input v-model="trofeoEditando.nombre" type="text" required /></div>
        <div><label>Juego:</label><input v-model="trofeoEditando.juego" type="text" required /></div>
        <div><label>Dificultad:</label><input v-model="trofeoEditando.dificultad" type="text" required /></div>
        <div><label>Requisito:</label><input v-model="trofeoEditando.requisito" type="text" required /></div>
        <div><label>URL de imagen:</label><input v-model="trofeoEditando.imagen" type="text" required /></div>
        <div><label><input v-model="trofeoEditando.estado" type="checkbox" /> Conseguido</label></div>
        <button type="submit">Guardar cambios</button>
        <button type="button" @click="cancelarEdicion">Cancelar</button>
      </form>
    </section>

    <hr v-if="trofeoEditando" />

    <!-- LISTA -->
    <section>
      <h2>Trofeos</h2>
      <div class="buscador">
        <label>Buscar por nombre:</label>
        <input v-model="busqueda" @input="reiniciarPagina" type="text" placeholder="Buscar trofeo..." />
      </div>

      <p v-if="cargando">Cargando trofeos...</p>
      <p v-else-if="trofeos.length === 0">No hay trofeos registrados.</p>
      <p v-else-if="trofeosFiltrados.length === 0">No se encontraron trofeos con ese nombre.</p>

      <div v-else>
        <div v-for="trofeo in trofeosPaginados" :key="trofeo.id" class="trofeo" :class="{ 'conseguido': trofeo.estado }">
          <h3>{{ trofeo.nombre }}</h3>
          <img :src="trofeo.imagen" :alt="trofeo.nombre" />
          <p><strong>Juego:</strong> {{ trofeo.juego }}</p>
          <p><strong>Dificultad:</strong> {{ trofeo.dificultad }}</p>
          <p><strong>Requisito:</strong> {{ trofeo.requisito }}</p>
          <p><strong>Estado:</strong> {{ trofeo.estado ? 'Conseguido' : 'Pendiente' }}</p>
          
          <template v-if="estaAutenticado">
            <button @click="editarTrofeo(trofeo)">Editar</button>
            <button @click="eliminarTrofeo(trofeo.id)">Eliminar</button>
          </template>

          <hr />
        </div>

        <div v-if="totalPaginas > 1" class="paginacion">
          <button @click="paginaAnterior" :disabled="paginaActual === 1">Anterior</button>
          <span>Página {{ paginaActual }} de {{ totalPaginas }}</span>
          <button @click="paginaSiguiente" :disabled="paginaActual === totalPaginas">Siguiente</button>
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
main {
  max-width: 1100px;
  margin: 40px auto;
  padding: 0 25px 50px;
}

h1 {
  color: #141c2f;
  font-size: 36px;
  margin-bottom: 5px;
}

h2 {
  color: #26334f;
  margin-bottom: 20px;
}

section {
  background: white;
  padding: 25px;
  margin-bottom: 25px;
  border-radius: 12px;

  box-shadow:
    0 2px 8px rgba(0, 0, 0, 0.08);
}

/* Formularios */

form {
  max-width: 650px;
}

form div {
  margin-bottom: 16px;
}

label {
  display: block;
  margin-bottom: 6px;
  font-weight: bold;
  color: #374151;
}

input[type='text'],
input[type='email'],
input[type='password'] {
  width: 100%;
  padding: 11px 12px;

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

/* Botones */

button {
  padding: 10px 17px;
  margin-right: 8px;

  border: none;
  border-radius: 7px;

  background: #26334f;
  color: white;

  font-weight: bold;
  cursor: pointer;
}

button:hover {
  background: #141c2f;
}

button:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

/* Buscador */

.buscador {
  max-width: 500px;
  margin-bottom: 30px;
}

/* Tarjetas */

.trofeo {
  background: #f8f9fb;

  border: 1px solid #e1e4e9;
  border-left: 5px solid #d9a928;

  border-radius: 10px;

  padding: 20px;
  margin-bottom: 20px;
}

.conseguido {
  border-left: 5px solid green;
}

.trofeo h3 {
  color: #141c2f;
  font-size: 23px;
  margin-top: 0;
  margin-bottom: 15px;
}

.trofeo img {
  width: 180px;
  height: 130px;

  object-fit: cover;

  border-radius: 8px;
  margin-bottom: 10px;
}

.trofeo p {
  margin: 8px 0;
  line-height: 1.5;
}

.trofeo hr {
  display: none;
}

/* Paginación */

.paginacion {
  display: flex;
  align-items: center;

  gap: 10px;

  margin-top: 25px;
}

.paginacion span {
  font-weight: bold;
  color: #374151;
}

/* Separadores antiguos */

main > hr {
  display: none;
}

/* Responsive */

@media (max-width: 650px) {
  main {
    margin-top: 20px;
    padding: 0 15px 30px;
  }

  h1 {
    font-size: 29px;
  }

  section {
    padding: 18px;
  }

  .trofeo img {
    width: 100%;
    height: auto;
  }
}
</style>