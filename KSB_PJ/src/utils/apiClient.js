import { clearAuthSession, getAccessToken } from '@/utils/authStore'

const DEFAULT_API_BASE_URL = 'https://khaosatbep-api.onrender.com'
const DEFAULT_TIMEOUT_MS = 45000

export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || DEFAULT_API_BASE_URL).replace(/\/$/, '')

const buildHeaders = customHeaders => {
  const headers = {
    'Content-Type': 'application/json',
    ...customHeaders
  }

  const token = getAccessToken()
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  return headers
}

export const apiRequest = async (path, options = {}) => {
  const { timeoutMs = DEFAULT_TIMEOUT_MS, ...fetchOptions } = options
  const controller = new AbortController()
  const timeoutId = window.setTimeout(() => controller.abort(), timeoutMs)

  const response = await fetch(`${API_BASE_URL}/api${path}`, {
    ...fetchOptions,
    cache: fetchOptions.cache || 'no-store',
    headers: buildHeaders(fetchOptions.headers),
    signal: fetchOptions.signal || controller.signal
  }).finally(() => {
    window.clearTimeout(timeoutId)
  })

  const contentType = response.headers.get('content-type') || ''
  const body = contentType.includes('application/json')
    ? await response.json().catch(() => null)
    : await response.text().catch(() => '')

  if (!response.ok) {
    if (response.status === 401) {
      clearAuthSession()
    }

    const errorMessage =
      body?.message ||
      body?.title ||
      (typeof body === 'string' && body) ||
      `Yeu cau that bai (${response.status})`

    const error = new Error(errorMessage)
    error.status = response.status
    error.body = body
    throw error
  }

  return body
}

export const warmUpApi = () => {
  fetch(`${API_BASE_URL}/health`, {
    method: 'GET',
    cache: 'no-store'
  }).catch(() => null)
}
