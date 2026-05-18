const DRAFT_PREFIX = 'ksb_form_draft_'

const getDraftKey = key => `${DRAFT_PREFIX}${key}`

export const loadFormDraft = key => {
  if (typeof window === 'undefined') {
    return null
  }

  try {
    const raw = window.localStorage.getItem(getDraftKey(key))
    return raw ? JSON.parse(raw) : null
  } catch (error) {
    console.error('Khong the doc ban nhap tam:', error)
    return null
  }
}

export const saveFormDraft = (key, payload) => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.setItem(getDraftKey(key), JSON.stringify({
    ...payload,
    savedAt: new Date().toISOString()
  }))
}

export const clearFormDraft = key => {
  if (typeof window === 'undefined') {
    return
  }

  window.localStorage.removeItem(getDraftKey(key))
}

export const scrollFocusedFieldIntoView = event => {
  window.setTimeout(() => {
    event?.target?.scrollIntoView?.({
      behavior: 'smooth',
      block: 'center',
      inline: 'nearest'
    })
  }, 240)
}
