import Vue from 'vue'
import axios from 'axios'
import router from './router/index'
import store from './store'
import { sync } from 'vuex-router-sync'
import App from 'components/app-root'
import { FontAwesomeIcon } from './icons'

require('expose-loader?$!expose-loader?jQuery!jquery') // eslint-disable-line
require('bootstrap')

// Registration of global components
Vue.component('icon', FontAwesomeIcon)

Vue.prototype.$http = axios

// Check if the user is authorized as a user.
store.dispatch('loginCheck')

sync(store, router)

const app = new Vue({
  store,
  router,
  ...App
})

export {
  app,
  router,
  store
}
