export function getAccount() {
  const savedAccount = localStorage.getItem('account')

  if (!savedAccount) {
    return null
  }

  try {
    return JSON.parse(savedAccount)
  } catch (error) {
    console.error('Invalid account session:', error)
    return null
  }
}

export function getToken() {
  return localStorage.getItem('authToken')
}

export function getRole() {
  return getAccount()?.role || null
}

export function getUser() {
  return getAccount()?.user || null
}

export function isLoggedIn() {
  return !!getToken() && !!getAccount()
}

export function logout() {
  localStorage.removeItem('account')
  localStorage.removeItem('authToken')

  // Parent-specific session cleanup
  localStorage.removeItem('selectedParentChild')
  localStorage.removeItem('parentUser')
  localStorage.removeItem('selectedChild')
}