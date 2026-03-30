import { BrowserRouter, Routes, Route } from 'react-router-dom';
import AppLayout from './components/AppLayout';
import { RequireAuth } from './components/RequireAuth';
import { RequireRole } from './components/RequireRole';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import TransactionsPage from './pages/user/TransactionsPage';
import NewTransactionPage from './pages/user/NewTransactionPage';
import CategoriesPage from './pages/admin/CategoriesPage';
import UsersPage from './pages/admin/UsersPage';
import PageNotFound from './components/PageNotFound';
import { RootRedirect } from './app/RootRedirect';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Private routes — wrapped by RequireAuth, rendered inside AppLayout */}
        <Route
          element={
            <RequireAuth>
              <AppLayout />
            </RequireAuth>
          }
        >
          {/* User */}
          <Route
            path="/transactions"
            element={
              <RequireRole roles={['User']}>
                <TransactionsPage />
              </RequireRole>
            }
          />
          <Route
            path="/transactions/new"
            element={
              <RequireRole roles={['User']}>
                <NewTransactionPage />
              </RequireRole>
            }
          />

          {/* Admin */}
          <Route
            path="/admin/categories"
            element={
              <RequireRole roles={['Admin']}>
                <CategoriesPage />
              </RequireRole>
            }
          />
          <Route
            path="/admin/users"
            element={
              <RequireRole roles={['Admin']}>
                <UsersPage />
              </RequireRole>
            }
          />
        </Route>

        {/* Default redirect */}
        <Route path="/" element={<RootRedirect />} />
        <Route path="*" element={<PageNotFound />} />
      </Routes>
    </BrowserRouter>
  );
}
