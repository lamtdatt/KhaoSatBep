import { clearAuthSession, getAccessToken } from '@/utils/authStore'

const DEFAULT_API_BASE_URL = 'http://localhost:5264/api'

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
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: buildHeaders(options.headers)
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
