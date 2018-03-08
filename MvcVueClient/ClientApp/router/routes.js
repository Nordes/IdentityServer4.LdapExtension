import CounterExample from 'components/counter-example'
import FetchData from 'components/fetch-data'
import HomePage from 'components/home-page'
import ProfilePage from 'components/profile-page'

export const routes = [
  {
    path: '/',
    name: 'home',
    component: HomePage,
    meta: {
      display: 'Home',
      icon: 'home'
    }
  },
  {
    path: '/counter',
    name: 'counter',
    component: CounterExample,
    meta: {
      display: 'Counter',
      icon: 'graduation-cap'
    }
  },
  {
    name: 'fetch-data',
    path: '/fetch-data',
    component: FetchData,
    meta: {
      display: 'Fetch data',
      icon: 'list'
    }
  },
  {
    name: 'profile',
    path: '/profile',
    component: ProfilePage,
    meta: {
      display: 'Profile',
      icon: 'user-circle',
      requiresAuth: true
      // You could also add some other properties.
      // Cref: https://router.vuejs.org/en/advanced/meta.html
    }
  }
]
