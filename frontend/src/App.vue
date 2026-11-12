<script setup>
  import { estaAutenticado } from "./services/authService";
</script>

<template>
  <header class="barra">
    <nav class="navegacion">
      <RouterLink class="logo" to="/">
        🏆 Trophy Base
      </RouterLink>

      <div class="enlaces" v-if="!estaAutenticado">
        <RouterLink to="/">
          Trofeos
        </RouterLink>

        <RouterLink class="boton-sesion" to="/login" >
        Iniciar sesión
      </RouterLink>

      <RouterLink to="/registro">
        Registrarse
      </RouterLink>
      
      </div>
    </nav>
  </header>

  <RouterView />
</template>

<style>
* {
  box-sizing: border-box;
}

body {
  margin: 0;
  background: #f3f4f7;
  color: #1f2937;
  font-family: Arial, Helvetica, sans-serif;
}

a {
  text-decoration: none;
}
</style>

<style scoped>
.barra {
  background: #141c2f;
  border-bottom: 3px solid #d9a928;
}

.navegacion {
  max-width: 1100px;
  margin: auto;
  padding: 16px 24px;

  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo {
  color: white;
  font-size: 21px;
  font-weight: bold;
}

.enlaces {
  display: flex;
  align-items: center;
  gap: 20px;
}

.enlaces a {
  color: #e5e7eb;
  font-weight: 500;
}

.enlaces a:hover {
  color: #f4c542;
}

.boton-sesion {
  border: none;
  background: #308c91;
  font-weight: bold;
  padding: 9px 14px;
  border-radius: 7px;
  cursor: pointer;
}

.boton-sesion:hover {
  background: rgb(91, 100, 89);
}

@media (max-width: 650px) {
  .navegacion {
    flex-direction: column;
    gap: 15px;
  }

  .enlaces {
    flex-wrap: wrap;
    justify-content: center;
  }
}
</style>