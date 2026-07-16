import { clearAuthSession, getAccessToken } from '@/utils/authStore'

const DEFAULT_API_BASE_URL = 'https://khaosatbep-api.onrender.com'
const DEFAULT_TIMEOUT_MS = 45000

// In dev mode, use Vite proxy (empty base URL); in production, use the full remote URL
const resolvedBase = import.meta.env.VITE_API_BASE_URL || (import.meta.env.DEV ? '' : DEFAULT_API_BASE_URL)
export const API_BASE_URL = resolvedBase.replace(/\/$/, '')

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

  let response

  try {
    response = await fetch(`${API_BASE_URL}/api${path}`, {
      ...fetchOptions,
      cache: fetchOptions.cache || 'no-store',
      headers: buildHeaders(fetchOptions.headers),
      signal: fetchOptions.signal || controller.signal
    })
  } catch (error) {
    if (error?.name === 'AbortError') {
      throw new Error('Máy chủ phản hồi chậm, vui lòng thử lại sau ít phút.')
    }

    throw error
  } finally {
    window.clearTimeout(timeoutId)
  }

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

/**
 * Warm-up API server (Render.com free tier cold start).
 * Retries up to `maxRetries` times with `intervalMs` gap.
 * Returns a promise that resolves to true if server responded, false otherwise.
 */
export const warmUpApi = (maxRetries = 5, intervalMs = 3000) => {
  let resolved = false

  const ping = () =>
    fetch(`${API_BASE_URL}/health`, {
      method: 'GET',
      cache: 'no-store'
    })
      .then(res => res.ok)
      .catch(() => false)

  return new Promise(resolve => {
    let attempt = 0

    const tryPing = async () => {
      if (resolved) return
      attempt++
      const ok = await ping()

      if (ok || attempt >= maxRetries) {
        resolved = true
        resolve(ok)
      } else {
        window.setTimeout(tryPing, intervalMs)
      }
    }

    tryPing()
  })
}
