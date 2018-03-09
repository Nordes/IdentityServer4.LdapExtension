<template>
    <div class="main-nav">
        <nav class="navbar navbar-expand-md navbar-dark bg-dark">
            <button class="navbar-toggler" type="button" @click="toggleCollapsed">
                <span class="navbar-toggler-icon"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
            </button>
            <router-link class="navbar-brand" to="/">
              <icon :icon="['fab', 'microsoft']"/> ASP.NET Core with Vue.js 2
            </router-link>
            <h6 class="text-white" v-if="isLoggedIn">
              <small>
                {{user.name}}
                <a class="text-white pl-3" href="/logout"><icon icon="sign-in-alt" flip="horizontal" class="mr-2" /><span>Sign-Out</span></a>
              </small>
            </h6>
            <transition name="slide">
                <div :class="'collapse navbar-collapse' + (!collapsed ? ' show':'')" v-show="!collapsed">
                    <ul class="navbar-nav mr-auto">
                        <li class="nav-item" v-for="(route, index) in availableRoutes" :key="index">
                            <router-link :to="route.path" exact-active-class="active">
                                <icon :icon="route.meta.icon" class="mr-2" /><span>{{ route.meta.display }}</span> 
                            </router-link>
                        </li>
                        <li class="nav-item" v-if="!isLoggedIn">
                            <a href="/login"><icon icon="sign-in-alt" class="mr-2" /><span>Sign-In</span></a>
                        </li>
                    </ul>
                </div>
            </transition>
        </nav>
    </div>
</template>

<script>
import { routes } from "../router/routes";
import { mapGetters } from 'vuex'

export default {
  computed: {
    ...mapGetters({
      isLoggedIn: 'isLoggedIn',
      user: 'user'
    }),

    availableRoutes: function () {
      // If not authenticated.
      console.log(this.routes.filter(f=> !f.meta.requiresAuth))
      return this.routes.filter(f=> this.isLoggedIn || !f.meta.requiresAuth)
    }
  },
  data() {
    return {
      routes,
      collapsed: true
    };
  },
  methods: {
    toggleCollapsed: function(event) {
      this.collapsed = !this.collapsed;
    }    
  }
};
</script>

<style scoped>
.slide-enter-active,
.slide-leave-active {
  transition: max-height 0.35s;
}
.slide-enter,
.slide-leave-to {
  max-height: 0px;
}

.slide-enter-to,
.slide-leave {
  max-height: 20em;
}
</style>