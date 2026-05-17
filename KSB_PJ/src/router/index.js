import { createRouter, createWebHistory } from 'vue-router'
import { getCurrentUser, isAuthenticated } from '@/utils/authStore'
import LoginPage from '../components/LoginPage.vue'

const EmployeeDashboard = () => import('../components/EmployeeDashboard.vue')
const AdminDashboard = () => import('../components/AdminDashboard.vue')
const FormCoSoHaTang = () => import('../components/forms/FormCoSoHaTang.vue')
const FormHoSo = () => import('../components/forms/FormHoSo.vue')
const FormVeSinhATTP = () => import('../components/forms/FormVeSinhATTP.vue')
const FormSuatAnNguoiBenh = () => import('../components/forms/FormSuatAnNguoiBenh.vue')
const SignaturePadPage = () => import('../components/SignaturePadPage.vue')

const routes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginPage
  },
  {
    path: '/employee',
    component: EmployeeDashboard,
    children: [
      {
        path: '',
        name: 'EmployeeHome',
        component: () => import('../components/EmployeeHome.vue')
      },
      {
        path: 'bb-csht',
        name: 'BBCoSoHaTang',
        component: FormCoSoHaTang
      },
      {
        path: 'bb-hoso',
        name: 'BBHoSo',
        component: FormHoSo
      },
      {
        path: 'bb-vsattp',
        name: 'BBVeSinhATTP',
        component: FormVeSinhATTP
      },
      {
        path: 'bb-suatan',
        name: 'BBSuatAn',
        component: FormSuatAnNguoiBenh
      },
      {
        path: 'chu-ky',
        name: 'EmployeeSignature',
        component: SignaturePadPage
      }
    ]
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: AdminDashboard
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

const DYNAMIC_IMPORT_RELOAD_KEY = 'ksb_dynamic_import_reload'
const isDynamicImportError = error => {
  const message = String(error?.message || error || '')
  return /Failed to fetch dynamically imported module|Importing a module script failed|error loading dynamically imported module/i.test(message)
}

router.beforeEach((to, from, next) => {
  const loggedIn = isAuthenticated()
  const currentUser = getCurrentUser()

  if (to.path !== '/login' && !loggedIn) {
    next('/login')
    return
  }

  if (to.path === '/admin' && currentUser?.role !== 'Admin') {
    next('/employee')
    return
  }

  if (to.path.startsWith('/employee') && currentUser?.role === 'Admin') {
    next('/admin')
    return
  }

  if (to.path === '/login' && loggedIn) {
    next(currentUser?.role === 'Admin' ? '/admin' : '/employee')
    return
  }

  next()
})

router.afterEach(() => {
  window.sessionStorage.removeItem(DYNAMIC_IMPORT_RELOAD_KEY)
})

router.onError(error => {
  if (!isDynamicImportError(error)) {
    return
  }

  if (window.sessionStorage.getItem(DYNAMIC_IMPORT_RELOAD_KEY) === '1') {
    return
  }

  window.sessionStorage.setItem(DYNAMIC_IMPORT_RELOAD_KEY, '1')
  window.location.reload()
})

export default router
