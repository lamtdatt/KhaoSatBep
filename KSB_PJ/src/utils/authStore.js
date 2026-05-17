const AUTH_STORAGE_KEY = 'ksb_auth_session'

const readJson = key => {
  if (typeof window === 'undefined') {
    return null
  }

  try {
    const raw = window.localStorage.getItem(key)
    return raw ? JSON.parse(raw) : null
  } catch (error) {
    console.error(`Khong the doc du lieu tu ${key}:`, error)
    return null
  }
}

const writeJson = (key, value) => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(key, JSON.stringify(value))
}

export const getAuthSession = () => readJson(AUTH_STORAGE_KEY)

export const getAccessToken = () => getAuthSession()?.token || ''

export const getCurrentUser = () => {
  const session = getAuthSession()
  if (!session) {
    return null
  }

  return {
    name: session.hoTen,
    email: session.email,
    role: session.vaiTro
  }
}

export const isAuthenticated = () => Boolean(getAccessToken())

export const setAuthSession = session => {
  writeJson(AUTH_STORAGE_KEY, session)
  window.dispatchEvent(new CustomEvent('ksb-auth-changed'))
}

export const clearAuthSession = () => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(AUTH_STORAGE_KEY)
  window.dispatchEvent(new CustomEvent('ksb-auth-changed'))
}

export { AUTH_STORAGE_KEY }
