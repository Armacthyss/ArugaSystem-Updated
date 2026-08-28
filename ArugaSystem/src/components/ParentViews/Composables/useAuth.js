export function getCurrentParent() {
  const savedUser = localStorage.getItem('parentUser')

  if (!savedUser) {
    return null
  }

  try {
    return JSON.parse(savedUser)
  } catch (error) {
    console.error('Invalid parent session:', error)
    return null
  }
}

export function getToken() {
  return localStorage.getItem('authToken')
}

export function isLoggedIn() {
  return !!localStorage.getItem('authToken')
}

export function logout() {
  localStorage.removeItem('parentUser')
  localStorage.removeItem('authToken')
  localStorage.removeItem('selectedParentChild')
}