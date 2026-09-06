import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/qlgx.css'
import { AuthProvider } from './api/AuthContext.tsx'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <App />
    </AuthProvider>
  </StrictMode>,
)
