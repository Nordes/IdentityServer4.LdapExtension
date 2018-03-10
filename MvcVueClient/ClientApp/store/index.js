import Vue from 'vue'
import Vuex from 'vuex'
import axios from 'axios'

Vue.use(Vuex)

// TYPES
const MAIN_SET_COUNTER = 'MAIN_SET_COUNTER'
const USER_LOGGED_IN = 'USER_LOGGED_IN'
const USER_LOGGED_IN_CHECK = 'USER_LOGGED_IN_CHECK'
const USER_NOT_LOGGED_IN = 'USER_NOT_LOGGED_IN'

// STATE
const state = {
  counter: 1,
  isLoggedIn: false,
  isLoginPending: false,
  user: {
    first: null,
    last: null,
    name: null
  }
}

// MUTATIONS
const mutations = {
  [MAIN_SET_COUNTER] (state, obj) {
    state.counter = obj.counter
  },
  [USER_NOT_LOGGED_IN] (state) {
    state.isPending = false
    state.isLoginPending = false

    state.user.first = null
    state.user.last = null
    state.user.name = null
  },
  [USER_LOGGED_IN_CHECK] (state) {
    state.isLoggedIn = false
    state.isLoginPending = true
  },
  [USER_LOGGED_IN] (state, payload) {
    state.user.first = payload.find(f => f.type === 'given_name').value
    state.user.last = payload.find(f => f.type === 'family_name').value
    state.user.name = payload.find(f => f.type === 'name').value

    state.isLoggedIn = true
    state.isLoginPending = false
  }
}

// ACTIONS
const actions = ({
  setCounter ({ commit }, obj) {
    commit(MAIN_SET_COUNTER, obj)
  },
  async loginCheck ({ commit }, obj) {
    commit(USER_LOGGED_IN_CHECK)
    // Todo: Keep a check for the login, we should do a call
    // frequently and kick the user out. We already have the router doing that, but
    // I would prefer to kick out in case the person stay idle.
    if (!window.isAuthenticated) {
      // Small hack in Asp.Net. But basically if the user even try, he will get a
      // 401 and then the user will not be logged anyway
      return commit(USER_NOT_LOGGED_IN)
    }
    try {
      var response = await axios.get('api/user/current')
    } catch (e) {
      return commit(USER_NOT_LOGGED_IN)
    }

    if (response.status === 200) {
      commit(USER_LOGGED_IN, response.data)
    } else {
      commit(USER_NOT_LOGGED_IN)
    }
  }
})

const store = new Vuex.Store({
  state,
  mutations,
  actions,
  getters: {
    isLoggedIn: (state) => {
      return state.isLoggedIn
    },
    isLoginPending: (state) => {
      return state.isLoginPending
    },
    user: (state) => {
      return state.user
    }
  }
})

export default store
