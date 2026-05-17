const STORAGE_KEY = 'ksb_employee_signature'
const ADMIN_STORAGE_KEY = 'ksb_admin_signature'

const getStorageKey = role => (role === 'admin' ? ADMIN_STORAGE_KEY : STORAGE_KEY)

export const getSignatureProfile = (role = 'employee') => {
  if (typeof window === 'undefined') {
    return null
  }

  try {
    const raw = window.localStorage.getItem(getStorageKey(role))
    return raw ? JSON.parse(raw) : null
  } catch (error) {
    console.error('Khong the doc chu ky da luu:', error)
    return null
  }
}

export const saveSignatureProfile = (profile, role = 'employee') => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(getStorageKey(role), JSON.stringify(profile))
}

export const clearSignatureProfile = (role = 'employee') => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(getStorageKey(role))
}

export { ADMIN_STORAGE_KEY, STORAGE_KEY }
