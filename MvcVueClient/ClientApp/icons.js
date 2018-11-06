import { library } from '@fortawesome/fontawesome-svg-core'
// Official documentation available at: https://github.com/FortAwesome/vue-fontawesome
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// If not present, it won't be visible within the application. Considering that you
// don't want all the icons for no reason. This is a good way to avoid importing too many
// unnecessary things.
import { faEnvelope, faGraduationCap, faHome, faList, faSpinner, faSignInAlt, faUserCircle } from '@fortawesome/free-solid-svg-icons'
import { faFontAwesome, faMicrosoft, faVuejs } from '@fortawesome/free-brands-svg-icons'

library.add(
  faEnvelope,
  faGraduationCap,
  faHome,
  faList,
  faSpinner,
  faSignInAlt,
  faUserCircle,
  // Brands
  faFontAwesome,
  faMicrosoft,
  faVuejs
)

export {
  FontAwesomeIcon
}
