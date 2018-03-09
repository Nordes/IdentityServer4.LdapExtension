import Vue from 'vue'
import VueRouter from 'vue-router'
import { routes } from './routes'
import Store from '../store/index'

Vue.use(VueRouter)

const router = new VueRouter({
  mode: 'history',
  routes
})

router.beforeResolve((to, from, next) => {
  function checkAuth () {
    if (Store.getters.isLoginPending) {
      setTimeout(checkAuth, 100)
    } else {
      if (to.matched.some(record => record.meta.requiresAuth)) {
        // // Get the auth details
        if (!Store.getters.isLoggedIn) {
          window.location = `/login?redirect=${to.fullPath}`
        } else {
          next()
        }
      } else {
        next() // make sure to always call next()!
      }
    }
  }

  checkAuth()
})

export default router
