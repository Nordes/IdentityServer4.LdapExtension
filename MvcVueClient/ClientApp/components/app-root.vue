<template>
<div>
  <div class="spinner display-1" v-if="$store.getters.isLoginPending">
    <icon icon="spinner" pulse/>
  </div>

  <div id="app" class="container-fluid" v-if="!$store.getters.isLoginPending">
    <div class="row">
      <div class="col-md-3">
        <nav-menu params="route: route"></nav-menu>
      </div>
      <div class="col-sm-9">
        <router-view></router-view>
      </div>
    </div>
    
  </div>
</div>
</template>

<script>
  import NavMenu from './nav-menu'

  export default {
    components: {
      'nav-menu': NavMenu
    },

    data () {
      return {}
    },
    created () {
    }
  }
</script>

<style scoped>
.spinner {
  -webkit-transform: translate(-50%,-50%);
  transform: translate(-50%,-50%);
  position: absolute;
  top: 50%;
  left: 50%;
}
</style>
