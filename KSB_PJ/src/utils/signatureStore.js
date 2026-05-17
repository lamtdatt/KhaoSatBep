const STORAGE_KEY = 'ksb_employee_signature'
const ADMIN_STORAGE_KEY = 'ksb_admin_signature'
const BACKUP_PREFIX = 'ksb_signature_backup_'

const getStorageKey = role => (role === 'admin' ? ADMIN_STORAGE_KEY : STORAGE_KEY)
const getBackupKey = role => `${BACKUP_PREFIX}${role === 'admin' ? 'admin' : 'employee'}`

export const getSignatureProfile = (role = 'employee') => {
  if (typeof window === 'undefined') {
    return null
  }

  try {
    const raw =
      window.localStorage.getItem(getStorageKey(role)) ||
      window.localStorage.getItem(getBackupKey(role)) ||
      window.sessionStorage.getItem(getBackupKey(role))
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
  window.localStorage.setItem(getBackupKey(role), JSON.stringify(profile))
  window.sessionStorage.setItem(getBackupKey(role), JSON.stringify(profile))
}

export const clearSignatureProfile = (role = 'employee') => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(getStorageKey(role))
  window.localStorage.removeItem(getBackupKey(role))
  window.sessionStorage.removeItem(getBackupKey(role))
}

export { ADMIN_STORAGE_KEY, STORAGE_KEY }
