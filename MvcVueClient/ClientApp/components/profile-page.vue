<template>
  <div>
    <button class="btn btn-info" @click="displayFullClaims = !displayFullClaims">Get all claims</button>
    <button class="btn btn-info" @click="displayVueX = !displayVueX">{{displayVueX ? 'Hide' : 'Show'}} current user details (VueX)</button><br>
    <br>
    <div class="card" v-if="displayVueX">
      <div class="card-header bg-success text-white">
        Current user information (VueX)
      </div>
      <div class="card-body">
        <pre>{{$store.getters.user}}</pre>
      </div>
    </div>
    <div class="card" v-if="displayFullClaims">
      <div class="card-header bg-success text-white">
        Current claims
      </div>
      <div class="card-body">
        <pre>{{this.claims}}</pre>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  watch: {
    displayFullClaims: function (newValue, previousValue) {
      if (newValue) {
        this.fetchClaims()
      }
    }
  },
  data () {
    return {
      displayVueX: false,
      displayFullClaims: false,
      claims: null
    }
  },
  methods: {
    fetchClaims: async function () {
      this.displayApi1Result = true // Display probably a spinner...
      this.claims = (await this.$http.get('/api/user/current')).data
    }
  }
}
</script>
