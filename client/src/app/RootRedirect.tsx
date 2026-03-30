import { Navigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export function RootRedirect() {
  const { role, isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (role === 'Admin') {
    return <Navigate to="/admin/categories" replace />
  }

  if (role === 'Operator') {
    return <Navigate to="/queue/new" replace />
  }

  return <Navigate to="/tickets" replace />
}